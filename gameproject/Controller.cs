using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gameproject
{
    // Controller
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
            // Update the game model with the current input state
            _model.Update(gameTime, input);
        }

        public void Draw()
        {
            // Draw the game view
            _view.Draw();
        }
    }

    // Input State
    public class InputState
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyState.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
        }
    }
}
