using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gameproject
{
    // Controllers
    public class GameController
    {
        private GameModel _model;
        private GameView _view;

        public GameController(GameModel model, GameView view)
        {
            _model = model;
            _view = view;
        }

        public void Update(GameTime gameTime, InputState input)
        {
            _model.Update(gameTime, input);
        }

        public void Draw()
        {
            _view.Draw();
        }
    }

    public class InputState
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        public InputState()
        {
            _currentKeyboardState = Keyboard.GetState();
        }

        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }
    }
}
