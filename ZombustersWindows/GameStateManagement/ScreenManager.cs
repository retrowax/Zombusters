#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if !WINDOWS_PHONE

#endif
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using System.Xml.Linq;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields

        private const string StateFilename = "ScreenManagerState.xml";

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        InputState input = new InputState();

        SpriteBatch spriteBatch;
        SpriteFont font;
#if WINDOWS_PHONE
        SpriteFont menuHeaderFont;
#endif
        Texture2D blankTexture;

        bool isInitialized;

        bool traceEnabled;

        #endregion

        #region Properties


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }


        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }

#if WINDOWS_PHONE
        public SpriteFont HeaderFont
        {
            get { return menuHeaderFont; }
        }
#endif


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;

            foreach (GameScreen screen in screens)
            {
                screen.Initialize();
            }                        

        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
#if WINDOWS_PHONE
            menuHeaderFont = Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
#endif
            blankTexture = Game.Content.Load<Texture2D>("blank");

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.  Unless the guide is up
                    if (!otherScreenHasFocus 
#if !WINDOWS_PHONE && !WINDOWS
                        && !Guide.IsVisible
#endif
                        )
                    {
                        screen.HandleInput(input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            //// Print debug trace?
            //if (traceEnabled)
            //    TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            //REVISAR!!
            //Trace.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if ((screen.ScreenState == ScreenState.Hidden) ||
                    (screen.HadFirstUpdate == false))
                    continue;

                screen.Draw(gameTime);
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (isInitialized)
            {
                screen.Initialize();
                screen.LoadContent();
            }

            screens.Add(screen);

            // update the TouchPanel to respond to gestures this screen is interested in
            TouchPanel.EnabledGestures = screen.EnabledGestures;

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (isInitialized)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);

            // if there is a screen still in the manager, update TouchPanel
            // to respond to gestures that screen is interested in.
            if (screens.Count > 0)
            {
                TouchPanel.EnabledGestures = screens[screens.Count - 1].EnabledGestures;
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(int alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             new Color(0, 0, 0, (byte)alpha));

            spriteBatch.End();
        }


        /// <summary>
        /// Informs the screen manager to serialize its state to disk.
        /// </summary>
        public void Deactivate()
        {
#if !WINDOWS_PHONE
            return;
#else
            // Open up isolated storage
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create an XML document to hold the list of screen types currently in the stack
                XDocument doc = new XDocument();
                XElement root = new XElement("ScreenManager");
                doc.Add(root);

                // Make a copy of the master screen list, to avoid confusion if
                // the process of deactivating one screen adds or removes others.
                screensToUpdate.Clear();
                foreach (GameScreen screen in screens)
                    screensToUpdate.Add(screen);

                // Iterate the screens to store in our XML file and deactivate them
                foreach (GameScreen screen in screensToUpdate)
                {
                    // Only add the screen to our XML if it is serializable
                    if (screen.IsSerializable)
                    {
                        // We store the screen's controlling player so we can rehydrate that value
                        string playerValue = screen.ControllingPlayer.HasValue
                            ? screen.ControllingPlayer.Value.ToString()
                            : "";

                        root.Add(new XElement(
                            "GameScreen",
                            new XAttribute("Type", screen.GetType().AssemblyQualifiedName),
                            new XAttribute("ControllingPlayer", playerValue)));
                    }

                    // Deactivate the screen regardless of whether we serialized it
                    screen.Deactivate();
                }

                // Save the document
                using (IsolatedStorageFileStream stream = storage.CreateFile(StateFilename))
                {
                    doc.Save(stream);
                }
            }
