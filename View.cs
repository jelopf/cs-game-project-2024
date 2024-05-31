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
        private Texture2D _collectionPointUp;
        private Texture2D _collectionPointDown;
        private SpriteFont _font;
        private Texture2D _backgroundTexture;
        private Texture2D _treesTexture;
        private Texture2D _grassTexture;
        private Texture2D _enemyNeutralTexture;
        private Texture2D _enemyHesitateTexture;
        private Texture2D _enemyAngryTexture;
        private Texture2D _playerDeadTexture;
        private Texture2D _playerEndTexture;
        private float _grassOffset;
        private float _treesOffset;

        private double _timeSinceLastBlink;
        private bool _isPlayerDeadVisible;
        private float _enemyAngle;
        private float _enemySpeed;
        private Vector2 _enemyPosition;
        private float _enemyRadius;
        private bool _enemyMovingForward;

        public GameView(SpriteBatch spriteBatch,
                        GameModel model,
                        Texture2D noteTextureUp,
                        Texture2D noteTextureDown,
                        Texture2D attentionMeterTexture,
                        Texture2D playerIdleTexture,
                        Texture2D playerUpTexture,
                        Texture2D playerDownTexture,
                        Texture2D enemyNeutralTexture,
                        Texture2D enemyHesitateTexture,
                        Texture2D enemyAngryTexture,
                        Texture2D playerDeadTexture,
                        Texture2D playerEndTexture,
                        Texture2D collectionPointUp,
                        Texture2D collectionPointDown,
                        SpriteFont font,
                        Texture2D backgroundTexture,
                        Texture2D treesTexture,
                        Texture2D grassTexture)
        {
            _spriteBatch = spriteBatch;
            _model = model;
            _noteTextureDown = noteTextureDown ?? throw new ArgumentNullException(nameof(noteTextureDown));
            _noteTextureUp = noteTextureUp ?? throw new ArgumentNullException(nameof(noteTextureUp));
            _attentionMeterTexture = attentionMeterTexture ?? throw new ArgumentNullException(nameof(attentionMeterTexture));
            _playerIdleTexture = playerIdleTexture ?? throw new ArgumentNullException(nameof(playerIdleTexture));
            _playerUpTexture = playerUpTexture ?? throw new ArgumentNullException(nameof(playerUpTexture));
            _playerDownTexture = playerDownTexture ?? throw new ArgumentNullException(nameof(playerDownTexture));
            _enemyNeutralTexture = enemyNeutralTexture ?? throw new ArgumentNullException(nameof(enemyNeutralTexture));
            _enemyHesitateTexture = enemyHesitateTexture ?? throw new ArgumentNullException(nameof(enemyHesitateTexture));
            _enemyAngryTexture = enemyAngryTexture ?? throw new ArgumentNullException(nameof(enemyAngryTexture));
            _playerDeadTexture = playerDeadTexture ?? throw new ArgumentNullException(nameof(playerDeadTexture));
            _playerEndTexture = playerEndTexture ?? throw new ArgumentNullException(nameof(playerEndTexture));
            _collectionPointUp = collectionPointUp ?? throw new ArgumentNullException(nameof(collectionPointUp));
            _collectionPointDown = collectionPointDown ?? throw new ArgumentNullException(nameof(collectionPointDown));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _backgroundTexture = backgroundTexture ?? throw new ArgumentNullException(nameof(backgroundTexture));
            _treesTexture = treesTexture ?? throw new ArgumentNullException(nameof(treesTexture));
            _grassTexture = grassTexture ?? throw new ArgumentNullException(nameof(grassTexture));

            _enemyRadius = 50f; // Радиус движения злодея
            _enemySpeed = model.CurrentLevel.BPM / 60f * MathHelper.Pi; // Устанавливаем скорость в такт музыке (полуокружность)
            _enemyAngle = 0f;
            _enemyMovingForward = true;
            _enemyPosition = new Vector2(400, 100); // Начальная позиция злодея
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

            // Обновляем положение врага по верхней полуокружности
            if (_model.AttentionMeterValue < 100f)
            {
                if (_enemyMovingForward)
                {
                    _enemyAngle += _enemySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_enemyAngle >= MathHelper.Pi)
                    {
                        _enemyAngle = MathHelper.Pi;
                        _enemyMovingForward = false;
                    }
                }
                else
                {
                    _enemyAngle -= _enemySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_enemyAngle <= 0)
                    {
                        _enemyAngle = 0;
                        _enemyMovingForward = true;
                    }
                }

                _enemyPosition = new Vector2(350 + _enemyRadius * (float)Math.Cos(_enemyAngle), 30 - _enemyRadius * (float)Math.Sin(_enemyAngle));
            }
            else
            {
                _model.EnemyPosition = new Vector2(400, 0); // Враг стоит на месте при AttentionMeter = 100
            }

            // Обновляем состояние мигания игрока при AttentionMeter = 100
            if (_model.AttentionMeterValue >= 100f)
            {
                _timeSinceLastBlink += gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeSinceLastBlink >= 0.25)
                {
                    _isPlayerDeadVisible = !_isPlayerDeadVisible;
                    _timeSinceLastBlink = 0;
                }
            }
        }

        public void Draw()
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height), Color.White);
            DrawBackground();

            // точки сбора
            _spriteBatch.Draw(_collectionPointUp, _model.CollectionPoint1.Position, Color.White);
            _spriteBatch.Draw(_collectionPointDown, _model.CollectionPoint2.Position, Color.White);

            // первая дорожка
            foreach (var note in _model.Track1.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTextureUp, note.Position, Color.White);
                }
            }

            // вторая дорожка
            foreach (var note in _model.Track2.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTextureDown, note.Position, Color.White);
                }
            }

            // Отрисовка шкалы внимания
            DrawAttentionMeter();

            // выбор текстуры игрока
            Texture2D playerTexture = _playerIdleTexture;
            if (_model.AttentionMeterValue >= 100f)
            {
                // Если показатель внимания игрока равен 100%, отображаем спрайт мертвого игрока
                playerTexture = _playerDeadTexture;
            }
            else if (_model.InputState.IsKeyDown(Keys.W) || _model.InputState.IsKeyDown(Keys.Up))
            {
                playerTexture = _playerUpTexture;
            }
            else if (_model.InputState.IsKeyDown(Keys.S) || _model.InputState.IsKeyDown(Keys.Down))
            {
                playerTexture = _playerDownTexture;
            }

            _spriteBatch.Draw(playerTexture, new Vector2(60, 100), Color.White);
            _spriteBatch.Draw(GetEnemyTexture(), _enemyPosition, Color.White);

            _spriteBatch.DrawString(_font, $"Score: {_model.Score}", new Vector2(10, 40), Color.White);
            _spriteBatch.DrawString(_font, $"Super: {_model.SuperCount}", new Vector2(10, 60), Color.White);
            _spriteBatch.DrawString(_font, $"Good: {_model.GoodCount}", new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_font, $"Ok: {_model.OkCount}", new Vector2(10, 100), Color.White);
            _spriteBatch.DrawString(_font, $"Bad: {_model.BadCount}", new Vector2(10, 120), Color.White);
            _spriteBatch.DrawString(_font, $"Miss: {_model.MissCount}", new Vector2(10, 140), Color.White);

            _spriteBatch.End();
        }

        private void DrawAttentionMeter()
        {
            float meterWidth = 200f;
            float meterHeight = 20f;
            float meterX = (_spriteBatch.GraphicsDevice.Viewport.Width - meterWidth) / 2;
            float meterY = 20f;
            float fillWidth = meterWidth * (_model.AttentionMeterValue / 100f);

            _spriteBatch.Draw(_attentionMeterTexture, new Rectangle((int)meterX, (int)meterY, (int)meterWidth, (int)meterHeight), Color.LightSlateGray);
            _spriteBatch.Draw(_attentionMeterTexture, new Rectangle((int)meterX, (int)meterY, (int)fillWidth, (int)meterHeight), Color.Red);

        }

        private Texture2D GetEnemyTexture()
        {
            // Возвращает текстуру злодея в зависимости от его состояния
            if (_model.AttentionMeterValue >= 100f)
            {
                return _enemyAngryTexture;
            }
            else if (_model.AttentionMeterValue > 75f)
            {
                return _enemyHesitateTexture;
            }
            else
            {
                return _enemyNeutralTexture;
            }
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
