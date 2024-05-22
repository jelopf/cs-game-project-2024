using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace gameproject
{
    // Models
    public enum NoteType
    {
        Tap,
        Hold,
        Avoid
    }

    public class Note
    {
        public NoteType Type { get; set; }
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; }
        public float Duration { get; set; }
    }

    public class Track
    {
        public List<Note> Notes { get; set; } = new List<Note>();
        public Vector2 StartPosition { get; private set; }

        public Track(Vector2 startPosition)
        {
            StartPosition = startPosition;
        }

        public void AddNote(NoteType type, float duration = 0.0f)
        {
            var note = new Note { Type = type, Position = StartPosition, IsActive = true, Duration = duration };
            Notes.Add(note);
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
        public Vector2 Position { get; private set; }
        public CollectionPointState State { get; private set; }

        public NoteCollectionPoint(Vector2 initialPosition)
        {
            Position = initialPosition;
            State = CollectionPointState.Miss;
        }

        public void Update(GameTime gameTime, InputState input, List<Note> notes, ref float attentionMeter)
        {
            foreach (var note in notes)
            {
                if (IsCollidingWith(note))
                {
                    State = GetCollectionPointStateForNote(note, input);
                    if (State == CollectionPointState.Miss)
                    {
                        attentionMeter += 0.1f;
                    }
                    note.IsActive = false;
                }
            }
        }

        private bool IsCollidingWith(Note note)
        {
            return note.IsActive && Vector2.Distance(Position, note.Position) < 10;
        }

        private CollectionPointState GetCollectionPointStateForNote(Note note, InputState input)
        {
            if (note.Type == NoteType.Tap && input.IsKeyPressed(Keys.Space))
            {
                return CollectionPointState.Super;
            }
            return CollectionPointState.Miss;
        }
    }

    public class NoteData
    {
        public NoteType Type { get; set; }
        public float Time { get; set; }
        public int Track { get; set; }
    }

    public class Level
    {
        public List<NoteData> Notes { get; set; } = new List<NoteData>();
    }

    public class LevelLoader
    {
        public static Level LoadLevel(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Level>(json);
        }
    }

    public class GameModel
    {
        public Track Track1 { get; set; }
        public Track Track2 { get; set; }
        private float _attentionMeter;
        public float AttentionMeter
        {
            get { return _attentionMeter; }
            private set { _attentionMeter = value; }
        }
        public NoteCollectionPoint CollectionPoint { get; set; }
        public Level CurrentLevel { get; private set; }
        private float _elapsedTime;

        public int SuperCount { get; private set; }
        public int GoodCount { get; private set; }
        public int OkCount { get; private set; }
        public int BadCount { get; private set; }
        public int MissCount { get; private set; }
        public int Score { get; private set; }

        public InputState InputState { get; private set; }

        public GameModel(Level level, GraphicsDevice graphicsDevice)
        {
            CollectionPoint = new NoteCollectionPoint(new Vector2(10, graphicsDevice.Viewport.Height - 100));
            CurrentLevel = level;

            Track1 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 100));
            Track2 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 200));
            InputState = new InputState();
        }

        public void Update(GameTime gameTime, InputState input)
        {
            InputState = input;
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var noteData in CurrentLevel.Notes)
            {
                if (noteData.Time <= _elapsedTime)
                {
                    if (noteData.Track == 1)
                    {
                        Track1.AddNote(noteData.Type);
                    }
                    else if (noteData.Track == 2)
                    {
                        Track2.AddNote(noteData.Type);
                    }
                }
            }

            CollectionPoint.Update(gameTime, input, Track1.Notes, ref _attentionMeter);
            CollectionPoint.Update(gameTime, input, Track2.Notes, ref _attentionMeter);

            UpdateNotes(gameTime, Track1.Notes);
            UpdateNotes(gameTime, Track2.Notes);

            UpdateScore();

            if (_attentionMeter >= 1.0f)
            {
                // Handle game over logic
            }
        }

        private void UpdateNotes(GameTime gameTime, List<Note> notes)
        {
            float speed = 100f; // Скорость движения нот
            foreach (var note in notes)
            {
                // Обновление позиции нот
                note.Position = new Vector2(note.Position.X - speed * (float)gameTime.ElapsedGameTime.TotalSeconds, note.Position.Y);
            }
        }


        private void UpdateScore()
        {
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
            _spriteBatch.Begin();

            // Отрисовка нот на первом треке
            foreach (var note in _model.Track1.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }

            // Отрисовка нот на втором треке
            foreach (var note in _model.Track2.Notes)
            {
                if (note.IsActive)
                {
                    _spriteBatch.Draw(_noteTexture, note.Position, Color.White);
                }
            }

            _spriteBatch.Draw(_collectionPoint, new Vector2(50, 300), Color.Red);
            _spriteBatch.Draw(_attentionMeterTexture, new Rectangle(10, 10, (int)(_model.AttentionMeter * 200), 20), Color.Green);

            // Выбор текстуры игрока на основе ввода
            Texture2D playerTexture = _playerIdleTexture;
            if (_model.InputState.IsKeyDown(Keys.W) || _model.InputState.IsKeyDown(Keys.Up))
            {
                playerTexture = _playerUpTexture;
            }
            else if (_model.InputState.IsKeyDown(Keys.S) || _model.InputState.IsKeyDown(Keys.Down))
            {
                playerTexture = _playerDownTexture;
            }

            _spriteBatch.Draw(playerTexture, new Vector2(100, 150), Color.White);
            _spriteBatch.Draw(_enemyTexture, new Vector2(500, 100), Color.White);

            _spriteBatch.DrawString(_font, $"Score: {_model.Score}", new Vector2(10, 40), Color.White);
            _spriteBatch.DrawString(_font, $"Super: {_model.SuperCount}", new Vector2(10, 60), Color.White);
            _spriteBatch.DrawString(_font, $"Good: {_model.GoodCount}", new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_font, $"Ok: {_model.OkCount}", new Vector2(10, 100), Color.White);
            _spriteBatch.DrawString(_font, $"Bad: {_model.BadCount}", new Vector2(10, 120), Color.White);
            _spriteBatch.DrawString(_font, $"Miss: {_model.MissCount}", new Vector2(10, 140), Color.White);

            _spriteBatch.End();
        }
    }

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

    // Main Game Class
    public class RhythmGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameModel _model;
        private GameView _view;
        private GameController _controller;
        private InputState _inputState;

        private Texture2D _noteTexture;
        private Texture2D _attentionMeterTexture;
        private Texture2D _playerIdleTexture;
        private Texture2D _playerUpTexture;
        private Texture2D _playerDownTexture;
        private Texture2D _enemyTexture;
        private Texture2D _collectionPoint;
        private SpriteFont _font;

        public RhythmGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            var level = LevelLoader.LoadLevel("Content/level.json");
            _model = new GameModel(level, GraphicsDevice);
            _inputState = new InputState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _noteTexture = Content.Load<Texture2D>("Note");
            _attentionMeterTexture = Content.Load<Texture2D>("AttentionMeter");
            _playerIdleTexture = Content.Load<Texture2D>("playerIdleTexture");
            _playerUpTexture = Content.Load<Texture2D>("playerUpTexture");
            _playerDownTexture = Content.Load<Texture2D>("playerDownTexture");
            _enemyTexture = Content.Load<Texture2D>("enemyTexture");
            _collectionPoint = Content.Load<Texture2D>("CollectionPoint");
            _font = Content.Load<SpriteFont>("font");

            _view = new GameView(_spriteBatch, _model, _noteTexture, _attentionMeterTexture, _playerIdleTexture, _playerUpTexture, _playerDownTexture, _enemyTexture, _collectionPoint, _font);
            _controller = new GameController(_model, _view);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _inputState.Update();
            _controller.Update(gameTime, _inputState);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _controller.Draw();

            base.Draw(gameTime);
        }
    }
}
