using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static GameProject.Game1;
using System.Diagnostics;

namespace GameProject
{
    public class IntroScreen
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _backgroundTexture;
        private readonly string _introText;
        private float _alpha = 0f;
        private float _fadeSpeed = 0.5f;
        private bool _isFadingIn = true;
        private KeyboardState _prevKeyboardState;
        private float _displayTime = 0f;
        private const float MaxDisplayTime = 20f;
        private float _skipDelay = 0f;
        private const float SKIP_DELAY_TIME = 5f;

        public IntroScreen(SpriteFont font, Texture2D backgroundTexture, string introText)
        {
            _font = font;
            _backgroundTexture = backgroundTexture;
            _introText = introText;
            _prevKeyboardState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


            if (_isFadingIn)
            {
                _alpha += _fadeSpeed * deltaTime;
                if (_alpha >= 1f)
                {
                    _alpha = 1f;
                    _isFadingIn = false;
                }
            }
            else
            {
                _displayTime += deltaTime;
               
                if (_displayTime >= MaxDisplayTime)
                {
                    _alpha -= _fadeSpeed * deltaTime;
                    if (_alpha <= 0f)
                    {
                        CurrentGameState = GameState.Playing;
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.Space) && !_prevKeyboardState.IsKeyDown(Keys.Space))
            {
                CurrentGameState = GameState.Playing;
            }

            _prevKeyboardState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            
             graphicsDevice.Clear(Color.Black * _alpha);
            

            float scale = 0.6f; 
            Vector2 origin = Vector2.Zero;

            string[] lines = _introText.Split('\n');
            float lineHeight = _font.LineSpacing * scale;
            float totalHeight = lines.Length * lineHeight;
            float startY = (graphicsDevice.Viewport.Height - totalHeight) / 2;

            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 textSize = _font.MeasureString(lines[i]) * scale;
                Vector2 position = new Vector2(
                    (graphicsDevice.Viewport.Width - textSize.X) / 2,
                    startY + i * lineHeight
                );

                spriteBatch.DrawString(_font, lines[i],
                    position + new Vector2(2, 2),
                    Color.Black * _alpha, 0, origin, scale, SpriteEffects.None, 0);

                spriteBatch.DrawString(_font, lines[i],
                    position,
                    Color.White * _alpha, 0, origin, scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        public void Reset()
        {
            _alpha = 0f;
            _isFadingIn = true;
            _displayTime = 0f;
        }
    }
} 