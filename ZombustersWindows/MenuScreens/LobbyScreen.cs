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

#if !WINDOWS_PHONE && !WINDOWS
namespace ZombustersWindows
{
    public class LobbyScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnStart;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar
        Texture2D lobbyHeaderImage;

        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

#region Network Settings

        // Multiplayer event handler
        public event EventHandler<MenuSelection> MenuGamerCardSelected;
        public event EventHandler<MenuSelection> MenuInviteSelected;
        public event EventHandler<MenuSelection> MenuPartySelected;
        public event EventHandler<MenuSelection> MenuStartGameSelected;

        Boolean CreateGame = false;

        static String[] PlaylistSettings = { "STORY MODE", "TIMECRISIS MODE", "HARDCORE MODE" };
        //static int CurrentPlaylistSetting = 0;


#endregion

#region Game Settings

        public int LevelSettings = 0;
        public int PrivateSlotsSettings = 0;
        public int MaxPlayersSettings = 0;

#endregion

        public LobbyScreen(Boolean create)
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

            //((Game1)this.ScreenManager.Game).gamerManager.UpdateOnline(true, ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession);

            //Initialize Main Menu
            menu.Initialize();
/*
            //Adding Options text for the Menu
            for ( i=0; i<((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count; i++)
            //foreach (String gamertag in ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag())
            {
                String gamertag = ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag()[i];
                menu.AddText(gamertag, gamertag);
            }
*/

            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            //menu.SelectedColor = new Color(198,34,40);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            MenuInviteSelected += new EventHandler<MenuSelection>(menu_InvitePlayers);
            MenuPartySelected += new EventHandler<MenuSelection>(menu_InviteParty);
            MenuStartGameSelected += new EventHandler<MenuSelection>(menu_StartGame);
            MenuGamerCardSelected += new EventHandler<MenuSelection>(menu_MenuShowGamerCard);
            
            //Posiciona el menu
            menu.CenterInXLeftMenu(view);

            this.PresenceMode = GamerPresenceMode.WaitingInLobby;
            
            //bloom.Visible = !bloom.Visible;

            base.Initialize();

            this.isBackgroundOn = true;
        }