#endif
        }

        public bool Activate(bool instancePreserved)
        {
#if !WINDOWS_PHONE
            return false;
#else
            // If the game instance was preserved, the game wasn't dehydrated so our screens still exist.
            // We just need to activate them and we're ready to go.
            if (instancePreserved)
            {
                // Make a copy of the master screen list, to avoid confusion if
                // the process of activating one screen adds or removes others.
                screensToUpdate.Clear();

                foreach (GameScreen screen in screens)
                    screensToUpdate.Add(screen);

                foreach (GameScreen screen in screensToUpdate)
                    screen.Activate(true);
            }

            // Otherwise we need to refer to our saved file and reconstruct the screens that were present
            // when the game was deactivated.
            else
            {
                // Try to get the screen factory from the services, which is required to recreate the screens
                IScreenFactory screenFactory = Game.Services.GetService(typeof(IScreenFactory)) as IScreenFactory;
                if (screenFactory == null)
                {
                    throw new InvalidOperationException(
                        "Game.Services must contain an IScreenFactory in order to activate the ScreenManager.");
                }

                // Open up isolated storage
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Check for the file; if it doesn't exist we can't restore state
                    if (!storage.FileExists(StateFilename))
                        return false;

                    // Read the state file so we can build up our screens
                    using (IsolatedStorageFileStream stream = storage.OpenFile(StateFilename, FileMode.Open))
                    {
                        XDocument doc = XDocument.Load(stream);

                        // Iterate the document to recreate the screen stack
                        foreach (XElement screenElem in doc.Root.Elements("GameScreen"))
                        {
                            // Use the factory to create the screen
                            Type screenType = Type.GetType(screenElem.Attribute("Type").Value);
                            GameScreen screen = screenFactory.CreateScreen(screenType, (Zombusters.Game1)this.Game);

                            // Rehydrate the controlling player for the screen
                            PlayerIndex? controllingPlayer = screenElem.Attribute("ControllingPlayer").Value != ""
                                ? (PlayerIndex)Enum.Parse(typeof(PlayerIndex), screenElem.Attribute("ControllingPlayer").Value, true)
                                : (PlayerIndex?)null;
                            screen.ControllingPlayer = controllingPlayer;

                            // Add the screen to the screens list and activate the screen
                            //screen.ScreenManager = this;
                            //screen.ScreenManager.AddScreen(screen);

                            if (screen.GetType() != typeof(Zombusters.GamePlayScreen))
                            {
                                ((Zombusters.Game1)this.Game).screenManager.AddScreen(screen);
                            }
                            else
                            {
                                //((Zombusters.Game1)this.Game).Main = new Zombusters.Player(((Zombusters.Game1)this.Game).options, ((Zombusters.Game1)this.Game).audio);
                                //((Zombusters.Game1)this.Game).currentPlayers[0] = new Zombusters.Avatar();
                                ((Zombusters.Game1)this.Game).currentPlayers[0].Initialize(((Zombusters.Game1)this.Game).GraphicsDevice.Viewport);
                                ((Zombusters.Game1)this.Game).InitializeMain(PlayerIndex.One);
                                //((Zombusters.Game1)this.Game).currentPlayers[0].Player = ((Zombusters.Game1)this.Game).Main;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].Activate(((Zombusters.Game1)this.Game).Main);
                                ((Zombusters.Game1)this.Game).currentPlayers[0].Player.isReady = true;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].Player.levelsUnlocked = 1;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].character = 0;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].isReady = true;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].lives = 3;
                                ((Zombusters.Game1)this.Game).currentPlayers[0].status = Zombusters.ObjectStatus.Active;

                                List<int> ListPlayersAreGoingToPlay = new List<int>();
                                ListPlayersAreGoingToPlay.Add(0);
                                //((Zombusters.Game1)this.Game).BeginLocalGame(Zombusters.Subsystem_Managers.CLevel.Level.One, ListPlayersAreGoingToPlay);
                                ((Zombusters.Game1)this.Game).currentGameState = Zombusters.GameState.InGame;
                                ((Zombusters.Game1)this.Game).playScreen = new Zombusters.GamePlayScreen((Zombusters.Game1)this.Game, Zombusters.Subsystem_Managers.CLevel.Level.One, Zombusters.Subsystem_Managers.CSubLevel.SubLevel.One);
                                ((Zombusters.Game1)this.Game).screenManager.AddScreen(((Zombusters.Game1)this.Game).playScreen);
                                //((Zombusters.Game1)this.Game).Restart();
                                //((Zombusters.Game1)this.Game).GamePlayStatus = Zombusters.GameplayState.StartLevel;
                                //((Zombusters.Game1)this.Game).playScreen.StartNewLevel(true, false);
                                ((Zombusters.Game1)this.Game).playScreen.bGameOver = false;
                            }
                            //((Zombusters.Game1)this.Game).playScreen = new Zombusters.GamePlayScreen((Zombusters.Game1)this.Game, Zombusters.Subsystem_Managers.CLevel.Level.One, Zombusters.Subsystem_Managers.CSubLevel.SubLevel.One);
                            //screens.Add(screen);
                            //screen.Activate(true);

                            // update the TouchPanel to respond to gestures this screen is interested in
                            TouchPanel.EnabledGestures = screen.EnabledGestures;
                        }
                    }
                }
            }

            return true;
#endif
        }

        #endregion
    }
}
