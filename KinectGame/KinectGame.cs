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
    public abstract class KinectGame
    {
        protected GraphicsDeviceManager graphics;
        private bool finished;
        private SpriteBatch spriteBatch;
        private Texture2D jointTexture;

        private Size resolution;
        public Size Resolution { get { return new Size(resolution.Width, resolution.Height); } }

        protected Skeleton SkeletonA { get { return KinectManager.SkeletonA; } }
        protected Skeleton SkeletonB { get { return KinectManager.SkeletonB; } }

        public abstract GameConfig GetConfig();
        public abstract void Initialize();
        public abstract void LoadContent(ContentLoader content);
        public virtual void UnloadContent() { }
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);

        public bool Finished
        {
            get { return finished; }
        }

        public KinectGame(GraphicsDeviceManager graphics, Size resolution)
        {
            this.resolution = resolution;
            this.graphics = graphics;
        }

        public static KinectGame Create(Type t, GraphicsDeviceManager graphics, Size resolution)
        {
            return (KinectGame)Activator.CreateInstance(t, new object[] { graphics, resolution });
        }

        public void Exit()
        {
            finished = true;
        }

        public void LoadContent(ContentManager content, String contentRoot)
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            jointTexture = content.Load<Texture2D>("joint");
            LoadContent(new ContentLoader(content, contentRoot));
        }

        public virtual void OnKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
        }

        public virtual void OnKinectVideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
        }

        public virtual void OnKinectDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
        }

        protected Vector2 GetJointPosOnScreen(Joint joint)
        {
            return KinectManager.GetJointPosOnScreen(joint, resolution);
        }

        protected void DebugSkeletons()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            DebugSkeletons(spriteBatch);
            spriteBatch.End();
        }

        protected void DebugSkeletons(SpriteBatch spriteBatch)
        {
            DrawSkeleton(spriteBatch, KinectManager.SkeletonA, new Color(255, 0, 0, 100));
            DrawSkeleton(spriteBatch, KinectManager.SkeletonB, new Color(0, 0, 255, 100));
        }

        protected void DrawSkeleton(SpriteBatch spriteBatch, Skeleton skeleton, Color color)
        {
            if (skeleton == null) return;
            foreach (Joint joint in skeleton.Joints)
            {
                Vector2 position = KinectManager.GetJointPosOnScreen(joint, resolution);
                spriteBatch.Draw(jointTexture, new Rectangle(Convert.ToInt32(position.X) - 5, Convert.ToInt32(position.Y) - 5, 10, 10), color);
            }
        }

        protected void DrawCamera()
        {
            DrawCamera(new Rectangle(0, 0, Resolution.Width, Resolution.Height));
        }

        protected void DrawCamera(Rectangle rect)
        {

            spriteBatch.Begin();
            DrawCamera(spriteBatch, rect);
            spriteBatch.End();
        }

        protected void DrawCamera(SpriteBatch spriteBatch)
        {
            DrawCamera(spriteBatch, new Rectangle(0, 0, resolution.Width, resolution.Height));
        }

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

