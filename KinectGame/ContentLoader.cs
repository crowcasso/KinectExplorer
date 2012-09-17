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
    public class ContentLoader
    {
        private ContentManager content;
        private String contentRoot;

        public ContentLoader(ContentManager content, String contentRoot)
        {
            this.content = content;
            this.contentRoot = contentRoot;
        }

        public T Load<T>(String path)
        {
            return content.Load<T>(contentRoot + path);
        }
    }
}
