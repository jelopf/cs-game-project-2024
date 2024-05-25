using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        public RhythmGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            var level = LevelLoader.LoadLevel("Content/level.json");
            _model = new GameModel(level, GraphicsDevice, Content);
            _inputState = new InputState();

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
            

            _view = new GameView(_spriteBatch, _model, _noteTextureDown, _noteTextureUp, _attentionMeterTexture, _playerIdleTexture, _playerUpTexture, _playerDownTexture, _enemyTexture, _collectionPointDown, _collectionPointUp, _font, _backgroundTexture, _treesTexture, _grassTexture);
            _controller = new GameController(_model, _view);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _inputState.Update(); // Обновление состояния ввода
            _controller.Update(gameTime, _inputState); // Передача обновленного состояния ввода
            _view.Update(gameTime); // Обновление фона

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