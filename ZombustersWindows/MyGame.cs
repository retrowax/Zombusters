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
//using Steamworks;

namespace ZombustersWindows
{
    #region Global Variables

    // Represents different states of the game
    public enum GameState { SignIn, FindSession, CreateSession, Start, InGame, GameOver, InLobby, SelectPlayer, WaitingToBegin, Paused }

    // Represents different types of network messages
    public enum MessageType {
        StartGame = 0,
        EndGame = 1,
        RestartGame = 2,
        RejoinLobby = 3,
        UpdatePlayerPos = 4,
        SelectPlayer = 5,
        ChangeLevel = 6,
        ToggleReady = 7,
        ChangeCharacter = 8,
        SendPlayerSlot = 9,
        ReadyToBegin = 10,
        BeginToPlay = 11,
        GameplayStatePlaying = 12,
        GameplayStateStageCleared = 13,
        GameplayStatePause = 14,
        GameplayStateStartLevel = 15,
        GameplayStateGameOver = 16,
        GameplayPosition = 17,
        GameplayAddBullet = 18,
        GameplayPlayerCrashed = 19,
        GameplayPlayerLifecounter = 20,
        ZombieEnemyPosition = 21,
        TankEnemyPosition = 22,
        ZombieDestroyed = 23,
        TankDestroyed = 24,
        PowerUpAdded = 25,
        PowerUpPicked = 26
    }

    public enum GameplayState {
        Playing, StageCleared, Pause, StartLevel, GameOver, NotPlaying
    }

    #endregion

    public struct NeutralInput {
        public Vector2 StickLeftMovement, StickRightMovement;
        public Vector2 Fire;
        public bool DPadLeft, DPadRight, ButtonA, ButtonB, ButtonY, ButtonLT, ButtonRT, ButtonStart, ButtonRB;
    }

    public class MyGame : Game {
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Avatar[] currentPlayers;
        public float TotalGameSeconds;
        public Player Main;
        public Player Player2;
        public Player Player3;
        public Player Player4;
        public ScreenManager screenManager;
        public GamePlayScreen playScreen;
        public static float BackgroundDriftRatePerSec = 64.0f;
        private bool bPaused = false;
        private bool bStateReady = false;
        public OptionsState options;
        public OptionsState Options {
            get { return options; }
        }

        public AudioManager audio;
        public InputManager input;
        //public BloomComponent bloom;
        //int bloomSettingsIndex = 0;
        public String[] NetworkSettings = { "XBOX LIVE", "SYSTEM LINK" };
        public int CurrentNetworkSetting;
        public int maxGamers = 4;
        public int maxLocalGamers = 4;
        public Random rand;
        public List<Texture2D> NoisedMap;
        //public StorageDeviceManager storageDeviceManager;
        public GameState currentGameState;
        
        public Vector2 position = new Vector2(-200, 0);
        public Vector2 position2 = new Vector2(-200, 0);
        public bool directionRight = true;
        public TopScoreListContainer mScores;
#if DEBUG
        //public FrameRateCounter FrameRateComponent;
        //public DebugInfoComponent DebugComponent;
#endif
        public MusicComponent musicComponent;
        public Texture2D LivePowerUp, ExtraLivePowerUp, ShotgunAmmoPowerUp, MachinegunAmmoPowerUp, FlamethrowerAmmoPowerUp, ImmunePowerUp, heart, shotgunammoUI, pistolammoUI, grenadeammoUI, flamethrowerammoUI, blackTexture;
        public bool isInMenu = false;
        SpriteFont MenuInfoFont;

        public MyGame() {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";
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
            Main = new Player(options, audio);
            Player2 = new Player(options, audio);
            Player3 = new Player(options, audio);
            Player4 = new Player(options, audio);
            CurrentNetworkSetting = 0;
            NoisedMap = new List<Texture2D>(4);
            rand = new Random(4);
#if DEBUG
            /*FrameRateComponent = new FrameRateCounter(this);
            Components.Add(FrameRateComponent);
            DebugComponent = new DebugInfoComponent(this);
            Components.Add(DebugComponent);*/
            //Guide.SimulateTrialMode = true;
#endif
            musicComponent = new MusicComponent(this);
            Components.Add(musicComponent);
            musicComponent.Enabled = true;
        }

