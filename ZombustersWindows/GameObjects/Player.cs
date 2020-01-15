using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;

namespace ZombustersWindows
{
    public class Player
    {
        private static string OPTIONS_FILENAME = "options.xml";
        private static string LEADERBOARD_FILENAME = "leaderboard.txt";
        private static string SAVE_GAME_FILENAME = "savegame.sav";

        public bool IsPlaying;
        public PlayerIndex Controller;
        public bool IsRemote;
        public InputMode Options;
        public OptionsState optionsState;
        public AudioManager audioManager;
        public bool isReady;
        public int characterSelected;
        public int levelsUnlocked;
        private MyGame game;
        public Texture2D gamerPicture;
        private string name;

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

        public Player(OptionsState options, AudioManager audio, MyGame game)
        {
            this.optionsState = options;
            this.audioManager = audio;
            this.game = game;
        }

        public void InitLocal(PlayerIndex controller, string name, InputMode options, MyGame game)
        {
            this.game = game;
            this.Controller = controller;
            this.Name = name;
            this.Options = options;
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
                PlayerName = game.currentPlayers[0].Player.Name,
                Level = currentLevelNumber
            };
            if (game.currentPlayers[0].Player.levelsUnlocked >= currentLevelNumber)
                return;
            game.storageDataSource.SaveXMLFile(SAVE_GAME_FILENAME, data);
        }

        public void LoadSavedGame()
        {
            SaveGameData data = new SaveGameData();
            data = (SaveGameData)game.storageDataSource.LoadXMLFile(SAVE_GAME_FILENAME, data);
            game.currentPlayers[0].Player.levelsUnlocked = (byte)data.Level;
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
                game.graphics.ToggleFullScreen();
            }
        }

        public void LoadLeaderBoard()
        {
            LoadDefaultLeaderBoard();
            BinaryReader binaryReader = game.storageDataSource.GetBinaryReader(LEADERBOARD_FILENAME);
            if (binaryReader != null)
            {
                game.topScoreListContainer = new TopScoreListContainer(binaryReader);
            }
        }

        public void SaveLeaderBoard(int score)
        {
            BinaryWriter binaryWriter = game.storageDataSource.GetBinaryWriter(LEADERBOARD_FILENAME);
            if (binaryWriter != null)
            {
                TopScoreEntry entry = new TopScoreEntry(name, score);
                game.topScoreListContainer.addEntry(0, entry);
                game.topScoreListContainer.save(binaryWriter);
            }
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
