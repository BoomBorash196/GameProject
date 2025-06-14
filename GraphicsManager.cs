using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static GameProject.Game1;

namespace GameProject
{
    public class GraphicsManager
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Renderer _renderer;
        private readonly Texture2D _pixel;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly MainMenu _mainMenu;
        private readonly SettingsMenu _settings;
        private readonly SoundMenu _soundMenu;
        private readonly ControlsMenu _controlsMenu;
        private readonly Radio _radioMenu;
        private readonly Shotgun _shotgun;
        private readonly Minimap _minimap;
        private readonly IntroScreen _introScreen;

        public GraphicsManager(
            SpriteBatch spriteBatch,
            GraphicsDevice graphicsDevice,
            Renderer renderer,
            Texture2D pixel,
            int screenWidth,
            int screenHeight,
            MainMenu mainMenu,
            SettingsMenu settings,
            SoundMenu soundMenu,
            ControlsMenu controlsMenu,
            Radio radioMenu,
            Shotgun shotgun, 
            Minimap minimap,
            IntroScreen introScreen)
        {
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
            _renderer = renderer;
            _pixel = pixel;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _mainMenu = mainMenu;
            _settings = settings;
            _soundMenu = soundMenu;
            _controlsMenu = controlsMenu;
            _radioMenu = radioMenu;
            _shotgun = shotgun;
            _minimap = minimap;
            _introScreen = introScreen;
        }

        public void DrawAll(int[,] map, List<Enemy> enemies, Vector2 playerPosition, float playerRotation)
        {
            _renderer.RenderScene(_spriteBatch, map, enemies, playerPosition, playerRotation);
        }

        public void DrawCrosshair()
        {
            int centerX = _screenWidth / 2;
            int centerY = _screenHeight / 2;

            int crosshairLength = 10;
            int crosshairThickness = 2;

            _spriteBatch.Draw(_pixel, 
                new Rectangle(centerX - crosshairLength, centerY - crosshairThickness / 2, 
                crosshairLength * 2, crosshairThickness), Color.White);
            
            _spriteBatch.Draw(_pixel, 
                new Rectangle(centerX - crosshairThickness / 2, centerY - crosshairLength, 
                crosshairThickness, crosshairLength * 2), Color.White);
        }

        public void Draw(int[,] map, List<Enemy> enemies, Vector2 playerPosition, float playerRotation)
        {
            _graphicsDevice.Clear(Color.Black);

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    _mainMenu.Draw(_spriteBatch, _graphicsDevice);
                    break;
                case GameState.Intro:
                    _introScreen.Draw(_spriteBatch, _graphicsDevice);
                    break;
                case GameState.Playing:
                    DrawAll(map, enemies, playerPosition, playerRotation);
                    _spriteBatch.Begin();
                    _shotgun.Draw(_spriteBatch);
                    DrawCrosshair();
                    _minimap.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
                case GameState.Settings:
                    _settings.Draw(_spriteBatch, _graphicsDevice);
                    break;
                case GameState.SoundSettings:
                    _soundMenu.Draw(_spriteBatch, _graphicsDevice);
                    break;
                case GameState.ControlsSettings:
                    _controlsMenu.Draw(_spriteBatch, _graphicsDevice);
                    break;
                case GameState.Radio:
                    _radioMenu.Draw(_spriteBatch, _graphicsDevice);
                    break;
            }
        }
    }
}
