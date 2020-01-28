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
        MyGame game;
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

        List<Texture2D> avatarTexture, avatarPixelatedTexture, cardBkgTexture;
        List<String> avatarNameList;

        NeutralInput playerOneInput;
        NeutralInput playerTwoInput;
        NeutralInput playerThreeInput;
        NeutralInput playerFourInput;

        SpriteFont DigitBigFont, DigitLowFont;
        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        private MenuComponent menu;
        readonly Boolean isMatchmaking;
        Boolean canStartGame;
        Boolean isMMandHost;
        public byte levelSelected;

        public SelectPlayerScreen(Boolean ismatchmaking)
        {
            this.isMatchmaking = ismatchmaking;
        }

        public override void Initialize()
        {
            this.game = (MyGame)this.ScreenManager.Game;

            byte i;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

            titleBounds = new Rectangle(0, 0, 1280, 720);

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);

            DigitBigFont = game.Content.Load<SpriteFont>(@"menu\DigitBig");
            DigitLowFont = game.Content.Load<SpriteFont>(@"menu\DigitLow");
            MenuHeaderFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            HTPHeaderImage = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/lobby_header_image");

            menu = new MenuComponent(game, MenuListFont);
            menu.Initialize();
            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            avatarTexture = new List<Texture2D>(4);
            avatarPixelatedTexture = new List<Texture2D>(4);
            cardBkgTexture = new List<Texture2D>(4);
            for (i = 0; i < 4; i++)
            {
                game.currentPlayers[i].character = 0;
            }
            avatarNameList = new List<string>(4);
            avatarNameList.Add("Tracy");
            avatarNameList.Add("Charles");
            avatarNameList.Add("Ryan");
            avatarNameList.Add("Peter");

            levelSelected = 1;
            canStartGame = false;
            isMMandHost = false;

            game.currentPlayers[0].Player.LoadSavedGame();

            // Nos aseguramos que no se queda marcado el flag de selección de personaje
            foreach (Avatar player in game.currentPlayers)
            {
                if (game.currentPlayers[0].Equals(player))
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
            kbUp = game.Content.Load<Texture2D>(@"Keyboard/key_up");

            // Key "Down"
            kbDown = game.Content.Load<Texture2D>(@"Keyboard/key_down");

            //Button "A"
            btnA = game.Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Button "Select"
            btnStart = game.Content.Load<Texture2D>("xboxControllerStart");

            // Button Left Trigger
            btnLT = game.Content.Load<Texture2D>("xboxControllerLeftTrigger");

            // Button Right Trigger
            btnRT = game.Content.Load<Texture2D>("xboxControllerRightTrigger");

            //Linea blanca separatoria
            lineaMenu = game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = game.Content.Load<Texture2D>(@"menu/logo_menu");

            //Arrow
            arrow = game.Content.Load<Texture2D>(@"InGame/SelectPlayer/arrowLeft");

            //Fondo negro fade menu
            myGroupBKG = game.Content.Load<Texture2D>(@"menu/mygroup_bkg");

            for (i=1; i <= 4; i++)
            {
                avatarTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar"));
                avatarPixelatedTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar_pixel"));
            }

            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_bkg"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_border"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_gradient"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_ready"));

            base.LoadContent();
        }

        void ToggleReady(int PlayerIndex)
        {
            if (game.currentPlayers[PlayerIndex].isReady == true)
            {
                game.currentPlayers[PlayerIndex].isReady = false;
            }
            else
            {
                game.currentPlayers[PlayerIndex].isReady = true;
            }

            for (int i = 0; i < game.currentPlayers.Length; i++)
            {
                if (i != PlayerIndex)
                {
                    if (game.currentPlayers[i].character == game.currentPlayers[PlayerIndex].character && game.currentPlayers[i].isReady == true)
                    {
                        game.currentPlayers[PlayerIndex].isReady = false;
                    }
                }
            }
        }

        void ChangeCharacterSelected(int PlayerIndex, Boolean directionToRight)
        {
            if (directionToRight == true)
            {
                if (game.currentPlayers[PlayerIndex].isReady == false)
                {
                    game.currentPlayers[PlayerIndex].character += 1;
                    if (game.currentPlayers[PlayerIndex].character > 3)
                    {
                        game.currentPlayers[PlayerIndex].character = 0;
                    }
                }
            }
            else
            {
                if (game.currentPlayers[PlayerIndex].isReady == false)
                {
                    game.currentPlayers[PlayerIndex].character -= 1;
                    if (game.currentPlayers[PlayerIndex].character < 0 || game.currentPlayers[PlayerIndex].character > 3)
                    {
                        game.currentPlayers[PlayerIndex].character = 3;
                    }
                }
            }
        }

        public override void HandleInput(InputState input)
        {
            if (game.currentPlayers[0].Player.Controller == PlayerIndex.One && GamePad.GetState(PlayerIndex.One).IsConnected)
                playerOneInput = ProcessPlayer(game.currentPlayers[0], input);
            if (game.currentPlayers[1].Player.Controller == PlayerIndex.Two && GamePad.GetState(PlayerIndex.Two).IsConnected)
                playerTwoInput = ProcessPlayer(game.currentPlayers[1], input);
            if (game.currentPlayers[2].Player.Controller == PlayerIndex.Three && GamePad.GetState(PlayerIndex.Three).IsConnected)
                playerThreeInput = ProcessPlayer(game.currentPlayers[2], input);
            if (game.currentPlayers[3].Player.Controller == PlayerIndex.Four && GamePad.GetState(PlayerIndex.Four).IsConnected)
                playerFourInput = ProcessPlayer(game.currentPlayers[3], input);

            base.HandleInput(input);
        }


        private static NeutralInput ProcessPlayer(Avatar player, InputState input)
        {
            NeutralInput state = new NeutralInput
            {
                Fire = Vector2.Zero
            };
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
            if (game.currentGameState != GameState.Paused)
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

                CheckIfCanStartTheGame();
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


        private void CheckIfCanStartTheGame()
        {
            int playersActive = 0;
            int numPlayersReady = 0;

            foreach (Avatar player in game.currentPlayers)
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
                if (game.currentPlayers[player].IsPlaying == false)
                {
                    game.currentPlayers[player].status = ObjectStatus.Active;
                    canStartGame = false;
                    //if (game.currentPlayers[player].Player.SignedInGamer != null)

                    {
                        switch (player)
                        {
                            case 0:
                                game.player1 = new Player(game.options, game.audio, game);
                                game.currentPlayers[player] = new Avatar();
                                game.currentPlayers[player].Initialize(game.GraphicsDevice.Viewport);
                                game.InitializeMain((PlayerIndex)player);
                                game.currentPlayers[player].Player = game.player1;
                                game.currentPlayers[player].Activate(game.player1);
                                game.currentPlayers[player].Player.isReady = false;

                                break;
                            case 1:
                                game.player2 = new Player(game.options, game.audio, game);
                                game.currentPlayers[player] = new Avatar();
                                game.currentPlayers[player].Initialize(game.GraphicsDevice.Viewport);
                                game.InitializeMain((PlayerIndex)player);
                                game.currentPlayers[player].Player = game.player2;
                                game.currentPlayers[player].Player.isReady = false;
                                break;
                            case 2:
                                game.player3 = new Player(game.options, game.audio, game);
                                game.currentPlayers[player] = new Avatar();
                                game.currentPlayers[player].Initialize(game.GraphicsDevice.Viewport);
                                game.InitializeMain((PlayerIndex)player);
                                game.currentPlayers[player].Player = game.player3;
                                game.currentPlayers[player].Player.isReady = false;
                                break;
                            case 3:
                                game.player4 = new Player(game.options, game.audio, game);
                                game.currentPlayers[player] = new Avatar();
                                game.currentPlayers[player].Initialize(game.GraphicsDevice.Viewport);
                                game.InitializeMain((PlayerIndex)player);
                                game.currentPlayers[player].Player = game.player4;
                                game.currentPlayers[player].Player.isReady = false;
                                break;
                            default:
                                game.player1 = new Player(game.options, game.audio, game);
                                game.currentPlayers[player] = new Avatar();
                                game.currentPlayers[player].Initialize(game.GraphicsDevice.Viewport);
                                game.InitializeMain((PlayerIndex)player);
                                game.currentPlayers[player].Player = game.player1;
                                game.currentPlayers[player].Activate(game.player1);
                                game.currentPlayers[player].Player.isReady = false;
                                break;
                        }
                    }
                }
            }

            if (input.ButtonA)
            {
                ToggleReady(player);
            }

            if (input.ButtonB)
            {
                //Return to main menu
                MenuCanceled(this);
            }

            if (input.DPadLeft)
            {
                //Choose other character
                ChangeCharacterSelected(player, false);
                input.DPadLeft = false;
            }
            if (input.DPadRight)
            {
                //Choose other character
                ChangeCharacterSelected(player, true);
                input.DPadRight = false;
            }

            if (isMMandHost == true || isMatchmaking == false)
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
                                if ((byte)game.currentPlayers[player].Player.levelsUnlocked != 0)
                                {
                                    levelSelected = (byte)game.currentPlayers[player].Player.levelsUnlocked;
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
                            if (levelSelected < game.currentPlayers[player].Player.levelsUnlocked)
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
                        for (int i = 0; i < game.currentPlayers.Length; i++)
                        {
                            if (game.currentPlayers[i].status == ObjectStatus.Active)
                            {
                                ListPlayersAreGoingToPlay.Add(i);
                            }
                        }

                        switch (levelSelected)
                        {
                            case 1:
                                game.BeginLocalGame(LevelType.One, ListPlayersAreGoingToPlay);
                                break;
                            case 2:
                                game.BeginLocalGame(LevelType.Two, ListPlayersAreGoingToPlay);
                                break;
                            case 3:
                                game.BeginLocalGame(LevelType.Three, ListPlayersAreGoingToPlay);
                                break;
                            case 4:
                                game.BeginLocalGame(LevelType.Four, ListPlayersAreGoingToPlay);
                                break;
                            case 5:
                                game.BeginLocalGame(LevelType.Five, ListPlayersAreGoingToPlay);
                                break;
                            case 6:
                                game.BeginLocalGame(LevelType.Six, ListPlayersAreGoingToPlay);
                                break;
                            case 7:
                                game.BeginLocalGame(LevelType.Seven, ListPlayersAreGoingToPlay);
                                break;
                            case 8:
                                game.BeginLocalGame(LevelType.Eight, ListPlayersAreGoingToPlay);
                                break;
                            case 9:
                                game.BeginLocalGame(LevelType.Nine, ListPlayersAreGoingToPlay);
                                break;
                            case 10:
                                game.BeginLocalGame(LevelType.Ten, ListPlayersAreGoingToPlay);
                                break;
                            default:
                                game.BeginLocalGame(LevelType.One, ListPlayersAreGoingToPlay);
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
            ExitScreen();
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

            if (game.currentGameState != GameState.Paused)
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
                this.ScreenManager.SpriteBatch.Draw(game.blackTexture, new Vector2(0, 0), Color.White);
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
            Vector2 ConnectControlerStringPosition = new Vector2(CharacterPosition.X - 5 + cardBkgTexture[1].Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //foreach (Avatar cplayer in ((Game1)this.ScreenManager.Game).currentPlayers)
            for (i = 0; i < game.currentPlayers.Length; i++)
            {
                if (i == 0)

                {
                    cplayer = game.currentPlayers[i];

                    if (cplayer.Player != null)
                    {
                        if (cplayer.status == ObjectStatus.Active)
                        {

                            // Background Images
                            batch.Draw(cardBkgTexture[0], CharacterPosition, Color.White);
                            batch.Draw(cardBkgTexture[1], CharacterPosition, Color.White);

                            // Avatar Drawed
                            batch.Draw(avatarTexture[cplayer.character], CharacterPosition, Color.White);

                            // Background Images
                            batch.Draw(cardBkgTexture[2], CharacterPosition, Color.White);

                            // Ready
                            if (cplayer.isReady == true)
                            {
                                batch.Draw(cardBkgTexture[3], CharacterPosition, Color.White);
                                batch.DrawString(DigitLowFont, Strings.ReadySelPlayerString,
                                    new Vector2(CharacterPosition.X + cardBkgTexture[0].Width / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).X) / 2, CharacterPosition.Y + cardBkgTexture[0].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).Y)),
                                    Color.Black);
                            }

                            // Avatar Pixelated
                            batch.Draw(avatarPixelatedTexture[cplayer.character], new Vector2(CharacterPosition.X + (avatarTexture[cplayer.character].Width / 2) - avatarPixelatedTexture[cplayer.character].Width / 2,
                                CharacterPosition.Y + (avatarTexture[cplayer.character].Height - avatarPixelatedTexture[cplayer.character].Height)), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                            // Gamertag
                            if (cplayer.Player.Name != null)
                            {
                                batch.DrawString(MenuInfoFont, cplayer.Player.Name, new Vector2(CharacterPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(cplayer.Player.Name).Y), CharacterPosition.Y + 2 + cardBkgTexture[1].Height),
                                        Color.White, 4.70f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                            }

                            // Character Name
                            batch.DrawString(MenuInfoFont, avatarNameList[cplayer.character], new Vector2(CharacterPosition.X - 5 + cardBkgTexture[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(avatarNameList[cplayer.character]).X), CharacterPosition.Y + 3),
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
                                batch.Draw(arrow, new Vector2(CharacterPosition.X - arrow.Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2 - arrow.Height / 2),
                                    null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);

                                // Arrow Right
                                batch.Draw(arrow, new Vector2(CharacterPosition.X + 8 + cardBkgTexture[1].Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2 - arrow.Height / 2),
                                    null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.FlipHorizontally, 1.0f);
                            }
                        }
                        else
                        {
                            if (GamePad.GetState(cplayer.Player.Controller).IsConnected && cplayer.Player.Name != null)
                            {
                                // Background Images
                                batch.Draw(cardBkgTexture[0], CharacterPosition, Color.White);
                                batch.Draw(cardBkgTexture[1], CharacterPosition, Color.White);

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
                                        new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
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
                                        new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
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
                                new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
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
                    ConnectControlerStringPosition.Y = CharacterPosition.Y + cardBkgTexture[1].Height / 2;
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
            if (game.player1.Options == InputMode.Keyboard)
            {   
                batch.Draw(kbDown, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, kbDown.Width, kbDown.Height), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.0f);
            }
            else
            {
                batch.Draw(btnLT, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, btnLT.Width, btnLT.Height), Color.White, 0.0f, Vector2.Zero, 0.28f, SpriteEffects.None, 0.0f);
            }

            // RT
            batch.DrawString(DigitLowFont, "+", new Vector2(uiBounds.X + uiBounds.Width - 5, 80), Color.White);
            if (game.player1.Options == InputMode.Keyboard)
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
            if (game.player1.Options == InputMode.Keyboard)
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