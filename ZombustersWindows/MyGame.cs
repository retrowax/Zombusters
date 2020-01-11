using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.MainScreens;
using ZombustersWindows.Localization;
using Bugsnag.Clients;

namespace ZombustersWindows
{
    public class MyGame : Game {
        public GraphicsDeviceManager graphics;
        public ScreenManager screenManager;
        public GamePlayScreen playScreen;
        public Avatar[] currentPlayers;
        public Player player1;
        public Player player2;
        public Player player3;
        public Player player4;
        public OptionsState options;
        public AudioManager audio;
        public InputManager input;
        public GameState currentGameState;
        public TopScoreListContainer topScoreListContainer;
        public MusicComponent musicComponent;
        public Texture2D blackTexture;
        public BaseClient bugSnagClient;
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
            Resolution.SetVirtualResolution(1280, 720);
            Resolution.SetResolution(1680, 1050, false);
            options = new OptionsState();
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            screenManager.TraceEnabled = true;
            audio = new AudioManager(this);
            Components.Add(audio);
            audio.SetOptions(0.7f, 0.5f);
            input = new InputManager(this);
            Components.Add(input);
            //Bloom Component
            //REVISAR!!!
            //graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;            
            /*
            bloom = new BloomComponent(this);
            Components.Add(bloom);
            bloom.Settings = BloomSettings.PresetSettings[6];
            bloom.Visible = true;
            */
            player1 = new Player(options, audio, this);
            player2 = new Player(options, audio, this);
            player3 = new Player(options, audio, this);
            player4 = new Player(options, audio, this);
            currentNetworkSetting = 0;
#if DEBUG
            FrameRateComponent = new FrameRateCounter(this);
            Components.Add(FrameRateComponent);
            DebugComponent = new DebugInfoComponent(this);
            Components.Add(DebugComponent);
            //Guide.SimulateTrialMode = true;
#endif
            musicComponent = new MusicComponent(this);
            Components.Add(musicComponent);
            musicComponent.Enabled = true;

            bugSnagClient = new BaseClient("1cad9818fb8d84290d776245cd1f948d");
            bugSnagClient.StartAutoNotify();
        }

