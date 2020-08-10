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
    public class Rat : BaseEnemy
    {
        private const int RAT_X_OFFSET = 20;
        private const int RAT_Y_OFFSET = 48;

        public float MAX_VELOCITY = 1.5f;
        public const float MAX_STRENGTH = 0.15f;

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

        public Rat(Vector2 posicion, float boundingRadius, float life, float speed, ref Random gameRandom)
        {
            this.entity = new SteeringEntity
            {
                Velocity = new Vector2(0, 0),
                Position = posicion,
                BoundingRadius = boundingRadius
            };

            this.random = gameRandom;
            this.entity.MaxSpeed = MAX_VELOCITY + speed;

            this.status = ObjectStatus.Active;
            this.invert = true;
            this.deathTimeTotalSeconds = 0;
            this.TimeOnScreen = 4.5f;
            this.speed = speed;
            this.angle = 1f;
            this.playerChased = 0;
            this.lifecounter = life;
            this.isLoosingLife = false;
            this.entityYOffset = RAT_Y_OFFSET;

            behaviors = new SteeringBehaviors(MAX_STRENGTH, CombinationType.prioritized);
        }

        override public void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            LoadTextures(ref content);
            LoadAnimations();
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

        override public void Update(GameTime gameTime, MyGame game, List<BaseEnemy> EnemyList)
        {
            if (this.status != ObjectStatus.Dying)
            {
                runAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.players[this.playerChased].avatar.position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.players[this.playerChased].avatar.entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                foreach (BaseEnemy enemy in EnemyList)
                {
                    if (entity.Position != enemy.entity.Position && enemy.status == ObjectStatus.Active)
                    {
                        Vector2 ToEntity = entity.Position - enemy.entity.Position;

                        float DistFromEachOther = ToEntity.Length();
                        float AmountOfOverLap = entity.BoundingRadius + 20.0f - DistFromEachOther;

                        if (AmountOfOverLap >= 0)
                        {
                            entity.Position = (entity.Position + (ToEntity / DistFromEachOther) * AmountOfOverLap);
                        }
                    }
                }

                if (IsInRange(game.players))
                {
                    isInPlayerRange = true;
                    attackAnimation.Update(gameTime);
                } 
                else
                {
                    isInPlayerRange = false;
                }
            }
            else
            {
                deathAnimation.Update(gameTime);
            }
        }

        override public void Draw(SpriteBatch batch, float TotalGameSeconds, List<Furniture> furniturelist, GameTime gameTime)
        {
            Color color;
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

                if (entity.Velocity.X == 0 && entity.Velocity.Y == 0)
                {
                    idleAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.None, layerIndex, 0f, color);
                }
                else
                {
                    if (isInPlayerRange)
                    {
                        if (entity.Velocity.X > 0)
                        {
                            attackAnimation.Draw(batch, new Vector2(this.entity.Position.X - RAT_X_OFFSET, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.None, layerIndex, 0f, color);
                        }
                        else
                        {
                            attackAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                        }
                    }
                    else
                    {
                        if (entity.Velocity.X > 0)
                        {
                            runAnimation.Draw(batch, new Vector2(this.entity.Position.X - RAT_X_OFFSET, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.None, layerIndex, 0f, color);
                        }
                        else
                        {
                            runAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                        }
                    }
                }

                batch.Draw(this.shadowTexture, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 56 + this.idleTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, 
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
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - RAT_X_OFFSET, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.None, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }

                        batch.Draw(this.shadowTexture, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 56 + this.idleTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);
                    }
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer <= 1.4f)
                    {
                        if (this.entity.Velocity.X > 0)
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - RAT_X_OFFSET, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.None, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - RAT_Y_OFFSET), SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }

                        batch.Draw(this.shadowTexture, new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 56 + this.idleTexture.Height), null, new Color(255, 255, 255, 50), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, layerIndex + 0.01f);
                    }
                }

                int score = 10;
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2 + 1, this.entity.Position.Y - RAT_Y_OFFSET - 1), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2, this.entity.Position.Y - RAT_Y_OFFSET), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex - 0.1f);
                }
            }

            base.Draw(batch, TotalGameSeconds, furniturelist, gameTime);
        }
    }
}
