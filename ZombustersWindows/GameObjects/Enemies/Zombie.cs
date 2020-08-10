using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.GameObjects;

namespace ZombustersWindows
{
    public class Zombie : BaseEnemy
    {
        public float MAX_VELOCITY = 1.5f;
        public const float MAX_STRENGTH = 0.15f;
        private const int ZOMBIE_Y_OFFSET = 70;

        private Vector2 ZombieOrigin;
        public Vector2 BurningZombieOrigin;
        public Vector2 ZombieDeathOrigin;

        public Texture2D ZombieTexture;
        public Texture2D ZombieShadow;
        public Texture2D BurningZombieTexture;
        public Texture2D ZombieDeathTexture;

        Animation ZombieAnimation;
        Animation BurningZombieAnimation;
        Animation ZombieDeathAnimation;

        public Zombie(Vector2 velocidad, Vector2 posicion, float boundingRadius, float life, float speed, ref Random gameRandom)
        {
            this.entity = new SteeringEntity
            {
                Velocity = velocidad,
                Position = posicion,
                BoundingRadius = boundingRadius
            };

            this.random = gameRandom;
            if (this.random.Next(0, 2) == 0)
            {
                speed = 0.0f;
            }
            this.entity.MaxSpeed = MAX_VELOCITY + speed;

            this.status = ObjectStatus.Active;
            this.invert = true;
            this.deathTimeTotalSeconds = 0;
            this.TimeOnScreen = 4.5f;
            this.speed = 0;
            this.angle = 1f;
            this.playerChased = 0;
            this.lifecounter = life;
            this.isLoosingLife = false;
            this.entityYOffset = ZOMBIE_Y_OFFSET;

            this.ZombieOrigin = new Vector2(0, 0);
            behaviors = new SteeringBehaviors(MAX_STRENGTH, CombinationType.prioritized);
        }

        override public void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            // Load multiple animations form XML definition
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");

            //ZOMBIE ANIMATION
            // Get the Zombie animation from the XML definition
            var definition = doc.Root.Element("ZombieDef");
            int rand = random.Next(1, 6);
            ZombieTexture = content.Load<Texture2D>(@"InGame/zombie" + rand.ToString());
            ZombieShadow = content.Load<Texture2D>(@"InGame/character_shadow");
            ZombieOrigin = new Vector2(ZombieTexture.Width / 2, ZombieTexture.Height / 2);

            Point frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);

            Point sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);

            TimeSpan frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));

            // Define a new Animation instance
            ZombieAnimation = new Animation(ZombieTexture, frameSize, sheetSize, frameInterval);
            //END ZOMBIE ANIMATION

            definition = doc.Root.Element("BurningZombieDef");
            BurningZombieTexture = content.Load<Texture2D>(@"InGame/burningzombie"); // + rand.ToString());
            BurningZombieOrigin = new Vector2(BurningZombieTexture.Width / 2, BurningZombieTexture.Height / 2);
            frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            BurningZombieAnimation = new Animation(BurningZombieTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("ZombieDeathDef");
            ZombieDeathTexture = content.Load<Texture2D>(@"InGame/zombiedeath" + rand.ToString());
            ZombieDeathOrigin = new Vector2(ZombieDeathTexture.Width / 2, ZombieDeathTexture.Height / 2);
            frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            ZombieDeathAnimation = new Animation(ZombieDeathTexture, frameSize, sheetSize, frameInterval);
        }

        override public void Update(GameTime gameTime, MyGame game, List<BaseEnemy> enemyList)
        {
            if (this.status != ObjectStatus.Dying)
            {
                ZombieAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.players[this.playerChased].avatar.position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.players[this.playerChased].avatar.entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                foreach (BaseEnemy enemy in enemyList)
                {
                    if (entity.Position != enemy.entity.Position && enemy.status == ObjectStatus.Active)
                    {
                        //calculate the distance between the positions of the entities
                        Vector2 ToEntity = entity.Position - enemy.entity.Position;

                        float DistFromEachOther = ToEntity.Length();

                        //if this distance is smaller than the sum of their radii then this
                        //entity must be moved away in the direction parallel to the
                        //ToEntity vector   
                        float AmountOfOverLap = entity.BoundingRadius + 20.0f - DistFromEachOther;

                        if (AmountOfOverLap >= 0)
                        {
                            //move the entity a distance away equivalent to the amount of overlap.
                            entity.Position = (entity.Position + (ToEntity / DistFromEachOther) * AmountOfOverLap);
                        }
                    }
                }
            }
            else
            {
                BurningZombieAnimation.Update(gameTime);
                ZombieDeathAnimation.Update(gameTime);
            }
        }

        override public void Draw(SpriteBatch batch, float TotalGameSeconds, List<Furniture> furniturelist, GameTime gameTime)
        {
            Color color = new Color();
            float layerIndex = GetLayerIndex(this.entity, furniturelist);

            if (this.status == ObjectStatus.Active)
            {
                if (this.isLoosingLife == true)
                {
                    color = Color.Red;
                }
                else
                {
                    color = Color.White;
                }

                if (this.entity.Velocity.X > 0)
                {
                    ZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                }
                else
                {
                    ZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, color);
                }

                batch.Draw(this.ZombieShadow, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 58 + this.ZombieTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, 
                    new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);

                this.isLoosingLife = false;
            }
            else if (this.status == ObjectStatus.Dying)
            {
                // Produce animation
                if (this.currentgun != GunType.flamethrower)
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer <= 1.2f)
                    {
                        if (this.entity.Velocity.X > 0)
                        {
                            ZombieDeathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            ZombieDeathAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, Color.White);
                        }

                        batch.Draw(this.ZombieShadow, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 58 + this.ZombieTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);
                    }
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer <= 1.4f)
                    {
                        if (this.entity.Velocity.X > 0)
                        {
                            BurningZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            BurningZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, Color.White);
                        }

                        batch.Draw(this.ZombieShadow, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 58 + this.ZombieTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);
                    }
                }
            }

            base.Draw(batch, TotalGameSeconds, furniturelist, gameTime);
        }
    }
}
