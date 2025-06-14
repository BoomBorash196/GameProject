using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameProject
{
    public class Renderer
    {
        private SpriteBatch _spriteBatch;
        private Texture2D pixel;
        private List<Game1.TextureData> wallTextures;
        private int screenWidth;
        private int screenHeight;
        private double posX, posY, dirX, dirY, planeX, planeY;
        private float globalLight = 0.8f;
        private float depthDarkness = 1.0f;

        private Texture2D screenBuffer;
        private Color[] colorBuffer;
        private float[] _wallDistances;
        private bool _enableDebugLogging = false; // Флаг для логирования

        private int[,] _currentMap;

        public Renderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Game1.TextureData> wallTextures,
                        int screenWidth, int screenHeight, double posX, double posY,
                        double dirX, double dirY, double planeX, double planeY)
        {
            _spriteBatch = spriteBatch;
            this.wallTextures = wallTextures;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.posX = posX;
            this.posY = posY;
            this.dirX = dirX;
            this.dirY = dirY;
            this.planeX = planeX;
            this.planeY = planeY;

            InitScreenBuffer(graphicsDevice);

            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _wallDistances = new float[screenWidth];
            for (int i = 0; i < screenWidth; i++)
            {
                _wallDistances[i] = float.MaxValue;
            }
        }

        private void InitScreenBuffer(GraphicsDevice graphicsDevice)
        {
            screenBuffer = new Texture2D(graphicsDevice, screenWidth, screenHeight);
            colorBuffer = new Color[screenWidth * screenHeight];

            for (int i = 0; i < colorBuffer.Length; i++)
            {
                colorBuffer[i] = Color.White;
            }
        }

        public void DrawCeilingAndFloor()
        {
            for (int y = 0; y < screenHeight; y += 2) // Пропускаем каждый второй пиксель
            {
                int p = y - screenHeight / 2;
                bool isCeiling = p < 0;

                if (p == 0) p = 1;

                double rowDistance = Math.Abs((0.5 * screenHeight) / p);

                double floorStepX = rowDistance * (dirX + planeX - (dirX - planeX)) / screenWidth;
                double floorStepY = rowDistance * (dirY + planeY - (dirY - planeY)) / screenWidth;

                double floorX = posX + rowDistance * (dirX - planeX);
                double floorY = posY + rowDistance * (dirY - planeY);

                for (int x = 0; x < screenWidth; x += 2)
                {
                    int texNum = isCeiling ? 2 : 3; // номер тектур пола и потолка + 1 к каждому
                    if (texNum < 0 || texNum >= wallTextures.Count)
                        texNum = 0;

                    var tex = wallTextures[texNum];

                    if (tex == null || tex.Data == null || tex.Texture == null)
                        continue;

                    int texWidth = tex.Texture.Width;
                    int texHeight = tex.Texture.Height;

                    // Считаем координаты текстуры
                    int texX = (int)(texWidth * (floorX - Math.Floor(floorX))) % texWidth;
                    int texY = (int)(texHeight * (floorY - Math.Floor(floorY))) % texHeight;

                    if (texX < 0) texX = 0;
                    if (texY < 0) texY = 0;
                    if (texX >= texWidth) texX = texWidth - 1;
                    if (texY >= texHeight) texY = texHeight - 1;

                    float light = globalLight * (1.0f - (float)(depthDarkness * rowDistance / 10.0));
                    light = MathHelper.Clamp(light, 0.2f, 1.0f);

                    int index = texX + texY * texWidth;
                    if (index >= 0 && index < tex.Data.Length)
                    {
                        Color color = tex.Data[index];
                        //if (color.A == 0) continue;
                        color *= light;
                        //color.A = 255;
                        for (int dy = 0; dy < 2 && y + dy < screenHeight; dy++)
                        {
                            for (int dx = 0; dx < 2 && x + dx < screenWidth; dx++)
                            {
                                int bufferIndex = (x + dx) + (y + dy) * screenWidth;
                                if (bufferIndex >= 0 && bufferIndex < colorBuffer.Length)
                                {
                                    colorBuffer[bufferIndex] = color;
                                }
                            }
                        }
                    }

                    floorX += floorStepX * 2;
                    floorY += floorStepY * 2;
                }
            }
        }

        public void DrawWalls(int[,] map)
        {
            for (int x = 0; x < screenWidth; x++)
            {
                _wallDistances[x] = float.MaxValue;

                double cameraX = 2 * x / (double)screenWidth - 1;
                double rayDirX = dirX + planeX * cameraX;
                double rayDirY = dirY + planeY * cameraX;

                int mapX = (int)posX;
                int mapY = (int)posY;

                // Считаем расстояние до следующих клеток на карте 
                double sideDistX, sideDistY;
                double deltaDistX = Math.Abs(rayDirX) < 0.00001 ? 1e30 : Math.Abs(1 / rayDirX); // расстояние до клетки X
                double deltaDistY = Math.Abs(rayDirY) < 0.00001 ? 1e30 : Math.Abs(1 / rayDirY); // расстояние до клетки Y
                double perpWallDist;

                int stepX, stepY;
                bool hit = false;
                int side = 0;
                // DDA
                // Определяем расстояние до граници Х и У
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1 - posX) * deltaDistX;
                }

                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1 - posY) * deltaDistY;
                }

                int maxIterations = 100;
                int iterations = 0;
                
                while (!hit && iterations < maxIterations)
                {
                    iterations++;
                    // проверяем куда луч придет раньше
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    // луч сталкнулся со стеный
                    if (map[mapX, mapY] > 0)
                        hit = true;
                }


                if (side == 0)
                    perpWallDist = (sideDistX - deltaDistX);
                else
                    perpWallDist = (sideDistY - deltaDistY);

                _wallDistances[x] = (float)perpWallDist;

                // Считаем высоту стены на экране
                int lineHeight = (int)(screenHeight / perpWallDist);

                if (lineHeight < 0) lineHeight = 0;
                if (lineHeight > screenHeight * 5) lineHeight = screenHeight * 5;

                // Определяем где начинаем и заканчиваем рисовать
                int drawStart = -lineHeight / 2 + screenHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + screenHeight / 2;
                if (drawEnd >= screenHeight) drawEnd = screenHeight - 1;

                int texNum = map[mapX, mapY] - 1;
                if (texNum < 0 || texNum >= wallTextures.Count)
                    texNum = 0;

                // Выбираем тектуру по номеру
                var tex = wallTextures[texNum];
                if (tex == null || tex.Data == null || tex.Texture == null)
                    continue;

                double wallX = side == 0
                    ? posY + perpWallDist * rayDirY
                    : posX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                int texWidth = tex.Texture.Width;
                int texHeight = tex.Texture.Height;

                int texX = (int)(wallX * (double)texWidth);
                // Считаем какую честь тектуры ирсовать чтобы стены выглядела ровно
                if (side == 0 && rayDirX > 0) texX = texWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = texWidth - texX - 1;

                if (texX < 0) texX = 0;
                if (texX >= texWidth) texX = texWidth - 1;

                double step = 1.0 * texHeight / lineHeight;
                double texPos = (drawStart - screenHeight / 2 + lineHeight / 2) * step;

                // Рисуем тектуру писель за пикселем
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int texY = (int)texPos & (texHeight - 1);
                    texPos += step;

                    if (texY < 0) texY = 0;
                    if (texY >= texHeight) texY = texHeight - 1;

                    int texIndex = texY * texWidth + texX;
                    if (texIndex < 0 || texIndex >= tex.Data.Length)
                        continue;

                    Color color = tex.Data[texIndex];
                    //if (color.B == 0) continue;

                    float wallLight = globalLight * (1.0f - (float)(depthDarkness * perpWallDist / 8.0));
                    wallLight = MathHelper.Clamp(wallLight, 0.3f, 1.0f);

                    color *= wallLight;
                    //color.A = 255;
                    int bufferIndex = x + y * screenWidth;
                    if (bufferIndex >= 0 && bufferIndex < colorBuffer.Length)
                    {
                        colorBuffer[bufferIndex] = color;
                    }
                }
            }
        }

        public void RenderBufferToScreen()
        {
            screenBuffer.SetData(colorBuffer);

            _spriteBatch.Begin();
            _spriteBatch.Draw(screenBuffer, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            _spriteBatch.End();
        }

        public void UpdateData(double posX, double posY, double dirX, double dirY, double planeX, double planeY)
        {
            this.posX = posX;
            this.posY = posY;
            this.dirX = dirX;
            this.dirY = dirY;
            this.planeX = planeX;
            this.planeY = planeY;
        }

        public float[] GetWallDistances()
        {
            return _wallDistances;
        }

        public void RenderEnemies(SpriteBatch spriteBatch, List<Enemy> enemies, Vector2 playerPos, float playerRot)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsActive)
                {
                    if (_enableDebugLogging)
                        Debug.WriteLine($"Enemy at ({enemy.Position.X}, {enemy.Position.Y}) is inactive");
                    continue;
                }

                double spriteX = enemy.Position.X - playerPos.X;
                double spriteY = enemy.Position.Y - playerPos.Y;
                double spriteZ = enemy.Z;

                double invDet = 1.0 / (planeX * dirY - dirX * planeY);
                double transformX = invDet * (dirY * spriteX - dirX * spriteY);
                double transformY = invDet * (-planeY * spriteX + planeX * spriteY);

                Texture2D currentTexture = enemy.AnimationFrames[enemy.CurrentFrame];

                if (transformY <= 0.01)
                {
                    if (_enableDebugLogging)
                        Debug.WriteLine($"Enemy at ({enemy.Position.X}, {enemy.Position.Y}) behind camera: transformY={transformY}");
                    continue;
                }

                float angle = (float)Math.Atan2(spriteY, spriteX) - playerRot;
                angle = MathHelper.WrapAngle(angle);
                if (Math.Abs(angle) > MathHelper.Pi)
                {
                    if (_enableDebugLogging)
                        Debug.WriteLine($"Enemy at ({enemy.Position.X}, {enemy.Position.Y}) out of FOV: angle={angle}");
                    continue;
                }

                int screenX = (int)((screenWidth / 2) * (1 + transformX / transformY));
                float spriteScreenHeight = Math.Abs((float)(screenHeight / transformY)) * Enemy.SpriteHeight;
                float spriteScreenWidth = spriteScreenHeight * (currentTexture.Width / (float)currentTexture.Height);

                float screenY = (screenHeight / 2) - (float)(spriteZ / transformY) * screenHeight - spriteScreenHeight / 2;

                screenX = MathHelper.Clamp(screenX, (int)-spriteScreenWidth, screenWidth);
                screenY = MathHelper.Clamp(screenY, (int)-spriteScreenHeight, screenHeight);

                int xCenter = (int)(screenX + spriteScreenWidth / 2);
                if (xCenter >= 0 && xCenter < screenWidth)
                {
                    if (transformY > _wallDistances[xCenter])
                    {
                        if (_enableDebugLogging)
                            Debug.WriteLine($"Enemy at ({enemy.Position.X}, {enemy.Position.Y}) behind wall: transformY={transformY}, wallDistance={_wallDistances[xCenter]}");
                        continue;
                    }
                }

                Color color = enemy.HitTimer > 0 ? Color.Red : Color.White;
                enemy.HitTimer -= 0.016f;
                Texture2D currentFrameTexture = enemy.AnimationFrames[enemy.CurrentFrame];

                Rectangle sourceRect = new Rectangle(0, 0, currentFrameTexture.Width, currentFrameTexture.Height);
                spriteBatch.Draw(
                    currentTexture,
                    new Rectangle((int)screenX, (int)screenY, (int)spriteScreenWidth, (int)spriteScreenHeight),
                    sourceRect,
                    color,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
                if (_enableDebugLogging)
                    Debug.WriteLine($"Enemy drawn at ({enemy.Position.X}, {enemy.Position.Y}): screenX={screenX}, screenY={screenY}, distance={transformY}");
            }
        }

        public void UpdateEnemies(List<Enemy> enemies, Vector2 playerPos, GameTime gameTime)
        {
            foreach (var enemy in enemies.ToList())
            {
                if (!enemy.IsActive || enemy == null) continue;

                // Проверка наличия кадров анимации
                if (enemy.AnimationFrames == null || enemy.AnimationFrames.Length == 0)
                {
                    Debug.WriteLine($"Enemy at ({enemy.Position}) has no animation frames!");
                    enemies.Remove(enemy); // Удаляем  врага
                    continue;
                }

                enemy.FrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (enemy.FrameTimer >= enemy.FrameInterval)
                {
                    enemy.FrameTimer = 0;
                    enemy.CurrentFrame = (enemy.CurrentFrame + 1) % enemy.AnimationFrames.Length;
                }

                Vector2 direction = playerPos - enemy.Position;
                if (direction.LengthSquared() > 0.1f)
                {
                    direction.Normalize();
                    Vector2 newPos = enemy.Position + direction * enemy.Speed;

                    // Проверка коллизий с картой
                    if (IsValidPosition(newPos))
                    {
                        enemy.Position = newPos;
                    }
                }
            }
        }

        private bool IsValidPosition(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            return x >= 0 && y >= 0 && x < _currentMap.GetLength(0) && y < _currentMap.GetLength(1) && _currentMap[x, y] == 0;
        }

        public void SetCurrentMap(int[,] map)
        {
            _currentMap = map;
        }

        public void UpdateWallTextures(List<Game1.TextureData> newTextures)
        {
            wallTextures = newTextures;
        }

        public void RenderScene(SpriteBatch spriteBatch, int[,] map, List<Enemy> enemies, Vector2 playerPos, float playerRot)
        {
            Array.Fill(colorBuffer, Color.Black);

            DrawCeilingAndFloor();

            DrawWalls(map);

            RenderBufferToScreen();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            RenderEnemies(spriteBatch, enemies, playerPos, playerRot);
            spriteBatch.End();
        }
    }
}