        protected override void Initialize() {
            //SteamAPI.Init();
            currentPlayers = new Avatar[maxGamers];
            for (int i = 0; i < maxGamers; i++) {
                currentPlayers[i] = new Avatar();
                currentPlayers[i].Initialize(GraphicsDevice.Viewport);
                if (i == 0) {
                    this.InitializeMain(PlayerIndex.One);
                }
            }      
            base.Initialize();
            screenManager.AddScreen(new LogoScreen(this));
            currentGameState = GameState.SignIn;
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            for (int i = 0; i < 4; i++) {
                NoisedMap.Add(new Texture2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color));
            }

            NoisedMap[0].SetData<Color>(CreateTexture.FillNoise(NoisedMap[0].Width, NoisedMap[0].Height, 0.5f));
            NoisedMap[1].SetData<Color>(CreateTexture.FillNoise(NoisedMap[1].Width, NoisedMap[1].Height, 0.4f));
            NoisedMap[2].SetData<Color>(CreateTexture.FillNoise(NoisedMap[2].Width, NoisedMap[2].Height, 0.6f));
            NoisedMap[3].SetData<Color>(CreateTexture.FillNoise(NoisedMap[3].Width, NoisedMap[3].Height, 0.7f));
            MenuInfoFont = screenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            blackTexture = new Texture2D(GraphicsDevice, 1280, 720, false, SurfaceFormat.Color);
            Color[] colors = new Color[1280 * 720];
            for (int j = 0; j < colors.Length; j++) {
                colors[j] = Color.Black;
            }
            blackTexture.SetData(colors);
        }

        protected override void Update(GameTime gameTime) {
            //SteamAPI.RunCallbacks();
            if (currentGameState != GameState.Paused) {
                if (!bPaused && bStateReady) {
                    TotalGameSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    foreach (Avatar cplayer in currentPlayers) {
                        if (cplayer.Player != null) {
                            if (cplayer.Player.IsPlaying) {
                                cplayer.Update(gameTime, TotalGameSeconds);
                            }
                        }
                    }
                }
                base.Update(gameTime);
            }
        }

