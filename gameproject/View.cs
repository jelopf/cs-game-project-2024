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
        private Texture2D _noteTexture;
        private Texture2D _attentionMeterTexture;
        private Texture2D _playerIdleTexture;
        private Texture2D _playerUpTexture;
        private Texture2D _playerDownTexture;
        private Texture2D _enemyTexture;
        private Texture2D _collectionPoint;
        private SpriteFont _font;

        public GameView(SpriteBatch spriteBatch, GameModel model, Texture2D noteTexture, Texture2D attentionMeterTexture, Texture2D playerIdleTexture, Texture2D playerUpTexture, Texture2D playerDownTexture, Texture2D enemyTexture, Texture2D collectionPoint, SpriteFont font)
        {
            _spriteBatch = spriteBatch;
            _model = model;
            _noteTexture = noteTexture ?? throw new ArgumentNullException(nameof(noteTexture));
            _attentionMeterTexture = attentionMeterTexture ?? throw new ArgumentNullException(nameof(attentionMeterTexture));
            _playerIdleTexture = playerIdleTexture ?? throw new ArgumentNullException(nameof(playerIdleTexture));
            _playerUpTexture = playerUpTexture ?? throw new ArgumentNullException(nameof(playerUpTexture));
            _playerDownTexture = playerDownTexture ?? throw new ArgumentNullException(nameof(playerDownTexture));
            _enemyTexture = enemyTexture ?? throw new ArgumentNullException(nameof(enemyTexture));
            _collectionPoint = collectionPoint ?? throw new ArgumentNullException(nameof(collectionPoint));
            _font = font ?? throw new ArgumentNullException(nameof(font));
        }

        public void Draw()
        {
            // Begin drawing sprites
            _spriteBatch.Begin();

            // Draw notes on the first track
            foreach (var note in _model.Track1.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }

            // Draw notes on the second track
            foreach (var note in _model.Track2.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }

            // Draw collection point
            _spriteBatch.Draw(_collectionPoint, new Vector2(150, 300), Color.Red);

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
            _spriteBatch.Draw(playerTexture, new Vector2(100, 150), Color.White);
            _spriteBatch.Draw(_enemyTexture, new Vector2(500, 100), Color.White);

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
    }

}
