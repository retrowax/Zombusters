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
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using ZombustersWindows;

namespace ZombustersWindows.Subsystem_Managers
{
#if !WINDOWS_PHONE
    public class StorageDeviceManager : DrawableGameComponent
    {
        MyGame game;
        public StorageDevice device;
        public bool isSavingOrLoading;
        public bool isStartScreen;

        Texture2D SaveAnimationTexture;
        Vector2 SaveAnimationOrigin;
        Animation SaveAnimation;
        Vector2 SaveAnimationPosition;

        private float timer;

        public StorageDeviceManager(MyGame game)
            : base(game)
        {
            this.game = game;
        }

        public override void  Initialize()
        {
            SaveAnimationPosition = new Vector2(100, 100);
            this.isSavingOrLoading = false;
            this.isStartScreen = false;


 	        base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load multiple animations form XML definition
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");

            //Save  Animation
            var definition = doc.Root.Element("SaveDef");
            SaveAnimationTexture = game.Content.Load<Texture2D>(@"Menu/saveAnimation");
            SaveAnimationOrigin = new Vector2(SaveAnimationTexture.Width / 2, SaveAnimationTexture.Height / 2);

            Point frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);

            Point sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);

            TimeSpan frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));

            // Define a new Animation instance
            SaveAnimation = new Animation(SaveAnimationTexture, frameSize, sheetSize, frameInterval);
        }


        //[Serializable]
        public struct SaveGameData
        {
            public string PlayerName;
            public int Level;
        }

        public void GetDevice(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            if (device != null && device.IsConnected)
            {
                //DoSaveGame(device);
                //DoLoadGame(device);
                //DoCreate(device);
                //DoOpen(device);
                //DoCopy(device);
                //DoEnumerate(device);
                //DoRename(device);
                //DoDelete(device);
                //DoOpenFile();
            }
        }


        public override void Update(GameTime gameTime)
        {
            SaveAnimation.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This method serializes a data object into
        /// the StorageContainer for this game.
        /// </summary>
        /// <param name="device"></param>
        public void DoSaveGame(Avatar player, int level)
        {
            /*
            if (player.Device != null)
            {
                string filename = "leaderboard.txt";
                Stream stream = player.Container.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryWriter writer = new BinaryWriter(stream);
                mScores.save(writer);
                writer.Close();
                stream.Close();
            }
            */

            // Create the data to save.
            SaveGameData data = new SaveGameData();
            string filename = "savegame.sav";

            data.PlayerName = player.Player.Name;
            data.Level = level;

            if (player.Player.levelsUnlocked >= level)
                return;

            this.isSavingOrLoading = true;

            // Open a storage container.
            if (player.Player.Device != null)
            {
                if (player.Player.Device.IsConnected)
                {
                    IAsyncResult result = player.Player.Device.BeginOpenContainer("Zombusters", null, null);
                    // Wait for the WaitHandle to become signaled.  
                    result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted)
                    {
                        StorageContainer container = player.Player.Device.EndOpenContainer(result);

                        if (container.FileExists(filename))
                            container.DeleteFile(filename);

                        Stream stream = container.CreateFile(filename);
                        XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));

                        serializer.Serialize(stream, data);

                        stream.Dispose();

                        container.Dispose();
                    }
                    // Close the wait handle.  
                    result.AsyncWaitHandle.Dispose();
                }
            }
        }
        /// <summary>
        /// This method loads a serialized data object
        /// from the StorageContainer for this game.
        /// </summary>
        /// <param name="device"></param>
        public void DoLoadGame(Avatar player)
        {
            this.isSavingOrLoading = true;
            string filename = "savegame.sav";

            if (player.Player.Device != null)
            {
                if (player.Player.Device.IsConnected)
                {
                    // Nos aseguramos de que el Container estee disponible
                    if (player.Player.Container.IsDisposed == false)
                    {
                        player.Player.Container.Dispose();
                    }

                    IAsyncResult result = player.Player.Device.BeginOpenContainer("Zombusters", null, null);
                    // Wait for the WaitHandle to become signaled.  
                    result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted)
                    {
                        if (player.Player.Device.IsConnected)
                        {
                            StorageContainer container = player.Player.Device.EndOpenContainer(result);

                            // Check to see whether the save exists.
                            if (!container.FileExists(filename))
                            {
                                player.Player.levelsUnlocked = 1;
                                // Notify the user there is no save.
                                return;
                            }

                            // Open the file.
                            Stream stream = container.OpenFile(filename, FileMode.Open, FileAccess.Read);

                            // Read the data from the file.
                            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                            SaveGameData data = (SaveGameData)serializer.Deserialize(stream);

                            // Close the file.
                            stream.Dispose();

                            if (data.PlayerName == player.Player.Name)
                            {
                                player.Player.levelsUnlocked = data.Level;
                            }

                            // Dispose the container.
                            container.Dispose();

                            // Report the data to the console.
#if XBOX && DEBUG
                    Debug.WriteLine("Name:     " + data.PlayerName);
                    Debug.WriteLine("Level:    " + data.Level.ToString());
#endif
                        }
                        else
                        {
                            player.Player.levelsUnlocked = 1;
                            return;
                        }
                    }
                    // Close the wait handle.  
                    result.AsyncWaitHandle.Dispose();
                }
            }
            else
            {
                player.Player.levelsUnlocked = 1;
            }

