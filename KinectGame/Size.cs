using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectExplorer
{
    /// <summary>
    /// Represents a integer size
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// The width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Creates a Size with the given width and height
        /// </summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        public Size(int width, int height) : this()
        {
            Width = width;
            Height = height;
        }
    }
}
