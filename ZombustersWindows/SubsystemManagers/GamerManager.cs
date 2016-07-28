//using System;
//using System.Globalization;
//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
//#endif
//using GameStateManagement;
//using System.Xml.Serialization;
//using System.Xml;
//using System.IO;

//namespace ZombustersWindows.Subsystem_Managers
//{
//    public class GamerManager : DrawableGameComponent
//    {
//        Game game;

//        public List<Texture2D> colorBlock;
//        public List<Texture2D> colorBlockBig;
//        public Texture2D grayBlock;
//        public Texture2D grayBlockShort;
//        public Texture2D myGroupBKG;
//        public Texture2D squareFilled;
//        public Texture2D squareEmpty;
//        public Texture2D notLoggedInGamerPic;
//        public Texture2D lineaLobbyPlayers;
//        public Texture2D connectedIcon;
//        public Texture2D hostIcon;

//        List<Texture2D> PlayerIcons, ControllerIdImg;
//        List<String> PlayerGamertag;
//        List<Boolean> PlayerSignedIn;

//        List<Texture2D> OnlinePlayerIcons;
//        List<String> OnlinePlayerGamertag;
//        List<Boolean> OnlinePlayerSignedIn, isLocalGamer;

//        int playersCount;
//        int playersCountOnline;

//        Boolean changedFlag;
//        Boolean changedFlagOnline;

//        SpriteFont germanFont;

//        public GamerManager(Game game) 
//            : base(game)
//        {
//            this.game = game;
//            this.Visible = false;
//        }

//        public override void Initialize()
//        {
//            // Check to see if we have an account that it's signed into LIVE
//            PlayerIcons = new List<Texture2D>();
//            ControllerIdImg = new List<Texture2D>();
//            PlayerGamertag = new List<string>();
//            PlayerSignedIn = new List<Boolean>();
//            colorBlock = new List<Texture2D>();
//            colorBlockBig = new List<Texture2D>();

//            OnlinePlayerIcons = new List<Texture2D>();
//            OnlinePlayerGamertag = new List<string>();
//            OnlinePlayerSignedIn = new List<Boolean>();
//            isLocalGamer = new List<Boolean>();

//            playersCount = 0;
//            playersCountOnline = 0;
//            changedFlag = true;
//            changedFlagOnline = true;
//        }

//        public void Load()
//        {
//            int i;

//            //Color Block
//            //colorBlock = game.Content.Load<Texture2D>(@"menu/colorBlock");
//            colorBlock.Add(game.Content.Load<Texture2D>(@"menu/colorBlockBlue"));
//            colorBlock.Add(game.Content.Load<Texture2D>(@"menu/colorBlockRed"));
//            colorBlock.Add(game.Content.Load<Texture2D>(@"menu/colorBlockGreen"));
//            colorBlock.Add(game.Content.Load<Texture2D>(@"menu/colorBlockYellow"));

//            //Color Block Big
//            colorBlockBig.Add(game.Content.Load<Texture2D>(@"menu/colorBlockBigBlue"));
//            colorBlockBig.Add(game.Content.Load<Texture2D>(@"menu/colorBlockBigRed"));
//            colorBlockBig.Add(game.Content.Load<Texture2D>(@"menu/colorBlockBigGreen"));
//            colorBlockBig.Add(game.Content.Load<Texture2D>(@"menu/colorBlockBigYellow"));

//            //Gray Block
//            grayBlock = game.Content.Load<Texture2D>(@"menu/grayBlock");

//            //Gray Block Short
//            grayBlockShort = game.Content.Load<Texture2D>(@"menu/grayBlockShort");

//            //Fondo negro fade menu
//            myGroupBKG = game.Content.Load<Texture2D>(@"menu/mygroup_bkg");

//            //Square Filled
//            squareFilled = game.Content.Load<Texture2D>(@"menu/square_filled");

//            //Square Empty
//            squareEmpty = game.Content.Load<Texture2D>(@"menu/square_empty");

//            //Not Logged In Gamer Picture
//            notLoggedInGamerPic = game.Content.Load<Texture2D>(@"menu/notLoggedInGamerPicture");

