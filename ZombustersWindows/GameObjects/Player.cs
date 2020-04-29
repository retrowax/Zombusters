using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class Player
    {
        private static readonly string OPTIONS_FILENAME = "options.xml";
        private static readonly string LEADERBOARD_FILENAME = "leaderboard.txt";
        private static readonly string SAVE_GAME_FILENAME = "savegame.sav";

        public bool IsPlaying;
        public PlayerIndex Controller;
        public bool IsRemote;
        public InputMode inputMode = InputMode.NotExistent;
        public OptionsState optionsState;
        public AudioManager audioManager;
        public bool isReady;
        public int characterSelected;
        public int levelsUnlocked;
        private MyGame game;
        public Texture2D gamerPicture;
        private string name;
        public Avatar avatar;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public struct SaveGameData
        {
            public string PlayerName;
            public int Level;
        }

        public Player(OptionsState options, AudioManager audio, MyGame game, Color color, string name)
        {
            this.optionsState = options;
            this.audioManager = audio;
            this.game = game;
            this.avatar = new Avatar(color);
            this.avatar.Initialize(game.GraphicsDevice.Viewport);
            this.inputMode = InputMode.NotExistent;
            this.name = name;
        }

        #region Managing Gamer Presence
        //GamerPresenceMode previousMode;
        public void BeginPause()
        {
            //if (SignedInGamer != null)
            //{
            //    previousMode = this.SignedInGamer.Presence.PresenceMode;
            //    this.SignedInGamer.Presence.PresenceMode = GamerPresenceMode.Paused;
            //}
        }
        public void EndPause()
        {
            //if (SignedInGamer != null)
            //    this.SignedInGamer.Presence.PresenceMode = previousMode;
        }
        
        #endregion


        public void SaveGame(int currentLevelNumber)
        {
            SaveGameData data = new SaveGameData
            {
                PlayerName = game.players[0].Name,
                Level = currentLevelNumber
            };
            if (game.players[0].levelsUnlocked >= currentLevelNumber)
                return;
            game.storageDataSource.SaveXMLFile(SAVE_GAME_FILENAME, data);
        }

        public void LoadSavedGame()
        {
            SaveGameData data = new SaveGameData();
            data = (SaveGameData)game.storageDataSource.LoadXMLFile(SAVE_GAME_FILENAME, data);
            game.players[0].levelsUnlocked = (byte)data.Level;
        }

        public void SaveOptions()
        {
            game.storageDataSource.SaveXMLFile(OPTIONS_FILENAME, optionsState);
        }

        public void LoadOptions()
        {
            optionsState = new OptionsState
            {
                FXLevel = 0.7f,
                MusicLevel = 0.6f,
                Player = InputMode.Keyboard, 
                FullScreenMode = false
            };
            optionsState = (OptionsState)game.storageDataSource.LoadXMLFile(OPTIONS_FILENAME, optionsState);

            audioManager.SetOptions(optionsState.FXLevel, optionsState.MusicLevel);
            if (optionsState.FullScreenMode && game.graphics.IsFullScreen == false)
            {
                int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Resolution.SetResolution(screenWidth, screenHeight, true);
                optionsState.FullScreenMode = true;
                game.graphics.IsFullScreen = false;
                game.graphics.ToggleFullScreen();
            }
        }

        public void LoadLeaderBoard()
        {
            LoadDefaultLeaderBoard();
            TopScoreListContainer topScoreListContainer = game.storageDataSource.LoadScoreList(LEADERBOARD_FILENAME);
            if (topScoreListContainer != null)
            {
                game.topScoreListContainer = topScoreListContainer;
            }
        }

        public void SaveLeaderBoard(int score)
        {
            TopScoreEntry entry = new TopScoreEntry(name, score);
            game.topScoreListContainer.addEntry(0, entry);
            game.storageDataSource.SaveScoreListToFile(LEADERBOARD_FILENAME, game.topScoreListContainer.scoreList);
        }

        private void LoadDefaultLeaderBoard()
        {
            // Create a List and a fake list
            if (game != null)
            {
                game.topScoreListContainer = new TopScoreListContainer(1, 10);
                string gtag;
                int fakescore;
                int i;

                for (i = 0; i < 10; i++)
                {
                    switch (i)
                    {
                        case 0:
                            gtag = "Gordon Freemon";
                            fakescore = 100000;
                            break;
                        case 1:
                            gtag = "Markus Fonix";
                            fakescore = 50000;
                            break;
                        case 2:
                            gtag = "John Chepard";
                            fakescore = 25000;
                            break;
                        case 3:
                            gtag = "Nikko Belich";
                            fakescore = 12500;
                            break;
                        case 4:
                            gtag = "Frank Western";
                            fakescore = 7500;
                            break;
                        case 5:
                            gtag = "Isaac Clarc";
                            fakescore = 5000;
                            break;
                        case 6:
                            gtag = "Alan Woke";
                            fakescore = 2500;
                            break;
                        case 7:
                            gtag = "Chris Redfold";
                            fakescore = 1000;
                            break;
                        case 8:
                            gtag = "Master Chef";
                            fakescore = 500;
                            break;
                        case 9:
                            gtag = "Lora Craft";
                            fakescore = 250;
                            break;
                        default:
                            gtag = "Solid Smoke";
                            fakescore = 100;
                            break;
                    }

                    TopScoreEntry entry = new TopScoreEntry(gtag, fakescore);
                    game.topScoreListContainer.addEntry(0, entry);
                }
            }
        }
    }
}
