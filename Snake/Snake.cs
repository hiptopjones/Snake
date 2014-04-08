using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class Snake
    {
        private const int StartingSpeedInMsPerSegment = 350;
        private const int StartingGrowthInMsPerSegment = 5000;
        private const int StartingLengthInSegments = 5;
        private const int GrowthLengthInSegments = 2;
        private const int MinAbsoluteTouchDelta = 5;

        private Rectangle _bounds;
        private Random _random;

        private Texture2D _texture;
        private int _textureSize;
        private Rectangle _grid;

        private int _speedInMsPerSegment;
        private Vector2 _direction;
        private List<Vector2> _nodes;
        private int _lengthInSegments;

        private double _lastMoveTotalMilliseconds;

        private bool _collidedWithSelf;

        public SpriteFont Font
        {
            get;
            set;
        }

        public Snake(Rectangle bounds)
        {
            _bounds = bounds;

            Initialize();
        }

        private void Initialize()
        {
            _random = new Random();
        }

        public void LoadContent(ContentManager contentManager)
        {
            _texture = contentManager.Load<Texture2D>(@"Players\SnakeBodyRounded50x50");

            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            // Assumes the texture is a square sprite
            _textureSize = _texture.Width;

            // Create the grid based on squares that are the texture size
            _grid = new Rectangle(0, 0, _bounds.Width / _textureSize, _bounds.Height / _textureSize);
        }

        public void Update(GameTime time, Vector2 inputDelta)
        {
            if (_nodes == null)
            {
                Spawn();
            }

            ApplyTouchDelta(inputDelta);

            // NOTE: Should always be at least two nodes (head and tail)
            Debug.Assert(_nodes.Count > 0);

            // Adjust the position of the head of the snake
            double totalMilliseconds = time.TotalGameTime.TotalMilliseconds;
            int numSegmentsToMove = (int)Math.Floor((totalMilliseconds - _lastMoveTotalMilliseconds) / _speedInMsPerSegment);
            if (numSegmentsToMove > 0)
            {
                Vector2 head = _nodes[0] + (_direction * numSegmentsToMove);
                if (IsPartOfSnake(head))
                {
                    Debug.WriteLine("Collided with self!");

                    // Collision with itself
                    _collidedWithSelf = true;
                }
                else
                {
                    _nodes[0] = head;
                    _lastMoveTotalMilliseconds = totalMilliseconds;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            int digitIndex = 0;

            // TODO: ToList() may be too expensive
            GetSnakeSegementEnumerator().ToList().ForEach(position => 
                {
                    int x = (int)position.X * _textureSize;
                    int y = (int)position.Y * _textureSize;

                    batch.Draw(_texture, new Rectangle(x, y, _textureSize, _textureSize), Color.White);
                    batch.DrawString(Font, Pi.Instance[digitIndex++].ToString(), new Vector2(x, y), Color.Blue);
                });
        }

        public void Spawn()
        {
            // Clear game state
            _collidedWithSelf = false;

            // Start with a head and tail
            _nodes = new List<Vector2>();
            _nodes.Add(new Vector2(_grid.Width / 2, _grid.Height / 2));
            _nodes.Add(new Vector2(_grid.Width / 2, _grid.Height / 2));

            // Set the starting length
            _lengthInSegments = StartingLengthInSegments;

            // Set the starting direction
            _direction = new Vector2(1, 0);

            // Set the starting speed
            _speedInMsPerSegment = StartingSpeedInMsPerSegment;
        }

        public void Grow()
        {
            _lengthInSegments += GrowthLengthInSegments;
        }

        public void Accelerate()
        {
            _speedInMsPerSegment -= 10;
        }

        private void ApplyTouchDelta(Vector2 delta)
        {
            int absDeltaX = (int)Math.Abs(delta.X);
            int absDeltaY = (int)Math.Abs(delta.Y);

            if (absDeltaX >= MinAbsoluteTouchDelta && absDeltaX > absDeltaY)
            {
                // Only apply if changing direction
                if (_direction.X != 0)
                {
                    return;
                }

                _direction.X = Math.Sign(delta.X);
                _direction.Y = 0;

                // Duplicate current node
                _nodes.Insert(0, _nodes[0]);
            }
            else if (absDeltaY >= MinAbsoluteTouchDelta && absDeltaY > absDeltaX)
            {
                // Only apply if changing direction
                if (_direction.Y != 0)
                {
                    return;
                }

                _direction.Y = Math.Sign(delta.Y);
                _direction.X = 0;

                // Duplicate current node
                _nodes.Insert(0, _nodes[0]);
            }
            else
            {
                // Ignore when equal
            }
        }

        public bool IsOutOfBounds()
        {
            Vector2 position = _nodes[0];

            return (position.X < _grid.Left ||
                position.Y < _grid.Top ||
                (position.X) > _grid.Right ||
                (position.Y) > _grid.Bottom);
        }

        public bool IsCollidedWithSelf()
        {
            return _collidedWithSelf;
        }

        private IEnumerable<Vector2> GetSnakeSegementEnumerator()
        {
            float remainingLengthInSegments = _lengthInSegments;
            Vector2 previousNode = _nodes[0];

            for (int i = 1; i < _nodes.Count; i++)
            {
                if (remainingLengthInSegments <= 0)
                {
                    // Trim end nodes if possible
                    while (_nodes.Count > i)
                    {
                        _nodes.RemoveRange(i, _nodes.Count - i);
                    }

                    break;
                }

                Vector2 currentNode = _nodes[i];
                Vector2 delta = currentNode - previousNode;

                remainingLengthInSegments -= delta.Length();
                if (remainingLengthInSegments < 0)
                {
                    // Adjust the current node's position based on how far it overshot the remaining length
                    if (delta.X != 0)
                    {
                        delta.X = delta.X - (Math.Abs(remainingLengthInSegments) * Math.Sign(delta.X));
                    }

                    if (delta.Y != 0)
                    {
                        delta.Y = delta.Y - (Math.Abs(remainingLengthInSegments) * Math.Sign(delta.Y));
                    }

                    currentNode = previousNode + delta;

                    // Save the new current node
                    _nodes[i] = currentNode;
                }

                if (delta.X != 0)
                {
                    if (delta.X < 0)
                    {
                        for (float x = previousNode.X; x > currentNode.X; x--)
                        {
                            yield return new Vector2(x, currentNode.Y);
                        }
                    }
                    else
                    {
                        for (float x = previousNode.X; x < currentNode.X; x++)
                        {
                            yield return new Vector2(x, currentNode.Y);
                        }
                    }
                }
                else if (delta.Y != 0)
                {
                    if (delta.Y < 0)
                    {
                        for (float y = previousNode.Y; y > currentNode.Y; y--)
                        {
                            yield return new Vector2(currentNode.X, y);
                        }
                    }
                    else
                    {
                        for (float y = previousNode.Y; y < currentNode.Y; y++)
                        {
                            yield return new Vector2(currentNode.X, y);
                        }
                    }
                }

                previousNode = currentNode;
            }
        }

        private bool IsPartOfSnake(Vector2 test)
        {
            return !GetSnakeSegementEnumerator().All(position => 
            {
                return position != test;
            });
        }
    }
}
