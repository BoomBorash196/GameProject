using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameProject
{
    public class Enemy
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public float Speed { get; set; }
        public int Health { get; set; }
        public bool IsActive { get; set; } = true;
        public float Z { get; set; }
        public Vector2 Size { get; set; }

        public float HitTimer { get; set; } 
        public Rectangle[] Frames { get; set; }
        public int CurrentFrame { get; set; }
        public float FrameTimer { get; set; }

        public float CollisionRadius { get; set; }
        public float FrameInterval { get; set; } = 0.15f;
        public Texture2D[] AnimationFrames { get; set; }
        public float FrameTime { get; set; }

        public const float SpriteWidth = 0.5f;
        public const float SpriteHeight = 0.5f;

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            // Обновление анимации
            FrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (FrameTimer >= FrameTime)
            {
                FrameTimer = 0f;
                CurrentFrame = (CurrentFrame + 1) % AnimationFrames.Length;
            }

            // Движение к игроку
            Vector2 direction = playerPosition - Position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Position += direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (AnimationFrames != null && AnimationFrames.Length > 0)
            {
                Texture2D currentTexture = AnimationFrames[CurrentFrame];
                spriteBatch.Draw(
                    currentTexture,
                    new Rectangle(
                        (int)Position.X,
                        (int)Position.Y,
                        (int)Size.X,
                        (int)Size.Y),
                    Color.White);
            }
        }
    }
}