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
using Kinect.Toolbox;

namespace KinectExplorer
{
    public static class KinectManager
    {
        private static KinectSensor kinect;
        private static Skeleton skeletonA, skeletonB;
        
        private static Skeleton[] skeletonData;
        private static byte[] colorFrameData;
        private static Size colorFrameSize;
        private static Texture2D colorFrameTexture;

        public static Skeleton SkeletonA { get { return skeletonA; } }
        public static Skeleton SkeletonB { get { return skeletonB; } }
        public static KinectSensor Kinect { get { return kinect; } }

        private static Boolean started;

        public static void Start()
        {
            if (started)
                throw new Exception("Kinect already started! Do not call Start() or Stop() from your game");

            if (KinectSensor.KinectSensors.Count == 0)
            {
                throw new Exception("Prollem!");
            }

            kinect = KinectSensor.KinectSensors[0];
            kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(onKinectSkeletonFrameReady);
            kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(onKinectVideoFrameReady);
            kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(onKinectDepthFrameReady);
            kinect.Start();
            //kinect.ElevationAngle = 10;

            started = true;
        }

        private static void checkStarted()
        {
            if (!started) throw new Exception("Kinect not initialized. Do not call Kinect methods from your constructor - use the Initialize() method.");
        }

        private static void onKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            skeletonA = null;
            skeletonB = null;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    //Copy the skeleton data to our array
                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                }
            }

            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeletonA == null)
                        {
                            skeletonA = skel;
                        }
                        else if (skeletonB == null)
                        {
                            skeletonB = skel;
                        }
                    }
                }
            }
        }

        private static void onKinectVideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // Open image
            ColorImageFrame colorVideoFrame = e.OpenColorImageFrame();
            if (colorVideoFrame != null)
            {
                //Create array for pixel data and copy it from the image frame
                Byte[] pixelData = new Byte[colorVideoFrame.PixelDataLength];

                colorVideoFrame.CopyPixelDataTo(pixelData);
                //Convert RGBA to BGRA
                Byte[] bgraPixelData = new Byte[colorVideoFrame.PixelDataLength];
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    bgraPixelData[i] = pixelData[i + 2];
                    bgraPixelData[i + 1] = pixelData[i + 1];
                    bgraPixelData[i + 2] = pixelData[i];
                    bgraPixelData[i + 3] = (Byte)255; //The video comes with 0 alpha so it is transparent
                }

                colorFrameSize = new Size(colorVideoFrame.Width, colorVideoFrame.Height);
                colorFrameData = bgraPixelData;
                if (colorFrameTexture != null) colorFrameTexture.Dispose();
                colorFrameTexture = null;

                colorVideoFrame.Dispose();
            }
        }

        private static void onKinectDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

        }

        public static Microsoft.Xna.Framework.Vector2 GetJointPosOnScreen(Joint joint, Size screenSize)
        {
            checkStarted();
            Kinect.Toolbox.Vector2 v = Tools.Convert(kinect, joint.Position);
            return new Microsoft.Xna.Framework.Vector2(v.X * screenSize.Width, v.Y * screenSize.Height);
        }

        public static Texture2D GetScreen(GraphicsDeviceManager graphics)
        {
            checkStarted();
            if (colorFrameTexture != null) return colorFrameTexture;
            if (colorFrameData == null) return null;

            colorFrameTexture = new Texture2D(graphics.GraphicsDevice, colorFrameSize.Width, colorFrameSize.Height);
            colorFrameTexture.SetData<Byte>(colorFrameData);
            return colorFrameTexture;

        }

        public static void Stop()
        {
            checkStarted();
            kinect.Stop();
            started = false;
        }
    }
}
