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

namespace ZombustersWindows
{
    public class GamePlayScreen : BackgroundScreen
    {        
        MyGame game;
        Rectangle uiBounds;
        NeutralInput playerOneInput;
        NeutralInput playerTwoInput;
        NeutralInput playerThreeInput;
        NeutralInput playerFourInput;
        MouseState mouseState;
        InputState input = new InputState();
        Texture2D gamerPictureBorder;
        private GamePlayMenu menu = null;
        private GameOverMenu gomenu = null;
        private bool bPaused = false;
        public bool bGameOver = false;
        public Vector2 accumMove, accumFire;
        public Texture2D Map;
        public List<Player> PlayersInSession;
        public Level Level;
        private LevelType currentLevel;
        private SubLevel.SubLevelType currentSublevel;

        public Texture2D livePowerUp, extraLivePowerUp, shotgunAmmoPowerUp, machinegunAmmoPowerUp, flamethrowerAmmoPowerUp, immunePowerUp, heart, shotgunammoUI, pistolammoUI, grenadeammoUI, flamethrowerammoUI;
        Texture2D bullet;
        Vector2 bulletorigin;
        Texture2D flamethrowerTexture, FT_CollisionTexture;
        Animation flamethrowerAnimation;
        List<Texture2D> IdleTrunkTexture, IdleLegsTexture, DiedTexture; // Idle Combo
        List<Texture2D> RunEastTexture; //, RunNETexture, RunSETexture, RunNorthTexture, RunSouthTexture; // Run Textures
        List<Texture2D> PistolShotEastTexture, PistolShotNETexture, PistolShotSETexture, PistolShotNorthTexture, PistolShotSouthTexture; // Pistol Shot Textures
        List<Texture2D> ShotgunEastTexture, ShotgunNETexture, ShotgunSETexture, ShotgunNorthTexture, ShotgunSouthTexture;
        List<Vector2> IdleTrunkOrigin, RunEastOrigin; //, RunNEOrigin, RunSEOrigin, RunNorthOrigin, RunSouthOrigin; // Animation Origin
        List<Vector2> PistolShotEastOrigin, PistolShotNEOrigin, PistolShotSEOrigin, PistolShotNorthOrigin, PistolShotSouthOrigin;
        List<Vector2> ShotgunShotEastOrigin, ShotgunNEOrigin, ShotgunSEOrigin, ShotgunNorthOrigin, ShotgunSouthOrigin;
        List<Animation> IdleTrunkAnimation; // Animacion de posicion estatica (tronco)
        List<Animation> RunEastAnimation; // RunNEAnimation, RunSEAnimation, RunNorthAnimation, RunSouthAnimation; // Animacion de Piernas (correr)
        List<Animation> PistolShotEastAnimation, PistolShotNEAnimation, PistolShotSEAnimation, PistolShotNorthAnimation, PistolShotSouthAnimation; // Animacion de Disparo con pistola (tronco)
        List<Animation> ShotgunShotEastAnimation, ShotgunNEAnimation, ShotgunSEAnimation, ShotgunNorthAnimation, ShotgunSouthAnimation;
#if DEBUG
        Texture2D PositionReference;
#endif
        Texture2D CharacterShadow;
        Texture2D cursorTexture;
        private Vector2 cursorPos;
        Texture2D gameover;
        Vector2 gameoverOrigin;
        Texture2D UIStats, jadeUI, rayUI, peterUI, richardUI, lineaBlanca;
        Texture2D UIStatsBlue, UIStatsRed, UIStatsGreen, UIStatsYellow, UIPlayerBlue, UIPlayerRed, UIPlayerGreen, UIPlayerYellow;
        SpriteFont arcade14, arcade28, courier, fixedfont;
        SpriteFont MenuHeaderFont, MenuInfoFont, MenuListFont;
        Texture2D pause_icon;
        Texture2D left_thumbstick;
        Texture2D right_thumbstick;
        public Random random;
        private float timer, timerplayer;
        private int subLevelIndex;
        //PRUEBA!!
        //ParticleEffect SmokeEffect;
        //Renderer particleRenderer;

        public List<TankState> Tanks;
        public List<ZombieState> Zombies;
        public List<PowerUp> PowerUpList;
        public GameplayState GamePlayStatus = GameplayState.NotPlaying;

        public GamePlayScreen(MyGame game, LevelType startingLevel, SubLevel.SubLevelType startingSublevel)
            : base()
        {            
            this.game = game;
            this.currentLevel = startingLevel;
            this.currentSublevel = startingSublevel;
#if DEBUG
            //this.currentSublevel = CSubLevel.SubLevel.Ten;
#endif
            menu = new GamePlayMenu();
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            gomenu = new GameOverMenu();
            gomenu.GameOverMenuOptionSelected += new EventHandler<MenuSelection>(menu_GameOverMenuOptionSelected);
            //gomenu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            //musicComponent = new MusicComponent(game);
            //game.Components.Add(musicComponent);
        }

        private int activeSeekers;
        public int ActiveSeekers
        {
            get { return activeSeekers; }
            set { activeSeekers = value; }
        }

        private int activeZombies;
        public int ActiveZombies
        {
            get { return activeZombies; }
            set { activeZombies = value; }
        }

        private int activeTanks;
        public int ActiveTanks
        {
            get { return activeTanks; }
            set { activeTanks = value; }
        }

        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            // Do nothing, game will resume on its own
        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            QuitToMenu();
        }

        void menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            switch (selection.Selection)
            {
                case 0: // Resume                    
                    break;
                case 1: // Help
                    ScreenManager.AddScreen(new HowToPlayScreen());
                    break;

                case 2: // Options
                    game.DisplayOptions(0);
                    break;

                case 3: // Restart
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, false);
                    break;

