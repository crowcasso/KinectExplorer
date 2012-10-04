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
    /// A class for helping with text formatting
    /// </summary>
    public static class TextUtils
    {
        /// <summary>
        /// Returns a String cut off to a given max width using the given font
        /// </summary>
        /// <param name="font">The font to use when measuring</param>
        /// <param name="text">The text to cut</param>
        /// <param name="maxWidth">The max width for the text</param>
        /// <returns>The cutoff string</returns>
        public static String CutTextToWidth(SpriteFont font, String text, int maxWidth)
        {
            float textWidth = font.MeasureString(text).X;
            while (text.Length > 0 && textWidth > maxWidth)
            {
                text = text.Substring(0, text.Length - 1);
                textWidth = font.MeasureString(text).X;
            }
            return text;
        }

        public static String FitTextToWidth(SpriteFont font, String text, int width)
        {
            String line = String.Empty;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (font.MeasureString(line + word).Length() > width)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }

        public static Rectangle DrawTextInRect(SpriteBatch spriteBatch, SpriteFont font, String text, Rectangle rect, Color color, bool centered)
        {
            text = FitTextToWidth(font, text, rect.Width);
            String[] lines = text.Split('\n');
            float maxWidth = 0;
            float y = rect.Y;
            foreach (String line in lines)
            {
                Vector2 size = font.MeasureString(line);
                maxWidth = Math.Max(maxWidth, size.X);
                if (centered)
                {
                    y += DrawCenteredText(spriteBatch, font, line, rect.Left, rect.Right, (int)y, color);
                }
                else
                {
                    y += DrawCenteredText(spriteBatch, font, line, rect.Left, (int)(size.X + 1), (int)y, color);
                }

            }
            return new Rectangle(rect.X, rect.Y, centered ? rect.Width : (int)maxWidth, (int)y);

        }

        public static float DrawCenteredText(SpriteBatch spriteBatch, SpriteFont font, String text, int startX, int endX, int y, Color color)
        {
            text = text.Trim();
            Vector2 size = font.MeasureString(text);
            spriteBatch.DrawString(font, text, new Vector2((startX + endX - size.X) / 2, y), color);
            return size.Y;
        }
    }
}
