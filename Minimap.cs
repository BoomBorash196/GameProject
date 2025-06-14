using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameProject
{
    public class Minimap
    {
        private Texture2D _pixel;
        private List<int[,]> _levels;
        private Vector2 _playerPosition;
        private int _tileSize = 4; 
        private Vector2 _position;
        private int _currentLevel;

        public Minimap(Texture2D pixel, List<int[,]> levels)
        {
            _pixel = pixel;
            _levels = levels;
            _position = new Vector2(10, 10); 
            _currentLevel = 0;
        }

        public void Update(Vector2 playerPosition, int currentLevel)
        {
            _playerPosition = playerPosition;
            _currentLevel = currentLevel;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_levels == null || _levels.Count == 0 || _currentLevel >= _levels.Count)
                return;

            var currentMap = _levels[_currentLevel];

            spriteBatch.Draw(_pixel, new Rectangle((int)_position.X - 2, (int)_position.Y - 2,
                currentMap.GetLength(0) * _tileSize + 4, currentMap.GetLength(1) * _tileSize + 4), Color.Black);

            string levelText = $"Level: {_currentLevel + 1}";
            

            for (int x = 0; x < currentMap.GetLength(0); x++)
            {
                for (int y = 0; y < currentMap.GetLength(1); y++)
                {
                    if (currentMap[x, y] > 0)
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