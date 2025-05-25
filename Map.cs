using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GameProject
{
    public static class Map
    {
        public const int ENTRY_POINT = -1;  // Точка входа
        public const int EXIT_POINT = -2;   // Точка выхода

        public static int[,] GiveMap()
        {
            return new int[,]
            {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,-1,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,2,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,2,2,2,2,0,2,2,2,2,0,0,0,0,3,3,3,3,3,0,0,0,1},
            {1,0,0,0,0,2,0,0,0,0,2,3,3,3,3,0,0,0,0,3,0,0,0,1},
            {1,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,1},
            {1,0,0,0,0,0,2,2,0,2,2,3,3,3,3,3,3,3,3,3,0,0,0,1},
            {1,0,0,0,0,0,0,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,5,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,4,4,4,7,7,7,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-2,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };
        }

        //
        public static Vector2 FindSpecialPoint(int[,] map, int pointType)
        {
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (map[y, x] == pointType)
                    {
                        return new Vector2(x + 0.5f, y + 0.5f);
                    }
                }
            }
            return new Vector2(1.5f, 1.5f); // Позиция по умолчанию
        }

        public static bool IsPointType(int[,] map, Vector2 position, int pointType)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            return x >= 0 && y >= 0 &&
                   y < map.GetLength(0) &&
                   x < map.GetLength(1) &&
                   map[y, x] == pointType;
        }

        public static int[,] LoadLevelFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            int[,] map = new int[lines.Length, lines[0].Length];

            for (int i = 0; i < lines.Length; i++)
                for (int j = 0; j < lines[i].Length; j++)
                    map[i, j] = int.Parse(lines[i][j].ToString());

            return map;
        }

        public static int[,] LoadLevel2() 
        { 
            return new int[,] {
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            { 1,-1,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            { 1,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            { 1,2,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            { 1,4,4,4,7,7,7,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-2,1},
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };
        }
    }


}
