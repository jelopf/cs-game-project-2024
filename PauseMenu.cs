using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameproject
{
    public class PauseMenu
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private string[] _menuItems = { "Resume", "Game Volume", "Main Menu" };
        private int _selectedIndex;
        private float _gameVolume;
        private const float VolumeChangeStep = 0.1f;

        public PauseMenu(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font, float gameVolume)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
            _selectedIndex = 0;
            _gameVolume = gameVolume;
        }

        public void Update(InputState input, RhythmGame game)
        {
            if (input.IsKeyPressed(Keys.Up))
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                    _selectedIndex = _menuItems.Length - 1;
            }

            if (input.IsKeyPressed(Keys.Down))
            {
                _selectedIndex++;
                if (_selectedIndex >= _menuItems.Length)
                    _selectedIndex = 0;
            }

            if (_selectedIndex == 1) // Game Volume
            {
                if (input.IsKeyPressed(Keys.Left))
                {
                    _gameVolume = MathHelper.Clamp(_gameVolume - VolumeChangeStep, 0.0f, 1.0f);
                    game.SetGameVolume(_gameVolume);
                }
                else if (input.IsKeyPressed(Keys.Right))
                {
                    _gameVolume = MathHelper.Clamp(_gameVolume + VolumeChangeStep, 0.0f, 1.0f);
                    game.SetGameVolume(_gameVolume);
                }
            }

            if (input.IsKeyPressed(Keys.Enter))
            {
                SelectMenuItem(game);
            }
        }

        private void SelectMenuItem(RhythmGame game)
        {
            switch (_selectedIndex)
            {
                case 0: // Resume
                    game.ResumeGame();
                    break;
                case 1: // Game Volume
                    // No action needed on selection, handled in Update
                    break;
                case 2: // Main Menu
                    game.ReturnToMainMenuFromPause();
                    break;
                default:
                    break;
            }
        }

        public void Draw()
        {
            _spriteBatch.Begin();

            Vector2 titlePosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString("Pause Menu").X / 2, 150);
            _spriteBatch.DrawString(_font, "Pause Menu", titlePosition, Color.White);

            // Calculate total height of menu items
            float totalHeight = _menuItems.Length * _font.LineSpacing;
            Vector2 startPosition = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2 - totalHeight / 2);

            for (int i = 0; i < _menuItems.Length; i++)
            {
                Color itemColor = (_selectedIndex == i) ? Color.Yellow : Color.White;
                Vector2 itemPosition = startPosition + new Vector2(-_font.MeasureString(_menuItems[i]).X / 2, i * _font.LineSpacing);

                if (i == 1) // Game Volume
                {
                    string volumeText = $"Game Volume: {_gameVolume * 100:0}%";
                    Vector2 volumePosition = itemPosition + new Vector2(-_font.MeasureString(volumeText).X / 6, 4 -_font.LineSpacing * 0.2f); // Adjusted spacing
                    _spriteBatch.DrawString(_font, volumeText, volumePosition, itemColor);
                }
                else
                {
                    _spriteBatch.DrawString(_font, _menuItems[i], itemPosition, itemColor);
                }
            }

            _spriteBatch.End();
        }

    }
}