//            //Linea blanca lobby player
//            lineaLobbyPlayers = game.Content.Load<Texture2D>(@"menu/linea_lobby_players_selected");

//            //Player Connected Icon
//            connectedIcon = game.Content.Load<Texture2D>(@"menu/connected_icon");

//            // Host Icon
//            hostIcon = game.Content.Load<Texture2D>(@"InGame/immune_ammo_powerup");

//            for (i = 0; i < 4; i++)
//            {
//                ControllerIdImg.Add(game.Content.Load<Texture2D>(@"menu\controllerInd_menu_P" + (i + 1)));
//            }

//            germanFont = game.Content.Load<SpriteFont>(@"menu/ArialMusicItalic");
//        }

//        public Boolean GamerListHasChanged()
//        {
//            if (playersCount != Gamer.SignedInGamers.Count)
//            {
//                changedFlag = true;
//                playersCount = Gamer.SignedInGamers.Count;
//            }

//            return changedFlag;
//        }

//#if !WINDOWS_PHONE && !WINDOWS
//        public Boolean GamerOnlineListHasChanged(Boolean isInLobby, NetworkSession session)
//        {
//            if ((isInLobby == true) && (session != null))
//            {
//                if (playersCountOnline != session.AllGamers.Count)
//                {
//                    changedFlagOnline = true;
//                    playersCountOnline = session.AllGamers.Count;
//                }
//            }

//            return changedFlagOnline;
//        }
//#endif

//        public List<Texture2D> getLocalPlayerIcons()
//        {
//            return this.PlayerIcons;
//        }

//        public List<Texture2D> getOnlinePlayerIcons()
//        {
//            return this.OnlinePlayerIcons;
//        }

//        public List<Texture2D> getControllerIdImg()
//        {
//            return this.ControllerIdImg;
//        }

//        public List<String> getLocalPlayerGamertag()
//        {
//            return this.PlayerGamertag;
//        }

//        public List<String> getOnlinePlayerGamertag()
//        {
//            return this.OnlinePlayerGamertag;
//        }

//        public List<Boolean> getPlayerSignedIn()
//        {
//            return this.PlayerSignedIn;
//        }

//        public List<Boolean> getIfIsLocalGamer()
//        {
//            return this.isLocalGamer;
//        }

//#if !WINDOWS_PHONE
//        public void LoadGamerPicture(NetworkGamer gamer)
//        {
//            gamer.BeginGetProfile(
//                delegate(IAsyncResult result)
//                {
//                    try
//                    {
//                        Gamer clone = result.AsyncState as Gamer;
//                        if (clone != null)
//                        {
//                            GamerProfile profile = clone.EndGetProfile(result);
//                            // Update picture  
//                        }
//                    }
//                    catch (GamerPrivilegeException)
//                    {
//                        // Could not access profile, don't update picture  
//                    }
//                },
//                gamer
//            );
//        }


//        public void UpdateOnline(Boolean isInLobby, NetworkSession session)
//        {
//            List<String> supportGamertagList = new List<String>();
//            GamerProfile profile;
//            Stream stream;

//            OnlinePlayerIcons.Clear();
//            OnlinePlayerGamertag.Clear();
//            OnlinePlayerSignedIn.Clear();
//            isLocalGamer.Clear();

