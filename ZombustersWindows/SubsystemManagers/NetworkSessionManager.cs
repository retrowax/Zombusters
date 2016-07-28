using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using ZombustersWindows.Subsystem_Managers;

#if !WINDOWS_PHONE && !WINDOWS
namespace ZombustersWindows
{
    /// <summary>
    /// This GameScreen prompts for signin remains active until signin is complete.
    /// </summary>
    public class SignInScreen : BackgroundScreen
    {
        public SignInScreen()
        {
        }
        public SignInScreen(int paneCount, bool onlineOnly)
        {
            this.paneCount = paneCount;
            this.onlineOnly = onlineOnly;
        }
        public event EventHandler ScreenFinished;
        public int paneCount = 1;
        public bool onlineOnly = true;
        public override void Initialize()
        {            
            this.IsPopup = false;
            base.Initialize();
        }
        bool GuideShown = false;
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            // If we haven't activated the guide yet, 
            // and it's not up for another purpose, activate it now
            if ((!GuideShown) && (!Guide.IsVisible))
            {
                Guide.ShowSignIn(paneCount, onlineOnly);
                GuideShown = true;
            }
            else if ((GuideShown) && (Guide.IsVisible))
            {
                // If the guide is up, do nothing
            }
            // Screen must have been shown and closed
            else if (!Guide.IsVisible)
            {
                // Activate our callback function and exit
                if (ScreenFinished != null)
                {
                    ScreenFinished.Invoke(this, null);
                }
                ExitScreen();  
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }

    public class NetworkErrorScreen : ErrorScreen
    {
        public NetworkErrorScreen(Exception e) 
            : base(e.Message)
        {
            this.error = "Network Error: " + error + "\n\nPress A to exit";
        }
        public override void HandleInput(InputState input)
        {
            if (input.IsNewButtonPress(Buttons.A))
            {
                ((Game1)this.ScreenManager.Game).FailToMenu();
            }
            base.HandleInput(input);
        }
    }

    public class NetworkSessionManager : DrawableGameComponent
    {
        public NetworkSession networkSession;
        public NetworkSessionState networkSessionState;
        public PacketWriter packetWriter = new PacketWriter();
        public PacketReader packetReader = new PacketReader();
        int maxLocalGamers;
        int maxGamers;

        public NetworkSessionManager(Game game)
            : base(game)
        {
            this.Visible = false;
        }

        public NetworkSessionManager(Game game, int maxLG, int maxG) 
            : base(game)
        {
            this.maxLocalGamers = maxLG;
            this.maxGamers = maxG;
            this.networkSessionState = NetworkSessionState.Ended;
        }

        /// <summary>
        /// Finds a SignedInGamer associated with a controller.
        /// </summary>
        /// <param name="index">The controller to query.</param>
        /// <returns>The SignedInGamer for the controller, or null if none is found.</returns>
        public static SignedInGamer FindGamer(PlayerIndex index)
        {
            foreach (SignedInGamer gamer in SignedInGamer.SignedInGamers)
            {
                if (gamer.PlayerIndex == index)
                    return gamer;
            }
            return null;
        }

        /// <summary>
        /// Event handler for when the asynchronous create network session
        /// operation has completed.
        /// </summary>
        void CreateSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous create network session operation.
                this.networkSession = NetworkSession.EndCreate(e.AsyncResult);

                // Allow Host Migration
                this.networkSession.AllowHostMigration = true;

                // Allow Join in progress
                this.networkSession.AllowJoinInProgress = true;

                this.HookSessionEvents(networkSession);

