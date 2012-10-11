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
    public class DepthGame : KinectGame
    {
        private SpriteBatch spriteBatch;

        public DepthGame(GameParameters gParams) : base(gParams) { }

        public override GameConfig GetConfig()
        {
            return new GameConfig("Depth Test", "Thomas", "This shows you how the Kinect tracks depth.", true, 30);
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

            Texture2D depth = KinectManager.GetDepthMap(graphics);
            spriteBatch.Begin();
            spriteBatch.Draw(depth, new Rectangle(0, 0, Resolution.Width, Resolution.Height), Color.White);
            spriteBatch.End();
        }
    }
}
