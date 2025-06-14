using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static GameProject.Game1;

namespace GameProject
{
    public class GameOverScreen
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _backgroundTexture;
        private KeyboardState _prevKeyboardState;
        private float _alpha = 0f;
        private float _fadeSpeed = 1f;
        private bool _isFadingIn = true;

        public GameOverScreen(SpriteFont font, Texture2D backgroundTexture)
        {
            _font = font;
            _backgroundTexture = backgroundTexture;
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

            if ((keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space)) && 
                !_prevKeyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Space))
            {
                CurrentGameState = GameState.MainMenu;
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
                    Color.Black * 0.7f * _alpha);
            }
            else
            {
                graphicsDevice.Clear(Color.Black * _alpha);
            }

            string gameOverText = "GAME OVER";
            Vector2 textSize = _font.MeasureString(gameOverText);
            Vector2 position = new Vector2(
                (graphicsDevice.Viewport.Width - textSize.X) / 2,
                (graphicsDevice.Viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, gameOverText,
                position + new Vector2(2, 2),
                Color.Black * _alpha);

            spriteBatch.DrawString(_font, gameOverText,
                position,
                Color.Red * _alpha);

            string instructionText = "Нажмите ENTER или SPACE чтобы вернуться в меню";
            Vector2 instructionSize = _font.MeasureString(instructionText);
            Vector2 instructionPosition = new Vector2(
                (graphicsDevice.Viewport.Width - instructionSize.X) / 2,
                position.Y + textSize.Y + 50
            );

            spriteBatch.DrawString(_font, instructionText,
                instructionPosition,
                Color.White * _alpha);

            spriteBatch.End();
        }

        public void Reset()
        {
            _alpha = 0f;
            _isFadingIn = true;
        }
    }
} 