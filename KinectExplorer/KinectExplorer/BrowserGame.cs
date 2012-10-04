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

using Berkelium.Managed;

using System.Windows.Forms;

namespace KinectExplorer
{
    public class BrowserGame : KinectGame
    {
        WebBrowser browser;

        public BrowserGame(GameParameters gParams) : base(gParams) { }

        public override GameConfig GetConfig()
        {
            GameConfig config = new GameConfig("Pointillism", "Joel", "Draws a picture with dots.");
            //config.IsPassive = true;
            return config;
        }

        public override void Initialize()
        {
            browser = new WebBrowser();
            browser.Location = new System.Drawing.Point(-10, -15);
            browser.Size = new System.Drawing.Size(Resolution.Width - browser.Location.X + 20, Resolution.Height - browser.Location.Y + 20);
            browser.Navigate(@"http://trumpy.cs.elon.edu/joel/pointillism/");
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            Control.FromHandle(Window.Handle).Controls.Add(browser);
        }

        public override void LoadContent(ContentLoader content)
        {
        }

        public override void UnloadContent()
        {
            browser.Dispose();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            DrawCamera();
        }
    }
}
