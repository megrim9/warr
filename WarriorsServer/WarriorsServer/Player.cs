using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Utils;
using System.Linq;

namespace WarriorsServer
{
    public class Player
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Wood { get; set; }
        public int Clay { get; set; }
        public int Iron { get; set; }
        public int Crop { get; set; }

        public int SightRange { get; set; }

        private readonly List<City> _cities = new();
        private readonly List<Email> _emails = new();

        public int WoodFarmLevel { get; set; }
        public int ClayFarmLevel { get; set; }
        public int IronFarmLevel { get; set; }
        public int CropFarmLevel { get; set; }
        public Socket ClientSocket { get; set; }

        public Player(string login, string password, int id, int cityXPosition, int cityYPosition)
        {
            Login = login;
            Password = password;
            Id = id;
            SightRange = 5;
            City mainCity = Map.CreateNewCity($"{login}'s city", cityXPosition, cityYPosition, this);
            _cities.Add(mainCity);
            WoodFarmLevel = 1; ClayFarmLevel = 1; IronFarmLevel = 1; CropFarmLevel = 1;
        }

        public City GetCity(int cityIndex)
        {
            if (_cities.Count < cityIndex)
            {
                Server.SendToClient(ClientSocket, new Message
                {
                    Type = MessageType.Error,
                    Text = $"City at index {cityIndex} not found."
                });
                return null;
            }
            return _cities[cityIndex];
        }
        internal Army GetArmy(int cityIndex)
        {
            City c = GetCity(cityIndex);
            return c.GetAssociatedArmy();
        }
        internal List<City> GetCities()
        {
            return _cities;
        }
        internal List<Army> GetArmies()
        {
            return GetCities().Select(city => city.GetAssociatedArmy()).ToList();
        }

        internal void GetResources()
        {
            Server.SendToClient(ClientSocket, new ResourcesMessage
            {
                Type = MessageType.GetResources,
                Wood = Wood,
                Clay = Clay,
                Iron = Iron,
                Crop = Crop
            });
        }

        internal void GetEmails()
        {
            Server.SendToClient(ClientSocket, new EmailMessage
            {
                Type = MessageType.GetEmails,
                Emails = _emails
            }) ;
        }
        internal void SaveEmail(Email email)
        {
            _emails.Add(email);
        }
        internal void GetTroops(Army army)
        {
            Server.SendToClient(ClientSocket, new TroopsMessage
            {
                Type = MessageType.GetTroops,
                ArcherCount = army.ArcherCount,
                KnightCount = army.KnightCount,
                PikemanCount = army.PikemanCount
            });
        }

        internal void GetGridAroundPlayer(GridMessage gMessage)
        {
            City city = GetCity(gMessage.CityIndex);
            if (city == null)
                return;
            if (Map.Grid == null || SightRange <= 0)
            {
                // Handle invalid input
                throw new ArgumentException("Invalid grid or sub-grid size");
            }

            // Ensure center coordinates are within valid range
            int centerX = Math.Max(0, Math.Min(Map.MapWidth, city.X));
            int centerY = Math.Max(0, Math.Min(Map.MapHeight, city.Y));

            int startX = Math.Max(0, centerX - SightRange);
            int startY = Math.Max(0, centerY - SightRange);
            int endX = Math.Min(Map.MapWidth, centerX + SightRange);
            int endY = Math.Min(Map.MapHeight, centerY + SightRange);

            if (startX > endX || startY > endY)
            {
                // Handle case where sub-grid is entirely outside the valid range
                throw new ArgumentException("Sub-grid is outside the valid range");
            }

            int subGridSizeX = endX - startX + 1;
            int subGridSizeY = endY - startY + 1;

            TileType[,] subGrid = new TileType[subGridSizeX, subGridSizeY];

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    subGrid[x - startX, y - startY] = Map.Grid[x, y];
                }
            }
            Server.SendToClient(ClientSocket, new GridMessage
            {
                Type = MessageType.GetGridAroundPlayer,
                Grid = subGrid
            });
        }

        internal void AddResources(int wood, int clay, int iron, int crop)
        {
            Wood += wood;
            Clay += clay;
            Iron += iron;
            Crop += crop;
        }

        internal void RemoveResources(int wood, int clay, int iron, int crop)
        {
            try
            {
                if (Wood >= wood && Clay >= clay && Iron >= iron && Crop >= crop)
                {
                    Wood -= wood;
                    Clay -= clay;
                    Iron -= iron;
                    Crop -= crop;
                }
                else
                {
                    // Handle insufficient resources error
                    Server.SendToClient(ClientSocket, new Message { Type = MessageType.Error, Text = "insufficient resources" });
                }
            }
            catch (Exception ex)
            {
                // Handle exception and log error...
                Console.WriteLine($"Exception in RemoveResources: {Login} -> {ex.Message}");
            }
        }
        internal void ProcessAttack(AttackMessage attackTypeMessage)
        {

        }
        internal void UpgradeBuilding(UpgradeMessage buildingTypeMessage)
        {
            bool lackResources = false;
            int toDeduce;
            switch (buildingTypeMessage.BuildingType) // convert this into an interface
            {
                case BuildingType.WoodFarm:
                    toDeduce = WoodFarmLevel * 10;
                    if (Clay >= toDeduce && Crop > toDeduce)
                    {
                        Clay -= toDeduce;
                        Crop -= toDeduce;
                        WoodFarmLevel++;
                    }
                    else
                        lackResources = true;
                    break;
                case BuildingType.ClayFarm:
                    toDeduce = ClayFarmLevel * 10;
                    if (Wood >= toDeduce && Crop > toDeduce)
                    {
                        Wood -= toDeduce;
                        Crop -= toDeduce;
                        ClayFarmLevel++;
                    }
                    else
                        lackResources = true;
                    break;
                case BuildingType.IronFarm:
                    toDeduce = IronFarmLevel * 10;
                    if (Wood >= toDeduce && Iron > toDeduce)
                    {
                        Wood -= toDeduce;
                        Iron -= toDeduce;
                        IronFarmLevel++;
                    }
                    else
                        lackResources = true;
                    break;
                case BuildingType.CropFarm:
                    toDeduce = CropFarmLevel * 10;
                    if (Clay >= toDeduce && Iron > toDeduce)
                    {
                        Clay -= toDeduce;
                        Iron -= toDeduce;
                        CropFarmLevel++;
                    }
                    else
                        lackResources = true;
                    break;
                default:
                    Server.SendToClient(ClientSocket, new Message { Type = MessageType.Error, Text = $"Building Type not recognized: {buildingTypeMessage.BuildingType}" });
                    break;
            }
            if (lackResources)
                Server.SendToClient(ClientSocket, new Message { Type = MessageType.Error, Text = $"insufficient resources for {buildingTypeMessage.BuildingType} upgrade" });
            else
                Server.SendToClient(ClientSocket, new Message { Type = MessageType.Success, Text = $"{buildingTypeMessage.BuildingType} upgrade successful!" });
        }
    }
}
