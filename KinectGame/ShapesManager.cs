using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

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
    /// A convenience class for creating shape Textures
    /// </summary>
    public static class ShapesManager
    {
        /// <summary>
        /// Converts the given System.Drawing.Image into a Texture2D.
        /// </summary>
        /// <param name="image">The image to be converted</param>
        /// <param name="graphicsDevice">The GraphicsDevice with which the image will be displayed</param>
        /// <param name="texture">A texture to reuse - can be null</param>
        /// <returns>A Texture2D of the image</returns>
        public static Texture2D Image2Texture(System.Drawing.Image image, GraphicsDevice graphicsDevice, Texture2D texture)
        {
            GraphicsDevice graphics = graphicsDevice;

            if (graphics == null) return null;

            if (image == null)
            {
                return null;
            }

            if (texture == null || texture.IsDisposed ||
                texture.Width != image.Width ||
                texture.Height != image.Height ||
                texture.Format != SurfaceFormat.Color)
            {
                if (texture != null && !texture.IsDisposed)
                {
                    texture.Dispose();
                }

                texture = new Texture2D(graphics, image.Width, image.Height, false, SurfaceFormat.Color);
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    if (graphics.Textures[i] == texture)
                    {
                        graphics.Textures[i] = null;
                        break;
                    }
                }
            }

            //Memory stream to store the bitmap data.
            MemoryStream ms = new MemoryStream();

            //Save to that memory stream.
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            //Go to the beginning of the memory stream.
            ms.Seek(0, SeekOrigin.Begin);

            //Fill the texture.
            texture = Texture2D.FromStream(graphics, ms, image.Width, image.Height, false);

            //Close the stream.
            ms.Close();
            ms = null;

            return texture;
        }

        private static System.Drawing.Color convertColor(Microsoft.Xna.Framework.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static Pen getPen(Microsoft.Xna.Framework.Color color)
        {
            return new Pen(convertColor(color));
        }

        private static Brush getBrush(Microsoft.Xna.Framework.Color color)
        {
            return new SolidBrush(convertColor(color));
        }

        /// <summary>
        /// Creates an ellipse (oval) shape texture of the given with and height
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use when creating the Texture2D</param>
        /// <param name="width">The width of the ellipse</param>
        /// <param name="height">The height of the ellipse</param>
        /// <param name="color">The color of the ellipse</param>
        /// <param name="fill">Whether or not to fill in the ellipse</param>
        /// <returns>The created Texture2D</returns>
        public static Texture2D CreateEllipseTexture(GraphicsDevice graphicsDevice, int width, int height, Microsoft.Xna.Framework.Color color, bool fill)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            if (fill)
            {
                g.FillEllipse(getBrush(color), 0, 0, width, height);
            }
            else
            {
                g.DrawEllipse(getPen(color), 0, 0, width, height);
            }
            return Image2Texture(bitmap, graphicsDevice, null);
        }
    }
}
