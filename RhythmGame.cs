using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace gameproject
{
    // Main Game Class
    public class RhythmGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameModel _model;
        private GameView _view;
        private GameController _controller;
        private InputState _inputState;
        private Texture2D _noteTextureDown;
        private Texture2D _noteTextureUp;
        private Texture2D _attentionMeterTexture;
        private Texture2D _playerIdleTexture;
        private Texture2D _playerUpTexture;
        private Texture2D _playerDownTexture;
        private Texture2D _enemyTexture;
        private Texture2D _collectionPointDown;
        private Texture2D _collectionPointUp;
        private SpriteFont _font;
        private Texture2D _grassTexture;
        private Texture2D _treesTexture;
        private Texture2D _backgroundTexture;

        private MainMenu _mainMenu;
        private VolumeSettingsMenu _volumeSettingsMenu;
        private PauseMenu _pauseMenu;
        private bool _isGameActive;
        private bool _isVolumeSettingsActive;
        private bool _isPaused;
        private float _menuVolume = 1.0f;
        private float _gameVolume = 1.0f;
        private Song _menuMusic;
        private Song _backgroundMusic;
        private Texture2D _enemyNeutralTexture;
        private Texture2D _enemyHesitateTexture;
        private Texture2D _enemyAngryTexture;
        private Texture2D _playerDeadTexture;
        private Texture2D _playerEndTexture;

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
            _noteTextureDown = Content.Load<Texture2D>("NoteDown");
            _noteTextureUp = Content.Load<Texture2D>("NoteUp");
            _attentionMeterTexture = Content.Load<Texture2D>("AttentionMeter");
            _playerIdleTexture = Content.Load<Texture2D>("playerIdleTexture");
            _playerUpTexture = Content.Load<Texture2D>("playerUpTexture");
            _playerDownTexture = Content.Load<Texture2D>("playerDownTexture");
            _enemyTexture = Content.Load<Texture2D>("enemyTexture");
            _collectionPointUp = Content.Load<Texture2D>("CollectionPointUp");
            _collectionPointDown = Content.Load<Texture2D>("CollectionPointDown");
            _font = Content.Load<SpriteFont>("font");
            _backgroundTexture = Content.Load<Texture2D>("level_bg");
            _grassTexture = Content.Load<Texture2D>("grass_bg");
            _treesTexture = Content.Load<Texture2D>("trees_bg");
            _enemyNeutralTexture = Content.Load<Texture2D>("enemyNeutral");
            _enemyHesitateTexture = Content.Load<Texture2D>("enemyHesitate");
            _enemyAngryTexture = Content.Load<Texture2D>("enemyAngry");
            _playerDeadTexture = Content.Load<Texture2D>("PlayerDead");
            _playerEndTexture = Content.Load<Texture2D>("PlayerEnd");

            _mainMenu = new MainMenu(GraphicsDevice, _spriteBatch, _font);
            _volumeSettingsMenu = new VolumeSettingsMenu(GraphicsDevice, _spriteBatch, _font, _menuVolume, _gameVolume);
            _pauseMenu = new PauseMenu(GraphicsDevice, _spriteBatch, _font, _gameVolume);
            _inputState = new InputState();

            _menuMusic = Content.Load<Song>("MenuMusic");
            _backgroundMusic = Content.Load<Song>("BackgroundMusic");
            MediaPlayer.Play(_menuMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = _menuVolume;
        }

        protected override void Update(GameTime gameTime)
        {
            _inputState.Update();

            if (_isGameActive)
            {
                if (!_isPaused)
                {
                    _controller.Update(gameTime, _inputState);
                    _view.Update(gameTime); // Обновление фона
                    if (_inputState.IsKeyPressed(Keys.Escape))
                    {
                        PauseGame();
                    }
                }
                else
                {
                    _pauseMenu.Update(_inputState, this);
                    if (_inputState.IsKeyPressed(Keys.Escape))
                    {
                        ResumeGame();
                    }
                }
            }
            else if (_isVolumeSettingsActive)
            {
                _volumeSettingsMenu.Update(_inputState, this);
            }
            else
            {
                _mainMenu.Update(_inputState, this);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_isGameActive)
            {
                if (!_isPaused)
                {
                    _controller.Draw();
                }
                else
                {
                    _pauseMenu.Draw();
                }
            }
            else if (_isVolumeSettingsActive)
            {
                _volumeSettingsMenu.Draw();
            }
            else
            {
                _mainMenu.Draw();
            }

            base.Draw(gameTime);
        }

        public void AdjustVolume()
        {
            _isVolumeSettingsActive = true;
        }

        public void OpenVolumeSettings()
        {
            _isPaused = false;
            _isVolumeSettingsActive = true;
        }

        public void SaveVolumeSettings(float menuVolume, float gameVolume)
        {
            _menuVolume = menuVolume;
            _gameVolume = gameVolume;
            MediaPlayer.Volume = _menuVolume; // Обновление громкости музыки меню
        }

        public void GoToMainMenu()
        {
            _isVolumeSettingsActive = false;
            _isGameActive = false;
            _volumeSettingsMenu.Reset(); // Reset the volume settings menu state
            MediaPlayer.Play(_menuMusic);
            MediaPlayer.Volume = _menuVolume;
        }

        public void StartGame()
        {
            var level = LevelLoader.LoadLevel("Content/level.json");
            _model = new GameModel(level, GraphicsDevice, Content);
            _view = new GameView(_spriteBatch,
                                _model,
                                _noteTextureDown,
                                _noteTextureUp,
                                _attentionMeterTexture,
                                _playerIdleTexture,
                                _playerUpTexture,
                                _playerDownTexture,
                                _enemyNeutralTexture,
                                _enemyHesitateTexture,
                                _enemyAngryTexture,
                                _playerDeadTexture,
                                _playerEndTexture,
                                _collectionPointDown,
                                _collectionPointUp,
                                _font,
                                _backgroundTexture,
                                _treesTexture,
                                _grassTexture);

            _controller = new GameController(_model, _view);
            _isGameActive = true;
            MediaPlayer.Play(_backgroundMusic);
            MediaPlayer.Volume = _gameVolume;
        }

        public void PauseGame()
        {
            _isPaused = true;
            MediaPlayer.Pause(); // Pause any playing music
        }

        public void ResumeGame()
        {
            _isPaused = false;
            MediaPlayer.Resume(); // Resume music if paused
        }

        public new void Exit()
        {
            base.Exit();
        }

        public void ReturnToMainMenuFromPause()
        {
            _isPaused = false;
            _isGameActive = false;
            _volumeSettingsMenu.Reset();
            MediaPlayer.Stop();
            GoToMainMenu();
        }

        public void OpenVolumeSettingsFromPause()
        {
            _isPaused = false;
            _isVolumeSettingsActive = true;
        }

        public bool IsGamePaused()
        {
            return _isPaused;
        }

        public float GetGameVolume()
        {
            return _gameVolume;
        }

        public void SetGameVolume(float volume)
        {
            _gameVolume = volume;
            MediaPlayer.Volume = _gameVolume;
        }
    }
}
