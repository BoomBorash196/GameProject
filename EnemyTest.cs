//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Audio;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;

//namespace GameProject
//{
//    public class Game1 : Game
//    {
//        private GraphicsDeviceManager _graphics;
//        private SpriteBatch _spriteBatch;

//        private int[,] map = Map.GiveMap();
//        private double posX = 1.5, posY = 1.5;
//        private double dirX = -1.0, dirY = 0.0;
//        private double planeX = 0.0, planeY = 0.66;
//        private float mouseSensitivity = 0.002f;
//        private MouseState prevMouseState;

//        private Texture2D pixel;
//        private List<TextureData> wallTextures;
//        private const double CollisionBuffer = 0.3;

//        public class TextureData
//        {
//            public Texture2D Texture { get; set; }
//            public Color[] Data { get; set; }
//        }

//        private int screenWidth = 640;
//        private int screenHeight = 480;

//        public enum GameState
//        {
//            MainMenu,
//            Playing,
//            Settings,
//            SoundSettings,
//            ControlsSettings,
//            Radio,
//            Exit
//        }
//        public static GameState CurrentGameState = GameState.MainMenu;

//        private MainMenu _mainMenu;
//        private SpriteFont _font;
//        private SettingsMenu _settings;
//        private SoundMenu _soundMenu;
//        private ControlsMenu _controlsMenu;
//        private Texture2D _backgroundTexture;
//        private float[] _wallDistances;
//        private Radio _radioMenu;
//        public SoundEffect[] _radioTracks;
//        private Renderer _renderer;
//        private KeyboardState _prevKeyboardState;
//        private KeyboardState _keyboardState;
//        public static GameState _prevGameState = GameState.MainMenu;
//        private Shotgun _shotgun;
//        private List<Texture2D> _shotgunFrames;
//        private Texture2D _shotgunTexture;
//        private Texture2D _bulletTexture;
//        private List<Texture2D> _muzzleFlashTextures;
//        private SoundEffect _shotgunSound;
//        private Minimap _minimap;
//        private List<Enemy> _enemies;
//        private Random _random = new Random();
//        private double _enemySpawnTimer = 0;
//        private const double EnemySpawnInterval = 10.0; // Спавн врага каждые 10 секунд

//        public Game1()
//        {
//            _graphics = new GraphicsDeviceManager(this);
//            Content.RootDirectory = "Content";
//            IsMouseVisible = false;
//            _graphics.IsFullScreen = false;
//        }

//        protected override void Initialize()
//        {
//            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
//            prevMouseState = Mouse.GetState();
//            _prevKeyboardState = Keyboard.GetState();
//            _wallDistances = new float[screenWidth];
//            _graphics.PreferredBackBufferWidth = screenWidth;
//            _graphics.PreferredBackBufferHeight = screenHeight;
//            _graphics.ApplyChanges();

//            _enemies = new List<Enemy>();
//            base.Initialize();
//        }

//        protected override void LoadContent()
//        {
//            _radioTracks = new SoundEffect[]
//            {
//                Content.Load<SoundEffect>("Radio/antisocial"),
//                Content.Load<SoundEffect>("Radio/ashes"),
//                Content.Load<SoundEffect>("Radio/Shikami")
//            };
//            _backgroundTexture = Content.Load<Texture2D>("backgrounds/wolfenstein");
//            _spriteBatch = new SpriteBatch(GraphicsDevice);
//            _font = Content.Load<SpriteFont>("RetroFont");
//            SoundEffect _backgroundMusic = Content.Load<SoundEffect>("background_sounds/bgSound");

//            _mainMenu = new MainMenu(_font, _backgroundTexture);
//            _settings = new SettingsMenu(_font, _backgroundTexture, this);
//            _soundMenu = new SoundMenu(_font, _backgroundTexture, _backgroundMusic);
//            _controlsMenu = new ControlsMenu(_font, _backgroundTexture);
//            _radioMenu = new Radio(_font, _backgroundTexture, _radioTracks);

//            pixel = new Texture2D(GraphicsDevice, 1, 1);
//            pixel.SetData(new[] { Color.White });

