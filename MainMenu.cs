using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameproject
{
    public class MainMenu
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private string[] _menuItems = { "Start Game", "Adjust Volume", "Exit" };
        private int _selectedIndex = 0;
        private GraphicsDevice _graphicsDevice;

        public MainMenu(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public void Update(InputState input, RhythmGame game)
        {
            if (input.IsKeyPressed(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0) _selectedIndex = _menuItems.Length - 1;
            }

            if (input.IsKeyPressed(Keys.Down))
            {
                _selectedIndex++;
                if (_selectedIndex >= _menuItems.Length) _selectedIndex = 0;
            }

            if (input.IsKeyPressed(Keys.Enter))
            {
                switch (_selectedIndex)
                {
                    case 0:
                        game.StartGame();
                        break;
                    case 1:
                        game.AdjustVolume();
                        break;
                    case 2:
                        game.Exit();
                        break;
                }
            }
        }

        public void Draw()
        {
            _spriteBatch.Begin();
            Vector2 titlePosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString("Main Menu").X / 2, 150);
            _spriteBatch.DrawString(_font, "Main Menu", titlePosition, Color.White);

            for (int i = 0; i < _menuItems.Length; i++)
            {
                Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                Vector2 position = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString(_menuItems[i]).X / 2, 200 + i * 30);
                _spriteBatch.DrawString(_font, _menuItems[i], position, color);
            }

            _spriteBatch.End();
        }
    }
}
