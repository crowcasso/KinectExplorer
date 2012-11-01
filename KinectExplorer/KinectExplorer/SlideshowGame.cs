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

using System.IO;
using System.Diagnostics;

namespace KinectExplorer
{
    public class SlideshowGame : KinectGame
    {
        SpriteBatch spriteBatch;
        List<Texture2D> slides;
        TimeSpan SLIDE_LENGTH = new TimeSpan(0, 0, 30);
        TimeSpan currentSlideRuntime;
        int slideIndex = -1;
        TimeSpan TRANS_LENGTH = new TimeSpan(0, 0, 3);
        TimeSpan currentTrans;

        public SlideshowGame(GameParameters gParams) : base(gParams) { }

        public override GameConfig GetConfig()
        {
           return new GameConfig("Announcements", "Thomas", "The latest announcements.", true, 30);
        }

        public override void Initialize()
        {
            nextSlide(false);
        }

        public override void LoadContent(ContentLoader content)
        {
            slides = new List<Texture2D>();
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            String[] files = Directory.GetFiles("Slideshow");
            foreach (String file in files)
            {
                try
                {
                    Stream stream = new FileStream(file, FileMode.Open);
                    slides.Add(Texture2D.FromStream(graphics.GraphicsDevice, stream));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            if (slides.Count == 0)
            {
                slides.Add(new Texture2D(graphics.GraphicsDevice, 1, 1));
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            currentSlideRuntime = currentSlideRuntime.Add(gameTime.ElapsedGameTime);
            if (currentSlideRuntime.CompareTo(SLIDE_LENGTH) > 0) nextSlide(true);
            currentTrans = currentTrans.Add(gameTime.ElapsedGameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            Color color = Color.White;
            if (currentTrans.CompareTo(TRANS_LENGTH) < 0)
            {
                int lastSlide = (slideIndex + slides.Count - 1) % slides.Count;
                int alpha = (int)(currentTrans.TotalMilliseconds * 255 / TRANS_LENGTH.TotalMilliseconds);
                spriteBatch.Draw(slides[lastSlide], new Rectangle(0, 0, Resolution.Width, Resolution.Height), new Color(255, 255, 255, 255));
                color = new Color(255, 255, 255, alpha);
            }
            spriteBatch.Draw(slides[slideIndex], new Rectangle(0, 0, Resolution.Width, Resolution.Height), color);
            spriteBatch.End();
        }

        private void nextSlide(bool trans)
        {
            slideIndex = (slideIndex + 1) % slides.Count;
            currentSlideRuntime = new TimeSpan();
            if (trans)
                currentTrans = new TimeSpan();
            else
                currentTrans = new TimeSpan(TRANS_LENGTH.Ticks);
        }
    }
}