//            if (isInLobby == true)
//            {
//                if (session.AllGamers.Count > 0)
//                {
//                    foreach (SignedInGamer player in Gamer.SignedInGamers)
//                    {
//                        if (player.IsSignedInToLive)
//                        {
//                            supportGamertagList.Add(player.Gamertag);
//                        }
//                    }
///*
//                    foreach (SignedInGamer player in Gamer.SignedInGamers)
//                    {
//                        //if (player.IsSignedInToLive)
//                        //{
//                            //supportGamertagList.Add(player.Gamertag);
//                        //}
//                        if (player.IsSignedInToLive)
//                        {
//                            player.BeginGetProfile(
//                               delegate(IAsyncResult result)
//                               {
//                                   profile = (result.AsyncState as Gamer).EndGetProfile(result);
//                                   stream = profile.GetGamerPicture();
//                                   OnlinePlayerIcons.Add(Texture2D.FromStream(((Game1)this.Game).GraphicsDevice, stream));   //Add Picture of the player to the list.
//                                   OnlinePlayerGamertag.Add((result.AsyncState as Gamer).Gamertag);                 //Add Gamertag to the list.
//                                   OnlinePlayerSignedIn.Add(true);
//                                   isLocalGamer.Add(true);
//                               },
//                               player);
//                        }
//                        else
//                        {
//                            OnlinePlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                            OnlinePlayerGamertag.Add(player.Gamertag);                 //Add Gamertag to the list.
//                            OnlinePlayerSignedIn.Add(true);
//                            isLocalGamer.Add(true);
//                        }
//                    }

//                    foreach (NetworkGamer networkGamer in session.AllGamers)
//                    {
//                        if (!networkGamer.IsLocal)
//                        {
//                            networkGamer.BeginGetProfile(
//                               delegate(IAsyncResult result)
//                               {
//                                   profile = (result.AsyncState as Gamer).EndGetProfile(result);
//                                   stream = profile.GetGamerPicture();
//                                   OnlinePlayerIcons.Add(Texture2D.FromStream(((Game1)this.Game).GraphicsDevice, stream));   //Add Picture of the player to the list.
//                                   OnlinePlayerGamertag.Add((result.AsyncState as Gamer).Gamertag);                 //Add Gamertag to the list.
//                                   OnlinePlayerSignedIn.Add(true);
//                                   isLocalGamer.Add(false);
//                               },
//                               networkGamer);
//                        }
//                    }
//*/

//                    foreach (NetworkGamer networkGamer in session.AllGamers)
//                    {
//                        if (supportGamertagList.Contains(networkGamer.Gamertag))
//                        {
//                            networkGamer.BeginGetProfile(
//                               delegate(IAsyncResult result)
//                               {
//                                   profile = (result.AsyncState as Gamer).EndGetProfile(result);
//                                   stream = profile.GetGamerPicture();
//                                   OnlinePlayerIcons.Add(Texture2D.FromStream(((Game1)this.Game).GraphicsDevice, stream));   //Add Picture of the player to the list.
//                                   OnlinePlayerGamertag.Add((result.AsyncState as Gamer).Gamertag);                 //Add Gamertag to the list.
//                                   OnlinePlayerSignedIn.Add(true);
//                                   if (supportGamertagList.Contains((result.AsyncState as Gamer).Gamertag))
//                                   {
//                                       isLocalGamer.Add(true);
//                                   }
//                                   else
//                                   {
//                                       isLocalGamer.Add(false);
//                                   }
//                               },
//                               networkGamer);

//                            //REVISAR!!!!
//                            //OnlinePlayerIcons.Add(networkGamer.GetProfile().GamerPicture);   //Add Picture of the player to the list.
//                        }
//                        else
//                        {
//                            OnlinePlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                            OnlinePlayerGamertag.Add(networkGamer.Gamertag);                 //Add Gamertag to the list.
//                            OnlinePlayerSignedIn.Add(true);
//                            if (networkGamer.IsLocal)
//                            {
//                                isLocalGamer.Add(true);
//                            }
//                            else
//                            {
//                                isLocalGamer.Add(false);
//                            }
//                        }
//                    }
//                }
//            }

//            changedFlagOnline = false;
//        }
//#endif


//        public void Update()
//        {
//            int i;
//            GamerProfile profile;
//            Stream stream;

//            PlayerIcons.Clear();
//            PlayerGamertag.Clear();
//            PlayerSignedIn.Clear();

            

