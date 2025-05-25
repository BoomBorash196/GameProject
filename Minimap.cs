using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject
{
    public class Minimap
    {
        private Texture2D _pixel;
        private int[,] _map;
        private Vector2 _playerPosition;
        private int _tileSize = 4; 
        private Vector2 _position;

        public Minimap(Texture2D pixel, int[,] map)
        {
            _pixel = pixel;
            _map = map;
            _position = new Vector2(10, 10); 
        }

        public void Update(Vector2 playerPosition)
        {
            _playerPosition = playerPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, new Rectangle((int)_position.X - 2, (int)_position.Y - 2,
                _map.GetLength(0) * _tileSize + 4, _map.GetLength(1) * _tileSize + 4), Color.Black);

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    if (_map[x, y] > 0)
                    {
                        spriteBatch.Draw(_pixel, new Rectangle(
                            (int)_position.X + x * _tileSize,
                            (int)_position.Y + y * _tileSize,
                            _tileSize, _tileSize), Color.Red);
                    }
                }
            }

            spriteBatch.Draw(_pixel, new Rectangle(
                (int)_position.X + (int)(_playerPosition.X * _tileSize),
                (int)_position.Y + (int)(_playerPosition.Y * _tileSize),
                _tileSize / 2, _tileSize / 2), Color.Blue);
        }
    }
}