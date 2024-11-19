using System;
using Utils;

namespace WarriorsClient
{
    public static class GridManager
    {
        public static void PrintGrid(TileType[,] grid)
        {
            if (grid == null)
            {
                Console.WriteLine("Grid is not initialized.");
                return;
            }

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    char symbol = GetSymbolForTile(grid[i, j]);
                    Console.Write(symbol + " ");
                }
                Console.WriteLine();
            }
        }

        private static char GetSymbolForTile(TileType tile)
        {
            // Map TileType values to corresponding symbols
            switch (tile)
            {
                case TileType.Grass:
                    return 'G';
                case TileType.Water:
                    return 'W';
                case TileType.Mountain:
                    return 'M';
                case TileType.Forest:
                    return 'F';
                case TileType.City:
                    return 'C';
                default:
                    return '?'; // Placeholder for unknown tile types
            }
        }
    }
}
