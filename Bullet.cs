using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameProject
{
    public class Bullet
    {
        private Texture2D _texture;
        private Vector2 _position3D;
        private float _positionZ;
        private Vector2 _directionXY;
        private float _directionZ;
        private float _speed = 0.1f;
        private bool _isActive = true;
        private int _screenWidth;
        private int _screenHeight;
        private float _scale;
        private float _distance;
        private float _maxDistance = 100f;
        private double _posX, _posY;
        private double _initialPosX, _initialPosY;
        private double _initialDirX, _initialDirY;
        private double _initialPlaneX, _initialPlaneY;
        private float[] _wallDistances;
        private List<Enemy> _enemies;
        private bool _enableDebugLogging = false; 

        public Bullet(Texture2D texture, Vector2 startPosition3D, float startPositionZ, Vector2 directionXY, float directionZ,
                      int screenWidth, int screenHeight, double posX, double posY, double dirX, double dirY, double planeX, double planeY,
                      float[] wallDistances, List<Enemy> enemies)
        {
            _texture = texture;
            _position3D = startPosition3D;
            _positionZ = startPositionZ;
            _directionXY = directionXY;
            _directionZ = directionZ;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _wallDistances = wallDistances;
            _enemies = enemies;

            _initialPosX = posX;
            _initialPosY = posY;
            _initialDirX = dirX;
            _initialDirY = dirY;
            _initialPlaneX = planeX;
            _initialPlaneY = planeY;

            _posX = posX;
            _posY = posY;

            _scale = 1.0f;
            _distance = 0f;
        }

        public bool IsActive => _isActive;

        public void Update(GameTime gameTime, double posX, double posY)
        {
            if (!_isActive) return;

            _posX = posX;
            _posY = posY;

            _position3D += _directionXY * _speed;
            _positionZ += _directionZ * _speed;

            _distance = Vector2.Distance(new Vector2((float)_posX, (float)_posY), _position3D);

            if (_distance >= _maxDistance)
            {
                _isActive = false;
                return;
            }

            int mapX = (int)_position3D.X;
            int mapY = (int)_position3D.Y;
            int[,] map = Map.GiveMap();
            if (mapX >= 0 && mapX < map.GetLength(0) && mapY >= 0 && mapY < map.GetLength(1))
            {
                if (map[mapX, mapY] > 0)
                {
                    _isActive = false;
                    return;
                }
            }
            else
            {
                
                _isActive = false;
                return;
            }

            foreach (var enemy in _enemies)
            {
                if (!enemy.IsActive) continue;

                float distSquared = Vector2.DistanceSquared(_position3D, enemy.Position);
                float hitRadius = Enemy.SpriteWidth * 0.75f; // Увеличен радиус
                if (distSquared <= hitRadius * hitRadius)
                {
                    enemy.Health -= 50;
                    enemy.HitTimer = 0.2f;
                   _isActive = false;
                    if (enemy.Health <= 0)
                    {
                        enemy.IsActive = false;
                    }
                    return;
                }
            }

            _scale = MathHelper.Lerp(1.0f, 0.1f, _distance / _maxDistance);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive) return;

            Vector2 screenPos = ProjectToScreen();

            if (screenPos.X < 0 || screenPos.X > _screenWidth || screenPos.Y < 0 || screenPos.Y > _screenHeight)
            {
                return;
            }

            int screenX = (int)screenPos.X;
            if (screenX >= 0 && screenX < _wallDistances.Length)
            {
                float wallDistance = _wallDistances[screenX];
                if (wallDistance > 0 && wallDistance < float.MaxValue && _distance > wallDistance)
                {
                    return;
                }
            }

            Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
            spriteBatch.Draw(_texture, screenPos, null, Color.White, 0f, origin, _scale, SpriteEffects.None, 0f);
            }

        private Vector2 ProjectToScreen()
        {
            double spriteX = _position3D.X - _posX;
            double spriteY = _position3D.Y - _posY;
            double spriteZ = _positionZ;

            double invDet = 1.0 / (_initialPlaneX * _initialDirY - _initialDirX * _initialPlaneY);
            double transformX = invDet * (_initialDirY * spriteX - _initialDirX * spriteY);
            double transformY = invDet * (-_initialPlaneY * spriteX + _initialPlaneX * spriteY);

            if (transformY <= 0.01)
            {
                return new Vector2(-100, -100);
            }

            int screenX = (int)((_screenWidth / 2) * (1 + transformX / transformY));
            float targetScreenY = (_screenHeight / 2) - (float)(spriteZ / transformY) * _screenHeight;
            float screenY = targetScreenY;

            return new Vector2(screenX, screenY);
        }
    }
}