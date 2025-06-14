using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static GameProject.Game1;
using System.Xml.Linq;

namespace GameProject
{
    public class GameLogic
    {
        private readonly ContentManager _content;
        private readonly Texture2D _pixel;
        private readonly LevelManager _levelManager;
        private readonly List<Enemy> _enemies;
        private readonly Renderer _renderer;
        private const double CollisionBuffer = 0.3;
        private float _spawnTimer = 0f;
        private float _spawnInterval = 10f;
        private int _maxEnemies = 2;
        private float _damageCooldown = 0f;
        private int _playerHealth;
        private Game1 _game;
        public GameLogic(
        ContentManager content,
        Texture2D pixel,
        LevelManager levelManager,
        List<Enemy> enemies,
        Renderer renderer,
        Game1 game) // Добавляем параметр game
        {
            _content = content;
            _pixel = pixel;
            _levelManager = levelManager;
            _enemies = enemies;
            _renderer = renderer;
            _game = game;
            _playerHealth = game.playerHealth;
        }

        private void UpdatePlayerEnemyCollision(GameTime gameTime)
        {
            if (_damageCooldown > 0)
            {
                _damageCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            Vector2 playerPosition = new Vector2((float)_game.posX, (float)_game.posY);
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsActive) continue;

                float distance = Vector2.Distance(playerPosition, enemy.Position);
                if (distance < 0.5f)
                {
                    _playerHealth -= 10;
                    _game.playerHealth = _playerHealth;
                    _damageCooldown = DAMAGE_COOLDOWN_TIME;

                    if (_playerHealth <= 0)
                    {
                        _playerHealth = 100;
                        _game.playerHealth = 100;
                        
                        _levelManager.ResetToFirstLevel();
                        var firstLevel = _levelManager.GetCurrentLevel();
                        
                        _game.posX = firstLevel.PlayerSpawnPoint.X;
                        _game.posY = firstLevel.PlayerSpawnPoint.Y;
                        _game.Map = firstLevel.Map;
                        
                        _renderer.SetCurrentMap(firstLevel.Map);
                        
                        _enemies.Clear();
                        SpawnEnemies(firstLevel.EnemyCount);
                        
                        CurrentGameState = GameState.GameOver;
                    }
                    break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentGameState == GameState.Playing)
            {
                UpdateEnemySpawning(gameTime);
                UpdatePlayerEnemyCollision(gameTime);
            }
        }

        private void UpdateEnemySpawning(GameTime gameTime)
        {
            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_spawnTimer >= _spawnInterval && _enemies.Count < _maxEnemies)
            {
                _spawnTimer = 0f;
                SpawnEnemies(1);
            }
        }

        
        private const float DAMAGE_COOLDOWN_TIME = 0.5f; 

        

        public void SpawnEnemies(int enemyCount)
        {
            var currentLevel = _levelManager.GetCurrentLevel();
            Texture2D[] enemyFrames = new Texture2D[6];
            try
            {
                enemyFrames[0] = _content.Load<Texture2D>("enemies/bigBoy/walk (1)");
                enemyFrames[1] = _content.Load<Texture2D>("enemies/bigBoy/walk (2)");
                enemyFrames[2] = _content.Load<Texture2D>("enemies/bigBoy/walk (3)");
                enemyFrames[3] = _content.Load<Texture2D>("enemies/bigBoy/walk (4)");
                enemyFrames[4] = _content.Load<Texture2D>("enemies/bigBoy/walk (5)");
                enemyFrames[5] = _content.Load<Texture2D>("enemies/bigBoy/walk (6)");
            }
            catch (Exception ex)
            {
                enemyFrames = new Texture2D[] { _pixel };
                Debug.WriteLine("Ошибка загрузки: " + ex.Message);
            }

            foreach (var spawnPoint in currentLevel.EnemySpawnPoints)
            {
                _enemies.Add(new Enemy
                {
                    Position = spawnPoint,
                    AnimationFrames = enemyFrames,
                    CurrentFrame = 0,
                    FrameTime = 0.1f,
                    Speed = 0.025f,
                    Health = 100,
                    Size = new Vector2(64, 64)
                });
            }
        }

        public void CollisionsBuffer(double newPosX, double newPosY, ref double posX, ref double posY, int[,] map)
        {
            bool collideX = false;
            bool collideY = false;

            for (double i = -CollisionBuffer; i <= CollisionBuffer; i += CollisionBuffer)
            {
                for (double j = -CollisionBuffer; j <= CollisionBuffer; j += CollisionBuffer)
                {
                    int checkX = (int)(newPosX + i);
                    int checkY = (int)(posY + j);
                    if (checkX >= 0 && checkX < map.GetLength(0) &&
                        checkY >= 0 && checkY < map.GetLength(1))
                    {
                        if (map[checkX, checkY] > 0) collideX = true;
                    }

                    checkX = (int)(posX + i);
                    checkY = (int)(newPosY + j);
                    if (checkX >= 0 && checkX < map.GetLength(0) &&
                        checkY >= 0 && checkY < map.GetLength(1))
                    {
                        if (map[checkX, checkY] > 0) collideY = true;
                    }
                }
            }

            if (!collideX) posX = newPosX;
            if (!collideY) posY = newPosY;
        }

        public void UpdateRendererData(double posX, double posY, double dirX, double dirY, double planeX, double planeY)
        {
            _renderer.UpdateData(posX, posY, dirX, dirY, planeX, planeY);
        }
    }
}
