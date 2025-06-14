using GameProject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static GameProject.Game1;
using System.Diagnostics;

namespace GameProject
{
    public class MainMenu
    {
        private readonly SpriteFont _font;
        private readonly string[] _menuItems = { "Играть", "Настройки", "Радио", "Выход" };
        private readonly string _menuTitle ="ВТОРЖЕНИЕ";
        private int _selectedIndex = 0;
        private KeyboardState _prevKeyboardState;
        private readonly Texture2D _backgroundTexture;
        private int[] _typingProgress;
        private float _typingSpeed = 0.1f;
        private float _typingTimer = 0f;
        private readonly Switcher _menuSwitcher;

        public MainMenu(SpriteFont font, Texture2D backgroundTexture)
        {
            _font = font;
            _backgroundTexture = backgroundTexture;
            _prevKeyboardState = Keyboard.GetState();
            _typingProgress = new int[_menuItems.Length];
            _menuSwitcher = new Switcher();
            SoundMenu._soundOn = true;
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
            {
                _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
            {
                _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
            }

            if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
            {
                HandleMenuSelection();
            }

            UpdateTypingAnimation(gameTime);

            _prevKeyboardState = keyboardState;
        }

        private void HandleMenuSelection()
        {
            switch (_selectedIndex)
            {
                case 0:
                    Game1.CurrentGameState = GameState.Intro;
                    break;
                case 1:
                    Game1.CurrentGameState = GameState.Settings;
                    break;
                case 2:
                    Game1.CurrentGameState = GameState.Radio;
                    break;
                case 3:
                    Game1.CurrentGameState = GameState.Exit;
                    break;
            }
        }

        private void UpdateTypingAnimation(GameTime gameTime)
        {
            _typingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_typingTimer >= _typingSpeed)
            {
                _typingTimer = 0f;
                for (int i = 0; i < _menuItems.Length; i++)
                {
                    if (_typingProgress[i] < _menuItems[i].Length)
                    {
                        _typingProgress[i]++;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            DrawBackground(spriteBatch, graphicsDevice);
            DrawMenuItems(spriteBatch, graphicsDevice);
            DrawTitle(spriteBatch, graphicsDevice);

            spriteBatch.End();
        }

        private void DrawBackground(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, 
                    new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), 
                    Color.White);
            }
            else
            {
                graphicsDevice.Clear(Color.DarkBlue);
            }
        }

        private void DrawTitle(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            float menuHeight = _menuItems.Length * 50;
            float startY = (graphicsDevice.Viewport.Height - menuHeight) / 2;

            
            Vector2 textSize = _font.MeasureString(_menuTitle);
            float x = (graphicsDevice.Viewport.Width - textSize.X) / 6;
            float y = startY - 100;
            Color color = Color.White;

            DrawMenuItemWithShadow(spriteBatch, _menuTitle, x, y, color, 3f);
        }

        private void DrawMenuItems(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            float menuHeight = _menuItems.Length * 50;
            float startY = (graphicsDevice.Viewport.Height - menuHeight) / 2;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                string menuItem = _menuItems[i].Substring(0, _typingProgress[i]);
                Vector2 textSize = _font.MeasureString(menuItem);
                float x = (graphicsDevice.Viewport.Width - textSize.X) / 2;
                float y = startY + i * 50;
                Color color = (i == _selectedIndex) ? Color.Red : Color.White;

                DrawMenuItemWithShadow(spriteBatch, menuItem, x, y, color, 1f);
            }
        }

        private void DrawMenuItemWithShadow(SpriteBatch spriteBatch, string text, float x, float y, Color color, float scale)
        {
            
            Vector2 shadowOffset = new Vector2(2, 2);

            spriteBatch.DrawString(_font, text, 
                new Vector2(x + shadowOffset.X, y + shadowOffset.Y), 
                Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, text, 
                new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public void Reset()
        {
            _typingProgress = new int[_menuItems.Length];
            _typingTimer = 0f;
            _selectedIndex = 0;
            _prevKeyboardState = Keyboard.GetState();
        }
    }
}