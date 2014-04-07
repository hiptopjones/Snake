using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class TouchInput
    {
        public TouchInput()
        {
            Initialize();
        }

        public Vector2 Delta
        {
            get;
            private set;
        }

        private void Initialize()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
        }

        public void Update(GameTime gameTime)
        {
            Delta = new Vector2();

            var samples = new List<GestureSample>();
            while (TouchPanel.IsGestureAvailable)
            {
                samples.Add(TouchPanel.ReadGesture());
            }

            foreach (var sample in samples)
            {
                if (sample.GestureType == GestureType.FreeDrag)
                {
                    Delta += sample.Delta;
                }
            }
        }
    }
}
