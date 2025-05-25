using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;
using static GameProject.Game1;

namespace GameProject
{
    public class SoundMenu
    {
        private SpriteFont _font;
        private string[] _soundItems = { "Громкость: 10%", "Вкл/Выкл: Вкл", "Назад" };
        private int _selectedIndex = 0;
        private int[] _typingProgress;
        private Switcher _menuSwitcher;
        private float _typingSpeed = 0.1f;
        private float _typingTimer = 0f;
        private KeyboardState _prevKeyboardState;
        private Texture2D _backgroundTexture;
        public static int _volume = 10;
        public static bool _soundOn = true;
        private SoundEffectInstance _musicInstance;

        public SoundMenu(SpriteFont font, Texture2D backgroundTexture, SoundEffectInstance musicInstance)
        {
            _font = font;
            _backgroundTexture = backgroundTexture;
            _prevKeyboardState = Keyboard.GetState();
            _typingProgress = new int[_soundItems.Length];
            _menuSwitcher = new Switcher();
            _musicInstance = musicInstance;

            UpdateSoundSettings();
            UpdateMenuItems();
        }

        private void UpdateMenuItems()
        {
            _soundItems[0] = $"Громкость: {_volume}%";
            _soundItems[1] = $"Вкл/Выкл: {(_soundOn ? "Вкл" : "Выкл")}";
            _typingProgress[0] = _soundItems[0].Length;
            _typingProgress[1] = _soundItems[1].Length;
        }

        private void UpdateSoundSettings()
        {
            float volume = _soundOn ? _volume / 100f : 0f;

            if (_musicInstance != null)
            {
                _musicInstance.Volume = volume;
            }

            SoundManager.MasterVolume = volume;
            SoundManager.IsSoundEnabled = _soundOn;
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            _selectedIndex = _menuSwitcher.MenuSwitcher(keyboardState, _selectedIndex, _soundItems.Length);
            _menuSwitcher.UpdateState(keyboardState);

            if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
            {
                switch (_selectedIndex)
                {
                    case 1:
                        _soundOn = !_soundOn;
                        UpdateMenuItems();
                        UpdateSoundSettings();
                        break;

                    case 2:
                        Game1.CurrentGameState = GameState.Settings;
                        break;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
            {
                if (_selectedIndex == 0)
                {
                    _volume = Math.Min(_volume + 10, 100);
                    UpdateMenuItems();
                    UpdateSoundSettings();
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
            {
                if (_selectedIndex == 0)
                {
                    _volume = Math.Max(_volume - 10, 0);
                    UpdateMenuItems();
                    UpdateSoundSettings();
                }
            }

            _typingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_typingTimer >= _typingSpeed)
            {
                _typingTimer = 0f;
                for (int i = 0; i < _soundItems.Length; i++)
                {
                    if (i == 0) continue;
                    if (_typingProgress[i] < _soundItems[i].Length)
                    {
                        _typingProgress[i]++;
                    }
                }
            }

            _prevKeyboardState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

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

            float menuHeight = _soundItems.Length * 50;
            float startY = (graphicsDevice.Viewport.Height - menuHeight) / 2;

            for (int i = 0; i < _soundItems.Length; i++)
            {
                int progress = Math.Min(_typingProgress[i], _soundItems[i].Length);
                string menuItem = _soundItems[i].Substring(0, progress);

                Vector2 textSize = _font.MeasureString(menuItem);
                float x = (graphicsDevice.Viewport.Width - textSize.X) / 2;
                float y = startY + i * 50;
                Color color = (i == _selectedIndex) ? Color.Red : Color.White;

                Vector2 shadowOffset = new Vector2(2, 2);
                spriteBatch.DrawString(_font, menuItem,
                    new Vector2(x + shadowOffset.X, y + shadowOffset.Y),
                    Color.Black * 0.5f);

                spriteBatch.DrawString(_font, menuItem,
                    new Vector2(x, y), color);
            }

            spriteBatch.End();
        }

        public void Reset()
        {
            _typingProgress = new int[_soundItems.Length];
            _typingTimer = 0f;
            _selectedIndex = 0;
            UpdateMenuItems();
        }
    }

    public static class SoundManager
    {
        public static float MasterVolume { get; set; } = 0.5f;
        public static bool IsSoundEnabled { get; set; } = true;

        public static void PlaySound(SoundEffect sound)
        {
            if (IsSoundEnabled)
            {
                sound.Play(MasterVolume, 0f, 0f);
            }
        }
    }
}