using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.MainScreens;
using ZombustersWindows.Localization;
using Bugsnag;
using GameAnalyticsSDK.Net;
using Steamworks;

namespace ZombustersWindows
{
    public class MyGame : Game {
        public int VIRTUAL_RESOLUTION_WIDTH = 1280;
        public int VIRTUAL_RESOLUTION_HEIGHT = 720;
        public const int MAX_PLAYERS = 4;
        private const string ANALYTICS_GAME_KEY = "2a9782ff7b0d7b1326cc50178f587678";
        private const string ANALYTICS_SEC_KEY = "8924590c2447e4a6e5335aea11e16f5ff8150d04";
        private const string BUGSNAG_KEY = "1cad9818fb8d84290d776245cd1f948d";


        public GraphicsDeviceManager graphics;
        public ScreenManager screenManager;
        public GamePlayScreen playScreen;
        public Player[] players = new Player[MAX_PLAYERS];
        public Color[] playerColors = new Color[MAX_PLAYERS];
        public OptionsState options;
        public AudioManager audio;
        public InputManager input;
        public GameState currentGameState;
        public InputMode currentInputMode = InputMode.Keyboard;
        public TopScoreListContainer topScoreListContainer;
        public MusicComponent musicComponent;
        public Texture2D blackTexture;
        public Client bugSnagClient;
        public StorageDataSource storageDataSource;
        public float totalGameSeconds;

        //public BloomComponent bloom;
        //public StorageDeviceManager storageDeviceManager;

#if DEBUG
        public FrameRateCounter FrameRateComponent;
        public DebugInfoComponent DebugComponent;
#endif

        //int bloomSettingsIndex = 0;
        
        public String[] networkSettings = { "XBOX LIVE", "SYSTEM LINK" };
        public int currentNetworkSetting;
        public int maxGamers = 4;
        public int maxLocalGamers = 4;
        public bool isInMenu = false;
        private bool bPaused = false;
        private bool bStateReady = false;

        public MyGame() {
            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false
            };
            Resolution.Init(ref graphics);
            Content.RootDirectory = "Content";

            Resolution.SetVirtualResolution(VIRTUAL_RESOLUTION_WIDTH, VIRTUAL_RESOLUTION_HEIGHT);
            Resolution.SetResolution(VIRTUAL_RESOLUTION_WIDTH, VIRTUAL_RESOLUTION_HEIGHT, graphics.IsFullScreen);

            options = new OptionsState();
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            screenManager.TraceEnabled = true;
            audio = new AudioManager(this);
            Components.Add(audio);
            audio.SetOptions(0.7f, 0.5f);
            input = new InputManager(this);
            Components.Add(input);

            bugSnagClient = new Client(BUGSNAG_KEY);
            InitSteamClient();

            storageDataSource = new StorageDataSource(ref bugSnagClient);

            
            currentNetworkSetting = 0;
#if DEBUG
            FrameRateComponent = new FrameRateCounter(this);
            Components.Add(FrameRateComponent);
            DebugComponent = new DebugInfoComponent(this);
            Components.Add(DebugComponent);
#endif
            musicComponent = new MusicComponent(this);
            Components.Add(musicComponent);
            musicComponent.Enabled = true;
        }

        private void InitPlayers()
        {
            players[(int)PlayerIndex.One] = new Player(options, audio, this, Color.Blue, Strings.PlayerOneString, GraphicsDevice.Viewport);
            players[(int)PlayerIndex.Two] = new Player(options, audio, this, Color.Red, Strings.PlayerTwoString, GraphicsDevice.Viewport);
            players[(int)PlayerIndex.Three] = new Player(options, audio, this, Color.Green, Strings.PlayerThreeString, GraphicsDevice.Viewport);
            players[(int)PlayerIndex.Four] = new Player(options, audio, this, Color.Yellow, Strings.PlayerFourString, GraphicsDevice.Viewport);

            for (int playerIndex = 0; playerIndex < MAX_PLAYERS; playerIndex++)
            {
                LoadOptions(players[playerIndex]);
            }
        }

        private void InitSteamClient()
        {
            try
            {
#if DEMO
                SteamClient.Init(1294640);
#else
                SteamClient.Init(1272300);
#endif
            } catch {}
        }

        protected override void Initialize() {
            InitializeMetrics();
            base.Initialize();
            InitPlayers();
            screenManager.AddScreen(new LogoScreen());
            currentGameState = GameState.SignIn;
        }

        protected override void LoadContent() {
            blackTexture = new Texture2D(GraphicsDevice, 1280, 720, false, SurfaceFormat.Color);
            Color[] colors = new Color[1280 * 720];
            for (int j = 0; j < colors.Length; j++) {
                colors[j] = Color.Black;
            }
            blackTexture.SetData(colors);
        }

        protected override void Update(GameTime gameTime) {
            if (currentGameState != GameState.Paused) {
                if (!bPaused && bStateReady) {
                    totalGameSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    foreach (Player player in players) {
                        if (player.IsPlaying)
                        {
                            player.avatar.Update(gameTime, totalGameSeconds);
                        }
                    }
                }
                base.Update(gameTime);
            }
        }

#region Start Games