//            //BUSCAR SOLUCION AL PROBLEMA DE LOS PERFILES QUE NO SON ONLINE, GAMERPICTURE!!!
//            if (Gamer.SignedInGamers.Count > 0)
//            {
//                foreach (SignedInGamer signedInGamer in Gamer.SignedInGamers)
//                {
//                    if (signedInGamer.IsSignedInToLive)
//                    {
//                        try
//                        {
//                            signedInGamer.BeginGetProfile(
//                               delegate(IAsyncResult result)
//                               {
//                                   profile = (result.AsyncState as Gamer).EndGetProfile(result);
//                                   stream = profile.GetGamerPicture();
//                                   PlayerIcons.Add(Texture2D.FromStream(((Game1)this.Game).GraphicsDevice, stream));   //Add Picture of the player to the list.
//                                   PlayerGamertag.Add((result.AsyncState as Gamer).Gamertag);                 //Add Gamertag to the list.
//                                   PlayerSignedIn.Add(true);
//                               },
//                               signedInGamer);

//                            //REVISAR!!!
//                            //PlayerIcons.Add(signedInGamer.GetProfile().GamerPicture);   //Add Picture of the player to the list.   
//                        }
//                        catch
//                        {
//                            PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                            PlayerGamertag.Add(signedInGamer.Gamertag);                 //Add Gamertag to the list.
//                            PlayerSignedIn.Add(true);
//                        }
//                    }
//                    else
//                    {
//                        PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                        PlayerGamertag.Add(signedInGamer.Gamertag);                 //Add Gamertag to the list.
//                        PlayerSignedIn.Add(true);
//                    }
//                }
//            }


//            for (i = 1; i <= (4 - Gamer.SignedInGamers.Count); i++)
//            {
//                if ((i + Gamer.SignedInGamers.Count) == 1)
//                {
//                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
//                    {
//                        PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                        PlayerGamertag.Add("P1 " + Strings.PlayerSignInString);                 //Add Gamertag to the list.
//                        PlayerSignedIn.Add(false);
//                    }
//                }

//                if ((i + Gamer.SignedInGamers.Count) == 2)
//                {
//                    if (GamePad.GetState(PlayerIndex.Two).IsConnected)
//                    {
//                        PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                        PlayerGamertag.Add("P2 " + Strings.PlayerSignInString);                 //Add Gamertag to the list.
//                        PlayerSignedIn.Add(false);
//                    }
//                }

//                if ((i + Gamer.SignedInGamers.Count) == 3)
//                {
//                    if (GamePad.GetState(PlayerIndex.Three).IsConnected)
//                    {
//                        PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                        PlayerGamertag.Add("P3 " + Strings.PlayerSignInString);                 //Add Gamertag to the list.
//                        PlayerSignedIn.Add(false);
//                    }
//                }

//                if ((i + Gamer.SignedInGamers.Count) == 4)
//                {
//                    if (GamePad.GetState(PlayerIndex.Four).IsConnected)
//                    {
//                        PlayerIcons.Add(game.Content.Load<Texture2D>(@"menu\notLoggedInGamerPicture"));   //Add Picture of the player to the list.
//                        PlayerGamertag.Add("P4 " + Strings.PlayerSignInString);                 //Add Gamertag to the list.
//                        PlayerSignedIn.Add(false);
//                    }
//                }
//            }


//            changedFlag = false;

//            //game.PresenceMode = GamerPresenceMode.AtMenu;
//        }


//        public void Draw(MenuComponent menu, Vector2 pos, SpriteBatch batch, SpriteFont font, Boolean isMatchmaking
//#if !WINDOWS_PHONE
//            , NetworkSession session
//#endif
//            )
//        {
//            int playersToJoin = 0;
//            int i = 0;
//            Vector2 playersToJoinPosition = new Vector2(pos.X - 31, pos.Y);

//            Vector2 myGroupPosition = new Vector2(pos.X - 60, pos.Y - 40);
//            Vector2 iconPosition = new Vector2(pos.X, pos.Y);
//            Vector2 textPosition = new Vector2(pos.X + 5, pos.Y + 5); //uiBounds.Height - 100);
//            Vector2 controllerPosition = new Vector2(pos.X + 250, pos.Y + 5);

//            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

//            batch.Draw(myGroupBKG, myGroupPosition, Color.White);
//            batch.DrawString(font, Strings.MyGroupString, new Vector2(myGroupPosition.X + 7, myGroupPosition.Y + 6), Color.White);

