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
    /// A wrapper class for a ContentManager. The methods work the same way,
    /// but this allows load paths to be modfied for each running KinectGame,
    /// allowing them to have separate content folders.
    /// </summary>
    public class ContentLoader
    {
        private ContentManager content;
        private String contentRoot;

        /// <summary>
        /// Gets the ContentManager that this is wrapping. Directly accessing this is not recommended for
        /// games that will be exported becasue it bypasses safe content loading.
        /// </summary>
        public ContentManager ContentManager { get { return content; } }

        /// <summary>
        /// Creates a ContentLoader. KinectGames should not use this constructor.
        /// </summary>
        /// <param name="content">The ContentManager to wrap</param>
        /// <param name="contentRoot">The content root for this game - should be "" by default.</param>
        public ContentLoader(ContentManager content, String contentRoot)
        {
            this.content = content;
            this.contentRoot = contentRoot;
        }

        /// <summary>
        /// Loads content to be used in a KinectGame
        /// </summary>
        /// <typeparam name="T">The type of content to load, such as Texture2D</typeparam>
        /// <param name="path">The path to the resource in your main content directory - do not add a file extension</param>
        /// <returns>The loaded resource</returns>
        public T Load<T>(String path)
        {
            return content.Load<T>(contentRoot + path);
        }
    }
}
