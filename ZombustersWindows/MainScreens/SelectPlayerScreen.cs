using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class SelectPlayerScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnStart;
        Texture2D btnLT, btnRT;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar
        Texture2D arrow;
        Texture2D myGroupBKG;
        Texture2D HTPHeaderImage;
        Texture2D kbUp;
        Texture2D kbDown;

#if WINDOWS_PHONE
        Texture2D submit_button;
        Texture2D goBackButton;
        int levelsUnlocked = 0;
#endif

        List<Texture2D> pAvatar, pAvatarPixelated, cardBkg;
        List<String> pAvatarName;

        NeutralInput playerOneInput;
        NeutralInput playerTwoInput;
        NeutralInput playerThreeInput;
        NeutralInput playerFourInput;

        //int levelSelected;

        SpriteFont DigitBigFont, DigitLowFont;
        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        private MenuComponent menu;
        Boolean isMatchmaking;
        Boolean canStartGame; //If all the players are ready we can start playing the game (and show the button START)
        Boolean IsMMandHost;
        public byte levelSelected;

        public SelectPlayerScreen(Boolean ismatchmaking)
        {
            this.isMatchmaking = ismatchmaking;
        }

        public override void Initialize()
        {
            byte i;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

            titleBounds = new Rectangle(0, 0, 1280, 720);

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);

            DigitBigFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\DigitBig");
            DigitLowFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\DigitLow");
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");


            HTPHeaderImage = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/lobby_header_image");

            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);

            //Initialize Main Menu
            menu.Initialize();
            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            pAvatar = new List<Texture2D>(4);
            pAvatarPixelated = new List<Texture2D>(4);
            cardBkg = new List<Texture2D>(4);
            for (i = 0; i < 4; i++)
            {
                ((MyGame)this.ScreenManager.Game).currentPlayers[i].character = 0;
            }
            pAvatarName = new List<string>(4);
            pAvatarName.Add("Tracy");
            pAvatarName.Add("Charles");
            pAvatarName.Add("Ryan");
            pAvatarName.Add("Peter");

            levelSelected = 1;
            canStartGame = false;
            IsMMandHost = false;

            ((MyGame)this.ScreenManager.Game).currentPlayers[0].Player.loadSavedGame();

            // Nos aseguramos que no se queda marcado el flag de selecci�n de personaje
            foreach (Avatar player in ((MyGame)this.ScreenManager.Game).currentPlayers)
            {
                if (((MyGame)this.ScreenManager.Game).currentPlayers[0].Equals(player))
                {
                    player.isReady = false;
                    player.status = ObjectStatus.Active;
                }
                else
                {
                    player.isReady = false;
                    player.status = ObjectStatus.Inactive;
                }
            }

            base.Initialize();

            this.isBackgroundOn = true;
        }

        public override void LoadContent()
        {
            byte i;

            // Key "Up"
            kbUp = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_up");

            // Key "Down"
            kbDown = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_down");

            //Button "A"
            btnA = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Button "Select"
            btnStart = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerStart");

            // Button Left Trigger
            btnLT = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerLeftTrigger");

            // Button Right Trigger
            btnRT = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerRightTrigger");

            //Linea blanca separatoria
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");

            //Arrow
            arrow = this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/arrowLeft");

            //Fondo negro fade menu
            myGroupBKG = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/mygroup_bkg");

            for (i=1; i <= 4; i++)
            {
                pAvatar.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar"));
                pAvatarPixelated.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar_pixel"));
            }

            cardBkg.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_bkg"));
            cardBkg.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_border"));
            cardBkg.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_gradient"));
            cardBkg.Add(this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_ready"));

            base.LoadContent();
        }

        void toggleReady(int PlayerIndex)
        {
            if (((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady == true)
            {
                ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady = false;
            }
            else
            {
                ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady = true;
            }

            for (int i = 0; i < ((MyGame)this.ScreenManager.Game).currentPlayers.Length; i++)
            {
                if (i != PlayerIndex)
                {
                    if (((MyGame)this.ScreenManager.Game).currentPlayers[i].character == ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character && ((MyGame)this.ScreenManager.Game).currentPlayers[i].isReady == true)
                    {
                        ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady = false;
                    }
                }
            }
        }

        void changeCharacterSelected(int PlayerIndex, Boolean directionToRight)
        {
            if (directionToRight == true)
            {
                if (((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady == false)
                {
                    ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character += 1;
                    if (((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character > 3)
                    {
                        ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character = 0;
                    }
                }
            }
            else
            {
                if (((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].isReady == false)
                {
                    ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character -= 1;
                    if (((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character < 0 || ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character > 3)
                    {
                        ((MyGame)this.ScreenManager.Game).currentPlayers[PlayerIndex].character = 3;
                    }
                }
            }
        }

        public override void HandleInput(InputState input)
        {
            if (((MyGame)this.ScreenManager.Game).currentPlayers[0].Player.Controller == PlayerIndex.One || GamePad.GetState(PlayerIndex.One).IsConnected)
                playerOneInput = ProcessPlayer(((MyGame)this.ScreenManager.Game).currentPlayers[0], input);
            if (((MyGame)this.ScreenManager.Game).currentPlayers[1].Player.Controller == PlayerIndex.Two || GamePad.GetState(PlayerIndex.Two).IsConnected)
                playerTwoInput = ProcessPlayer(((MyGame)this.ScreenManager.Game).currentPlayers[1], input);
            if (((MyGame)this.ScreenManager.Game).currentPlayers[2].Player.Controller == PlayerIndex.Three || GamePad.GetState(PlayerIndex.Three).IsConnected)
                playerThreeInput = ProcessPlayer(((MyGame)this.ScreenManager.Game).currentPlayers[2], input);
            if (((MyGame)this.ScreenManager.Game).currentPlayers[3].Player.Controller == PlayerIndex.Four || GamePad.GetState(PlayerIndex.Four).IsConnected)
                playerFourInput = ProcessPlayer(((MyGame)this.ScreenManager.Game).currentPlayers[3], input);

            base.HandleInput(input);
        }


        private static NeutralInput ProcessPlayer(Avatar player, InputState input)
        {
            NeutralInput state = new NeutralInput();
            //state.Fire = false;
            state.Fire = Vector2.Zero;
            Vector2 stickLeft = Vector2.Zero;
            Vector2 stickRight = Vector2.Zero;
            GamePadState gpState = input.GetCurrentGamePadStates()[(int)player.Player.Controller];

            if (player.IsPlaying)
            {
                if (input.IsNewButtonPress(Buttons.A, player.Player.Controller)
                || (input.IsNewKeyPress(Keys.Space))                                     
                    )
                {
                    state.ButtonA = true;
                }
                else
                {
                    state.ButtonA = false;
                }

                if (input.IsNewButtonPress(Buttons.DPadLeft, player.Player.Controller) ||
                input.IsNewButtonPress(Buttons.LeftThumbstickLeft, player.Player.Controller)
                || input.IsNewKeyPress(Keys.Left)                   
                    )
                {
                    if ((gpState.DPad.Left == ButtonState.Pressed) || (gpState.ThumbSticks.Left.X < 0)
                    || input.IsNewKeyPress(Keys.Left)                           
                        )
                    {
                        state.DPadLeft = true;
                    }
                    else
                    {
                        state.DPadLeft = false;
                    }
                }
                else
                {
                    state.DPadLeft = false;
                }

                if (input.IsNewButtonPress(Buttons.DPadRight, player.Player.Controller) ||
                    input.IsNewButtonPress(Buttons.LeftThumbstickRight, player.Player.Controller)
                    || input.IsNewKeyPress(Keys.Right)
                    )
                {
                    if ((gpState.DPad.Right == ButtonState.Pressed) || (gpState.ThumbSticks.Left.X > 0)
                    || input.IsNewKeyPress(Keys.Right)    
                        )
                    {
                        state.DPadRight = true;
                    }
                    else
                    {
                        state.DPadRight = false;
                    }
                }
                else
                {
                    state.DPadRight = false;
                }

                if (input.IsNewButtonPress(Buttons.LeftTrigger, player.Player.Controller)
                || (input.IsNewKeyPress(Keys.Down))                    
                    ) // || (gpState.Buttons.LeftShoulder == ButtonState.Pressed))
                {
                    state.ButtonLT = true;
                }
                else
                {
                    state.ButtonLT = false;
                }

                if (input.IsNewButtonPress(Buttons.RightTrigger, player.Player.Controller)
                || (input.IsNewKeyPress(Keys.Up))                    
                    ) // || (gpState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    state.ButtonRT = true;
                }
                else
                {
                    state.ButtonRT = false;
                }

                stickLeft = gpState.ThumbSticks.Left;
                stickRight = gpState.ThumbSticks.Right;
                state.Fire = gpState.ThumbSticks.Right;
                state.StickLeftMovement = stickLeft;
                state.StickRightMovement = stickRight;
            }

            if (input.IsNewButtonPress(Buttons.B, player.Player.Controller)
                || (input.IsNewKeyPress(Keys.Escape) || input.IsNewKeyPress(Keys.Back))
                )
            {
                state.ButtonB = true;
            }
            else
            {
                state.ButtonB = false;
            }

            if (input.IsNewButtonPress(Buttons.Start, player.Player.Controller)
            || (input.IsNewKeyPress(Keys.Enter))
                )
            {
                state.ButtonStart = true;
            }
            else
            {
                state.ButtonStart = false;
            }


            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Toggle Ready
                    if ((gesture.Position.X >= 489 && gesture.Position.X <= 775) &&
                        (gesture.Position.Y >= 612 && gesture.Position.Y <= 648))
                    {
                        state.ButtonA = true;
                    }

                    // Toggle Ready CARD
                    if ((gesture.Position.X >= 198 && gesture.Position.X <= 336) &&
                        (gesture.Position.Y >= 288 && gesture.Position.Y <= 586))
                    {
                        state.ButtonA = true;
                    }

                    // Start Game
                    if ((gesture.Position.X >= 815 && gesture.Position.X <= 1104) &&
                        (gesture.Position.Y >= 612 && gesture.Position.Y <= 648))
                    {
                        state.ButtonStart = true;
                    }


                    // Select Player Right
                    if ((gesture.Position.X >= 341 && gesture.Position.X <= 368) &&
                        (gesture.Position.Y >= 412 && gesture.Position.Y <= 452))
                    {
                        state.DPadRight = true;
                    }

                    // Select Player Left
                    if ((gesture.Position.X >= 171 && gesture.Position.X <= 191) &&
                        (gesture.Position.Y >= 412 && gesture.Position.Y <= 452))
                    {
                        state.DPadLeft = true;
                    }

                    // Select Level Left
                    if ((gesture.Position.X >= 907 && gesture.Position.X <= 995) &&
                        (gesture.Position.Y >= 75 && gesture.Position.Y <= 176))
                    {
                        state.ButtonLT = true;
                    }

                    // Select Level Right
                    if ((gesture.Position.X >= 1099 && gesture.Position.X <= 1204) &&
                        (gesture.Position.Y >= 75 && gesture.Position.Y <= 176))
                    {
                        state.ButtonRT = true;
                    }

                    // Go Back Button
                    if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        state.ButtonB = true;
                    }

                    // BUY NOW!!
                    //if ((gesture.Position.X >= 0 && gesture.Position.X <= 90) &&
                    //    (gesture.Position.Y >= 0 && gesture.Position.Y <= 90))
                    //{
                    //    if (Guide.IsTrialMode)
                    //    {
                    //        Guide.ShowMarketplace(PlayerIndex.One);
                    //    }
                    //}
                }
            }

            return state;
        }





        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            if (((MyGame)this.ScreenManager.Game).currentGameState != GameState.Paused)
            {
                {
                    UpdatePlayer(0, playerOneInput);
                    UpdatePlayer(1, playerTwoInput);
                    UpdatePlayer(2, playerThreeInput);
                    UpdatePlayer(3, playerFourInput);
                    /*
                    if (((Game1)this.ScreenManager.Game).currentPlayers[0].IsPlaying)
                    {
                        UpdatePlayer(0, playerOneInput);
                    }

                    if (((Game1)this.ScreenManager.Game).currentPlayers[1].IsPlaying)
                    {
                        UpdatePlayer(1, playerTwoInput);
                    }

                    if (((Game1)this.ScreenManager.Game).currentPlayers[2].IsPlaying)
                    {
                        UpdatePlayer(2, playerThreeInput);
                    }

                    if (((Game1)this.ScreenManager.Game).currentPlayers[3].IsPlaying)
                    {
                        UpdatePlayer(3, playerFourInput);
                    }
                     */
                }

                checkIfCanStartTheGame();
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public void UpdateCurrentPlayersList()
        {

        }

        public void UpdatePlayer(int player, NeutralInput input)
        {
            ProcessInput(player, input);
        }


        private void checkIfCanStartTheGame()
        {
            int playersActive = 0;
            int numPlayersReady = 0;

            foreach (Avatar player in ((MyGame)this.ScreenManager.Game).currentPlayers)
            {
                if (player.status == ObjectStatus.Active)
                {
                    playersActive += 1;
                }

                if (player.isReady == true)
                {
                    numPlayersReady += 1;
                }
            }

            if (playersActive != 0)
            {
                if (playersActive == numPlayersReady)
                {
                    canStartGame = true;
                }
                else
                {
                    canStartGame = false;
                }
            }
        }


        /// <summary>
        /// This function takes a NeutralInput structure for a player and turns that data into
        /// ship commands
        /// </summary>
        /// <param name="Player">The player giving the input.</param>
        /// <param name="TotalGameSeconds">The current game time.</param>
        /// <param name="ElapsedGameSeconds">The game time elapsed since the last Update.</param>
        /// <param name="input">The input structure for the player.</param>
        public void ProcessInput(int player, NeutralInput input)
        {
            if ((input.ButtonStart && GamePad.GetState((PlayerIndex)player).Buttons.Start == ButtonState.Pressed) ||
                input.ButtonStart && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (((MyGame)this.ScreenManager.Game).currentPlayers[player].IsPlaying == false)
                {
                    ((MyGame)this.ScreenManager.Game).currentPlayers[player].status = ObjectStatus.Active;
                    canStartGame = false;
                    //if (((Game1)this.ScreenManager.Game).currentPlayers[player].Player.SignedInGamer != null)

                    {
                        switch(player)
                        {
                            case 0:
                                ((MyGame)this.ScreenManager.Game).Main = new Player(((MyGame)this.ScreenManager.Game).options, ((MyGame)this.ScreenManager.Game).audio);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player] = new Avatar();
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Initialize(((MyGame)this.ScreenManager.Game).GraphicsDevice.Viewport);
                                ((MyGame)this.ScreenManager.Game).InitializeMain((PlayerIndex)player);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player = ((MyGame)this.ScreenManager.Game).Main;
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Activate(((MyGame)this.ScreenManager.Game).Main);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.isReady = false;

                                break;
                            case 1:
                                ((MyGame)this.ScreenManager.Game).Player2 = new Player(((MyGame)this.ScreenManager.Game).options, ((MyGame)this.ScreenManager.Game).audio);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player] = new Avatar();
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Initialize(((MyGame)this.ScreenManager.Game).GraphicsDevice.Viewport);
                                ((MyGame)this.ScreenManager.Game).InitializeMain((PlayerIndex)player);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player = ((MyGame)this.ScreenManager.Game).Player2;
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.isReady = false;
                                break;
                            case 2:
                                ((MyGame)this.ScreenManager.Game).Player3 = new Player(((MyGame)this.ScreenManager.Game).options, ((MyGame)this.ScreenManager.Game).audio);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player] = new Avatar();
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Initialize(((MyGame)this.ScreenManager.Game).GraphicsDevice.Viewport);
                                ((MyGame)this.ScreenManager.Game).InitializeMain((PlayerIndex)player);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player = ((MyGame)this.ScreenManager.Game).Player3;
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.isReady = false;
                                break;
                            case 3:
                                ((MyGame)this.ScreenManager.Game).Player4 = new Player(((MyGame)this.ScreenManager.Game).options, ((MyGame)this.ScreenManager.Game).audio);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player] = new Avatar();
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Initialize(((MyGame)this.ScreenManager.Game).GraphicsDevice.Viewport);
                                ((MyGame)this.ScreenManager.Game).InitializeMain((PlayerIndex)player);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player = ((MyGame)this.ScreenManager.Game).Player4;
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.isReady = false;
                                break;
                            default:
                                ((MyGame)this.ScreenManager.Game).Main = new Player(((MyGame)this.ScreenManager.Game).options, ((MyGame)this.ScreenManager.Game).audio);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player] = new Avatar();
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Initialize(((MyGame)this.ScreenManager.Game).GraphicsDevice.Viewport);
                                ((MyGame)this.ScreenManager.Game).InitializeMain((PlayerIndex)player);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player = ((MyGame)this.ScreenManager.Game).Main;
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Activate(((MyGame)this.ScreenManager.Game).Main);
                                ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.isReady = false;
                                break;
                        }
                    }
                }
            }

            if (input.ButtonA)
            {
                toggleReady(player);
            }

            if (input.ButtonB)
            {
                //Return to main menu
                MenuCanceled(this);
            }

            if (input.DPadLeft)
            {
                //Choose other character
                changeCharacterSelected(player, false);
                input.DPadLeft = false;
            }
            if (input.DPadRight)
            {
                //Choose other character
                changeCharacterSelected(player, true);
                input.DPadRight = false;
            }

            if (IsMMandHost == true || isMatchmaking == false)
            {
                if (player == 0)
                {
                    if (input.ButtonLT)
                    {
                        //if (!licenseInformation.IsTrial)
                        {
                            if (levelSelected > 1)
                            {
                                levelSelected -= 1;
                            }
                            else
                            {
                                if ((byte)((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.levelsUnlocked != 0)
                                {
                                    levelSelected = (byte)((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.levelsUnlocked;
                                }
                                else
                                {
                                    levelSelected = 1;
                                }
                            }
                        }
                    }

                    if (input.ButtonRT)
                    {
                        
                        //if (!licenseInformation.IsTrial)
                        {
                            //if (levelSelected < 10)
                            if (levelSelected < ((MyGame)this.ScreenManager.Game).currentPlayers[player].Player.levelsUnlocked)
                            {
                                levelSelected += 1;
                            }
                            else
                            {
                                levelSelected = 1;
                            }
                        }
                    }
                }
            }

            if (canStartGame)
            {
                if (input.ButtonStart)
                {
                    {
                        List<int> ListPlayersAreGoingToPlay = new List<int>();
                        for (int i = 0; i < ((MyGame)this.ScreenManager.Game).currentPlayers.Length; i++)
                        {
                            if (((MyGame)this.ScreenManager.Game).currentPlayers[i].status == ObjectStatus.Active)
                            {
                                ListPlayersAreGoingToPlay.Add(i);
                            }
                        }

                        switch (levelSelected)
                        {
                            case 1:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.One, ListPlayersAreGoingToPlay);
                                break;
                            case 2:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Two, ListPlayersAreGoingToPlay);
                                break;
                            case 3:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Three, ListPlayersAreGoingToPlay);
                                break;
                            case 4:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Four, ListPlayersAreGoingToPlay);
                                break;
                            case 5:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Five, ListPlayersAreGoingToPlay);
                                break;
                            case 6:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Six, ListPlayersAreGoingToPlay);
                                break;
                            case 7:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Seven, ListPlayersAreGoingToPlay);
                                break;
                            case 8:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Eight, ListPlayersAreGoingToPlay);
                                break;
                            case 9:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Nine, ListPlayersAreGoingToPlay);
                                break;
                            case 10:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.Ten, ListPlayersAreGoingToPlay);
                                break;
                            default:
                                ((MyGame)this.ScreenManager.Game).BeginLocalGame(CLevel.Level.One, ListPlayersAreGoingToPlay);
                                break;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            //REVISARRR debe volver donde toque!
            ScreenManager.AddScreen(new MenuScreen());
        }

        // Leave match
        void MenuCanceled(Object sender)
        {
            MessageBoxScreen confirmExitMessageBox =
                                    new MessageBoxScreen(Strings.CharacterSelectString, Strings.ConfirmReturnMainMenuString);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox);
        }

        void menu_ShowMarketPlace(Object sender, MenuSelection selection)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (((MyGame)this.ScreenManager.Game).currentGameState != GameState.Paused)
            {

                //Draw Level Selection Menu
                DrawLevelSelectionMenu(this.ScreenManager.SpriteBatch);

                //Draw Character Selection Menu
                DrawSelectCharacters(selectPos, this.ScreenManager.SpriteBatch);

                //Draw Context Menu
                DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);

                DrawHowToPlay(this.ScreenManager.SpriteBatch, selectPos, MenuInfoFont);
            }
            else
            {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                this.ScreenManager.SpriteBatch.Draw(((MyGame)this.ScreenManager.Game).blackTexture, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused.ToUpper(), new Vector2(5, 5), Color.White);
                this.ScreenManager.SpriteBatch.End();
            }
        }

        private void DrawSelectCharacters(Vector2 pos, SpriteBatch batch)
        {
            string[] lines;
            Avatar cplayer;
            byte i;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);
            Vector2 CharacterPosition = new Vector2(MenuTitlePosition.X + 50, MenuTitlePosition.Y + 70);
            Vector2 ConnectControlerStringPosition = new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width, CharacterPosition.Y + cardBkg[1].Height / 2);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //foreach (Avatar cplayer in ((Game1)this.ScreenManager.Game).currentPlayers)
            for (i = 0; i < ((MyGame)this.ScreenManager.Game).currentPlayers.Length; i++)
            {
                if (i == 0)

                {
                    cplayer = ((MyGame)this.ScreenManager.Game).currentPlayers[i];

                    if (cplayer.Player != null)
                    {
                        if (cplayer.status == ObjectStatus.Active)
                        {

                            // Background Images
                            batch.Draw(cardBkg[0], CharacterPosition, Color.White);
                            batch.Draw(cardBkg[1], CharacterPosition, Color.White);

                            // Avatar Drawed
                            batch.Draw(pAvatar[cplayer.character], CharacterPosition, Color.White);

                            // Background Images
                            batch.Draw(cardBkg[2], CharacterPosition, Color.White);

                            // Ready
                            if (cplayer.isReady == true)
                            {
                                batch.Draw(cardBkg[3], CharacterPosition, Color.White);
                                batch.DrawString(DigitLowFont, Strings.ReadySelPlayerString,
                                    new Vector2(CharacterPosition.X + cardBkg[0].Width / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).X) / 2, CharacterPosition.Y + cardBkg[0].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).Y)),
                                    Color.Black);
                            }

                            // Avatar Pixelated
                            batch.Draw(pAvatarPixelated[cplayer.character], new Vector2(CharacterPosition.X + (pAvatar[cplayer.character].Width / 2) - pAvatarPixelated[cplayer.character].Width / 2,
                                CharacterPosition.Y + (pAvatar[cplayer.character].Height - pAvatarPixelated[cplayer.character].Height)), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                            // Gamertag
                            if (cplayer.Player.Name != null)
                            {
                                batch.DrawString(MenuInfoFont, cplayer.Player.Name, new Vector2(CharacterPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(cplayer.Player.Name).Y), CharacterPosition.Y + 2 + cardBkg[1].Height),
                                        Color.White, 4.70f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                            }

                            // Character Name
                            batch.DrawString(MenuInfoFont, pAvatarName[cplayer.character], new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(pAvatarName[cplayer.character]).X), CharacterPosition.Y + 3),
                                    Color.Black);

                            // Character NOT AVAILABLE
                            /*
                            if (cplayer.character != 0)
                            {
                                batch.DrawString(MenuInfoFont, "NOT", new Vector2(CharacterPosition.X + (pAvatar[cplayer.character].Width / 2) - pAvatarPixelated[cplayer.character].Width / 2,
                                    CharacterPosition.Y + (pAvatar[cplayer.character].Height - pAvatarPixelated[cplayer.character].Height)), Color.Black);
                                batch.DrawString(MenuInfoFont, "AVAILABLE", new Vector2(CharacterPosition.X - 30 + (pAvatar[cplayer.character].Width / 2) - pAvatarPixelated[cplayer.character].Width / 2,
                                    CharacterPosition.Y + 20 + (pAvatar[cplayer.character].Height - pAvatarPixelated[cplayer.character].Height)), Color.Black);
                            }*/

                            if (cplayer.isReady == false)
                            {
                                // Arrow Left
                                batch.Draw(arrow, new Vector2(CharacterPosition.X - arrow.Width, CharacterPosition.Y + cardBkg[1].Height / 2 - arrow.Height / 2),
                                    null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);

                                // Arrow Right
                                batch.Draw(arrow, new Vector2(CharacterPosition.X + 8 + cardBkg[1].Width, CharacterPosition.Y + cardBkg[1].Height / 2 - arrow.Height / 2),
                                    null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.FlipHorizontally, 1.0f);
                            }
                        }
                        else
                        {
                            if (GamePad.GetState(cplayer.Player.Controller).IsConnected && cplayer.Player.Name != null)
                            {
                                // Background Images
                                batch.Draw(cardBkg[0], CharacterPosition, Color.White);
                                batch.Draw(cardBkg[1], CharacterPosition, Color.White);

                                //Texto de contexto de menu
                                switch (i)
                                {
                                    case 0:
                                        lines = Regex.Split(Strings.P1PressStartString, " ");
                                        break;
                                    case 1:
                                        lines = Regex.Split(Strings.P2PressStartString, " ");
                                        break;
                                    case 2:
                                        lines = Regex.Split(Strings.P3PressStartString, " ");
                                        break;
                                    case 3:
                                        lines = Regex.Split(Strings.P4PressStartString, " ");
                                        break;
                                    default:
                                        lines = Regex.Split(Strings.P1PressStartString, " ");
                                        break;
                                }

                                foreach (string line in lines)
                                {
                                    batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                                        new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkg[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                                        Color.Black);
                                    ConnectControlerStringPosition.Y += 20;
                                }
                                // Connect Controller to play
                                //batch.DrawString(MenuInfoFont, Strings.ConnectControllerToJoinString,
                                //    new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).X), CharacterPosition.Y + cardBkg[1].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).Y)),
                                //    Color.Black);
                            }
                            else
                            {
                                lines = Regex.Split(Strings.ConnectControllerToJoinString, " ");
                                foreach (string line in lines)
                                {
                                    batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                                        new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkg[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                                        Color.White);
                                    ConnectControlerStringPosition.Y += 20;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Background Images
                        //batch.Draw(cardBkg[0], CharacterPosition, Color.White);
                        //batch.Draw(cardBkg[1], CharacterPosition, Color.White);

                        //Texto de contexto de menu
                        lines = Regex.Split(Strings.ConnectControllerToJoinString, " ");
                        foreach (string line in lines)
                        {
                            batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                                new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkg[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                                Color.White);
                            ConnectControlerStringPosition.Y += 20;
                        }
                        // Connect Controller to play
                        //batch.DrawString(MenuInfoFont, Strings.ConnectControllerToJoinString,
                        //    new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).X), CharacterPosition.Y + cardBkg[1].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).Y)),
                        //    Color.Black);
                    }

                    CharacterPosition.X += 250;
                    ConnectControlerStringPosition.X += 250;
                    ConnectControlerStringPosition.Y = CharacterPosition.Y + cardBkg[1].Height / 2;
                }
            }

            batch.End();
        }

        private void DrawLevelSelectionMenu(SpriteBatch batch)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Background
            batch.Draw(myGroupBKG, new Vector2(uiBounds.X + uiBounds.Width - 260, 50), Color.White);

            // LEVEL SELECTION STRING
            batch.DrawString(MenuInfoFont, Strings.LevelSelectionString, new Vector2(uiBounds.X + uiBounds.Width - MenuInfoFont.MeasureString(Strings.LevelSelectionString).X / 2 - 100, 56), Color.White);

            // Level
            batch.DrawString(DigitBigFont, String.Format("{0:00}", levelSelected), new Vector2(uiBounds.X + uiBounds.Width - 135, 90), Color.Salmon);

            // LT
            batch.DrawString(DigitLowFont, "-", new Vector2(uiBounds.X + uiBounds.Width - 210, 80), Color.White);
            if (((MyGame)this.ScreenManager.Game).Main.Options == InputMode.Keyboard)
            {   
                batch.Draw(kbDown, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, kbDown.Width, kbDown.Height), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.0f);
            }
            else
            {
                batch.Draw(btnLT, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, btnLT.Width, btnLT.Height), Color.White, 0.0f, Vector2.Zero, 0.28f, SpriteEffects.None, 0.0f);
            }

            // RT
            batch.DrawString(DigitLowFont, "+", new Vector2(uiBounds.X + uiBounds.Width - 5, 80), Color.White);
            if (((MyGame)this.ScreenManager.Game).Main.Options == InputMode.Keyboard)
            {
                batch.Draw(kbUp, new Vector2(uiBounds.X + uiBounds.Width - 10, 115), new Rectangle(0, 0, kbUp.Width, kbUp.Height), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.0f);
            }
            else
            {
                batch.Draw(btnRT, new Vector2(uiBounds.X + uiBounds.Width - 10, 115), new Rectangle(0, 0, btnRT.Width, btnRT.Height), Color.White, 0.0f, Vector2.Zero, 0.28f, SpriteEffects.None, 0.0f);
            }

            batch.End();
        }


        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //LOBBY text
            batch.DrawString(MenuHeaderFont, Strings.CharacterSelectString, MenuTitlePosition, Color.White);

            if (this.isMatchmaking == true)
            {
                //MATCHMAKING fade rotated
                batch.DrawString(MenuHeaderFont, Strings.MatchmakingMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                    new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
            }
            else
            {
                //LOCAL GAME fade rotated
                batch.DrawString(MenuHeaderFont, Strings.LocalGameString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                    new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
            }

            //Linea divisoria
            pos.X -= 40;
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);


            menu.DrawSelectPlayerButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont, canStartGame, true);


            batch.End();
        }

        public void DrawHowToPlay(SpriteBatch batch, Vector2 position, SpriteFont MenuFont)
        {
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 320, position.Y - 105);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            string[] lines;

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Lobby Header Image
            batch.Draw(HTPHeaderImage, new Vector2(MenuTitlePosition.X, MenuTitlePosition.Y + 75), Color.White);

            //Linea divisoria
            position.X -= 40;
            position.Y -= 115;
            batch.Draw(lineaMenu, new Vector2(position.X + 300, position.Y - 40), Color.White);

            //Texto de contexto del How to Play
            contextMenuPosition = new Vector2(position.X + 300, position.Y - 30);
            if (((MyGame)this.ScreenManager.Game).Main.Options == InputMode.Keyboard)
            {
                lines = Regex.Split(Strings.PCExplanationString, "\r\n");
            }
            else
            {
                lines = Regex.Split(Strings.HTPExplanationString, "\r\n");
            }
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            batch.End();
        }
    }
}