//#if !WINDOWS_PHONE
//            if (isMatchmaking && session != null)
//            {
//                if (session.IsHost)
//                {
//                    for (i = 0; i < Gamer.SignedInGamers.Count; i++)
//                    {
//                        for (int j = 0; j < OnlinePlayerGamertag.Count; j++)
//                        {
//                            if (OnlinePlayerGamertag[j] == Gamer.SignedInGamers[i].Gamertag)
//                            {
//                                batch.Draw(OnlinePlayerIcons[j], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * i), 30, 30), Color.White);

//                                if (OnlinePlayerSignedIn[j] == true)
//                                {
//                                    //ColorBlock
//                                    batch.Draw(colorBlock[i], new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                }
//                                else  //Not Logged In but with Controller activated
//                                {
//                                    //Gray Block Short
//                                    batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                }

//                                //Controller
//                                if (i == 0)
//                                {
//                                    batch.Draw(hostIcon, new Vector2(controllerPosition.X + 2, controllerPosition.Y + 2 + (33 * i)), Color.White);
//                                }
//                                else
//                                {
//                                    batch.Draw(ControllerIdImg[i], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                                }

//                                //Gamertag Text
//                                batch.DrawString(font, OnlinePlayerGamertag[j], new Vector2(textPosition.X, textPosition.Y + (33 * i)), Color.White);

//                                //Players to Join
//                                playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                            }
//                        }
//                    }

//                    for (i = 0; i < session.AllGamers.Count; i++)
//                    {
//                        if (!session.AllGamers[i].IsLocal)
//                        {
//                            for (int j = 0; j < OnlinePlayerGamertag.Count; j++)
//                            {
//                                if (OnlinePlayerGamertag[j] == session.AllGamers[i].Gamertag)
//                                {
//                                    batch.Draw(OnlinePlayerIcons[j], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * i), 30, 30), Color.White);

//                                    if (OnlinePlayerSignedIn[j] == true)
//                                    {
//                                        //ColorBlock
//                                        batch.Draw(colorBlock[i], new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                    }
//                                    else  //Not Logged In but with Controller activated
//                                    {
//                                        //Gray Block Short
//                                        batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                    }

//                                    //Controller
//                                    if (i == 0)
//                                    {
//                                        batch.Draw(hostIcon, new Vector2(controllerPosition.X + 2, controllerPosition.Y + 2 + (33 * i)), Color.White);
//                                    }
//                                    else
//                                    {
//                                        batch.Draw(connectedIcon, new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                                    }

//                                    //Gamertag Text
//                                    batch.DrawString(font, OnlinePlayerGamertag[j], new Vector2(textPosition.X, textPosition.Y + (33 * i)), Color.White);

//                                    //Players to Join
//                                    playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    for (i = 0; i < session.AllGamers.Count; i++)
//                    {
//                        if (!session.AllGamers[i].IsLocal)
//                        {
//                            for (int j = 0; j < OnlinePlayerGamertag.Count; j++)
//                            {
//                                if (OnlinePlayerGamertag[j] == session.AllGamers[i].Gamertag)
//                                {
//                                    batch.Draw(OnlinePlayerIcons[j], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * i), 30, 30), Color.White);

//                                    if (OnlinePlayerSignedIn[j] == true)
//                                    {
//                                        //ColorBlock
//                                        batch.Draw(colorBlock[i], new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                    }
//                                    else  //Not Logged In but with Controller activated
//                                    {
//                                        //Gray Block Short
//                                        batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                                    }

//                                    //Controller
//                                    if (i == 0)
//                                    {
//                                        batch.Draw(hostIcon, new Vector2(controllerPosition.X + 2, controllerPosition.Y + 2 + (33 * i)), Color.White);
//                                    }
//                                    else
//                                    {
//                                        batch.Draw(connectedIcon, new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                                    }

//                                    //Gamertag Text
//                                    batch.DrawString(font, OnlinePlayerGamertag[j], new Vector2(textPosition.X, textPosition.Y + (33 * i)), Color.White);

//                                    //Players to Join
//                                    playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                                }
//                            }
//                        }
//                    }

