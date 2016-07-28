using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace ZombustersWindows.Subsystem_Managers
{
    //this enum must be synched up with the order in the XML file
    public enum Achievements
    {
        Dodger,
        Unstoppable,
        QuickDead,
        EagleEye
    }

    //[Serializable]
    public class Achievement
    {
        public int ID;
        public string Name;
        public int Points;
        public string Description;
    }

    //[Serializable]
    public class PlayerAchievement
    {
        public int ID;
        public DateTime DateUnlocked;
    }

    //used in AchievementScreen
    public class AchievementDisplay
    {
        public Texture2D Icon;
        public string Name;
        public string Points;
        public string DateUnlocked;
    }

    public class AchievementManager
    {
        private List<PlayerAchievement> _playerAchievements;
        private List<Achievement> _achievements;

        private Game _game;

        public AchievementManager(Game game, Player player)
        {
#if !WINDOWS_PHONE && !WINDOWS
            //load achievements
            _achievements = new List<Achievement>();
            StorageContainer container;

            // Open a storage container.
            IAsyncResult result = player.Device.BeginOpenContainer("Zombusters", null, null);
            // Wait for the WaitHandle to become signaled.  
            result.AsyncWaitHandle.WaitOne();
            if (result.IsCompleted)
            {
                
                if (player.Container == null)
                {
                    container = player.Device.EndOpenContainer(result);
                }
                else
                {
                    container = player.Container;
                }
                

                string filename = "Achievements.xml";

                Stream stream = TitleContainer.OpenStream(filename);
                //Stream stream = container.OpenFile(filename, FileMode.Open, FileAccess.Read);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Achievement>));

                _achievements = (List<Achievement>)serializer.Deserialize(stream);

                stream.Close();

                //load player achievements
                filename = "playerachievements.xml";

                if (container.FileExists(filename))
                {
                    stream = container.OpenFile(filename, FileMode.Open, FileAccess.Read);
                    serializer = new XmlSerializer(typeof(List<PlayerAchievement>));
                    _playerAchievements = (List<PlayerAchievement>)serializer.Deserialize(stream);
                }

                stream.Close();
                container.Dispose();

            }
            // Close the wait handle.  
            result.AsyncWaitHandle.Close();
/*
            //load achievements
            StorageContainer container = player.Device.OpenContainer("Zombusters");
            // Get the path of the save game.
            string filename = Path.Combine(StorageContainer.TitleLocation, "Achievements.xml");


            FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);

            //FileStream fs = new FileStream(StorageContainer.TitleLocation + "\\achievements.xml", FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Achievement>));

            _achievements = (List<Achievement>)serializer.Deserialize(fs);

            fs.Close();

            //load player achievements
            filename = Path.Combine(container.Path, "playerachievements.xml");
            if (File.Exists(filename))
            {
                serializer = new XmlSerializer(typeof(List<PlayerAchievement>));
                fs = File.Open(filename, FileMode.Open);
                //fs = new FileStream(StorageContainer.TitleLocation + "\\playerachievements.xml", FileMode.Open);

                _playerAchievements = (List<PlayerAchievement>)serializer.Deserialize(fs);
            }
*/
            _game = game;
#endif
        }

        public void Save(Player player)
        {
#if !WINDOWS_PHONE && !WINDOWS
            // Open a storage container.
            IAsyncResult result = player.Device.BeginOpenContainer("Zombusters", null, null);
            if (result.IsCompleted)
            {
                StorageContainer container = player.Device.EndOpenContainer(result);
                string filename = "playerachievements.xml";

                //load player achievements
                Stream stream = container.CreateFile(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerAchievement));
                serializer.Serialize(stream, _playerAchievements);

                stream.Close();
                container.Dispose();
            }
#endif
/*
            //achievements
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerAchievement));
            //load achievements
            StorageContainer container = player.Device.OpenContainer("Zombusters");
            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "playerachievements.xml");


            FileStream fs = File.Open(filename, FileMode.Create);
            //FileStream fs = new FileStream(StorageContainer.TitleLocation + "\\playerachievements.xml", FileMode.Create);

            serializer.Serialize(fs, _playerAchievements);

            fs.Close();
 */
        }

        public bool PlayerHasAchievements()
        {
            if (_playerAchievements != null)
                return (_playerAchievements.Count > 0);
            else
                return false;
        }

        public int PlayerAchievementCount()
        {
            if (_playerAchievements != null)
                return _playerAchievements.Count;
            else
                return 0;
        }

        public List<AchievementDisplay> Achievements()
        {
            List<AchievementDisplay> achievements = new List<AchievementDisplay>();

            AchievementDisplay display;

            //ContentManager content = new ContentManager(_game.Services);

            foreach (Achievement achievement in _achievements)
            {
                display = new AchievementDisplay();

                display.Icon = ((MyGame)this._game).Content.Load<Texture2D>(@"Achievements/achieve_" + achievement.ID.ToString());
                display.Name = achievement.Name;
                display.Points = achievement.Points.ToString();
                display.DateUnlocked = GetDateUnlocked(achievement.ID);

                achievements.Add(display);
            }

            return achievements;
        }

        private string GetDateUnlocked(int id)
        {
            if (_playerAchievements != null)
            {
                foreach (PlayerAchievement achievement in _playerAchievements)
                    if (achievement.ID == id)
                        return achievement.DateUnlocked.ToString();
            }

            return "";
        }

        public void AddAchievement(Achievements type)
        {
            PlayerAchievement achievement = new PlayerAchievement();

            achievement.DateUnlocked = DateTime.Now;
            achievement.ID = _achievements[(int)type].ID;

            if (_playerAchievements == null)
                _playerAchievements = new List<PlayerAchievement>();

            _playerAchievements.Add(achievement);
        }

        public bool IsAchievementEarned(Achievements type)
        {
            if (_playerAchievements != null)
            {
                for (int i = 0; i < _playerAchievements.Count; i++)
                    if (_playerAchievements[i].ID == _achievements[(int)type].ID)
                        return true;
            }

            return false;

        }
    }
}
