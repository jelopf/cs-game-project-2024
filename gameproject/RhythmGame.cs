using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace gameproject
{
    public enum NoteType
    {
        Tap,
        Hold,
        Avoid
    }

    public class Note
    {
        public NoteType Type { get; set; }
        public Vector2 Position { get; set; } // Позиция ноты на экране 
        public bool IsActive { get; set; } // Активна ли нота для взаимодействия
        public float Duration { get; set; } // Длительность для Hold нот
    }

    public class Track
    {
        public List<Note> Notes { get; set; } = new List<Note>();
        // Добавляем ноты в трек
        public void AddNote(NoteType type, Vector2 position, float duration = 0.0f)
        {
            Notes.Add(new Note { Type = type, Position = position, IsActive = true, Duration = duration });
        }
    }

    public enum CollectionPointState
    {
        Super,
        Good,
        Ok,
        Bad,
        Miss
    }

    public class NoteCollectionPoint
    {
        // Позиция точки сбора нот на треке
        public Vector2 Position { get; private set; }

        // Состояние будет отражать результат взаимодействия с нотами (Супер, Хорошо, Норм, Плохо, Мисс).
        public CollectionPointState State { get; private set; }

        public NoteCollectionPoint(Vector2 initialPosition)
        {
            Position = initialPosition;
            State = CollectionPointState.Miss;
        }

        // Метод для обновления состояния точки сбора нот
        public void Update(GameTime gameTime, InputState input, List<Note> notes, ref float attentionMeter)
        {
            foreach (var note in notes)
            {
                if (IsCollidingWith(note))
                {
                    State = GetCollectionPointStateForNote(note, input);
                    if (State == CollectionPointState.Miss)
                    {
                        attentionMeter += 0.1f; // Увеличиваем шкалу внимания при пропуске ноты
                    }
                    note.IsActive = false; // Деактивируем ноту после взаимодействия
                }
            }

            // Обновление позиции точки сбора нот в зависимости от ввода пользователя
            if (input.IsKeyDown(Keys.W) || input.IsKeyDown(Keys.Up))
            {
                Position = new Vector2(Position.X, Position.Y - 1);
            }
            if (input.IsKeyDown(Keys.S) || input.IsKeyDown(Keys.Down))
            {
                Position = new Vector2(Position.X, Position.Y + 1);
            }
        }

        // Метод для проверки столкновения точки сбора нот с нотой
        private bool IsCollidingWith(Note note)
        {
            return note.IsActive && Vector2.Distance(Position, note.Position) < 10; // примерное расстояние для столкновения
        }

        // Метод для получения состояния точки сбора нот в зависимости от ноты и ввода
        private CollectionPointState GetCollectionPointStateForNote(Note note, InputState input)
        {
            if (note.Type == NoteType.Tap && input.IsKeyPressed(Keys.Space))
            {
                return CollectionPointState.Super;
            }
            return CollectionPointState.Miss;
        }
    }


    public class GameModel
    {
        public Track Track1 { get; set; } = new Track();
        public Track Track2 { get; set; } = new Track();
        private float _attentionMeter; // Приватное поле для хранения значения AttentionMeter
        public float AttentionMeter
        {
            get { return _attentionMeter; }
            private set { _attentionMeter = value; }
        }
        public InputState InputState { get; private set; } = new InputState();
        public NoteCollectionPoint CollectionPoint { get; set; }

        // Добавленные свойства
        public int SuperCount { get; private set; }
        public int GoodCount { get; private set; }
        public int OkCount { get; private set; }
        public int BadCount { get; private set; }
        public int MissCount { get; private set; }
        public int Score { get; private set; }

        public GameModel()
        {
            CollectionPoint = new NoteCollectionPoint(new Vector2(100, 100)); // Начальная позиция точки сбора нот
        }

        // Обновляем модели игры
        public void Update(GameTime gameTime)
        {
            InputState.Update();

            // Обновляем точку сбора нот
            CollectionPoint.Update(gameTime, InputState, Track1.Notes, ref _attentionMeter);
            CollectionPoint.Update(gameTime, InputState, Track2.Notes, ref _attentionMeter);

            // Обновляем количество попаданий и очки
            UpdateScore();

            // Логика обновления позиций нот, обработки ввода и т.д.
            // Проверка на проигрыш
            if (_attentionMeter >= 1.0f)
            {
                // Игрок проиграл, можно добавить дополнительную логику обработки
            }
        }

        private void UpdateScore()
        {
            // Обновление счетчиков попаданий и очков
            switch (CollectionPoint.State)
            {
                case CollectionPointState.Super:
                    SuperCount++;
                    Score += 250;
                    break;
                case CollectionPointState.Good:
                    GoodCount++;
                    Score += 150;
                    break;
                case CollectionPointState.Ok:
                    OkCount++;
                    Score += 100;
                    break;
                case CollectionPointState.Bad:
                    BadCount++;
                    Score += 50;
                    break;
                case CollectionPointState.Miss:
                    MissCount++;
                    Score -= 10;
                    break;
            }
        }
    }


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
        private SpriteFont _font;

        public GameView(SpriteBatch spriteBatch, GameModel model, Texture2D noteTexture, Texture2D attentionMeterTexture, Texture2D playerIdleTexture, Texture2D playerUpTexture, Texture2D playerDownTexture, Texture2D enemyTexture, SpriteFont font)
        {
            _spriteBatch = spriteBatch;
            _model = model;
            _noteTexture = noteTexture ?? throw new ArgumentNullException(nameof(noteTexture));
            _attentionMeterTexture = attentionMeterTexture ?? throw new ArgumentNullException(nameof(attentionMeterTexture));
            _playerIdleTexture = playerIdleTexture ?? throw new ArgumentNullException(nameof(playerIdleTexture));
            _playerUpTexture = playerUpTexture ?? throw new ArgumentNullException(nameof(playerUpTexture));
            _playerDownTexture = playerDownTexture ?? throw new ArgumentNullException(nameof(playerDownTexture));
            _enemyTexture = enemyTexture ?? throw new ArgumentNullException(nameof(enemyTexture));
            _font = font ?? throw new ArgumentNullException(nameof(font));
        }

        public void Draw()
        {
            _spriteBatch.Begin();

            // Рисование нот
            foreach (var note in _model.Track1.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }
            foreach (var note in _model.Track2.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }

            // Рисование точки сбора нот
            _spriteBatch.Draw(_noteTexture, _model.CollectionPoint.Position, Color.Red);

            // Рисование индикатора уровня внимания
            _spriteBatch.Draw(_attentionMeterTexture, new Rectangle(10, 10, (int)(_model.AttentionMeter * 200), 20), Color.Green);

            // Рисование персонажа
            Texture2D playerTexture = _playerIdleTexture;
            if (_model.InputState.IsKeyDown(Keys.W) || _model.InputState.IsKeyDown(Keys.Up))
            {
                playerTexture = _playerUpTexture;
            }
            else if (_model.InputState.IsKeyDown(Keys.S) || _model.InputState.IsKeyDown(Keys.Down))
            {
                playerTexture = _playerDownTexture;
            }
            _spriteBatch.Draw(playerTexture, new Vector2(50, 300), Color.White);

            // Рисование врага
            _spriteBatch.Draw(_enemyTexture, new Vector2(500, 300), Color.White);

            // Рисование счета
            _spriteBatch.DrawString(_font, "Super: " + _model.SuperCount, new Vector2(10, 40), Color.White);
            _spriteBatch.DrawString(_font, "Good: " + _model.GoodCount, new Vector2(10, 60), Color.White);
            _spriteBatch.DrawString(_font, "Ok: " + _model.OkCount, new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_font, "Bad: " + _model.BadCount, new Vector2(10, 100), Color.White);
            _spriteBatch.DrawString(_font, "Miss: " + _model.MissCount, new Vector2(10, 120), Color.White);
            _spriteBatch.DrawString(_font, "Score: " + _model.Score, new Vector2(10, 140), Color.White);

            _spriteBatch.End();
        }
    }


    // Класс для обработки ввода
    public class InputState
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        // Обновление состояния клавиатуры
        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }

        // Проверка, нажата ли клавиша
        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        // Проверка, была ли клавиша нажата в текущем кадре
        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }
    }

    public class RhythmGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameModel _model;
        private GameView _view;

        public RhythmGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _model = new GameModel();

            // Загрузка текстур и шрифтов
            Texture2D noteTexture = Content.Load<Texture2D>("Note");
            Texture2D attentionMeterTexture = Content.Load<Texture2D>("AttentionMeter");
            Texture2D playerIdleTexture = Content.Load<Texture2D>("playerIdleTexture");
            Texture2D playerUpTexture = Content.Load<Texture2D>("playerUpTexture");
            Texture2D playerDownTexture = Content.Load<Texture2D>("playerDownTexture");
            Texture2D enemyTexture = Content.Load<Texture2D>("enemyTexture");
            SpriteFont font = Content.Load<SpriteFont>("font");

            _view = new GameView(_spriteBatch, _model, noteTexture, attentionMeterTexture, playerIdleTexture, playerUpTexture, playerDownTexture, enemyTexture, font);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _model.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _view.Draw();

            base.Draw(gameTime);
        }
    }
}
