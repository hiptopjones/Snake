using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Snake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SnakeGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _bounds;

        private TouchInput _touch;
        private Background _background;
        private Snake _snake;

        private SpriteFont _font;
        private SoundEffect _gameOverSound;

        private bool _soundEnabled;

        public SnakeGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _touch = new TouchInput();

            _soundEnabled = true;
        }

        public int DisplayWidth
        {
            get;
            private set;
        }

        public int DisplayHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _bounds = _graphics.GraphicsDevice.Viewport.Bounds;
            _background = new Background(_bounds);
            _snake = new Snake(_bounds);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Chrome sprites
            _background.LoadContent(Content);

            // Game sprites
            _snake.LoadContent(Content);

            // Sounds
            _gameOverSound = Content.Load<SoundEffect>(@"Audio\HomerSoftDoh");

            // Fonts
            // http://www.dafont.com/8bit-wonder.font
            _font = Content.Load<SpriteFont>(@"Fonts\RetroFont");
            _snake.Font = _font;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Check for back button presses
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            _background.Update(gameTime);

            // TODO: Need to check for collisions with self
            _touch.Update(gameTime);
            _snake.Update(gameTime, _touch.Delta);

            if (_snake.IsOutOfBounds() || _snake.IsCollidedWithSelf())
            {
                if (_soundEnabled)
                {
                    _gameOverSound.Play();
                }

                _snake.Spawn();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Chrome sprites
            _background.Draw(gameTime, _spriteBatch);

            // Game sprites
            _snake.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
