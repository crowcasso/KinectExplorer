using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace KinectExplorer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            List<Type> games = new List<Type>();
            List<String> gameDirs = new List<string>();

            Directory.CreateDirectory("Games");
            String[] dirs = Directory.GetDirectories("Games");
            foreach(String gameDir in dirs) 
            {
                foreach (String gameFile in Directory.GetFiles(gameDir))
                {
                    if (gameFile.ToLower().EndsWith(".exe") || gameFile.ToLower().EndsWith(".dll"))
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom(gameFile);
                            Type[] types = assembly.GetTypes();
                            foreach (Type t in types)
                            {
                                if (typeof(KinectGame).IsAssignableFrom(t))
                                {
                                    //KinectGame kinectGame = (KinectGame)Activator.CreateInstance(t);
                                    //games.Add(kinectGame);
                                    games.Add(t);
                                    Debug.WriteLine("Loading: " + t.Name);
                                    gameDirs.Add(gameDir);
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    }
                }
            }

            GameMenu menu = new GameMenu(games.ToArray(), gameDirs.ToArray());
            menu.Run();
        }
    }
#endif
}

