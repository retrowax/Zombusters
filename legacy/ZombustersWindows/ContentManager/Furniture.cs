using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace ZombustersWindows.Subsystem_Managers
{
    public partial class Furniture
    {
        [XmlElement]
        public FurnitureType Type;

        [XmlElement]
        public FurnitureOrientation Orientation;

        [XmlElement]
        public Vector2 Position;

        [XmlElement]
        public Vector2 ObstaclePosition;

        [XmlElement]
        public float ObstacleRadius;

        [XmlIgnore]
        public Texture2D Texture;

        [XmlIgnore]
        private Texture2D TextureShadow;

        [XmlIgnore]
        private Vector2 TextureShadowPosition;

        [XmlIgnore]
        private Animation TextureAnimation;

        [XmlIgnore]
        public float layerIndex;

        public void Load(MyGame game)
        {
            if (Type == FurnitureType.CocheArdiendo)
            {
                // Load multiple animations form XML definition
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(AppContext.BaseDirectory + "/Content/AnimationDef.xml");

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
            }
        }

        public void Draw(SpriteBatch batch, SpriteFont font)
        {
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
                    batch.Draw(TextureShadow, new Rectangle(Convert.ToInt32(Position.X + TextureShadowPosition.X), Convert.ToInt32(Position.Y + TextureShadowPosition.Y), TextureShadow.Width, TextureShadow.Height),
                        new Rectangle(0, 0, TextureShadow.Width, TextureShadow.Height), new Color(0, 0, 0, 60), 0.0f, Vector2.Zero, SpriteEffects.None, this.layerIndex + 0.1f);
                }

                batch.Draw(Texture, new Rectangle(Convert.ToInt32(Position.X), Convert.ToInt32(Position.Y), Texture.Width, Texture.Height),
                        new Rectangle(0, 0, Texture.Width, Texture.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, this.layerIndex);

#if DEBUG
                batch.DrawString(font, this.layerIndex.ToString(), Position, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.4f);
#endif
            }
        }

    }
}
