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