//            _shotgunFrames = new List<Texture2D>
//            {
//                Content.Load<Texture2D>("weapons/shotgun"),
//                Content.Load<Texture2D>("weapons/shotgun"),
//                Content.Load<Texture2D>("weapons/shotgun1"),
//                Content.Load<Texture2D>("weapons/shotgun2"),
//                Content.Load<Texture2D>("weapons/shotgun3")
//            };
//            _shotgunTexture = _shotgunFrames[0];
//            _bulletTexture = Content.Load<Texture2D>("weapons/bullet");
//            _muzzleFlashTextures = new List<Texture2D>
//            {
//                Content.Load<Texture2D>("weapons/shotgun_fire_effect1"),
//                Content.Load<Texture2D>("weapons/shotgun_fire_effect2")
//            };
//            _shotgunSound = Content.Load<SoundEffect>("weapons/sounds/shotgun_sound");

//            _shotgun = new Shotgun(_shotgunFrames, _bulletTexture,
//                                _muzzleFlashTextures, _shotgunSound,
//                                GraphicsDevice, screenWidth,
//                                screenHeight, _wallDistances);

//            _minimap = new Minimap(pixel, map);

//            wallTextures = new List<TextureData>
//            {
//                LoadTextureData("world_textures/redbrick"),
//                LoadTextureData("world_textures/eagle"),
//                LoadTextureData("world_textures/greenlight"),
//                LoadTextureData("world_textures/wood"),
//                LoadTextureData("world_textures/green"),
//                LoadTextureData("world_textures/mossy"),
//            };

//            // Загрузка текстур врагов
//            Texture2D enemyTex = Content.Load<Texture2D>("world_textures/green");

//            // Создаем начальных врагов
//            SpawnEnemies(3); // Спавним 3 врагов в начале игры

//            _renderer = new Renderer(_spriteBatch, GraphicsDevice, wallTextures,
//                                   screenWidth, screenHeight, posX, posY,
//                                   dirX, dirY, planeX, planeY);
//        }

//        private void SpawnEnemies(int count)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                Vector2 spawnPos = FindValidSpawnPosition();
//                if (spawnPos != Vector2.Zero)
//                {
//                    var enemyFrames = new Rectangle[4];
//                    for (int j = 0; j < 4; j++)
//                        enemyFrames[j] = new Rectangle(j * 64, 0, 64, 64);

//                    _enemies.Add(new Enemy()
//                    {
//                        Position = spawnPos,
//                        Texture = Content.Load<Texture2D>("world_textures/green"),
//                        Frames = enemyFrames,
//                        Speed = 0.02f + (float)_random.NextDouble() * 0.03f,
//                        Health = 100
//                    });
//                }
//            }
//        }

//        private Vector2 FindValidSpawnPosition()
//        {
//            int attempts = 0;
//            while (attempts < 100)
//            {
//                attempts++;

//                // Генерируем случайную позицию на карте
//                int x = _random.Next(1, map.GetLength(0) - 1);
//                int y = _random.Next(1, map.GetLength(1) - 1);

//                // Проверяем, что это пустая клетка и не слишком близко к игроку
//                if (map[x, y] == 0 &&
//                    Vector2.Distance(new Vector2(x, y), new Vector2((float)posX, (float)posY)) > 5.0f)
//                {
//                    // Добавляем небольшой случайный сдвиг внутри клетки
//                    float offsetX = (float)(_random.NextDouble() * 0.8 + 0.1);
//                    float offsetY = (float)(_random.NextDouble() * 0.8 + 0.1);
//                    return new Vector2(x + offsetX, y + offsetY);
//                }
//            }
//            return Vector2.Zero;
//        }

//        private TextureData LoadTextureData(string texturePath)
//        {
//            if (_textureCache.ContainsKey(texturePath))
//                return _textureCache[texturePath];

//            try
//            {
//                Texture2D texture = Content.Load<Texture2D>(texturePath);
//                Color[] data = new Color[texture.Width * texture.Height];
//                texture.GetData(data);

//                var textureData = new TextureData { Texture = texture, Data = data };
//                _textureCache[texturePath] = textureData;

//                return textureData;
//            }
//            catch (ContentLoadException ex)
//            {
//                throw;
//            }
//        }

//        protected override void Update(GameTime gameTime)
//        {
//            if (CurrentGameState == GameState.Exit)
//                Exit();

//            _keyboardState = Keyboard.GetState();

