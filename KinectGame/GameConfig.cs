using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectExplorer
{
    /// <summary>
    /// A configuration class that gives information on a KinectGame
    /// </summary>
    public class GameConfig
    {
        /// <summary>
        /// The name of this KinectGame
        /// </summary>
        public readonly String Name;
        /// <summary>
        /// The author of this KinectGame
        /// </summary>
        public readonly String Author;
        /// <summary>
        /// The description of this KinectGame
        /// </summary>
        public readonly String Description;
        /// <summary>
        /// Indicates whether this game is "passive," and can run without a user,
        /// meaning it can be used like a screensaver
        /// </summary>
        public bool IsPassive;
        public int RequestedTime;

        /// <summary>
        /// Creates a GameConfig to describe a KinectGame
        /// </summary>
        /// <param name="name">The game's name</param>
        /// <param name="author">The author(s) name(s)</param>
        /// <param name="description">A brief description of the game</param>
        public GameConfig(String name, String author, String description, bool isPassive, int requestedTime)
        {
            this.Name = name;
            this.Author = author;
            this.Description = description;
            this.IsPassive = isPassive;
            this.RequestedTime = requestedTime;
        }
    }
}