                this.networkSessionState = NetworkSessionState.Lobby;
                // Go to the lobby screen. We pass null as the controlling player,
                // because the lobby screen accepts input from all local players
                // who are in the session, not just a single controlling player.
                ((Game1)this.Game).DisplayLobby(true);
            }
            catch (Exception exception)
            {
                NetworkErrorScreen errorScreen = new NetworkErrorScreen(exception);

                //ScreenManager.AddScreen(errorScreen, ControllingPlayer);
            }
        }


        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        public void CreateSession(int currentNetworkSetting, int privateGamerSlots, int maxPlayers)
        {
            string errorMessage;
            NetworkSessionType sessionType;

            if (currentNetworkSetting == 0)         //Xbox Live
            {
                sessionType = NetworkSessionType.PlayerMatch;
            }
            else if (currentNetworkSetting == 1)    //SystemLink
            {
                sessionType = NetworkSessionType.SystemLink;
            }
            else
            {
                sessionType = NetworkSessionType.Local;

            }

            if (sessionType != NetworkSessionType.Local)
            {
                // Leave the current network session.
                if (this.networkSession != null)
                {
                    this.networkSession.Dispose();
                    //this.networkSession = null;
                }

                try
                {
                    // Create New Async session
                    IAsyncResult asyncResult = NetworkSession.BeginCreate(sessionType, maxLocalGamers, maxPlayers, privateGamerSlots, null, null, null);
                    //asyncResult.AsyncWaitHandle.WaitOne();
                    // Activate the network busy screen, which will display
                    // an animation until this operation has completed.
                    //game.DisplayNetworkBusyScreen(asyncResult);
                    NetworkBusyScreen busyScreen = new NetworkBusyScreen(asyncResult);

                    ((Game1)this.Game).screenManager.AddScreen(busyScreen);


                    busyScreen.OperationCompleted += CreateSessionOperationCompleted;
                    //asyncResult.AsyncWaitHandle.Close();
           

/*
                    // Create New session
                    this.networkSession = NetworkSession.Create(sessionType, maxLocalGamers, maxGamers);

                    // Allow Host Migration
                    this.networkSession.AllowHostMigration = true;

                    // Allow Join in progress
                    this.networkSession.AllowJoinInProgress = true;

                    this.HookSessionEvents(networkSession);
 */
                }
                catch (Exception error)
                {
                    errorMessage = error.Message;
                }
            }
        }

