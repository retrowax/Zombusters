using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

namespace ZombustersWindows.Subsystem_Managers
{
    public partial class Level
    {
        public List<Vector2> PlayerSpawnPosition;
        public List<Vector4> ZombieSpawnZones;
        public Texture2D BackgroundTexture;
        public List<Wall> wallList;
        public List<Furniture> furnitureList;
        public List<SubLevel> subLevelList;
        private List<EnemiesCount> enemiesList;
        public GameWorld gameWorld;
        public String mapTextureFileName;

        public Level(LevelType level)
        {
            gameWorld = new GameWorld(true, true);
            furnitureList = new List<Furniture>();
            wallList = new List<Wall>();
            subLevelList = new List<SubLevel>();
            PlayerSpawnPosition = new List<Vector2>();
            ZombieSpawnZones = new List<Vector4>();
            string furnituresfilename;
            string wallsfilename;
            string enemiesfilename;
            int i;
            int X, Y, W, Z;
            //float lIndex = 0.8f; //layer index
            CFurnitureComparer furnitureComparer = new CFurnitureComparer();
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/LevelsDef.xml");
            var levelparameters = doc.Root.Element("Level1");

            switch (level)
            {
                case LevelType.One:
                    furnituresfilename = "LevelXMLs/Level1/Level1_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level1/Level1_walls.xml";
                    enemiesfilename = "LevelXMLs/Level1/Level1_enemies.xml";
                    mapTextureFileName = @"Maps/map1";
                    levelparameters = doc.Root.Element("Level1");
                    break;
                case LevelType.Two:
                    furnituresfilename = "LevelXMLs/Level2/Level2_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level2/Level2_walls.xml";
                    enemiesfilename = "LevelXMLs/Level2/Level2_enemies.xml";
                    mapTextureFileName = @"Maps/map2";
                    levelparameters = doc.Root.Element("Level2");
                    break;
                case LevelType.Three:
                    furnituresfilename = "LevelXMLs/Level3/Level3_furnitures.xml";
                    enemiesfilename = "LevelXMLs/Level3/Level3_enemies.xml";
                    wallsfilename = "LevelXMLs/Level3/Level3WP_walls.xml";
                    mapTextureFileName = @"Maps/map3wp";
                    levelparameters = doc.Root.Element("Level3");
                    break;
                case LevelType.Four:
                    furnituresfilename = "LevelXMLs/Level4/Level4_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level4/Level4_walls.xml";
                    enemiesfilename = "LevelXMLs/Level4/Level4_enemies.xml";
                    mapTextureFileName = @"Maps/map4";
                    levelparameters = doc.Root.Element("Level4");
                    break;
                case LevelType.Five:
                    furnituresfilename = "LevelXMLs/Level5/Level5_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level5/Level5_walls.xml";
                    enemiesfilename = "LevelXMLs/Level5/Level5_enemies.xml";
                    mapTextureFileName = @"Maps/map5";
                    levelparameters = doc.Root.Element("Level5");
                    break;
                case LevelType.Six:
                    furnituresfilename = "LevelXMLs/Level6/Level6_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level6/Level6_walls.xml";
                    enemiesfilename = "LevelXMLs/Level6/Level6_enemies.xml";
                    mapTextureFileName = @"Maps/map6";
                    levelparameters = doc.Root.Element("Level6");
                    break;
                case LevelType.Seven:
                    furnituresfilename = "LevelXMLs/Level7/Level7_furnitures.xml";
                    enemiesfilename = "LevelXMLs/Level7/Level7_enemies.xml";
                    wallsfilename = "LevelXMLs/Level7/Level7WP_walls.xml";
                    mapTextureFileName = @"Maps/map7wp";
                    levelparameters = doc.Root.Element("Level7");
                    break;
                case LevelType.Eight:
                    furnituresfilename = "LevelXMLs/Level8/Level8_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level8/Level8_walls.xml";
                    enemiesfilename = "LevelXMLs/Level8/Level8_enemies.xml";
                    mapTextureFileName = @"Maps/map8";
                    levelparameters = doc.Root.Element("Level8");
                    break;
                case LevelType.Nine:
                    furnituresfilename = "LevelXMLs/Level9/Level9_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level9/Level9_walls.xml";
                    enemiesfilename = "LevelXMLs/Level9/Level9_enemies.xml";
                    mapTextureFileName = @"Maps/map9";
                    levelparameters = doc.Root.Element("Level9");
                    break;
                case LevelType.Ten:
                    furnituresfilename = "LevelXMLs/Level10/Level10_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level10/Level10_walls.xml";
                    enemiesfilename = "LevelXMLs/Level10/Level10_enemies.xml";
                    mapTextureFileName = @"Maps/map10";
                    levelparameters = doc.Root.Element("Level10");
                    break;
                default:
                    furnituresfilename = "LevelXMLs/Level1/Level1_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level1/Level1_walls.xml";
                    enemiesfilename = "LevelXMLs/Level1/Level1_enemies.xml";
                    mapTextureFileName = @"Maps/map1";
                    levelparameters = doc.Root.Element("Level1");
                    break;
            }

            for (i = 1; i < 5; i++)
            {
                // Add Spawn Positions for the Players
                X = int.Parse(levelparameters.Attribute("P" + i.ToString() + "SpawnPos").Value.Split(',')[0], NumberStyles.Integer);
                Y = int.Parse(levelparameters.Attribute("P" + i.ToString() + "SpawnPos").Value.Split(',')[1], NumberStyles.Integer);
                PlayerSpawnPosition.Add(new Vector2(X, Y));

                // Add Spawn Positions for the Enemies
                X = int.Parse(levelparameters.Attribute("ZSpawnZone" + i.ToString() + "Origin").Value.Split(',')[0], NumberStyles.Integer);
                Y = int.Parse(levelparameters.Attribute("ZSpawnZone" + i.ToString() + "Origin").Value.Split(',')[1], NumberStyles.Integer);
                Z = int.Parse(levelparameters.Attribute("ZSpawnZone" + i.ToString() + "End").Value.Split(',')[0], NumberStyles.Integer);
                W = int.Parse(levelparameters.Attribute("ZSpawnZone" + i.ToString() + "End").Value.Split(',')[1], NumberStyles.Integer);
                ZombieSpawnZones.Add(new Vector4(X, Y, Z, W));
            }

            // Load Furniture Stream
            Stream stream = TitleContainer.OpenStream(furnituresfilename); //"LevelXMLs/Level1/Level1_furnitures.xml"
            XmlSerializer serializer = new XmlSerializer(typeof(List<Furniture>));

            furnitureList = (List<Furniture>)serializer.Deserialize(stream);

/*
            // Sort furniture list by Position.Y to add layer index
            furnitureList.Sort(delegate(CFurniture p1, CFurniture p2)
            {
                return p1.Position.Y.CompareTo(p2.Position.Y);
            });

            furnitureList.Sort(furnitureComparer);

            // Apply layer index to sorted list
            foreach (CFurniture furniture in furnitureList)
            {
                furniture.layerIndex = lIndex;
                lIndex -= 0.01f;
            }
*/
            stream.Dispose();


            // Load Wall Stream
            stream = TitleContainer.OpenStream(wallsfilename);
            serializer = new XmlSerializer(typeof(List<Wall>));

            wallList = (List<Wall>)serializer.Deserialize(stream);

            stream.Dispose();

            // Load Wall Stream
            stream = TitleContainer.OpenStream(enemiesfilename);
            serializer = new XmlSerializer(typeof(List<EnemiesCount>));

            enemiesList = (List<EnemiesCount>)serializer.Deserialize(stream);

            for (i = 0; i < 10; i++)
            {
                switch (i)
                {
                    case 0:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.One));
                        break;
                    case 1:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Two));
                        break;
                    case 2:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Three));
                        break;
                    case 3:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Four));
                        break;
                    case 4:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Five));
                        break;
                    case 5:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Six));
                        break;
                    case 6:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Seven));
                        break;
                    case 7:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Eight));
                        break;
                    case 8:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Nine));
                        break;
                    case 9:
                        subLevelList.Add(new SubLevel(enemiesList[i], SubLevel.SubLevelType.Ten));
                        break;
                    default:
                        subLevelList.Add(new SubLevel(enemiesList[0], SubLevel.SubLevelType.One));
                        break;
                }
            }

            stream.Dispose();
        }

        public void Initialize(MyGame game)
        {
            foreach (Furniture furniture in furnitureList)
            {
                gameWorld.Obstacles.Add(new Circle(game.GraphicsDevice, furniture.ObstaclePosition, furniture.ObstacleRadius));
            }

            foreach (Wall wall in wallList)
            {
                gameWorld.Walls.Add(new ZombustersWindows.Wall(wall.From, wall.To, game.GraphicsDevice));
            }
        }

        public class CFurnitureComparer : IComparer<Furniture>
        {
            public int Compare(Furniture x, Furniture y)
            {
                if ((x.Position.Y + x.Texture.Height) == (y.Position.Y + y.Texture.Height)) return 0;
                return ((x.Position.Y + x.Texture.Height) > (y.Position.Y + y.Texture.Height)) ? 1 : -1;
            }
        } 

        public LevelType GetNextLevel(LevelType currentLevel)
        {
            LevelType nextLevel;

            switch (currentLevel)
            {
                case LevelType.One:
                    nextLevel = LevelType.Two;
                    break;
                case LevelType.Two:
#if DEMO
                    nextLevel = LevelType.EndDemo;
#else
                    nextLevel = LevelType.Three;
#endif
                    break;
                case LevelType.Three:
                    nextLevel = LevelType.Four;
                    break;
                case LevelType.Four:
                    nextLevel = LevelType.Five;
                    break;
                case LevelType.Five:
                    nextLevel = LevelType.Six;
                    break;
                case LevelType.Six:
                    nextLevel = LevelType.Seven;
                    break;
                case LevelType.Seven:
                    nextLevel = LevelType.Eight;
                    break;
                case LevelType.Eight:
                    nextLevel = LevelType.Nine;
                    break;
                case LevelType.Nine:
                    nextLevel = LevelType.Ten;
                    break;
                case LevelType.Ten:
                    nextLevel = LevelType.EndGame;
                    break;
                default:
                    nextLevel = LevelType.One;
                    break;
            }

            return nextLevel;
        }

        public int getLevelNumber(LevelType currentLevel)
        {
            int levelnumber = 1;

            switch (currentLevel)
            {
                case LevelType.One:
                    levelnumber = 1;
                    break;
                case LevelType.Two:
                    levelnumber = 2;
                    break;
                case LevelType.Three:
                    levelnumber = 3;
                    break;
                case LevelType.Four:
                    levelnumber = 4;
                    break;
                case LevelType.Five:
                    levelnumber = 5;
                    break;
                case LevelType.Six:
                    levelnumber = 6;
                    break;
                case LevelType.Seven:
                    levelnumber = 7;
                    break;
                case LevelType.Eight:
                    levelnumber = 8;
                    break;
                case LevelType.Nine:
                    levelnumber = 9;
                    break;
                case LevelType.Ten:
                    levelnumber = 10;
                    break;
                default:
                    levelnumber = 1;
                    break;
            }

            return levelnumber;
        }
    }
}