//                    for (i = 0; i < Gamer.SignedInGamers.Count; i++)
//                    {
//                        for (int j = 0; j < PlayerGamertag.Count; j++)
//                        {
//                            if (PlayerGamertag[j] == Gamer.SignedInGamers[i].Gamertag)
//                            {
//                                batch.Draw(PlayerIcons[j], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * (i + session.RemoteGamers.Count)), 30, 30), Color.White);

//                                if (PlayerSignedIn[j] == true)
//                                {
//                                    //ColorBlock
//                                    batch.Draw(colorBlock[i + session.RemoteGamers.Count], new Vector2(iconPosition.X, iconPosition.Y + (33 * (i + session.RemoteGamers.Count))), Color.White);
//                                }
//                                else  //Not Logged In but with Controller activated
//                                {
//                                    //Gray Block Short
//                                    batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * (i + session.RemoteGamers.Count))), Color.White);
//                                }

//                                // Controller
//                                batch.Draw(ControllerIdImg[i], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * (i + session.RemoteGamers.Count))), Color.White);

//                                //Gamertag Text
//                                batch.DrawString(font, PlayerGamertag[j], new Vector2(textPosition.X, textPosition.Y + (33 * (i + session.RemoteGamers.Count))), Color.White);

//                                //Players to Join
//                                playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                            }
//                        }
//                    }
//                }
///*
//                for ( i=0; i<OnlinePlayerIcons.Count; i++ )
//                {
//                    batch.Draw(OnlinePlayerIcons[i], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * i), 30, 30), Color.White);

//                    if (OnlinePlayerSignedIn[i] == true)
//                    {
//                        //ColorBlock
//                        batch.Draw(colorBlock, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                    }
//                    else  //Not Logged In but with Controller activated
//                    {
//                        //Gray Block Short
//                        batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                    }

//                    //Controller
//                    if (i == 0)
//                    {
//                        batch.Draw(hostIcon, new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                    }
//                    else
//                    {
//                        if (getIfIsLocalGamer()[i] == true)
//                        {
//                            batch.Draw(ControllerIdImg[i], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                        }
//                        else
//                        {
//                            batch.Draw(connectedIcon, new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);
//                        }
//                    }
                    
//                    //Gamertag Text
//                    batch.DrawString(font, OnlinePlayerGamertag[i], new Vector2(textPosition.X, textPosition.Y + (33 * i)), Color.White);

//                    //Players to Join
//                    playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                }
// */

//                //Not Logged In but with Controller activated
//                if (PlayerIcons.Count != Gamer.SignedInGamers.Count)
//                {
//                    for (i = 0; i < PlayerIcons.Count - Gamer.SignedInGamers.Count; i++)
//                    {
//                        batch.Draw(notLoggedInGamerPic, new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * (i + Gamer.SignedInGamers.Count)), 30, 30), Color.White);

//                        //Gray Block Short
//                        batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        // Controller
//                        batch.Draw(ControllerIdImg[(i + Gamer.SignedInGamers.Count)], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        //Gamertag Text
//                        batch.DrawString(font, Strings.PlayerSignInString, new Vector2(textPosition.X, textPosition.Y + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        //Players to Join
//                        playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                    }
//                }

//                playersToJoin = 4 - PlayerIcons.Count;

//                if (playersToJoin != 0)
//                {
//                    for (i = 0; i < playersToJoin; i++)
//                    {
//                        //Gray Block
//                        batch.Draw(grayBlock, new Vector2(playersToJoinPosition.X, playersToJoinPosition.Y + (33 * i)), new Color(255, 255, 255, (byte)MathHelper.Clamp(60, 0, 255)));

//                        //Gray Block
//                        batch.Draw(squareEmpty, new Vector2(playersToJoinPosition.X + 5, playersToJoinPosition.Y + (33 * i) + 5), new Color(255, 255, 255, (byte)MathHelper.Clamp(80, 0, 255)));

