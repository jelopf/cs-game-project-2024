using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace gameproject
{
    public class VolumeSettingsMenu
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;
        private float _menuVolume;
        private float _gameVolume;
        private bool _isActive;

        public VolumeSettingsMenu(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font, float menuVolume, float gameVolume)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
            _menuVolume = menuVolume;
            _gameVolume = gameVolume;
            _isActive = true; // Set the menu active by default
        }

        public void Update(InputState input, RhythmGame game)
        {
            if (!_isActive)
                return;

            if (input.IsKeyPressed(Keys.Up))
            {
                _menuVolume = MathHelper.Clamp(_menuVolume + 0.1f, 0.0f, 1.0f);
                MediaPlayer.Volume = _menuVolume;
            }

            if (input.IsKeyPressed(Keys.Down))
            {
                _menuVolume = MathHelper.Clamp(_menuVolume - 0.1f, 0.0f, 1.0f);
                MediaPlayer.Volume = _menuVolume;
            }

            if (input.IsKeyPressed(Keys.Right))
            {
                _gameVolume = MathHelper.Clamp(_gameVolume + 0.1f, 0.0f, 1.0f);
            }

            if (input.IsKeyPressed(Keys.Left))
            {
                _gameVolume = MathHelper.Clamp(_gameVolume - 0.1f, 0.0f, 1.0f);
            }

            if (input.IsKeyPressed(Keys.Back))
            {
                game.SaveVolumeSettings(_menuVolume, _gameVolume);
                GoToMainMenu(game);
            }
        }

        public void Draw()
        {
            if (!_isActive)
                return;

            _spriteBatch.Begin();
            Vector2 titlePosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString("Volume Settings").X / 2, 150);
            _spriteBatch.DrawString(_font, "Volume Settings", titlePosition, Color.White);

            string menuVolumeText = $"Menu Volume: {(Math.Round(_menuVolume * 100f)).ToString()}%";
            string gameVolumeText = $"Game Volume: {(Math.Round(_gameVolume * 100f)).ToString()}%";
            string instructions = "Use UP/DOWN to adjust Menu Volume\nUse LEFT/RIGHT to adjust Game Volume\nPress BACKSPACE to go back";

            Vector2 menuVolumePosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString(menuVolumeText).X / 2, 200);
            Vector2 gameVolumePosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString(gameVolumeText).X / 2, 250);
            Vector2 instructionsPosition = new Vector2(_graphicsDevice.Viewport.Width / 2 - _font.MeasureString(instructions).X / 2, 300);

            _spriteBatch.DrawString(_font, menuVolumeText, menuVolumePosition, Color.White);
            _spriteBatch.DrawString(_font, gameVolumeText, gameVolumePosition, Color.White);
            _spriteBatch.DrawString(_font, instructions, instructionsPosition, Color.White);

            _spriteBatch.End();
        }

        public void GoToMainMenu(RhythmGame game)
        {
            _isActive = false; // Deactivate the volume settings menu
            game.GoToMainMenu();
        }

        public void Reset()
        {
            _isActive = true; // Activate the volume settings menu
        }
    }
}
