using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using System.Diagnostics;

namespace KinectExplorer
{
    public class TestGame : KinectGame
    {
        private SpriteBatch spriteBatch;

        public TestGame(GameParameters gParams) : base(gParams) { }

        public override GameConfig GetConfig()
        {
            return new GameConfig("Kinect Test", "Thomas", "This shows you how the Kinect tracks your body.");
        }

        public override void Initialize()
        {
            
        }

        public override void LoadContent(ContentLoader content)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime time)
        {
            DrawCamera();
            DebugSkeletons();
        }
    }
}
