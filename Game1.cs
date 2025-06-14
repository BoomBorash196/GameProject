﻿using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System;


namespace GameProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private LevelManager _levelManager;
        private int[,] map;

        public double posX = 1.5, posY = 1.5;
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

        public static int screenWidth = 640;
        public static int screenHeight = 480;

        public enum GameState
        {
            MainMenu,
            Intro,
            Playing,
            Settings,
            SoundSettings,
            ControlsSettings,
            Radio,
            Exit,
            GameOver
        }
        public static GameState CurrentGameState = GameState.MainMenu;
        public static GameState _prevGameState = GameState.MainMenu;

        private int[,] _map;
        public int[,] Map 
        { 
            get => _map;
            set => _map = value;
        }

        private MainMenu _mainMenu;
        private SpriteFont _font;
        private SettingsMenu _settings;
        private SoundMenu _soundMenu;
        private ControlsMenu _controlsMenu;
        private IntroScreen _introScreen;

        private Texture2D _backgroundTexture;
        private float[] _wallDistances;

        public Radio _radioMenu;
        public SoundEffect[] _radioTracks;

        private SoundEffect _backgroundMusic;
        private SoundEffectInstance _backgroundMusicInstance;

        private Renderer _renderer;
        private GraphicsManager _graphicsManager;

        private KeyboardState _prevKeyboardState;

        private Shotgun _shotgun;
        private List<Texture2D> _shotgunFrames;
        private Texture2D _shotgunTexture;
        private Texture2D _bulletTexture;
        private List<Texture2D> _muzzleFlashTextures;
        private SoundEffect _shotgunSound;
        private SoundEffectInstance _shotgunSoundInstance;

        private Minimap _minimap;

        private List<Enemy> _enemies;
       
        private GameLogic _gameLogic;
        private GameOverScreen _gameOverScreen;

        public int playerHealth = 100;
        private HUD _hud;

        private bool _escapePressed = false;
        private float _escapeDelay = 0f;
        private const float ESCAPE_DELAY_TIME = 0.5f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
            _levelManager = new LevelManager();
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

            var currentLevel = _levelManager.GetCurrentLevel();
            posX = currentLevel.PlayerSpawnPoint.X;
            posY = currentLevel.PlayerSpawnPoint.Y;
            map = currentLevel.Map;

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

            _backgroundMusic = Content.Load<SoundEffect>("background_sounds/bgSound");
            _backgroundMusicInstance = _backgroundMusic.CreateInstance();
            _backgroundMusicInstance.IsLooped = true;
            _backgroundMusicInstance.Play();

            _mainMenu = new MainMenu(_font, _backgroundTexture);
            _settings = new SettingsMenu(_font, _backgroundTexture, this);
            _soundMenu = new SoundMenu(_font, _backgroundTexture, _backgroundMusicInstance);
            _controlsMenu = new ControlsMenu(_font, _backgroundTexture);
            _radioMenu = new Radio(_font, _backgroundTexture, _radioTracks);
            _introScreen = new IntroScreen(_font, _backgroundTexture, 
                "В 2025 году мир погрузился в хаос.\n\n" +
                "Города опустели, технологии вышли из-под контроля.\n" +
                "Человечество оказалось на грани выживания.\n\n" +
                "Вы сражаетесь в подземных катакомбах\n" +
                "Ваша миссия - выбраться живым.\n" +
                "Следуйте за фиолетовыми орлами.\n" +
                "Но будьте осторожны - враг вездесущ и опасен.\n\n" +
                "В вашем распоряжении только дробовик и ваша смекалка.\n" +
                "Используйте их с умом, и возможно, вы сможете спастись.\n\n" +
                "Нажмите SPACE для продолжения...");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            var levels = new List<int[,]>();
            for (int i = 1; i <= 10; i++)
            {
                var level = _levelManager.GetLevel(i);
                if (level != null)
                {
                    levels.Add(level.Map);
                }
            }
            _minimap = new Minimap(pixel, levels);
            _shotgunFrames = new List<Texture2D>
            {
                Content.Load<Texture2D>("weapons/shotgun"),
                Content.Load<Texture2D>("weapons/shotgun"),
                Content.Load<Texture2D>("weapons/shotgun1"),
                Content.Load<Texture2D>("weapons/shotgun2"),
                Content.Load<Texture2D>("weapons/shotgun3")
            };
            _shotgunTexture = _shotgunFrames[0];
            _bulletTexture = Content.Load<Texture2D>("weapons/bullet");
            _muzzleFlashTextures = new List<Texture2D>
            {
                Content.Load<Texture2D>("weapons/shotgun_fire_effect1"),
                Content.Load<Texture2D>("weapons/shotgun_fire_effect2")
            };
            _shotgunSound = Content.Load<SoundEffect>("weapons/sounds/shotgun_sound");

            wallTextures = new List<TextureData>
            {
                LoadTextureData("world_textures/redbrick"), //1
                LoadTextureData("world_textures/eagle"), //2
                LoadTextureData("world_textures/mossy"), //3 лампочки
                LoadTextureData("world_textures/wood"), //4 
                LoadTextureData("world_textures/green"), //5
                LoadTextureData("world_textures/mossy"), //6
                LoadTextureData("world_textures/bluestone"), //7
                LoadTextureData("world_textures/barrel", true), //8 не используем
                LoadTextureData("world_textures/pillar", true), //9 не используем
                LoadTextureData("world_textures/colorstone"), //10
                LoadTextureData("world_textures/purplestone"), //11
                LoadTextureData("world_textures/greystone"), //12
                LoadTextureData("world_textures/YouWin"), //13
            };

            _renderer = new Renderer(_spriteBatch, GraphicsDevice, wallTextures,
                screenWidth, screenHeight, posX, posY, dirX, dirY, planeX, planeY);
            _renderer.SetCurrentMap(map);

            _enemies = new List<Enemy>();
            _gameLogic = new GameLogic(Content, pixel, _levelManager, _enemies, _renderer, this);

            _shotgun = new Shotgun(_shotgunFrames, _bulletTexture,
                                   _muzzleFlashTextures, _shotgunSound,
                                   GraphicsDevice, screenWidth,
                                   screenHeight, _wallDistances, _enemies, map);

            _graphicsManager = new GraphicsManager(
                _spriteBatch, 
                GraphicsDevice, 
                _renderer, 
                pixel, 
                screenWidth, 
                screenHeight,
                _mainMenu,
                _settings,
                _soundMenu,
                _controlsMenu,
                _radioMenu,
                _shotgun,
                _minimap,
                _introScreen);

            _hud = new HUD(_font, pixel, screenWidth, screenHeight);
            _gameOverScreen = new GameOverScreen(_font, _backgroundTexture);
        }

        private TextureData LoadTextureData(string texturePath, bool makeTransparent = false)
        {
            if (_textureCache.ContainsKey(texturePath))
            {
                return _textureCache[texturePath];
            }

            try
            {
                Texture2D originalTexture = Content.Load<Texture2D>(texturePath);
                Color[] data = new Color[originalTexture.Width * originalTexture.Height];
                originalTexture.GetData(data);


                Texture2D processedTexture = new Texture2D(GraphicsDevice,
                                                         originalTexture.Width,
                                                         originalTexture.Height);
                processedTexture.SetData(data);

                var textureData = new TextureData
                {
                    Texture = processedTexture,
                    Data = data
                };

                _textureCache[texturePath] = textureData;
                return textureData;
            }
            catch (ContentLoadException ex)
            {

                Texture2D fallback = new Texture2D(GraphicsDevice, 1, 1);
                fallback.SetData(new[] { Color.White });

                return new TextureData
                {
                    Texture = fallback,
                    Data = new[] { Color.White }
                };
            }
        }

       

        protected override void Update(GameTime gameTime)
        {
            if (CurrentGameState == GameState.Exit)
                Exit();

            KeyboardState keyState = Keyboard.GetState();
            
            

            if (_prevGameState != CurrentGameState)
            {
                
                
                if (_prevGameState == GameState.Intro && CurrentGameState == GameState.Playing)
                {
                    _backgroundMusicInstance?.Stop();
                }
                else if (CurrentGameState == GameState.Intro && _prevGameState != GameState.Intro ||
                    (_prevGameState == GameState.Playing && CurrentGameState == GameState.MainMenu) ||
                    _prevGameState == GameState.GameOver && CurrentGameState == GameState.MainMenu)
                {
                    _backgroundMusicInstance?.Play();
                }

                if (_prevGameState == GameState.Intro && CurrentGameState == GameState.Playing)
                {
                    _radioMenu?.PlayCurrentStation();
                }
                else if (CurrentGameState == GameState.Intro && _prevGameState != GameState.Intro || 
                    (_prevGameState == GameState.Playing && CurrentGameState == GameState.MainMenu) ||
                    CurrentGameState == GameState.GameOver)
                {
                    _radioMenu?.StopRadio();
                }

                _prevGameState = CurrentGameState;
                if (CurrentGameState == GameState.MainMenu)
                {
                    _mainMenu.Reset();
                    _prevKeyboardState = keyState;
                }
                else if (CurrentGameState == GameState.Intro)
                    _introScreen.Reset();
                else if (CurrentGameState == GameState.Settings)
                    _settings.Reset();
                else if (CurrentGameState == GameState.SoundSettings)
                    _soundMenu.Reset();
                else if (CurrentGameState == GameState.ControlsSettings)
                    _controlsMenu.Reset();
            }

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    _mainMenu.Update(gameTime);
                    break;
                case GameState.Intro:
                    _introScreen.Update(gameTime);
                    break;
                case GameState.Playing:
                    UpdateGame(gameTime);
                    if (keyState.IsKeyDown(Keys.Escape))
                    {
                        CurrentGameState = GameState.MainMenu;
                    }
                    break;
                case GameState.Settings:
                    _settings.Update(gameTime);
                    break;
                case GameState.SoundSettings:
                    _soundMenu.Update(gameTime);
                    break;
                case GameState.ControlsSettings:
                    _controlsMenu.Update(gameTime);
                    break;
                case GameState.Radio:
                    _radioMenu.Update(gameTime);
                    break;
                case GameState.GameOver:
                    _gameOverScreen.Update(gameTime);
                    break;
            }

            _prevKeyboardState = keyState;
            base.Update(gameTime);
        }

        private void UpdateGame(GameTime gameTime)
        {
            UpdateCamera((float)gameTime.ElapsedGameTime.TotalSeconds);
            Vector2 playerPosition = new Vector2((float)posX, (float)posY);
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _renderer.UpdateEnemies(_enemies, playerPosition, gameTime);
            _shotgun.Update(gameTime, playerPosition, mousePosition, posX, posY, dirX, dirY, planeX, planeY, _wallDistances, _enemies, map);
            _gameLogic.Update(gameTime);
            _minimap.Update(playerPosition, _levelManager.CurrentLevelIndex - 1);
            _radioMenu.Update(gameTime);
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

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (!_escapePressed)
                {
                    _escapePressed = true;
                    _escapeDelay = ESCAPE_DELAY_TIME;
                }
            }
            else
            {
                _escapePressed = false;
            }

            if (_escapePressed)
            {
                _escapeDelay -= deltaTime;
                if (_escapeDelay <= 0)
                {
                    if (CurrentGameState == GameState.Playing)
                    {
                        CurrentGameState = GameState.MainMenu;
                    }
                    _escapePressed = false;
                }
            }

            _gameLogic.CollisionsBuffer(newPosX, newPosY, ref posX, ref posY, map);

            if (_levelManager.IsAtExitPoint(new Vector2((float)posX, (float)posY)))
            {
                if (_levelManager.TryLoadNextLevel())
                {
                    var nextLevel = _levelManager.GetCurrentLevel();
                    posX = nextLevel.PlayerSpawnPoint.X;
                    posY = nextLevel.PlayerSpawnPoint.Y;
                    map = nextLevel.Map;
                    _renderer.SetCurrentMap(map);
                    _enemies.Clear();
                    _gameLogic.SpawnEnemies(nextLevel.EnemyCount);
                }
            }

            _gameLogic.UpdateRendererData(posX, posY, dirX, dirY, planeX, planeY);

            Mouse.SetPosition(screenWidth / 2, screenHeight / 2);
            prevMouseState = Mouse.GetState();
        }

        private Dictionary<string, TextureData> _textureCache = new Dictionary<string, TextureData>();

        protected override void Draw(GameTime gameTime)
        {
            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    _mainMenu.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.Intro:
                    _introScreen.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.Playing:
                    GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.Clear(Color.Transparent);
                    _graphicsManager.Draw(map, _enemies, new Vector2((float)posX, (float)posY), (float)Math.Atan2(dirY, dirX));
                    _spriteBatch.Begin();
                    _hud.Draw(_spriteBatch, playerHealth);
                    _spriteBatch.End();
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
                case GameState.GameOver:
                    _gameOverScreen.Draw(_spriteBatch, GraphicsDevice);
                    break;
            }
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _backgroundMusicInstance?.Stop();
            _backgroundMusicInstance?.Dispose();
            base.UnloadContent();
        }
    }
}