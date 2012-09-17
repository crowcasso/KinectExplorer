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
        public TestGame(GraphicsDeviceManager graphics, Size resolution) : base(graphics, resolution) { }

        public override GameConfig GetConfig()
        {
            return new GameConfig("Test Game", "Thomas", "This is a test game to fill up space and be useless!");
        }

        public override void Initialize()
        {
            
        }

        public override void LoadContent(ContentLoader content)
        {
            
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
