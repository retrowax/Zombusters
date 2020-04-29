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
    public class ZombieState
    {
        public float MAX_VELOCITY = 1.5f;
        public const float MAX_STRENGTH = 0.15f;

        public SteeringBehaviors behaviors;
        public SteeringEntity entity;

        public ObjectStatus status;
        public float deathTimeTotalSeconds;
        public float TimeOnScreen;
        public bool invert;
        public float speed;
        public float angle;
        public int playerChased;

        public float lifecounter = 0.5f;
        public bool isLoosingLife;

        public Texture2D ZombieTexture, ZombieShadow;
        private Vector2 ZombieOrigin;
        Animation ZombieAnimation;

        public Texture2D BurningZombieTexture;
        public Vector2 BurningZombieOrigin;
        Animation BurningZombieAnimation;

        public Texture2D ZombieDeathTexture;
        public Vector2 ZombieDeathOrigin;
        Animation ZombieDeathAnimation;

        private readonly Random random = new Random();
        private GunType currentgun;
        private float timer;

#if DEBUG
        Texture2D PositionReference;
        SpriteFont DebugFont;
#endif

        public ZombieState(Texture2D textura, Vector2 velocidad, Vector2 posicion, float boundingRadius, float life, float speed)
        {
            this.entity = new SteeringEntity
            {
                Width = textura.Width,
                Height = textura.Height,
                Velocity = velocidad,
                Position = posicion,
                BoundingRadius = boundingRadius
            };
            if (random.Next(0, 2) == 0)
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

            this.ZombieTexture = textura;
            this.ZombieOrigin = new Vector2(0, 0);
            behaviors = new SteeringBehaviors(MAX_STRENGTH, CombinationType.prioritized);
        }

        public void LoadContent(ContentManager content)
        {
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


#if DEBUG
            PositionReference = content.Load<Texture2D>(@"InGame/position_reference_temporal");
            DebugFont = content.Load<SpriteFont>(@"menu/ArialMenuInfo");
#endif
        }

        public void Update(GameTime gameTime, MyGame game, List<ZombieState> zombies)
        {
            if (this.status != ObjectStatus.Dying)
            {
                ZombieAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.avatars[this.playerChased].position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.avatars[this.playerChased].entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                for (int i = 0; i < zombies.Count; i++)
                {
                    SteeringEntity zombieEntity = zombies[i].entity;
                    if (entity.Position != zombieEntity.Position && status == ObjectStatus.Active)
                    {
                        //calculate the distance between the positions of the entities
                        Vector2 ToEntity = entity.Position - zombieEntity.Position;

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

        public void DestroyZombie(float totalGameSeconds, GunType currentgun)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Dying;
            this.currentgun = currentgun;
        }

        // Destroy the seeker without leaving a powerup
        public void CrashZombie(float totalGameSeconds)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Inactive;
        }

        public bool IsInRange(SteeringEntity entity, Furniture furniture)
        {
            float distance = Vector2.Distance(new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 30), furniture.ObstaclePosition);
            if (distance < Avatar.CrashRadius + 10.0f)
            {
                return true;
            }

            return false;
        }

        public float GetLayerIndex(SteeringEntity entity, List<Furniture> furniturelist)
        {
            float furnitureInferior, playerBasePosition, lindex;
            int n = 0;

            playerBasePosition = entity.Position.Y;
            furnitureInferior = 0.0f;
            lindex = 0.0f;


            while (playerBasePosition > furnitureInferior)
            {
                if (n < furniturelist.Count)
                {
                    furnitureInferior = furniturelist[n].Position.Y + furniturelist[n].Texture.Height;
                    lindex = furniturelist[n].layerIndex;
                }
                else
                {
                    return lindex + 0.002f;
                }

                n++;
            }

            return lindex + 0.002f;
        }

        public void Draw(SpriteBatch batch, float TotalGameSeconds, SpriteFont font, List<Furniture> furniturelist, GameTime gameTime)
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

                // Produce animation
                if (this.entity.Velocity.X > 0)
                {
                    ZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                }
                else
                {
                    ZombieAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, color);
                }

#if DEBUG
                // Position Reference TEMPORAL
                //batch.Draw(PositionReference, new Rectangle(Convert.ToInt32(this.entity.Position.X), Convert.ToInt32(this.entity.Position.Y), PositionReference.Width, PositionReference.Height),
                    //new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);


                //batch.DrawString(DebugFont, collision.ToString(), this.entity.Position, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                //collision = false;
#endif

                // Draw Zombie SHADOW!
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
                    }
                }

                // Draw Score Bonus
                int score = 10;
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2 + 1, this.entity.Position.Y - 69), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2, this.entity.Position.Y - 70), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex - 0.1f);
                }
            }
        }
    }
}
