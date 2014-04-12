using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SnakeGame : Game
    {
        private const int StartingSpeedInMsPerSegment = 350;
        private const int StartingLengthInSegments = 4;
        private const int MaxCollectedDigitsToShow = 20;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _viewBounds;
        private Rectangle _gameBounds;
        private Rectangle _grid;

        private Dictionary<int, Vector2> _items;
        private Random _random;

        private Texture2D _backgroundTexture;
        private Texture2D _snakeTexture;

        private TouchInput _touch;
        private Snake _snake;

        private string _collectedDigits;
        private int _score;

        private SpriteFont _font;

        private SoundEffect _correctDigitSound;
        private SoundEffect _incorrectDigitSound;
        private SoundEffect _gameOverSound;

        private bool _soundEnabled;

        public SnakeGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _items = new Dictionary<int, Vector2>();
            _random = new Random();

            _touch = new TouchInput();
            _soundEnabled = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _viewBounds = _graphics.GraphicsDevice.Viewport.Bounds;
            _gameBounds = new Rectangle(0, 0, _viewBounds.Width, _viewBounds.Height - 100);

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

            // Sprites
            _snakeTexture = Content.Load<Texture2D>(@"Players\SnakeBodyRounded50x50");
            _backgroundTexture = Content.Load<Texture2D>(@"Chrome\Background");

            // Sounds
            _gameOverSound = Content.Load<SoundEffect>(@"Audio\HomerSoftDoh");
            _correctDigitSound = Content.Load<SoundEffect>(@"Audio\Success");
            _incorrectDigitSound = Content.Load<SoundEffect>(@"Audio\Fail");

            // Fonts
            _font = Content.Load<SpriteFont>(@"Fonts\SegoeFont");

            // Verify snake segments are squares
            Debug.Assert(_snakeTexture.Width == _snakeTexture.Height);

            // Generate game grid based on squares that are the texture size
            _grid = new Rectangle(0, 0, _gameBounds.Width / _snakeTexture.Width - 1, _gameBounds.Height / _snakeTexture.Height - 1);

            // Spawn the first snake
            RestartGame();
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

            _touch.Update(gameTime);

            _snake.ApplyTouchDelta(_touch.Delta);
            _snake.Update(gameTime);

            // Check for death, and respawn
            if (IsSnakeOutOfBounds() || _snake.IsCollidedWithSelf())
            {
                if (_soundEnabled)
                {
                    _gameOverSound.Play();
                }

                RestartGame();
            }

            // Check for collisions, and add any missing digits
            foreach (int i in Enumerable.Range(0, 10))
            {
                Vector2 position;

                if (_items.TryGetValue(i, out position))
                {
                    var head = _snake.GetSegments().FirstOrDefault();
                    if (head != null)
                    {
                        if (head.X == position.X && head.Y == position.Y)
                        {
                            string attemptedPi = _collectedDigits + i.ToString();
                            if (Pi.Instance.Digits.StartsWith(attemptedPi))
                            {
                                _collectedDigits = attemptedPi;

                                if (_soundEnabled)
                                {
                                    _correctDigitSound.Play();
                                }

                                _snake.Grow();
                                _snake.Accelerate();

                                _score++;
                            }
                            else
                            {
                                if (_soundEnabled)
                                {
                                    _incorrectDigitSound.Play();
                                }

                                // Incorrect guesses take one off your score
                                _score -= (_score > 0 ? 1 : 0);
                            }

                            _items.Remove(i);
                        }
                    }
                }
                else
                {
                    // Loop trying to find open spaces
                    while (true)
                    {
                        // TODO: Avoid putting it in front of the user
                        int x = _random.Next(0, _grid.Width - 1);
                        int y = _random.Next(0, _grid.Height - 1);

                        position = new Vector2(x, y);

                        // Check against snake segments
                        if (!_snake.IsPartOfSnake(position))
                        {
                            _items[i] = position;
                            break;
                        }

                        // Check against existing items
                        foreach (Vector2 test in _items.Values)
                        {
                            if (test.X == position.X && test.Y == position.Y)
                            {
                                break;
                            }
                        }
                    }
                }
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

            _spriteBatch.Draw(_backgroundTexture, _gameBounds, Color.White);

            int textureSize = _snakeTexture.Width;

            foreach (var segment in _snake.GetSegments())
            {
                int x = (int)segment.X * textureSize;
                int y = (int)segment.Y * textureSize;

                _spriteBatch.Draw(_snakeTexture, new Rectangle(x, y, textureSize, textureSize), Color.White);
            }

            foreach (var key in _items.Keys)
            {
                string digit = key.ToString();
                Vector2 position = _items[key];

                int x = (int)position.X * textureSize;
                int y = (int)position.Y * textureSize;

                // Draw the item
                _spriteBatch.Draw(_snakeTexture, new Rectangle(x, y, textureSize, textureSize), Color.White);

                // Draw the digit
                Vector2 digitPosition = new Vector2(x + (textureSize / 2), y + (textureSize / 2));
                _spriteBatch.DrawString(_font, digit, digitPosition, Color.Blue, 0, _font.MeasureString(digit) / 2, 1, SpriteEffects.None, 0);
            }

            // Draw the collected PI string
            string collectedDigits = _collectedDigits;
            if (collectedDigits.Length > MaxCollectedDigitsToShow)
            {
                collectedDigits = collectedDigits.Substring(collectedDigits.Length - MaxCollectedDigitsToShow);
            }

            Vector2 collectedDigitsPosition = new Vector2(10, _viewBounds.Height - _font.MeasureString(collectedDigits).Y - 10);
            _spriteBatch.DrawString(_font, collectedDigits, collectedDigitsPosition, Color.Yellow, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);

            // TODO: Draw the next digit as a hint (in grey?) after some time

            // Draw the collected score string
            string score = "Score: " + _score;
            Vector2 renderedScoreSize = _font.MeasureString(score);
            Vector2 scorePosition = new Vector2(_viewBounds.Width - renderedScoreSize.X - 10, _viewBounds.Height - renderedScoreSize.Y - 10);
            _spriteBatch.DrawString(_font, score, scorePosition, Color.Yellow, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void RestartGame()
        {
            _snake = new Snake(
                new Vector2(_grid.Width / 2, _grid.Height / 2),
                new Vector2(1, 0),
                StartingLengthInSegments,
                StartingSpeedInMsPerSegment);

            _items.Clear();

            _score = 0;

            // Default to what everyone knows
            _collectedDigits = "3.14";
        }

        private bool IsSnakeOutOfBounds()
        {
            var head = _snake.GetSegments().FirstOrDefault();
            if (head == null)
            {
                return false;
            }
            
            return (head.X < _grid.Left || head.Y < _grid.Top || head.X > _grid.Right || head.Y > _grid.Bottom);
        }
    }
}
