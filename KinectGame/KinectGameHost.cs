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

using System.Diagnostics;

namespace KinectExplorer
{
    /// <summary>
    /// Runs a KinectGame, simulating an environment in which
    /// it would be run with other games.
    /// 
    /// To test a game, only the following methods should be overridden:
    /// GetKinectGame(),
    /// GetResolution()
    /// </summary>
    public abstract class KinectGameHost : Game
    {
        protected KinectGame runningGame;
        protected GraphicsDeviceManager graphics;
        protected Size resolution;

        private bool altEnterUp = true;

        /// <summary>
        /// Creates the KinectGameHost.
        /// </summary>
        public KinectGameHost() : base()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Should return the type of your KinectGame.
        /// For instance, if you have a game named MyKinectGame, this should
        /// return typeof(MyKinectGame).
        /// </summary>
        /// <returns>The type of the KinectGame this is hosting</returns>
        protected abstract Type GetKinnectGame();
        /// <summary>
        /// Should return the resolution the game will be tested at.
        /// </summary>
        /// <returns></returns>
        protected virtual Size GetResolution() { return new Size(800, 600); }


        protected virtual String GetContentRoot(Type t) { return ""; }

        protected override void LoadContent()
        {
            base.LoadContent();
            Content.RootDirectory = "Content";
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            EndGame();
            KinectManager.Stop();
        }

        protected override void Initialize()
        {
            base.Initialize();

            resolution = GetResolution();

            graphics.PreferredBackBufferWidth = resolution.Width;
            graphics.PreferredBackBufferHeight = resolution.Height;
            graphics.ApplyChanges();

            KinectManager.Start(); 

            StartGame(GetKinnectGame());
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                if (altEnterUp)
                {
                    graphics.ToggleFullScreen();
                    altEnterUp = false;
                }
            }
            else
            {
                altEnterUp = true;
            }

            if (runningGame != null)
            {
                if (runningGame.Finished)
                {
                    EndGame();
                    Exit();
                }
                else
                {
                    UpdateGame(gameTime);
                }
            }
            else
            {
                UpdateHost(gameTime);
            }
        }

        protected virtual void UpdateGame(GameTime gameTime)
        {
            runningGame.Update(gameTime);
        }

        protected virtual void UpdateHost(GameTime gameTime) { }


        protected override void Draw(GameTime gameTime)
        {
            if (runningGame != null)
            {
                DrawGame(gameTime);
            }
            else
            {
                DrawHost(gameTime);
            }
        }

        protected virtual void DrawGame(GameTime gameTime)
        {
            runningGame.Draw(gameTime);
        }

        protected virtual void DrawHost(GameTime gameTime) { }

        protected virtual void StartGame(Type t)
        {
            try
            {
                KinectGame game = KinectGame.Create(t, new GameParameters(resolution, graphics, Window));

                game.LoadContent(Content, GetContentRoot(t));
                game.Initialize();

                KinectManager.Kinect.ColorFrameReady += game.OnKinectVideoFrameReady;
                KinectManager.Kinect.SkeletonFrameReady += game.OnKinectSkeletonFrameReady;
                KinectManager.Kinect.SkeletonFrameReady += game.OnKinectSkeletonFrameReady;
                KinectManager.Kinect.DepthFrameReady += game.OnKinectDepthFrameReady;
                runningGame = game;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        protected virtual void EndGame()
        {
            if (runningGame == null) return;

            KinectManager.Kinect.ColorFrameReady -= runningGame.OnKinectVideoFrameReady;
            KinectManager.Kinect.SkeletonFrameReady -= runningGame.OnKinectSkeletonFrameReady;
            KinectManager.Kinect.DepthFrameReady -= runningGame.OnKinectDepthFrameReady;

            runningGame.UnloadContent();
            runningGame = null;
        }
    }
}

