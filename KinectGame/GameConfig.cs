using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectExplorer
{
    public class GameConfig
    {
        public readonly String Name, Author, Description;
        public bool IsPassive;

        public GameConfig(String name, String author, String description)
        {
            this.Name = name;
            this.Author = author;
            this.Description = description;
        }
    }
}
