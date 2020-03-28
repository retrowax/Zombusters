using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;

#if !WINDOWS && !NETCOREAPP
namespace ZombustersWindows
{
    public class CreateFindMenuScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnStart;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar

        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

#region Network Settings

        Boolean CreateGame = false;

        static String[] PlaylistSettings = { "STORY MODE", "TIMECRISIS MODE", "HARDCORE MODE" };
        static int CurrentPlaylistSetting = 0;

        //static String[] NetworkSettings = { "XBOX LIVE", "SYSTEM LINK" };
        //static int CurrentNetworkSetting = 0;

#endregion

#region Game Settings

        public int LevelSettings = 1;
        public int PrivateSlotsSettings = 0;
        public int MaxPlayersSettings = 4;

#endregion

        public CreateFindMenuScreen(Boolean create)
        {
            this.CreateGame = create;
        }

        private MenuComponent menu;
        //private BloomComponent bloom;

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

            titleBounds = new Rectangle(0, 0, 1280, 720);

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);

            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);

            //bloom = new BloomComponent(this.ScreenManager.Game);

            //Initialize Main Menu
            menu.Initialize();

            //Adding Options text for the Menu
            if (CreateGame == true)
            {
                //CREATE GAME MENU
                menu.AddText(Strings.LevelSelectMenuString + " : " + LevelSettings, Strings.LevelSelectMMString);           // LEVEL
                menu.AddText(Strings.PrivateSlotsMenuString + " : " + PrivateSlotsSettings, Strings.PrivateSlotsMMString);  // PRIVATE SLOTS
                menu.AddText(Strings.MaxPlayersMenuString + " : " + MaxPlayersSettings, Strings.MaxPlayersMMString);        // MAX PLAYERS
                menu.AddText(Strings.CreateGameMenuString, Strings.CreateGameMMString);                                     // CREATE GAME
            }
            else
            {
                //FIND SESSION MENU
                menu.AddText(Strings.LevelSelectMenuString + " : " + LevelSettings, Strings.LevelSelectMMString);        // LEVEL
                menu.AddText(Strings.MaxPlayersMenuString + " : " + MaxPlayersSettings, Strings.MaxPlayersMMString);     // MAX PLAYERS
                menu.AddText(Strings.FindSessionMenuString, Strings.FindSessionMMString);                                // FIND SESSION
            }

            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            //menu.SelectedColor = new Color(198,34,40);
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            
            //Posiciona el menu
            menu.CenterInXLeftMenu(view);

#if !WINDOWS_PHONE
            this.PresenceMode = GamerPresenceMode.AtMenu;
#endif
            
            //bloom.Visible = !bloom.Visible;

            base.Initialize();

            this.isBackgroundOn = true;
        }


        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            // If they hit B or Back, go back to Menu Screen
            ScreenManager.AddScreen(new MatchmakingMenuScreen());
        }


        void menu_MenuConfigSelected(Object sender, MenuSelection selection)
        {
            // If they hit Start, go to Configuration Screen
            SignedInGamer gamer;
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.One);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(0);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Two);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(1);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Three);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(2);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Four);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(3);
                }
