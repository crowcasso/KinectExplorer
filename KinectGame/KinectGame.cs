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
using System.Reflection;


namespace KinectExplorer
{
    /// <summary>
    /// A game class with convenience methods for running with a Kinect.
    /// </summary>
    public abstract class KinectGame
    {
        /// <summary>
        /// The GraphicsDeviceManager, as would be found in a Game class
        /// </summary>
        protected readonly GraphicsDeviceManager graphics;


        private readonly GameWindow window;

        private bool finished;
        private SpriteBatch spriteBatch;
        private Texture2D jointTexture;

        private Size resolution;
        /// <summary>
        /// Gets the resolution of the running game
        /// </summary>
        public Size Resolution { get { return new Size(resolution.Width, resolution.Height); } }

        /// <summary>
        /// Gets the first Skeleton found by the Kinect
        /// </summary>
        protected Skeleton SkeletonA { get { return KinectManager.SkeletonA; } }
        /// <summary>
        /// Gets the second Skeleton found by the Kinect
        /// </summary>
        protected Skeleton SkeletonB { get { return KinectManager.SkeletonB; } }
        /// <summary>
        /// Get the GraphicsDevice for this game
        /// </summary>
        protected GraphicsDevice GraphicsDevice { get { return graphics.GraphicsDevice; } }
        /// <summary>
        /// The GameWindow, as would be found in a Game class
        /// </summary>
        protected GameWindow Window { get { return window; } }

        /// <summary>
        /// Should return a GameConfig describing this game
        /// </summary>
        /// <returns>The config</returns>
        public abstract GameConfig GetConfig();
        /// <summary>
        /// Should handle all initialization logic for this game
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Should load all content for this game
        /// </summary>
        /// <param name="content">A ContentLoader to load content</param>
        public abstract void LoadContent(ContentLoader content);
        /// <summary>
        /// Should unload all content for this game.
        /// It is VERY IMPORTANT that you unload all content here
        /// as other games may need to run after it.
        /// </summary>
        public virtual void UnloadContent() { }
        /// <summary>
        /// Should hande the update logic for this game.
        /// </summary>
        /// <param name="gameTime">The time elapsed since the last update</param>
        public abstract void Update(GameTime gameTime);
        /// <summary>
        /// Should handle the draw logic for this game.
        /// NOTE: You should only handle DRAW logic here. In threory, if this method
        /// is called twice without Update() being called, the screen should not change.
        /// </summary>
        /// <param name="gameTime">The time elapsed since the last draw</param>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Returns true if the Exit() method has been called
        /// to indicate that this game should exit.
        /// </summary>
        public bool Finished
        {
            get { return finished; }
        }

        /// <summary>
        /// Creates a KinectGame. This constructor should not
        /// be overridden.
        /// </summary>
        /// <param name="gParams">The GameParameters describing the environment in which the game is running.</param>
        public KinectGame(GameParameters gParams)
        {
            resolution = gParams.Resolution;
            graphics = gParams.Graphics;
            window = gParams.Window;
        }

        /// <summary>
        /// A factory method for creating an instance of a KinectGame
        /// from its type. For this method to work, a game must NOT
        /// modify the constructor.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="gParams"></param>
        /// <returns></returns>
        public static KinectGame Create(Type t, GameParameters gParams)
        {
            return (KinectGame)Activator.CreateInstance(t, new object[] { gParams });
        }

        /// <summary>
        /// When called, indicates that this game has finished and should exit.
        /// </summary>
        public void Exit()
        {
            finished = true;
        }

        /// <summary>
        /// Handles content loading for this game. To load custom content,
        /// override the LoadContent(ContentLoader) method instead.
        /// Games should not call this method.
        /// </summary>
        /// <param name="content">The ContentManager to use</param>
        /// <param name="contentRoot">The root directory for this game's content</param>
        public void LoadContent(ContentManager content, String contentRoot)
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            jointTexture = ShapesManager.CreateEllipseTexture(GraphicsDevice, 30, 30, Color.White, true);
            LoadContent(new ContentLoader(content, contentRoot));
        }

        /// <summary>
        /// Override this method to intercept Kinect SkeletonFrameReady events.
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="e">The parameters for this event</param>
        public virtual void OnKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// Override this method to intercept Kinect ColorFrameReady events.
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="e">The parameters for this event</param>
        public virtual void OnKinectVideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// Override this method to intercept Kinect DepthFrameReady events.
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="e">The parameters for this event</param>
        public virtual void OnKinectDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// A helper method to get the position of a Skeleton's join on the screen.
        /// This assumes the screen is projecting the Kinect's videosteam at full resolution.
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        protected Vector2 GetJointPosOnScreen(Joint joint)
        {
            return KinectManager.GetJointPosOnScreen(joint, resolution);
        }

        /// <summary>
        /// Draws SkeletonA and SkeletonB on the screen.
        /// </summary>
        protected void DebugSkeletons()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            DebugSkeletons(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws SkeletonA and SkeletonB on the screen using the given Spritebatch.
        /// </summary>
        /// <param name="spriteBatch"></param>
        protected void DebugSkeletons(SpriteBatch spriteBatch)
        {
            DrawSkeleton(spriteBatch, KinectManager.SkeletonA, new Color(255, 0, 0, 100));
            DrawSkeleton(spriteBatch, KinectManager.SkeletonB, new Color(0, 0, 255, 100));
        }

        /// <summary>
        /// Draws the given Skeleton on the screen using the given SpriteBatch and Color.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="skeleton"></param>
        /// <param name="color"></param>
        protected void DrawSkeleton(SpriteBatch spriteBatch, Skeleton skeleton, Color color)
        {
            if (skeleton == null) return;
            foreach (Joint joint in skeleton.Joints)
            {
                Vector2 position = KinectManager.GetJointPosOnScreen(joint, resolution);
                spriteBatch.Draw(jointTexture, new Rectangle(Convert.ToInt32(position.X) - 5, Convert.ToInt32(position.Y) - 5, 10, 10), color);
            }
        }

        /// <summary>
        /// Draws the camera to the full screen.
        /// </summary>
        protected void DrawCamera()
        {
            DrawCamera(new Rectangle(0, 0, Resolution.Width, Resolution.Height));
        }

        /// <summary>
        /// Draws the camera to the given Rectangle of the screen.
        /// </summary>
        /// <param name="rect"></param>
        protected void DrawCamera(Rectangle rect)
        {

            spriteBatch.Begin();
            DrawCamera(spriteBatch, rect);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws the Camera with the given Spritebatch to the full screen.
        /// </summary>
        /// <param name="spriteBatch"></param>
        protected void DrawCamera(SpriteBatch spriteBatch)
        {
            DrawCamera(spriteBatch, new Rectangle(0, 0, resolution.Width, resolution.Height));
        }

        /// <summary>
        /// Draws the Camera with the given Spritebatch to the given Rectangle of the screen.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="rect"></param>
        protected void DrawCamera(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D screen = KinectManager.GetScreen(graphics);
            if (screen != null)
            {
                spriteBatch.Draw(screen, rect, Color.White);
            }
        }
    }
}

