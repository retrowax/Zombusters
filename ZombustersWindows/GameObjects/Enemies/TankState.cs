using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    /// <summary>
    /// Objeto que representa cualquier entidad en nuestro juego, en este caso le hemos llamado "Tank"... 
    /// pero obviamente podía ser cualquier cosa
    /// </summary>
    public class TankState
    {
        public const float VELOCIDAD_MAXIMA = 1.0f;
        public const float FUERZA_MAXIMA = 0.15f;

        public SteeringBehaviors behaviors;
        public SteeringEntity entity;

        public float angle;
        public ObjectStatus status;
        public Vector2 position;
        public float deathTimeTotalSeconds;
        public float speed;

        public Texture2D TankTexture, TankShadow;
        private Vector2 TankOrigin;
        private Animation TankAnimation;

        public TankState(Texture2D textura, Vector2 velocidad, Vector2 posicion, float boundingRadius)
        {
            this.TankTexture = textura;

            this.entity = new SteeringEntity();

            this.entity.Width = this.TankTexture.Width;
            this.entity.Height = this.TankTexture.Height;
            this.entity.Velocity = velocidad;
            this.entity.Position = posicion;
            this.entity.BoundingRadius = boundingRadius;
            this.entity.MaxSpeed = VELOCIDAD_MAXIMA;

            this.status = ObjectStatus.Active;
            this.deathTimeTotalSeconds = 0;
            this.position = posicion;
            this.speed = 0;
            this.angle = 1f;

            this.TankTexture = textura;
            this.TankOrigin = new Vector2(0, 0);
            this.TankAnimation = new Animation(TankTexture, new Point(), new Point(), TimeSpan.FromSeconds(1.0f));

            behaviors = new SteeringBehaviors(FUERZA_MAXIMA, CombinationType.prioritized);
        }

        public void LoadContent(ContentManager content)
        {
            // Load multiple animations form XML definition
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");

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

        public void DestroyTank(float totalGameSeconds)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Dying;
        }

        // Destroy the seeker without leaving a powerup
        public void CrashTank(float totalGameSeconds)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Inactive;
        }

        public void Update(GameTime gameTime, MyGame game)
        {
            // Progress the Zombie animation
            bool isProgressed = TankAnimation.Update(gameTime);

            this.behaviors.Pursuit.Target = game.currentPlayers[0].position;
            this.behaviors.Pursuit.UpdateEvaderEntity(game.currentPlayers[0].entity);
            this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
            this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
            this.entity.Position += this.entity.Velocity;
        }

        public bool IsInRange(SteeringEntity entity, Furniture furniture)
        {
            float distance = Vector2.Distance(entity.Position, furniture.ObstaclePosition);
            if (distance < Avatar.CrashRadius + 10.0f)
            {
                return true;
            }

            return false;
        }

        public float GetLayerIndex(SteeringEntity entity, List<Furniture> furniturelist)
        {
            float furnitureBasePosition, playerBasePosition;

            foreach (Furniture furniture in furniturelist)
            {
                playerBasePosition = entity.Position.Y + entity.Height;
                furnitureBasePosition = furniture.Position.Y + furniture.Texture.Height;

                if (IsInRange(entity, furniture))
                {
                    if (playerBasePosition < furnitureBasePosition)
                    {
                        return furniture.layerIndex + 0.01f;
                    }
                    else if (playerBasePosition > furnitureBasePosition)
                    {
                        return furniture.layerIndex - 0.01f;
                    }
                }
                else
                {
                    if (playerBasePosition < furnitureBasePosition && entity.Position.X < furniture.ObstaclePosition.X)
                    {
                        return furniture.layerIndex + 0.01f;
                    }
                }
            }

            return 0.1f;
        }

        public void Draw(SpriteBatch batch, float TotalGameSeconds, SpriteFont font, List<Furniture> furniturelist)
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
            else if (this.status == ObjectStatus.Dying)
            {
                // Draw Score Bonus
                int score = 10;
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X + 1, this.entity.Position.Y + 1), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex + 0.1f);
                    batch.DrawString(font, score.ToString(), this.entity.Position, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                }
            }
        }
    }
}
