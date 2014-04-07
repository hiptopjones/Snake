using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class DirectionalController
    {
        
        // Distance from edge of button to the edge of the viewport
        private static int ExternalMargin = 20;

        // Distance from the center point to the edge of the button
        private static int InternalMargin = 20;

        private Rectangle _bounds;
        private Vector2 _center;

        private Texture2D _texture;

        public DirectionalController(Rectangle bounds)
        {
            _bounds = bounds;

            Initialize();
        }

        private void Initialize()
        {
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>(@"Chrome\ControllerButton");

            // This should be in Initialize(), but it depends on the loaded texture
            int centerX = _bounds.Right - _texture.Width - ExternalMargin - InternalMargin;
            int centerY = _bounds.Bottom - _texture.Height - ExternalMargin - InternalMargin;
            _center = new Vector2(centerX, centerY);
        }

        public void Update(GameTime gameTime)
        {
            // Nothing
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            // Down arrow
            batch.Draw(_texture, _center + new Vector2(0, InternalMargin), null, Color.White, 0, new Vector2(_texture.Width / 2, 0), 1, SpriteEffects.None, 1);

            // Left arrow
            batch.Draw(_texture, _center + new Vector2(-InternalMargin, 0), null, Color.White, (float)Math.PI / 2, new Vector2(_texture.Width / 2, 0), 1.5f, SpriteEffects.None, 1);
            
            // Up arrow
            batch.Draw(_texture, _center + new Vector2(-InternalMargin, -InternalMargin), null, Color.White, (float)Math.PI, new Vector2(_texture.Width / 2, 0), 2, SpriteEffects.None, 1);
            
            // Right arrow
            batch.Draw(_texture, _center + new Vector2(0, -InternalMargin), null, Color.White, (float)Math.PI * 3 / 2, new Vector2(_texture.Width / 2, 0), 1, SpriteEffects.None, 1);
        }
    }
}
