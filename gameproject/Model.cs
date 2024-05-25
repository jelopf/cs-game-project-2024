using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

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
            if (input.IsKeyPressed(collectionKey))
            {
                Note closestNote = GetClosestNote(notes);
                if (closestNote != null)
                {
                    float offset = CalculateOffset(closestNote); // Рассчитываем оффсет нажатия

                    State = GetCollectionPointStateForOffset(offset); // Получаем состояние точки сбора в зависимости от оффсета

                    UpdateScore(model, State); // Обновляем очки игрока

                    closestNote.IsActive = false; // Деактивация ноты
                    System.Diagnostics.Debug.WriteLine($"Note collected: {State}, Offset: {offset}, Score: {model.Score}"); // Отладочный вывод
                }
            }
        }

        private Note GetClosestNote(List<Note> notes)
        {
            Note closestNote = null;
            float minDistance = float.MaxValue;

            foreach (var note in notes.Where(n => n.IsActive))
            {
                float distance = CalculateOffset(note);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNote = note;
                }
            }

            return closestNote;
        }

        private float CalculateOffset(Note note)
        {
            // Рассчитываем оффсет нажатия в зависимости от положения ноты
            return Math.Abs(Position.X - note.Position.X);
        }

        private CollectionPointState GetCollectionPointStateForOffset(float offset)
        {
            // Определяем состояние точки сбора в зависимости от оффсета нажатия
            if (offset < 50) // Примерное значение порога для супер-попадания
            {
                return CollectionPointState.Super;
            }
            else if (offset < 100) // Примерное значение порога для хорошего попадания
            {
                return CollectionPointState.Good;
            }
            else if (offset < 150) // Примерное значение порога для нормального попадания
            {
                return CollectionPointState.Ok;
            }
            else if (offset < 200) // Примерное значение порога для плохого попадания
            {
                return CollectionPointState.Bad;
            }
            else
            {
                return CollectionPointState.Miss; // Если оффсет слишком большой, считаем, что игрок промахнулся
            }
        }

        private void UpdateScore(GameModel model, CollectionPointState state)
        {
            switch (state)
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
        public string Song { get; set; }
        public float Duration { get; set; }
        public float BPM { get; set; }
        public List<NoteData> Notes { get; set; } = new List<NoteData>();
    }

    public static class LevelLoader
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
        private Song _song; // Добавляем поле для песни

        public int SuperCount { get; set; }
        public int GoodCount { get; set; }
        public int OkCount { get; set; }
        public int BadCount { get; set; }
        public int MissCount { get; set; }
        public int Score { get; set; }

        public InputState InputState { get; private set; }

        public GameModel(Level level, GraphicsDevice graphicsDevice, ContentManager content)
        {
            CollectionPoint1 = new NoteCollectionPoint(new Vector2(150, graphicsDevice.Viewport.Height - 100));
            CollectionPoint2 = new NoteCollectionPoint(new Vector2(150, graphicsDevice.Viewport.Height - 200));
            CurrentLevel = level;

            Track1 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 100));
            Track2 = new Track(new Vector2(graphicsDevice.Viewport.Width + 100, graphicsDevice.Viewport.Height - 200));
            InputState = new InputState();

            // Загрузка и воспроизведение песни
            string songNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(CurrentLevel.Song);
            _song = content.Load<Song>(songNameWithoutExtension);
            MediaPlayer.Play(_song);
            MediaPlayer.IsRepeating = false;
        }

        public void Update(GameTime gameTime, InputState input)
        {
            InputState = input; // Убедитесь, что обновление происходит здесь
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

            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.S, this);
            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.Down, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.W, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.Up, this);

            UpdateNotes(gameTime, Track1.Notes);
            UpdateNotes(gameTime, Track2.Notes);

            if (_attentionMeter >= 1.0f)
            {
                // Handle game over logic
                System.Diagnostics.Debug.WriteLine("Game Over: Attention Meter is full.");
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