        public void CloseSession()
        {
            // Leave the current network session.
            if (this.networkSession != null)
            {
                this.networkSession.Dispose();
                this.networkSession = null;
                this.networkSessionState = NetworkSessionState.Ended;
            }
        }

        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        public void JoinSession(int currentNetworkSetting, int maxPlayers)
        {
            string errorMessage;
            NetworkSessionType sessionType;

            if (currentNetworkSetting == 0)    //XboxLive
            {
                /*
                 * 	Uses the Xbox LIVE servers. This enables connection to other machines over the Internet. 
                 * 	It requires a LIVE Silver Membership for Windows-based games or a LIVE Gold membership for Xbox 360 games. 
                 * 	Games in development will also require an XNA Creators Club premium membership. 
                 * 	While in trial mode, Indie games downloaded from Xbox LIVE Markeplace will not have access to LIVE matchmaking. 
                 */
                sessionType = NetworkSessionType.PlayerMatch;
            }
            else if (currentNetworkSetting == 1)    //SystemLink
            {
                /*
                 *  Connect multiple Xbox 360 consoles or computers over a local subnet. 
                 *  These machines do not require a connection to Xbox LIVE or any LIVE accounts. 
                 *  However, connection to machines on different subnets is not allowed.
                 *  If you are a Creators Club developer testing your game, you can use this type to connect an Xbox 360 console to a computer. 
                 *  However, cross-platform networking is not supported in games distributed to non–Creators Club community players.
                 */
                sessionType = NetworkSessionType.SystemLink;
            }
            else
            {
                /*
                 * All session matches are ranked. This option is available only for commercial games that have passed Xbox LIVE certification. 
                 * Due to the competitive nature of the gameplay, this session type does not support join-in-progress.
                 
                sessionType = NetworkSessionType.Ranked;
                */

                sessionType = NetworkSessionType.Local;
            }

            // Leave the current network session.
            if (this.networkSession != null)
            {
                this.networkSession.Dispose();
                this.networkSession = null;
            }

            try
            {
                // Begin an asynchronous find network sessions operation.
                IAsyncResult asyncResult = NetworkSession.BeginFind(sessionType, maxLocalGamers, null, null, null);

                // Activate the network busy screen, which will display
                // an animation until this operation has completed.
                NetworkBusyScreen busyScreen = new NetworkBusyScreen(asyncResult);

                ((Game1)this.Game).screenManager.AddScreen(busyScreen);

                busyScreen.OperationCompleted += FindSessionsOperationCompleted;

/*
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions = NetworkSession.Find(sessionType, maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    this.networkSession = NetworkSession.Join(availableSessions[0]);

                    this.HookSessionEvents(networkSession);
                }
 */
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
        }

        /// <summary>
        /// Event handler for when the asynchronous find network sessions
        /// operation has completed.
        /// </summary>
        void FindSessionsOperationCompleted(object sender,
                                            OperationCompletedEventArgs e)
        {
            GameScreen nextScreen;

            try
            {
                // End the asynchronous find network sessions operation.
                AvailableNetworkSessionCollection availableSessions =
                                                NetworkSession.EndFind(e.AsyncResult);

                if (availableSessions.Count == 0)
                {
                    // If we didn't find any sessions, display an error.
                    availableSessions.Dispose();

                    nextScreen = new MessageBoxScreen(Strings.MatchmakingMenuString, Strings.NoSessionsFound, false);
                }
                else
                {
                    // Join the first session we found.
                    this.networkSession = NetworkSession.Join(availableSessions[0]);

                    this.HookSessionEvents(networkSession);

                    if (this.networkSession.SessionState == NetworkSessionState.Lobby)
                    {
                        this.networkSessionState = NetworkSessionState.Lobby;
                    }
                    else if (this.networkSession.SessionState == NetworkSessionState.Playing)
                    {
                        this.networkSessionState = NetworkSessionState.Playing;
                    }

                    // Go to the lobby screen. We pass null as the controlling player,
                    // because the lobby screen accepts input from all local players
                    // who are in the session, not just a single controlling player.
                    ((Game1)this.Game).DisplayLobby(false);
                }
            }
            catch (Exception exception)
            {
                NetworkErrorScreen errorScreen = new NetworkErrorScreen(exception);
            }
        }

        public GameState Update_SignIn(GameState currentGameState)
        {
            // If no local gamers are signed in, show sign-in screen
            if (Gamer.SignedInGamers.Count < 1)
            {
                Guide.ShowSignIn(maxLocalGamers, false);
            }
            else
            {
                // Local Gamer signed in, move to find sessions
                //currentGameState = GameState.CreateSession;
            }

            return currentGameState;
        }

        /// <summary>
        /// This event handler will be called whenever the game recieves an invite
        /// notification. This can occur when the user accepts an invite that was
        /// sent to them by a friend (pull mode), or if they choose the "Join
        /// Session In Progress" option in their friends screen (push mode).
        /// The handler should leave the current session (if any), then join the
        /// session referred to by the invite. It is not necessary to prompt the
        /// user before doing this, as the Guide will already have taken care of
        /// the necessary confirmations before the invite was delivered to you.
        /// </summary>
        public void InviteAcceptedEventHandler(object sender, InviteAcceptedEventArgs e)
        {
            string errorMessage;

            // Leave the current network session.
            if (this.networkSession != null)
            {
                this.networkSession.Dispose();
                this.networkSession = null;
            }

            try
            {
                // Join a new session in response to the invite.
                this.networkSession = NetworkSession.JoinInvited(maxLocalGamers);

                this.HookSessionEvents(networkSession);
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
        }


        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        public void HookSessionEvents(NetworkSession networkSession)
        {
            this.networkSession.GamerJoined += this.GamerJoinedEventHandler;
            this.networkSession.SessionEnded += this.SessionEndedEventHandler;
        }


        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// We use it to allocate a Tank object, and associate it with the new gamer.
        /// </summary>
        public void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = this.networkSession.AllGamers.IndexOf(e.Gamer);
            //e.Gamer.Tag = new Tank(gamerIndex, Content, screenWidth, screenHeight);
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        public void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            string errorMessage;
            errorMessage = e.EndReason.ToString();

            this.networkSession.Dispose();
            this.networkSession = null;
        }


        /// <summary>
        /// Updates the state of the network session, moving the tanks
        /// around and synchronizing their state over the network.
        /// </summary>
        public void UpdateNetworkSession()
        {
            // Update our locally controlled tanks, and send their
            // latest position data to everyone in the session.
            /*
            if (this.networkSessionState == NetworkSessionState.Playing)
            {
                foreach (LocalNetworkGamer gamer in this.networkSession.LocalGamers)
                {
                    this.UpdateLocalGamer(gamer);
                }
            }
             */

            // Pump the underlying session object.
            if (this.networkSession != null && !this.networkSession.IsDisposed)
                this.networkSession.Update();

            // Make sure the session has not ended.
            if (this.networkSession == null)
                return;

            // Read any packets telling us the positions of remotely controlled tanks.
            /*
            if (this.networkSessionState == NetworkSessionState.Playing)
            {
                foreach (LocalNetworkGamer gamer in this.networkSession.LocalGamers)
                {
                    this.ReadIncomingPackets(gamer);
                }
            }
             */
        }

        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        public void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Look up what tank is associated with this local player.
            //Tank localTank = gamer.Tag as Tank;
            Avatar localGamer = gamer.Tag as Avatar;

            // Update the tank.
            //ReadTankInputs(localTank, gamer.SignedInGamer.PlayerIndex);
            //ReadPlayerInputs(localGamer, gamer.SignedInGamer.PlayerIndex);


            //localTank.Update();

            // Write the tank state into a network packet.
            //packetWriter.Write(localTank.Position);
            //packetWriter.Write(localTank.TankRotation);
            //packetWriter.Write(localTank.TurretRotation);

            packetWriter.Write(localGamer.position);

            // Send the data to everyone in the session.
            gamer.SendData(this.packetWriter, SendDataOptions.InOrder);
        }