                case 4: // Quit
                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.GPReturnToMainMenuString, Strings.ConfirmReturnMainMenuString);
                    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
                    ScreenManager.AddScreen(confirmExitMessageBox);
                    break;

                case 5: // Show MarketPlace if Trial Mode On
                    /*if (licenseInformation.IsTrial)
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:PDP?PFN=44468RetrowaxGames.Zombusters_rhyy9bbdeb2be"));
                    }*/
                    break;
                default:
                    break;
            }           
        }

        void menu_GameOverMenuOptionSelected(Object sender, MenuSelection selection)
        {
            switch (selection.Selection)
            {
                case 0: // Restart WAVE
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, true);
                    break;
                case 1: // Restart LEVEL
                    game.Restart();
                    timer = 0;
                    bPaused = game.EndPause();
                    GamePlayStatus = GameplayState.StartLevel;
                    StartNewLevel(true, false);
                    break;
                case 2: // Quit
                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.GPReturnToMainMenuString, Strings.ConfirmReturnMainMenuString);
                    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
                    ScreenManager.AddScreen(confirmExitMessageBox);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called by this screen to quit back to menu
        /// </summary>
        public void QuitToMenu()
        {
            GameScreen[] screenList = ScreenManager.GetScreens();
            screenList[screenList.Length - 1].ExitScreen();
            screenList[screenList.Length - 2].ExitScreen();
            screenList[screenList.Length - 3].ExitScreen();
        }

        public override void Initialize()
        {
            int i;
            Zombies = new List<ZombieState>();
            Tanks = new List<TankState>();
            this.random = new Random(16);
            float RandomX;
            float RandomY;
            int RandomSpawnZone;
            int howManySpawnZones = 4;
            List<int> numplayersIngame = new List<int>();
            float zombielife;
            float speed;

            PowerUpList = new List<PowerUp>();

            GamePlayStatus = GameplayState.StartLevel;

            uiBounds = GetTitleSafeArea();

            Level = new Level(currentLevel);

            /*if (licenseInformation.IsTrial)
            {
                Level = new CLevel(game.Main, Level.One);
            }*/

            Level.Initialize(game);

            for (i=0; i<game.currentPlayers.Length; i++)
            {
                game.currentPlayers[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));
                if (game.currentPlayers[i].status == ObjectStatus.Active)
                {
                    numplayersIngame.Add(i);
                }
            }

            switch (currentSublevel)
            {
                case SubLevel.SubLevelType.One:
                    subLevelIndex = 0;
                    break;
                case SubLevel.SubLevelType.Two:
                    subLevelIndex = 1;
                    break;
                case SubLevel.SubLevelType.Three:
                    subLevelIndex = 2;
                    break;
                case SubLevel.SubLevelType.Four:
                    subLevelIndex = 3;
                    break;
                case SubLevel.SubLevelType.Five:
                    subLevelIndex = 4;
                    break;
                case SubLevel.SubLevelType.Six:
                    subLevelIndex = 5;
                    break;
                case SubLevel.SubLevelType.Seven:
                    subLevelIndex = 6;
                    break;
                case SubLevel.SubLevelType.Eight:
                    subLevelIndex = 7;
                    break;
                case SubLevel.SubLevelType.Nine:
                    subLevelIndex = 8;
                    break;
                case SubLevel.SubLevelType.Ten:
                    subLevelIndex = 9;
                    break;
                default: 
                    subLevelIndex = 0;
                    break;
            }

            ActiveZombies = 0;
            ActiveTanks = 0;
            
            //Get How many spawn zones are
            for (i = 0; i < Level.ZombieSpawnZones.Count-1; i++)
            {
                if (Level.ZombieSpawnZones[i].X == 0 && Level.ZombieSpawnZones[i].Y == 0 && Level.ZombieSpawnZones[i].Z == 0 && Level.ZombieSpawnZones[i].W == 0)
                {
                    howManySpawnZones--;
                }
            }

            switch (currentLevel)
            {
                case LevelType.One:
                    zombielife = 0.5f;
                    speed = 0.0f;
                    break;
                case LevelType.Two:
                    zombielife = 1.0f;
                    speed = 0.2f;
                    break;
                case LevelType.Three:
                    zombielife = 1.5f;
                    speed = 0.3f;
                    break;
                case LevelType.Four:
                    zombielife = 2.0f;
                    speed = 0.4f;
                    break;
                case LevelType.Five:
                    zombielife = 2.5f;
                    speed = 0.5f;
                    break;
                case LevelType.Six:
                    zombielife = 3.0f;
                    speed = 0.6f;
                    break;
                case LevelType.Seven:
                    zombielife = 3.5f;
                    speed = 0.7f;
                    break;
                case LevelType.Eight:
                    zombielife = 4.0f;
                    speed = 0.8f;
                    break;
                case LevelType.Nine:
                    zombielife = 4.5f;
                    speed = 0.9f;
                    break;
                case LevelType.Ten:
                    zombielife = 5.0f;
                    speed = 1.0f;
                    break;
                default:
                    zombielife = 1.0f;
                    speed = 0.0f;
                    break;
            }

            for (i = 0; i < Level.subLevelList[subLevelIndex].enemies.Zombies; i++)
            {
                RandomSpawnZone = this.random.Next(0, howManySpawnZones - 1);
                RandomX = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].X), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Y));
                RandomY = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Z), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].W));
                Zombies.Add(new ZombieState(game.Content.Load<Texture2D>(@"InGame/zombie" + this.random.Next(1, 6).ToString()), new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f, zombielife, speed));
                Zombies[i].behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                Zombies[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));
                Zombies[i].playerChased = numplayersIngame[random.Next(numplayersIngame.Count)];
                ActiveZombies++;
            }

            for (i = 0; i < Level.subLevelList[subLevelIndex].enemies.Tanks; i++)
            {
                RandomSpawnZone = this.random.Next(0, howManySpawnZones - 1);
                RandomX = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].X), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Y));
                RandomY = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Z), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].W));
                Tanks.Add(new TankState(game.Content.Load<Texture2D>(@"InGame/tank"), new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f));
                Tanks[i].behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                Tanks[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));

                Zombies[i].playerChased = numplayersIngame[random.Next(numplayersIngame.Count)];

                ActiveTanks++;
            }

            for (i=0; i<game.currentPlayers.Length-1; i++)
            {
                game.currentPlayers[i].position = Level.PlayerSpawnPosition[i];
                game.currentPlayers[i].entity.Position = Level.PlayerSpawnPosition[i];
            }
            //Efecto Particulas
            //SmokeEffect.Initialise();
            base.Initialize();
        }

        public override void LoadContent()
        {
            float lIndex = 0.8f;
            CFurnitureComparer furnitureComparer = new CFurnitureComparer();
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");
            Map = game.Content.Load<Texture2D>(Level.mapTextureFileName);
            bullet = game.Content.Load<Texture2D>(@"InGame/Photon");
            bulletorigin = new Vector2(bullet.Width / 2, bullet.Height / 2);
#if DEBUG
            PositionReference = game.Content.Load<Texture2D>(@"InGame/position_reference_temporal");
#endif

            DiedTexture = new List<Texture2D>
            {
                game.Content.Load<Texture2D>(@"InGame/Jade/girl_died"),
                game.Content.Load<Texture2D>(@"InGame/Egon/egon_died"),
                game.Content.Load<Texture2D>(@"InGame/Ray/ray_died"),
                game.Content.Load<Texture2D>(@"InGame/Peter/peter_died")
            };

            JadeIdleTrunkAnimationInit(doc);
            JadeRunEastAnimationInit(doc);
            JadePistolShotEastAnimationInit(doc);
            JadePistolShotNorthEastAnimationInit(doc);
            JadePistolShotSouthEastAnimationInit(doc);
            JadePistolShotSouthAnimationInit(doc);
            JadePistolShotNorthAnimationInit(doc);
            JadeShotgunShotEastAnimationInit(doc);
            JadeShotgunShotNorthEastAnimationInit(doc);
            JadeShotgunShotSouthEastAnimationInit(doc);
            JadeShotgunShotSouthAnimationInit(doc);
            JadeShotgunShotNorthAnimationInit(doc);

            EgonIdleTrunkAnimationInit(doc);
            EgonRunEastAnimationInit(doc);
            EgonPistolShotEastAnimationInit(doc);
            EgonPistolShotNorthEastAnimationInit(doc);
            EgonPistolShotSouthEastAnimationInit(doc);
            EgonPistolShotSouthAnimationInit(doc);
            EgonPistolShotNorthAnimationInit(doc);
            EgonShotgunShotEastAnimationInit(doc);
            EgonShotgunShotNorthEastAnimationInit(doc);
            EgonShotgunShotSouthEastAnimationInit(doc);
            EgonShotgunShotSouthAnimationInit(doc);
            EgonShotgunShotNorthAnimationInit(doc);

            RayIdleTrunkAnimationInit(doc);
            RayRunEastAnimationInit(doc);
            RayPistolShotEastAnimationInit(doc);
            RayPistolShotNorthEastAnimationInit(doc);
            RayPistolShotSouthEastAnimationInit(doc);
            RayPistolShotSouthAnimationInit(doc);
            RayPistolShotNorthAnimationInit(doc);
            RayShotgunShotEastAnimationInit(doc);
            RayShotgunShotNorthEastAnimationInit(doc);
            RayShotgunShotSouthEastAnimationInit(doc);
            RayShotgunShotSouthAnimationInit(doc);
            RayShotgunShotNorthAnimationInit(doc);

            PeterIdleTrunkAnimationInit(doc);
            PeterRunEastAnimationInit(doc);
            PeterPistolShotEastAnimationInit(doc);
            PeterPistolShotNorthEastAnimationInit(doc);
            PeterPistolShotSouthEastAnimationInit(doc);
            PeterPistolShotSouthAnimationInit(doc);
            PeterPistolShotNorthAnimationInit(doc);
            PeterShotgunShotEastAnimationInit(doc);
            PeterShotgunShotNorthEastAnimationInit(doc);
            PeterShotgunShotSouthEastAnimationInit(doc);
            PeterShotgunShotSouthAnimationInit(doc);
            PeterShotgunShotNorthAnimationInit(doc);

            CharacterShadow = game.Content.Load<Texture2D>(@"InGame/character_shadow");

            gameover = game.Content.Load<Texture2D>(@"InGame/gameover");
            gameoverOrigin = new Vector2(gameover.Width / 2, gameover.Height / 2);

            arcade14 = game.Content.Load<SpriteFont>(@"InGame/GUI/ArcadeFont14");
            arcade28 = game.Content.Load<SpriteFont>(@"InGame/GUI/ArcadeFont28");
            courier = game.Content.Load<SpriteFont>(@"InGame/GUI/CourierNew");
            fixedfont = game.Content.Load<SpriteFont>(@"InGame/GUI/Fixedv03");
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            Explosion.Texture = game.Content.Load<Texture2D>(@"InGame/Explosion");

            // GUI Stats
            UIStats = game.Content.Load<Texture2D>(@"UI\gameplay_gui_stats");
            UIStatsBlue = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_blue");
            UIStatsRed = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_red");
            UIStatsGreen = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_green");
            UIStatsYellow = game.Content.Load<Texture2D>(@"UI\gui_stats_bkg_yellow");
            UIPlayerBlue = game.Content.Load<Texture2D>(@"UI\gui_player_blue");
            UIPlayerRed = game.Content.Load<Texture2D>(@"UI\gui_player_red");
            UIPlayerGreen = game.Content.Load<Texture2D>(@"UI\gui_player_green");
            UIPlayerYellow = game.Content.Load<Texture2D>(@"UI\gui_player_yellow");

            // PowerUps
            livePowerUp = game.Content.Load<Texture2D>(@"InGame/live_powerup");
            extraLivePowerUp = game.Content.Load<Texture2D>(@"InGame/extralife_powerup");
            shotgunAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/shotgun_ammo_powerup");
            machinegunAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/machinegun_ammo_powerup");
            flamethrowerAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/flamethrower_ammo_powerup");
            immunePowerUp = game.Content.Load<Texture2D>(@"InGame/immune_ammo_powerup");

            // GUI
            heart = game.Content.Load<Texture2D>(@"InGame/GUI/heart");
            shotgunammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/shotgunammo");
            pistolammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/pistolammo");
            grenadeammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/grenadeammo");
            flamethrowerammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/flamethrowerammo");
            gamerPictureBorder = game.Content.Load<Texture2D>(@"UI/gamerpicture_border");
            jadeUI = game.Content.Load<Texture2D>(@"InGame/GUI/jade_gui");
            rayUI = game.Content.Load<Texture2D>(@"InGame/GUI/ray_gui");
            peterUI = game.Content.Load<Texture2D>(@"InGame/GUI/peter_gui");
            richardUI = game.Content.Load<Texture2D>(@"InGame/GUI/richard_gui");
            lineaBlanca = game.Content.Load<Texture2D>(@"menu/linea_menu");

            pause_icon = game.Content.Load<Texture2D>(@"UI/pause_iconWP");
            left_thumbstick = game.Content.Load<Texture2D>(@"UI/left_thumbstick");
            right_thumbstick = game.Content.Load<Texture2D>(@"UI/right_thumbstick");

            foreach (ZombieState zombie in Zombies)
            {
                zombie.LoadContent(game.Content);
            }

            foreach (TankState tank in Tanks)
            {
                tank.LoadContent(game.Content);
            }

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

            // Mouse Cursor
            cursorTexture = game.Content.Load<Texture2D>(@"InGame/GUI/aimcursor");

            base.LoadContent();
        }

        public class CFurnitureComparer : IComparer<Furniture>
        {
            public int Compare(Furniture x, Furniture y)
            {
                if ((x.Position.Y + x.Texture.Height) == (y.Position.Y + y.Texture.Height)) return 0;
                return ((x.Position.Y + x.Texture.Height) > (y.Position.Y + y.Texture.Height)) ? 1 : -1;
            }
        } 

        #region Input Processing
        public override void HandleInput(InputState input)
        {
            playerOneInput = ProcessPlayer(game.player1, input);

            if (game.player2.IsPlaying)
                playerTwoInput = ProcessPlayer(game.player2, input);
            if (game.player3.IsPlaying)
                playerThreeInput = ProcessPlayer(game.player3, input);
            if (game.player4.IsPlaying)
                playerFourInput = ProcessPlayer(game.player4, input);

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
            NeutralInput state = new NeutralInput();
            //state.Fire = false;
            state.Fire = Vector2.Zero;
            Vector2 stickLeft = Vector2.Zero;
            Vector2 stickRight = Vector2.Zero;
            GamePadState gpState = input.GetCurrentGamePadStates()[(int)player.Controller];

            // Get gamepad state
            if (VirtualThumbsticks.LeftThumbstick != Vector2.Zero || VirtualThumbsticks.RightThumbstick != Vector2.Zero)
            {
                stickLeft = VirtualThumbsticks.LeftThumbstick;
                stickRight = VirtualThumbsticks.RightThumbstick;
                state.Fire = VirtualThumbsticks.RightThumbstick;
            }
            else
            {
                stickLeft = gpState.ThumbSticks.Left;
                stickRight = gpState.ThumbSticks.Right;
                //state.Fire = (gpState.Triggers.Right > 0);

                state.Fire = gpState.ThumbSticks.Right;
            }

            if (player.Controller == PlayerIndex.One)
            {
                if (input.IsNewKeyPress(Keys.Left))
                {
                    stickLeft = stickLeft + new Vector2(-1, 0);
                }
                if (input.IsNewKeyPress(Keys.Right))
                {
                    stickLeft = stickLeft + new Vector2(1, 0);
                }
                if (input.IsNewKeyPress(Keys.Up))
                {
                    stickLeft = stickLeft + new Vector2(0, 1);
                }
                if (input.IsNewKeyPress(Keys.Down))
                {
                    stickLeft = stickLeft + new Vector2(0, -1);
                }

                if (input.GetCurrentMouseState().LeftButton == ButtonState.Pressed)
                {
                    
                    MouseState mouseState = input.GetCurrentMouseState();
                    // Derecha
                    if (mouseState.X > game.currentPlayers[0].position.X && (mouseState.X - game.currentPlayers[0].position.X >=100))
                    {
                        stickRight = stickRight + new Vector2(1, 0);
                        state.Fire = state.Fire + new Vector2(1, 0);
                    }

                    // Abajo
                    if (mouseState.Y < game.currentPlayers[0].position.Y && (game.currentPlayers[0].position.Y - mouseState.Y >= 100))
                    {
                        stickRight = stickRight + new Vector2(0, 1);
                        state.Fire = state.Fire + new Vector2(0, 1);
                    }

                    // Izquierda
                    if (mouseState.X < game.currentPlayers[0].position.X && (game.currentPlayers[0].position.X - mouseState.X >= 100))
                    {
                        stickRight = stickRight + new Vector2(-1, 0);
                        state.Fire = state.Fire + new Vector2(-1, 0);
                    }

                    // Arriba
                    if (mouseState.Y >= game.currentPlayers[0].position.Y && (mouseState.Y - game.currentPlayers[0].position.Y >= 100))
                    {
                        stickRight = stickRight + new Vector2(0, -1);
                        state.Fire = state.Fire + new Vector2(0, -1);
                    }
                }
            }

            if (input.IsNewButtonPress(Buttons.Y, player.Controller) 
                || input.IsNewKeyPress(Keys.R) 
                || input.GetCurrentMouseState().RightButton == ButtonState.Pressed)
            {
                state.ButtonY = true;
            }
            else
            {
                state.ButtonY = false;
            }

            if (input.IsNewButtonPress(Buttons.RightShoulder, player.Controller)
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
            byte i = 0, j;

            cursorPos = new Vector2(mouseState.X, mouseState.Y);
            input.Update();

            // If the user activates the menu...
            for (i = 0; i < game.currentPlayers.Length; i++)
            {
                if ((GamePad.GetState(game.currentPlayers[i].Player.Controller).Buttons.Start == ButtonState.Pressed)
                    || (input.IsNewKeyPress(Keys.Escape) && i == 0) || (input.IsNewKeyPress(Keys.Back) && i == 0))
                {
                    if (game.currentPlayers[i].status == ObjectStatus.Active)
                    {
                        if (!bPaused && (GamePlayStatus != GameplayState.StartLevel && GamePlayStatus != GameplayState.StageCleared))
                        {
                            this.ScreenManager.AddScreen(menu);

                            // Use this to keep from adding more than one menu to the stack 
                            bPaused = game.BeginPause();
                            GamePlayStatus = GameplayState.Pause;
                            return;
                        }
                    }
                    else
                    {
                        if (game.currentPlayers[i].lives > 0)
                        {
                            game.currentPlayers[i].status = ObjectStatus.Active;
                            //if (((Game1)this.ScreenManager.Game).currentPlayers[i].Player.SignedInGamer == null)
                            {
                                switch (game.currentPlayers[i].Player.Controller)
                                {
                                    case PlayerIndex.One:
                                        game.currentPlayers[i].Player.Name = Strings.PlayerOneString;
                                        break;
                                    case PlayerIndex.Two:
                                        game.currentPlayers[i].Player.Name = Strings.PlayerTwoString;
                                        break;
                                    case PlayerIndex.Three:
                                        game.currentPlayers[i].Player.Name = Strings.PlayerThreeString;
                                        break;
                                    case PlayerIndex.Four:
                                        game.currentPlayers[i].Player.Name = Strings.PlayerFourString;
                                        break;
                                    default:
                                        game.currentPlayers[i].Player.Name = Strings.PlayerOneString;
                                        break;
                                }
                            }


                            for (j = 0; j < game.currentPlayers.Length; j++)
                            {
                                if (game.currentPlayers[j].character == 0)
                                {
                                    game.currentPlayers[i].character = 1;
                                }
                                else if (game.currentPlayers[j].character == 1)
                                {
                                    game.currentPlayers[i].character = 2;
                                }
                                else
                                {
                                    game.currentPlayers[i].character = 3;
                                }
                            }
                        }
                    }
                }

            }
            bool hidden = coveredByOtherScreen || otherScreenHasFocus;

            // If the user covers this screen, pause.
            if (hidden && !game.IsPaused && (GamePlayStatus != GameplayState.StartLevel && GamePlayStatus != GameplayState.StageCleared))
            {
                bPaused = game.BeginPause();
            }

            // If we're now active again, unpause.
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
                        updatePlayerPlaying(gameTime);
                    }

                    // Progress the Character animation
                    updatePlayersAnimations(gameTime);

                    // FlameThrower Animation
                    flamethrowerAnimation.Update(gameTime);

                    //foreach (ZombieState zombie in Zombies)
                    for (i = 0; i < Zombies.Count; i++)
                    {
                        ZombieState zombie = Zombies[i];
                        {
                            zombie.Update(gameTime, game);
                        }
                    }

                    //foreach (TankState tank in Tanks)
                    for (i = 0; i < Tanks.Count; i++)
                    {
                        TankState tank = Tanks[i];
                        tank.Update(gameTime, game);
                    }

                    for (i = 0; i < game.currentPlayers.Length; i++)
                    {
                        if (game.currentPlayers[i].IsPlaying)
                        {
                            HandleCollisions(game.currentPlayers[i], i, game.totalGameSeconds);
                        }
                    }

                    // Cambio de nivel
                    if (CheckIfStageCleared() == true)
                    {
                         GamePlayStatus = GameplayState.StageCleared;
                    }
                }
                else if (GamePlayStatus == GameplayState.StageCleared)
                {
                    updatePlayerPlaying(gameTime);
                    updatePlayersAnimations(gameTime);

                    for (i = 0; i < game.currentPlayers.Length; i++)
                    {
                        if (game.currentPlayers[i].IsPlaying)
                        {
                            HandleCollisions(game.currentPlayers[i], i, game.totalGameSeconds);
                        }
                    }

                    foreach (ZombieState zombie in Zombies)
                    {
                        if (zombie.status == ObjectStatus.Dying)
                        {
                            zombie.Update(gameTime, ((MyGame)this.ScreenManager.Game));
                        }
                    }

                    foreach (TankState tank in Tanks)
                    {
                        if (tank.status == ObjectStatus.Dying)
                        {
                            tank.Update(gameTime, ((MyGame)this.ScreenManager.Game));
                        }
                    }


                    // Mostramos Cartel y esperamos unos segundos para mostrar el mensaje del siguiente nivel
                    /*if (licenseInformation.IsTrial)
                    {
                        if (currentSublevel == CSubLevel.SubLevel.Ten && currentLevel == Level.Three)
                        {
                            game.currentPlayers[0].lives = 1;
                            DestroyPlayer(0);
                        }
                    }*/

                    changeGamplayStatusAfterSomeTimeTo(gameTime, GameplayState.StartLevel);
                }
                else if (GamePlayStatus == GameplayState.StartLevel)
                {
                    updatePlayerPlaying(gameTime);
                    updatePlayersAnimations(gameTime);

                    for (i = 0; i < game.currentPlayers.Length; i++)
                    {
                        if (game.currentPlayers[i].IsPlaying)
                        {
                            HandleCollisions(game.currentPlayers[i], i, game.totalGameSeconds);
                        }
                    }

                    changeGamplayStatusAfterSomeTimeTo(gameTime, GameplayState.Playing);
                }
                else if (GamePlayStatus == GameplayState.GameOver)
                {
                    /*
                    // Mostramos Cartel y esperamos unos segundos para empezar la paritda de nuevo.
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer >= 5.0f)
                    {
                        GamePlayStatus = GameplayState.Playing;
                        timer = 0;
                    }
                     */

                    if (currentLevel == LevelType.FinalJuego)
                    {
                        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (timer >= 5.0f)
                        {
                            foreach (Avatar player in game.currentPlayers)
                            {
                                if (player.Player.IsPlaying)
                                {
                                    // Add Score Entry to the LeaderBoard
                                    if (game.topScoreListContainer != null)
                                    {
                                        player.Player.SaveLeaderBoard(player.score);
                                    }

                                }
                            }

                            QuitToMenu();
                            ScreenManager.AddScreen(new CreditsScreen(false));
                        }
                    }
                    else
                    {
                        //NUSE
                    }
                }

                foreach (PowerUp powerup in PowerUpList)
                {
                    powerup.Update(gameTime);
                }

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

        private void changeGamplayStatusAfterSomeTimeTo(GameTime gameTime, GameplayState gameplayState)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= 3.0f)
            {
                GamePlayStatus = gameplayState;
                timer = 0;
                if (gameplayState == GameplayState.StartLevel)
                {
                    StartNewLevel(false, false);
                }
            }
        }

        private void updatePlayersAnimations(GameTime gameTime)
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

        private void updatePlayerPlaying(GameTime gameTime)
        {
            for (byte i = 0; i < game.currentPlayers.Length - 1; i++)
            {
                switch (i)
                {
                    case 0:
                        UpdatePlayer(i, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, playerOneInput);
                        break;
                    case 1:
                        UpdatePlayer(i, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, playerTwoInput);
                        break;
                    case 2:
                        UpdatePlayer(i, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, playerThreeInput);
                        break;
                    case 3:
                        UpdatePlayer(i, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, playerFourInput);
                        break;
                    default:
                        UpdatePlayer(0, game.totalGameSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds, playerOneInput);
                        break;
                }
            }
        }

        #region Player-centric code
        public void UpdatePlayer(int player, float totalGameSeconds, 
            float elapsedGameSeconds, NeutralInput input)
        {
            if ((game.currentPlayers[player].status == ObjectStatus.Active) ||
                (game.currentPlayers[player].status == ObjectStatus.Immune))
            {
                ProcessInput(player, totalGameSeconds, elapsedGameSeconds, input);
            }            
        }

        public void ProcessInput(int player, float totalGameSeconds, 
            float elapsedGameSeconds, NeutralInput input)
        {
            if (input.StickLeftMovement.X > 0)
                accumMove.X += Move(input.StickLeftMovement.X, elapsedGameSeconds);
            if (input.StickLeftMovement.X < 0)
                accumMove.X -= Move(-input.StickLeftMovement.X, elapsedGameSeconds);
            if (input.StickLeftMovement.Y > 0)
                accumMove.Y -= Move(input.StickLeftMovement.Y, elapsedGameSeconds);
            if (input.StickLeftMovement.Y < 0)
                accumMove.Y += Move(-input.StickLeftMovement.Y, elapsedGameSeconds);

            if (input.StickRightMovement.X > 0)
                accumFire.X += Move(input.StickRightMovement.X, elapsedGameSeconds);
            if (input.StickRightMovement.X < 0)
                accumFire.X -= Move(-input.StickRightMovement.X, elapsedGameSeconds);
            if (input.StickRightMovement.Y > 0)
                accumFire.Y -= Move(input.StickRightMovement.Y, elapsedGameSeconds);
            if (input.StickRightMovement.Y < 0)
                accumFire.Y += Move(-input.StickRightMovement.Y, elapsedGameSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                accumMove.X += Move(1, elapsedGameSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                accumMove.X -= Move(1, elapsedGameSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                accumMove.Y += Move(1, elapsedGameSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                accumMove.Y -= Move(1, elapsedGameSeconds);

            game.currentPlayers[player].accumFire = accumFire;

            TryMove(player);

            //if (input.Fire == true)
            //{
            //    TryFire((byte)player, totalGameSeconds);
            //}

            if (input.ButtonY == true)
            {
                if (game.currentPlayers[player].currentgun == 0)
                {
                    game.currentPlayers[player].currentgun = 3;
                }
                else
                {
                    game.currentPlayers[player].currentgun = 0;
                }

                /*
                game.currentPlayers[player].currentgun += 1;
                if (game.currentPlayers[player].currentgun > 3)
                {
                    game.currentPlayers[player].currentgun = 0;
                }
                 */
            }

            if (input.ButtonRB == true)
            {
                timerplayer = 0;
            }

            float angle = 0.0f;
            input.Fire.Normalize();
            if ((input.Fire.X >= 0 || input.Fire.X <= 0) || (input.Fire.Y >= 0 || input.Fire.Y <= 0))
            {
                //angle = (float)Math.Acos(input.Fire.Y);
                //if (input.Fire.X < 0.0f)
                //    angle = -angle;
                Vector2 direction = Vector2.Normalize(input.Fire);
                angle = (float)Math.Atan2(input.Fire.X, input.Fire.Y);
                game.currentPlayers[player].shotAngle = angle;
                TryFire((byte)player, totalGameSeconds, angle, direction);
            }

            accumFire = Vector2.Zero;
        }

//GAMEPLAYHOST CODE
        private static float AngleFromAxis(Vector2 axis, Vector2 vector)
        {
            float retval = AbsoluteAngle(vector) - AbsoluteAngle(axis);
            if (retval > MathHelper.Pi)
                return retval - MathHelper.TwoPi;
            if (retval < -MathHelper.Pi)
                return retval + MathHelper.TwoPi;

            return retval;
        }
        private static float AbsoluteAngle(Vector2 a)
        {
            return (float)Math.Atan2(a.Y, a.X);
        }
        private static Vector2 VectorFromAngle(float a)
        {
            return new Vector2((float)Math.Sin(a), (float)-Math.Cos(a));
        }

        private bool PowerUpIsInRange(ZombieState zombie)
        {
            Rectangle ScreenBounds;
            ScreenBounds = new Rectangle(game.GraphicsDevice.Viewport.X + 60, 60, game.GraphicsDevice.Viewport.Width - 60, game.GraphicsDevice.Viewport.Height - 55);
            if (ScreenBounds.Intersects(new Rectangle(Convert.ToInt32(zombie.entity.Position.X), Convert.ToInt32(zombie.entity.Position.Y), zombie.ZombieTexture.Width, zombie.ZombieTexture.Height)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void SpawnPowerUp(ZombieState zombie)
        {


            if (this.random.Next(1, 16) == 8)
            {
                //int r = random.Next(0, 8);
                int r = this.random.Next(0, 3);
                switch (r)
                {
                    case 0: // Live

                        {
                            PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUp.Type.live));
                        }
                        break;



                    case 1: // Machinegun Ammo

                        {
                            PowerUpList.Add(new PowerUp(machinegunAmmoPowerUp, pistolammoUI, zombie.entity.Position, PowerUp.Type.machinegun_ammo));
                        }
                        break;

                    case 2: // Flamethrower Ammo

                        {
                            PowerUpList.Add(new PowerUp(flamethrowerAmmoPowerUp, flamethrowerammoUI, zombie.entity.Position, PowerUp.Type.flamethrower_ammo));
                        }
                        break;

                    case 3: // ExtraLife

                        {
                            PowerUpList.Add(new PowerUp(extraLivePowerUp, extraLivePowerUp, zombie.entity.Position, PowerUp.Type.extralife));
                        }
                        break;

                    case 4: // Shotgun Ammo

                        {
                            PowerUpList.Add(new PowerUp(shotgunAmmoPowerUp, shotgunammoUI, zombie.entity.Position, PowerUp.Type.shotgun_ammo));
                        }
                        break;

                    case 5: // Grenade Ammo

                        {
                            PowerUpList.Add(new PowerUp(grenadeammoUI, grenadeammoUI, zombie.entity.Position, PowerUp.Type.grenadelauncher_ammo));
                        }
                        break;

                    case 6: // Speed Buff

                        {
                            PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUp.Type.speedbuff));
                        }
                        break;

                    case 7: // Immune Buff

                        {
                            PowerUpList.Add(new PowerUp(immunePowerUp, immunePowerUp, zombie.entity.Position, PowerUp.Type.immunebuff));
                        }
                        break;
                    default:

                        {
                            PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUp.Type.live));
                        }
                        break;
                }
            }
        }




        public void HandleCollisions(Avatar player, byte playerId, float totalGameSeconds)
        {

            // Skip a lot of processing if the player isn't playing
            if (player.status == ObjectStatus.Inactive)
                return;

            //Zombies
            //foreach (ZombieState zombie in Zombies)
            for (int i = 0; i < Zombies.Count; i++)
            {
                ZombieState zombie = Zombies[i];
                if (zombie.status == ObjectStatus.Active)
                {
                    if (player.currentgun == 3 && player.ammo[player.currentgun] > 0)
                    {
                        if (player.accumFire.Length() > .5)
                        {
                            if (player.FlameThrowerRectangle.Intersects(new Rectangle((int)zombie.entity.Position.X, (int)zombie.entity.Position.Y, 48, (int)zombie.entity.Height)))
                            {
                                if (zombie.lifecounter > 1.0f)
                                {
                                    zombie.lifecounter -= 0.2f;
                                    zombie.isLoosingLife = true;
                                }
                                else
                                {
                                    ZombieDestroyed(zombie, (byte)playerId, (byte)player.currentgun);
                                    if (PowerUpIsInRange(zombie))
                                    {
                                        SpawnPowerUp(zombie);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int l = 0; l < player.bullets.Count; l++)
                        {
                            // Did player kill seeker?
                            if (DetectCollision(player.currentgun, player.bullets[l],
                                zombie.entity.Position, totalGameSeconds))
                            {

                                {
                                    if (zombie.lifecounter > 1.0f)
                                    {
                                        zombie.lifecounter -= 1.0f;
                                        zombie.isLoosingLife = true;
                                        player.bullets.RemoveAt(l);
                                    }
                                    else
                                    {
                                        ZombieDestroyed(zombie, (byte)playerId, (byte)player.currentgun);
                                        player.bullets.RemoveAt(l);
                                        if (PowerUpIsInRange(zombie))
                                        {
                                            SpawnPowerUp(zombie);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Did the player crash into the seeker?
                if (DetectCrash(player, zombie.entity.Position))
                {
                    // The seeker is live, crash the player
                    if (zombie.status == ObjectStatus.Active)
                    {

                        {
                            if (player.lifecounter <= 0)
                            {
                                //game.ZombieCrashed(zombie);
                                DestroyPlayer((byte)playerId);
                                player.lifecounter = 100;
                            }
                            else
                            {
                                player.isLoosingLife = true;
                                player.lifecounter -= 1;
                            }
                        }
                    }

                    // The seeker is a popup, reward the player
                    //else if (zombie.status == ObjectStatus.Dying)
                    //{
                    //REVISARRR!!
                    //game.IncreaseLife(playerId);
                    //game.ZombieCrashed(zombie);

                    //PowerUpList.Add(new CPowerUp(LivePowerUp, zombie.entity.Position));
                    //}
                }
            }

            //Tanks
            //foreach (TankState tank in Tanks)
            for (int i = 0; i < Tanks.Count; i++)
            {
                TankState tank = Tanks[i];
                if (tank.status == ObjectStatus.Active)
                {
                    for (int l = 0; l < player.bullets.Count; l++)
                    {
                        // Did player kill seeker?
                        if (DetectCollision(player.currentgun, player.bullets[l],
                            tank.entity.Position, totalGameSeconds))
                        {

                            {
                                TankDestroyed(tank, (byte)playerId);
                                player.bullets.RemoveAt(l);
                            }
                        }
                    }
                }
            }

            // PowerUps
            //foreach (CPowerUp powerup in PowerUpList)
            for (int i = 0; i < PowerUpList.Count; i++)
            {
                PowerUp powerup = PowerUpList[i];
                if (powerup.status == ObjectStatus.Active)
                {
                    if (DetectCrash(player, powerup.Position))
                    {

                        {
                            // ExtraLife
                            if (powerup.PUType == PowerUp.Type.extralife)
                            {
                                IncreaseLife(playerId);
                                powerup.status = ObjectStatus.Dying;
                            }

                            // Live
                            if (powerup.PUType == PowerUp.Type.live)
                            {
                                if (player.lifecounter < 100)
                                {
                                    player.lifecounter += powerup.Value;

                                    if (player.lifecounter > 100)
                                    {
                                        player.lifecounter = 100;
                                    }
                                }

                                powerup.status = ObjectStatus.Dying;
                            }

                            // Machine Gun Ammo
                            if (powerup.PUType == PowerUp.Type.machinegun_ammo)
                            {
                                player.ammo[0] += powerup.Value;
                                powerup.status = ObjectStatus.Dying;

                            }

                            // Shotgun Ammo
                            if (powerup.PUType == PowerUp.Type.shotgun_ammo)
                            {
                                player.ammo[1] += powerup.Value;
                                powerup.status = ObjectStatus.Dying;

                            }

                            // Grenade launcher Ammo
                            if (powerup.PUType == PowerUp.Type.grenadelauncher_ammo)
                            {
                                player.ammo[2] += powerup.Value;
                                powerup.status = ObjectStatus.Dying;

                            }

                            // Flamethrower Ammo
                            if (powerup.PUType == PowerUp.Type.flamethrower_ammo)
                            {
                                player.ammo[3] += powerup.Value;
                                powerup.status = ObjectStatus.Dying;

                            }

                            // Speedbuff
                            if (powerup.PUType == PowerUp.Type.speedbuff || powerup.PUType == PowerUp.Type.immunebuff)
                            {
                                //player. += powerup.Value;
                                powerup.status = ObjectStatus.Dying;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Call this method to determine if a bullet hit an enemy
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectCollision(int currentgun, Vector4 bullet, Vector2 enemy,
            double totalGameSeconds)
        {

            Vector2 pos = FindBulletPosition(bullet, totalGameSeconds);
            if (Vector2.Distance(pos, enemy) < 30)
                return true;

            return false;
        }
        /// <summary>
        /// Call this method to determine if the enemy crashed into the player
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectCrash(Avatar player, Vector2 enemy)
            //EnemyInfo enemyType)
        {
            if (player.status == ObjectStatus.Active)
            {
                float distance = Vector2.Distance(player.position, enemy);
                return (distance < Avatar.CrashRadius + 20.0f);// + enemyType.CrashRadius);
            }
            return false;
        }

        public static Vector2 FindShotgunBulletPosition(Vector3 bullet, float angle, int pelletcount, double totalGameSeconds)
        {
            Vector2 pos = Vector2.Zero;
            float overAngle = 0;
            float pelletsSpreadRadians = MathHelper.ToRadians(2.5f);

            switch (pelletcount)
            {
                case 0:
                    overAngle = 0;
                    break;
                case 1:
                    overAngle += 0.25f;
                    break;
                case 2:
                    overAngle -= 0.25f;
                    break;
                default:
                    overAngle = 0;
                    break;
            }

            if (angle > -0.3925f && angle < 0.3925f) //NORTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 0.3925f && angle < 1.1775f) //NORTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(angle) + overAngle + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(angle) + overAngle - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 1.1775f && angle < 1.9625f) //EAST
            {
                pos.X = bullet.X + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (angle > 1.19625f && angle < 2.7275f) //SOUTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(angle) + overAngle + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(angle) + overAngle + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 2.7275f || angle < -2.7275f) //SOUTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle < -1.9625f && angle > -2.7275f) //SOUTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(angle) + overAngle - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(angle) + overAngle + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle < -1.1775f && angle > -1.9625f) //WEST
            {
                pos.X = bullet.X - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }

            else if (angle < -0.3925f && angle > -1.1775f) //NORTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(angle) + overAngle - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(angle) + overAngle - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }

            return pos;
        }


        public static float bulletspeed = 400;
        public static Vector2 FindBulletPosition(Vector4 bullet, double totalGameSeconds)
        {
            Vector2 pos = Vector2.Zero;

            //pos.X = bullet.X;
            //pos.Y = bullet.Y - (bulletspeed * ((float)totalGameSeconds - bullet.Z));

            if (bullet.W > -0.3925f && bullet.W < 0.3925f) //NORTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 0.3925f && bullet.W < 1.1775f) //NORTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(bullet.W) - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 1.1775f && bullet.W < 1.9625f) //EAST
            {
                pos.X = bullet.X + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (bullet.W > 1.19625f && bullet.W < 2.7275f) //SOUTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 2.7275f || bullet.W < -2.7275f) //SOUTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W < -1.9625f && bullet.W > -2.7275f) //SOUTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(bullet.W) - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W < -1.1775f && bullet.W > -1.9625f) //WEST
            {
                pos.X = bullet.X - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (bullet.W < -0.3925f && bullet.W > -1.1775f) //NORTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(bullet.W) - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(bullet.W) - (bulletspeed * ((float)totalGameSeconds - bullet.Z));
            }

            return pos;
        }

        private void DestroyPlayer(byte Player)
        {
            int i;
            int livesleft = 0;

            if ((Player == 0) && (game.currentPlayers[0].lives == 1))
            {
                if ((game.currentPlayers[1].status == ObjectStatus.Inactive) ||
                    (game.currentPlayers[1].lives == 0))
                    GameOver(0);
                else
                    PlayerDestroyed(0);
            }
            else if ((Player == 1) && (game.currentPlayers[1].lives == 1))
            {
                if ((game.currentPlayers[0].status == ObjectStatus.Inactive) ||
                    (game.currentPlayers[0].lives == 0))
                    GameOver(1);
                else
                    PlayerDestroyed(1);
            }
            else if ((Player == 2) && (game.currentPlayers[1].lives == 1))
            {
                if ((game.currentPlayers[0].status == ObjectStatus.Inactive) ||
                    (game.currentPlayers[0].lives == 0))
                    GameOver(2);
                else
                    PlayerDestroyed(2);
            }
            else if ((Player == 3) && (game.currentPlayers[1].lives == 1))
            {
                if ((game.currentPlayers[0].status == ObjectStatus.Inactive) ||
                    (game.currentPlayers[0].lives == 0))
                    GameOver(3);
                else
                    PlayerDestroyed(3);
            }
            else
                PlayerDestroyed(Player);

            // Check if Game Over for All
            for (i = 0; i < game.currentPlayers.Length - 1; i++)
            {
                if (game.currentPlayers[i].lives > 0 && (game.currentPlayers[i].status != ObjectStatus.Inactive))
                {
                    livesleft++;
                }
            }

            if (livesleft == 0)
            {

                GamePlayStatus = GameplayState.GameOver;
                this.ScreenManager.AddScreen(gomenu);

                if (game.currentPlayers[0].IsPlaying)
                {
                    if (game.topScoreListContainer != null && game.currentPlayers[0].score > 250)
                    {
                        game.currentPlayers[0].Player.SaveLeaderBoard(game.currentPlayers[0].score);
                        game.currentPlayers[0].Player.SaveGame(Level.getLevelNumber(currentLevel));
                    }
                }
            }
        }
//END GAMEPLAYHOST CODE

        public float DistanceLineSegmentToPoint(Vector2 A, Vector2 B, Vector2 p)
        {

            //get the normalized line segment vector
            Vector2 v = B - A;
            v.Normalize();

            //determine the point on the line segment nearest to the point p
            float distanceAlongLine = Vector2.Dot(p, v) - Vector2.Dot(A, v);
            Vector2 nearestPoint;
            if (distanceAlongLine < 0)
            {
                //closest point is A
                nearestPoint = A;
            }
            else if (distanceAlongLine > Vector2.Distance(A, B))
            {
                //closest point is B
                nearestPoint = B;
            }
            else
            {
                //closest point is between A and B... A + d  * ( ||B-A|| )
                nearestPoint = A + distanceAlongLine * v;
            }

            //Calculate the distance between the two points
            float actualDistance = Vector2.Distance(nearestPoint, p);
            return actualDistance;

        }

        private static float Move(float input, float ElapsedGameSeconds)
        {
            return input * ElapsedGameSeconds * Avatar.PixelsPerSecond;
        }             

        private void TryMove(int Player)
        {
            bool collision = false;
            game.currentPlayers[Player].accumMove = accumMove;

            if (accumMove.Length() > .5)
            {                
                // Find out how far we are allowed to move
                Vector2 move = game.currentPlayers[Player].VerifyMove(accumMove);
/*
                if (game.currentPlayers[Player].ObjectAvoidCalc != Vector2.Zero)
                {
                    if (game.currentPlayers[Player].ObjectAvoidCalc.X < 0)
                    {
                        move.X += move.X * 1.1f;
                    }
                    else
                    {
                        move.X -= move.X * 1.1f;
                    }

                    if (game.currentPlayers[Player].ObjectAvoidCalc.Y < 0)
                    {
                        move.Y += move.Y * 1.1f;
                    }
                    else
                    {
                        move.Y -= move.Y * 1.1f;
                    }
                }
*/

                // Send our movement event to Game1
                Vector2 pos = game.currentPlayers[Player].position + move;

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
                    float actualDistance = DistanceLineSegmentToPoint(Level.gameWorld.Walls[i].From, Level.gameWorld.Walls[i].To, pos);
                    if (actualDistance <= 5.0f)
                    {
                        collision = true;
                    }
                }

                if (!collision)
                {
                    PlayerMove((byte)Player, pos);
                }
                
                // Zero out accumulated input
                accumMove = Vector2.Zero;
            }
        }

        private void TryFire(byte Player, float TotalGameSeconds, float angle, Vector2 direction)
        {
            int RateOfFire;
            if (game.currentPlayers[Player].status != ObjectStatus.Active && game.currentPlayers[Player].status != ObjectStatus.Immune)
                return;

            // Check if we have ammo; if not we change the current gun to pistol
            if (game.currentPlayers[Player].ammo[game.currentPlayers[Player].currentgun] == 0)
            {
                game.currentPlayers[Player].currentgun = 0;
            }

            // if the time between the last shot and the current time 
            // is less than 1/ROF, don't fire
            if (game.currentPlayers[Player].currentgun == 0 && game.currentPlayers[Player].ammo[0] > 0)
            {
                RateOfFire = 10;
            }
            else if (game.currentPlayers[Player].currentgun == 3 && game.currentPlayers[Player].ammo[3] > 0)
            {
                RateOfFire = 15;
            }
            else
            {
                RateOfFire = game.currentPlayers[Player].RateOfFire;
            }

            if (game.currentPlayers[Player].currentgun == 0 || (game.currentPlayers[Player].currentgun != 0 && game.currentPlayers[Player].ammo[game.currentPlayers[Player].currentgun] > 0))
            {
                if (game.currentPlayers[Player].VerifyFire(TotalGameSeconds, RateOfFire))
                {
                    PlayerFire(Player, TotalGameSeconds, angle, direction);
                }
            }
        }

#endregion

        public Boolean CheckIfStageCleared()
        {
            int enemiesLeft = ActiveZombies + ActiveTanks;

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
            int i, RandomX, RandomY;
            int RandomSpawnZone;
            int howManySpawnZones = 4;
            List<int> numplayersIngame = new List<int>();
            float zombielife, speed;
            float lIndex = 0.8f;
            CFurnitureComparer furnitureComparer = new CFurnitureComparer();
            this.random = new Random(16);

            if (!restartLevel)
            {
                if (subLevelIndex == 9)
                {
                    currentLevel = Level.getNextLevel(currentLevel);
                    if (currentLevel == LevelType.FinalJuego)
                    {
                        {
                            GamePlayStatus = GameplayState.GameOver;
                        }
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

                        for (i = 0; i < game.currentPlayers.Length - 1; i++)
                        {
                            game.currentPlayers[i].position = Level.PlayerSpawnPosition[i];
                            game.currentPlayers[i].entity.Position = Level.PlayerSpawnPosition[i];
                        }

                        // Save Game
                        foreach (Avatar player in game.currentPlayers)
                        {
                            if (player.Player.IsPlaying)
                            {
                                player.Player.SaveGame(Level.getLevelNumber(currentLevel));
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

                for (i = 0; i < game.currentPlayers.Length - 1; i++)
                {
                    game.currentPlayers[i].position = Level.PlayerSpawnPosition[i];
                    game.currentPlayers[i].entity.Position = Level.PlayerSpawnPosition[i];
                }
            }

            //((Game1)this.ScreenManager.Game).PowerUpList.Clear();
            Zombies.Clear();
            Tanks.Clear();
            //((Game1)this.ScreenManager.Game).audio.

            if (currentLevel != LevelType.FinalJuego)
            {

                for (i = 0; i < game.currentPlayers.Length; i++)
                {
                    game.currentPlayers[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));
                    if (game.currentPlayers[i].status == ObjectStatus.Active || game.currentPlayers[i].status == ObjectStatus.Immune)
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

                ActiveZombies = 0;
                ActiveTanks = 0;

                //Get How many spawn zones are
                for (i = 0; i < Level.ZombieSpawnZones.Count - 1; i++)
                {
                    if (Level.ZombieSpawnZones[i].X == 0 && Level.ZombieSpawnZones[i].Y == 0 && Level.ZombieSpawnZones[i].Z == 0 && Level.ZombieSpawnZones[i].W == 0)
                    {
                        howManySpawnZones--;
                    }
                }

                switch (currentLevel)
                {
                    case LevelType.One:
                        zombielife = 1.0f;
                        speed = 0.0f;
                        break;
                    case LevelType.Two:
                        zombielife = 1.5f;
                        speed = 0.2f;
                        break;
                    case LevelType.Three:
                        zombielife = 2.0f;
                        speed = 0.3f;
                        break;
                    case LevelType.Four:
                        zombielife = 2.5f;
                        speed = 0.4f;
                        break;
                    case LevelType.Five:
                        zombielife = 3.0f;
                        speed = 0.5f;
                        break;
                    case LevelType.Six:
                        zombielife = 3.5f;
                        speed = 0.6f;
                        break;
                    case LevelType.Seven:
                        zombielife = 4.0f;
                        speed = 0.7f;
                        break;
                    case LevelType.Eight:
                        zombielife = 4.5f;
                        speed = 0.8f;
                        break;
                    case LevelType.Nine:
                        zombielife = 5.0f;
                        speed = 0.9f;
                        break;
                    case LevelType.Ten:
                        zombielife = 5.5f;
                        speed = 1.0f;
                        break;
                    default:
                        zombielife = 1.0f;
                        speed = 0.0f;
                        break;
                }

                for (i = 0; i < Level.subLevelList[subLevelIndex].enemies.Zombies; i++)
                {
                    RandomSpawnZone = this.random.Next(0, howManySpawnZones - 1);
                    RandomX = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].X), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Y));
                    RandomY = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Z), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].W));
                    float subspeed = subLevelIndex / 10;
                    Zombies.Add(new ZombieState(game.Content.Load<Texture2D>(@"InGame/zombie" + this.random.Next(1, 6).ToString()), new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f, zombielife, speed + subspeed));
                    Zombies[i].behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                    Zombies[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));

                    Zombies[i].playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                    ActiveZombies++;
                }

                for (i = 0; i < Level.subLevelList[subLevelIndex].enemies.Tanks; i++)
                {
                    RandomSpawnZone = this.random.Next(0, howManySpawnZones - 1);
                    RandomX = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].X), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Y));
                    RandomY = this.random.Next(Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].Z), Convert.ToInt32(Level.ZombieSpawnZones[RandomSpawnZone].W));
                    Tanks.Add(new TankState(game.Content.Load<Texture2D>(@"InGame/tank"), new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f));
                    Tanks[i].behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                    Tanks[i].behaviors.AddBehavior(new ObstacleAvoidance(ref Level.gameWorld, 15.0f));

                    Zombies[i].playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                    ActiveTanks++;
                }

                foreach (ZombieState zombie in Zombies)
                {
                    zombie.LoadContent(game.Content);
                }

                foreach (TankState tank in Tanks)
                {
                    tank.LoadContent(game.Content);
                }
            }
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (game.currentGameState != GameState.Paused)
            {
                Color c = new Color(new Vector4(0, 0, 0, 0));
                this.ScreenManager.GraphicsDevice.Clear(c);

                // Draw Map Layer
                DrawMap(Map);

                // Draw PowerUps
                foreach (PowerUp powerup in PowerUpList)
                {
                    powerup.Draw(this.ScreenManager.SpriteBatch, gameTime, MenuInfoFont);
                }


                // BACK TO FRONT SORT MODE
                //------------------------
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                // Draw each player
                foreach (Avatar cplayer in game.currentPlayers)
                {
                    if (cplayer.IsPlaying)
                    {

                        DrawPlayer(cplayer, game.totalGameSeconds, gameTime, Level.furnitureList);
                    }
                }

                // Draw the enemies
                //DrawEnemies(game.TotalGameSeconds);

                if (GamePlayStatus == GameplayState.StartLevel || GamePlayStatus == GameplayState.Playing || GamePlayStatus == GameplayState.Pause)
                {
                    foreach (ZombieState zombie in Zombies)
                    {
                        zombie.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, MenuInfoFont, Level.furnitureList, gameTime);
                    }

                    foreach (TankState tank in Tanks)
                    {
                        tank.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, MenuInfoFont, Level.furnitureList);
                    }
                }

                if (GamePlayStatus == GameplayState.StageCleared)
                {
                    foreach (ZombieState zombie in Zombies)
                    {
                        if (zombie.status == ObjectStatus.Dying)
                        {
                            zombie.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, MenuInfoFont, Level.furnitureList, gameTime);
                        }
                    }

                    foreach (TankState tank in Tanks)
                    {
                        if (tank.status == ObjectStatus.Dying)
                        {
                            tank.Draw(this.ScreenManager.SpriteBatch, game.totalGameSeconds, MenuInfoFont, Level.furnitureList);
                        }
                    }
                }

                // Draw Furniture
                foreach (Furniture furniture in Level.furnitureList)
                {
                    furniture.Draw(this.ScreenManager.SpriteBatch, MenuInfoFont);
                }

                this.ScreenManager.SpriteBatch.End();
                //----------------------------
                // END BACK TO FRONT SORT MODE

                foreach (Furniture furniture in Level.furnitureList)
                {
                    if (furniture.Type == FurnitureType.CocheArdiendo)
                    {
                        //furniture.particleRenderer.RenderEffect(furniture.SmokeEffect);
                    }
                }

                // Draw each player's bullets
                foreach (Avatar cplayer in game.currentPlayers)
                {
                    if (cplayer.IsPlaying)
                    {
                        DrawShotgunShots(cplayer.shotgunbullets, game.totalGameSeconds);
                        DrawBullets(cplayer.bullets, game.totalGameSeconds);
                        //DrawFlameThrower(cplayer, game.TotalGameSeconds);
                        //cplayer.DrawFlameThrower();
                    }
                }

                // Perlin Noise effect draw
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
#if DEBUG
            Level.gameWorld.Draw(this.ScreenManager.SpriteBatch, gameTime, this.ScreenManager.SpriteBatch);
#endif

                this.ScreenManager.SpriteBatch.End();
                // end Perlin Noise effect


                //#if XBOX
                // Draw the score
                DrawUI(gameTime);
                //#endif

                if (GamePlayStatus == GameplayState.StageCleared)
                {
                    DrawStageCleared();
                }

                if (GamePlayStatus == GameplayState.StartLevel)
                {
                    DrawStartLevel();
                }

                // If the game is over, draw a special graphic
                if (GamePlayStatus == GameplayState.GameOver)
                {
                    DrawGameOver();
                }

                // Draw the Storage Device Icon
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);


                if (game.player1.Options == InputMode.Keyboard)
                {
                    // Draw Cursor
                    this.ScreenManager.SpriteBatch.Draw(cursorTexture, cursorPos, Color.White);
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
            batch.Begin();

            batch.Draw(map, new Rectangle(0, 0, 1280, 720), Color.White);

            batch.End();
        }

        // Draw Text when all enemies are dead
        private void DrawStageCleared()
        {
            this.ScreenManager.FadeBackBufferToBlack(64);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2,
                uiBounds.Y + uiBounds.Height / 2);

            // Linea Blanca
            this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width / 2, UICenter.Y - 10), Color.White);

            // CLEARED String
            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, Strings.ClearedGameplayString.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(Strings.ClearedGameplayString.ToUpper()).X)/2, UICenter.Y), Color.White);

            // Prepare for next Wave String
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.PrepareNextWaveGameplayString.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(Strings.PrepareNextWaveGameplayString.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);

            // Linea Blanca
            this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width / 2, UICenter.Y + 90), Color.White);

            this.ScreenManager.SpriteBatch.End();
        }

        // Draw the next wave number
        private void DrawStartLevel()
        {
            string levelshowstring;
            int timeLeftWaitingPlayers;
            int fixedTimeLeft = 5;
            this.ScreenManager.FadeBackBufferToBlack(64);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2,
                uiBounds.Y + uiBounds.Height / 2);

            // Linea Blanca
            this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width /2, UICenter.Y - 10), Color.White);

            // LEVEL
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

            //levelshowstring = Strings.LevelSelectMenuString + " " + currentLevel.ToString();
            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y), Color.White);

            // WAVE
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

            //levelshowstring = Strings.WaveGameplayString + " " + currentSublevel.ToString();
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);

            // Linea Blanca
            this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width / 2, UICenter.Y + 90), Color.White);

            // Draw Waiting for Begin Text on Multiplayer
            if (game.currentGameState == GameState.WaitingToBegin)
            {
                timeLeftWaitingPlayers = fixedTimeLeft; //- (int)timerWaitingToBegin;
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.WaitingForPlayersMenuString.ToUpper(), new Vector2(uiBounds.Left, uiBounds.Height), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, timeLeftWaitingPlayers.ToString(), new Vector2(uiBounds.Left + MenuInfoFont.MeasureString(Strings.WaitingForPlayersMenuString.ToUpper()).X + 5, uiBounds.Height), Color.White);
            }

            this.ScreenManager.SpriteBatch.End();
        }

        private void DrawGameOver()
        {
            if (bGameOver)
            {
                this.ScreenManager.FadeBackBufferToBlack(64);
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2, 
                    uiBounds.Y + uiBounds.Height / 2);
                this.ScreenManager.SpriteBatch.Draw(gameover, UICenter, 
                    null, Color.Red, 0, gameoverOrigin, 1.0f, SpriteEffects.None, 
                    1.0f);
                this.ScreenManager.SpriteBatch.End();
            }

            // SHOW END GAME
            if (currentLevel == LevelType.FinalJuego)
            {
                string levelshowstring;
                this.ScreenManager.FadeBackBufferToBlack(64);
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                Vector2 UICenter = new Vector2(uiBounds.X + uiBounds.Width / 2,
                uiBounds.Y + uiBounds.Height / 2);

                // Linea Blanca
                this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width / 2, UICenter.Y - 10), Color.White);

                // CONGRATULATIONS
                levelshowstring = Strings.CongratssEndGameString;
                this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuHeaderFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y), Color.White);

                // YOU SAVE THE CITY
                levelshowstring = Strings.YouSavedCityEndGameString;
                this.ScreenManager.SpriteBatch.DrawString(MenuListFont, levelshowstring.ToUpper(), new Vector2(UICenter.X - Convert.ToInt32(MenuListFont.MeasureString(levelshowstring.ToUpper()).X) / 2, UICenter.Y + 50), Color.White);

                // Linea Blanca
                this.ScreenManager.SpriteBatch.Draw(lineaBlanca, new Vector2(UICenter.X - lineaBlanca.Width / 2, UICenter.Y + 90), Color.White);

                this.ScreenManager.SpriteBatch.End();
            }

        }

        public bool isInRange(Avatar state, Furniture furniture)
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

        private void DrawPlayer(Avatar state, double TotalGameSeconds, GameTime gameTime, List<Furniture> furniturelist)
        {
            //PRUEBA!!
            //AvatarCircle circulo = new AvatarCircle(this.game.GraphicsDevice, new Vector2(state.position.X + CharacterAnimation.frameSize.X/2, state.position.Y + CharacterAnimation.frameSize.Y), 15.0f, state.color);

            // Initialize our own SpriteBatch.  We want default Alpha blending, and the render
            // states set by SpriteBatch to be reset when End() is called.
            //SpriteBatch batch = this.ScreenManager.SpriteBatch;
            //this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            //batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, 
                //SaveStateMode.SaveState);

            float layerIndex = GetLayerIndex(state, furniturelist);
            Vector2 offsetPosition = new Vector2(-20, -55);

            if (GamePlayStatus == GameplayState.Playing)
            {
                timerplayer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timerplayer <= 5.0f)
                {
                    foreach (Avatar player in game.currentPlayers)
                    {
                        if (player.IsPlaying)
                        {
                            // Draw Player Name and Highlight for Blue
                            if (state.color == Color.Blue)
                            {
                                this.ScreenManager.SpriteBatch.Draw(UIPlayerBlue, new Vector2(state.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerBlue.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                            }

                            // Draw Player Name and Highlight for Red
                            if (state.color == Color.Red)
                            {
                                this.ScreenManager.SpriteBatch.Draw(UIPlayerRed, new Vector2(state.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerRed.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                            }

                            // Draw Player Name and Highlight for Green
                            if (state.color == Color.Green)
                            {
                                this.ScreenManager.SpriteBatch.Draw(UIPlayerGreen, new Vector2(state.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerGreen.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                            }

                            // Draw Player Name and Highlight for Yellow
                            if (state.color == Color.Yellow)
                            {
                                this.ScreenManager.SpriteBatch.Draw(UIPlayerYellow, new Vector2(state.position.X + IdleTrunkAnimation[0].frameSize.X / 2 - UIPlayerYellow.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                            }
                        }
                    }
                }
            }
            else
            {
                timerplayer = 0;
            }

            switch (state.status)
            {
                case ObjectStatus.Inactive:  
                    break; // Draw nothing
                case ObjectStatus.Active:
                    Color color;
                    if (state.isLoosingLife == true)
                    {
                        color = Color.Red;
                    }
                    else
                    {
                        color = Color.White;
                    }

                    // Draws our avatar at the current position with no tinting
                    if (state.accumFire.Length() > .5)
                    {
                        if (state.shotAngle > -0.3925f && state.shotAngle < 0.3925f) //NORTH
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 6 + offsetPosition.X, state.position.Y + offsetPosition.Y - 34), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 12 + offsetPosition.X, state.position.Y + offsetPosition.Y - 36), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 12 + offsetPosition.X, state.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 6 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 34), 26, ShotgunNorthTexture[state.character].Height),
                                            new Rectangle(0, 0, 26, ShotgunNorthTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 12 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 36), 21, ShotgunNorthTexture[state.character].Height),
                                            new Rectangle(0, 0, 21, ShotgunNorthTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle > 0.3925f && state.shotAngle < 1.1775f) //NORTH-EAST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    break;
                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[state.character].Height),
                                            new Rectangle(0, 0, 59, ShotgunNETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 53, ShotgunNETexture[state.character].Height),
                                            new Rectangle(0, 0, 53, ShotgunNETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle > 1.1775f && state.shotAngle < 1.9625f) //EAST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle > 1.19625f && state.shotAngle < 2.7275f) //SOUTH-EAST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[state.character].Height),
                                            new Rectangle(0, 0, 58, ShotgunSETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 54, ShotgunSETexture[state.character].Height),
                                            new Rectangle(0, 0, 54, ShotgunSETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle > 2.7275f || state.shotAngle < -2.7275f) //SOUTH
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 3 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 21, ShotgunSouthTexture[state.character].Height),
                                            new Rectangle(0, 0, 21, ShotgunSouthTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 20, ShotgunSouthTexture[state.character].Height),
                                            new Rectangle(0, 0, 20, ShotgunSouthTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle < -1.9625f && state.shotAngle > -2.7275f) //SOUTH-WEST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 1 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 4 + offsetPosition.X, state.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 1 + offsetPosition.X - 29), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[state.character].Height),
                                            new Rectangle(0, 0, 58, ShotgunSETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 4 + offsetPosition.X - 27), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 54, ShotgunSETexture[state.character].Height),
                                            new Rectangle(0, 0, 54, ShotgunSETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle < -1.1775f && state.shotAngle > -1.9625f) //WEST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 4 + offsetPosition.X, state.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 2 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (state.shotAngle < -0.3925f && state.shotAngle > -1.1775f) //NORTH-WEST
                        {
                            switch (state.currentgun)
                            {
                                case 0:
                                    if (state.character == 0)
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    else
                                    {
                                        if (state.ammo[0] > 0)
                                        {
                                            ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 4, state.position.Y + offsetPosition.Y - 13), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                        else
                                        {
                                            PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 2 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                        }
                                    }
                                    break;

                                case 3:
                                    if (state.character == 0)
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + offsetPosition.X - 29), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[state.character].Height),
                                            new Rectangle(0, 0, 59, ShotgunNETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + offsetPosition.X + 4 - 26), Convert.ToInt32(state.position.Y + offsetPosition.Y - 13), 53, ShotgunNETexture[state.character].Height),
                                            new Rectangle(0, 0, 53, ShotgunNETexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                    DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    // Draw Movement LEGS
                    if (state.accumMove.Length() > .5)
                    {
                        if (state.character == 0)
                        {
                            if (state.accumMove.X > 0)
                            {
                                if (state.accumFire.Length() < .5)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }

                                RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(state.position.X - 7 + offsetPosition.X, state.position.Y - 26), SpriteEffects.None, layerIndex + 0.001f, 0f, color);
                            }
                            else
                            {
                                if (state.accumFire.Length() < .5)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 16, state.position.Y + offsetPosition.Y), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                }

                                RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(state.position.X + 18 + offsetPosition.X, state.position.Y - 26), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, color);
                            }
                        }
                        else
                        {
                            if (state.accumMove.X > 0)
                            {
                                if (state.accumFire.Length() < .5)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }

                                RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(state.position.X - 4 + offsetPosition.X, state.position.Y - 24), SpriteEffects.None, layerIndex + 0.001f, 0f, color);
                            }
                            else
                            {
                                if (state.accumFire.Length() < .5)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 19, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }
                                }

                                RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                    new Vector2(state.position.X + 20 + offsetPosition.X, state.position.Y - 24), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, color);
                            }
                        }
                    }
                    else
                    {
                        if (state.accumFire.Length() < .5)
                        {
                            if (state.character == 0)
                            {
                                if (state.currentgun == 0 && state.ammo[0] == 0)
                                {
                                    IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, color);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }
                            else
                            {
                                if (state.currentgun == 0 && state.ammo[0] == 0)
                                {
                                    IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, color);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }
                        }

                        this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 3), IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height),
                        new Rectangle(0, 0, IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height), color, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                    }