/*
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "savegame.sav");

            // Check to see whether the save exists.
            if (!File.Exists(filename))
                // Notify the user there is no save.
                return;

            // Open the file.
            FileStream stream = File.Open(filename, FileMode.Open,
                FileAccess.Read);

            // Read the data from the file.
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
            SaveGameData data = (SaveGameData)serializer.Deserialize(stream);

            // Close the file.
            stream.Close();

            // Dispose the container.
            container.Dispose();

            // Report the data to the console.
            #if XBOX && DEBUG
            Debug.WriteLine("Name:     " + data.PlayerName);
            Debug.WriteLine("Level:    " + data.Level.ToString());
            Debug.WriteLine("Score:    " + data.Score.ToString());
            #endif
 */
        }


        /// <summary>
        /// This method serializes a data object into
        /// the StorageContainer for this game.
        /// </summary>
        /// <param name="device"></param>
        public void DoSaveSettings(Player player)
        {
            string filename = "options.xml";

            // Open a storage container.
            if (player.Device != null)
            {
                if (player.Device.IsConnected)
                {
                    // Open a storage container.
                    IAsyncResult result = player.Device.BeginOpenContainer("Zombusters", null, null);
                    // Wait for the WaitHandle to become signaled.  
                    result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted)
                    {
                        StorageContainer container = player.Device.EndOpenContainer(result);

                        if (container.FileExists(filename))
                            container.DeleteFile(filename);

                        Stream stream = container.CreateFile(filename);
                        XmlSerializer serializer = new XmlSerializer(typeof(OptionsState));

                        serializer.Serialize(stream, player.optionsState);

                        stream.Dispose();
                        if (container != null)
                            container.Dispose();
                    }
                    // Close the wait handle.  
                    result.AsyncWaitHandle.Dispose();
                }
            }
        }

        /// <summary>
        /// This method serializes a data object into
        /// the StorageContainer for this game.
        /// </summary>
        /// <param name="device"></param>
        public void DoSaveLeaderBoards(Avatar player)
        {
            string filename = "leaderboard.txt";

            // Open a storage container.
            if (player.Player.Device != null)
            {
                if (player.Player.Device.IsConnected)
                {
                    // Open a storage container.
                    IAsyncResult result = player.Player.Device.BeginOpenContainer("Zombusters", null, null);
                    // Wait for the WaitHandle to become signaled.  
                    result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted)
                    {
                        StorageContainer container = player.Player.Device.EndOpenContainer(result);

                        if (container.FileExists(filename))
                            container.DeleteFile(filename);

                        Stream stream = container.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        BinaryWriter writer = new BinaryWriter(stream);
                        game.mScores.save(writer);
                        writer.Dispose();

                        stream.Dispose();
                        if (container != null)
                            container.Dispose();
                    }
                    // Close the wait handle.  
                    result.AsyncWaitHandle.Dispose();
                }
            }
        }


        /// <summary>
        /// This method creates a file called demobinary.sav and places
        /// it in the StorageContainer for this game.
        /// </summary>
        /// <param name="device"></param>
        private static void DoCreate(StorageDevice device)
        {
/* REVISAR
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Add the container path to our file name.
            string filename = Path.Combine(container.Path, "demobinary.sav");

            // Create a new file.
            if (!File.Exists(filename))
            {
                FileStream file = File.Create(filename);
                file.Close();
            }
            // Dispose the container, to commit the data.
            container.Dispose();
 */
        }
        /// <summary>
        /// This method illustrates how to open a file. It presumes
        /// that demobinary.sav has been created.
        /// </summary>
        /// <param name="device"></param>
        private static void DoOpen(StorageDevice device)
        {
/* REVISAR!!
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Add the container path to our file name.
            string filename = Path.Combine(container.Path, "demobinary.sav");

            FileStream file = File.Open(filename, FileMode.Open);
            file.Close();

            // Dispose the container.
            container.Dispose();
 */
        }
        /// <summary>
        /// This method illustrates how to copy files.  It presumes
        /// that demobinary.sav has been created.
        /// </summary>
        /// <param name="device"></param>
        private static void DoCopy(StorageDevice device)
        {
/* REVISAR!!!
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Add the container path to our file name.
            string filename = Path.Combine(container.Path, "demobinary.sav");
            string copyfilename = Path.Combine(container.Path, "copybinary.sav");

            File.Copy(filename, copyfilename, true);

            // Dispose the container, to commit the change.
            container.Dispose();
 */ 
        }
        /// <summary>
        /// This method illustrates how to rename files.  It presumes
        /// that demobinary.sav has been created.
        /// </summary>
        /// <param name="device"></param>
        private static void DoRename(StorageDevice device)
        {
/* REVISAR!!
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Add the container path to our file name.
            string oldfilename = Path.Combine(container.Path, "demobinary.sav");
            string newfilename = Path.Combine(container.Path, "renamebinary.sav");

            if (!File.Exists(newfilename))
                File.Move(oldfilename, newfilename);

            // Dispose the container, to commit the change
            container.Dispose();
 */
        }
        /// <summary>
        /// This method illustrates how to enumerate files in a 
        /// StorageContainer.
        /// </summary>
        /// <param name="device"></param>
        private static void DoEnumerate(StorageDevice device)
        {
/* REVISAR!!
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            ICollection<string> FileList = Directory.GetFiles(container.Path);
            foreach (string filename in FileList)
            {
                Console.WriteLine(filename);
            }

            // Dispose the container.
            container.Dispose();
 */
        }
        /// <summary>
        /// This method deletes a file previously created by this demo.
        /// </summary>
        /// <param name="device"></param>
        private static void DoDelete(StorageDevice device)
        {
/* REVISAR!!!
            // Open a storage container.
            StorageContainer container =
                device.OpenContainer("StorageDemo");

            // Add the container path to our file name.
            string filename = Path.Combine(container.Path, "demobinary.sav");

            // Delete the new file.
            if (File.Exists(filename))
                File.Delete(filename);

            // Dispose the container, to commit the change.
            container.Dispose();
 */
        }
        /// <summary>
        /// This method opens a file using System.IO classes and the
        /// TitleLocation property.  It presumes that a file named
        /// ship.dds has been deployed alongside the game.
        /// </summary>
        private static void DoOpenFile()
        {
/*REVISAR!!
            FileStream file = OpenTitleFile(
                "ship.dds", FileMode.Open, FileAccess.Read);
            Console.WriteLine("File Size: " + file.Length);
            file.Close();
 */
        }
/* REVISAR!!!
        private static FileStream OpenTitleFile(
            string filename, FileMode mode, FileAccess access)
        {
           
            string fullpath = Path.Combine(StorageContainer.TitleLocation, filename);
            return File.Open(fullpath, mode, access);
        }
*/

        public void Draw(SpriteBatch batch, GameTime gameTime, Vector2 Position, bool inStartScreen)
        {
            if (isStartScreen == true && inStartScreen)
            {
                SaveAnimation.Draw(batch, Position, SpriteEffects.None, 0.1f, 0f, Color.White);
            }

            if (isSavingOrLoading == true && !inStartScreen)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 5.0f)
                {
                    this.isSavingOrLoading = false;
                    timer = 0;
                }

                SaveAnimation.Draw(batch, Position, SpriteEffects.None, 0.1f, 0f, Color.White);
            }
        }

    }
#endif
}