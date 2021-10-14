using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class Tank : BaseEnemy
    {
        public const float VELOCIDAD_MAXIMA = 1.0f;
        public const float FUERZA_MAXIMA = 0.15f;

        public Texture2D TankTexture, TankShadow;
        private Vector2 TankOrigin;
        private Animation TankAnimation;

        public Tank(Vector2 velocidad, Vector2 posicion, float boundingRadius)
        {
            this.entity = new SteeringEntity
            {
                Width = this.TankTexture.Width,
                Height = this.TankTexture.Height,
                Velocity = velocidad,
                Position = posicion,
                BoundingRadius = boundingRadius,
                MaxSpeed = VELOCIDAD_MAXIMA
            };

            this.status = ObjectStatus.Active;
            this.deathTimeTotalSeconds = 0;
            this.speed = 0;
            this.angle = 1f;

            this.TankOrigin = new Vector2(0, 0);
            this.TankAnimation = new Animation(TankTexture, new Point(), new Point(), TimeSpan.FromSeconds(1.0f));

            behaviors = new SteeringBehaviors(FUERZA_MAXIMA, CombinationType.prioritized);
        }

        override public void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            // Load multiple animations form XML definition
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(AppContext.BaseDirectory + "/Content/AnimationDef.xml");

            //ZOMBIE ANIMATION
            // Get the Zombie animation from the XML definition
            var definition = doc.Root.Element("ZombieDef");

            TankTexture = content.Load<Texture2D>(@"InGame/tank");
            TankShadow = content.Load<Texture2D>(@"InGame/character_shadow");
            TankOrigin = new Vector2(TankTexture.Width / 2, TankTexture.Height / 2);

            Point frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);

            Point sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);

            TimeSpan frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));

            // Define a new Animation instance
            TankAnimation = new Animation(TankTexture, frameSize, sheetSize, frameInterval);
            //END ZOMBIE ANIMATION
        }

        override public void Update(GameTime gameTime, MyGame game, List<BaseEnemy> enemyList)
        {
            // Progress the Zombie animation
            bool isProgressed = TankAnimation.Update(gameTime);

            this.behaviors.Pursuit.Target = game.players[0].avatar.position;
            this.behaviors.Pursuit.UpdateEvaderEntity(game.players[0].avatar.entity);
            this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
            this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
            this.entity.Position += this.entity.Velocity;
        }

        override public void Draw(SpriteBatch batch, float TotalGameSeconds, List<Furniture> furniturelist, GameTime gameTime)
        {
            float layerIndex = GetLayerIndex(this.entity, furniturelist);

            if (this.status == ObjectStatus.Active)
            {
                // Produce animation
                if (this.entity.Velocity.X > 0)
                {
                    batch.Draw(this.TankTexture, this.entity.Position, null, Color.White, 0.0f, new Vector2(this.TankTexture.Width / 2.0f, this.TankTexture.Width / 2.0f), 1.0f, SpriteEffects.None, 0);
                }
                else
                {
                    batch.Draw(this.TankTexture, this.entity.Position, null, Color.White, 0.0f, new Vector2(this.TankTexture.Width / 2.0f, this.TankTexture.Width / 2.0f), 1.0f, SpriteEffects.FlipHorizontally, 0);
                }

                // Draw Tank SHADOW!
                batch.Draw(this.TankShadow, new Vector2(this.entity.Position.X + 5, this.entity.Position.Y + this.TankTexture.Height), null, 
                    new Color(255, 255, 255, 50), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.1f);

            }
        }
    }
}