#region InputMethods

        // Invite Players
        void menu_InvitePlayers(Object sender, MenuSelection selection)
        {
            if (!Guide.IsVisible)
            {
                Guide.ShowGameInvite(PlayerIndex.One, null);
            }

        }

        // Invite Party
        void menu_InviteParty(Object sender, MenuSelection selection)
        {
            if (!Guide.IsVisible)
            {
                // Send invitations to all members of each local network gamer's LIVE party.
                LocalNetworkGamer gamer =  ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.LocalGamers[0];
                
                // get the party size for the first signed-in gamer.
                if (gamer.SignedInGamer.PartySize > 1)
                {
                    gamer.SendPartyInvites();
                }
                
                //Guide.ShowParty(PlayerIndex.One);
            }
        }

        // Leave match
        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            //networkSessionManager.CloseSession();
            //currentGameState = GameState.SignIn;

            MessageBoxScreen confirmExitMessageBox =
                                    new MessageBoxScreen(Strings.LobbyMenuString, Strings.ConfirmReturnMainMenuString, true);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox);            
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ((Game1)this.ScreenManager.Game).networkSessionManager.CloseSession();
            // If they hit B or Back, go back to Menu Screen
            ExitScreen();
        }

        // Setup Menu
        void menu_MenuConfigSelected(Object sender, MenuSelection selection)
        {
            // If they hit Start, go to Configuration Screen
            //((Game1)this.ScreenManager.Game).DisplayOptions();
        }

        // Toggle Ready to the player
        void menu_StartGame(Object sender, MenuSelection selection)
        {
            // Get local gamer
            LocalNetworkGamer localgamer = ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.LocalGamers[0];

            if (localgamer.IsHost)
            {
                //((Game1)this.ScreenManager.Game).networkSessionManager.networkSessionState = NetworkSessionState.Playing;
                ((Game1)this.ScreenManager.Game).currentGameState = GameState.SelectPlayer;

                // Send message to other player that we'are starting
                ((Game1)this.ScreenManager.Game).networkSessionManager.packetWriter.Write((int)MessageType.SelectPlayer);
                localgamer.SendData(((Game1)this.ScreenManager.Game).networkSessionManager.packetWriter, SendDataOptions.Reliable);

                //Call StartGame
                //((Game1)this.ScreenManager.Game).BeginMultiplayerPlayerGame();
                ExitScreen();
                ((Game1)this.ScreenManager.Game).BeginSelectPlayerScreen(true);
            }
        }

        // Show Gamer Card
        void menu_MenuShowGamerCard(Object sender, MenuSelection selection)
        {
            int i,j;
            string errorMessage;

            if (((Game1)this.ScreenManager.Game).networkSessionManager.networkSession != null && !((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.IsDisposed)
            {
                // If HOST
                if (((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.IsHost)
                {
                    for (i = 0; i < Gamer.SignedInGamers.Count; i++)
                    {
                        if (selection.Selection == i && !Guide.IsVisible)
                        {
                            SignedInGamer gamer = Gamer.SignedInGamers[i];
                            if (gamer.IsSignedInToLive)
                            {
                                Guide.ShowGamerCard(PlayerIndex.One, Gamer.SignedInGamers[i]);
                            }
                        }
                    }

                    for (i = 0; i < ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers.Count; i++)
                    {
                        NetworkGamer netGamer = ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers[i];
                        if (!netGamer.IsLocal)
                        {
                            for (j = 0; j < ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count; j++)
                            {
                                if (((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag()[j] == netGamer.Gamertag)
                                {
                                    if (selection.Selection == i && !Guide.IsVisible)
                                    {
                                        try
                                        {
                                            Guide.ShowGamerCard(PlayerIndex.One, netGamer);
                                        }
                                        catch (Exception error)
                                        {
                                            errorMessage = error.Message;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (i = 0; i < ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers.Count; i++)
                    {
                        NetworkGamer netGamer = ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers[i];
                        if (!netGamer.IsLocal)
                        {
                            for (j = 0; j < ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count; j++)
                            {
                                if (((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag()[j] == netGamer.Gamertag)
                                {
                                    if (selection.Selection == i && !Guide.IsVisible)
                                    {
                                        try
                                        {
                                            Guide.ShowGamerCard(PlayerIndex.One, netGamer);
                                        }
                                        catch (Exception error)
                                        {
                                            errorMessage = error.Message;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (i = 0; i < Gamer.SignedInGamers.Count; i++)
                    {
                        if (selection.Selection == (i + ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.RemoteGamers.Count) && !Guide.IsVisible)
                        {
                            SignedInGamer gamer = Gamer.SignedInGamers[i];
                            if (gamer.IsSignedInToLive)
                            {
                                Guide.ShowGamerCard(PlayerIndex.One, Gamer.SignedInGamers[i]);
                            }
                        }
                    }
                }
            }

            


/*
            List<String> supportGamertagList = new List<String>();

            foreach (SignedInGamer player in Gamer.SignedInGamers)
            {
                if (player.IsSignedInToLive)
                {
                    supportGamertagList.Add(player.Gamertag);
                }
            }

            for (i = 0; i < ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count; i++)
            {
                //if (selection.Selection == i && !Guide.IsVisible && Gamer.SignedInGamers[selection.Selection].IsSignedInToLive)
                if (selection.Selection == i && !Guide.IsVisible)
                {
                    for ( j=0; j<Gamer.SignedInGamers.Count; j++)
                    {
                        SignedInGamer gamer = Gamer.SignedInGamers[j];
                        if (gamer.Gamertag == ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag()[i])
                        {
                            if (gamer.IsSignedInToLive)
                            {
                                Guide.ShowGamerCard(PlayerIndex.One, Gamer.SignedInGamers[j]);
                            }
                        }
                    }
                }
            }


            for ( i=0; i<((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers.Count; i++ )
            //foreach (NetworkGamer networkGamer in ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers)
            {
                NetworkGamer networkGamer = ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.AllGamers[i];
                if (supportGamertagList.Contains(networkGamer.Gamertag))
                {
                    if (selection.Selection == i && !Guide.IsVisible && Gamer.SignedInGamers[selection.Selection].IsSignedInToLive)
                    {
                        Guide.ShowGamerCard(PlayerIndex.One, Gamer.SignedInGamers[selection.Selection]);
                    }
                }
            }
             */
        }

#endregion

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

            // Lobby Header Image
            lobbyHeaderImage = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/lobby_header_image");

            ((Game1)this.ScreenManager.Game).gamerManager.Load();

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            if (input.IsNewButtonPress(Buttons.Y))
            {
                if (MenuGamerCardSelected != null)
                    MenuGamerCardSelected(this, new MenuSelection(menu.Selection));

                return;
            }

            if (input.IsNewButtonPress(Buttons.A))
            {
                MenuStartGameSelected.Invoke(this, new MenuSelection(menu.Selection));
                return;
            }

            if (input.IsNewButtonPress(Buttons.X))
            {
                MenuInviteSelected.Invoke(this, new MenuSelection(menu.Selection));
                return;
            }

            if (input.IsNewButtonPress(Buttons.RightShoulder))
            {
                MenuPartySelected.Invoke(this, new MenuSelection(menu.Selection));
                return;
            }

            menu.HandleInput(input);
            base.HandleInput(input);
        }

        void UpdateMenuLobby()
        {
            byte i;
            menu.RemoveAll();
            menu.Initialize();

            //Adding Options text for the Menu

            //Adding Options text for the Menu
            for (i = 0; i < ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count; i++)
            //foreach (String gamertag in ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag())
            {
                String gamertag = ((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag()[i];
                menu.AddText(gamertag, gamertag);
            }

            if (((Game1)this.ScreenManager.Game).networkSessionManager.networkSession.IsHost)
            {
                this.CreateGame = true;
            }
            else
            {
                this.CreateGame = false;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !Guide.IsVisible)
            {
                menu.Update(gameTime);
            }

            if (((Game1)this.ScreenManager.Game).gamerManager.GamerOnlineListHasChanged(true, ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession))
            {
                ((Game1)this.ScreenManager.Game).gamerManager.UpdateOnline(true, ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession);
                UpdateMenuLobby();
            }

            if (((Game1)this.ScreenManager.Game).gamerManager.getOnlinePlayerGamertag().Count != menu.Count)
            {
                UpdateMenuLobby();
            }

            ((Game1)this.ScreenManager.Game).networkSessionManager.ProcessIncomingData(gameTime);

            ((Game1)this.ScreenManager.Game).networkSessionManager.UpdateNetworkSession();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

#region Draw

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Draw Retrowax Logo (No lo dibujamos porque no cabe!)
            //menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height));

            //menu.Draw(gameTime);
            menu.DrawLobby(gameTime, new Vector2(uiBounds.X + uiBounds.Width - 300, 90), ((Game1)this.ScreenManager.Game).gamerManager, MenuInfoFont, ((Game1)this.ScreenManager.Game).networkSessionManager.networkSession);

            //Draw Context Menu
            DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
        }


        //Draw all the Selection buttons on the bottom of the menu
        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            string[] lines;

            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            // Lobby Header Image
            batch.Draw(lobbyHeaderImage, new Vector2(MenuTitlePosition.X, MenuTitlePosition.Y + 75), Color.White);

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //LOBBY text
            batch.DrawString(MenuHeaderFont, Strings.LobbyMenuString, MenuTitlePosition, Color.White);

            //MATCHMAKING fade rotated
            batch.DrawString(MenuHeaderFont, Strings.MatchmakingMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70), 
                new Color(255,255,255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);

            //Linea divisoria
            pos.X -= 40;
            pos.Y -= 270;
            //batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;

            pos.Y -= 115;

            // Texto ESPERANDO MAS JUGADORES...
            batch.DrawString(MenuInfoFont, Strings.WaitingForPlayersMenuString.ToUpper(), new Vector2(pos.X, pos.Y - 30), Color.White);

            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 115;

            //Texto de contexto del Lobby
            lines = Regex.Split(Strings.MatchmakingMMString, "\r\n");
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            //Linea divisoria
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);

            // Draw context buttons
            menu.DrawMenuButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont, true, this.CreateGame, false);

            batch.End();
        }

#endregion

    }

}
#endif
