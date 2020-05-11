using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.MainScreens;
using ZombustersWindows.Localization;
using System.Xml.Linq;
using ZombustersWindows.GameObjects;
using GameAnalyticsSDK.Net;

namespace ZombustersWindows
{
    public class GamePlayScreen : BackgroundScreen
    {
        private const string FRAME_WIDTH = "FrameWidth";
        private const string FRAME_HEIGHT = "FrameHeight";
        private const string SHEET_COLUMNS = "SheetColumns";
        private const string SHEET_ROWS = "SheetRows";
        private const string SPEED = "Speed";
        private const int MACHINEGUN_RATE_OF_FIRE = 10;
        private const int FLAMETHROWER_RATE_OF_FIRE = 15;


        readonly MyGame game;
        Rectangle uiBounds;
        MouseState mouseState;
        readonly InputState input = new InputState();
        private readonly GamePlayMenu menu = null;
        private readonly GameOverMenu gomenu = null;
        private bool bPaused = false;
        public Vector2 accumMove, accumFire;
        public Texture2D Map;
        public List<Player> PlayersInSession;
        public Level Level;
        private LevelType currentLevel;
        private SubLevel.SubLevelType currentSublevel;

        Texture2D bullet;
        Vector2 bulletorigin;
        Texture2D flamethrowerTexture;
        Animation flamethrowerAnimation;
        List<Texture2D> IdleTrunkTexture, IdleLegsTexture, DiedTexture;
        List<Texture2D> RunEastTexture;
        List<Texture2D> PistolShotEastTexture, PistolShotNETexture, PistolShotSETexture, PistolShotNorthTexture, PistolShotSouthTexture;
        List<Texture2D> ShotgunEastTexture, ShotgunNETexture, ShotgunSETexture, ShotgunNorthTexture, ShotgunSouthTexture;
        List<Vector2> IdleTrunkOrigin, RunEastOrigin;
        List<Vector2> PistolShotEastOrigin, PistolShotNEOrigin, PistolShotSEOrigin, PistolShotNorthOrigin, PistolShotSouthOrigin;
        List<Vector2> ShotgunShotEastOrigin, ShotgunNEOrigin, ShotgunSEOrigin, ShotgunNorthOrigin, ShotgunSouthOrigin;
        List<Animation> IdleTrunkAnimation;
        List<Animation> RunEastAnimation;
        List<Animation> PistolShotEastAnimation, PistolShotNEAnimation, PistolShotSEAnimation, PistolShotNorthAnimation, PistolShotSouthAnimation;
        List<Animation> ShotgunShotEastAnimation, ShotgunNEAnimation, ShotgunSEAnimation, ShotgunNorthAnimation, ShotgunSouthAnimation;
#if DEBUG
        Texture2D PositionReference;
#endif
        Texture2D CharacterShadow;
        Texture2D cursorTexture;
        private Vector2 cursorPos;
        Texture2D UIStats, jadeUI, rayUI, peterUI, richardUI, whiteLine;
        Texture2D UIStatsBlue, UIStatsRed, UIStatsGreen, UIStatsYellow, UIPlayerBlue, UIPlayerRed, UIPlayerGreen, UIPlayerYellow;
        SpriteFont arcade14, arcade28;
        SpriteFont MenuHeaderFont, MenuInfoFont, MenuListFont;
        Texture2D pause_icon;
        Texture2D left_thumbstick;
        Texture2D right_thumbstick;

        public Random random = new Random(16);
        private float timer, timerplayer;
        private int subLevelIndex;

        public Enemies enemies;
        public GameplayState GamePlayStatus = GameplayState.NotPlaying;

        public GamePlayScreen(MyGame game, LevelType startingLevel, SubLevel.SubLevelType startingSublevel)
            : base()
        {
            this.game = game;
            enemies = new Enemies(ref game, ref random);
            this.currentLevel = startingLevel;
            this.currentSublevel = startingSublevel;
#if DEBUG
            //this.currentSublevel = CSubLevel.SubLevel.Ten;
#endif
            menu = new GamePlayMenu();
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(GameplayMenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(GameplayMenuCanceled);
            gomenu = new GameOverMenu();
            gomenu.GameOverMenuOptionSelected += new EventHandler<MenuSelection>(GameOverMenuOptionSelected);
        }

        private int activeSeekers;
        public int ActiveSeekers
        {
            get { return activeSeekers; }
            set { activeSeekers = value; }
        }

        void GameplayMenuCanceled(Object sender, MenuSelection selection)
        {
            // Do nothing, game will resume on its own
        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            QuitToMenu();
        }

        void GameplayMenuOptionSelected(Object sender, MenuSelection selection)
        {
            switch (selection.Selection)
            {
                case 0: // Resume
                    break;
                case 1: // Help
                    GameAnalytics.AddDesignEvent("Gameplay:Menu:HowToPlay");
                    ScreenManager.AddScreen(new HowToPlayScreen());
                    break;

                case 2: // Options
                    GameAnalytics.AddDesignEvent("Gameplay:Menu:Options");
                    game.DisplayOptions(0);
                    break;

                case 3: // Restart
                    GameAnalytics.AddDesignEvent("Gameplay:Menu:RestartGame");
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, false);
                    break;

                case 4: // Quit
                    GameAnalytics.AddDesignEvent("Gameplay:Menu:QuitGame");
                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.GPReturnToMainMenuString, Strings.ConfirmReturnMainMenuString);
                    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
                    ScreenManager.AddScreen(confirmExitMessageBox);
                    break;
                default:
                    break;
            }
        }

