using GameProject;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using static GameProject.Game1;
using System.Diagnostics;

public class ControlsMenu
{
    private SpriteFont _font;
    private string[] _controlsItems = { "Вперед: W", "Назад: S", "Влево: A", "Вправо: D", "Атака: ЛКМ", "Назад" };
    private int _selectedIndex = 0;
    private int[] _typingProgress;
    private float _typingSpeed = 0.1f;
    private float _typingTimer = 0f;
    private KeyboardState _prevKeyboardState;
    private Texture2D _backgroundTexture;
    private Switcher _menuSwitcher;

    public ControlsMenu(SpriteFont font, Texture2D backgroundTexture)
    {
        _font = font;
        _backgroundTexture = backgroundTexture;
        _prevKeyboardState = Keyboard.GetState();
        _typingProgress = new int[_controlsItems.Length];
        _menuSwitcher = new Switcher();
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        _selectedIndex = _menuSwitcher.MenuSwitcher(keyboardState, _selectedIndex, _controlsItems.Length);
        _menuSwitcher.UpdateState(keyboardState);
        

        if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
        {
            switch (_selectedIndex)
            {
                case 5:
                        Game1.CurrentGameState = GameState.Settings;
                    break;
            }
        }

        _typingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_typingTimer >= _typingSpeed)
        {
            _typingTimer = 0f;
            for (int i = 0; i < _controlsItems.Length; i++)
            {
                if (_typingProgress[i] < _controlsItems[i].Length)
                {
                    _typingProgress[i]++;
                }
            }
        }

        _prevKeyboardState = keyboardState;
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.Begin();

        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
        }
        else
        {
            graphicsDevice.Clear(Color.DarkBlue);
        }

        float menuHeight = _controlsItems.Length * 50;
        float startY = (graphicsDevice.Viewport.Height - menuHeight) / 2;

        for (int i = 0; i < _controlsItems.Length; i++)
        {
            string menuItem = _controlsItems[i].Substring(0, _typingProgress[i]);
            Vector2 textSize = _font.MeasureString(menuItem);
            float x = (graphicsDevice.Viewport.Width - textSize.X) / 2;
            float y = startY + i * 50;
            Color color = (i == _selectedIndex) ? Color.Red : Color.White;

            float scale = 1f;

            Vector2 shadowOffset = new Vector2(2, 2);
            spriteBatch.DrawString(_font, menuItem, new Vector2(x + shadowOffset.X, y + shadowOffset.Y), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, menuItem, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
    }

    public void Reset()
    {
        _typingProgress = new int[_controlsItems.Length];
        _typingTimer = 0f;
        _selectedIndex = 0;
    }
}