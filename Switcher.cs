using Microsoft.Xna.Framework.Input;
using System;

namespace GameProject
{
    public class Switcher
    {
        private KeyboardState _prevKeyboardState;

        public void UpdateState(KeyboardState currentState)
        {
            _prevKeyboardState = currentState;
        }

        public int MenuSwitcher(KeyboardState currentState, int currentIndex, int itemsCount)
        {
            if (currentState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
            {
                currentIndex = (currentIndex + 1) % itemsCount;
            }
            else if (currentState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
            {
                currentIndex = (currentIndex - 1 + itemsCount) % itemsCount;
            }

            _prevKeyboardState = currentState;
            return currentIndex;
        }
    }
}