        /// <summary>
        /// Helper for reading incoming network packets.
        /// </summary>
        public void ReadIncomingPackets(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(this.packetReader, out sender);

                // Discard packets sent by local gamers: we already know their state!
                if (sender.IsLocal)
                    continue;

                // Look up the tank associated with whoever sent this packet.
                //Tank remoteTank = sender.Tag as Tank;

                Avatar remoteGamer = sender.Tag as Avatar;

                // Read the state of this tank from the network packet.
                //remoteTank.Position = packetReader.ReadVector2();
                //remoteTank.TankRotation = packetReader.ReadSingle();
                //remoteTank.TurretRotation = packetReader.ReadSingle();

                remoteGamer.position = packetReader.ReadVector2();
            }
        }


        public void ProcessIncomingData(GameTime gameTime)
        {
            if (this.networkSession != null && !this.networkSession.IsDisposed)
            {
                // Process Incoming Data
                LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

                // While there are packets to be read
                while (localGamer.IsDataAvailable)
                {
                    // Get the packet
                    NetworkGamer sender;
                    localGamer.ReceiveData(packetReader, out sender);

                    // Ignore the packet if you send it
                    if (!sender.IsLocal)
                    {
                        int player, bulletID, zombieID, tankID;
                        byte i, numcharacter, powerupType, currentgun;
                        double angle, totalGameSeconds;
                        Vector2 position = new Vector2(0,0);
                        Vector2 velocity = new Vector2(0, 0);
                        Boolean isReady, isRemote;
                        String playerName;

                        LocalNetworkGamer server = ((Game1)this.Game).networkSessionManager.networkSession.LocalGamers[0];

                        // Read messagetype from start of packet and call appropiate method
                        MessageType messageType = (MessageType)packetReader.ReadInt32();
                        switch (messageType)
                        {
                            case MessageType.StartGame:
                                CLevel.Level level;
                                CSubLevel.SubLevel sublevel;
                                ((Game1)this.Game).currentGameState = GameState.InGame;
                                int levelSelected = packetReader.ReadByte();
                                int sublevelSelected = packetReader.ReadByte();

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV StartGame: Level " + levelSelected.ToString() + " - Sublevel " + sublevelSelected.ToString() + "\r\n";
#endif

                                switch (levelSelected)
                                {
                                    case 1:
                                        level = CLevel.Level.One;
                                        break;
                                    case 2:
                                        level = CLevel.Level.Two;
                                        break;
                                    case 3:
                                        level = CLevel.Level.Three;
                                        break;
                                    case 4:
                                        level = CLevel.Level.Four;
                                        break;
                                    case 5:
                                        level = CLevel.Level.Five;
                                        break;
                                    case 6:
                                        level = CLevel.Level.Six;
                                        break;
                                    case 7:
                                        level = CLevel.Level.Seven;
                                        break;
                                    case 8:
                                        level = CLevel.Level.Eight;
                                        break;
                                    case 9:
                                        level = CLevel.Level.Nine;
                                        break;
                                    case 10:
                                        level = CLevel.Level.Ten;
                                        break;
                                    default:
                                        level = CLevel.Level.One;
                                        break;
                                }

                                switch (sublevelSelected)
                                {
                                    case 1:
                                        sublevel = CSubLevel.SubLevel.One;
                                        break;
                                    case 2:
                                        sublevel = CSubLevel.SubLevel.Two;
                                        break;
                                    case 3:
                                        sublevel = CSubLevel.SubLevel.Three;
                                        break;
                                    case 4:
                                        sublevel = CSubLevel.SubLevel.Four;
                                        break;
                                    case 5:
                                        sublevel = CSubLevel.SubLevel.Five;
                                        break;
                                    case 6:
                                        sublevel = CSubLevel.SubLevel.Six;
                                        break;
                                    case 7:
                                        sublevel = CSubLevel.SubLevel.Seven;
                                        break;
                                    case 8:
                                        sublevel = CSubLevel.SubLevel.Eight;
                                        break;
                                    case 9:
                                        sublevel = CSubLevel.SubLevel.Nine;
                                        break;
                                    case 10:
                                        sublevel = CSubLevel.SubLevel.Ten;
                                        break;
                                    default:
                                        sublevel = CSubLevel.SubLevel.One;
                                        break;
                                }

                                ((Game1)this.Game).BeginMultiplayerPlayerGame(level, sublevel, true);
                                break;

                            case MessageType.SelectPlayer:
#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV SelectPlayer" + "\r\n";
#endif

                                ((Game1)this.Game).currentGameState = GameState.SelectPlayer;
                                ((Game1)this.Game).screenManager.RemoveScreen(((Game1)this.Game).screenManager.GetScreens()[((Game1)this.Game).screenManager.GetScreens().Length - 1]); //ESTO ES UN MUNYONACO DEL COPON BENDITO
                                ((Game1)this.Game).BeginSelectPlayerScreen(true);
                                break;

                            case MessageType.ToggleReady:
                                player = packetReader.ReadByte();
                                isReady = packetReader.ReadBoolean();
                                ((Game1)this.Game).currentPlayers[player].isReady = isReady;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV ToggleReady: player " + player.ToString() + " - isReady " + isReady.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.ChangeLevel:
                                byte levelnum = packetReader.ReadByte();
                                ((Game1)this.Game).levelSelected = levelnum;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV ChangeLevel: levelnum " + levelnum.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.ChangeCharacter:
                                player = packetReader.ReadByte();
                                numcharacter = packetReader.ReadByte();
                                ((Game1)this.Game).currentPlayers[player].character = numcharacter;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV ChangeCharacter: player " + player.ToString() + " - numcharacter " + numcharacter.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.SendPlayerSlot:
                                List<String> supportList = new List<string>();

                                playerName = packetReader.ReadString();
                                player = packetReader.ReadByte();
                                isRemote = packetReader.ReadBoolean();

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV SendPlayerSlot: playerName " + playerName.ToString() + " - player " + player.ToString() + " - isRemote " + isRemote.ToString() + "\r\n";
#endif

                                if (isRemote == false)
                                {
                                    for (i = 0; i < Gamer.SignedInGamers.Count; i++ )
                                    {
                                        ((Game1)this.Game).InitializePlayers((PlayerIndex)i);
                                        if (Gamer.SignedInGamers[i].Gamertag == playerName)
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    ((Game1)this.Game).currentPlayers[player].Player = ((Game1)this.Game).Main;
                                                    ((Game1)this.Game).currentPlayers[player].Activate(((Game1)this.Game).Main);
                                                    break;
                                                case 1:
                                                    ((Game1)this.Game).currentPlayers[player].Player = ((Game1)this.Game).Player2;
                                                    ((Game1)this.Game).currentPlayers[player].Activate(((Game1)this.Game).Player2);
                                                    break;
                                                case 2:
                                                    ((Game1)this.Game).currentPlayers[player].Player = ((Game1)this.Game).Player3;
                                                    ((Game1)this.Game).currentPlayers[player].Activate(((Game1)this.Game).Player3);
                                                    break;
                                                case 3:
                                                    ((Game1)this.Game).currentPlayers[player].Player = ((Game1)this.Game).Player4;
                                                    ((Game1)this.Game).currentPlayers[player].Activate(((Game1)this.Game).Player4);
                                                    break;
                                                default:
                                                    ((Game1)this.Game).currentPlayers[player].Player = ((Game1)this.Game).Main;
                                                    ((Game1)this.Game).currentPlayers[player].Activate(((Game1)this.Game).Main);
                                                    break;
                                            }
                                        }
                                    }
                                    switch (player)
                                    {
                                        case 0:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Blue;
                                            break;
                                        case 1:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Red;
                                            break;
                                        case 2:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Green;
                                            break;
                                        case 3:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Yellow;
                                            break;
                                        default:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Blue;
                                            break;
                                    }
                                    ((Game1)this.Game).currentPlayers[player].Player.Name = playerName;
                                    ((Game1)this.Game).currentPlayers[player].Player.IsPlaying = true;
                                    ((Game1)this.Game).currentPlayers[player].Player.IsRemote = false;
                                    ((Game1)this.Game).currentPlayers[player].status = ObjectStatus.Active;
                                }
                                else
                                {
                                    Player networkPlayer = new Player(((Game1)this.Game).options, ((Game1)this.Game).audio);
                                    ((Game1)this.Game).currentPlayers[player].Player = networkPlayer;
                                    ((Game1)this.Game).currentPlayers[player].Activate(networkPlayer);
                                    switch (player)
                                    {
                                        case 0:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Blue;
                                            break;
                                        case 1:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Red;
                                            break;
                                        case 2:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Green;
                                            break;
                                        case 3:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Yellow;
                                            break;
                                        default:
                                            ((Game1)this.Game).currentPlayers[player].color = Color.Blue;
                                            break;
                                    }
                                    ((Game1)this.Game).currentPlayers[player].Player.Name = playerName;
                                    ((Game1)this.Game).currentPlayers[player].Player.IsPlaying = true;
                                    ((Game1)this.Game).currentPlayers[player].Player.IsRemote = true;
                                    ((Game1)this.Game).currentPlayers[player].status = ObjectStatus.Active;
                                }

                                break;

                            case MessageType.ReadyToBegin:
                                if (server.IsHost)
                                {
                                    ((Game1)this.Game).networkSessionManager.packetWriter.Write((int)MessageType.BeginToPlay);
                                    server.SendData(((Game1)this.Game).networkSessionManager.packetWriter, SendDataOptions.Reliable);
                                    ((Game1)this.Game).currentGameState = GameState.InGame;
                                }

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV ReadyToBegin \r\n";
#endif
                                break;

                            case MessageType.BeginToPlay:
                                ((Game1)this.Game).currentGameState = GameState.InGame;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV BeginToPlay \r\n";
#endif
                                break;

                            case MessageType.GameplayStatePlaying:
                                ((Game1)this.Game).GamePlayStatus = GameplayState.Playing;
#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay State: Playing \r\n";
#endif
                                break;

                            case MessageType.GameplayStateStageCleared:
                                ((Game1)this.Game).GamePlayStatus = GameplayState.StageCleared;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay State: Stage Cleared \r\n";
#endif
                                break;

                            case MessageType.GameplayStatePause:
                                ((Game1)this.Game).GamePlayStatus = GameplayState.Pause;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay State: Pause \r\n";
#endif
                                break;

                            case MessageType.GameplayStateStartLevel:
                                ((Game1)this.Game).GamePlayStatus = GameplayState.StartLevel;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay State: Start Level \r\n";
#endif
                                break;

                            case MessageType.GameplayStateGameOver:
                                ((Game1)this.Game).GamePlayStatus = GameplayState.GameOver;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay State: GameOver \r\n";
#endif
                                break;

                            case MessageType.GameplayPosition:
                                player = packetReader.ReadByte();
                                position = packetReader.ReadVector2();
                                ((Game1)this.Game).currentPlayers[player].position = position;
                                ((Game1)this.Game).currentPlayers[player].entity.Position = position;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay Position: player " + player.ToString() + " - position " + position.ToString() + "\r\n";
#endif

                                if (server.IsHost)
                                {
                                    //((Game1)this.Game).networkSessionManager.packetWriter.Write((int)MessageType.GameplayPosition);
                                    //((Game1)this.Game).networkSessionManager.packetWriter.Write((byte)player);
                                    //((Game1)this.Game).networkSessionManager.packetWriter.Write((Vector2)position);
                                    //server.SendData(((Game1)this.Game).networkSessionManager.packetWriter, SendDataOptions.InOrder);
                                }
                                break;

                            case MessageType.GameplayAddBullet:
                                player = packetReader.ReadByte();
                                position = packetReader.ReadVector2();
                                totalGameSeconds = packetReader.ReadDouble();
                                angle = packetReader.ReadDouble();
                                ((Game1)this.Game).currentPlayers[player].bullets.Add(new Vector4(position.X, position.Y, (float)totalGameSeconds, (float)angle));

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay AddBullet: player " + player.ToString() + " - position " + position.ToString() + " - totalGameSeconds " + totalGameSeconds.ToString() + " - angle " + angle.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.GameplayPlayerCrashed:
                                player = packetReader.ReadByte();
                                //DestroyPlayer((byte)player);
                                ((Game1)this.Game).currentPlayers[player].lifecounter = 100;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay Player Crached: player " + player.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.GameplayPlayerLifecounter:
                                player = packetReader.ReadByte();
                                int life = packetReader.ReadInt32();
                                ((Game1)this.Game).currentPlayers[player].lifecounter = life;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Gameplay LifeCounter: player " + player.ToString() + " - life " + life.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.ZombieEnemyPosition:
                                zombieID = packetReader.ReadInt32();
                                velocity = packetReader.ReadVector2();
                                position = packetReader.ReadVector2();
                                ((Game1)this.Game).Zombies[zombieID].entity.Velocity = velocity;
                                ((Game1)this.Game).Zombies[zombieID].entity.Position = position;

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV Zombie Position: ZombieID " + zombieID.ToString() + " - velocity " + velocity.ToString() + " - position " + position.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.TankEnemyPosition:
                                tankID = packetReader.ReadInt32();
                                velocity = packetReader.ReadVector2();
                                position = packetReader.ReadVector2();
                                ((Game1)this.Game).Tanks[tankID].entity.Velocity = velocity;
                                ((Game1)this.Game).Tanks[tankID].entity.Position = position;
                                break;

                            case MessageType.ZombieDestroyed:
                                player = packetReader.ReadByte();
                                zombieID = packetReader.ReadInt32();
                                bulletID = packetReader.ReadInt32();
                                currentgun = packetReader.ReadByte();
                                ((Game1)this.Game).ZombieDestroyed(((Game1)this.Game).Zombies[zombieID], (byte)player, (byte)currentgun);
                                try
                                {
                                    ((Game1)this.Game).currentPlayers[player].bullets.RemoveAt(bulletID);
                                }
                                catch (Exception exception)
                                {
                                    NetworkErrorScreen errorScreen = new NetworkErrorScreen(exception);
                                }

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV ZombieDestroyed: player " + player.ToString() + " - zombieID " + zombieID.ToString() + " - bulletID " + bulletID.ToString() + " - currentgun " + currentgun.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.TankDestroyed:
                                player = packetReader.ReadByte();
                                tankID = packetReader.ReadInt32();
                                bulletID = packetReader.ReadInt32();
                                ((Game1)this.Game).TankDestroyed(((Game1)this.Game).Tanks[tankID], (byte)player);
                                try
                                {
                                    ((Game1)this.Game).currentPlayers[player].bullets.RemoveAt(bulletID);
                                }
                                catch (Exception exception)
                                {
                                    NetworkErrorScreen errorScreen = new NetworkErrorScreen(exception);
                                }
                                break;

                            case MessageType.PowerUpAdded:
                                position = packetReader.ReadVector2();
                                powerupType = packetReader.ReadByte();
                                switch (powerupType)
                                {
                                    case 0:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).LivePowerUp, ((Game1)this.Game).heart, position, CPowerUp.Type.live));
                                        break;
                                    case 1:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).ExtraLivePowerUp, ((Game1)this.Game).ExtraLivePowerUp, position, CPowerUp.Type.extralife));
                                        break;
                                    case 2:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).ShotgunAmmoPowerUp, ((Game1)this.Game).shotgunammoUI, position, CPowerUp.Type.shotgun_ammo));
                                        break;
                                    case 3:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).MachinegunAmmoPowerUp, ((Game1)this.Game).pistolammoUI, position, CPowerUp.Type.machinegun_ammo));
                                        break;
                                    case 4:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).FlamethrowerAmmoPowerUp, ((Game1)this.Game).flamethrowerammoUI, position, CPowerUp.Type.flamethrower_ammo));
                                        break;
                                    case 5:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).grenadeammoUI, ((Game1)this.Game).grenadeammoUI, position, CPowerUp.Type.grenadelauncher_ammo));
                                        break;
                                    case 6:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).LivePowerUp, ((Game1)this.Game).heart, position, CPowerUp.Type.speedbuff));
                                        break;
                                    case 7:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).ImmunePowerUp, ((Game1)this.Game).ImmunePowerUp, position, CPowerUp.Type.immunebuff));
                                        break;
                                    default:
                                        ((Game1)this.Game).PowerUpList.Add(new CPowerUp(((Game1)this.Game).LivePowerUp, ((Game1)this.Game).heart, position, CPowerUp.Type.live));
                                        break;
                                }

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV PowerUp Added: position " + position.ToString() + " - poweruptype " + powerupType.ToString() + "\r\n";
#endif
                                break;

                            case MessageType.PowerUpPicked:
                                player = packetReader.ReadByte();
                                powerupType = packetReader.ReadByte();
                                int powerUpID = packetReader.ReadInt32();

                                // ExtraLife
                                if (powerupType == (byte)CPowerUp.Type.extralife)
                                {
                                    ((Game1)this.Game).IncreaseLife((byte)player);
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Live
                                if (powerupType == (byte)CPowerUp.Type.live)
                                {
                                    if (((Game1)this.Game).currentPlayers[player].lifecounter < 100)
                                    {
                                        ((Game1)this.Game).currentPlayers[player].lifecounter += ((Game1)this.Game).PowerUpList[powerUpID].Value;

                                        if (((Game1)this.Game).currentPlayers[player].lifecounter > 100)
                                        {
                                            ((Game1)this.Game).currentPlayers[player].lifecounter = 100;
                                        }
                                    }

                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Machine Gun Ammo
                                if (powerupType == (byte)CPowerUp.Type.machinegun_ammo)
                                {
                                    ((Game1)this.Game).currentPlayers[player].ammo[0] += ((Game1)this.Game).PowerUpList[powerUpID].Value;
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Shotgun Ammo
                                if (powerupType == (byte)CPowerUp.Type.shotgun_ammo)
                                {
                                    ((Game1)this.Game).currentPlayers[player].ammo[1] += ((Game1)this.Game).PowerUpList[powerUpID].Value;
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Grenade launcher Ammo
                                if (powerupType == (byte)CPowerUp.Type.grenadelauncher_ammo)
                                {
                                    ((Game1)this.Game).currentPlayers[player].ammo[2] += ((Game1)this.Game).PowerUpList[powerUpID].Value;
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Flamethrower Ammo
                                if (powerupType == (byte)CPowerUp.Type.flamethrower_ammo)
                                {
                                    ((Game1)this.Game).currentPlayers[player].ammo[3] += ((Game1)this.Game).PowerUpList[powerUpID].Value;
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

                                // Speedbuff
                                if (powerupType == (byte)CPowerUp.Type.speedbuff || powerupType == (byte)CPowerUp.Type.immunebuff)
                                {
                                    //player. += powerup.Value;
                                    ((Game1)this.Game).PowerUpList[powerUpID].status = ObjectStatus.Dying;
                                }

#if DEBUG
                                ((Game1)this.Game).DebugComponent.NetDebugText += "RCV PowerUp Picked: player " + player.ToString() + " - powerupType " + powerupType.ToString() + "\r\n";
#endif
                                break;
                        }
                    }
                }
            }
        }
    }