//            if (_prevGameState != CurrentGameState)
//            {
//                _prevGameState = CurrentGameState;
//                if (CurrentGameState == GameState.MainMenu)
//                    _mainMenu.Reset();
//                else if (CurrentGameState == GameState.Settings)
//                    _settings.Reset();
//                else if (CurrentGameState == GameState.SoundSettings)
//                    _soundMenu.Reset();
//                else if (CurrentGameState == GameState.ControlsSettings)
//                    _controlsMenu.Reset();
//            }

//            if (CurrentGameState == GameState.Playing)
//            {
//                // Обновляем таймер спавна врагов
//                _enemySpawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
//                if (_enemySpawnTimer >= EnemySpawnInterval)
//                {
//                    _enemySpawnTimer = 0;
//                    SpawnEnemies(1); // Спавним 1 врага каждые 10 секунд
//                }

//                // Обновляем врагов
//                for (int i = _enemies.Count - 1; i >= 0; i--)
//                {
//                    _enemies[i].Update(gameTime, new Vector2((float)posX, (float)posY), map);

//                    // Удаляем мертвых врагов
//                    if (!_enemies[i].IsActive)
//                    {
//                        _enemies.RemoveAt(i);
//                    }
//                }

//                // Проверка попаданий
//                if (_keyboardState.IsKeyDown(Keys.Space) && !_prevKeyboardState.IsKeyDown(Keys.Space))
//                {
//                    CheckEnemyHits();
//                }
//            }

//            switch (CurrentGameState)
//            {
//                case GameState.MainMenu:
//                    _mainMenu.Update(gameTime);
//                    break;
//                case GameState.Playing:
//                    UpdateCamera((float)gameTime.ElapsedGameTime.TotalSeconds);
//                    Vector2 playerPosition = new Vector2((float)posX, (float)posY);
//                    Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
//                    _shotgun.Update(gameTime, playerPosition, mousePosition, posX, posY, dirX, dirY, planeX, planeY, _wallDistances);
//                    _minimap.Update(playerPosition);
//                    break;
//                case GameState.Settings:
//                    _settings.Update(gameTime);
//                    break;
//                case GameState.SoundSettings:
//                    _soundMenu.Update(gameTime);
//                    break;
//                case GameState.Radio:
//                    _radioMenu.Update(gameTime);
//                    break;
//                case GameState.ControlsSettings:
//                    _controlsMenu.Update(gameTime);
//                    break;
//            }

//            _prevKeyboardState = _keyboardState;
//            base.Update(gameTime);
//        }

//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.Black);

//            switch (CurrentGameState)
//            {
//                case GameState.MainMenu:
//                    _mainMenu.Draw(_spriteBatch, GraphicsDevice);
//                    break;
//                case GameState.Playing:
//                    DrawAll();
//                    _spriteBatch.Begin();
//                    _shotgun.Draw(_spriteBatch);
//                    _minimap.Draw(_spriteBatch);
//                    DrawCrosshair();

//                    // Отрисовка врагов
//                    foreach (var enemy in _enemies)
//                    {
//                        enemy.Draw(_spriteBatch, new Vector2((float)posX, (float)posY),
//                            (float)Math.Atan2(dirY, dirX), _wallDistances, screenWidth, screenHeight);
//                    }

//                    _spriteBatch.End();
//                    break;
//                case GameState.Settings:
//                    _settings.Draw(_spriteBatch, GraphicsDevice);
//                    break;
//                case GameState.SoundSettings:
//                    _soundMenu.Draw(_spriteBatch, GraphicsDevice);
//                    break;
//                case GameState.ControlsSettings:
//                    _controlsMenu.Draw(_spriteBatch, GraphicsDevice);
//                    break;
//                case GameState.Radio:
//                    _radioMenu.Draw(_spriteBatch, GraphicsDevice);
//                    break;
//            }

//            base.Draw(gameTime);
//        }

//        private void DrawCrosshair()
//        {
//            int centerX = screenWidth / 2;
//            int centerY = screenHeight / 2;
//            int crosshairLength = 10;
//            int crosshairThickness = 2;

//            _spriteBatch.Draw(pixel, new Rectangle(centerX - crosshairLength, centerY - crosshairThickness / 2,
//                crosshairLength * 2, crosshairThickness), Color.Red);
//            _spriteBatch.Draw(pixel, new Rectangle(centerX - crosshairThickness / 2, centerY - crosshairLength,
//                crosshairThickness, crosshairLength * 2), Color.Red);
//        }

