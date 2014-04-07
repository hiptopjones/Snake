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
    public class Background
    {
        private Rectangle _bounds;
        private Texture2D _texture;

        public Background(Rectangle bounds)
        {
            _bounds = bounds;
        }

        private void Initialize()
        {
            // Nothing
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>(@"Chrome\Background");
        }

        public void Update(GameTime gameTime)
        {
            // Nothing
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.Draw(_texture, _bounds, Color.White);
        }
    }
}