//                        //Gamertag Text
//                        batch.DrawString(font, Strings.ConnectControllerToJoinString, new Vector2(playersToJoinPosition.X + 36, playersToJoinPosition.Y + 5 + (33 * i)), new Color(255, 255, 255, (byte)MathHelper.Clamp(80, 0, 255)));
//                        //(i+1+count).ToString() + "P " + 
//                    }
//                }
//            }
//            else
//#endif
//            {
//                for (i = 0; i < Gamer.SignedInGamers.Count; i++)
//                {
//                    for (int j = 0; j < PlayerGamertag.Count; j++)
//                    {
//                        if (PlayerGamertag[j] == Gamer.SignedInGamers[i].Gamertag)
//                        {
//                            batch.Draw(PlayerIcons[j], new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * i), 30, 30), Color.White);

//                            if (PlayerSignedIn[j] == true)
//                            {
//                                //ColorBlock
//                                batch.Draw(colorBlock[i], new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                            }
//                            else  //Not Logged In but with Controller activated
//                            {
//                                //Gray Block Short
//                                batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * i)), Color.White);
//                            }

//                            // Controller
//                            batch.Draw(ControllerIdImg[i], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * i)), Color.White);

//                            //Gamertag Text
//                            batch.DrawString(font, PlayerGamertag[j], new Vector2(textPosition.X, textPosition.Y + (33 * i)), Color.White);

//                            //Players to Join
//                            playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                        }
//                    }
//                }

//                //Not Logged In but with Controller activated
//                if (PlayerIcons.Count != Gamer.SignedInGamers.Count)
//                {
//                    for (i = 0; i < PlayerIcons.Count - Gamer.SignedInGamers.Count; i++)
//                    {
//                        batch.Draw(notLoggedInGamerPic, new Rectangle(Convert.ToInt32(pos.X) - 31, Convert.ToInt32(pos.Y) + (33 * (i + Gamer.SignedInGamers.Count)), 30, 30), Color.White);
                        
//                        //Gray Block Short
//                        batch.Draw(grayBlockShort, new Vector2(iconPosition.X, iconPosition.Y + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        // Controller
//                        batch.Draw(ControllerIdImg[(i + Gamer.SignedInGamers.Count)], new Vector2(controllerPosition.X, controllerPosition.Y + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        //Gamertag Text
//                        batch.DrawString(germanFont, Strings.PlayerSignInString, new Vector2(textPosition.X, textPosition.Y + 2 + (33 * (i + Gamer.SignedInGamers.Count))), Color.White);

//                        //Players to Join
//                        playersToJoinPosition.Y = playersToJoinPosition.Y + 33;
//                    }
//                }

//                playersToJoin = 4 - PlayerIcons.Count;

//                if (playersToJoin != 0)
//                {
//                    for (i = 0; i < playersToJoin; i++)
//                    {
//                        //Gray Block
//                        batch.Draw(grayBlock, new Vector2(playersToJoinPosition.X, playersToJoinPosition.Y + (33 * i)), new Color(255, 255, 255, (byte)MathHelper.Clamp(60, 0, 255)));

//                        //Gray Block
//                        batch.Draw(squareEmpty, new Vector2(playersToJoinPosition.X + 5, playersToJoinPosition.Y + (33 * i) + 5), new Color(255, 255, 255, (byte)MathHelper.Clamp(80, 0, 255)));

//                        //Gamertag Text
//                        if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de")
//                        {
//                            batch.DrawString(germanFont, Strings.ConnectControllerToJoinString, new Vector2(playersToJoinPosition.X + 36, playersToJoinPosition.Y + 7 + (33 * i)), new Color(255, 255, 255, (byte)MathHelper.Clamp(80, 0, 255)));
//                        }
//                        else
//                        {
//                            batch.DrawString(font, Strings.ConnectControllerToJoinString, new Vector2(playersToJoinPosition.X + 36, playersToJoinPosition.Y + 5 + (33 * i)), new Color(255, 255, 255, (byte)MathHelper.Clamp(80, 0, 255)));
//                            //(i+1+count).ToString() + "P " + 
//                        }
//                    }
//                }
//            }


//            batch.End();
//        }
//    }
//}