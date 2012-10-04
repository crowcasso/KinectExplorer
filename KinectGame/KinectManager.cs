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
    /// <summary>
    /// A singleton class controlling the Kinect.
    /// </summary>
    public static class KinectManager
    {
        private static KinectSensor kinect;
        private static Skeleton skeletonA, skeletonB;
        
        private static Skeleton[] skeletonData;
        private static byte[] colorFrameData;
        private static Size colorFrameSize;
        private static Texture2D colorFrameTexture;
        private static short[] depthData;
        private static int depthFrameWidth, depthFrameHeight;

        /// <summary>
        /// Gets the first Skeleton the Kinect is tracking.
        /// </summary>
        public static Skeleton SkeletonA { get { return skeletonA; } }
        /// <summary>
        /// Gets the second Sekelton the Kinect is tracking.
        /// </summary>
        public static Skeleton SkeletonB { get { return skeletonB; } }
        /// <summary>
        /// Gets the KinectSensor class with the underlying functionality of the Kinect.
        /// </summary>
        public static KinectSensor Kinect { get { return kinect; } }

        private static Boolean started;

        /// <summary>
        /// Starts the Kinect. This should not be called from inside a KinectGame
        /// </summary>
        public static void Start()
        {
            if (started)
                throw new Exception("Kinect already started! Do not call Start() or Stop() from your game");

            if (KinectSensor.KinectSensors.Count == 0)
            {
                throw new Exception("No Kinect detected!");
            }

            kinect = KinectSensor.KinectSensors[0];
            if (kinect.Status != KinectStatus.Connected)
            {
                throw new Exception("Kinect not plugged in!");
            }
            kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinect.SkeletonStream.Enable();
            kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(onKinectSkeletonFrameReady);
            kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(onKinectVideoFrameReady);
            kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(onKinectDepthFrameReady);
            kinect.Start();
            kinect.ElevationAngle = 10;

            started = true;
        }

        private static void checkStarted()
        {
            if (!started) throw new Exception("Kinect not initialized. Do not call Kinect methods from your constructor - use the Initialize() method.");
        }

        private static void onKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            int trackingIdA = skeletonA == null ? -1 : skeletonA.TrackingId;
            int trackingIdB = skeletonB == null ? -1 : skeletonB.TrackingId;

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
                        if (skel.TrackingId == trackingIdA)
                        {
                            if (skeletonA != null)
                                skeletonB = skeletonA;
                            skeletonA = skel;
                        } else if (skel.TrackingId == trackingIdB) {
                            skeletonB = skel;
                        } else if (skeletonA == null) {
                            skeletonA = skel;
                        } else if (skeletonB == null) {
                            skeletonB = skel;
                        }
                    }
                }
            }
        }

        private static void onKinectVideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            //Debug.WriteLine(DateTime.Now.ToString());
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
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    if (depthData == null || depthData.Length != frame.PixelDataLength)
                    {
                        depthData = new short[frame.PixelDataLength];
                    }
                    frame.CopyPixelDataTo(depthData);
                    depthFrameWidth = frame.Width;
                    depthFrameHeight = frame.Height;
                }
            }
        }

        /// <summary>
        /// Gets the position of a Skeleton's Joint on the screen.
        /// </summary>
        /// <param name="joint">The joint</param>
        /// <param name="screenSize">The size of the screen.</param>
        /// <returns>The position on the screen of the Joint</returns>
        public static Microsoft.Xna.Framework.Vector2 GetJointPosOnScreen(Joint joint, Size screenSize)
        {
            return GetSkeletonPointPosOnScreen(joint.Position, screenSize);
        }

        /// <summary>
        /// Gets the depths in mm at the given pixel on screen, or -1 if invalid.
        /// </summary>
        /// <param name="x">The x coordinate on screen</param>
        /// <param name="y">The y coordinate on screen</param>
        /// <param name="screenSize">The size of the screen</param>
        /// <returns>The depth, in mm</returns>
        public static int GetDepthAtPixel(int x, int y, Size screenSize)
        {
            checkStarted();
            if (depthData == null) return -1;
            int dx = x * depthFrameWidth / screenSize.Width;
            int dy = y * depthFrameHeight / screenSize.Height;
            int index = dy * depthFrameWidth + dx;
            if (index >= 0 && index < depthData.Length)
            {
                return depthData[index] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the player at the given pixel on screen, or -1 if invalid.
        /// </summary>
        /// <param name="x">The x coordinate on screen</param>
        /// <param name="y">The y coordinate on screen</param>
        /// <param name="screenSize">The size of the screen</param>
        /// <returns>The player index</returns>
        public static int GetPlayerIndexAtPixel(int x, int y, Size screenSize)
        {
            checkStarted();
            if (depthData == null) return -1;
            int dx = x * depthFrameWidth / screenSize.Width;
            int dy = y * depthFrameHeight / screenSize.Height;
            int index = dy * depthFrameWidth + dx;
            if (index >= 0 && index < depthData.Length)
            {
                return depthData[index] & DepthImageFrame.PlayerIndexBitmask;
            }
            return -1;
        }

        /// <summary>
        /// Returns the depth in mm at the given SkeletonPoint, or -1 if the point is invalid.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>The depth, in mm</returns>
        public static int GetDepthAtPoint(SkeletonPoint point)
        {
            Size depthSize = new Size(depthFrameWidth, depthFrameHeight);
            Microsoft.Xna.Framework.Vector2 pos = GetSkeletonPointPosOnScreen(point, depthSize);
            int x = (int)pos.X, y = (int)pos.Y;
            return GetDepthAtPixel(x, y, depthSize);
        }

        /// <summary>
        /// Gets the position of a SkeletonPoint on the screen.
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="screenSize">The size of the screen.</param>
        /// <returns>The position on the screen of the point</returns>
        public static Microsoft.Xna.Framework.Vector2 GetSkeletonPointPosOnScreen(SkeletonPoint point, Size screenSize)
        {
            checkStarted();
            Kinect.Toolbox.Vector2 v = Tools.Convert(kinect, point);
            return new Microsoft.Xna.Framework.Vector2(v.X * screenSize.Width, v.Y * screenSize.Height);
        }

        /// <summary>
        /// Returns a Texture2D of the current video frame from the Kinect
        /// </summary>
        /// <param name="graphics">The GraphicsDeviceManager to use to create the Texture2D</param>
        /// <returns></returns>
        public static Texture2D GetScreen(GraphicsDeviceManager graphics)
        {
            checkStarted();
            if (colorFrameTexture != null) return colorFrameTexture;
            if (colorFrameData == null) return null;

            colorFrameTexture = new Texture2D(graphics.GraphicsDevice, colorFrameSize.Width, colorFrameSize.Height);
            colorFrameTexture.SetData<Byte>(colorFrameData);
            return colorFrameTexture;
        }

        public static Texture2D GetDepthMap(GraphicsDeviceManager graphics)
        {
            checkStarted();
            if (depthData == null) return null;

            Texture2D texture = new Texture2D(graphics.GraphicsDevice, depthFrameWidth, depthFrameHeight);
            byte[] bytes = ConvertDepthFrame(depthData, depthData.Length * 4);
            texture.SetData<byte>(bytes);

            return texture;
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        private static byte[] ConvertDepthFrame(short[] depthFrame, int depthFrame32Length)
        {
            int tooNearDepth = 0; //depthStream.TooNearDepth;
            int tooFarDepth = 0;// depthStream.TooFarDepth;
            int unknownDepth = 0;// depthStream.UnknownDepth;

            int[] IntensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
            int[] IntensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
            int[] IntensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

            int RedIndex = 2;
            int GreenIndex = 1;
            int BlueIndex = 0;
            int AlphaIndex = 3;

            byte[] depthFrame32 = new byte[depthFrame32Length];

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < depthFrame32.Length; i16++, i32 += 4)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));

                depthFrame32[i32 + AlphaIndex] = 125;

                if (player == 0 && realDepth == 0)
                {
                    // white 
                    depthFrame32[i32 + RedIndex] = 255;
                    depthFrame32[i32 + GreenIndex] = 255;
                    depthFrame32[i32 + BlueIndex] = 255;
                }
                else if (player == 0 && realDepth == tooFarDepth)
                {
                    // dark purple
                    depthFrame32[i32 + RedIndex] = 66;
                    depthFrame32[i32 + GreenIndex] = 0;
                    depthFrame32[i32 + BlueIndex] = 66;
                }
                else if (player == 0 && realDepth == unknownDepth)
                {
                    // dark brown
                    depthFrame32[i32 + RedIndex] = 66;
                    depthFrame32[i32 + GreenIndex] = 66;
                    depthFrame32[i32 + BlueIndex] = 33;
                }
                else
                {
                    int depth = depthData[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    Color c = ColorFromHSV((depth / 3 % 255), 1, 1);

                    if (player == 0) c.R = (byte)(c.R + (255 - c.R) / 2);
                    if (player == 1) c.G = (byte)(c.G + (255 - c.G) / 2);
                    if (player == 2) c.B = (byte)(c.B + (255 - c.B) / 2);

                    //if (player >= 0) depthFrame32[i32 + AlphaIndex] = 255;

                    depthFrame32[i32 + RedIndex] = c.R;
                    depthFrame32[i32 + GreenIndex] = c.G;
                    depthFrame32[i32 + BlueIndex] = c.B;

                    // tint the intensity by dividing by per-player values
                    //depthFrame32[i32 + RedIndex] = (byte)(intensity >> IntensityShiftByPlayerR[player]);
                    //depthFrame32[i32 + GreenIndex] = (byte)(intensity >> IntensityShiftByPlayerG[player]);
                    //depthFrame32[i32 + BlueIndex] = (byte)(intensity >> IntensityShiftByPlayerB[player]);
                }

            }

            return depthFrame32;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));


            if (hi == 0)
                return new Color(v, t, p, 255);
            else if (hi == 1)
                return new Color(q, v, p, 255);
            else if (hi == 2)
                return new Color( p, v, t, 255);
            else if (hi == 3)
                return new Color(p, q, v, 255);
            else if (hi == 4)
                return new Color(t, p, v, 255);
            else
                return new Color(v, p, q, 255);
        }



        /// <summary>
        /// Stops the Kinect. This should not be called from a KinectGame.
        /// </summary>
        public static void Stop()
        {
            checkStarted();
            kinect.Stop();
            started = false;
        }
    }
}
