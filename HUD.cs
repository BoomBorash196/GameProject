using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject
{
    public class HUD
    {
        private SpriteFont _font;
        private Texture2D _pixel;
        private int _screenWidth;
        private int _screenHeight;

        public HUD(SpriteFont font, Texture2D pixel, int screenWidth, int screenHeight)
        {
            _font = font;
            _pixel = pixel;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void Draw(SpriteBatch spriteBatch, int health)
        {
            Rectangle hudRect = new Rectangle(0, _screenHeight - 60, _screenWidth, 60);
            spriteBatch.Draw(_pixel, hudRect, Color.Black * 0.7f);

            string hudText = $"Здоровье: {health}";
            spriteBatch.DrawString(_font, hudText, new Vector2(20, _screenHeight - 45), Color.White);
        }
    }
} 