/*
        /// <summary>
        /// This event handler will be called whenever the game recieves an invite
        /// notification. This can occur when the user accepts an invite that was
        /// sent to them by a friend (pull mode), or if they choose the "Join
        /// Session In Progress" option in their friends screen (push mode).
        /// The handler should leave the current session (if any), then join the
        /// session referred to by the invite. It is not necessary to prompt the
        /// user before doing this, as the Guide will already have taken care of
        /// the necessary confirmations before the invite was delivered to you.
        /// </summary>
        void InviteAcceptedEventHandler(object sender, InviteAcceptedEventArgs e)
        {
            //Draw JOIN from invites!
            //DrawMessage("Joining session from invite...");

            // Leave the current network session.
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }

            try
            {
                // Join a new session in response to the invite.
                networkSession = NetworkSession.JoinInvited(maxLocalGamers);

                WireUpEvents();
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }

        }

        public void Update_SignIn()
        {
            // If no local gamers are signed in, show sign-in screen
            if (Gamer.SignedInGamers.Count < 1)
            {
                Guide.ShowSignIn(maxLocalGamers, false);
            }
            else
            {
                // Local Gamer signed in, move to find sessions
                currentGameState = GameState.CreateSession;
            }
        }

        public void Update_FindSession()
        {
            // Find sessions of the current game
            AvailableNetworkSessionCollection sessions = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

            if (sessions.Count == 0)
            {
                // If no sessions exist, move to the CreateSession game state
                currentGameState = GameState.CreateSession;
            }
            else
            {
                // If a session does exist, join it, wire up events, and move to the Start game state
                networkSession = NetworkSession.Join(sessions[0]);
                WireUpEvents();
                currentGameState = GameState.Start;
            }
        }

        public void Update_CreateSession()
        {
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }

            //networkSession = NetworkSession.Create(NetworkSessionType.PlayerMatch, maxLocalGamers, maxGamers,);

            networkSession.AllowHostMigration = true;
            if (CurrentNetworkSetting != 0) //Si no es local puede entrar jugadores
                networkSession.AllowJoinInProgress = true;

            WireUpEvents();
        }


        public void Update_Start(GameTime gameTime)
        {
            // Get local gamer
            LocalNetworkGamer localgamer = networkSession.LocalGamers[0];

            // Send message to other player that we'are starting
            packetWriter.Write((int)MessageType.StartGame);
            localgamer.SendData(packetWriter, SendDataOptions.Reliable);

            //Call StartGame
            //BeginGame();

            // Process any, incoming packets
            ProcessIncomingData(gameTime);
        }

        protected void ProcessIncomingData(GameTime gameTime)
        {
            // Process Incoming Data
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // While there are packets to be read
            while (localGamer.IsDataAvailable)
            {
                // Get the packet
                NetworkGamer sender;
                localGamer.ReceiveData(packetReader, out sender);

                // Ignore the packet if you send it
                if (!sender.IsLocal)
                {
                    // Read messagetype from start of packet and call appropiate method
                    MessageType messageType = (MessageType)packetReader.ReadInt32();
                    //switch (messageType)
                    //{
                        //TODO!!
                        
                    //}
                }
            }
        }

        protected void WireUpEvents()
        {
            // Wire up events for gamers joining and leaving
            networkSession.GamerJoined += GamerJoined;
            networkSession.GamerLeft += GamerLeft;
        }

        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // Gamer joined. Set the tag for the gamer to a new *userControlledSprite*.
            // If the gamer is the Host, create ....; if not, ....
            if (e.Gamer.IsHost)
            {
                //e.Gamer.Tag = CreateChasingSprite();
            }
            else
            {
                //e.Gamer.Tag = CreateChasedSprite();
            }
        }

        void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            // Dispose of the network session, set it to null.
            // Stop the soundtrack and go back to searching for sessions.
            networkSession.Dispose();
            networkSession = null;

            // Stop music
            //trackInstance.Stop();

            currentGameState = GameState.FindSession;
        }
*/

}
#endif