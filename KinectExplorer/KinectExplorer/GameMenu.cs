using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using System.Diagnostics;
using System.IO;

namespace KinectExplorer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameMenu : KinectGameHost
    {
        private Type[] games;
        private GameConfig[] gameConfigs;
        private String[] gameDirs;

        private Rectangle[] gameRects;
        private int gameRectOffset;

        private SpriteBatch spriteBatch;
        private SpriteFont titleFont, authorFont, textFont, hugeFont, directionsFont;

        private Texture2D cursorTexture;
        private float cursorRotation;
        private Vector2 cursorPosition;
        private bool cursorVisible;
        private float cursorOpacity;

        private Random random;

        private List<SkeletonPoint> cursorTrail = new List<SkeletonPoint>();
        private const int MAX_TRAIL = 10;

        private int selectedGame = -1;
        private float selectionProgress;
        
        private TimeSpan QUIT_TIME = new TimeSpan(0, 0, 2);
        private TimeSpan tryingQuit = new TimeSpan();

        private TimeSpan SCREEN_SAVER_TIMEOUT = new TimeSpan(0, 0, 20);
        private TimeSpan noSkeletons = new TimeSpan();

        private TimeSpan BREAK_SCREEN_SAVER = new TimeSpan(0, 0, 3);
        private TimeSpan handRaised = new TimeSpan();

        private const int SCROLL_AREA_X = 1440;

        private Texture2D borderTexture, titleTexture;
        
        private bool gamePaused;

        private Skeleton PrimarySkeleton
        {
            get
            {
                return KinectManager.SkeletonA == null ? KinectManager.SkeletonB : KinectManager.SkeletonA;
            }
        }

        protected override Size GetResolution() 
        { 
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen; 
            //return new Size(screen.Bounds.Width, screen.Bounds.Height); 
            return new Size(1920, 1080); 
        }

        protected override string GetContentRoot(Type t)
        {
            int index = Array.IndexOf(games, t);
            if (index >= 0) return gameDirs[index] + "\\";
            return "";
        }

        protected override Type GetKinnectGame()
        {
            return null;
        }

        private void addLocalGame(Type game)
        {
            Array.Resize<Type>(ref games, games.Length + 1);
            games[games.Length - 1] = game;

            Array.Resize<String>(ref gameDirs, gameDirs.Length + 1);
            gameDirs[gameDirs.Length - 1] = "Content";
        }

        public GameMenu(Type[] games, String[] gameDirs) : base()
        {


            this.games = games;
            this.gameDirs = gameDirs;
            for (int i = 0; i < 1; i++)
            {
                addLocalGame(typeof(TestGame));
            }
            addLocalGame(typeof(DepthGame));
            addLocalGame(typeof(BrowserGame));
            addLocalGame(typeof(SlideshowGame));
            games = this.games;
            gameDirs = this.gameDirs;

            gameConfigs = new GameConfig[games.Length];
            for (int i = 0; i < games.Length; i++)
            {
                KinectGame game = KinectGame.Create(games[i], new GameParameters(resolution, graphics, Window));
                gameConfigs[i] = game.GetConfig();
            }

            gameRects = new Rectangle[games.Length];
            updateGameRects(0);

            this.random = new Random();
        }

        protected override void Initialize()
        {
            base.Initialize();

            graphics.PreferredBackBufferHeight = resolution.Height;
            graphics.PreferredBackBufferWidth = resolution.Width;

            System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Bounds = new System.Drawing.Rectangle(0, 0, resolution.Width, resolution.Height);
            
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            titleFont = Content.Load<SpriteFont>("TitleFont");
            authorFont = Content.Load<SpriteFont>("AuthorFont");
            textFont = Content.Load<SpriteFont>("TextFont");
            hugeFont = Content.Load<SpriteFont>("HugeFont");
            directionsFont = Content.Load<SpriteFont>("DirectionsFont");
            cursorTexture = Content.Load<Texture2D>("cursor");
            borderTexture = Content.Load<Texture2D>("border");
            titleTexture = Content.Load<Texture2D>("title");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (PrimarySkeleton == null)
            {
                noSkeletons = noSkeletons.Add(gameTime.ElapsedGameTime);
                if (noSkeletons.CompareTo(SCREEN_SAVER_TIMEOUT) > 0)
                {
                    if (runningGame == null || !runningGame.GetConfig().IsPassive)
                    {
                        EndGame();

                        // randomly choose a passive game
                        bool picked = false;
                        while (!picked)
                        {
                            int which = random.Next(games.Length);
                            if (gameConfigs[which].IsPassive)
                            {
                                StartGame(games[which]);
                                picked = true;
                            }
                        }
                    }
                }
            }
            else
            {
                noSkeletons = noSkeletons.Subtract(gameTime.ElapsedGameTime);
                if (noSkeletons.TotalMilliseconds < 0) noSkeletons = new TimeSpan();
            }
        }

        protected override void UpdateGame(GameTime gameTime)
        {
            if (runningGame.Finished)
            {
                EndGame();
                return;
            }
            if (!gamePaused)
            {
                runningGame.Update(gameTime);
            }

            if (handsOnHead())
            {
                tryingQuit = tryingQuit.Add(gameTime.ElapsedGameTime);
                if (tryingQuit.CompareTo(new TimeSpan(0, 0, 1)) > 0)
                {
                    gamePaused = true;
                }
                if (tryingQuit.CompareTo(QUIT_TIME) > 0)
                {
                    EndGame();
                }
            }
            else
            {
                tryingQuit = new TimeSpan();
                gamePaused = false;
            }

            /* JOEL_CUT
            if (runningGame != null && runningGame.GetConfig().IsPassive && isHandRaised())
            {
                noSkeletons = new TimeSpan();
                handRaised = handRaised.Add(gameTime.ElapsedGameTime);
                if (handRaised.CompareTo(BREAK_SCREEN_SAVER) > 0)
                {
                    handRaised = new TimeSpan();
                    EndGame();
                }
            }
            */
        }

        protected override void UpdateHost(GameTime gameTime)
        {
            cursorVisible = false;
            if (PrimarySkeleton != null)
            {
                if (PrimarySkeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.Tracked)
                {
                    return;
                }

                cursorVisible = true;

                float friction = 0.85f;
                float depth = PrimarySkeleton.Joints[JointType.HandRight].Position.Z;
                Vector2 rightHandPos = KinectManager.GetJointPosOnScreen(PrimarySkeleton.Joints[JointType.HandRight], resolution);
                Vector2 rightElbowPos = KinectManager.GetJointPosOnScreen(PrimarySkeleton.Joints[JointType.ElbowRight], resolution);
                
                float nCursorRotation = (float)(Math.Atan2(rightHandPos.Y - rightElbowPos.Y, rightHandPos.X - rightElbowPos.X) + Math.PI / 2);
                if (nCursorRotation - cursorRotation > Math.PI)
                    nCursorRotation -= (float)Math.PI * 2;
                else if (nCursorRotation - cursorRotation < -Math.PI)
                    nCursorRotation += (float)Math.PI * 2;
                cursorRotation = lerp(cursorRotation, nCursorRotation, friction) % (float)(Math.PI * 2);

                float factor = depth;
                rightHandPos.X = (rightHandPos.X - resolution.Width / 2) * factor + resolution.Width / 2;
                rightHandPos.Y = (rightHandPos.Y - resolution.Height / 2) * factor + resolution.Height / 2;
                cursorPosition.X = lerp(cursorPosition.X, rightHandPos.X, friction);
                cursorPosition.Y = lerp(cursorPosition.Y, rightHandPos.Y, friction);


                cursorTrail.Add(PrimarySkeleton.Joints[JointType.HandRight].Position);
                while (cursorTrail.Count > MAX_TRAIL) cursorTrail.RemoveAt(0);
                if (cursorTrail.Count == MAX_TRAIL)
                {
                    float swipe = cursorTrail[cursorTrail.Count - 1].Y - cursorTrail[0].Y;
                    float swipeThresh = 0.5f;
                    if (swipe > swipeThresh)
                    {
                        swipeUp();
                    }
                    else if (swipe < -swipeThresh)
                    {
                        swipeDown();
                    }
                }


                Point hand = new Point((int)cursorPosition.X, (int)cursorPosition.Y);
                int selection = -1;
                for (int i = 0; i < gameRects.Length; i++)
                {
                    if (gameRects[i].Contains(hand))
                    {
                        selection = i;
                    }
                }
                if (selection != selectedGame) this.selectionProgress = 0;
                selectedGame = selection;
                if (selectedGame >= 0)
                {
                    Point center = gameRects[selectedGame].Center;
                    int dx = center.X - hand.X;
                    int dy = center.Y - hand.Y;
                    //float dis = (float)Math.Pow(dx * dx + dy * dy, 0.75) / gameRects[selectedGame].Width;
                    float dis = (float)Math.Pow(dx * dx + dy * dy, 0.5f);

                    //selectionProgress += 0.006f / Math.Max(0.3f, dis);
                    if (dis > gameRects[selectedGame].Width / 3.5)
                        selectionProgress += 0.002f;
                    else
                        selectionProgress += 0.004f;
                }

                if (selectionProgress >= 1)
                {
                    selectionProgress = 1;
                    startGame(selectedGame);
                    return;
                }

            }
            else
            {
                selectionProgress = -1;
                cursorTrail.Clear();
            }

            updateGameRects(0.9f);
        }

        protected override void DrawGame(GameTime gameTime)
        {
            base.DrawGame(gameTime);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            //spriteBatch.DrawString(titleFont, runningGame.GetConfig().Name, new Vector2(10, 10), new Color(255, 255, 255, 150));

            if (gamePaused)
            {
                //spriteBatch.Draw(borderTexture, new Rectangle(0, 0, resolution.Width, resolution.Height), new Color(100, 100, 100, 150));
            }


            if (runningGame.GetConfig().IsPassive)
            {
                int interval = 30000;
                int duration = 5000;

                int border = 300;
                Rectangle rect = new Rectangle(border, border, resolution.Width - border * 2, resolution.Height - border * 2);

                double alphaMult = Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * 2 * Math.PI / interval);
                double startAlpha = Math.Sin(Math.PI * 2 * (0.25 - (double)duration / interval / 2));
                double m = 1 / (1 - startAlpha);
                alphaMult = m * (alphaMult - startAlpha);
                alphaMult = Math.Max(alphaMult, 0);

                spriteBatch.Draw(borderTexture, rect, new Color(0, 0, 0, (int)(200 * alphaMult)));
                TextUtils.DrawTextInRect(spriteBatch, hugeFont,
                    "To interact, stand a few feet back and hold up your right hand.",
                    rect, new Color(255, 255, 255, (int)(255 * alphaMult)), true);
            }
            spriteBatch.End();
        }

        protected override void DrawHost(GameTime gameTime)
        {
            base.DrawHost(gameTime);
            graphics.GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            Texture2D screen = KinectManager.GetScreen(graphics);
            if (screen != null)
            {
                spriteBatch.Draw(screen, new Rectangle(0, 0, resolution.Width, resolution.Height), new Color(255, 255, 255, 255));
            }

            for (int i = 0; i < games.Length; i++)
            {
                drawGameRect(i, spriteBatch);
            }

            /* JOEL_CUT
            int border = 25;
            Rectangle rect = new Rectangle(border, 10, resolution.Width - border * 2, 90);
            spriteBatch.Draw(titleTexture, rect, new Color(75, 0, 150));
            rect.Offset(0, 15);
            String message = "To play an app, hover over its center. To quit it, put your hands on your head.";
            TextUtils.DrawTextInRect(spriteBatch, directionsFont, message, rect, Color.White, true);
            */

            if (PrimarySkeleton != null)
            {
                if (cursorVisible)
                {
                    cursorOpacity = 0.1f + 0.9f * cursorOpacity;
                }
                else
                {
                    cursorOpacity = 0.9f * cursorOpacity;
                }
                int width = 50;
                int height = 50 * cursorTexture.Height / cursorTexture.Width;
                Color color = Color.White;
                color.A = (byte)(255 * cursorOpacity);
                spriteBatch.Draw(cursorTexture, new Rectangle((int)cursorPosition.X, (int)cursorPosition.Y, width, height), null, color, cursorRotation, new Vector2(width / 2, height / 2), SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        private bool isHandRaised()
        {
            if (PrimarySkeleton == null) return false;
            Joint rightHand = PrimarySkeleton.Joints[JointType.HandRight];
            if (rightHand.TrackingState != JointTrackingState.Tracked) return false;

            return PrimarySkeleton.Joints[JointType.HandRight].Position.Y - 
                PrimarySkeleton.Joints[JointType.ElbowRight].Position.Y > 0.1;
        }

        private bool handsOnHead()
        {
            Skeleton sk = PrimarySkeleton;
            if (sk == null) return false;
            SkeletonPoint rightHand = sk.Joints[JointType.HandRight].Position;
            SkeletonPoint leftHand = sk.Joints[JointType.HandLeft].Position;
            SkeletonPoint head = sk.Joints[JointType.Head].Position;
            float reqDis = 0.3f;
            return distance(rightHand, head) < reqDis && distance(leftHand, head) < reqDis;
        }

        private float distance(SkeletonPoint p1, SkeletonPoint p2)
        {
            float dx = (p1.X - p2.X);
            float dy = (p1.Y - p2.Y);
            float dz = (p1.Z - p2.Z);
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private void swipeUp()
        {
            Debug.WriteLine("swipeUp");
            cursorTrail.Clear();
            if ((gameRects.Length + 1) > 2 + gameRectOffset)
                gameRectOffset += 1;
        }

        private void swipeDown()
        {
            Debug.WriteLine("swipeDown");
            cursorTrail.Clear();
            if (gameRectOffset > 0)
                gameRectOffset -= 1;
        }

        /* JOEL_CUT
        private void swipeLeft()
        {
            cursorTrail.Clear();
            for (int i = 0; i < 2; i++)
            {
                if ((gameRects.Length + 1) / 2 > 3 + gameRectOffset)
                    gameRectOffset += 1;
            }
        }

        private void swipeRight()
        {
            cursorTrail.Clear();
            for (int i = 0; i < 2; i++)
            {
                if (gameRectOffset > 0)
                    gameRectOffset -= 1;
            }
        }
        */

        private void updateGameRects(float friction)
        {
            int offset = 12;
            int width = (resolution.Width - 1440 - (offset * 2));
            int height = width;

            Debug.WriteLine("gameRectOffset = " + gameRectOffset);

            for (int i = 0; i < gameRects.Length; i++)
            {
                int offI = i - gameRectOffset;

                Rectangle rect = new Rectangle(0, 0, width, height);
                rect.Offset(new Point(SCROLL_AREA_X + offset, offI * (height + offset)));

                gameRects[i] = new Rectangle((int)Math.Round(lerp(gameRects[i].X, rect.X, friction)), (int)Math.Round(lerp(gameRects[i].Y, rect.Y, friction)), rect.Width, rect.Height);
            }
        }

        private float lerp(float x0, float x1, float friction)
        {
            return x0 * friction + x1 * (1 - friction);
        }

        private void drawGameRect(int index, SpriteBatch spriteBatch)
        {
            Color color = new Color(255, 255, 255, 250);
            if (selectedGame >= 0 && selectedGame != index)
            {
                double thresh = 0.10f;
                if (selectionProgress > thresh)
                {
                    color = new Color(255, 255, 255, (int)((1 - (selectionProgress - thresh) / (1 - thresh)) * 255));
                }
            }

            Rectangle rect = gameRects[index];
            spriteBatch.Draw(borderTexture, rect, color);

            String name = gameConfigs[index].Name;
            String author = "by " + gameConfigs[index].Author;
            String description = gameConfigs[index].Description;

            float y = rect.Y + 25;

            name = TextUtils.FitTextToWidth(titleFont, name, rect.Width - 40);
            String[] nameLines = name.Split('\n');
            for (int i = 0; i < 2 && i < nameLines.Length; i++)
            {
                y += TextUtils.DrawCenteredText(spriteBatch, titleFont, nameLines[i], rect.Left, rect.Right, (int)y, Color.White);
            }

            author = TextUtils.CutTextToWidth(authorFont, author, rect.Width - 40);
            y += TextUtils.DrawCenteredText(spriteBatch, authorFont, author, rect.Left, rect.Right, (int)y, Color.Orange);

            y += 10;
            description = TextUtils.FitTextToWidth(textFont, description, rect.Width - 40);
            String[] descLines = description.Split('\n');
            foreach (String line in descLines)
            {
                if (y + textFont.MeasureString(line).Y > rect.Bottom) break;
                y += TextUtils.DrawCenteredText(spriteBatch, textFont, line, rect.Left, rect.Right, (int)y, Color.LightBlue);
            }
        }

        private void startGame(int index)
        {
            if (index < 0 || index >= games.Length) return;

            cursorTrail.Clear();
            selectedGame = -1;
            gamePaused = false;
            StartGame(games[index]);
        }

        protected override void EndGame()
        {
            tryingQuit = new TimeSpan();
            base.EndGame();
        }
    }
}
