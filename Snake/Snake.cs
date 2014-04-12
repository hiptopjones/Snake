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
        private const int GrowthSpeedInMsPerSegment = 5000;
        private const int GrowthLengthInSegments = 2;
        private const int MinAbsoluteTouchDelta = 5;

        private int _speedInMsPerSegment;
        private Vector2 _direction;
        private List<Vector2> _nodes;
        private int _lengthInSegments;

        private double _lastMoveTotalMilliseconds;

        private bool _collidedWithSelf;

        public Snake(Vector2 position, Vector2 direction, int lengthInSegments, int speedInMsPerSegment)
        {
            // Start with a head and tail
            _nodes = new List<Vector2>();
            _nodes.Add(position);
            _nodes.Add(position);

            // Set the starting length
            _lengthInSegments = lengthInSegments;

            // Set the starting direction
            _direction = direction;

            // Set the starting speed
            _speedInMsPerSegment = speedInMsPerSegment;
        }

        public IEnumerable<Vector2> GetSegments()
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

        public void Update(GameTime time)
        {
            // In case we get re-spawned, reset the expected time
            double totalMilliseconds = time.TotalGameTime.TotalMilliseconds;
            if (_lastMoveTotalMilliseconds == 0)
            {
                _lastMoveTotalMilliseconds = totalMilliseconds;
            }

            // NOTE: Should always be at least two nodes (head and tail)
            Debug.Assert(_nodes.Count > 0);

            // Adjust the position of the head of the snake
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

        public void Grow()
        {
            _lengthInSegments++;
        }

        public void Accelerate()
        {
            _speedInMsPerSegment -= 5;
        }

        public void ApplyTouchDelta(Vector2 delta)
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

        public bool IsCollidedWithSelf()
        {
            return _collidedWithSelf;
        }

        public bool IsPartOfSnake(Vector2 test)
        {
            return !GetSegments().All(position => 
            {
                return position != test;
            });
        }
    }
}