#endif
            }
            //((Game1)this.ScreenManager.Game).DisplayOptions();
        }


        void UpdatePlaylistSettings()
        {
            CurrentPlaylistSetting++;

            if (CurrentPlaylistSetting > PlaylistSettings.Length - 1)
            {
                CurrentPlaylistSetting = 0;
            }

            menu.UpdateText(Strings.PlaylistMatchmakingString + " : " + PlaylistSettings[CurrentPlaylistSetting], Strings.PlaylistMatchmakingMMString, 0);
        }

        void UpdateLevelSettings()
        {
            LevelSettings++;

            if (LevelSettings > 10) //ESTA HARDCODED REVISARRRR PUTILLA!!!
            {
                LevelSettings = 1;
            }

            menu.UpdateText(Strings.LevelSelectMenuString + " : " + LevelSettings, Strings.LevelSelectMMString, 0);
        }

        void UpdatePrivateSlotsSettings()
        {
            PrivateSlotsSettings++;

            if (PrivateSlotsSettings > 3) //ESTA HARDCODED REVISARRRR PUTILLA!!!
            {
                PrivateSlotsSettings = 0;
            }

            menu.UpdateText(Strings.PrivateSlotsMenuString + " : " + PrivateSlotsSettings, Strings.PrivateSlotsMMString, 1);
        }

        void UpdateMaxPlayersSettings()
        {
            MaxPlayersSettings++;

            if (MaxPlayersSettings > 4) //ESTA HARDCODED REVISARRRR PUTILLA!!!
            {
                MaxPlayersSettings = 0;
            }

            if (this.CreateGame == true)
            {
                menu.UpdateText(Strings.MaxPlayersMenuString + " : " + MaxPlayersSettings, Strings.MaxPlayersMMString, 2);
            }
            else
            {
                menu.UpdateText(Strings.MaxPlayersMenuString + " : " + MaxPlayersSettings, Strings.MaxPlayersMMString, 1);
            }
        }


        void menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            if (this.CreateGame == true)
            {
                switch (selection.Selection)
                {
                    case 0: // LEVEL
                        UpdateLevelSettings();
                        break;

                    case 1: // PRIVATE SLOTS
                        UpdatePrivateSlotsSettings();
                        break;

                    case 2: // MAX PLAYERS
                        UpdateMaxPlayersSettings();
                        break;

                    case 3: // CREATE GAME
#if !WINDOWS_PHONE
                        // Stop the Leaderboard Sync Manager Before create a NetworkSession
                        ((Game1)this.ScreenManager.Game).mSyncManager.stop(delegate()
                        {
                            ((Game1)this.ScreenManager.Game).networkSessionManager.CreateSession(((Game1)this.ScreenManager.Game).CurrentNetworkSetting, PrivateSlotsSettings, MaxPlayersSettings);
                        }, true);
#endif
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (selection.Selection)
                {
                    case 0: // LEVEL
                        UpdateLevelSettings();
                        break;

                    case 1: // MAX PLAYERS
                        UpdateMaxPlayersSettings();
                        break;

                    case 2: // FIND SESSION
#if !WINDOWS_PHONE
                        //((Game1)this.ScreenManager.Game).DisplayLobby(false);
                        // Stop the Leaderboard Sync Manager Before joining a NetworkSession
                        ((Game1)this.ScreenManager.Game).mSyncManager.stop(delegate()
                        {
                            ((Game1)this.ScreenManager.Game).networkSessionManager.JoinSession(((Game1)this.ScreenManager.Game).CurrentNetworkSetting, MaxPlayersSettings);
                        }, true);
#endif
                        break;

                    default:
                        break;
                }
            }
        }
        
        public override void LoadContent()
        {
            //Button "A"
            btnA = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Button "Select"
            btnStart = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerStart");

            //Linea blanca separatoria
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            // Menu update
            if (!coveredByOtherScreen && !Guide.IsVisible)
            {
                menu.Update(gameTime);
            }

            // Network Session Manager Update
            //((Game1)this.ScreenManager.Game).networkSessionManager.UpdateNetworkSession();

#if XBOX
            // Gamer Manager Update
            if (((Game1)this.ScreenManager.Game).gamerManager.GamerListHasChanged())
            {
                ((Game1)this.ScreenManager.Game).gamerManager.Update();
            }
#endif

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Draw Menu
            menu.Draw(gameTime);

            // Draw Retrowax Logo
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);

            // Draw Context Menu
            DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
#if XBOX
            // Draw "My Group" info
            ((Game1)this.ScreenManager.Game).gamerManager.Draw(menu, new Vector2(uiBounds.X + uiBounds.Width - 300, 90), this.ScreenManager.SpriteBatch, MenuInfoFont, false
#if !WINDOWS_PHONE
                , null
#endif
                );
#endif
        }


        //Draw all the Selection buttons on the bottom of the menu
        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            string[] lines;

            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //MATCHMAKING fade rotated
            batch.DrawString(MenuHeaderFont, Strings.MatchmakingMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);

            //Texto de CREATE GAME or FIND GAME
            if (this.CreateGame == true)
            {
                batch.DrawString(MenuHeaderFont, Strings.CreateGameMenuString, MenuTitlePosition, Color.White);
            }
            else
            {
                batch.DrawString(MenuHeaderFont, Strings.FindSessionMenuString, MenuTitlePosition, Color.White);
            }

            //Linea divisoria
            pos.X -= 40;
            pos.Y -= 270;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;

            pos.Y -= 115;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 115;

            //Texto de contexto de menu
            lines = Regex.Split(menu.HelpText[menu.Selection], "\r\n");
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            //Linea divisoria
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);

#if !WINDOWS_PHONE
            // Draw context buttons
            menu.DrawMenuButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont, false, false, false);
#endif

            batch.End();
        }
    }

}
#endif