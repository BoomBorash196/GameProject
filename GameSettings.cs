using GameProject;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using static GameProject.Game1;

public class SettingsMenu
{
    private SpriteFont _font;
    private string[] _settingsItems = { "Настройки", "Звук", "Управление", "Назад" };
    private int _selectedIndex;
    private int[] _typingProgress;
    private float _typingSpeed = 0.1f;
    private float _typingTimer = 0f;
    private KeyboardState _prevKeyboardState;
    private Texture2D _backgroundTexture;
    private Switcher _menuSwitcher;
    private Game1 _game;

    public SettingsMenu(SpriteFont font, Texture2D backgroundTexture, Game1 game)
    {
        _backgroundTexture = backgroundTexture;
        _font = font;
        _game = game;
        _prevKeyboardState = Keyboard.GetState();
        _typingProgress = new int[_settingsItems.Length];
        _menuSwitcher = new Switcher();
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        _selectedIndex = _menuSwitcher.MenuSwitcher(keyboardState, _selectedIndex, _settingsItems.Length);
        _menuSwitcher.UpdateState(keyboardState);


        if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
        {
            switch (_selectedIndex)
            {
                case 1:
                    Game1.CurrentGameState = GameState.SoundSettings;
                    break;
                case 2:
                    Game1.CurrentGameState = GameState.ControlsSettings;
                    break;
                case 3:
                    Game1.CurrentGameState = GameState.MainMenu;
                    break;
            }
        }

        _typingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_typingTimer >= _typingSpeed)
        {
            _typingTimer = 0f;
            for (int i = 0; i < _settingsItems.Length; i++)
            {
                if (_typingProgress[i] < _settingsItems[i].Length)
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

        // Отрисовка фона
        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
        }
        else
        {
            graphicsDevice.Clear(Color.DarkBlue);
        }

        float menuHeight = _settingsItems.Length * 50;
        float startY = (graphicsDevice.Viewport.Height - menuHeight) / 2;

        // Отрисовка пунктов меню
        for (int i = 0; i < _settingsItems.Length; i++)
        {
            string menuItem = _settingsItems[i].Substring(0, _typingProgress[i]);
            Vector2 textSize = _font.MeasureString(menuItem);
            float x = (graphicsDevice.Viewport.Width - textSize.X) / 2;
            float y = startY + i * 50;
            Color color = (i == _selectedIndex) ? Color.Red : Color.White; 

            float scale = 1f; 

            // Отрисовка тени
            Vector2 shadowOffset = new Vector2(2, 2);
            spriteBatch.DrawString(_font, menuItem, new Vector2(x + shadowOffset.X, y + shadowOffset.Y), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // Отрисовка основного текста
            spriteBatch.DrawString(_font, menuItem, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
    }

    public void Reset()
    {
        _typingProgress = new int[_settingsItems.Length];
        _typingTimer = 0f;
        _selectedIndex = 0;
    }
}