        public void BeginLocalGame(LevelType level, List<int> PlayersActive) {
            byte i;

            Reset();
            /*
            for (i = 0; i < avatars.Length; i++) {
                if (i == 0) {
                    avatars[i].Activate(player1);
                    avatars[i].Player.Controller = PlayerIndex.One;
                    avatars[i].color = Color.Blue;
                } else if (i == 1) {
                    avatars[i].Player = player2;
                    avatars[i].Player.Controller = PlayerIndex.Two;
                    avatars[i].Activate(player2);
                    avatars[i].color = Color.Red;
                } else if (i == 2) {
                    avatars[i].Player = player3;
                    avatars[i].Player.Controller = PlayerIndex.Three;
                    avatars[i].Activate(player3);
                    avatars[i].color = Color.Green;
                } else if (i == 3){
                    avatars[i].Player = player4;
                    avatars[i].Player.Controller = PlayerIndex.Four;
                    avatars[i].Activate(player4);
                    avatars[i].color = Color.Yellow;
                } else
                {
                    avatars[i].Activate(player1);
                    avatars[i].Player.Controller = PlayerIndex.One;
                    avatars[i].color = Color.Blue;
                }
                avatars[i].Player.IsRemote = true;
                if (PlayersActive.Contains(i)) {
                    avatars[i].Player.IsPlaying = true;
                    avatars[i].status = ObjectStatus.Active;
                } else {
                    avatars[i].Player.IsPlaying = false;
                    avatars[i].status = ObjectStatus.Inactive;
                }
            }
            */
            bStateReady = true;
            this.audio.StopMenuMusic();
            currentGameState = GameState.InGame;
            playScreen = new GamePlayScreen(this, level, SubLevel.SubLevelType.One);
            screenManager.AddScreen(playScreen);
        }

        public void BeginSelectPlayerScreen(Boolean isMatchmaking) {
            Reset();
            bStateReady = true;
            currentGameState = GameState.InLobby;
            screenManager.AddScreen(new SelectPlayerScreen(isMatchmaking));
        }

        public void Reset() {
            totalGameSeconds = 0;
            for (int playerIndex = 0; playerIndex < MAX_PLAYERS; playerIndex++)
            {
                players[playerIndex].avatar.Reset();
            }
            /*avatars[0].Reset(Color.Blue);
            avatars[1].Reset(Color.Red);
            avatars[2].Reset(Color.Green);
            avatars[3].Reset(Color.Yellow);
            avatars[0].Player = player1;
            avatars[1].Player = player2;
            avatars[2].Player = player3;
            avatars[3].Player = player4;*/
        }

        public void Restart() {
            for (int playerIndex = 0; playerIndex < MAX_PLAYERS; playerIndex++)
            {
                if (players[playerIndex].IsPlaying)
                {
                    players[playerIndex].avatar.Restart();
                }
            }
            totalGameSeconds = 0;
        }

#endregion

#region Extras Menu

        public void DisplayHowToPlay() {
            screenManager.AddScreen(new HowToPlayScreen());
        }

        public void DisplayLeaderBoard() {
            screenManager.AddScreen(new LeaderBoardScreen());
        }

        public void DisplayExtrasMenu() {
            screenManager.AddScreen(new ExtrasMenuScreen());
        }

        public void DisplayCredits() {
            screenManager.AddScreen(new CreditsScreen(true));
        }

        public void DisplayAchievementsScreen(Player player) {
            screenManager.AddScreen(new AchievementsScreen(player));
        }

#endregion

        public void TrySignIn(bool isSignedInGamer, EventHandler handler) {
            LoadInScreen screen = new LoadInScreen(1, false);
            screen.ScreenFinished += new EventHandler(handler);
            screenManager.AddScreen(screen);
        }

        public void FailToMenu() {
            foreach (GameScreen item in screenManager.GetScreens()) {
                screenManager.RemoveScreen(item);
            }
            QuitGame();
        }

        public void QuitGame() {
            playScreen = null;
            Reset();
            bPaused = EndPause();
        }

#region Setting Options
        public void DisplayOptions(int player) {
            screenManager.AddScreen(new OptionsScreen(this, players[player]));
        }

        public void SetOptions(OptionsState state, Player player) {
            this.options = state;
            player.optionsState = state;
            audio.SetOptions(state.FXLevel, state.MusicLevel);
            player.SaveOptions();
        }

        public void LoadOptions(Player player) {
            player.LoadOptions();
            player.LoadLeaderBoard();
        }

#endregion

#region Pausing
        public bool IsPaused {
            get { return bPaused; }
        }

        public bool BeginPause() {
            if (!bPaused)
            {
                bPaused = true;
                audio.PauseSounds();
                input.BeginPause();
                for (int playerIndex = 0; playerIndex < MAX_PLAYERS; playerIndex++)
                {
                    players[playerIndex].BeginPause();
                }
            }
            return IsPaused;
        }

        public bool EndPause() {
            if (bPaused) {
                audio.ResumeAll();
                input.EndPause();
                for (int playerIndex = 0; playerIndex < MAX_PLAYERS; playerIndex++)
                {
                    players[playerIndex].EndPause();
                }
                bPaused = false;
            }
            return IsPaused;
        }
#endregion

        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

        private void InitializeMetrics()
        {
#if DEBUG
            GameAnalytics.SetEnabledInfoLog(true);
            GameAnalytics.SetEnabledVerboseLog(true);
#endif
            GameAnalytics.ConfigureBuild("windows 1.1.0");
            GameAnalytics.Initialize(ANALYTICS_GAME_KEY, ANALYTICS_SEC_KEY);
            GameAnalytics.AddDesignEvent("GameStart", 1);
        }
    }
}
