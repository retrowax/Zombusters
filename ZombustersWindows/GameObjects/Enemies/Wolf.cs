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
    public class Wolf
    {
        private const int WOLF_X_OFFSET = 20;
        private const int WOLF_Y_OFFSET = 48;
        private const float WOLF_SCALE = 1.1f;

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

        private SpriteFont font;

        Animation attackAnimation;
        Animation deathAnimation;
        Animation hitAnimation;
        Animation idleAnimation;
        Animation runAnimation;

        private readonly Random random = new Random();
        private GunType currentgun;
        private float timer;
        private bool isInPlayerRange;

#if DEBUG
        Texture2D PositionReference;
        SpriteFont DebugFont;
#endif

        public Wolf(Vector2 posicion, float boundingRadius, float life)
        {
            this.entity = new SteeringEntity
            {
                Velocity = new Vector2(0, 0),
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
            attackTexture = content.Load<Texture2D>(@"InGame/wolf/80x48Wolf_JumpAttackMove");
            deathTexture = content.Load<Texture2D>(@"InGame/wolf/80x48Wolf_Death");
            hitTexture = content.Load<Texture2D>(@"InGame/wolf/80x48Wolf_hit");
            idleTexture = content.Load<Texture2D>(@"InGame/wolf/80x48Wolf_Idle");
            runTexture = content.Load<Texture2D>(@"InGame/wolf/80x48Wolf_Run");
            shadowTexture = content.Load<Texture2D>(@"InGame/character_shadow");

            font = content.Load<SpriteFont>(@"menu\ArialMenuInfo");
        }

        private void LoadAnimations()
        {
            XElement definition;
            TimeSpan frameInterval;
            Point frameSize = new Point();
            Point sheetSize = new Point();
            
            XDocument doc = XDocument.Load("Content/InGame/wolf/WolfAnimationDef.xml");

            definition = doc.Root.Element("WolfJumpAttackMoveDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            attackAnimation = new Animation(attackTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("WolfDeathDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            deathAnimation = new Animation(deathTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("WolfHitDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            hitAnimation = new Animation(hitTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("WolfIdleDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            idleAnimation = new Animation(idleTexture, frameSize, sheetSize, frameInterval);

            definition = doc.Root.Element("WolfRunDef");
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value, NumberStyles.Integer);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value, NumberStyles.Integer);
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value, NumberStyles.Integer);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value, NumberStyles.Integer);
            frameInterval = TimeSpan.FromSeconds(1.0f / int.Parse(definition.Attribute("Speed").Value, NumberStyles.Integer));
            runAnimation = new Animation(runTexture, frameSize, sheetSize, frameInterval);
        }

        public void Update(GameTime gameTime, MyGame game, List<Wolf> wolfs)
        {
            if (this.status != ObjectStatus.Dying)
            {
                runAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.players[this.playerChased].avatar.position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.players[this.playerChased].avatar.entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                foreach (Wolf wolf in wolfs)
                {
                    if (entity.Position != wolf.entity.Position && wolf.status == ObjectStatus.Active)
                    {
                        Vector2 ToEntity = entity.Position - wolf.entity.Position;
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

        private bool IsInRange(Player[] players)
        {
            foreach (Player player in players)
            {
                float distance = Vector2.Distance(entity.Position, player.avatar.position);
                if (distance < Avatar.CrashRadius + 20.0f)
                {
                    return true;
                }
            }
            return false;
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

        public void Draw(SpriteBatch batch, float TotalGameSeconds, List<Furniture> furniturelist, GameTime gameTime)
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
                    idleAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.None, layerIndex, 0f, color);
                }
                else
                {
                    if (isInPlayerRange)
                    {
                        if (entity.Velocity.X > 0)
                        {
                            attackAnimation.Draw(batch, new Vector2(this.entity.Position.X - WOLF_X_OFFSET, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.None, layerIndex, 0f, color);
                        }
                        else
                        {
                            attackAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
                        }
                    }
                    else
                    {
                        if (entity.Velocity.X > 0)
                        {
                            runAnimation.Draw(batch, new Vector2(this.entity.Position.X - WOLF_X_OFFSET, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.None, layerIndex, 0f, color);
                        }
                        else
                        {
                            runAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.FlipHorizontally, layerIndex, 0f, color);
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
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - WOLF_X_OFFSET, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.None, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
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
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X - WOLF_X_OFFSET, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.None, layerIndex, 0f, Color.White);
                        }
                        else
                        {
                            deathAnimation.Draw(batch, new Vector2(this.entity.Position.X, this.entity.Position.Y - WOLF_Y_OFFSET), WOLF_SCALE, SpriteEffects.FlipHorizontally, layerIndex, 0f, Color.White);
                        }
                    }
                }

                int score = 10;
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2 + 1, this.entity.Position.Y - WOLF_Y_OFFSET - 1), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2, this.entity.Position.Y - WOLF_Y_OFFSET), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex - 0.1f);
                }
            }
        }
    }
}