        void GameOverMenuOptionSelected(Object sender, MenuSelection selection)
        {
            switch (selection.Selection)
            {
                case 0: // Restart WAVE
                    GameAnalytics.AddDesignEvent("Gameplay:GameOverMenu:RestartWave");
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, true);
                    break;
                case 1: // Restart LEVEL
                    GameAnalytics.AddDesignEvent("Gameplay:GameOverMenu:RestartLevel");
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, false);
                    break;
                case 2: // Quit
                    GameAnalytics.AddDesignEvent("Gameplay:GameOverMenu:QuitGame");
                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.GPReturnToMainMenuString, Strings.ConfirmReturnMainMenuString);
                    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
                    ScreenManager.AddScreen(confirmExitMessageBox);
                    break;
                default:
                    break;
            }
        }

        public void QuitToMenu()
        {
            foreach (Player player in game.players)
            {
                player.avatar.Reset();
                player.isReady = false;
            }
            GameScreen[] screenList = ScreenManager.GetScreens();
            for (int i = screenList.Length -1; i > 0; i--)
            {
                screenList[i].ExitScreen();
            }
        }

        public override void Initialize()
        {
            GamePlayStatus = GameplayState.StartLevel;
            uiBounds = GetTitleSafeArea();
            Level = new Level(currentLevel);
            Level.Initialize(game);

            StartNewLevel(true, true);
            base.Initialize();
        }

        public override void LoadContent()
        {
            XDocument animationDefinitionDocument = XDocument.Load("Content/AnimationDef.xml");

            DiedTexture = new List<Texture2D>
            {
                game.Content.Load<Texture2D>(@"InGame/Jade/girl_died"),
                game.Content.Load<Texture2D>(@"InGame/Egon/egon_died"),
                game.Content.Load<Texture2D>(@"InGame/Ray/ray_died"),
                game.Content.Load<Texture2D>(@"InGame/Peter/peter_died")
            };

            JadeIdleTrunkAnimationLoad(animationDefinitionDocument);
            JadeRunEastAnimationLoad(animationDefinitionDocument);
            JadePistolShotEastAnimationLoad(animationDefinitionDocument);
            JadePistolShotNorthEastAnimationLoad(animationDefinitionDocument);
            JadePistolShotSouthEastAnimationLoad(animationDefinitionDocument);
            JadePistolShotSouthAnimationLoad(animationDefinitionDocument);
            JadePistolShotNorthAnimationLoad(animationDefinitionDocument);
            JadeShotgunShotEastAnimationLoad(animationDefinitionDocument);
            JadeShotgunShotNorthEastAnimationLoad(animationDefinitionDocument);
            JadeShotgunShotSouthEastAnimationLoad(animationDefinitionDocument);
            JadeShotgunShotSouthAnimationLoad(animationDefinitionDocument);
            JadeShotgunShotNorthAnimationLoad(animationDefinitionDocument);

            EgonIdleTrunkAnimationLoad(animationDefinitionDocument);
            EgonRunEastAnimationLoad(animationDefinitionDocument);
            EgonPistolShotEastAnimationLoad(animationDefinitionDocument);
            EgonPistolShotNorthEastAnimationLoad(animationDefinitionDocument);
            EgonPistolShotSouthEastAnimationLoad(animationDefinitionDocument);
            EgonPistolShotSouthAnimationLoad(animationDefinitionDocument);
            EgonPistolShotNorthAnimationLoad(animationDefinitionDocument);
            EgonShotgunShotEastAnimationLoad(animationDefinitionDocument);
            EgonShotgunShotNorthEastAnimationLoad(animationDefinitionDocument);
            EgonShotgunShotSouthEastAnimationLoad(animationDefinitionDocument);
            EgonShotgunShotSouthAnimationLoad(animationDefinitionDocument);
            EgonShotgunShotNorthAnimationLoad(animationDefinitionDocument);

            RayIdleTrunkAnimationLoad(animationDefinitionDocument);
            RayRunEastAnimationLoad(animationDefinitionDocument);
            RayPistolShotEastAnimationLoad(animationDefinitionDocument);
            RayPistolShotNorthEastAnimationLoad(animationDefinitionDocument);
            RayPistolShotSouthEastAnimationLoad(animationDefinitionDocument);
            RayPistolShotSouthAnimationLoad(animationDefinitionDocument);
            RayPistolShotNorthAnimationLoad(animationDefinitionDocument);
            RayShotgunShotEastAnimationLoad(animationDefinitionDocument);
            RayShotgunShotNorthEastAnimationLoad(animationDefinitionDocument);
            RayShotgunShotSouthEastAnimationLoad(animationDefinitionDocument);
            RayShotgunShotSouthAnimationLoad(animationDefinitionDocument);
            RayShotgunShotNorthAnimationLoad(animationDefinitionDocument);

            PeterIdleTrunkAnimationLoad(animationDefinitionDocument);
            PeterRunEastAnimationLoad(animationDefinitionDocument);
            PeterPistolShotEastAnimationLoad(animationDefinitionDocument);
            PeterPistolShotNorthEastAnimationLoad(animationDefinitionDocument);
            PeterPistolShotSouthEastAnimationLoad(animationDefinitionDocument);
            PeterPistolShotSouthAnimationLoad(animationDefinitionDocument);
            PeterPistolShotNorthAnimationLoad(animationDefinitionDocument);
            PeterShotgunShotEastAnimationLoad(animationDefinitionDocument);
            PeterShotgunShotNorthEastAnimationLoad(animationDefinitionDocument);
            PeterShotgunShotSouthEastAnimationLoad(animationDefinitionDocument);
            PeterShotgunShotSouthAnimationLoad(animationDefinitionDocument);
            PeterShotgunShotNorthAnimationLoad(animationDefinitionDocument);

            FlamethrowerAnimationLoad(animationDefinitionDocument);

            FontsLoad();
            UIStatsLoad();
            UIComponentsLoad();
            enemies.LoadContent(game.Content);
            FurnitureLoad();

            base.LoadContent();
        }

        #region Input Processing
        public override void HandleInput(InputState input)
        {
            foreach (Player player in game.players)
            {
                if (player.IsPlaying)
                {
                    player.neutralInput = ProcessPlayer(player, input);
                }
            }

            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Pause Game
                    if ((gesture.Position.X >= 1088 && gesture.Position.X <= 1135) &&
                        (gesture.Position.Y >= 39 && gesture.Position.Y <= 83))
                    {
                        if (!bPaused && (GamePlayStatus != GameplayState.StartLevel && GamePlayStatus != GameplayState.StageCleared))
                        {
                            this.ScreenManager.AddScreen(menu);

                            // Use this to keep from adding more than one menu to the stack
                            bPaused = game.BeginPause();
                            GamePlayStatus = GameplayState.Pause;
                        }
                    }
                }
            }
            mouseState = Mouse.GetState();
            base.HandleInput(input);
        }

        private NeutralInput ProcessPlayer(Player player, InputState input)
        {
            NeutralInput state = new NeutralInput
            {
                GamePadFire = Vector2.Zero
            };
            Vector2 stickLeft = Vector2.Zero;
            Vector2 stickRight = Vector2.Zero;
            GamePadState gpState = input.GetCurrentGamePadStates()[(int)player.playerIndex];

            // Get gamepad state
            if (VirtualThumbsticks.LeftThumbstick != Vector2.Zero || VirtualThumbsticks.RightThumbstick != Vector2.Zero)
            {
                stickLeft = VirtualThumbsticks.LeftThumbstick;
                stickRight = VirtualThumbsticks.RightThumbstick;
                state.GamePadFire = VirtualThumbsticks.RightThumbstick;
            }
            else
            {
                stickLeft = gpState.ThumbSticks.Left;
                stickRight = gpState.ThumbSticks.Right;
                //state.Fire = (gpState.Triggers.Right > 0);

                state.GamePadFire = gpState.ThumbSticks.Right;
            }

            if (player.inputMode == InputMode.Keyboard)
            {
                if (input.IsNewKeyPress(Keys.Left))
                {
                    stickLeft += new Vector2(-1, 0);
                }
                if (input.IsNewKeyPress(Keys.Right))
                {
                    stickLeft += new Vector2(1, 0);
                }
                if (input.IsNewKeyPress(Keys.Up))
                {
                    stickLeft += new Vector2(0, 1);
                }
                if (input.IsNewKeyPress(Keys.Down))
                {
                    stickLeft += new Vector2(0, -1);
                }

                if (input.GetCurrentMouseState().LeftButton == ButtonState.Pressed)
                {
                    MouseState mouseState = input.GetCurrentMouseState();

                    if (mouseState.X > player.avatar.position.X && (mouseState.X - player.avatar.position.X >=100))
                    {
                        state.MouseFire += new Vector2(1, 0); // Right
                    }

                    if (mouseState.Y < player.avatar.position.Y && (player.avatar.position.Y - mouseState.Y >= 100))
                    {
                        state.MouseFire += new Vector2(0, 1); // Down
                    }

                    if (mouseState.X < player.avatar.position.X && (player.avatar.position.X - mouseState.X >= 100))
                    {
                        state.MouseFire += new Vector2(-1, 0); // Left
                    }

                    if (mouseState.Y >= player.avatar.position.Y && (mouseState.Y - player.avatar.position.Y >= 100))
                    {
                        state.MouseFire += new Vector2(0, -1); // Top
                    }
                }
            }

            if (input.IsNewButtonPress(Buttons.Y, player.playerIndex)
                || input.IsNewKeyPress(Keys.LeftControl)
                || input.IsNewKeyPress(Keys.E)
                || input.GetCurrentMouseState().RightButton == ButtonState.Pressed)
            {
                state.ButtonY = true;
            }
            else
            {
                state.ButtonY = false;
            }

            if (input.IsNewButtonPress(Buttons.RightShoulder, player.playerIndex)
                || input.IsNewKeyPress(Keys.Tab)
                || input.GetCurrentMouseState().MiddleButton == ButtonState.Pressed)
            {
                state.ButtonRB = true;
            }
            else
            {
                state.ButtonRB = false;
            }

            state.StickLeftMovement = stickLeft;
            state.StickRightMovement = stickRight;
            return state;
        }
        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            cursorPos = new Vector2(mouseState.X, mouseState.Y);
            input.Update();

            foreach (Player player in game.players)
            {
                if ((GamePad.GetState(player.playerIndex).Buttons.Start == ButtonState.Pressed)
                    || input.IsNewKeyPress(Keys.Escape) || input.IsNewKeyPress(Keys.Back))
                {
                    if (!bPaused && (GamePlayStatus != GameplayState.StartLevel && GamePlayStatus != GameplayState.StageCleared))
                    {
                        this.ScreenManager.AddScreen(menu);
                        bPaused = game.BeginPause();
                        GamePlayStatus = GameplayState.Pause;
                        return;
                    }
                }
            }
            bool hidden = coveredByOtherScreen || otherScreenHasFocus;

            // If the user covers this screen, pause.
            if (hidden && !game.IsPaused && (GamePlayStatus != GameplayState.StartLevel && GamePlayStatus != GameplayState.StageCleared))
            {
                bPaused = game.BeginPause();
            }

            if (!hidden && (game.IsPaused))
            {
                bPaused = game.EndPause();
                GamePlayStatus = GameplayState.Playing;
            }

            if (game.currentGameState == GameState.InGame)
            {
                if (GamePlayStatus == GameplayState.Playing)
                {
                    if (!game.IsPaused)
                    {
                        UpdatePlayerPlaying(gameTime);
                    }

                    UpdatePlayersAnimations(gameTime);
                    flamethrowerAnimation.Update(gameTime);
                    enemies.Update(ref gameTime, game);

                    foreach (Player player in game.players)
                    {
                        if (player.avatar.IsPlayingTheGame)
                        {
                            HandleCollisions(player, game.totalGameSeconds);
                        }
                    }

                    if (CheckIfStageCleared() == true)
                    {
                        GamePlayStatus = GameplayState.StageCleared;
                        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Complete, currentLevel.ToString(), currentSublevel.ToString());
                    }
                }
                else if (GamePlayStatus == GameplayState.StageCleared)
                {
                    UpdatePlayerPlaying(gameTime);
                    UpdatePlayersAnimations(gameTime);
                    enemies.Update(ref gameTime, game);

                    foreach (Player player in game.players)
                    {
                        if (player.avatar.IsPlayingTheGame)
                        {
                            HandleCollisions(player, game.totalGameSeconds);
                        }
                    }

                    ChangeGamplayStatusAfterSomeTimeTo(gameTime, GameplayState.StartLevel);
                }
                else if (GamePlayStatus == GameplayState.StartLevel)
                {
                    UpdatePlayerPlaying(gameTime);
                    UpdatePlayersAnimations(gameTime);

                    foreach (Player player in game.players)
                    {
                        if (player.avatar.IsPlayingTheGame)
                        {
                            HandleCollisions(player, game.totalGameSeconds);
                        }
                    }

                    ChangeGamplayStatusAfterSomeTimeTo(gameTime, GameplayState.Playing);
                }
                else if (GamePlayStatus == GameplayState.GameOver)
                {
                    if (currentLevel == LevelType.EndGame)
                    {
                        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (timer >= 5.0f)
                        {
                            foreach (Player player in game.players)
                            {
                                if (player.IsPlaying)
                                {
                                    if (game.topScoreListContainer != null)
                                    {
                                        player.SaveLeaderBoard();
                                    }

                                }
                            }

                            QuitToMenu();
                            ScreenManager.AddScreen(new CreditsScreen(false));
                        }
                    } else if(currentLevel == LevelType.EndDemo)
                    {
                        foreach (Player player in game.players)
                        {
                            if (player.IsPlaying)
                            {
                                if (game.topScoreListContainer != null)
                                {
                                    player.SaveLeaderBoard();
                                }

                            }
                        }

                        QuitToMenu();
                        ScreenManager.AddScreen(new DemoEndingScreen());
                    }
                }

                enemies.UpdatePowerUps(ref gameTime, game);

                foreach (Furniture furniture in Level.furnitureList)
                {
                    if (furniture.Type == FurnitureType.CocheArdiendo)
                    {
                        furniture.Update(gameTime);
                    }
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        private void ChangeGamplayStatusAfterSomeTimeTo(GameTime gameTime, GameplayState gameplayState)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= 2.0f)
            {
                GamePlayStatus = gameplayState;
                timer = 0;
                if (gameplayState == GameplayState.StartLevel)
                {
                    StartNewLevel(false, false);
                }
            }
        }

        private void UpdatePlayersAnimations(GameTime gameTime)
        {
            for (byte i = 0; i < IdleTrunkAnimation.Count; i++)
            {
                IdleTrunkAnimation[i].Update(gameTime);
                PistolShotEastAnimation[i].Update(gameTime);
                ShotgunShotEastAnimation[i].Update(gameTime);
                RunEastAnimation[i].Update(gameTime);
                PistolShotNEAnimation[i].Update(gameTime);
                PistolShotSEAnimation[i].Update(gameTime);
                PistolShotSouthAnimation[i].Update(gameTime);
                PistolShotNorthAnimation[i].Update(gameTime);
                ShotgunNEAnimation[i].Update(gameTime);
                ShotgunSEAnimation[i].Update(gameTime);
                ShotgunNorthAnimation[i].Update(gameTime);
                ShotgunSouthAnimation[i].Update(gameTime);
            }
        }

        private void UpdatePlayerPlaying(GameTime gameTime)
        {
            foreach (Player player in game.players)
            {
                UpdatePlayer(player, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, player.neutralInput);
            }
        }

        #region Player-centric code
        public void UpdatePlayer(Player player, float totalGameSeconds,
            float elapsedGameSeconds, NeutralInput input)
        {
            if ((player.avatar.status == ObjectStatus.Active) ||
                (player.avatar.status == ObjectStatus.Immune))
            {
                ProcessInput(player, totalGameSeconds, elapsedGameSeconds, input);
            }
        }

        public void ProcessInput(Player player, float totalGameSeconds,
            float elapsedGameSeconds, NeutralInput input)
        {
            if (player.inputMode == InputMode.GamePad)
            {
                if (input.StickLeftMovement.X > 0)
                    accumMove.X += GameplayHelper.Move(input.StickLeftMovement.X, elapsedGameSeconds);
                if (input.StickLeftMovement.X < 0)
                    accumMove.X -= GameplayHelper.Move(-input.StickLeftMovement.X, elapsedGameSeconds);
                if (input.StickLeftMovement.Y > 0)
                    accumMove.Y -= GameplayHelper.Move(input.StickLeftMovement.Y, elapsedGameSeconds);
                if (input.StickLeftMovement.Y < 0)
                    accumMove.Y += GameplayHelper.Move(-input.StickLeftMovement.Y, elapsedGameSeconds);

                if (input.StickRightMovement.X > 0)
                    accumFire.X += GameplayHelper.Move(input.StickRightMovement.X, elapsedGameSeconds);
                if (input.StickRightMovement.X < 0)
                    accumFire.X -= GameplayHelper.Move(-input.StickRightMovement.X, elapsedGameSeconds);
                if (input.StickRightMovement.Y > 0)
                    accumFire.Y -= GameplayHelper.Move(input.StickRightMovement.Y, elapsedGameSeconds);
                if (input.StickRightMovement.Y < 0)
                    accumFire.Y += GameplayHelper.Move(-input.StickRightMovement.Y, elapsedGameSeconds);

                input.GamePadFire.Normalize();
                if ((input.GamePadFire.X >= 0 || input.GamePadFire.X <= 0) || (input.GamePadFire.Y >= 0 || input.GamePadFire.Y <= 0))
                {
                    //float angle = 0.0f;
                    //angle = (float)Math.Acos(input.Fire.Y);
                    //if (input.Fire.X < 0.0f)
                    //    angle = -angle;
                    Vector2 direction = Vector2.Normalize(input.GamePadFire);
                    float angle = (float)Math.Atan2(input.GamePadFire.X, input.GamePadFire.Y);
                    player.avatar.shotAngle = angle;
                    TryFire(player, totalGameSeconds, angle, direction);
                }
            }

            if (player.inputMode == InputMode.Keyboard)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                    accumMove.X += GameplayHelper.Move(1, elapsedGameSeconds);
                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                    accumMove.X -= GameplayHelper.Move(1, elapsedGameSeconds);
                if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                    accumMove.Y += GameplayHelper.Move(1, elapsedGameSeconds);
                if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                    accumMove.Y -= GameplayHelper.Move(1, elapsedGameSeconds);

                if (Mouse.GetState().X > 0  && Mouse.GetState().LeftButton == ButtonState.Pressed)
                    accumFire.X += GameplayHelper.Move(Mouse.GetState().X, elapsedGameSeconds);
                if (Mouse.GetState().X < 0 && Mouse.GetState().LeftButton == ButtonState.Pressed)
                    accumFire.X -= GameplayHelper.Move(-Mouse.GetState().X, elapsedGameSeconds);
                if (Mouse.GetState().Y > 0 && Mouse.GetState().LeftButton == ButtonState.Pressed)
                    accumFire.Y -= GameplayHelper.Move(Mouse.GetState().Y, elapsedGameSeconds);
                if (Mouse.GetState().Y < 0 && Mouse.GetState().LeftButton == ButtonState.Pressed)
                    accumFire.Y += GameplayHelper.Move(-Mouse.GetState().Y, elapsedGameSeconds);

                input.MouseFire.Normalize();
                if ((input.MouseFire.X >= 0 || input.MouseFire.X <= 0) || (input.MouseFire.Y >= 0 || input.MouseFire.Y <= 0))
                {
                    Vector2 direction = Vector2.Normalize(input.MouseFire);
                    float angle = (float)Math.Atan2(input.MouseFire.X, input.MouseFire.Y);
                    player.avatar.shotAngle = angle;
                    TryFire(player, totalGameSeconds, angle, direction);
                }
            }

            player.avatar.accumFire = accumFire;

            TryMove(player);

            if (input.ButtonY == true)
            {
                if (player.avatar.currentgun == GunType.pistol)
                {
                    player.avatar.currentgun = GunType.machinegun;
                }
                else if (player.avatar.currentgun == GunType.machinegun)
                {
                    player.avatar.currentgun = GunType.shotgun;
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.currentgun = GunType.flamethrower;
                }
                else if (player.avatar.currentgun == GunType.flamethrower)
                {
                    player.avatar.currentgun = GunType.pistol;
                }
                else
                {
                    player.avatar.currentgun = GunType.pistol;
                }
            }

            if (input.ButtonRB == true)
            {
                timerplayer = 0;
            }

            accumFire = Vector2.Zero;
        }

        public void HandleCollisions(Player player, float totalGameSeconds)
        {
            if (player.avatar.status == ObjectStatus.Inactive)
                return;

            enemies.HandleCollisions(player, totalGameSeconds);

            if (player.avatar.lives == 0) {
                GamePlayStatus = GameplayState.GameOver;
                GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Fail, currentLevel.ToString(), currentSublevel.ToString());
                this.ScreenManager.AddScreen(gomenu);
            }
        }
        
        private void TryMove(Player player)
        {
            bool collision = false;
            player.avatar.accumMove = accumMove;

            if (accumMove.Length() > .5)
            {
                Vector2 move = player.avatar.VerifyMove(accumMove);
                Vector2 pos = player.avatar.position + move;

                for (int i = 0; i < Level.gameWorld.Obstacles.Count; i++)
                {
                    float rangeDistance;
                    if (Level.gameWorld.Obstacles[i].Radius == 5.0f)
                    {
                        rangeDistance = 10.0f;
                    }
                    else if (Level.gameWorld.Obstacles[i].Radius == 10.0f)
                    {
                        rangeDistance = 15.0f;
                    }
                    else if (Level.gameWorld.Obstacles[i].Radius == 20.0f)
                    {
                        rangeDistance = 30.0f;
                    }
                    else
                    {
                        rangeDistance = 45.0f;
                    }

                    if (Vector2.Distance(pos, Level.gameWorld.Obstacles[i].Center) < rangeDistance)
                    {
                        collision = true;
                    }
                }

                for (int i = 0; i < Level.gameWorld.Walls.Count; i++)
                {
                    float actualDistance = GameplayHelper.DistanceLineSegmentToPoint(Level.gameWorld.Walls[i].From, Level.gameWorld.Walls[i].To, pos);
                    if (actualDistance <= 5.0f)
                    {
                        collision = true;
                    }
                }

                if (!collision)
                {
                    PlayerMove(player, pos);
                }

                accumMove = Vector2.Zero;
            }
        }

        private void TryFire(Player player, float TotalGameSeconds, float angle, Vector2 direction)
        {
            int RateOfFire;
            if (player.avatar.status != ObjectStatus.Active && player.avatar.status != ObjectStatus.Immune)
                return;

            // Check if we have ammo; if not we change the current gun to pistol
            if (player.avatar.ammo[(int)player.avatar.currentgun] == 0)
            {
                player.avatar.currentgun = GunType.pistol;
            }

            if (player.avatar.currentgun == GunType.machinegun && player.avatar.ammo[(int)GunType.machinegun] > 0)
            {
                RateOfFire = MACHINEGUN_RATE_OF_FIRE;
            }
            else if (player.avatar.currentgun == GunType.flamethrower && player.avatar.ammo[(int)GunType.flamethrower] > 0)
            {
                RateOfFire = FLAMETHROWER_RATE_OF_FIRE;
            }
            else
            {
                RateOfFire = player.avatar.RateOfFire;
            }

            if (player.avatar.currentgun == GunType.pistol
                || (player.avatar.currentgun != GunType.pistol
                && player.avatar.ammo[(int)player.avatar.currentgun] > 0))
            {
                if (player.avatar.VerifyFire(TotalGameSeconds, RateOfFire))
                {
                    PlayerFire(player, TotalGameSeconds, angle, direction);
                }
            }
        }

