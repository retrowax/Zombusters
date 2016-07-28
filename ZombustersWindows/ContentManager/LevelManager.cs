using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace ZombustersWindows.Subsystem_Managers
{
    //Main CLevel Class
    public class CLevel
    {
        public enum Level
        {
            One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, FinalJuego
        }

        public List<Vector2> PlayerSpawnPosition;
        public List<Vector4> ZombieSpawnZones;
        public Texture2D BackgroundTexture;
        public List<CWall> wallList;
        public List<CFurniture> furnitureList;
        public List<CSubLevel> subLevelList;
        private List<CEnemies> enemiesList;
        public GameWorld gameWorld;
        public String mapTextureFileName;

        public CLevel(Player player, Level level)
        {
            gameWorld = new GameWorld(true, true);
            furnitureList = new List<CFurniture>();
            wallList = new List<CWall>();
            subLevelList = new List<CSubLevel>();
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
                case Level.One:
                    furnituresfilename = "LevelXMLs/Level1/Level1_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level1/Level1_walls.xml";
                    enemiesfilename = "LevelXMLs/Level1/Level1_enemies.xml";
                    mapTextureFileName = @"Maps/map1";
                    levelparameters = doc.Root.Element("Level1");
                    break;
                case Level.Two:
                    furnituresfilename = "LevelXMLs/Level2/Level2_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level2/Level2_walls.xml";
                    enemiesfilename = "LevelXMLs/Level2/Level2_enemies.xml";
                    mapTextureFileName = @"Maps/map2";
                    levelparameters = doc.Root.Element("Level2");
                    break;
                case Level.Three:
                    furnituresfilename = "LevelXMLs/Level3/Level3_furnitures.xml";
                    enemiesfilename = "LevelXMLs/Level3/Level3_enemies.xml";
                    wallsfilename = "LevelXMLs/Level3/Level3_walls.xml";
                    mapTextureFileName = @"Maps/map3";
                    levelparameters = doc.Root.Element("Level3");
                    break;
                case Level.Four:
                    furnituresfilename = "LevelXMLs/Level4/Level4_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level4/Level4_walls.xml";
                    enemiesfilename = "LevelXMLs/Level4/Level4_enemies.xml";
                    mapTextureFileName = @"Maps/map4";
                    levelparameters = doc.Root.Element("Level4");
                    break;
                case Level.Five:
                    furnituresfilename = "LevelXMLs/Level5/Level5_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level5/Level5_walls.xml";
                    enemiesfilename = "LevelXMLs/Level5/Level5_enemies.xml";
                    mapTextureFileName = @"Maps/map5";
                    levelparameters = doc.Root.Element("Level5");
                    break;
                case Level.Six:
                    furnituresfilename = "LevelXMLs/Level6/Level6_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level6/Level6_walls.xml";
                    enemiesfilename = "LevelXMLs/Level6/Level6_enemies.xml";
                    mapTextureFileName = @"Maps/map6";
                    levelparameters = doc.Root.Element("Level6");
                    break;
                case Level.Seven:
                    furnituresfilename = "LevelXMLs/Level7/Level7_furnitures.xml";
                    enemiesfilename = "LevelXMLs/Level7/Level7_enemies.xml";
                    wallsfilename = "LevelXMLs/Level7/Level7_walls.xml";
                    mapTextureFileName = @"Maps/map7";
                    levelparameters = doc.Root.Element("Level7");
                    break;
                case Level.Eight:
                    furnituresfilename = "LevelXMLs/Level8/Level8_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level8/Level8_walls.xml";
                    enemiesfilename = "LevelXMLs/Level8/Level8_enemies.xml";
                    mapTextureFileName = @"Maps/map8";
                    levelparameters = doc.Root.Element("Level8");
                    break;
                case Level.Nine:
                    furnituresfilename = "LevelXMLs/Level9/Level9_furnitures.xml";
                    wallsfilename = "LevelXMLs/Level9/Level9_walls.xml";
                    enemiesfilename = "LevelXMLs/Level9/Level9_enemies.xml";
                    mapTextureFileName = @"Maps/map9";
                    levelparameters = doc.Root.Element("Level9");
                    break;
                case Level.Ten:
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
            XmlSerializer serializer = new XmlSerializer(typeof(List<CFurniture>));

            furnitureList = (List<CFurniture>)serializer.Deserialize(stream);

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
            serializer = new XmlSerializer(typeof(List<CWall>));

            wallList = (List<CWall>)serializer.Deserialize(stream);

            stream.Dispose();

            // Load Wall Stream
            stream = TitleContainer.OpenStream(enemiesfilename);
            serializer = new XmlSerializer(typeof(List<CEnemies>));

            enemiesList = (List<CEnemies>)serializer.Deserialize(stream);

            for (i = 0; i < 10; i++)
            {
                switch (i)
                {
                    case 0:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.One));
                        break;
                    case 1:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Two));
                        break;
                    case 2:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Three));
                        break;
                    case 3:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Four));
                        break;
                    case 4:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Five));
                        break;
                    case 5:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Six));
                        break;
                    case 6:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Seven));
                        break;
                    case 7:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Eight));
                        break;
                    case 8:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Nine));
                        break;
                    case 9:
                        subLevelList.Add(new CSubLevel(enemiesList[i], CSubLevel.SubLevel.Ten));
                        break;
                    default:
                        subLevelList.Add(new CSubLevel(enemiesList[0], CSubLevel.SubLevel.One));
                        break;
                }
            }

            stream.Dispose();
        }

        public void Initialize(MyGame game)
        {
            foreach (CFurniture furniture in furnitureList)
            {
                gameWorld.Obstacles.Add(new Circle(game.GraphicsDevice, furniture.ObstaclePosition, furniture.ObstacleRadius));
            }

            foreach (CWall wall in wallList)
            {
                gameWorld.Walls.Add(new Wall(wall.From, wall.To, game.GraphicsDevice));
            }
        }

        public class CFurnitureComparer : IComparer<CFurniture>
        {
            public int Compare(CFurniture x, CFurniture y)
            {
                if ((x.Position.Y + x.Texture.Height) == (y.Position.Y + y.Texture.Height)) return 0;
                return ((x.Position.Y + x.Texture.Height) > (y.Position.Y + y.Texture.Height)) ? 1 : -1;
            }
        } 

        public CLevel.Level getNextLevel(CLevel.Level currentLevel)
        {
            CLevel.Level nextLevel;

            switch (currentLevel)
            {
                case Level.One:
                    nextLevel = Level.Two;
                    break;
                case Level.Two:
                    nextLevel = Level.Three;
                    break;
                case Level.Three:
                    nextLevel = Level.Four;
                    break;
                case Level.Four:
                    nextLevel = Level.Five;
                    break;
                case Level.Five:
                    nextLevel = Level.Six;
                    break;
                case Level.Six:
                    nextLevel = Level.Seven;
                    break;
                case Level.Seven:
                    nextLevel = Level.Eight;
                    break;
                case Level.Eight:
                    nextLevel = Level.Nine;
                    break;
                case Level.Nine:
                    nextLevel = Level.Ten;
                    break;
                case Level.Ten:
                    nextLevel = Level.FinalJuego;
                    break;
                default:
                    nextLevel = Level.One;
                    break;
            }

            return nextLevel;
        }

        public int getLevelNumber(Level currentLevel)
        {
            int levelnumber = 1;

            switch (currentLevel)
            {
                case CLevel.Level.One:
                    levelnumber = 1;
                    break;
                case CLevel.Level.Two:
                    levelnumber = 2;
                    break;
                case CLevel.Level.Three:
                    levelnumber = 3;
                    break;
                case CLevel.Level.Four:
                    levelnumber = 4;
                    break;
                case CLevel.Level.Five:
                    levelnumber = 5;
                    break;
                case CLevel.Level.Six:
                    levelnumber = 6;
                    break;
                case CLevel.Level.Seven:
                    levelnumber = 7;
                    break;
                case CLevel.Level.Eight:
                    levelnumber = 8;
                    break;
                case CLevel.Level.Nine:
                    levelnumber = 9;
                    break;
                case CLevel.Level.Ten:
                    levelnumber = 10;
                    break;
                default:
                    levelnumber = 1;
                    break;
            }

            return levelnumber;
        }
    }

    public class CSubLevel
    {
        public enum SubLevel
        {
            One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten
        }

        public CEnemies enemies;
        public SubLevel subLevel;
        //public List<PowerUps> powerUpsList;

        public CSubLevel(CEnemies enemies, SubLevel subLevel)
        {
            this.enemies = enemies;
            this.subLevel = subLevel;
        }
    }

    public class CEnemies
    {
        public int Zombies;
        public int Tanks;
    }

    public class CWall
    {
        public Vector2 From;
        public Vector2 To;
    }

    public class CFurniture
    {
        public enum FurnitureType
        {
            Basura,Arbol,Banco,Farola,Coche,CocheArdiendo,Puente
        }

        public enum FurnitureOrientation
        {
            NorthWest, NorthEast, SouthWest, SouthEast
        }

        [XmlElement]
        public FurnitureType Type;

        [XmlElement]
        public FurnitureOrientation Orientation;

        [XmlElement]
        public Vector2 Position;

        [XmlElement]
        public Vector2 ObstaclePosition;

        [XmlIgnore]
        public float ObstacleRadius;

        [XmlIgnore]
        public Texture2D Texture;

        [XmlIgnore]
        private Texture2D TextureShadow;

        [XmlIgnore]
        private Vector2 TextureShadowPosition;

        [XmlIgnore]
        private Animation TextureAnimation;
#if XBOX
        public Renderer particleRenderer;
        public ParticleEffect SmokeEffect;
#endif
        [XmlIgnore]
        public float layerIndex;
        

        /*
        public CFurniture(FurnitureType type, Orientation orientation, Vector2 position)
        {
            this.tipo = type;
            this.orientacion = orientation;
            this.Position = position;
        }*/

        public void Load(MyGame game)
        {
            if (Type == FurnitureType.CocheArdiendo)
            {
                // Load multiple animations form XML definition
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");

                //ZOMBIE ANIMATION
                // Get the Zombie animation from the XML definition
                var definition = doc.Root.Element("crashCar");
                Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheArdiendo");
                TextureShadowPosition = new Vector2(0, 0);

                Point frameSize = new Point();
                frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
                frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);

                Point sheetSize = new Point();
                sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
                sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);

                TimeSpan frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));

                // Define a new Animation instance
                TextureAnimation = new Animation(Texture, frameSize, sheetSize, frameInterval);