        public void checkIfControllerChanged(InputState inputCheck) {
            if (inputCheck.IsNewKeyPress(Keys.Enter) || inputCheck.IsNewKeyPress(Keys.Space) || inputCheck.IsNewKeyPress(Keys.Escape) || inputCheck.IsNewKeyPress(Keys.A) ||
                inputCheck.IsNewKeyPress(Keys.S) || inputCheck.IsNewKeyPress(Keys.D) || inputCheck.IsNewKeyPress(Keys.W) || inputCheck.IsNewKeyPress(Keys.Up) ||
                inputCheck.IsNewKeyPress(Keys.Down) || inputCheck.IsNewKeyPress(Keys.Left) || inputCheck.IsNewKeyPress(Keys.Right))
            {
                Main.Options = InputMode.Keyboard;
            }

            if (inputCheck.IsNewButtonPress(Buttons.A) || inputCheck.IsNewButtonPress(Buttons.B) || inputCheck.IsNewButtonPress(Buttons.X) || inputCheck.IsNewButtonPress(Buttons.Y) ||
                inputCheck.IsNewButtonPress(Buttons.Back) || inputCheck.IsNewButtonPress(Buttons.Start) || inputCheck.IsNewButtonPress(Buttons.RightShoulder) ||
                inputCheck.IsNewButtonPress(Buttons.LeftShoulder) || inputCheck.IsNewButtonPress(Buttons.DPadLeft) || inputCheck.IsNewButtonPress(Buttons.DPadRight)
                || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickLeft) || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickRight) || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickDown)
                || inputCheck.IsNewButtonPress(Buttons.LeftThumbstickUp))
            {
                Main.Options = InputMode.GamePad;
            }

            // Read in our gestures
            foreach (GestureSample gesture in inputCheck.GetGestures()) {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap || gesture.GestureType == GestureType.DoubleTap || gesture.GestureType == GestureType.DragComplete ||
                    gesture.GestureType == GestureType.Flick || gesture.GestureType == GestureType.FreeDrag || gesture.GestureType == GestureType.Hold ||
                    gesture.GestureType == GestureType.HorizontalDrag || gesture.GestureType == GestureType.Pinch || gesture.GestureType == GestureType.PinchComplete ||
                    gesture.GestureType == GestureType.VerticalDrag)
                {
                    Main.Options = InputMode.Touch;
                }
            }
        }

        #region Start Games

        public void InitializeMain(PlayerIndex index) {
            if (!Main.IsPlaying) {
                Main.InitLocal(index, Strings.PlayerOneString, InputMode.Keyboard, this);
                LoadOptions(Main);
            }
        }

        public void BeginLocalGame(CLevel.Level level, List<int> PlayersActive) {
            byte i;

            Reset();
            for (i = 0; i < currentPlayers.Length; i++) {
                if (i == 0) {
                    currentPlayers[i].Activate(Main);
                    currentPlayers[i].Player.Controller = PlayerIndex.One;
                    currentPlayers[i].color = Color.Blue;
                } else if (i == 1) {
                    currentPlayers[i].Player = Player2;
                    currentPlayers[i].Player.Controller = PlayerIndex.Two;
                    currentPlayers[i].Activate(Player2);
                    currentPlayers[i].color = Color.Red;
                } else if (i == 2) {
                    currentPlayers[i].Player = Player3;
                    currentPlayers[i].Player.Controller = PlayerIndex.Three;
                    currentPlayers[i].Activate(Player3);
                    currentPlayers[i].color = Color.Green;
                } else {
                    currentPlayers[i].Player = Player4;
                    currentPlayers[i].Player.Controller = PlayerIndex.Four;
                    currentPlayers[i].Activate(Player4);
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
            playScreen = new GamePlayScreen(this, level, CSubLevel.SubLevel.One);
            screenManager.AddScreen(playScreen);
        }

        public void BeginSelectPlayerScreen(Boolean isMatchmaking) {
            int i;
            Reset();
            for (i = 0; i < maxGamers; i++) {
                currentPlayers[i].Player = Player4;
            }
            bStateReady = true;
            currentGameState = GameState.InLobby;
            screenManager.AddScreen(new SelectPlayerScreen(isMatchmaking));
        }

        public void Reset() {
            TotalGameSeconds = 0;
            currentPlayers[0].Reset(Color.Blue);
            currentPlayers[1].Reset(Color.Red);
            currentPlayers[2].Reset(Color.Green);
            currentPlayers[3].Reset(Color.Yellow);
        }

        public void Restart() {
            if (Main.IsPlaying) {
                currentPlayers[0].Restart();
                currentPlayers[0].Activate(Main);
                currentPlayers[0].color = Color.Blue;
            }
            if (Player2.IsPlaying) {
                currentPlayers[1].Restart();
                currentPlayers[1].Activate(Player2);
                currentPlayers[1].color = Color.Red;
            }
            if (Player3.IsPlaying) {
                currentPlayers[2].Restart();
                currentPlayers[2].Activate(Player3);
                currentPlayers[2].color = Color.Green;
            }
            if (Player4.IsPlaying) {
                currentPlayers[3].Restart();
                currentPlayers[3].Activate(Player4);
                currentPlayers[3].color = Color.Yellow;
            }

            TotalGameSeconds = 0;
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
                    screenManager.AddScreen(new OptionsScreen(this, this.Main.optionsState));
                    break;
                case 1:
                    screenManager.AddScreen(new OptionsScreen(this, this.Player2.optionsState));
                    break;
                case 2:
                    screenManager.AddScreen(new OptionsScreen(this, this.Player3.optionsState));
                    break;
                case 3:
                    screenManager.AddScreen(new OptionsScreen(this, this.Player4.optionsState));
                    break;
                default:
                    screenManager.AddScreen(new OptionsScreen(this, this.Main.optionsState));
                    break;
            }
        }

        public void SetOptions(OptionsState state, Player player) {
            this.options = state;
            player.Options = state.Player;
            player.optionsState = state;
            audio.SetOptions(state.FXLevel, state.MusicLevel);
            player.saveOptions();
        }

        public void LoadOptions(Player player) {
            player.loadOptions();
            player.loadLeaderBoard();
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
                Main.BeginPause();
            }
            return IsPaused;
        }

        public bool EndPause() {
            if (bPaused) {
                audio.ResumeAll();
                input.EndPause();
                Main.EndPause();
                bPaused = false;
            }
            return IsPaused;
        }
        #endregion

        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.Black);
            //if (currentGameState == GameState.Paused)
            //{
            //    screenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //    screenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused"), new Vector2(0, 0), Color.White, 4.70f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            //    screenManager.SpriteBatch.End();
            //}
            base.Draw(gameTime);
        }
    }
}
