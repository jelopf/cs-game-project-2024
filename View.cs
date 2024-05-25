using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace gameproject
{
    // Views
    public class GameView
    {
        private SpriteBatch _spriteBatch;
        private GameModel _model;
        private Texture2D _noteTextureUp;
        private Texture2D _noteTextureDown;
        private Texture2D _attentionMeterTexture;
        private Texture2D _playerIdleTexture;
        private Texture2D _playerUpTexture;
        private Texture2D _playerDownTexture;
        private Texture2D _enemyTexture;
        private Texture2D _collectionPointUp;
        private Texture2D _collectionPointDown;
        private SpriteFont _font;
        private Texture2D _backgroundTexture;
        private Texture2D _treesTexture;
        private Texture2D _grassTexture;
        private float _grassOffset;
        private float _treesOffset;

        public GameView(SpriteBatch spriteBatch, GameModel model, Texture2D noteTextureUp, Texture2D noteTextureDown, Texture2D attentionMeterTexture, Texture2D playerIdleTexture, Texture2D playerUpTexture, Texture2D playerDownTexture, Texture2D enemyTexture, Texture2D collectionPointUp, Texture2D collectionPointDown, SpriteFont font, Texture2D backgroundTexture, Texture2D treesTexture, Texture2D grassTexture)
        {
            _spriteBatch = spriteBatch;
            _model = model;
            _noteTextureUp = noteTextureUp ?? throw new ArgumentNullException(nameof(noteTextureUp));
            _noteTextureDown = noteTextureDown ?? throw new ArgumentNullException(nameof(noteTextureDown));
            _attentionMeterTexture = attentionMeterTexture ?? throw new ArgumentNullException(nameof(attentionMeterTexture));
            _playerIdleTexture = playerIdleTexture ?? throw new ArgumentNullException(nameof(playerIdleTexture));
            _playerUpTexture = playerUpTexture ?? throw new ArgumentNullException(nameof(playerUpTexture));
            _playerDownTexture = playerDownTexture ?? throw new ArgumentNullException(nameof(playerDownTexture));
            _enemyTexture = enemyTexture ?? throw new ArgumentNullException(nameof(enemyTexture));
            _collectionPointUp = collectionPointUp ?? throw new ArgumentNullException(nameof(collectionPointUp));
            _collectionPointDown = collectionPointDown ?? throw new ArgumentNullException(nameof(collectionPointDown));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _backgroundTexture = backgroundTexture ?? throw new ArgumentNullException(nameof(backgroundTexture));
            _treesTexture = treesTexture ?? throw new ArgumentNullException(nameof(treesTexture));
            _grassTexture = grassTexture ?? throw new ArgumentNullException(nameof(grassTexture));
            
        }

        public void Update(GameTime gameTime)
        {
            float grassSpeed = 200f; // Скорость движения травы
            float treesSpeed = 100f; // Скорость движения деревьев

            _grassOffset += grassSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _treesOffset += treesSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_grassOffset >= _grassTexture.Width)
                _grassOffset -= _grassTexture.Width;

            if (_treesOffset >= _treesTexture.Width)
                _treesOffset -= _treesTexture.Width;
        }


        public void Draw()
        {
            // Begin drawing sprites
            _spriteBatch.Begin();

            // Draw background
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height), Color.White);
            DrawBackground();

            // Draw collection points
            _spriteBatch.Draw(_collectionPointUp, _model.CollectionPoint1.Position, Color.White);
            _spriteBatch.Draw(_collectionPointDown, _model.CollectionPoint2.Position, Color.White);

            // Draw notes on the first track
            foreach (var note in _model.Track1.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTextureUp, note.Position, Color.White);
                }
            }

            // Draw notes on the second track
            foreach (var note in _model.Track2.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTextureDown, note.Position, Color.White);
                }
            }

            // Draw attention meter
            _spriteBatch.Draw(_attentionMeterTexture, new Rectangle(10, 10, (int)(_model.AttentionMeter * 200), 20), Color.Green);

            // Choose player texture based on input
            Texture2D playerTexture = _playerIdleTexture;
            if (_model.InputState.IsKeyDown(Keys.W) || _model.InputState.IsKeyDown(Keys.Up))
            {
                playerTexture = _playerUpTexture;
            }
            else if (_model.InputState.IsKeyDown(Keys.S) || _model.InputState.IsKeyDown(Keys.Down))
            {
                playerTexture = _playerDownTexture;
            }

            // Draw player and enemy
            _spriteBatch.Draw(playerTexture, new Vector2(100, 200), Color.White);
            _spriteBatch.Draw(_enemyTexture, new Vector2(600, 75), Color.White);

            // Draw score and other statistics
            _spriteBatch.DrawString(_font, $"Score: {_model.Score}", new Vector2(10, 40), Color.White);
            _spriteBatch.DrawString(_font, $"Super: {_model.SuperCount}", new Vector2(10, 60), Color.White);
            _spriteBatch.DrawString(_font, $"Good: {_model.GoodCount}", new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_font, $"Ok: {_model.OkCount}", new Vector2(10, 100), Color.White);
            _spriteBatch.DrawString(_font, $"Bad: {_model.BadCount}", new Vector2(10, 120), Color.White);
            _spriteBatch.DrawString(_font, $"Miss: {_model.MissCount}", new Vector2(10, 140), Color.White);

            // End drawing sprites
            _spriteBatch.End();
        }

        private void DrawBackground()
        {
            DrawLayer(_treesTexture, _treesOffset);
            DrawLayer(_grassTexture, _grassOffset);
        }

        private void DrawLayer(Texture2D texture, float offset)
        {
            int textureWidth = texture.Width;
            int screenWidth = _spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = _spriteBatch.GraphicsDevice.Viewport.Height;

            for (float x = -offset; x < screenWidth; x += textureWidth)
            {
                _spriteBatch.Draw(texture, new Rectangle((int)x, 0, textureWidth, screenHeight), Color.White);
            }
        }
    }
}