using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameProject
{
    public class Shotgun
    {
        private List<Texture2D> _shotgunFrames;
        private Texture2D _bulletTexture;
        private List<Texture2D> _muzzleFlashTextures;
        private Vector2 _position;
        private Vector2 _muzzleOffset;
        private List<Bullet> _bullets;
        private SoundEffect _shotgunSound;
        private float _fireCooldown = 0.2f;
        private float _fireTimer = 0f;
        private float _animationTimer = 0f;
        private float _muzzleFlashTimer = 0f;
        private float _frameDuration = 0.1f;
        private float _muzzleFlashDuration = 0.025f;
        private int _currentFrame = 0;
        private int _currentMuzzleFlashFrame = 0;
        private int _muzzleFlashCycles = 0;
        private SoundEffectInstance _shotgunSoundInstance;
        private const int _maxMuzzleFlashCycles = 2;
        private bool _isFiring = false;
        private int _screenWidth;
        private int _screenHeight;
        private double _posX, _posY;
        private double _dirX, _dirY;
        private double _planeX, _planeY;
        private const int _pelletCount = 1;
        private float[] _wallDistances;
        private List<Enemy> _enemies;
        private int[,] _currentMap;
        private GraphicsDevice _graphicsDevice;

        public Shotgun(List<Texture2D> shotgunFrames, Texture2D bulletTexture, List<Texture2D> muzzleFlashTextures,
            SoundEffect shotgunSound, GraphicsDevice graphicsDevice,
            int screenWidth, int screenHeight, float[] wallDistances, List<Enemy> enemies, int[,] currentMap)
        {
            _shotgunFrames = shotgunFrames;
            _bulletTexture = bulletTexture;
            _muzzleFlashTextures = muzzleFlashTextures;
            _shotgunSound = shotgunSound;
            _bullets = new List<Bullet>();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _wallDistances = wallDistances;
            _enemies = enemies;
            _currentMap = currentMap;
            _graphicsDevice = graphicsDevice;

            _position = new Vector2(
                (graphicsDevice.Viewport.Width - _shotgunFrames[0].Width) / 2,
                graphicsDevice.Viewport.Height - _shotgunFrames[0].Height
            );

            _muzzleOffset = new Vector2(_shotgunFrames[0].Width / 2 + 5, 5);
        }

        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2 mousePosition,
                          double posX, double posY, double dirX, double dirY, double planeX, double planeY,
                          float[] wallDistances, List<Enemy> enemies, int[,] currentMap)
        {
            _wallDistances = wallDistances;
            _enemies = enemies;
            _currentMap = currentMap;

            _fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;
            _muzzleFlashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            _posX = posX;
            _posY = posY;
            _dirX = dirX;
            _dirY = dirY;
            _planeX = planeX;
            _planeY = planeY;

            if (_isFiring)
            {
                if (_animationTimer >= _frameDuration)
                {
                    _animationTimer -= _frameDuration;
                    _currentFrame++;
                    if (_currentFrame >= _shotgunFrames.Count)
                    {
                        _currentFrame = 0;
                        _isFiring = false;
                        _currentMuzzleFlashFrame = 0;
                        _muzzleFlashCycles = 0;
                    }
                }

                if (_currentFrame == 1 && _muzzleFlashTimer >= _muzzleFlashDuration && _muzzleFlashCycles < _maxMuzzleFlashCycles)
                {
                    _muzzleFlashTimer -= _muzzleFlashDuration;
                    _currentMuzzleFlashFrame++;
                    if (_currentMuzzleFlashFrame >= _muzzleFlashTextures.Count)
                    {
                        _currentMuzzleFlashFrame = 0;
                        _muzzleFlashCycles++;
                    }
                }
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && _fireTimer >= _fireCooldown && !_isFiring)
            {
                _fireTimer = 0f;
                _isFiring = true;
                _animationTimer = 0f;
                _muzzleFlashTimer = 0f;
                _currentFrame = 1;
                _currentMuzzleFlashFrame = 0;
                _muzzleFlashCycles = 0;
                _shotgunSoundInstance = _shotgunSound.CreateInstance();
                _shotgunSoundInstance.Volume = 0.05f;
                _shotgunSoundInstance.Play();

                Vector2 bulletStartPosition = new Vector2((float)_posX, (float)_posY);
                float bulletStartZ = -0.1f;

                for (int i = 0; i < _pelletCount; i++)
                {
                    Vector2 bulletDirectionXY = new Vector2((float)_dirX, (float)_dirY);
                    float bulletDirectionZ = 0f;

                    bulletDirectionXY.Normalize();

                    Bullet bullet = new Bullet(_bulletTexture, startPosition3D: bulletStartPosition, startPositionZ: bulletStartZ, directionXY: bulletDirectionXY, directionZ: bulletDirectionZ,
                                              _screenWidth, _screenHeight, _posX, _posY, _dirX, _dirY, _planeX, _planeY, _wallDistances, _enemies, _currentMap, _graphicsDevice);
                    _bullets.Add(bullet);
                }
            }

            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                _bullets[i].Update(gameTime, _posX, _posY);
                if (!_bullets[i].IsActive)
                {
                    _bullets.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentFrame = _shotgunFrames[_currentFrame];
            Vector2 drawPosition = new Vector2(
                (_screenWidth - currentFrame.Width) / 2,
                _screenHeight - currentFrame.Height
            );

            spriteBatch.Draw(currentFrame, drawPosition, Color.White);

            if (_isFiring && _currentFrame == 1 && _muzzleFlashCycles < _maxMuzzleFlashCycles)
            {
                Vector2 muzzlePosition = drawPosition + _muzzleOffset;
                spriteBatch.Draw(_muzzleFlashTextures[_currentMuzzleFlashFrame], muzzlePosition, null, Color.White, 0f, new Vector2(_muzzleFlashTextures[_currentMuzzleFlashFrame].Width / 2, _muzzleFlashTextures[_currentMuzzleFlashFrame].Height / 2), 1f, SpriteEffects.None, 0f);
            }

            foreach (var bullet in _bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }
    }
}