#if DEBUG
                    this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, layerIndex.ToString(), state.position, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.4f);

                    // Position Reference TEMPORAL
                    this.ScreenManager.SpriteBatch.Draw(PositionReference, new Rectangle(Convert.ToInt32(state.position.X), Convert.ToInt32(state.position.Y), PositionReference.Width, PositionReference.Height),
                        new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
#endif

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(state.position.X + IdleLegsTexture[state.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(state.position.Y + IdleLegsTexture[state.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);

                    state.isLoosingLife = false;
                    break;


                case ObjectStatus.Dying:
                    if (state.accumMove.X > 0)
                    {
                        this.ScreenManager.SpriteBatch.Draw(DiedTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y - 27), DiedTexture[state.character].Width, DiedTexture[state.character].Height),
                            new Rectangle(0, 0, DiedTexture[state.character].Width, DiedTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex + 0.001f);
                    }
                    else
                    {
                        this.ScreenManager.SpriteBatch.Draw(DiedTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y - 27), DiedTexture[state.character].Width, DiedTexture[state.character].Height),
                            new Rectangle(0, 0, DiedTexture[state.character].Width, DiedTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                    }

#if DEBUG
                    // Position Reference TEMPORAL
                    this.ScreenManager.SpriteBatch.Draw(PositionReference, new Rectangle(Convert.ToInt32(state.position.X), Convert.ToInt32(state.position.Y), PositionReference.Width, PositionReference.Height),
                        new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
#endif

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(state.position.X + IdleLegsTexture[state.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(state.position.Y + IdleLegsTexture[state.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);
                    break;


                case ObjectStatus.Immune://blink 5 times per second
                    // Draw Player Name and Highlight for Blue
                    if (state.color == Color.Blue)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerBlue, new Vector2(state.position.X + IdleTrunkAnimation[state.character].frameSize.X / 2 - UIPlayerBlue.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    // Draw Player Name and Highlight for Red
                    if (state.color == Color.Red)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerRed, new Vector2(state.position.X + IdleTrunkAnimation[state.character].frameSize.X / 2 - UIPlayerRed.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    // Draw Player Name and Highlight for Green
                    if (state.color == Color.Green)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerGreen, new Vector2(state.position.X + IdleTrunkAnimation[state.character].frameSize.X / 2 - UIPlayerGreen.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    // Draw Player Name and Highlight for Yellow
                    if (state.color == Color.Yellow)
                    {
                        this.ScreenManager.SpriteBatch.Draw(UIPlayerYellow, new Vector2(state.position.X + IdleTrunkAnimation[state.character].frameSize.X / 2 - UIPlayerYellow.Width / 2 + offsetPosition.X, state.position.Y - 20 + offsetPosition.Y), Color.White);
                    }

                    if (((int)(TotalGameSeconds * 10) % 2) == 0)
                    {
                        // Draws our avatar at the current position with no tinting
                        if (state.accumFire.Length() > .5)
                        {
                            if (state.shotAngle > -0.3925f && state.shotAngle < 0.3925f) //NORTH
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 6 + offsetPosition.X, state.position.Y + offsetPosition.Y - 34), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 12 + offsetPosition.X, state.position.Y + offsetPosition.Y - 36), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNorthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 12 + offsetPosition.X, state.position.Y + offsetPosition.Y - 30), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 6 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 34), 26, ShotgunNorthTexture[state.character].Height),
                                                new Rectangle(0, 0, 26, ShotgunNorthTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNorthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 12 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 36), 21, ShotgunNorthTexture[state.character].Height),
                                                new Rectangle(0, 0, 21, ShotgunNorthTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle > 0.3925f && state.shotAngle < 1.1775f) //NORTH-EAST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;
                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[state.character].Height),
                                                new Rectangle(0, 0, 59, ShotgunNETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 53, ShotgunNETexture[state.character].Height),
                                                new Rectangle(0, 0, 53, ShotgunNETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle > 1.1775f && state.shotAngle < 1.9625f) //EAST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle > 1.19625f && state.shotAngle < 2.7275f) //SOUTH-EAST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[state.character].Height),
                                                new Rectangle(0, 0, 58, ShotgunSETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 54, ShotgunSETexture[state.character].Height),
                                                new Rectangle(0, 0, 54, ShotgunSETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle > 2.7275f || state.shotAngle < -2.7275f) //SOUTH
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 10 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSouthAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 3 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 21, ShotgunSouthTexture[state.character].Height),
                                                new Rectangle(0, 0, 21, ShotgunSouthTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSouthTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 20, ShotgunSouthTexture[state.character].Height),
                                                new Rectangle(0, 0, 20, ShotgunSouthTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle < -1.9625f && state.shotAngle > -2.7275f) //SOUTH-WEST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 1 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 4 + offsetPosition.X, state.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotSEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 7 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 1 + offsetPosition.X - 29), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 58, ShotgunSETexture[state.character].Height),
                                                new Rectangle(0, 0, 58, ShotgunSETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunSETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 4 + offsetPosition.X - 27), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 54, ShotgunSETexture[state.character].Height),
                                                new Rectangle(0, 0, 54, ShotgunSETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle < -1.1775f && state.shotAngle > -1.9625f) //WEST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y + 4), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 4 + offsetPosition.X, state.position.Y + offsetPosition.Y + 2), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 2 + offsetPosition.X, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else if (state.shotAngle < -0.3925f && state.shotAngle > -1.1775f) //NORTH-WEST
                            {
                                switch (state.currentgun)
                                {
                                    case 0:
                                        if (state.character == 0)
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y - 14), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X - 6 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        else
                                        {
                                            if (state.ammo[0] > 0)
                                            {
                                                ShotgunNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 4, state.position.Y + offsetPosition.Y - 13), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                            else
                                            {
                                                PistolShotNEAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + 2 + offsetPosition.X, state.position.Y + offsetPosition.Y - 18), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (state.character == 0)
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + offsetPosition.X - 29), Convert.ToInt32(state.position.Y + offsetPosition.Y - 14), 59, ShotgunNETexture[state.character].Height),
                                                new Rectangle(0, 0, 59, ShotgunNETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunNETexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + offsetPosition.X + 4 - 26), Convert.ToInt32(state.position.Y + offsetPosition.Y - 13), 53, ShotgunNETexture[state.character].Height),
                                                new Rectangle(0, 0, 53, ShotgunNETexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                        DrawFlameThrower(state, game.totalGameSeconds, layerIndex);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }

                        // Draw Movement LEGS
                        if (state.accumMove.Length() > .5)
                        {
                            if (state.character == 0)
                            {
                                if (state.accumMove.X > 0)
                                {
                                    if (state.accumFire.Length() < .5)
                                    {
                                        if (state.currentgun == 0 && state.ammo[0] == 0)
                                        {
                                            IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X - 7 + offsetPosition.X, state.position.Y - 26), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (state.accumFire.Length() < .5)
                                    {
                                        if (state.currentgun == 0 && state.ammo[0] == 0)
                                        {
                                            IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 16, state.position.Y + offsetPosition.Y), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                    new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X + 18 + offsetPosition.X, state.position.Y - 26), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                            else
                            {
                                if (state.accumMove.X > 0)
                                {
                                    if (state.accumFire.Length() < .5)
                                    {
                                        if (state.currentgun == 0 && state.ammo[0] == 0)
                                        {
                                            IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X - 4 + offsetPosition.X, state.position.Y - 24), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (state.accumFire.Length() < .5)
                                    {
                                        if (state.currentgun == 0 && state.ammo[0] == 0)
                                        {
                                            IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 19, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                        }
                                        else
                                        {
                                            this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                        }
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X + 20 + offsetPosition.X, state.position.Y - 24), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                        }
                        else
                        {
                            if (state.accumFire.Length() < .5)
                            {
                                if (state.character == 0)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }
                                else
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }
                                }
                            }

                            this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 3), IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height),
                            new Rectangle(0, 0, IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                        }

/*
                        if (state.accumMove.Length() > .5)
                        {
                            if (state.character == 0)
                            {
                                if (state.accumMove.X > 0)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X - 7 + offsetPosition.X, state.position.Y - 26), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 16, state.position.Y + offsetPosition.Y), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 6 + offsetPosition.X - 35), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                                new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X + 18 + offsetPosition.X, state.position.Y - 26), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                            else
                            {
                                if (state.accumMove.X > 0)
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X - 4 + offsetPosition.X, state.position.Y - 24), SpriteEffects.None, layerIndex + 0.001f, 0f, Color.White);
                                }
                                else
                                {
                                    if (state.currentgun == 0 && state.ammo[0] == 0)
                                    {
                                        IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 19, state.position.Y + offsetPosition.Y + 1), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                                    }
                                    else
                                    {
                                        this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X - 4 + offsetPosition.X - 34), Convert.ToInt32(state.position.Y + offsetPosition.Y + 2), 69, ShotgunEastTexture[state.character].Height),
                                            new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, layerIndex);
                                    }

                                    RunEastAnimation[state.character].Draw(this.ScreenManager.SpriteBatch,
                                        new Vector2(state.position.X + 20 + offsetPosition.X, state.position.Y - 24), SpriteEffects.FlipHorizontally, layerIndex + 0.001f, 0f, Color.White);
                                }
                            }
                        }
                        else
                        {
                            if (state.character == 0)
                            {
                                if (state.currentgun == 0 && state.ammo[0] == 0)
                                {
                                    IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X, state.position.Y + offsetPosition.Y), SpriteEffects.None, layerIndex, 0f, Color.White);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 4), 71, ShotgunEastTexture[state.character].Height),
                                        new Rectangle(0, 0, 71, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }
                            else
                            {
                                if (state.currentgun == 0 && state.ammo[0] == 0)
                                {
                                    IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, new Vector2(state.position.X + offsetPosition.X + 10, state.position.Y + offsetPosition.Y + 1), SpriteEffects.None, layerIndex, 0f, Color.White);
                                }
                                else
                                {
                                    this.ScreenManager.SpriteBatch.Draw(ShotgunEastTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 10 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 1), 69, ShotgunEastTexture[state.character].Height),
                                        new Rectangle(0, 0, 69, ShotgunEastTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex);
                                }
                            }

                            this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[state.character], new Rectangle(Convert.ToInt32(state.position.X + 7 + offsetPosition.X), Convert.ToInt32(state.position.Y + offsetPosition.Y + 3), IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height),
                            new Rectangle(0, 0, IdleLegsTexture[state.character].Width, IdleLegsTexture[state.character].Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.001f);
                        }
 */

                        //IdleTrunkAnimation[state.character].Draw(this.ScreenManager.SpriteBatch, state.position, SpriteEffects.None, layerIndex, 0f);
                        //this.ScreenManager.SpriteBatch.Draw(IdleLegsTexture[state.character], new Vector2(state.position.X + 7, state.position.Y + 3), Color.White);
                        //batch.Draw(character, state.position, null, Color.Silver, 0, characterorigin, 1.0f, SpriteEffects.None, 0.5f);
                    }

                    // Draw the SHADOW OF THE CHARACTER
                    this.ScreenManager.SpriteBatch.Draw(CharacterShadow, new Rectangle(Convert.ToInt32(state.position.X + IdleLegsTexture[state.character].Width / 2 - 5 + offsetPosition.X), Convert.ToInt32(state.position.Y + IdleLegsTexture[state.character].Height - 6 + offsetPosition.Y), CharacterShadow.Width, CharacterShadow.Height),
                        new Rectangle(0, 0, CharacterShadow.Width, CharacterShadow.Height), new Color(255, 255, 255, 50), 0.0f, Vector2.Zero, SpriteEffects.None, layerIndex + 0.01f);
                    break;
                default:
                    break;
            }

            //batch.End();
        }


        private void DrawFlameThrower(Avatar player, double TotalGameSeconds, float layerIndex)
        {
            if (player.currentgun == 3 && player.ammo[3] > 0)
            {
                flamethrowerAnimation.Draw(this.ScreenManager.SpriteBatch, player.FlameThrowerPosition, SpriteEffects.None, layerIndex, player.FlameThrowerAngle, Color.White);


#if DEBUG
                //Rectangle aPositionAdjusted = new Rectangle(player.FlameThrowerRectangle.X, player.FlameThrowerRectangle.Y, player.FlameThrowerRectangle.Width, player.FlameThrowerRectangle.Height);
                //this.ScreenManager.SpriteBatch.Draw(FT_CollisionTexture, aPositionAdjusted, new Rectangle(0, 0, 88, 43), Color.BlueViolet, player.FlameThrowerRectangle.Rotation, Vector2.Zero, SpriteEffects.None, 0.1f);
#endif
            }
            //this.ScreenManager.SpriteBatch.End();
        }


        private void DrawShotgunShots(List<ShotgunShell> shotgunbullets, double TotalGameSeconds)
        {
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin();
            Vector2 pos;
            for (int i = 0; i < shotgunbullets.Count; i++)
            {
                for (int j = 0; j < shotgunbullets[i].Pellet.Count; j++)
                {
                    pos = FindShotgunBulletPosition(shotgunbullets[i].Pellet[j], shotgunbullets[i].Angle, j, TotalGameSeconds);

                    if (pos.X < -10 || pos.X > 1290 || pos.Y < -10 || pos.Y > 730)
                    {
                        // Since we are iterating each bullet anyway, now is a good time
                        // to remove bullets that go offscreen.
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
            batch.Begin();
            Vector2 pos;
            for (int i = 0; i < bullets.Count; i++)
            {
                pos = FindBulletPosition(bullets[i], TotalGameSeconds);

                if (pos.X < -10 || pos.X > 1290 || pos.Y < -10 || pos.Y > 730)
                {
                    // Since we are iterating each bullet anyway, now is a good time
                    // to remove bullets that go offscreen.
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
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (Avatar cplayer in game.currentPlayers)
            {
                if (cplayer.Player != null)
                {
                    if (cplayer.status == ObjectStatus.Active || cplayer.IsPlaying)
                    {
                        if (cplayer.lives != 0)
                        {
                            // Draw UI Player Background
                            batch.Draw(UIStats, new Vector2(Pos.X, Pos.Y - 20), Color.White);

                            // Draw UI Player Color Background
                            if (cplayer.color == Color.Blue)
                            {
                                batch.Draw(UIStatsBlue, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (cplayer.color == Color.Red)
                            {
                                batch.Draw(UIStatsRed, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (cplayer.color == Color.Green)
                            {
                                batch.Draw(UIStatsGreen, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            if (cplayer.color == Color.Yellow)
                            {
                                batch.Draw(UIStatsYellow, new Vector2(Pos.X, Pos.Y - 20), Color.White);
                            }

                            // Draw Character Avatar
                            switch (cplayer.character)
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

                            // Draw Player Lives
                            batch.DrawString(arcade14, "x", new Vector2(Pos.X + 40, Pos.Y + 17), Color.White);
                            batch.DrawString(arcade28, cplayer.lives.ToString(), new Vector2(Pos.X + 60, Pos.Y), Color.White);

                            // Draw Player Life Counter
                            batch.Draw(heart, new Vector2(Pos.X + 120, Pos.Y + 3), Color.White);
                            batch.DrawString(arcade14, cplayer.lifecounter.ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y), Color.White);

                            // Draw Player Ammo counter
                            switch (cplayer.currentgun)
                            {
                                case 0:
                                    batch.Draw(pistolammoUI, new Vector2(Pos.X + 124, Pos.Y + 23), Color.White);
                                    batch.DrawString(arcade14, cplayer.ammo[0].ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case 1:
                                    batch.Draw(shotgunammoUI, new Vector2(Pos.X + 123, Pos.Y + 22), Color.White);
                                    batch.DrawString(arcade14, cplayer.ammo[1].ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case 2:
                                    batch.Draw(grenadeammoUI, new Vector2(Pos.X + 122, Pos.Y + 21), Color.White);
                                    batch.DrawString(arcade14, cplayer.ammo[2].ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                case 3:
                                    batch.Draw(flamethrowerammoUI, new Vector2(Pos.X + 124, Pos.Y + 23), Color.White);
                                    batch.DrawString(arcade14, cplayer.ammo[3].ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                                default:
                                    batch.Draw(pistolammoUI, new Vector2(Pos.X + 124, Pos.Y + 23), Color.White);
                                    batch.DrawString(arcade14, cplayer.ammo[0].ToString("000"), new Vector2(Pos.X + heart.Width + 125, Pos.Y + 20), Color.White);
                                    break;
                            }


                            // Draw Player Score
                            //batch.DrawString(fixedfont, "SCORE", new Vector2(Pos.X, Pos.Y + 48), Color.White);
                            batch.DrawString(arcade14, "SC" + cplayer.score.ToString("0000000"), new Vector2(Pos.X + 13, Pos.Y + 48), Color.White);

                            /*
                            // Draw Player Gamer Picture
                            if (cplayer.Player.SignedInGamer != null)
                            {
                                if (cplayer.Player.SignedInGamer.IsSignedInToLive)
                                {
                                    //batch.Draw(cplayer.Player.gamerPicture, new Vector2(Pos.X + 43, Pos.Y + 24), Color.White);
                                    batch.Draw(cplayer.Player.gamerPicture, new Rectangle(Convert.ToInt32(Pos.X + 10), Convert.ToInt32(Pos.Y - 15), 40, 40),
                                        new Rectangle(0, 0, cplayer.Player.gamerPicture.Width, cplayer.Player.gamerPicture.Height), Color.White,
                                        0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                                }
                            }
                            

                            // Gamer Picture Border
                            batch.Draw(gamerPictureBorder, new Rectangle(Convert.ToInt32(Pos.X + 8), Convert.ToInt32(Pos.Y - 17),
                                gamerPictureBorder.Width, gamerPictureBorder.Height), Color.White);
                            */

                            

                            Pos.X += UIStats.Width + 50;
                        }
                        else
                        {
                            // Game Over String
                            batch.DrawString(MenuInfoFont, Strings.GameOverString, new Vector2(Pos.X, Pos.Y), Color.White);
                            Pos.X += (int)(MenuInfoFont.MeasureString(Strings.GameOverString).X) + 100;
                        }
                    }
                }
            }

            // Draw Level / Sublevel Context
            // LEVEL
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

            // WAVE
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


            if (game.player1.Options == InputMode.Touch)
            {
                // Pause Icon
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


        public void ZombieMoved(Enemies enemies, byte zombie, Vector2 pos, float angle)
        {
            enemies.zombies[zombie].entity.Position = pos;
            enemies.zombies[zombie].angle = angle;
        }

        public void TankMoved(Enemies enemies, byte tank, Vector2 pos, float angle)
        {
            enemies.tanks[tank].position = pos;
            enemies.tanks[tank].angle = angle;
        }

        public void GameOver(byte player)
        {
            PlayerDestroyed(player);
            game.playScreen.bGameOver = true;
        }

        public void TankCrashed(TankState tank)
        {
            tank.CrashTank(game.totalGameSeconds);
            ActiveTanks--;
        }

        public void ZombieCrashed(ZombieState zombie)
        {
            zombie.CrashZombie(game.totalGameSeconds);
            ActiveZombies--;
        }

        public void IncreaseLife(byte player)
        {
            if (game.currentPlayers[player].lives < 9)
            {
                game.currentPlayers[player].lives++;
            }
        }

        public void ZombieDestroyed(ZombieState zombie, byte player, byte currentgun)
        {
            zombie.DestroyZombie(game.totalGameSeconds, currentgun);
            ActiveZombies--;
            game.currentPlayers[player].score += 10;
            game.audio.PlayZombieDying();

            if (game.currentPlayers[player].score % 8000 == 0)
            {
                game.currentPlayers[player].lives += 1;
            }
        }

        public void TankDestroyed(TankState tank, byte player)
        {
            tank.DestroyTank(game.totalGameSeconds);
            ActiveTanks--;
            game.currentPlayers[player].score += 10;
            game.audio.PlayZombieDying();

            if (game.currentPlayers[player].score % 8000 == 0)
            {
                game.currentPlayers[player].lives += 1;
            }
        }

        public void PlayerDestroyed(byte player)
        {
            game.currentPlayers[player].DestroyShip(game.totalGameSeconds);
            game.currentPlayers[player].lives--;
            if (game.currentPlayers[player].character == 0)
            {
                game.audio.PlayWomanScream();
            }
            else
            {
                game.audio.PlayManScream();
            }
        }

        public void IncreaseScore(byte player, short amount)
        {
            game.currentPlayers[player].score += amount;
        }

        public void SpawnNextWave(Enemies enemies, float totalGameSeconds)
        {
            //enemies.SpawnNextWave(totalGameSeconds);
        }

        public void PlayerMove(byte player, Vector2 pos)
        {
            game.currentPlayers[player].position = pos;
            game.currentPlayers[player].entity.Position = pos;
        }

        public void PlayerFire(byte player, float totalGameSeconds, float angle, Vector2 direction)
        {
            float angle2, angle3;
            angle2 = angle - 0.15f;
            angle3 = angle + 0.15f;

            //NORTH
            if (angle > -0.3925f && angle < 0.3925f)
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 27, game.currentPlayers[player].position.Y - 55), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 25, game.currentPlayers[player].position.Y - 61), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 4, game.currentPlayers[player].position.Y - 55, totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 6, game.currentPlayers[player].position.Y - 61, totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 5, game.currentPlayers[player].position.Y - 57, totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 8, game.currentPlayers[player].position.Y - 63, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle > 0.3925f && angle < 1.1775f) //NORTH-EAST
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 3, game.currentPlayers[player].position.Y - 67), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X, game.currentPlayers[player].position.Y - 70), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 33, game.currentPlayers[player].position.Y - 60, totalGameSeconds, angle));
                    }
                    else
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 27, game.currentPlayers[player].position.Y - 60, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle > 1.1775f && angle < 1.9625f) //EAST
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 30, game.currentPlayers[player].position.Y - 58), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 30, game.currentPlayers[player].position.Y - 60), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(new Vector2(game.currentPlayers[player].position.X + 35, game.currentPlayers[player].position.Y - 27), totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(new Vector2(game.currentPlayers[player].position.X + 37, game.currentPlayers[player].position.Y - 29), totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(new Vector2(game.currentPlayers[player].position.X + 35, game.currentPlayers[player].position.Y - 34), totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(new Vector2(game.currentPlayers[player].position.X + 36, game.currentPlayers[player].position.Y - 38), totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle > 1.19625f && angle < 2.7275f) //SOUTH-EAST 
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 45, game.currentPlayers[player].position.Y - 27), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 47, game.currentPlayers[player].position.Y - 28), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 27, game.currentPlayers[player].position.Y, totalGameSeconds, angle));
                    }
                    else
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 32, game.currentPlayers[player].position.Y - 5, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle > 2.7275f || angle < -2.7275f) //SOUTH
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 35, game.currentPlayers[player].position.Y + 2), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X + 37, game.currentPlayers[player].position.Y + 2), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 1, game.currentPlayers[player].position.Y + 5, totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 4, game.currentPlayers[player].position.Y + 7, totalGameSeconds, angle));
                        }
                    }
                    else
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X + 5, game.currentPlayers[player].position.Y + 5, totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 7, game.currentPlayers[player].position.Y + 5, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle < -1.9625f && angle > -2.7275f) //SOUTH-WEST
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 3, game.currentPlayers[player].position.Y + 19), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X, game.currentPlayers[player].position.Y + 19), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 28, game.currentPlayers[player].position.Y, totalGameSeconds, angle));
                    }
                    else
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 35, game.currentPlayers[player].position.Y - 5, totalGameSeconds, angle));
                    }
                }
            }
            else if (angle < -1.1775f && angle > -1.9625f) //WEST
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 30, game.currentPlayers[player].position.Y + 6), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 30, game.currentPlayers[player].position.Y + 6), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(new Vector2(game.currentPlayers[player].position.X - 35, game.currentPlayers[player].position.Y - 26), totalGameSeconds, angle));
                    }
                    else
                    {
                        if (game.currentPlayers[player].character == 0)
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 37, game.currentPlayers[player].position.Y - 34, totalGameSeconds, angle));
                        }
                        else
                        {
                            game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 36, game.currentPlayers[player].position.Y - 38, totalGameSeconds, angle));
                        }
                    }
                }
            }
            else if (angle < -0.3925f && angle > -1.1775f) //NORTH-WEST
            {
                if (game.currentPlayers[player].currentgun == 3)
                {
                    if (game.currentPlayers[player].character == 0)
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 46, game.currentPlayers[player].position.Y - 23), angle);
                    }
                    else
                    {
                        game.currentPlayers[player].setFlameThrower(new Vector2(game.currentPlayers[player].position.X - 44, game.currentPlayers[player].position.Y - 22), angle);
                    }
                }
                else if (game.currentPlayers[player].currentgun == 1)
                {
                    game.currentPlayers[player].shotgunbullets.Add(new ShotgunShell(game.currentPlayers[player].position, direction, angle, totalGameSeconds));
                }
                else
                {
                    if (game.currentPlayers[player].ammo[0] > 0)
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 36, game.currentPlayers[player].position.Y - 57, totalGameSeconds, angle));
                    }
                    else
                    {
                        game.currentPlayers[player].bullets.Add(new Vector4(game.currentPlayers[player].position.X - 32, game.currentPlayers[player].position.Y - 60, totalGameSeconds, angle));
                    }
                }
            }

            if (game.currentPlayers[player].ammo[game.currentPlayers[player].currentgun] > 0)
            {
                game.currentPlayers[player].ammo[game.currentPlayers[player].currentgun] -= 1;
            }

            switch (game.currentPlayers[player].currentgun)
            {
                case 0:
                    if (game.currentPlayers[player].ammo[game.currentPlayers[player].currentgun] > 0)
                    {
                        game.audio.PlayMachineGun();
                    }
                    else
                    {
                        game.audio.PlayShot();
                    }
                    break;
                case 3:
                    game.audio.PlayFlameThrower();
                    break;
                default:
                    game.audio.PlayShot();
                    break;
            }

            game.input.PlayShot(player);
        }

        private void JadeIdleTrunkAnimationInit(XDocument doc)
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
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            IdleTrunkAnimation = new List<Animation>();
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[0], frameSize, sheetSize, frameInterval)); // Define a new Animation instance
        }

        private void JadeRunEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeRunEDef");
            RunEastTexture = new List<Texture2D>();
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_run_E"));
            RunEastOrigin = new List<Vector2>();
            RunEastOrigin.Add(new Vector2(RunEastTexture[0].Width / 2, RunEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            RunEastAnimation = new List<Animation>();
            RunEastAnimation.Add(new Animation(RunEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotDef");
            PistolShotEastTexture = new List<Texture2D>();
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_E"));
            PistolShotEastOrigin = new List<Vector2>();
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[0].Width / 2, PistolShotEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotEastAnimation = new List<Animation>();
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotNEDef");
            PistolShotNETexture = new List<Texture2D>();
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_NE"));
            PistolShotNEOrigin = new List<Vector2>();
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[0].Width / 2, PistolShotNETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNEAnimation = new List<Animation>();
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotSEDef");
            PistolShotSETexture = new List<Texture2D>();
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_SE"));
            PistolShotSEOrigin = new List<Vector2>();
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[0].Width / 2, PistolShotSETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSEAnimation = new List<Animation>();
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotSouthDef");
            PistolShotSouthTexture = new List<Texture2D>();
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_S"));
            PistolShotSouthOrigin = new List<Vector2>();
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[0].Width / 2, PistolShotSouthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSouthAnimation = new List<Animation>();
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadePistolShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadePistolShotNorthDef");
            PistolShotNorthTexture = new List<Texture2D>();
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shot_N"));
            PistolShotNorthOrigin = new List<Vector2>();
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[0].Width / 2, PistolShotNorthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNorthAnimation = new List<Animation>();
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotDef");
            ShotgunEastTexture = new List<Texture2D>();
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun"));
            ShotgunShotEastOrigin = new List<Vector2>();
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[0].Width / 2, PistolShotEastTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunShotEastAnimation = new List<Animation>();
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotNEDef");
            ShotgunNETexture = new List<Texture2D>();
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_NE"));
            ShotgunNEOrigin = new List<Vector2>();
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[0].Width / 2, ShotgunNETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNEAnimation = new List<Animation>();
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotSEDef");
            ShotgunSETexture = new List<Texture2D>();
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_SE"));
            ShotgunSEOrigin = new List<Vector2>();
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[0].Width / 2, ShotgunSETexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSEAnimation = new List<Animation>();
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotSouthDef");
            ShotgunSouthTexture = new List<Texture2D>();
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_S"));
            ShotgunSouthOrigin = new List<Vector2>();
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[0].Width / 2, ShotgunSouthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSouthAnimation = new List<Animation>();
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void JadeShotgunShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("JadeShotgunShotNorthDef");
            ShotgunNorthTexture = new List<Texture2D>();
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Jade/girl_anim_shotgun_N"));
            ShotgunNorthOrigin = new List<Vector2>();
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[0].Width / 2, ShotgunNorthTexture[0].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNorthAnimation = new List<Animation>();
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[0], frameSize, sheetSize, frameInterval));
        }

        private void EgonIdleTrunkAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[1].Width / 2, IdleTrunkTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonRunEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[1].Width / 2, RunEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[1].Width / 2, PistolShotEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[1].Width / 2, PistolShotNETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[1].Width / 2, PistolShotSETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[1].Width / 2, PistolShotSouthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonPistolShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[1].Width / 2, PistolShotNorthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[1].Width / 2, PistolShotEastTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[1].Width / 2, ShotgunNETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[1].Width / 2, ShotgunSETexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[1].Width / 2, ShotgunSouthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[1], frameSize, sheetSize, frameInterval));

        }
        private void EgonShotgunShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("EgonShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Egon/egon_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[1].Width / 2, ShotgunNorthTexture[1].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[1], frameSize, sheetSize, frameInterval));

        }

        private void RayIdleTrunkAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[2].Width / 2, IdleTrunkTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayRunEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[2].Width / 2, RunEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[2].Width / 2, PistolShotEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[2].Width / 2, PistolShotNETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[2].Width / 2, PistolShotSETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[2].Width / 2, PistolShotSouthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayPistolShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[2].Width / 2, PistolShotNorthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[2].Width / 2, PistolShotEastTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[2].Width / 2, ShotgunNETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[2].Width / 2, ShotgunSETexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[2], frameSize, sheetSize, frameInterval));

        }
        private void RayShotgunShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[2].Width / 2, ShotgunSouthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[2], frameSize, sheetSize, frameInterval));
        }
        private void RayShotgunShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("RayShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Ray/ray_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[2].Width / 2, ShotgunNorthTexture[2].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[2], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }

        private void PeterIdleTrunkAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterIdleTrunkDef");
            IdleLegsTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_legs_idle"));
            IdleTrunkTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_idle"));
            IdleTrunkOrigin.Add(new Vector2(IdleTrunkTexture[3].Width / 2, IdleTrunkTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            IdleTrunkAnimation.Add(new Animation(IdleTrunkTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterRunEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterRunEDef");
            RunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_run_E"));
            RunEastOrigin.Add(new Vector2(RunEastTexture[3].Width / 2, RunEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            RunEastAnimation.Add(new Animation(RunEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolEDef");
            PistolShotEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_E"));
            PistolShotEastOrigin.Add(new Vector2(PistolShotEastTexture[3].Width / 2, PistolShotEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotEastAnimation.Add(new Animation(PistolShotEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolNEDef");
            PistolShotNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_NE"));
            PistolShotNEOrigin.Add(new Vector2(PistolShotNETexture[3].Width / 2, PistolShotNETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNEAnimation.Add(new Animation(PistolShotNETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolSEDef");
            PistolShotSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_SE"));
            PistolShotSEOrigin.Add(new Vector2(PistolShotSETexture[3].Width / 2, PistolShotSETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSEAnimation.Add(new Animation(PistolShotSETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolSouthDef");
            PistolShotSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_S"));
            PistolShotSouthOrigin.Add(new Vector2(PistolShotSouthTexture[3].Width / 2, PistolShotSouthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotSouthAnimation.Add(new Animation(PistolShotSouthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterPistolShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterPistolNorthDef");
            PistolShotNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_pistol_N"));
            PistolShotNorthOrigin.Add(new Vector2(PistolShotNorthTexture[3].Width / 2, PistolShotNorthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            PistolShotNorthAnimation.Add(new Animation(PistolShotNorthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunEDef");
            ShotgunEastTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_E"));
            ShotgunShotEastOrigin.Add(new Vector2(ShotgunEastTexture[3].Width / 2, PistolShotEastTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunShotEastAnimation.Add(new Animation(ShotgunEastTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotNorthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunNEDef");
            ShotgunNETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_NE"));
            ShotgunNEOrigin.Add(new Vector2(ShotgunNETexture[3].Width / 2, ShotgunNETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNEAnimation.Add(new Animation(ShotgunNETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotSouthEastAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunSEDef");
            ShotgunSETexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_SE"));
            ShotgunSEOrigin.Add(new Vector2(ShotgunSETexture[3].Width / 2, ShotgunSETexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSEAnimation.Add(new Animation(ShotgunSETexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotSouthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunSouthDef");
            ShotgunSouthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_S"));
            ShotgunSouthOrigin.Add(new Vector2(ShotgunSouthTexture[3].Width / 2, ShotgunSouthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunSouthAnimation.Add(new Animation(ShotgunSouthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }
        private void PeterShotgunShotNorthAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("PeterShotgunNorthDef");
            ShotgunNorthTexture.Add(game.Content.Load<Texture2D>(@"InGame/Peter/peter_shotgun_N"));
            ShotgunNorthOrigin.Add(new Vector2(ShotgunNorthTexture[3].Width / 2, ShotgunNorthTexture[3].Height / 2));
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ShotgunNorthAnimation.Add(new Animation(ShotgunNorthTexture[3], frameSize, sheetSize, frameInterval)); // Define a new Animation instance

        }

        private void FlamethrowerAnimationInit(XDocument doc)
        {
            TimeSpan frameInterval;
            Point sheetSize = new Point();
            Point frameSize = new Point();
            var definition = doc.Root.Element("FlameThrowerDef");
            flamethrowerTexture = game.Content.Load<Texture2D>(@"InGame/flamethrower");
            FT_CollisionTexture = game.Content.Load<Texture2D>(@"InGame/FT_CollisionTexture");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            flamethrowerAnimation = new Animation(flamethrowerTexture, frameSize, sheetSize, frameInterval);

        }
    }
}
