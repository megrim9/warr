using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Utils;

namespace WarriorsClient
{
    public class CommandProcessor
    {
        private readonly NetworkStream _stream;
        private readonly TcpClient _client;

        public CommandProcessor(NetworkStream stream, TcpClient client)
        {
            this._stream = stream;
            this._client = client;
        }

        public void ProcessCommand(string command)
        {
            switch (command)
            {
                case "addresources":
                    AddResources();
                    break;
                case "addtroops":
                    AddTroops();
                    break;
                case "remove":
                    RemoveResources();
                    break;
                case "get":
                    GetResources();
                    break;
                case "gettroops":
                    GetTroops();
                    break;
                case "getemails":
                    GetEmails();
                    break;
                case "sendemail":
                    SendEmail();
                    break;
                case "grid":
                    GetGridAroundPlayer();
                    break;
                case "upgrade":
                    UpgradeBuilding();
                    break;
                case "login":
                    DoLogin();
                    break;
                case "register":
                    DoRegister();
                    break;
                case "exit":
                    Disconnect();
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }

        private void AddResources()
        {
            Console.WriteLine("Enter the amount of resources to add:");
            Console.Write("Wood: ");
            int wood = Utils.Utils.GetIntInput();
            Console.Write("Clay: ");
            int clay = Utils.Utils.GetIntInput();
            Console.Write("Iron: ");
            int iron = Utils.Utils.GetIntInput();
            Console.Write("Crop: ");
            int crop = Utils.Utils.GetIntInput();

            var message = new ResourcesMessage
            {
                Type = MessageType.AddResources,
                Wood = wood,
                Clay = clay,
                Iron = iron,
                Crop = crop
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }
        private void AddTroops()
        {
            Console.WriteLine("to what city?:");
            int cityIndex = Utils.Utils.GetIntInput();
            Console.WriteLine("Enter the amount of troops to add:"); 
            int amount = Utils.Utils.GetIntInput();
            Console.Write("what type of unit? (Knight, Archer, Pikeman)");
            string troopType = Console.ReadLine()?.ToLower(); 

            TroopsMessage message = new TroopsMessage()
            {
                Type = MessageType.AddTroops,
                CityIndex = cityIndex,
                Amount = amount,
                TroopType = ParseEnum<TroopType>(troopType)
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private void RemoveResources()
        {
            Console.WriteLine("Enter the amount of resources to remove:");
            Console.Write("Wood: ");
            int wood = Utils.Utils.GetIntInput();
            Console.Write("Clay: ");
            int clay = Utils.Utils.GetIntInput();
            Console.Write("Iron: ");
            int iron = Utils.Utils.GetIntInput();
            Console.Write("Crop: ");
            int crop = Utils.Utils.GetIntInput();

            var message = new ResourcesMessage
            {
                Type = MessageType.RemoveResources,
                Wood = wood,
                Clay = clay,
                Iron = iron,
                Crop = crop
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }
        private void GetResources()
        {
            var message = new Message
            {
                Type = MessageType.GetResources
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        private void GetTroops()
        {
            Console.Write("choose city index: ");
            int cityIndex = Utils.Utils.GetIntInput();

            var message = new GridMessage
            {
                Type = MessageType.GetTroops,
                CityIndex = cityIndex
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        private void GetEmails()
        {
            var message = new EmailMessage
            {
                Type = MessageType.GetEmails,
            };
            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }
        private void SendEmail()
        {
            Console.WriteLine("write the email: (<target> <subject> <message>)");
            string email = Console.ReadLine();
            string[] emailSplitted = email.Split(" ");
            if (emailSplitted.Length != 3)
            {
                Console.WriteLine($"Invalid Email.");
                return;
            }
            string target = emailSplitted[0];
            string subject = emailSplitted[1];
            string body = emailSplitted[2];
            List<Email> emailList = new();
            emailList.Add(new Email { Target = target, Sender = Client.GetLoggedInAs(), Subject = subject, Body = body, Date = DateTime.UtcNow });
            var message = new EmailMessage
            {
                Type = MessageType.SendEmail,
                Emails = emailList
            };
            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        private void GetGridAroundPlayer()
        {
            Console.Write("choose city index: ");
            int cityIndex = Utils.Utils.GetIntInput();

            var message = new GridMessage
            {
                Type = MessageType.GetGridAroundPlayer,
                CityIndex = cityIndex
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        private void UpgradeBuilding()
        {
            Console.Write("building to upgrade (WoodFarm,ClayFarm, IronFarm, CropFarm): ");
            string typeNr = Console.ReadLine();
            if (Enum.TryParse(typeNr, out BuildingType selectedOption))
            {
                var message = new UpgradeMessage
                {
                    Type = MessageType.Upgrade,
                    BuildingType = selectedOption
                };

                string serializedMessage = JsonConvert.SerializeObject(message);
                SendMessage(serializedMessage);
            }
            else
            {
                Console.WriteLine($"Invalid building type: {typeNr}");
            }

        }

        private void DoLogin()
        {
            Console.Write("login: ");
            string login = Console.ReadLine();
            Console.Write("password: ");
            string password = Console.ReadLine();

            var message = new LoginMessage
            {
                Login = login,
                Password = password,
                Type = MessageType.Login
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }
        private void DoRegister()
        {
            Console.Write("login: ");
            string login = Console.ReadLine();
            Console.Write("password: ");
            string password = Console.ReadLine();

            var message = new RegisterMessage
            {
                Login = login,
                Password = password,
                Type = MessageType.Register
            };

            string serializedMessage = JsonConvert.SerializeObject(message);
            SendMessage(serializedMessage);
        }

        private void Disconnect()
        {
            try
            {
                _stream.Close();
                _client.Close();
                Console.WriteLine("Disconnected from the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