        protected override void Initialize() {
            currentPlayers = new Avatar[maxGamers];
            for (int i = 0; i < maxGamers; i++) {
                currentPlayers[i] = new Avatar();
                currentPlayers[i].Initialize(GraphicsDevice.Viewport);
                if (i == 0) {
                    this.InitializeMain(PlayerIndex.One);
                }
            }      
            base.Initialize();
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
                    foreach (Avatar cplayer in currentPlayers) {
                        if (cplayer.Player != null) {
                            if (cplayer.Player.IsPlaying) {
                                cplayer.Update(gameTime, totalGameSeconds);
                            }
                        }
                    }
                }
                base.Update(gameTime);
            }
        }

        public void CheckIfControllerChanged(InputState inputCheck) {
            if (inputCheck.IsNewKeyPress(Keys.Enter) || inputCheck.IsNewKeyPress(Keys.Space) || inputCheck.IsNewKeyPress(Keys.Escape) || inputCheck.IsNewKeyPress(Keys.A) ||
                inputCheck.IsNewKeyPress(Keys.S) || inputCheck.IsNewKeyPress(Keys.D) || inputCheck.IsNewKeyPress(Keys.W) || inputCheck.IsNewKeyPress(Keys.Up) ||
                inputCheck.IsNewKeyPress(Keys.Down) || inputCheck.IsNewKeyPress(Keys.Left) || inputCheck.IsNewKeyPress(Keys.Right))
            {
                player1.Options = InputMode.Keyboard;
            }

            if (inputCheck.IsNewButtonPress(Buttons.A) || inputCheck.IsNewButtonPress(Buttons.B) || inputCheck.IsNewButtonPress(Buttons.X) || inputCheck.IsNewButtonPress(Buttons.Y) ||
                inputCheck.IsNewButtonPress(Buttons.Back) || inputCheck.IsNewButtonPress(Buttons.Start) || inputCheck.IsNewButtonPress(Buttons.RightShoulder) ||
                inputCheck.IsNewButtonPress(Buttons.LeftShoulder) || inputCheck.IsNewButtonPress(Buttons.DPadLeft) || inputCheck.IsNewButtonPress(Buttons.DPadRight)
                || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickLeft) || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickRight) || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickDown)
                || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickUp))
            {
                player1.Options = InputMode.GamePad;
            }

            foreach (GestureSample gesture in inputCheck.GetGestures()) {
                if (gesture.GestureType == GestureType.Tap || gesture.GestureType == GestureType.DoubleTap || gesture.GestureType == GestureType.DragComplete ||
                    gesture.GestureType == GestureType.Flick || gesture.GestureType == GestureType.FreeDrag || gesture.GestureType == GestureType.Hold ||
                    gesture.GestureType == GestureType.HorizontalDrag || gesture.GestureType == GestureType.Pinch || gesture.GestureType == GestureType.PinchComplete ||
                    gesture.GestureType == GestureType.VerticalDrag)
                {
                    player1.Options = InputMode.Touch;
                }
            }
        }

        #region Start Games

        public void InitializeMain(PlayerIndex index) {
            if (!player1.IsPlaying) {
                player1.InitLocal(index, Strings.PlayerOneString, InputMode.Keyboard, this);
                LoadOptions(player1);
            }
        }

        public void BeginLocalGame(LevelType level, List<int> PlayersActive) {
            byte i;

            Reset();
            for (i = 0; i < currentPlayers.Length; i++) {
                if (i == 0) {
                    currentPlayers[i].Activate(player1);
                    currentPlayers[i].Player.Controller = PlayerIndex.One;
                    currentPlayers[i].color = Color.Blue;
                } else if (i == 1) {
                    currentPlayers[i].Player = player2;
                    currentPlayers[i].Player.Controller = PlayerIndex.Two;
                    currentPlayers[i].Activate(player2);
                    currentPlayers[i].color = Color.Red;
                } else if (i == 2) {
                    currentPlayers[i].Player = player3;
                    currentPlayers[i].Player.Controller = PlayerIndex.Three;
                    currentPlayers[i].Activate(player3);
                    currentPlayers[i].color = Color.Green;
                } else {
                    currentPlayers[i].Player = player4;
                    currentPlayers[i].Player.Controller = PlayerIndex.Four;
                    currentPlayers[i].Activate(player4);
                    currentPlayers[i].color = Color.Yellow;
                }
                currentPlayers[i].Player.IsRemote = true;
                if (PlayersActive.Contains(i)) {
                    currentPlayers[i].Player.IsPlaying = true;
                    currentPlayers[i].status = ObjectStatus.Active;
                } else {
                    currentPlayers[i].Player.IsPlaying = false;
                    currentPlayers[i].status = ObjectStatus.Inactive;
                }
            }
            bStateReady = true;
            this.audio.StopMenuMusic();
            currentGameState = GameState.InGame;
            playScreen = new GamePlayScreen(this, level, SubLevel.SubLevelType.One);
            screenManager.AddScreen(playScreen);
        }

        public void BeginSelectPlayerScreen(Boolean isMatchmaking) {
            int i;
            Reset();
            for (i = 0; i < maxGamers; i++) {
                currentPlayers[i].Player = player4;
            }
            bStateReady = true;
            currentGameState = GameState.InLobby;
            screenManager.AddScreen(new SelectPlayerScreen(isMatchmaking));
        }

        public void Reset() {
            totalGameSeconds = 0;
            currentPlayers[0].Reset(Color.Blue);
            currentPlayers[1].Reset(Color.Red);
            currentPlayers[2].Reset(Color.Green);
            currentPlayers[3].Reset(Color.Yellow);
        }

        public void Restart() {
            if (player1.IsPlaying) {
                currentPlayers[0].Restart();
                currentPlayers[0].Activate(player1);
                currentPlayers[0].color = Color.Blue;
            }
            if (player2.IsPlaying) {
                currentPlayers[1].Restart();
                currentPlayers[1].Activate(player2);
                currentPlayers[1].color = Color.Red;
            }
            if (player3.IsPlaying) {
                currentPlayers[2].Restart();
                currentPlayers[2].Activate(player3);
                currentPlayers[2].color = Color.Green;
            }
            if (player4.IsPlaying) {
                currentPlayers[3].Restart();
                currentPlayers[3].Activate(player4);
                currentPlayers[3].color = Color.Yellow;
            }

            totalGameSeconds = 0;
            playScreen.bGameOver = false;
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
            InitializeMain(PlayerIndex.One);
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
            //bloom.Visible = true;
            Reset();
            bPaused = EndPause();
            //GamePlayStatus = GameplayState.StartLevel;
            screenManager.AddScreen(new MenuScreen());
        }

        #region Setting Options
        public void DisplayOptions(int player) {
            this.InitializeMain((PlayerIndex)player);
            switch (player) {
                case 0:
                    screenManager.AddScreen(new OptionsScreen(this, this.player1.optionsState));
                    break;
                case 1:
                    screenManager.AddScreen(new OptionsScreen(this, this.player2.optionsState));
                    break;
                case 2:
                    screenManager.AddScreen(new OptionsScreen(this, this.player3.optionsState));
                    break;
                case 3:
                    screenManager.AddScreen(new OptionsScreen(this, this.player4.optionsState));
                    break;
                default:
                    screenManager.AddScreen(new OptionsScreen(this, this.player1.optionsState));
                    break;
            }
        }

        public void SetOptions(OptionsState state, Player player) {
            this.options = state;
            player.Options = state.Player;
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
            if (!bPaused) {
                bPaused = true;
                audio.PauseSounds();
                input.BeginPause();
                player1.BeginPause();
            }
            return IsPaused;
        }

        public bool EndPause() {
            if (bPaused) {
                audio.ResumeAll();
                input.EndPause();
                player1.EndPause();
                bPaused = false;
            }
            return IsPaused;
        }
        #endregion

        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
    }
}
