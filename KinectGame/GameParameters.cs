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

namespace KinectExplorer
{
    /// <summary>
    /// An wrapper object for the data a KinectGame needs to run.
    /// This class will be passed to a KinectGame by the KinectGameHost and read
    /// - there is no reason to interact with it.
    /// </summary>
    public struct GameParameters
    {
        public readonly Size Resolution;
        public readonly GraphicsDeviceManager Graphics;
        public readonly GameWindow Window;

        public GameParameters(Size resolution, GraphicsDeviceManager graphics, GameWindow window)
        {
            this.Resolution = resolution;
            this.Graphics = graphics;
            this.Window = window;
        }
    }
}
