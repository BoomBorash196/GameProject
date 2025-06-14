using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameProject
{
    public class Level
    {
        public int LevelNumber { get; set; }
        public int[,] Map { get; set; }
        public Vector2 PlayerSpawnPoint { get; set; }
        public Vector2 ExitPoint { get; set; }
        public int EnemyCount { get; set; }
        public List<Vector2> EnemySpawnPoints { get; set; }
        public int NextLevelNumber { get; set; }

        public Level(int levelNumber, int[,] map, Vector2 playerSpawn, Vector2 exitPoint, int enemyCount, List<Vector2> enemySpawnPoints, int nextLevelNumber)
        {
            LevelNumber = levelNumber;
            Map = map;
            PlayerSpawnPoint = playerSpawn;
            ExitPoint = exitPoint;
            EnemyCount = enemyCount;
            EnemySpawnPoints = enemySpawnPoints;
            NextLevelNumber = nextLevelNumber;
        }
    }
} 