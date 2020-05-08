using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.GameObjects;
using System.Xml.Linq;

namespace ZombustersWindows
{
    public class Rat
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

        private Texture2D attackTexture;
        private Texture2D deathTexture;
        private Texture2D hitTexture;
        private Texture2D idleTexture;
        private Texture2D runTexture;
        private Texture2D shadowTexture;

        Animation attackAnimation;
        Animation deathAnimation;
        Animation hitAnimation;
        Animation idleAnimation;
        Animation runAnimation;

        private readonly Random random = new Random();
        private GunType currentgun;
        private float timer;

#if DEBUG
        Texture2D PositionReference;
        SpriteFont DebugFont;
#endif

        public Rat(Vector2 velocidad, Vector2 posicion, float boundingRadius, float life, float speed)
        {
            this.entity = new SteeringEntity
            {
                Width = idleTexture.Width,
                Height = idleTexture.Height,
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

            behaviors = new SteeringBehaviors(MAX_STRENGTH, CombinationType.prioritized);
        }

        public void LoadContent(ContentManager content)
        {
            LoadTextures(ref content);
            LoadAnimations();
#if DEBUG
            PositionReference = content.Load<Texture2D>(@"InGame/position_reference_temporal");
            DebugFont = content.Load<SpriteFont>(@"menu/ArialMenuInfo");
#endif
        }

        private void LoadTextures(ref ContentManager content)
        {
            attackTexture = content.Load<Texture2D>(@"InGame/rat/48x48Rat_Attack");
            deathTexture = content.Load<Texture2D>(@"InGame/rat/48x48Rat_Death");
            hitTexture = content.Load<Texture2D>(@"InGame/rat/48x48Rat_Hit");
            idleTexture = content.Load<Texture2D>(@"InGame/rat/48x48Rat_Idle");
            runTexture = content.Load<Texture2D>(@"InGame/rat/48x48Rat_Run");
            shadowTexture = content.Load<Texture2D>(@"InGame/character_shadow");
        }

        private void LoadAnimations()
        {
            XElement definition;
            TimeSpan frameInterval;
            Point frameSize = new Point();
            Point sheetSize = new Point();
            
            XDocument doc = XDocument.Load("Content/InGame/rat/RatAnimationDef.xml");

            definition = doc.Root.Element("RatAttackDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            attackAnimation = new Animation(attackTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("RatDeathDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            deathAnimation = new Animation(deathTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("RatHitDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            hitAnimation = new Animation(hitTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("RatIdleDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            idleAnimation = new Animation(idleTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("RatRunDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            runAnimation = new Animation(runTexture, frameSize, sheetSize, frameInterval);
        }

        public void Update(GameTime gameTime, MyGame game, List<Rat> rats)
        {
            if (this.status != ObjectStatus.Dying)
            {
                runAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.players[this.playerChased].avatar.position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.players[this.playerChased].avatar.entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                for (int i = 0; i < rats.Count; i++)
                {
                    SteeringEntity zombieEntity = rats[i].entity;
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
                deathAnimation.Update(gameTime);
            }
        }

        public void Destroy(float totalGameSeconds, GunType currentgun)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Dying;
            this.currentgun = currentgun;
        }

        // Destroy the seeker without leaving a powerup
        public void Crash(float totalGameSeconds)
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

                if (this.entity.Velocity.X > 0)
                {
                    runAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                }
                else
                {
                    idleAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, color);
                }

                batch.Draw(this.shadowTexture, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 58 + this.idleTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, 
                    new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);

                this.isLoosingLife = false;
            }
            else if (this.status == ObjectStatus.Dying)
            {
                if (this.currentgun != GunType.flamethrower)
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer <= 1.2f)
                    {
                        if (this.entity.Velocity.X > 0)
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, Color.White);
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
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - 50), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - 21, this.entity.Position.Y - 50), SpriteEffects.None, layerIndex, 0f, Color.White);
                        }
                    }
                }

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
