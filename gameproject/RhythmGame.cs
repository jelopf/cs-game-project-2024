using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    public class NoteCollectionPoint
    {
        // Позиция точки сбора нот на треке
        public Vector2 Position { get; private set; }

        // Состояние будет отражать результат взаимодействия с нотами (Супер, Хорошо, Норм, Плохо, Мисс).
        public CollectionPointState State { get; private set; }

        // Метод для обновления состояния точки сбора нот
        public void Update(GameTime gameTime, InputState input, List<Note> notes)
        {
            foreach (var note in notes)
            {
                if (IsCollidingWith(note))
                {
                    State = GetCollectionPointStateForNote(note, input);
                    // Дополнительная логика для обработки взаимодействия с нотой
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
            return Vector2.Distance(Position, note.Position) < 10; // примерное расстояние для столкновения
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

    public enum CollectionPointState
    {
        Super,
        Good,
        Ok,
        Bad,
        Miss
    }

    public class GameModel
    {
        public Track Track1 { get; set; } = new Track();
        public Track Track2 { get; set; } = new Track();
        public float AttentionMeter { get; set; } = 0.0f;
        public InputState InputState { get; private set; } = new InputState(); // Добавлено

        // Обновляем модели игры
        public void Update(GameTime gameTime)
        {
            InputState.Update(); // Добавлено

            // Логика обновления позиций нот, обработки ввода и т.д.
        }
    }

    public class GameView
    {
        private SpriteBatch _spriteBatch;
        private GameModel _model;

        public GameView(SpriteBatch spriteBatch, GameModel model)
        {
            _spriteBatch = spriteBatch;
            _model = model;
        }

        public void Draw()
        {
            // Рисование игры, используя _spriteBatch и _model
        }
    }

    public class GameController
    {
        private GameModel _model;

        public GameController(GameModel model)
        {
            _model = model;
        }

        public void Update(GameTime gameTime)
        {
            _model.InputState.Update(); // Добавлено
            // Дополнительная логика обновления модели на основе ввода пользователя
        }
    }

    public class RhythmGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameModel _model;
        private GameView _view;
        private GameController _controller;

        public RhythmGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _model = new GameModel();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new GameView(_spriteBatch, _model);
            _controller = new GameController(_model);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Загрузка контента
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _controller.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _view.Draw();

            base.Draw(gameTime);
        }
    }

    // Класс InputState для управления состоянием ввода пользователя
    public class InputState
    {
        public KeyboardState CurrentKeyboardState { get; private set; }
        public KeyboardState LastKeyboardState { get; private set; }

        public MouseState CurrentMouseState { get; private set; }
        public MouseState LastMouseState { get; private set; }

        // Конструктор
        public InputState()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
        }

        // Обновление состояний клавиатуры и мыши
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
        }

        // Проверка нажатия клавиши
        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
        }

        // Проверка отпуска клавиши
        public bool IsKeyReleased(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key) && LastKeyboardState.IsKeyDown(key);
        }

        // Проверка удержания клавиши
        public bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        // Проверка клика мыши
        public bool IsLeftMouseClicked()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
        }
    }
}
