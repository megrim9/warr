using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Utils;

namespace WarriorsServer
{
    public class Map
    {
        public static TileType[,] Grid { get; private set; }
        public static List<City> Cities { get; private set; }

        public static int MapWidth = 1000;
        public static int MapHeight = 1000;

        public static void InitializeMap()
        {
            Grid = new TileType[MapWidth, MapHeight]; // initial grid, must be loaded from DB, prob will hold more info related to the city
            Cities = new List<City>();
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    Grid[x, y] = GetRandomTileType();
                }
            }
        }

        private static TileType GetRandomTileType()
        {
            // Generate a random terrain type (excluding City)
            TileType[] terrainTypes = Enum.GetValues(typeof(TileType)) as TileType[];
            List<TileType> nonCityTerrainTypes = terrainTypes.Where(type => type != TileType.City).ToList();
            Random random = new Random();
            int randomIndex = random.Next(nonCityTerrainTypes.Count);
            return nonCityTerrainTypes[randomIndex];
        }

        public static City CreateNewCity(string cityName, int x, int y, Player p)
        {
            City newCity = null;
            if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight && Grid[x, y] != TileType.City)
            {
                newCity = new City(cityName, x, y,p);
                Cities.Add(newCity);
                Grid[x, y] = TileType.City; // Mark the tile as a city
            }
            else
            {
                Console.WriteLine("Error creating city. Wrong coordinates.");
                // Handle invalid city placement
            }
            return newCity;
        }
    }
}
