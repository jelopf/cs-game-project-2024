using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void Update(GameTime gameTime, InputState input, List<Note> notes, ref float attentionMeter, Keys collectionKey, GameModel model)
        {
            foreach (var note in notes.Where(n => n.IsActive).ToList())
            {
                if (IsCollidingWith(note) && input.IsKeyPressed(collectionKey))
                {
                    State = GetCollectionPointStateForNote(note, input, collectionKey);
                    UpdateScore(model, note);
                    note.IsActive = false; // Деактивация ноты
                    System.Diagnostics.Debug.WriteLine($"Note collected: {State}, Score: {model.Score}"); // Отладочный вывод
                }
            }
        }

        private bool IsCollidingWith(Note note)
        {
            return Vector2.Distance(Position, note.Position) < 10;
        }

        private CollectionPointState GetCollectionPointStateForNote(Note note, InputState input, Keys collectionKey)
        {
            if (note.Type == NoteType.Tap && input.IsKeyPressed(collectionKey))
            {
                return CollectionPointState.Super; // Поменяйте на нужную логику, если требуется
            }
            return CollectionPointState.Miss;
        }

        private void UpdateScore(GameModel model, Note note)
        {
            switch (State)
            {
                case CollectionPointState.Super:
                    model.SuperCount++;
                    model.Score += 250;
                    break;
                case CollectionPointState.Good:
                    model.GoodCount++;
                    model.Score += 150;
                    break;
                case CollectionPointState.Ok:
                    model.OkCount++;
                    model.Score += 100;
                    break;
                case CollectionPointState.Bad:
                    model.BadCount++;
                    model.Score += 50;
                    break;
                case CollectionPointState.Miss:
                    model.MissCount++;
                    model.Score -= 10;
                    break;
            }
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
        public NoteCollectionPoint CollectionPoint1 { get; set; }
        public NoteCollectionPoint CollectionPoint2 { get; set; }
        public Level CurrentLevel { get; private set; }
        private float _elapsedTime;

        public int SuperCount { get; set; }
        public int GoodCount { get; set; }
        public int OkCount { get; set; }
        public int BadCount { get; set; }
        public int MissCount { get; set; }
        public int Score { get; set; }

        public InputState InputState { get; private set; }

        public GameModel(Level level, GraphicsDevice graphicsDevice)
        {
            CollectionPoint1 = new NoteCollectionPoint(new Vector2(150, graphicsDevice.Viewport.Height - 100));
            CollectionPoint2 = new NoteCollectionPoint(new Vector2(150, graphicsDevice.Viewport.Height - 200));
            CurrentLevel = level;

            Track1 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 100));
            Track2 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 200));
            InputState = new InputState();
        }

        public void Update(GameTime gameTime, InputState input)
        {
            InputState = input;
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var noteData in CurrentLevel.Notes.Where(n => n.Time <= _elapsedTime).ToList())
            {
                if (noteData.Track == 1)
                {
                    Track1.AddNote(noteData.Type);
                }
                else if (noteData.Track == 2)
                {
                    Track2.AddNote(noteData.Type);
                }
                CurrentLevel.Notes.Remove(noteData); // Удаляем ноту из уровня после её добавления
            }

            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.W, this);
            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.Up, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.S, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.Down, this);

            UpdateNotes(gameTime, Track1.Notes);
            UpdateNotes(gameTime, Track2.Notes);

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
                // Деактивация нот, которые вышли за пределы экрана
                if (note.Position.X < 0)
                {
                    note.IsActive = false;
                }
            }
            // Удаление деактивированных нот
            notes.RemoveAll(n => !n.IsActive);
        }
    }
}