#endregion

        public Boolean CheckIfStageCleared()
        {
            int enemiesLeft = enemies.Count();

            if (enemiesLeft <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartNewLevel(bool restartLevel, bool restartWave)
        {
            int howManySpawnZones = 4;
            List<int> numplayersIngame = new List<int>();
            float lIndex = 0.8f;
            FurnitureComparer furnitureComparer = new FurnitureComparer();
            this.random = new Random(16);

            if (!restartLevel)
            {
                if (subLevelIndex == 9)
                {
                    currentLevel = Level.GetNextLevel(currentLevel);
                    if (currentLevel == LevelType.EndGame || currentLevel == LevelType.EndDemo)
                    {
                        GamePlayStatus = GameplayState.GameOver;
                        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Undefined, currentLevel.ToString(), currentSublevel.ToString());
                    }
                    else
                    {
                        Level = new Level(currentLevel);
                        Level.Initialize(game);
                        Map = game.Content.Load<Texture2D>(Level.mapTextureFileName);
                        subLevelIndex = 0;

                        foreach (Furniture furniture in Level.furnitureList)
                        {
                            furniture.Load(game);
                        }

                        Level.furnitureList.Sort(furnitureComparer);
                        // Apply layer index to sorted list
                        foreach (Furniture furniture in Level.furnitureList)
                        {
                            furniture.layerIndex = lIndex;
                            lIndex -= 0.004f;
                        }

                        for (int i = 0; i < game.players.Length - 1; i++)
                        {
                            game.players[i].avatar.position = Level.PlayerSpawnPosition[i];
                            game.players[i].avatar.entity.Position = Level.PlayerSpawnPosition[i];
                        }

                        foreach (Player player in game.players)
                        {
                            if (player.IsPlaying)
                            {
                                player.SaveGame(Level.getLevelNumber(currentLevel));
                            }
                        }
                    }
                }
                else
                {
                    subLevelIndex++;
                }
            }
            else
            {
                if (!restartWave)
                {
                    subLevelIndex = 0;
                }

                for (int i = 0; i < game.players.Length; i++)
                {
                    game.players[i].avatar.position = Level.PlayerSpawnPosition[i];
                    game.players[i].avatar.entity.Position = Level.PlayerSpawnPosition[i];
                }
            }

            enemies.Clear();

            if (currentLevel != LevelType.EndGame && currentLevel != LevelType.EndDemo)
            {

                for (int i = 0; i < game.players.Length; i++)
                {
                    game.players[i].avatar.behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));
                    if (game.players[i].avatar.status == ObjectStatus.Active || game.players[i].avatar.status == ObjectStatus.Immune)
                    {
                        numplayersIngame.Add(i);
                    }
                }

                switch (subLevelIndex)
                {
                    case 0:
                        currentSublevel = SubLevel.SubLevelType.One;
                        break;
                    case 1:
                        currentSublevel = SubLevel.SubLevelType.Two;
                        break;
                    case 2:
                        currentSublevel = SubLevel.SubLevelType.Three;
                        break;
                    case 3:
                        currentSublevel = SubLevel.SubLevelType.Four;
                        break;
                    case 4:
                        currentSublevel = SubLevel.SubLevelType.Five;
                        break;
                    case 5:
                        currentSublevel = SubLevel.SubLevelType.Six;
                        break;
                    case 6:
                        currentSublevel = SubLevel.SubLevelType.Seven;
                        break;
                    case 7:
                        currentSublevel = SubLevel.SubLevelType.Eight;
                        break;
                    case 8:
                        currentSublevel = SubLevel.SubLevelType.Nine;
                        break;
                    case 9:
                        currentSublevel = SubLevel.SubLevelType.Ten;
                        break;
                    default:
                        currentSublevel = SubLevel.SubLevelType.One;
                        break;
                }

                for (int i = 0; i < Level.ZombieSpawnZones.Count - 1; i++)
                {
                    if (Level.ZombieSpawnZones[i].X == 0 && Level.ZombieSpawnZones[i].Y == 0 && Level.ZombieSpawnZones[i].Z == 0 && Level.ZombieSpawnZones[i].W == 0)
                    {
                        howManySpawnZones--;
                    }
                }

                float zombieLife;
                float zombieSpeed;
                float ratLife;
                float ratSpeed;
                float wolfLife;
                float wolfSpeed;
                float minotaurLife = 100.0f;
                float minotaurSpeed = 1.0f;
                switch (currentLevel)
                {
                    case LevelType.One:
                        zombieLife = 1.0f;
                        zombieSpeed = 0.0f;
                        ratLife = 1.0f;
                        ratSpeed = 0.8f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.0f;
                        break;
                    case LevelType.Two:
                        zombieLife = 1.5f;
                        zombieSpeed = 0.2f;
                        ratLife = 1.0f;
                        ratSpeed = 0.9f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.0f;
                        break;
                    case LevelType.Three:
                        zombieLife = 2.0f;
                        zombieSpeed = 0.3f;
                        ratLife = 1.5f;
                        ratSpeed = 1.0f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.1f;
                        break;
                    case LevelType.Four:
                        zombieLife = 2.5f;
                        zombieSpeed = 0.4f;
                        ratLife = 2.0f;
                        ratSpeed = 1.1f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.1f;
                        break;
                    case LevelType.Five:
                        zombieLife = 3.0f;
                        zombieSpeed = 0.5f;
                        ratLife = 2.5f;
                        ratSpeed = 1.1f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.2f;
                        break;
                    case LevelType.Six:
                        zombieLife = 3.5f;
                        zombieSpeed = 0.6f;
                        ratLife = 3.0f;
                        ratSpeed = 1.2f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.2f;
                        break;
                    case LevelType.Seven:
                        zombieLife = 4.0f;
                        zombieSpeed = 0.7f;
                        ratLife = 3.5f;
                        ratSpeed = 1.2f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.3f;
                        break;
                    case LevelType.Eight:
                        zombieLife = 4.5f;
                        zombieSpeed = 0.8f;
                        ratLife = 4.0f;
                        ratSpeed = 1.3f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.3f;
                        break;
                    case LevelType.Nine:
                        zombieLife = 5.0f;
                        zombieSpeed = 0.9f;
                        ratLife = 4.5f;
                        ratSpeed = 1.3f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.4f;
                        break;
                    case LevelType.Ten:
                        zombieLife = 5.5f;
                        zombieSpeed = 1.0f;
                        ratLife = 5.0f;
                        ratSpeed = 1.4f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.4f;
                        break;
                    default:
                        zombieLife = 1.0f;
                        zombieSpeed = 0.0f;
                        ratLife = 1.0f;
                        ratSpeed = 0.8f;
                        wolfLife = 1.0f;
                        wolfSpeed = 3.0f;
                        break;
                }

                enemies.InitializeEnemy(
                    Level.subLevelList[subLevelIndex].enemies.Zombies,
                    EnemyType.Zombie,
                    Level,
                    subLevelIndex,
                    zombieLife,
                    zombieSpeed,
                    numplayersIngame
                );

                enemies.InitializeEnemy(
                    Level.subLevelList[subLevelIndex].enemies.Tanks,
                    EnemyType.Tank,
                    Level,
                    subLevelIndex,
                    zombieLife,
                    zombieSpeed,
                    numplayersIngame
                );

                enemies.InitializeEnemy(
                    Level.subLevelList[subLevelIndex].enemies.Rats,
                    EnemyType.Rat,
                    Level,
                    subLevelIndex,
                    ratLife,
                    ratSpeed,
                    numplayersIngame
                );

                enemies.InitializeEnemy(
                    Level.subLevelList[subLevelIndex].enemies.Wolfs,
                    EnemyType.Wolf,
                    Level,
                    subLevelIndex,
                    wolfLife,
                    wolfSpeed,
                    numplayersIngame
                );

                enemies.InitializeEnemy(
                    Level.subLevelList[subLevelIndex].enemies.Minotaurs,
                    EnemyType.Minotaur,
                    Level,
                    subLevelIndex,
                    minotaurLife,
                    minotaurSpeed,
                    numplayersIngame
                );

                enemies.LoadContent(game.Content);
            }

            GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, currentLevel.ToString(), currentSublevel.ToString());
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (game.currentGameState != GameState.Paused)
            {
                Color c = new Color(new Vector4(0, 0, 0, 0));
                this.ScreenManager.GraphicsDevice.Clear(c);

                DrawMap(Map);

                enemies.DrawPowerUps(this.ScreenManager.SpriteBatch, gameTime);

                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                foreach (Player player in game.players)
                {
                    if (player.avatar.IsPlayingTheGame)
                    {
                        DrawPlayer(player, game.totalGameSeconds, gameTime, Level.furnitureList);
                    }
                }

                if (GamePlayStatus == GameplayState.StartLevel || GamePlayStatus == GameplayState.Playing || GamePlayStatus == GameplayState.Pause)
                {
                    enemies.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, Level.furnitureList, gameTime);
                }

                if (GamePlayStatus == GameplayState.StageCleared)
                {
                    enemies.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, Level.furnitureList, gameTime);
                }

                foreach (Furniture furniture in Level.furnitureList)
                {
                    furniture.Draw(this.ScreenManager.SpriteBatch, MenuInfoFont);
                }

                this.ScreenManager.SpriteBatch.End();

                foreach (Furniture furniture in Level.furnitureList)
                {
                    if (furniture.Type == FurnitureType.CocheArdiendo)
                    {
                        //furniture.particleRenderer.RenderEffect(furniture.SmokeEffect);
                    }
                }

                foreach (Player player in game.players)
                {
                    if (player.avatar.IsPlayingTheGame)
                    {
                        DrawShotgunShots(player.avatar.shotgunbullets, game.totalGameSeconds);
                        DrawBullets(player.avatar.bullets, game.totalGameSeconds);
                    }
                }

                // Perlin Noise effect draw
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Resolution.getTransformationMatrix());
#if DEBUG
                Level.gameWorld.Draw(this.ScreenManager.SpriteBatch, gameTime, this.ScreenManager.SpriteBatch);
