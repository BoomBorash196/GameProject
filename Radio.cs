using GameProject;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using static GameProject.Game1;
using System;

namespace GameProject
{
    public class Radio
    {
        private SpriteFont _font;
        private string[] _radioItems = {  
            $"Станция: {_stationItems[_currentStation]}",                                          
            "Громкость: 50%",                                           
            "Вкл/Выкл: Выкл",                                            
            "Назад"
        };
        private int _selectedIndex = 0;
        private int[] _typingProgress;
        private float _typingSpeed = 0.1f;
        private float _typingTimer = 0f;
        private KeyboardState _prevKeyboardState;
        private Texture2D _backgroundTexture;
        private int _volume = 30;
        public bool _isRadioOn = true;
        private static int _currentStation = 0;
        private static string[] _stationItems = { 
            "Bloody Bathroom FM",                        
            "Witch Broom FM",
            "133.7 FM",
        };
        private int _totalStations = _stationItems.Length;
        private SoundEffect[] _stationTracks;
        private SoundEffectInstance _currentTrack; 
        private Switcher _menuSwitcher;

        public Radio(SpriteFont font, Texture2D backgroundTexture, SoundEffect[] stationTracks)
        {
            _font = font;
            _backgroundTexture = backgroundTexture;
            _stationTracks = stationTracks;
            _prevKeyboardState = Keyboard.GetState();
            _typingProgress = new int[_radioItems.Length];
            _menuSwitcher = new Switcher();
            UpdateMenuItems();
        }

        private void UpdateMenuItems()
        {
            _radioItems[0] = $"Станция: {_stationItems[_currentStation]}";
            _radioItems[1] = $"Громкость: {_volume}%";
            _radioItems[2] = $"Вкл/Выкл: {(_isRadioOn ? "Вкл" : "Выкл")}";

            for (int i = 0; i < _radioItems.Length; i++)
                _typingProgress[i] = _radioItems[i].Length;
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            _selectedIndex = _menuSwitcher.MenuSwitcher(keyboardState, _selectedIndex, _radioItems.Length);
            _menuSwitcher.UpdateState(keyboardState);

            switch (_selectedIndex)
            {
                case 1:
                    if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
                        _volume = Math.Min(_volume + 10, 100);
                    if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
                        _volume = Math.Max(_volume - 10, 0);
                    UpdateVolume();
                    UpdateMenuItems();
                    break;

                case 0:
                    if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
                    {
                        _currentStation = (_currentStation + 1) % _totalStations;
                        PlayCurrentStation();
                    }
                    if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
                    {
                        _currentStation = (_currentStation - 1 + _totalStations) % _totalStations;
                        PlayCurrentStation();
                    }
                    UpdateMenuItems();
                    break;

                case 2: 
                    if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
                    {
                        _isRadioOn = !_isRadioOn;
                        ToggleRadio();
                        UpdateMenuItems();
                    }
                    break;

                case 3: 
                    if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
                        Game1.CurrentGameState = GameState.MainMenu;
                    break;
            }

            _prevKeyboardState = keyboardState;
        }

        private void UpdateVolume()
        {
            if (_currentTrack != null)
                _currentTrack.Volume = _volume / 100f;
        }

        public void PlayCurrentStation()
        {
            StopRadio();
            if (!_isRadioOn) return;

            _currentTrack = _stationTracks[_currentStation].CreateInstance();
            _currentTrack.Volume = _volume / 100f;
            _currentTrack.IsLooped = true;
            _currentTrack.Play();
        }

        public void StopRadio()
        {
            _currentTrack?.Stop();
            _currentTrack?.Dispose();
            _currentTrack = null;
        }

        private void ToggleRadio()
        {
            if (_isRadioOn)
                PlayCurrentStation();
            else
                StopRadio();
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            
            if (_backgroundTexture != null)
                spriteBatch.Draw(_backgroundTexture, graphicsDevice.Viewport.Bounds, Color.White);
            else
                graphicsDevice.Clear(Color.DarkSlateBlue);

            float startY = graphicsDevice.Viewport.Height * 0.3f;
            for (int i = 0; i < _radioItems.Length; i++)
            {
                string text = _radioItems[i].Substring(0, _typingProgress[i]);
                Vector2 pos = new Vector2(
                    graphicsDevice.Viewport.Width / 2 - _font.MeasureString(text).X / 2,
                    startY + i * 50
                );

                spriteBatch.DrawString(_font, text, pos + Vector2.One, Color.Black * 0.5f);
                spriteBatch.DrawString(_font, text, pos,
                    _selectedIndex == i ? Color.Red : Color.White);
            }

            spriteBatch.End();
        }

        public void Reset()
        {
            _selectedIndex = 0;
            _typingProgress = new int[_radioItems.Length];
            UpdateMenuItems();
        }
    }
}