//        private void UpdateCamera(float deltaTime)
//        {
//            float moveSpeed = 5.0f * deltaTime;
//            MouseState currentMouseState = Mouse.GetState();

//            int deltaX = currentMouseState.X - prevMouseState.X;
//            double rotation = -deltaX * mouseSensitivity;

//            double oldDirX = dirX;
//            dirX = dirX * Math.Cos(rotation) - dirY * Math.Sin(rotation);
//            dirY = oldDirX * Math.Sin(rotation) + dirY * Math.Cos(rotation);

//            double oldPlaneX = planeX;
//            planeX = planeX * Math.Cos(rotation) - planeY * Math.Sin(rotation);
//            planeY = oldPlaneX * Math.Sin(rotation) + planeY * Math.Cos(rotation);

//            double newPosX = posX;
//            double newPosY = posY;

//            if (Keyboard.GetState().IsKeyDown(Keys.W))
//            {
//                newPosX += dirX * moveSpeed;
//                newPosY += dirY * moveSpeed;
//            }
//            if (Keyboard.GetState().IsKeyDown(Keys.S))
//            {
//                newPosX -= dirX * moveSpeed;
//                newPosY -= dirY * moveSpeed;
//            }
//            if (Keyboard.GetState().IsKeyDown(Keys.A))
//            {
//                newPosX -= dirY * moveSpeed;
//                newPosY += dirX * moveSpeed;
//            }
//            if (Keyboard.GetState().IsKeyDown(Keys.D))
//            {
//                newPosX += dirY * moveSpeed;
//                newPosY -= dirX * moveSpeed;
//            }
//            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
//            {
//                CurrentGameState = GameState.MainMenu;
//            }

//            bool collideX = false;
//            bool collideY = false;

//            for (double i = -CollisionBuffer; i <= CollisionBuffer; i += CollisionBuffer)
//            {
//                for (double j = -CollisionBuffer; j <= CollisionBuffer; j += CollisionBuffer)
//                {
//                    int checkX = (int)(newPosX + i);
//                    int checkY = (int)(posY + j);
//                    if (checkX >= 0 && checkX < map.GetLength(0) &&
//                        checkY >= 0 && checkY < map.GetLength(1))
//                    {
//                        if (map[checkX, checkY] > 0) collideX = true;
//                    }

//                    checkX = (int)(posX + i);
//                    checkY = (int)(newPosY + j);
//                    if (checkX >= 0 && checkX < map.GetLength(0) &&
//                        checkY >= 0 && checkY < map.GetLength(1))
//                    {
//                        if (map[checkX, checkY] > 0) collideY = true;
//                    }
//                }
//            }

//            if (!collideX) posX = newPosX;
//            if (!collideY) posY = newPosY;

//            UpdateRendererData();
//            Mouse.SetPosition(screenWidth / 2, screenHeight / 2);
//            prevMouseState = Mouse.GetState();
//        }

//        private void UpdateRendererData()
//        {
//            _renderer.UpdateData(posX, posY, dirX, dirY, planeX, planeY);
//        }

//        private void DrawAll()
//        {
//            _renderer.DrawCeilingAndFloor();
//            _renderer.DrawWalls(map);
//            _wallDistances = _renderer.GetWallDistances();
//            _renderer.RenderBufferToScreen();
//        }

//        private Dictionary<string, TextureData> _textureCache = new Dictionary<string, TextureData>();

//        private void CheckEnemyHits()
//        {
//            int centerX = screenWidth / 2;
//            float rayDistance = _wallDistances[centerX];

//            foreach (var enemy in _enemies)
//            {
//                if (!enemy.IsActive) continue;

//                Vector2 enemyDir = new Vector2(
//                    enemy.Position.X - (float)posX,
//                    enemy.Position.Y - (float)posY);

//                float enemyDist = enemyDir.Length();

//                if (Math.Abs(enemyDist - rayDistance) < 0.5f)
//                {
//                    enemy.TakeDamage(50); // Урон из дробовика
//                    if (enemy.Health <= 0)
//                    {
//                        enemy.IsActive = false;
//                    }
//                    break;
//                }
//            }
//        }
//    }
//}