#endif

                this.ScreenManager.SpriteBatch.End();
                // end Perlin Noise effect

                DrawUI(gameTime);

                if (GamePlayStatus == GameplayState.StageCleared)
                {
                    DrawStageCleared();
                }

                if (GamePlayStatus == GameplayState.StartLevel)
                {
                    DrawStartLevel();
                }

                if (GamePlayStatus == GameplayState.GameOver)
                {
                    DrawGameOver();
                }

                // Draw the Storage Device Icon
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

                foreach (Player player in game.players)
                {
                    if (player.inputMode == InputMode.Keyboard)
                    {
                        this.ScreenManager.SpriteBatch.Draw(cursorTexture, cursorPos, Color.White);
                    }
                }

                this.ScreenManager.SpriteBatch.End();
            }
            else
            {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                this.ScreenManager.SpriteBatch.Draw(game.blackTexture, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused.ToUpper(), new Vector2(5, 5), Color.White);
                this.ScreenManager.SpriteBatch.End();
            }
        }

        #region Drawing Code

        private void DrawMap(Texture2D map)
        {
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            batch.Draw(map, new Rectangle(0, 0, 1280, 720), Color.White);

            batch.End();
        }

        private void DrawStageCleared()
        {
            this.ScreenManager.FadeBackBufferToBlack(64);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2, uiBounds.Y + uiBounds.Height / 2);
            this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width / 2, UICenter.Y - 10), Color.White);
            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, Strings.ClearedGameplayString.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(Strings.ClearedGameplayString.ToUpper()).X)/2, UICenter.Y), Color.White);
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.PrepareNextWaveGameplayString.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(Strings.PrepareNextWaveGameplayString.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);
            this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width / 2, UICenter.Y + 90), Color.White);

            this.ScreenManager.SpriteBatch.End();
        }

        private void DrawStartLevel()
        {
            string levelshowstring;
            int timeLeftWaitingPlayers;
            int fixedTimeLeft = 5;
            this.ScreenManager.FadeBackBufferToBlack(64);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2, uiBounds.Y + uiBounds.Height / 2);
            this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width /2, UICenter.Y - 10), Color.White);

            switch (currentLevel)
            {
                case LevelType.One:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberOne;
                    break;

                case LevelType.Two:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberTwo;
                    break;

                case LevelType.Three:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberThree;
                    break;

                case LevelType.Four:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberFour;
                    break;

                case LevelType.Five:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberFive;
                    break;

                case LevelType.Six:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberSix;
                    break;

                case LevelType.Seven:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberSeven;
                    break;

                case LevelType.Eight:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberEight;
                    break;

                case LevelType.Nine:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberNine;
                    break;

                case LevelType.Ten:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberTen;
                    break;

                default:
                    levelshowstring = Strings.LevelSelectMenuString + " " + Strings.NumberOne;
                    break;

            }

            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y), Color.White);

            switch (currentSublevel)
            {
                case SubLevel.SubLevelType.One:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberOne;
                    break;

                case SubLevel.SubLevelType.Two:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberTwo;
                    break;

                case SubLevel.SubLevelType.Three:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberThree;
                    break;

                case SubLevel.SubLevelType.Four:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberFour;
                    break;

                case SubLevel.SubLevelType.Five:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberFive;
                    break;

                case SubLevel.SubLevelType.Six:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberSix;
                    break;

                case SubLevel.SubLevelType.Seven:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberSeven;
                    break;

                case SubLevel.SubLevelType.Eight:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberEight;
                    break;

                case SubLevel.SubLevelType.Nine:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberNine;
                    break;

                case SubLevel.SubLevelType.Ten:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberTen;
                    break;

                default:
                    levelshowstring = Strings.WaveGameplayString + " " + Strings.NumberOne;
                    break;

            }

            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);
            this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width / 2, UICenter.Y + 90), Color.White);

            if (game.currentGameState == GameState.WaitingToBegin)
            {
                timeLeftWaitingPlayers = fixedTimeLeft;
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.WaitingForPlayersMenuString.ToUpper(), new Vector2(uiBounds.Left, uiBounds.Height), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, timeLeftWaitingPlayers.ToString(), new Vector2(uiBounds.Left + MenuInfoFont.MeasureString(Strings.WaitingForPlayersMenuString.ToUpper()).X + 5, uiBounds.Height), Color.White);
            }

            this.ScreenManager.SpriteBatch.End();
        }

        private void DrawGameOver()
        {
            if (currentLevel == LevelType.EndGame)
            {
                string levelshowstring;
                this.ScreenManager.FadeBackBufferToBlack(64);
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

                Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2, uiBounds.Y + uiBounds.Height / 2);
                this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width / 2, UICenter.Y - 10), Color.White);

                levelshowstring = Strings.CongratssEndGameString;
                this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y), Color.White);

                levelshowstring = Strings.YouSavedCityEndGameString;
                this.ScreenManager.SpriteBatch.DrawString(MenuListFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);

                this.ScreenManager.SpriteBatch.Draw(whiteLine, new Vector2(UICenter.X - whiteLine.Width / 2, UICenter.Y + 90), Color.White);

                this.ScreenManager.SpriteBatch.End();
            }

            if (currentLevel == LevelType.EndDemo)
            {

            }
        }

        public bool IsInRange(Avatar state, Furniture furniture)
        {
            float distance = Vector2.Distance(state.position, furniture.ObstaclePosition);
            if (distance < Avatar.CrashRadius + 10.0f)
            {
                return true;
            }

            return false;
        }

        public float GetLayerIndex(Avatar state, List<Furniture> furniturelist)
        {
            float furnitureInferior, playerBasePosition, lindex;
            int n = 0;

            playerBasePosition = state.position.Y;
            furnitureInferior = 0.0f;
            lindex = 0.0f;

            while (playerBasePosition > furnitureInferior)
            {
                if (n < furniturelist.Count)
                {
                    furnitureInferior = furniturelist[n].Position.Y + furniturelist[n].Texture.Height;
                    lindex = furniturelist[n].layerIndex;
                }
                else
                {
                    return lindex + 0.002f;
                }

                n++;
            }

            return lindex + 0.002f;
        }

        private void DrawPlayer(Player player, double TotalGameSeconds, GameTime gameTime, List<Furniture> furniturelist)
        {
            float layerIndex = GetLayerIndex(player.avatar, furniturelist);
            Vector2 offsetPosition = new Vector2(-20, -55);

            if (GamePlayStatus == GameplayState.Playing
                || GamePlayStatus == GameplayState.StageCleared
                || GamePlayStatus == GameplayState.StartLevel)
            {
                timerplayer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (player.avatar.IsPlayingTheGame)
                {
                    if (player.avatar.color == Color.Blue)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerBlue, new Vector2(player.avatar.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerBlue.Width / 2 + offsetPosition.X, player.avatar.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    if (player.avatar.color == Color.Red)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerRed, new Vector2(player.avatar.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerRed.Width / 2 + offsetPosition.X, player.avatar.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    if (player.avatar.color == Color.Green)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerGreen, new Vector2(player.avatar.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerGreen.Width / 2 + offsetPosition.X, player.avatar.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    if (player.avatar.color == Color.Yellow)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerYellow, new Vector2(player.avatar.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerYellow.Width / 2 + offsetPosition.X, player.avatar.position.Y - 20 + offsetPosition.Y), Color.White);
                    }
                }
            }
            else
            {
                timerplayer = 0;
            }

            switch (player.avatar.status)
            {
                case ObjectStatus.Inactive:
                    break;
                case ObjectStatus.Active:
                    Color color;
                    if (player.avatar.isLoosingLife == true)
                    {
                        color = Color.Red;
                    }
                    else
                    {
                        color = Color.White;
                    }

                    if (player.avatar.accumFire.Length() > .5)
                    {
                        if (player.avatar.shotAngle > -0.3925f && player.avatar.shotAngle < 0.3925f) //NORTH
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 12 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 34), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 12 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 36), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 6 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 34), 26, ShotgunNorthTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 26, ShotgunNorthTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 12 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 36), 21, ShotgunNorthTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 21, ShotgunNorthTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle > 0.3925f && player.avatar.shotAngle < 1.1775f) //NORTH-EAST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 59, ShotgunNETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 53, ShotgunNETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 53, ShotgunNETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle > 1.1775f && player.avatar.shotAngle < 1.9625f) //EAST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle > 1.19625f && player.avatar.shotAngle < 2.7275f) //SOUTH-EAST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 58, ShotgunSETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 54, ShotgunSETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 54, ShotgunSETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle > 2.7275f || player.avatar.shotAngle < -2.7275f) //SOUTH
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 3 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 21, ShotgunSouthTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 21, ShotgunSouthTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 20, ShotgunSouthTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 20, ShotgunSouthTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle < -1.9625f && player.avatar.shotAngle > -2.7275f) //SOUTH-WEST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 1 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 4 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 1 + offsetPosition.X - 29), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 58, ShotgunSETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 4 + offsetPosition.X - 27), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 54, ShotgunSETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 54, ShotgunSETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle < -1.1775f && player.avatar.shotAngle > -1.9625f) //WEST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 2 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 4 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (player.avatar.shotAngle < -0.3925f && player.avatar.shotAngle > -1.1775f) //NORTH-WEST
                        {
                            switch (player.avatar.currentgun)
                            {
                                case GunType.pistol:
                                    if (player.avatar.character == 0)
                                    {
                                        PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 2 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;
                                case GunType.shotgun:
                                case GunType.machinegun:
                                    if (player.avatar.character == 0)
                                    {
                                        ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 4, player.avatar.position.Y + offsetPosition.Y - 13), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    break;

                                case GunType.flamethrower:
                                    if (player.avatar.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + offsetPosition.X - 29), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 59, ShotgunNETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + offsetPosition.X + 4 - 26), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 13), 53, ShotgunNETexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 53, ShotgunNETexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(player.avatar, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    // Draw Movement LEGS
                    if (player.avatar.accumMove.Length() > .5)
                    {
                        if (player.avatar.character == 0)
                        {
                            if (player.avatar.accumMove.X > 0)
                            {
                                if (player.avatar.accumFire.Length() < .5)
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }

                                RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(player.avatar.position.X - 7 + offsetPosition.X, player.avatar.position.Y - 26), SpriteEffects.None, layerIndex + 0.001f, 0f, color);
                            }
                            else
                            {
                                if (player.avatar.accumFire.Length() < .5)
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 16, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                }

                                RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(player.avatar.position.X + 18 + offsetPosition.X, player.avatar.position.Y - 26), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, color);
                            }
                        }
                        else
                        {
                            if (player.avatar.accumMove.X > 0)
                            {
                                if (player.avatar.accumFire.Length() < .5)
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 10, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }

                                RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(player.avatar.position.X - 4 + offsetPosition.X, player.avatar.position.Y - 24), SpriteEffects.None, layerIndex + 0.001f, 0f, color);
                            }
                            else
                            {
                                if (player.avatar.accumFire.Length() < .5)
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 19, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                }

                                RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(player.avatar.position.X + 20 + offsetPosition.X, player.avatar.position.Y - 24), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, color);
                            }
                        }
                    }
                    else
                    {
                        if (player.avatar.accumFire.Length() < .5)
                        {
                            if (player.avatar.character == 0)
                            {
                                if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                {
                                    IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, color);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }
                            else
                            {
                                if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                {
                                    IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 10, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }
                        }

                        this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 3), IdleLegsTexture[player.avatar.character].Width, IdleLegsTexture[player.avatar.character].Height),
                        new Rectangle(0, 0, IdleLegsTexture[player.avatar.character].Width, IdleLegsTexture[player.avatar.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                    }

#if DEBUG
                    this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, layerIndex.ToString(), player.avatar.position, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.4f);

                    // Position Reference TEMPORAL
                    this.ScreenManager.SpriteBatch.Draw(PositionReference, new Rectangle(Convert.ToInt32(player.avatar.position.X), Convert.ToInt32(player.avatar.position.Y), PositionReference.Width, PositionReference.Height),
                        new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
#endif

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(player.avatar.position.X + IdleLegsTexture[player.avatar.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + IdleLegsTexture[player.avatar.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);

                    player.avatar.isLoosingLife = false;
                    break;


                case ObjectStatus.Dying:
                    if (player.avatar.accumMove.X > 0)
                    {
                        this.ScreenManager.SpriteBatch.Draw(DiedTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y - 27), DiedTexture[player.avatar.character].Width, DiedTexture[player.avatar.character].Height),
                            new Rectangle(0, 0, DiedTexture[player.avatar.character].Width, DiedTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex + 0.001f);
                    }
                    else
                    {
                        this.ScreenManager.SpriteBatch.Draw(DiedTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y - 27), DiedTexture[player.avatar.character].Width, DiedTexture[player.avatar.character].Height),
                            new Rectangle(0, 0, DiedTexture[player.avatar.character].Width, DiedTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                    }

#if DEBUG
                    // Position Reference TEMPORAL
                    this.ScreenManager.SpriteBatch.Draw(PositionReference, new Rectangle(Convert.ToInt32(player.avatar.position.X), Convert.ToInt32(player.avatar.position.Y), PositionReference.Width, PositionReference.Height),
                        new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
#endif

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(player.avatar.position.X + IdleLegsTexture[player.avatar.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + IdleLegsTexture[player.avatar.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);
                    break;


                case ObjectStatus.Immune://blink 5 times per second
                    if (((int)(TotalGameSeconds * 10) % 2) == 0)
                    {
                        // Draws our avatar at the current position with no tinting
                        if (player.avatar.accumFire.Length() > .5)
                        {
                            if (player.avatar.shotAngle > -0.3925f && player.avatar.shotAngle < 0.3925f) //NORTH
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 34), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 12 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 36), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNorthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 12 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 6 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 34), 26, ShotgunNorthTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 26, ShotgunNorthTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 12 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 36), 21, ShotgunNorthTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 21, ShotgunNorthTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle > 0.3925f && player.avatar.shotAngle < 1.1775f) //NORTH-EAST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;
                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 59, ShotgunNETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 53, ShotgunNETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 53, ShotgunNETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle > 1.1775f && player.avatar.shotAngle < 1.9625f) //EAST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle > 1.19625f && player.avatar.shotAngle < 2.7275f) //SOUTH-EAST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 58, ShotgunSETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 54, ShotgunSETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 54, ShotgunSETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle > 2.7275f || player.avatar.shotAngle < -2.7275f) //SOUTH
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 10 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSouthAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 3 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 21, ShotgunSouthTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 21, ShotgunSouthTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 20, ShotgunSouthTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 20, ShotgunSouthTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle < -1.9625f && player.avatar.shotAngle > -2.7275f) //SOUTH-WEST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 1 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 4 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 7 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 1 + offsetPosition.X - 29), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 58, ShotgunSETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 4 + offsetPosition.X - 27), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 54, ShotgunSETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 54, ShotgunSETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle < -1.1775f && player.avatar.shotAngle > -1.9625f) //WEST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 4 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 2 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (player.avatar.shotAngle < -0.3925f && player.avatar.shotAngle > -1.1775f) //NORTH-WEST
                            {
                                switch (player.avatar.currentgun)
                                {
                                    case GunType.pistol:
                                    case GunType.machinegun:
                                        if (player.avatar.character == 0)
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 14), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X - 6 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                                            {
                                                ShotgunNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 4, player.avatar.position.Y + offsetPosition.Y - 13), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + 2 + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case GunType.flamethrower:
                                        if (player.avatar.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + offsetPosition.X - 29), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 59, ShotgunNETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + offsetPosition.X + 4 - 26), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y - 13), 53, ShotgunNETexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 53, ShotgunNETexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(player.avatar, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }

                        // Draw Movement LEGS
                        if (player.avatar.accumMove.Length() > .5)
                        {
                            if (player.avatar.character == 0)
                            {
                                if (player.avatar.accumMove.X > 0)
                                {
                                    if (player.avatar.accumFire.Length() < .5)
                                    {
                                        if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                        {
                                            IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(player.avatar.position.X - 7 + offsetPosition.X, player.avatar.position.Y - 26), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (player.avatar.accumFire.Length() < .5)
                                    {
                                        if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                        {
                                            IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 16, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                    new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(player.avatar.position.X + 18 + offsetPosition.X, player.avatar.position.Y - 26), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                            else
                            {
                                if (player.avatar.accumMove.X > 0)
                                {
                                    if (player.avatar.accumFire.Length() < .5)
                                    {
                                        if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                        {
                                            IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 10, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(player.avatar.position.X - 4 + offsetPosition.X, player.avatar.position.Y - 24), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (player.avatar.accumFire.Length() < .5)
                                    {
                                        if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                        {
                                            IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 19, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(player.avatar.position.X + 20 + offsetPosition.X, player.avatar.position.Y - 24), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                        }
                        else
                        {
                            if (player.avatar.accumFire.Length() < .5)
                            {
                                if (player.avatar.character == 0)
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X, player.avatar.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }
                                else
                                {
                                    if (player.avatar.currentgun == GunType.pistol && player.avatar.ammo[(int)GunType.pistol] == 0)
                                    {
                                        IdleTrunkAnimation[player.avatar.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(player.avatar.position.X + offsetPosition.X + 10, player.avatar.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 10 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[player.avatar.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }
                            }

                            this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[player.avatar.character], new Rectangle(Convert.ToInt32(player.avatar.position.X + 7 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + offsetPosition.Y + 3), IdleLegsTexture[player.avatar.character].Width, IdleLegsTexture[player.avatar.character].Height),
                            new Rectangle(0, 0, IdleLegsTexture[player.avatar.character].Width, IdleLegsTexture[player.avatar.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                        }

                    }

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(player.avatar.position.X + IdleLegsTexture[player.avatar.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(player.avatar.position.Y + IdleLegsTexture[player.avatar.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);
                    break;
                default:
                    break;
            }
        }

        private void DrawFlameThrower(Avatar player, float layerIndex)
        {
            if (player.currentgun == GunType.flamethrower && player.ammo[(int)GunType.flamethrower] > 0)
            {
                flamethrowerAnimation.Draw(this.ScreenManager.SpriteBatch, player.FlameThrowerPosition, SpriteEffects.None, layerIndex, player.FlameThrowerAngle, Color.White);
            }
        }

        private void DrawShotgunShots(List<ShotgunShell> shotgunbullets, double TotalGameSeconds)
        {
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
            Vector2 pos;
            for (int i = 0; i < shotgunbullets.Count; i++)
            {
                for (int j = 0; j < shotgunbullets[i].Pellet.Count; j++)
                {
                    pos = GameplayHelper.FindShotgunBulletPosition(shotgunbullets[i].Pellet[j], shotgunbullets[i].Angle, j, TotalGameSeconds);

                    if (pos.X < -10 || pos.X > 1290 || pos.Y < -10 || pos.Y > 730)
                    {
                        shotgunbullets[i].Pellet.RemoveAt(j);
                    }
                    else
                    {
                        batch.Draw(bullet, pos, null, Color.White, shotgunbullets[i].Angle, bulletorigin, 1.0f, SpriteEffects.None, 0.6f);
                    }
                }

                if (shotgunbullets[i].Pellet.Count == 0)
                {
                    shotgunbullets.RemoveAt(i);
                }
            }
            batch.End();
        }


        private void DrawBullets(List<Vector4> bullets, double TotalGameSeconds)
        {
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
            Vector2 pos;
            for (int i = 0; i < bullets.Count; i++)
            {
                pos = GameplayHelper.FindBulletPosition(bullets[i], TotalGameSeconds);

                if (pos.X < -10 || pos.X > 1290 || pos.Y < -10 || pos.Y > 730)
                {
                    bullets.RemoveAt(i);
                }
                else
                {
                    batch.Draw(bullet, pos, null, Color.White, bullets[i].W,
                        bulletorigin, 1.0f, SpriteEffects.None, 0.6f);
                }
            }
            batch.End();
        }

        private void DrawUI(GameTime gameTime)
        {
            Vector2 Pos = new Vector2(uiBounds.X+ 10, uiBounds.Y+20);
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            foreach (Player player in game.players)
            {
                if (player != null)
                {
                    if (player.avatar.status == ObjectStatus.Active || player.avatar.IsPlayingTheGame)
                    {
                        if (player.avatar.lives != 0)
                        {
                            batch.Draw(UIStats, new Vector2(Pos.X, Pos.Y - 20), Color.White);

                            if (player.avatar.color == Color.Blue)
                            {
                                batch.Draw(UIStatsBlue, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (player.avatar.color == Color.Red)
                            {
                                batch.Draw(UIStatsRed, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (player.avatar.color == Color.Green)
                            {
                                batch.Draw(UIStatsGreen, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (player.avatar.color == Color.Yellow)
                            {
                                batch.Draw(UIStatsYellow, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            switch (player.avatar.character)
                            {
                                case 0:
                                    batch.Draw(jadeUI, new Rectangle(Convert.ToInt32(Pos.X) - jadeUI.Width / 2 + 5, Convert.ToInt32(Pos.Y - 34), jadeUI.Width, jadeUI.Height), Color.White);
                                    break;
                                case 1:
                                    batch.Draw(richardUI, new Rectangle(Convert.ToInt32(Pos.X - 48), Convert.ToInt32(Pos.Y - 43), richardUI.Width, richardUI.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 1.0f);
                                    break;
                                case 2:
                                    batch.Draw(rayUI, new Rectangle(Convert.ToInt32(Pos.X - 30), Convert.ToInt32(Pos.Y - 39), rayUI.Width, rayUI.Height), Color.White);
                                    break;
                                case 3:
                                    batch.Draw(peterUI, new Rectangle(Convert.ToInt32(Pos.X + 20) - peterUI.Width / 2, Convert.ToInt32(Pos.Y - 35), peterUI.Width, peterUI.Height), Color.White);
                                    break;
                            }

                            batch.DrawString(arcade14, "x", new Vector2(Pos.X + 40, Pos.Y + 17), Color.White);
                            batch.DrawString(arcade28, player.avatar.lives.ToString(), new Vector2(Pos.X + 60, Pos.Y), Color.White);

                            // Draw Player Life Counter
                            batch.Draw(enemies.heart, new Vector2(Pos.X + 120, Pos.Y + 3), Color.White);
                            batch.DrawString(arcade14, player.avatar.lifecounter.ToString("000"), new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y), Color.White);

                            switch (player.avatar.currentgun)
                            {
                                case GunType.machinegun:
                                    batch.Draw(enemies.pistolammoUI, new Vector2(Pos.X + 124, Pos.Y + 23), Color.White);
                                    batch.DrawString(arcade14, player.avatar.ammo[(int)GunType.machinegun].ToString("000"), new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case GunType.shotgun:
                                    batch.Draw(enemies.shotgunammoUI, new Vector2(Pos.X + 123, Pos.Y + 22), Color.White);
                                    batch.DrawString(arcade14, player.avatar.ammo[(int)GunType.shotgun].ToString("000"), new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case GunType.grenade:
                                    batch.Draw(enemies.grenadeammoUI, new Vector2(Pos.X + 122, Pos.Y + 21), Color.White);
                                    batch.DrawString(arcade14, player.avatar.ammo[(int)GunType.grenade].ToString("000"), new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case GunType.flamethrower:
                                    batch.Draw(enemies.flamethrowerammoUI, new Vector2(Pos.X + 124, Pos.Y + 23), Color.White);
                                    batch.DrawString(arcade14, player.avatar.ammo[(int)GunType.flamethrower].ToString("000"), new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case GunType.pistol:
                                default:
                                    batch.DrawString(arcade14, "000", new Vector2(Pos.X + enemies.heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                            }

                            batch.DrawString(arcade14, "SC" + player.avatar.score.ToString("0000000"), new Vector2(Pos.X + 13, Pos.Y + 48), Color.White);
                            Pos.X += UIStats.Width + 50;
                        }
                        else
                        {
                            batch.DrawString(MenuInfoFont, Strings.GameOverString, new Vector2(Pos.X, Pos.Y), Color.White);
                            Pos.X += (int)(MenuInfoFont.MeasureString(Strings.GameOverString).X) + 100;
                        }
                    }
                }
            }

            string levelstring;
            switch (currentLevel)
            {
                case LevelType.One:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberOne;
                    break;

                case LevelType.Two:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberTwo;
                    break;

                case LevelType.Three:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberThree;
                    break;

                case LevelType.Four:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberFour;
                    break;

                case LevelType.Five:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberFive;
                    break;

                case LevelType.Six:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberSix;
                    break;

                case LevelType.Seven:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberSeven;
                    break;

                case LevelType.Eight:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberEight;
                    break;

                case LevelType.Nine:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberNine;
                    break;

                case LevelType.Ten:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberTen;
                    break;

                default:
                    levelstring = Strings.LevelSelectMenuString + " " + Strings.NumberOne;
                    break;

            }

            batch.DrawString(MenuInfoFont, levelstring.ToUpper(), new Vector2(uiBounds.Width - MenuInfoFont.MeasureString(levelstring).X / 2, uiBounds.Height - 20), Color.White);

            string sublevelstring;
            switch (currentSublevel)
            {
                case SubLevel.SubLevelType.One:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberOne;
                    break;

                case SubLevel.SubLevelType.Two:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberTwo;
                    break;

                case SubLevel.SubLevelType.Three:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberThree;
                    break;

                case SubLevel.SubLevelType.Four:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberFour;
                    break;

                case SubLevel.SubLevelType.Five:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberFive;
                    break;

                case SubLevel.SubLevelType.Six:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberSix;
                    break;

                case SubLevel.SubLevelType.Seven:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberSeven;
                    break;

                case SubLevel.SubLevelType.Eight:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberEight;
                    break;

                case SubLevel.SubLevelType.Nine:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberNine;
                    break;

                case SubLevel.SubLevelType.Ten:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberTen;
                    break;

                default:
                    sublevelstring = Strings.WaveGameplayString + " " + Strings.NumberOne;
                    break;

            }

            batch.DrawString(MenuInfoFont, sublevelstring.ToUpper(), new Vector2(uiBounds.Width - MenuInfoFont.MeasureString(sublevelstring).X / 2, uiBounds.Height), Color.White);

#if DEMO
            batch.DrawString(MenuInfoFont, Strings.TrialModeMenuString.ToUpper(),
                    new Vector2(uiBounds.Width - MenuInfoFont.MeasureString(Strings.TrialModeMenuString.ToUpper()).X / 2, uiBounds.Height + 20), Color.White);
#endif

            if (game.currentInputMode == InputMode.Touch)
            {
                batch.Draw(pause_icon, new Vector2(uiBounds.Width + 70, uiBounds.Y - 30), Color.White);

                // if the user is touching the screen and the thumbsticks have positions,
                // draw our thumbstick sprite so the user knows where the centers are
                if (VirtualThumbsticks.LeftThumbstickCenter.HasValue && !bPaused && GamePlayStatus == GameplayState.Playing)
                {
                    batch.Draw(
                        left_thumbstick,
                        VirtualThumbsticks.LeftThumbstickCenter.Value - new Vector2(left_thumbstick.Width / 2f, left_thumbstick.Height / 2f),
                        Color.White);
                }

                if (VirtualThumbsticks.RightThumbstickCenter.HasValue && !bPaused && GamePlayStatus == GameplayState.Playing)
                {
                    batch.Draw(
                        right_thumbstick,
                        VirtualThumbsticks.RightThumbstickCenter.Value - new Vector2(right_thumbstick.Width / 2f, right_thumbstick.Height / 2f),
                        Color.White);
                }
            }

            batch.End();
        }

        #endregion

        public void IncreaseScore(byte player, short amount)
        {
            game.players[player].avatar.score += amount;
        }

        public void PlayerMove(Player player, Vector2 pos)
        {
            player.avatar.position = pos;
            player.avatar.entity.Position = pos;
        }

        public void PlayerFire(Player player, float totalGameSeconds, float angle, Vector2 direction)
        {
            //NORTH
            if (angle > -0.3925f && angle < 0.3925f)
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 27, player.avatar.position.Y - 55), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 25, player.avatar.position.Y - 61), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0 || player.avatar.ammo[(int)GunType.shotgun] > 0)
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 4, player.avatar.position.Y - 55, totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 6, player.avatar.position.Y - 61, totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 5, player.avatar.position.Y - 57, totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 8, player.avatar.position.Y - 63, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle > 0.3925f && angle < 1.1775f) //NORTH-EAST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 3, player.avatar.position.Y - 67), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X, player.avatar.position.Y - 70), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 33, player.avatar.position.Y - 60, totalGameSeconds, angle));
                    }
                    else
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 27, player.avatar.position.Y - 60, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle > 1.1775f && angle < 1.9625f) //EAST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 30, player.avatar.position.Y - 58), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 30, player.avatar.position.Y - 60), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(new Vector2(player.avatar.position.X + 35, player.avatar.position.Y - 27), totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(new Vector2(player.avatar.position.X + 37, player.avatar.position.Y - 29), totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(new Vector2(player.avatar.position.X + 35, player.avatar.position.Y - 34), totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(new Vector2(player.avatar.position.X + 36, player.avatar.position.Y - 38), totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle > 1.19625f && angle < 2.7275f) //SOUTH-EAST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 45, player.avatar.position.Y - 27), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 47, player.avatar.position.Y - 28), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 27, player.avatar.position.Y, totalGameSeconds, angle));
                    }
                    else
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 32, player.avatar.position.Y - 5, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle > 2.7275f || angle < -2.7275f) //SOUTH
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 35, player.avatar.position.Y + 2), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X + 37, player.avatar.position.Y + 2), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 1, player.avatar.position.Y + 5, totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 4, player.avatar.position.Y + 7, totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X + 5, player.avatar.position.Y + 5, totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 7, player.avatar.position.Y + 5, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle < -1.9625f && angle > -2.7275f) //SOUTH-WEST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 3, player.avatar.position.Y + 19), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X, player.avatar.position.Y + 19), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 28, player.avatar.position.Y, totalGameSeconds, angle));
                    }
                    else
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 35, player.avatar.position.Y - 5, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle < -1.1775f && angle > -1.9625f) //WEST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 30, player.avatar.position.Y + 6), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 30, player.avatar.position.Y + 6), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        player.avatar.bullets.Add(new Vector4(new Vector2(player.avatar.position.X - 35, player.avatar.position.Y - 26), totalGameSeconds, angle));
                    }
                    else
                    {
                        if (player.avatar.character == 0)
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 37, player.avatar.position.Y - 34, totalGameSeconds, angle));
                        }
                        else
                        {
                            player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 36, player.avatar.position.Y - 38, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle < -0.3925f && angle > -1.1775f) //NORTH-WEST
            {
                if (player.avatar.currentgun == GunType.flamethrower)
                {
                    if (player.avatar.character == 0)
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 46, player.avatar.position.Y - 23), angle);
                    }
                    else
                    {
                        player.avatar.SetFlameThrower(new Vector2(player.avatar.position.X - 44, player.avatar.position.Y - 22), angle);
                    }
                }
                else if (player.avatar.currentgun == GunType.shotgun)
                {
                    player.avatar.shotgunbullets.Add(new ShotgunShell(player.avatar.position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (player.avatar.ammo[(int)GunType.machinegun] > 0)
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 36, player.avatar.position.Y - 57, totalGameSeconds, angle));
                    }
                    else
                    {
                        player.avatar.bullets.Add(new Vector4(player.avatar.position.X - 32, player.avatar.position.Y - 60, totalGameSeconds, angle));
                    }
                }
            }

            if (player.avatar.ammo[(int)player.avatar.currentgun] > 0)
            {
                player.avatar.ammo[(int)player.avatar.currentgun] -= 1;
            }

            switch (player.avatar.currentgun)
            {
                case GunType.pistol:
                    game.audio.PlayShot();
                    break;
                case GunType.machinegun:
                    game.audio.PlayMachineGun();
                    break;
                case GunType.flamethrower:
                    game.audio.PlayFlameThrower();
                    break;
                default:
                    game.audio.PlayShot();
                    break;
            }

            game.input.PlayShot(player);
        }

        private void JadeIdleTrunkAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeIdleTrunkDef");
            IdleLegsTexture = new List<Texture2D>();
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_legs_idle"));
            IdleTrunkTexture = new List<Texture2D>();
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_idle"));
            IdleTrunkOrigin = new List<Vector2>();
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[0].Width / 2, IdleTrunkTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            IdleTrunkAnimation = new List<Animation>();
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[0], frameSize, sheetSize, frameInterval)); // Define a new Animation instance
        }

        private void JadeRunEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeRunEDef");
            RunEastTexture = new List<Texture2D>();
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_run_E"));
            RunEastOrigin = new List<Vector2>();
            RunEastOrigin.Add(new Vector2(RunEastTexture[0].Width / 2, RunEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            RunEastAnimation = new List<Animation>();
            RunEastAnimation.Add(new Animation(RunEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotDef");
            PistolShotEastTexture = new List<Texture2D>();
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_E"));
            PistolShotEastOrigin = new List<Vector2>();
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[0].Width / 2, PistolShotEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotEastAnimation = new List<Animation>();
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotNEDef");
            PistolShotNETexture = new List<Texture2D>();
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_NE"));
            PistolShotNEOrigin = new List<Vector2>();
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[0].Width / 2, PistolShotNETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNEAnimation = new List<Animation>();
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotSEDef");
            PistolShotSETexture = new List<Texture2D>();
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_SE"));
            PistolShotSEOrigin = new List<Vector2>();
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[0].Width / 2, PistolShotSETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSEAnimation = new List<Animation>();
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotSouthDef");
            PistolShotSouthTexture = new List<Texture2D>();
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_S"));
            PistolShotSouthOrigin = new List<Vector2>();
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[0].Width / 2, PistolShotSouthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSouthAnimation = new List<Animation>();
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotNorthDef");
            PistolShotNorthTexture = new List<Texture2D>();
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_N"));
            PistolShotNorthOrigin = new List<Vector2>();
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[0].Width / 2, PistolShotNorthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNorthAnimation = new List<Animation>();
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotDef");
            ShotgunEastTexture = new List<Texture2D>();
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun"));
            ShotgunShotEastOrigin = new List<Vector2>();
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[0].Width / 2, PistolShotEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunShotEastAnimation = new List<Animation>();
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotNEDef");
            ShotgunNETexture = new List<Texture2D>();
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_NE"));
            ShotgunNEOrigin = new List<Vector2>();
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[0].Width / 2, ShotgunNETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNEAnimation = new List<Animation>();
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotSEDef");
            ShotgunSETexture = new List<Texture2D>();
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_SE"));
            ShotgunSEOrigin = new List<Vector2>();
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[0].Width / 2, ShotgunSETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSEAnimation = new List<Animation>();
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotSouthDef");
            ShotgunSouthTexture = new List<Texture2D>();
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_S"));
            ShotgunSouthOrigin = new List<Vector2>();
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[0].Width / 2, ShotgunSouthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSouthAnimation = new List<Animation>();
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotNorthDef");
            ShotgunNorthTexture = new List<Texture2D>();
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_N"));
            ShotgunNorthOrigin = new List<Vector2>();
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[0].Width / 2, ShotgunNorthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNorthAnimation = new List<Animation>();
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void EgonIdleTrunkAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[1].Width / 2, IdleTrunkTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonRunEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[1].Width / 2, RunEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[1].Width / 2, PistolShotEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[1].Width / 2, PistolShotNETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[1].Width / 2, PistolShotSETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[1].Width / 2, PistolShotSouthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[1].Width / 2, PistolShotNorthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[1].Width / 2, PistolShotEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[1].Width / 2, ShotgunNETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[1].Width / 2, ShotgunSETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[1].Width / 2, ShotgunSouthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[1].Width / 2, ShotgunNorthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[1], frameSize, sheetSize, frameInterval));

        }

        private void RayIdleTrunkAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[2].Width / 2, IdleTrunkTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayRunEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[2].Width / 2, RunEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[2].Width / 2, PistolShotEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[2].Width / 2, PistolShotNETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[2].Width / 2, PistolShotSETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[2].Width / 2, PistolShotSouthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[2].Width / 2, PistolShotNorthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[2].Width / 2, PistolShotEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[2].Width / 2, ShotgunNETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[2].Width / 2, ShotgunSETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[2].Width / 2, ShotgunSouthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[2], frameSize, sheetSize, frameInterval));
        }
        private void RayShotgunShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[2].Width / 2, ShotgunNorthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[2], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }

        private void PeterIdleTrunkAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[3].Width / 2, IdleTrunkTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterRunEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[3].Width / 2, RunEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[3].Width / 2, PistolShotEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[3].Width / 2, PistolShotNETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[3].Width / 2, PistolShotSETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[3].Width / 2, PistolShotSouthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[3].Width / 2, PistolShotNorthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[3].Width / 2, PistolShotEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotNorthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[3].Width / 2, ShotgunNETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotSouthEastAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[3].Width / 2, ShotgunSETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotSouthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[3].Width / 2, ShotgunSouthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotNorthAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[3].Width / 2, ShotgunNorthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }

        private void FlamethrowerAnimationLoad(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("FlameThrowerDef");
            flamethrowerTexture = game.Content.Load<Texture2D>(@"InGame/flamethrower");
            frameSize.X = int.Parse(definition.Attribute(FRAME_WIDTH).Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute(FRAME_HEIGHT).Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute(SHEET_COLUMNS).Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute(SHEET_ROWS).Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute(SPEED).Value, NumberStyles.Integer));
            flamethrowerAnimation = new Animation(flamethrowerTexture, frameSize, sheetSize, frameInterval);

        }

        private void UIStatsLoad()
        {
            UIStats = game.Content.Load<Texture2D>(@"UI\gameplay_gui_stats");
            UIStatsBlue = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_blue");
            UIStatsRed = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_red");
            UIStatsGreen = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_green");
            UIStatsYellow = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_yellow");
            UIPlayerBlue = game.Content.Load<Texture2D>(@"UI\gui_player_blue");
            UIPlayerRed = game.Content.Load<Texture2D>(@"UI\gui_player_red");
            UIPlayerGreen = game.Content.Load<Texture2D>(@"UI\gui_player_green");
            UIPlayerYellow = game.Content.Load<Texture2D>(@"UI\gui_player_yellow");
        }

        private void UIComponentsLoad()
        {
            Map = game.Content.Load<Texture2D>(Level.mapTextureFileName);
            bullet = game.Content.Load<Texture2D>(@"InGame/Photon");
            bulletorigin = new Vector2(bullet.Width / 2, bullet.Height / 2);
#if DEBUG
            PositionReference = game.Content.Load<Texture2D>(@"InGame/position_reference_temporal");
#endif
            CharacterShadow = game.Content.Load<Texture2D>(@"InGame/character_shadow");
            Explosion.Texture = game.Content.Load<Texture2D>(@"InGame/Explosion");
            cursorTexture = game.Content.Load<Texture2D>(@"InGame/GUI/aimcursor");

            jadeUI = game.Content.Load<Texture2D>(@"InGame/GUI/jade_gui");
            rayUI = game.Content.Load<Texture2D>(@"InGame/GUI/ray_gui");
            peterUI = game.Content.Load<Texture2D>(@"InGame/GUI/peter_gui");
            richardUI = game.Content.Load<Texture2D>(@"InGame/GUI/richard_gui");
            whiteLine = game.Content.Load<Texture2D>(@"menu/linea_menu");

            pause_icon = game.Content.Load<Texture2D>(@"UI/pause_iconWP");
            left_thumbstick = game.Content.Load<Texture2D>(@"UI/left_thumbstick");
            right_thumbstick = game.Content.Load<Texture2D>(@"UI/right_thumbstick");
        }

        private void FontsLoad()
        {
            arcade14 = game.Content.Load<SpriteFont>(@"InGame/GUI/ArcadeFont14");
            arcade28 = game.Content.Load<SpriteFont>(@"InGame/GUI/ArcadeFont28");
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");
        }

        private void FurnitureLoad()
        {
            float lIndex = 0.8f;
            FurnitureComparer furnitureComparer = new FurnitureComparer();
            foreach (Furniture furniture in Level.furnitureList)
            {
                furniture.Load(game);
            }

            Level.furnitureList.Sort(furnitureComparer);
            // Apply layer index to sorted list
            foreach (Furniture furniture in Level.furnitureList)
            {
                furniture.layerIndex = lIndex;
                lIndex -= 0.004f;
            }
        }
    }
}
