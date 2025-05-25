using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int[,] map;
        private int currentLevel = 1;
        private List<int[,]> levels = new List<int[,]>();
        private bool isTransitioning = false;
        private float transitionAlpha = 0f;
        private SoundEffect teleportSound;

        private double posX = 1.5, posY = 1.5;
        private double dirX = -1.0, dirY = 0.0;
        private double planeX = 0.0, planeY = 0.66;

        private float mouseSensitivity = 0.002f;
        private MouseState prevMouseState;

        private Texture2D pixel;
        private List<TextureData> wallTextures;
        private const double CollisionBuffer = 0.3;

        public class TextureData
        {
            public Texture2D Texture { get; set; }
            public Color[] Data { get; set; }
        }

        private int screenWidth = 640;
        private int screenHeight = 480;

        public enum GameState
        {
            MainMenu,
            Playing,
            Settings,
            SoundSettings,
            ControlsSettings,
            Radio,
            Exit
        }
        public static GameState CurrentGameState = GameState.MainMenu;

        private MainMenu _mainMenu;
        private SpriteFont _font;
        private SettingsMenu _settings;
        private SoundMenu _soundMenu;
        private ControlsMenu _controlsMenu;

        private Texture2D _backgroundTexture;
        private float[] _wallDistances;

        public Radio _radioMenu;
        public SoundEffect[] _radioTracks;

        private SoundEffect _backgroundMusic;
        private SoundEffectInstance _backgroundMusicInstance;

        private Renderer _renderer;

        private KeyboardState _prevKeyboardState;
        public static GameState _prevGameState = GameState.MainMenu;

        private Shotgun _shotgun;
        private List<Texture2D> _shotgunFrames;
        private Texture2D _shotgunTexture;
        private Texture2D _bulletTexture;
        private List<Texture2D> _muzzleFlashTextures;
        private SoundEffect _shotgunSound;
        private SoundEffectInstance _shotgunSoundInstance;

        private Minimap _minimap;

        private List<Enemy> _enemies;
        private float _spawnTimer = 0f;
        private float _spawnInterval = 10f;
        private int _maxEnemies = 5;

        private Dictionary<string, TextureData> _textureCache = new Dictionary<string, TextureData>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = false;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        }

        protected override void Initialize()
        {
            Mouse.SetPosition(
                GraphicsDevice.Viewport.Width / 2,
                GraphicsDevice.Viewport.Height / 2
            );
            prevMouseState = Mouse.GetState();

            _prevKeyboardState = Keyboard.GetState();
            _wallDistances = new float[screenWidth];

            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;

            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _radioTracks = new SoundEffect[]
            {
                Content.Load<SoundEffect>("Radio/antisocial"),
                Content.Load<SoundEffect>("Radio/ashes"),
                Content.Load<SoundEffect>("Radio/Shikami")
            };
            _backgroundTexture = Content.Load<Texture2D>("backgrounds/bg3-back");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("RetroFont");

            teleportSound = Content.Load<SoundEffect>("background_sounds/bgSound");
            levels.Add(Map.GiveMap());
            levels.Add(Map.LoadLevel2());

            _backgroundMusic = Content.Load<SoundEffect>("background_sounds/bgSound");
            _backgroundMusicInstance = _backgroundMusic.CreateInstance();
            _backgroundMusicInstance.IsLooped = true;
            _backgroundMusicInstance.Play();

            _mainMenu = new MainMenu(_font, _backgroundTexture);
            _settings = new SettingsMenu(_font, _backgroundTexture, this);
            _soundMenu = new SoundMenu(_font, _backgroundTexture, _backgroundMusicInstance);
            _controlsMenu = new ControlsMenu(_font, _backgroundTexture);
            _radioMenu = new Radio(_font, _backgroundTexture, _radioTracks);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _shotgunFrames = new List<Texture2D>
            {
                Content.Load<Texture2D>("weapons/shotgun"),
                Content.Load<Texture2D>("weapons/shotgun"),
                Content.Load<Texture2D>("weapons/shotgun1"),
                Content.Load<Texture2D>("weapons/shotgun2"),
                Content.Load<Texture2D>("weapons/shotgun3")
            };
            _bulletTexture = Content.Load<Texture2D>("weapons/bullet");
            _muzzleFlashTextures = new List<Texture2D>
            {
                Content.Load<Texture2D>("weapons/shotgun_fire_effect1"),
                Content.Load<Texture2D>("weapons/shotgun_fire_effect2")
            };
            _shotgunSound = Content.Load<SoundEffect>("weapons/sounds/shotgun_sound");
            _shotgun = new Shotgun(_shotgunFrames, _bulletTexture, _muzzleFlashTextures, _shotgunSound, GraphicsDevice, screenWidth, screenHeight, _wallDistances, _enemies);
            wallTextures = new List<TextureData>
            {
                LoadTextureData("world_textures/redbrick"),
                LoadTextureData("world_textures/eagle"),
                LoadTextureData("world_textures/greenlight"),
                LoadTextureData("world_textures/wood"),
                LoadTextureData("world_textures/green"),
                LoadTextureData("world_textures/mossy"),
                LoadTextureData("world_textures/mossy"),
            };

            LoadLevel(currentLevel);
        }

        private TextureData LoadTextureData(string texturePath)
        {
            if (_textureCache.ContainsKey(texturePath))
                return _textureCache[texturePath];

            try
            {
                Texture2D texture = Content.Load<Texture2D>(texturePath);
                Color[] data = new Color[texture.Width * texture.Height];
                texture.GetData(data);

                var textureData = new TextureData { Texture = texture, Data = data };
                _textureCache[texturePath] = textureData;
                return textureData;
            }
            catch (ContentLoadException ex)
            {
                Debug.WriteLine($"Ошибка загрузки текстуры {texturePath}: {ex.Message}");
                throw;
            }
        }

        private void LoadLevel(int levelIndex)
        {
            if (levelIndex < levels.Count)
            {
                map = levels[levelIndex];
                var entryPos = Map.FindSpecialPoint(map, Map.ENTRY_POINT);
                posX = entryPos.X;
                posY = entryPos.Y;

                _renderer = new Renderer(_spriteBatch, GraphicsDevice, wallTextures,
                                       screenWidth, screenHeight, posX, posY,
                                       dirX, dirY, planeX, planeY);

                _enemies = new List<Enemy>();
                SpawnEnemies(1);
            }
            else
            {
                CurrentGameState = GameState.MainMenu;
            }
        }

        private void SpawnEnemies(int enemyCount)
        {
            Texture2D[] enemyFrames = new Texture2D[6];
            try
            {
                enemyFrames[0] = Content.Load<Texture2D>("enemies/bigBoy/walk (1)");
                enemyFrames[1] = Content.Load<Texture2D>("enemies/bigBoy/walk (2)");
                enemyFrames[2] = Content.Load<Texture2D>("enemies/bigBoy/walk (3)");
                enemyFrames[3] = Content.Load<Texture2D>("enemies/bigBoy/walk (4)");
                enemyFrames[4] = Content.Load<Texture2D>("enemies/bigBoy/walk (5)");
                enemyFrames[5] = Content.Load<Texture2D>("enemies/bigBoy/walk (6)");
            }
            catch
            {
                enemyFrames = new Texture2D[] { pixel };
            }

            Random rand = new Random();
            List<Vector2> freeCells = new List<Vector2>();
            Vector2 playerSpawn = new Vector2((float)posX, (float)posY);

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] == 0)
                    {
                        Vector2 cellPos = new Vector2(x + 0.5f, y + 0.5f);
                        if (Vector2.Distance(playerSpawn, cellPos) >= 10f)
                            freeCells.Add(cellPos);
                    }
                }
            }

            enemyCount = Math.Min(enemyCount, freeCells.Count);
            for (int i = 0; i < enemyCount; i++)
            {
                if (freeCells.Count == 0) break;

                int index = rand.Next(freeCells.Count);
                Vector2 position = freeCells[index];
                freeCells.RemoveAt(index);

                _enemies.Add(new Enemy
                {
                    Position = position,
                    AnimationFrames = enemyFrames,
                    CurrentFrame = 0,
                    FrameTime = 0.1f,
                    Speed = 0.05f,
                    Health = 100,
                    Size = new Vector2(64, 64)
                });
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (CurrentGameState == GameState.Exit)
                Exit();

            KeyboardState keyState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyState.IsKeyDown(Keys.Escape))
                HandleEscapeKey();

            if (_prevGameState != CurrentGameState)
                HandleGameStateChange();

            _prevKeyboardState = keyState;

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    _mainMenu.Update(gameTime);
                    break;
                case GameState.Playing:
                    UpdatePlayingState(deltaTime, gameTime);
                    break;
                case GameState.Settings:
                    _settings.Update(gameTime);
                    break;
                case GameState.SoundSettings:
                    _soundMenu.Update(gameTime);
                    break;
                case GameState.Radio:
                    _radioMenu.Update(gameTime);
                    break;
                case GameState.ControlsSettings:
                    _controlsMenu.Update(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        private void HandleEscapeKey()
        {
            if (CurrentGameState == GameState.Playing)
                CurrentGameState = GameState.MainMenu;
            else if (CurrentGameState == GameState.MainMenu)
                CurrentGameState = GameState.Exit;
        }

        private void HandleGameStateChange()
        {
            if (_prevGameState == GameState.MainMenu && CurrentGameState == GameState.Playing)
            {
                _backgroundMusicInstance?.Stop();
                LoadLevel(currentLevel);
            }
            else if (CurrentGameState == GameState.MainMenu && _prevGameState != GameState.MainMenu)
            {
                _backgroundMusicInstance?.Play();
            }

            switch (CurrentGameState)
            {
                case GameState.MainMenu: _mainMenu.Reset(); break;
                case GameState.Settings: _settings.Reset(); break;
                case GameState.SoundSettings: _soundMenu.Reset(); break;
                case GameState.ControlsSettings: _controlsMenu.Reset(); break;
            }

            _prevGameState = CurrentGameState;
        }

        private void UpdatePlayingState(float deltaTime, GameTime gameTime)
        {
            if (!isTransitioning)
            {
                UpdateCamera(deltaTime);
                Vector2 playerPos = new Vector2((float)posX, (float)posY);
                Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                _renderer.UpdateEnemies(_enemies, playerPos, gameTime);
                _shotgun.Update(gameTime, playerPos, mousePos, posX, posY,
                                dirX, dirY, planeX, planeY, _wallDistances, _enemies);

                _spawnTimer += deltaTime;
                if (_spawnTimer >= _spawnInterval && _enemies.Count < _maxEnemies)
                {
                    _spawnTimer = 0f;
                    SpawnEnemies(1);
                }

                if (Map.IsPointType(map, playerPos, Map.EXIT_POINT))
                    StartLevelTransition();
            }
            else
            {
                UpdateLevelTransition(deltaTime);
            }
        }

        private void UpdateCamera(float deltaTime)
        {
            float moveSpeed = 5.0f * deltaTime;
            MouseState currentMouseState = Mouse.GetState();

            int deltaX = currentMouseState.X - prevMouseState.X;
            double rotation = -deltaX * mouseSensitivity;

            double oldDirX = dirX;
            dirX = dirX * Math.Cos(rotation) - dirY * Math.Sin(rotation);
            dirY = oldDirX * Math.Sin(rotation) + dirY * Math.Cos(rotation);

            double oldPlaneX = planeX;
            planeX = planeX * Math.Cos(rotation) - planeY * Math.Sin(rotation);
            planeY = oldPlaneX * Math.Sin(rotation) + planeY * Math.Cos(rotation);

            double newPosX = posX;
            double newPosY = posY;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                newPosX += dirX * moveSpeed;
                newPosY += dirY * moveSpeed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                newPosX -= dirX * moveSpeed;
                newPosY -= dirY * moveSpeed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                newPosX -= dirY * moveSpeed;
                newPosY += dirX * moveSpeed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                newPosX += dirY * moveSpeed;
                newPosY -= dirX * moveSpeed;
            }

            HandleCollisions(newPosX, newPosY);
            Mouse.SetPosition(screenWidth / 2, screenHeight / 2);
            prevMouseState = currentMouseState;
            _renderer.UpdateData(posX, posY, dirX, dirY, planeX, planeY);
        }

        private void HandleCollisions(double newPosX, double newPosY)
        {
            bool collideX = false, collideY = false;

            for (double i = -CollisionBuffer; i <= CollisionBuffer; i += CollisionBuffer)
            {
                for (double j = -CollisionBuffer; j <= CollisionBuffer; j += CollisionBuffer)
                {
                    int checkX = (int)(newPosX + i);
                    int checkY = (int)(posY + j);
                    if (checkX >= 0 && checkX < map.GetLength(0) &&
                        checkY >= 0 && checkY < map.GetLength(1) &&
                        map[checkX, checkY] > 0)
                        collideX = true;

                    checkX = (int)(posX + i);
                    checkY = (int)(newPosY + j);
                    if (checkX >= 0 && checkX < map.GetLength(0) &&
                        checkY >= 0 && checkY < map.GetLength(1) &&
                        map[checkX, checkY] > 0)
                        collideY = true;
                }
            }

            if (!collideX) posX = newPosX;
            if (!collideY) posY = newPosY;
        }

        private void StartLevelTransition()
        {
            isTransitioning = true;
            transitionAlpha = 0f;
            teleportSound.Play();
        }

        private void UpdateLevelTransition(float deltaTime)
        {
            transitionAlpha += deltaTime * 2f;
            if (transitionAlpha >= 1f)
            {
                currentLevel++;
                LoadLevel(currentLevel);
                isTransitioning = false;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    _mainMenu.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.Playing:
                    DrawGameScene();
                    break;
                case GameState.Settings:
                    _settings.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.SoundSettings:
                    _soundMenu.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.ControlsSettings:
                    _controlsMenu.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.Radio:
                    _radioMenu.Draw(_spriteBatch, GraphicsDevice);
                    break;
            }

            if (isTransitioning)
                DrawTransitionEffect();

            base.Draw(gameTime);
        }

        private void DrawGameScene()
        {
            float playerRot = (float)Math.Atan2(dirY, dirX);
            _renderer.RenderScene(_spriteBatch, map, _enemies,
                                new Vector2((float)posX, (float)posY), playerRot);
            _wallDistances = _renderer.GetWallDistances();

            _spriteBatch.Begin();
            _shotgun.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        private void DrawTransitionEffect()
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(pixel, new Rectangle(0, 0, screenWidth, screenHeight),
                            Color.Black * transitionAlpha);
            _spriteBatch.End();
        }

        protected override void UnloadContent()
        {
            _backgroundMusicInstance?.Stop();
            _backgroundMusicInstance?.Dispose();
            base.UnloadContent();
        }
    }
}