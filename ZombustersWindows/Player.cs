#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//-----------------------------------------------------------------------------
#endregion

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

        public Player(OptionsState options, AudioManager audio)
        {
            this.optionsState = options;
            this.audioManager = audio;
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
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(SAVE_GAME_FILENAME, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    SaveGameData data = new SaveGameData();
                    data.PlayerName = game.currentPlayers[0].Player.Name;
                    data.Level = currentLevelNumber;
                    if (game.currentPlayers[0].Player.levelsUnlocked >= currentLevelNumber)
                        return;

                    XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                    serializer.Serialize(isolatedFileStream, data);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        public void LoadSavedGame()
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(SAVE_GAME_FILENAME, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                    SaveGameData data = (SaveGameData)serializer.Deserialize(isolatedFileStream);
                    game.currentPlayers[0].Player.levelsUnlocked = (byte)data.Level;
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        public void SaveOptions()
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(OPTIONS_FILENAME, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(OptionsState));
                    serializer.Serialize(isolatedFileStream, optionsState);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        public void LoadOptions()
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            if (isolatedStorageFile.FileExists(OPTIONS_FILENAME))
            {
                LoadOptionsFromStorage(isolatedStorageFile);
            } 
            else
            {
                optionsState = new OptionsState();
                optionsState.FXLevel = 0.7f;
                optionsState.MusicLevel = 0.6f;
                optionsState.Player = InputMode.Keyboard;
            }
            audioManager.SetOptions(optionsState.FXLevel, optionsState.MusicLevel); 
        }

        public void LoadLeaderBoard()
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            if (isolatedStorageFile.FileExists(LEADERBOARD_FILENAME))
            {
                LoadLeaderBoardsFromStorage(isolatedStorageFile);
            }
            else
            {
                LoadDefaultLeaderBoard();
            }
        }

        public void SaveLeaderBoard(int score)
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(LEADERBOARD_FILENAME, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    TopScoreEntry entry = new TopScoreEntry(name, score);
                    game.mScores.addEntry(0, entry);

                    using (BinaryWriter writer = new BinaryWriter(isolatedFileStream))
                    {
                        game.mScores.save(writer);
                    }
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        private void LoadDefaultLeaderBoard()
        {
            // Create a List and a fake list
            if (game != null)
            {
                game.mScores = new TopScoreListContainer(1, 10);
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
                    game.mScores.addEntry(0, entry);
                }
            }
        }

        private void LoadLeaderBoardsFromStorage(IsolatedStorageFile isolatedStorageFile)
        {
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(LEADERBOARD_FILENAME, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(isolatedFileStream);
                    if (game != null)
                    {
                        game.mScores = new TopScoreListContainer(reader);
                    }
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        private void LoadOptionsFromStorage(IsolatedStorageFile isolatedStorageFile)
        {
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(OPTIONS_FILENAME, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(OptionsState));
                    optionsState = (OptionsState)serializer.Deserialize(isolatedFileStream);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                game.BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }
    }
}