#if XBOX
                // Efecto particulas de Humo
                SmokeEffect = new ParticleEffect 
                { 
                    new ConeEmitter 
                    { 
                        Budget = 500, 
                        Term = 3f,

                        Enabled = true,
                        Name = "SmokeEmitter",
                        BlendMode = EmitterBlendMode.Alpha,
                        TriggerOffset = new Vector2(0,0),
                        MinimumTriggerPeriod = 0.15f,
                        ReleaseImpulse = new Vector2(0,0),
                        ReleaseOpacity = new VariableFloat { Value = 1f, Variation = 0f},
                        ReleaseQuantity = 1,
                        ReleaseRotation = new VariableFloat { Value = 0, Variation = MathHelper.Pi },
                        ReleaseScale = new VariableFloat { Value = 5, Variation = 0 },
                        ReleaseSpeed = new VariableFloat { Value = 100, Variation = 50 },
                        ParticleTextureAssetName = "Cloud001",
                        Direction = -1.57f,
                        ConeAngle = 0.6f,

                        Modifiers = new ModifierCollection
                        {
                            new DampingModifier { DampingCoefficient = 0.95f },
                            new ScaleModifier 
                            {
                                InitialScale = 20,
                                UltimateScale = 150,
                            },
                            new RotationRateModifier
                            {
                                InitialRate = 1.57f,
                                FinalRate = 0,
                            },
                            new LinearGravityModifier { Gravity = new Vector2(150,0) },
                            new OpacityInterpolatorModifier
                            {
                                InitialOpacity = 0,
                                MiddleOpacity = 0.5f,
                                MiddlePosition = 0.1f,
                                FinalOpacity = 0,
                            },
                            new OpacityModifier
                            {
                                Initial = 1f,
                                Ultimate = 0f,
                            },
                            new ColourModifier
                            {
                                InitialColour = Color.Gray.ToVector3(),
                                UltimateColour = Color.Linen.ToVector3(),
                            },
                        },
                    } 
                };
