using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public void Update(GameTime gameTime, InputState input, List<Note> notes, ref AttentionMeter attentionMeter, Keys collectionKey, GameModel model)
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

                    // Обновляем значение attentionMeter в зависимости от состояния
                    switch (State)
                    {
                        case CollectionPointState.Miss:
                            attentionMeter.Increase(20f); // Прибавляем 20%
                            break;
                        case CollectionPointState.Bad:
                            attentionMeter.Increase(5f); // Прибавляем 5%
                            break;
                        case CollectionPointState.Good:
                        case CollectionPointState.Super:
                            // Уменьшаем внимание на 5% с каждой пятой хорошей/супер нотой
                            model.GoodCount++;
                            if (model.GoodCount % 5 == 0)
                            {
                                attentionMeter.Decrease(5f);
                            }
                            break;
                    }
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
            if (offset < 50)
            {
                return CollectionPointState.Super;
            }
            else if (offset < 100)
            {
                return CollectionPointState.Good;
            }
            else if (offset < 150)
            {
                return CollectionPointState.Ok;
            }
            else if (offset < 200)
            {
                return CollectionPointState.Bad;
            }
            else
            {
                return CollectionPointState.Miss;
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

    public class AttentionMeter
    {
        private float _value = 0f;
        private bool _songEnded = false;

        public float Value
        {
            get { return _value; }
            private set { _value = MathHelper.Clamp(value, 0f, 100f); }
        }

        public bool SongEnded => _songEnded;

        public void Increase(float amount)
        {
            Value += amount;
        }

        public void Decrease(float amount)
        {
            Value -= amount;
        }

        public void Reset()
        {
            Value = 0f;
            _songEnded = false;
        }

        public void CheckWinLossCondition(float elapsedTime, float songDuration)
        {
            if (Value >= 100f && elapsedTime < songDuration)
            {
                // Проигрыш
                _songEnded = true;
            }
            else if (Value < 100f && elapsedTime >= songDuration)
            {
                // Победа
                _songEnded = true;
            }
        }
    }

    public class GameModel
    {
        public Track Track1 { get; set; }
        public Track Track2 { get; set; }
        private AttentionMeter _attentionMeter = new AttentionMeter();
        public float AttentionMeterValue => _attentionMeter.Value;
        public NoteCollectionPoint CollectionPoint1 { get; set; }
        public NoteCollectionPoint CollectionPoint2 { get; set; }
        public Level CurrentLevel { get; private set; }
        private float _elapsedTime;
        private Song _song;

        public int SuperCount { get; set; }
        public int GoodCount { get; set; }
        public int OkCount { get; set; }
        public int BadCount { get; set; }
        public int MissCount { get; set; }
        public int Score { get; set; }

        public InputState InputState { get; private set; }

        public Vector2 EnemyPosition { get; set; }
        public float MusicPosition => _elapsedTime / CurrentLevel.Duration;

        public GameModel(Level level, GraphicsDevice graphicsDevice, ContentManager content)
        {
            CollectionPoint1 = new NoteCollectionPoint(new Vector2(75, graphicsDevice.Viewport.Height - 110));
            CollectionPoint2 = new NoteCollectionPoint(new Vector2(75, graphicsDevice.Viewport.Height - 170));
            CurrentLevel = level;

            Track1 = new Track(new Vector2(graphicsDevice.Viewport.Width + 0, graphicsDevice.Viewport.Height - 110));
            Track2 = new Track(new Vector2(graphicsDevice.Viewport.Width + 0, graphicsDevice.Viewport.Height - 170));
            InputState = new InputState();

            // Загрузка и воспроизведение песни
            string songName = System.IO.Path.GetFileNameWithoutExtension(CurrentLevel.Song);
            _song = content.Load<Song>(songName);
            MediaPlayer.Play(_song);
            MediaPlayer.IsRepeating = false;
        }

        public void Update(GameTime gameTime, InputState input)
        {
            InputState = input;
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Проверяем, завершилась ли песня или игрок проиграл
            _attentionMeter.CheckWinLossCondition(_elapsedTime, CurrentLevel.Duration);
            if (_attentionMeter.SongEnded)
            {
                if (_attentionMeter.Value >= 100)
                {
                    // Проигрыш
                    System.Diagnostics.Debug.WriteLine("Game Over: Attention Meter is full.");
                }
                else
                {
                    // Победа
                    System.Diagnostics.Debug.WriteLine("Victory!");
                }
                return;
            }

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
                CurrentLevel.Notes.Remove(noteData);
            }

            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.S, this);
            CollectionPoint1.Update(gameTime, input, Track1.Notes, ref _attentionMeter, Keys.Down, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.W, this);
            CollectionPoint2.Update(gameTime, input, Track2.Notes, ref _attentionMeter, Keys.Up, this);

            UpdateNotes(gameTime, Track1.Notes);
            UpdateNotes(gameTime, Track2.Notes);
        }

        private void UpdateNotes(GameTime gameTime, List<Note> notes)
        {
            float speed = 100f;
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
