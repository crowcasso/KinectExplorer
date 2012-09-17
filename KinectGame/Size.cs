using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectExplorer
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Size(int width, int height) : this()
        {
            Width = width;
            Height = height;
        }
    }
}