#endif
#if XBOX
                switch (Orientation)
                {
                    case FurnitureOrientation.SouthEast:
                        smokeEffectPosition = new Vector2(Position.X + 68, Position.Y + 17);
                        break;
                    case FurnitureOrientation.SouthWest:
                        smokeEffectPosition = new Vector2(Position.X - 18, Position.Y + 17);
                        break;
                    default:
                        break;
                }
                //particleEffect = new ParticleEffect();
                //particleEffect = effect.DeepCopy();
                SmokeEffect.Initialise();
                SmokeEffect.LoadContent(game.Content);

                particleRenderer = new SpriteBatchRenderer
                {
                    GraphicsDeviceService = game.graphics
                };
                particleRenderer.LoadContent(game.Content);
#endif
            }

            if (Type == FurnitureType.Puente)
            {
                Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/Puente");
                TextureShadowPosition = new Vector2(0, 0);
            }

            if (Type == FurnitureType.Arbol)
            {
                Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/Arbol");
                TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/Arbol_shadow");
                TextureShadowPosition = new Vector2(45, 138);
            }

            if (Type == FurnitureType.Basura)
            {
                Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/Basura");
                TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/Basura_shadow");
                TextureShadowPosition = new Vector2(2, 26);
            }

            if (Type == FurnitureType.Banco)
            {
                switch (Orientation)
                {
                    case FurnitureOrientation.NorthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoNE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoNE_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    case FurnitureOrientation.NorthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoNW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoNW_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    case FurnitureOrientation.SouthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoSE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoSE_shadow");
                        TextureShadowPosition = new Vector2(6, 17);
                        break;
                    case FurnitureOrientation.SouthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoSW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/BancoSW_shadow");
                        TextureShadowPosition = new Vector2(3, 18);
                        break;
                    default:
                        break;
                }
            }

            if (Type == FurnitureType.Farola)
            {
                switch (Orientation)
                {
                    case FurnitureOrientation.NorthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaNE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaNE_shadow");
                        TextureShadowPosition = new Vector2(2, 146);
                        break;
                    case FurnitureOrientation.NorthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaNW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaNW_shadow");
                        TextureShadowPosition = new Vector2(44, 146);
                        break;
                    case FurnitureOrientation.SouthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaSE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaSE_shadow");
                        TextureShadowPosition = new Vector2(2, 122);
                        break;
                    case FurnitureOrientation.SouthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaSW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/FarolaSW_shadow");
                        TextureShadowPosition = new Vector2(44, 122);
                        break;
                    default:
                        break;
                }
            }


            if (Type == FurnitureType.Coche)
            {
                switch (Orientation)
                {
                    case FurnitureOrientation.NorthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheNE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheNE_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    case FurnitureOrientation.NorthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheNW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheNW_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    case FurnitureOrientation.SouthEast:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheSE");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheSE_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    case FurnitureOrientation.SouthWest:
                        Texture = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheSW");
                        TextureShadow = game.Content.Load<Texture2D>(@"Maps/Furniture/CocheSW_shadow");
                        TextureShadowPosition = new Vector2(0, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (TextureAnimation != null)
            {
                TextureAnimation.Update(gameTime);
#if XBOX
                if (this.Type == FurnitureType.CocheArdiendo && SmokeEffect != null)
                {
                    SmokeEffect.Trigger(smokeEffectPosition);
                    float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    SmokeEffect.Update(deltaSeconds);
                }
#endif
            }
        }

        public void Draw(SpriteBatch batch, SpriteFont font)
        {
            //SpriteBatch batch = spriteBatch;
            //batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            if (TextureAnimation != null)
            {
                // Produce animation
                if (this.Orientation == FurnitureOrientation.SouthEast)
                {
                    TextureAnimation.Draw(batch, this.Position, SpriteEffects.None, this.layerIndex, 0f, Color.White);
                }
                else
                {
                    TextureAnimation.Draw(batch, this.Position, SpriteEffects.FlipHorizontally, this.layerIndex, 0f, Color.White);
                }
            }
            else
            {
                if (TextureShadow != null)
                {
                    //batch.Draw(TextureShadow, new Vector2(Position.X + TextureShadowPosition.X, Position.Y + TextureShadowPosition.Y), new Color(0, 0, 0, 60)); //new Color(65, 63, 60, (byte)MathHelper.Clamp(100, 0, 255)));
                    batch.Draw(TextureShadow, new Rectangle(Convert.ToInt32(Position.X + TextureShadowPosition.X), Convert.ToInt32(Position.Y + TextureShadowPosition.Y), TextureShadow.Width, TextureShadow.Height),
                        new Rectangle(0, 0, TextureShadow.Width, TextureShadow.Height), new Color(0, 0, 0, 60), 0.0f, Vector2.Zero, SpriteEffects.None, this.layerIndex + 0.1f);
                }

                batch.Draw(Texture, new Rectangle(Convert.ToInt32(Position.X), Convert.ToInt32(Position.Y), Texture.Width, Texture.Height),
                        new Rectangle(0, 0, Texture.Width, Texture.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, this.layerIndex);

#if DEBUG
                batch.DrawString(font, this.layerIndex.ToString(), Position, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.4f);
#endif
            }
            //batch.End();

            /*
            if (this.Type == FurnitureType.CocheArdiendo)
            {
                // Render Efecto Particulas
                particleRenderer.RenderEffect(SmokeEffect);
            }
             */
        }

    }

    public class CPowerUp
    {
        public Texture2D Texture, UITexture;
        public Vector2 Position;
        public int Value;
        public ObjectStatus status;
        public Type PUType;
        public float buffTimer;

        private float timer;
        private float dyingtimer;
        private Color color;

        public enum Type
        {
            live = 0, 
            extralife = 1, 
            shotgun_ammo = 2, 
            machinegun_ammo = 3, 
            flamethrower_ammo = 4, 
            grenadelauncher_ammo = 5, 
            speedbuff = 6, 
            immunebuff = 7
        }

        public CPowerUp(Texture2D texture, Texture2D uitexture, Vector2 position, Type type)
        {
            this.Texture = texture;
            this.UITexture = uitexture;
            this.Position = position;
            this.status = ObjectStatus.Active;
            this.color = Color.White;
            this.PUType = type;

            if (this.PUType == Type.extralife)
            {
                Value = 1;
            }

            if (this.PUType == Type.live)
            {
                Value = 30;
            }

            if (this.PUType == Type.shotgun_ammo)
            {
                Value = 50;
            }

            if (this.PUType == Type.machinegun_ammo)
            {
                Value = 50;
            }

            if (this.PUType == Type.flamethrower_ammo)
            {
                Value = 50;
            }

            if (this.PUType == Type.grenadelauncher_ammo)
            {
                Value = 10;
            }

            if (this.PUType == Type.speedbuff)
            {
                // Timer
                buffTimer = 20.0f;
            }

            if (this.PUType == Type.immunebuff)
            {
                // Timer
                buffTimer = 20.0f;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this.status == ObjectStatus.Active)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 20.0f)
                {
                    this.status = ObjectStatus.Inactive;
                    timer = 0;
                }
            }

            if (this.status == ObjectStatus.Dying)
            {
                dyingtimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (dyingtimer >= 1.5f)
                {
                    this.status = ObjectStatus.Inactive;
                    dyingtimer = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, SpriteFont font)
        {
            String textToShow;
            Vector2 texturePosition;
            Vector2 startPosition;
            SpriteBatch batch = spriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (this.status == ObjectStatus.Active)
            {
                if (timer >= 10.0f)
                {
                    float interval = 10.0f; // Two second interval
                    float value = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds
                        * interval);
                    value = (value + 1) / 2;  // Shift the sine wave into positive 
                    // territory, then back to 0-1 range
                    this.color = new Color(value, value, value, value);
                }
                else
                {
                    this.color = Color.White;
                }

                batch.Draw(this.Texture, this.Position, this.color);
            }

            if (this.status == ObjectStatus.Dying)
            {
                if (this.PUType == Type.live)
                {
                    this.color = Color.Red;
                }

                if (this.PUType == Type.shotgun_ammo)
                {
                    this.color = Color.LightYellow;
                }

                if (this.PUType == Type.machinegun_ammo)
                {
                    this.color = Color.LightYellow;
                }

                if (this.PUType == Type.flamethrower_ammo)
                {
                    this.color = Color.LightYellow;
                }

                if (this.PUType == Type.grenadelauncher_ammo)
                {
                    this.color = Color.LightYellow;
                }

                if (this.PUType == Type.speedbuff)
                {
                    this.color = new Color(95, 172, 226, 255);
                }

                if (this.PUType == Type.immunebuff)
                {
                    this.color = Color.BlueViolet;
                }

                if (this.PUType == Type.immunebuff || this.PUType == Type.speedbuff)
                {
                    textToShow = "+ " + Convert.ToInt32(this.buffTimer).ToString() + "s";
                }
                else
                {
                    textToShow = "+ " + this.Value.ToString();
                }

                startPosition = new Vector2(this.Position.X - (font.MeasureString(textToShow).X / 2), this.Position.Y);
                texturePosition = new Vector2(startPosition.X + font.MeasureString(textToShow).X + 2, startPosition.Y);

                batch.DrawString(font, textToShow, new Vector2(startPosition.X + 1, startPosition.Y + 1), Color.Black);
                batch.DrawString(font, textToShow, startPosition, color);
                batch.Draw(this.UITexture, texturePosition, Color.White);
            }

            batch.End();
        }
    }
}
