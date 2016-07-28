#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.Globalization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public enum EnemyType
    {
        Basic,
        Looper,
        Seeker,
        Zombie,
        Boomer,
        Hunter,
        Tank,
        Smoker,
        Witch,
        Flamer,
        Charger,
        Spitter,
        Jockey
    }

/*
    public struct ZombieState
    {
        public ZombieState(float totalGameSeconds, EnemyType type)
        {
            this.status = ObjectStatus.Active;
            this.startTimeTotalSeconds = totalGameSeconds;
            this.type = type;
            this.invert = true;
            this.deathTimeTotalSeconds = 0;
            this.TimeOnScreen = 4.5f;
            this.position = new Vector2(0, 0);
            this.speed = 0;
            this.angle = 1f;
        }
        public ObjectStatus status;
        public float startTimeTotalSeconds;
        public float deathTimeTotalSeconds;
        public float TimeOnScreen;
        public bool invert;
        public EnemyType type;
        public Vector2 position;
        public float speed;
        public float angle;
    }


    public struct TankState
    {
        public float angle;
        public ObjectStatus status;
        public Vector2 position;
        public float deathTimeTotalSeconds;
        public float speed;

    }
*/

    public class ZombieState
    {
        public float VELOCIDAD_MAXIMA = 1.5f;
        public const float FUERZA_MAXIMA = 0.15f;

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

        private int currentgun;

        private float timer;

#if DEBUG
        Texture2D PositionReference;
        SpriteFont DebugFont;
#endif

        private Random random;

        public ZombieState(Texture2D textura, Vector2 velocidad, Vector2 posicion, float boundingRadius, float life, float speed)
        {
            random = new Random();

            this.entity = new SteeringEntity();
            this.entity.Width = textura.Width;
            this.entity.Height = textura.Height;
            this.entity.Velocity = velocidad;
            this.entity.Position = posicion;
            this.entity.BoundingRadius = boundingRadius;
            if (random.Next(0, 2) == 0)
            {
                speed = 0.0f;
            }
            this.entity.MaxSpeed = VELOCIDAD_MAXIMA + speed;

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
            //this.ZombieAnimation = new Animation(ZombieTexture, new Point(), new Point(), TimeSpan.FromSeconds(1.0f));

            behaviors = new SteeringBehaviors(FUERZA_MAXIMA, CombinationType.prioritized);
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

        public void Update(GameTime gameTime, MyGame game)
        {
            if (this.status != ObjectStatus.Dying)
            {
                ZombieAnimation.Update(gameTime);
                this.behaviors.Pursuit.Target = game.currentPlayers[this.playerChased].position;
                this.behaviors.Pursuit.UpdateEvaderEntity(game.currentPlayers[this.playerChased].entity);
                this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
                this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);
                this.entity.Position += this.entity.Velocity;

                //for (int i = 0; i < Zombies.Count; i++)
                //{
                    if (entity.Position != this.entity.Position && status == ObjectStatus.Active)
                    {
                        //calculate the distance between the positions of the entities
                        Vector2 ToEntity = entity.Position - entity.Position;

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
                //}
            }
            else
            {
                BurningZombieAnimation.Update(gameTime);
                ZombieDeathAnimation.Update(gameTime);
            }
        }

/*
        private void UpdateZombie(Enemies enemies, float TotalGameSeconds, float ElapsedGameSeconds,
            ZombieState zombie, byte index, Avatar[] players)
        {
            float zombieTurnRate = MathHelper.PiOver2;
            Random rand = new Random(2);
            switch (zombie.status)
            {
                case ObjectStatus.Inactive:
                    break;
                case ObjectStatus.Active:
                    float distance = zombie.speed * (float)ElapsedGameSeconds;
                    float currentangle = zombie.angle;

                    // move forward
                    Vector2 forward = VectorFromAngle(currentangle);
                    forward.Normalize();
                    zombie.entity.Position += forward * distance;

                    // find player with highest score
                    //REVISAR!!
                    //int leader = (players[0].score > players[1].score) ? 0 : 1;
                    int leader = rand.Next(0, 1);

                    // turn toward player                                        
                    float desiredangle = AngleFromAxis(forward,
                        players[leader].position - zombie.entity.Position);
                    float maxTurnRate = zombieTurnRate * ElapsedGameSeconds;

                    // get the closest to the desired angle I can given my turn rate
                    zombie.angle = MathHelper.Clamp(currentangle + desiredangle,
                        currentangle - maxTurnRate,
                        currentangle + maxTurnRate);

                    zombie.behaviors.Pursuit.Target = players[leader].position;
                    zombie.behaviors.Pursuit.UpdateEvaderEntity(players[leader].entity);
                    zombie.entity.Velocity += zombie.behaviors.Update(new GameTime(), zombie.entity);
                    zombie.entity.Velocity = VectorHelper.TruncateVector(zombie.entity.Velocity, zombie.entity.MaxSpeed / 1.5f);
                    zombie.entity.Position += zombie.entity.Velocity;

                    Game.ZombieMoved(enemies, index, zombie.entity.Position, zombie.angle);
                    break;
                case ObjectStatus.Dying:
                    // Move the powerup toward the bottom of the screen
                    //zombie.entity.Position.Y += ElapsedGameSeconds *
                    //Game1.BackgroundDriftRatePerSec;

                    // Check to see if the powerup is expired yet.
                    if (TotalGameSeconds > zombie.deathTimeTotalSeconds +
                        Enemies.ZombieInfo.TimeToDie)
                    {
                        //seeker.status = ObjectStatus.Inactive;
                        Game.ZombieCrashed(enemies, index);
                    }
                    Game.ZombieMoved(enemies, index, zombie.entity.Position, zombie.angle);
                    break;
                default:
                    break;
            }
        }
 */

        public void DestroyZombie(float totalGameSeconds, byte currentgun)
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

        public bool isInRange(SteeringEntity entity, CFurniture furniture)
        {
            /*
            uint Xdistance = Convert.ToUInt32(Math.Abs(state.position.X - furniture.Position.X));
            uint Ydistance = Convert.ToUInt32(Math.Abs(state.position.Y - furniture.Position.Y));

            if (Xdistance <= 15 && Ydistance <= 15)
            {
                return true;
            }

            return false;
             */

            float distance = Vector2.Distance(new Vector2(this.entity.Position.X - 10, this.entity.Position.Y - 30), furniture.ObstaclePosition);
            if (distance < Avatar.CrashRadius + 10.0f)
            {
                return true;
            }

            return false;
            //return (distance < Avatar.CrashRadius + 20.0f);// + enemyType.CrashRadius);
        }

        public float GetLayerIndex(SteeringEntity entity, List<CFurniture> furniturelist)
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

        public void Draw(SpriteBatch batch, float TotalGameSeconds, SpriteFont font, List<CFurniture> furniturelist, GameTime gameTime)
        {
            Color color = new Color();
            //SpriteBatch batch = spriteBatch;
            //batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

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
                // Draw the powerup
                //batch.Draw(TZombie, zombie.position, null, Color.White,
                //0, TZombieOrigin, 0.5f, SpriteEffects.None, .6f);

                // Produce animation
                if (this.currentgun == 0)
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

                // Draw the explosion
                //Explosion.Draw(batch, this.entity.Position, this.deathTimeTotalSeconds, TotalGameSeconds, this.deathTimeTotalSeconds, this.deathTimeTotalSeconds + .5);
            }

            //batch.End();
        }
    }

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

        public bool isInRange(SteeringEntity entity, CFurniture furniture)
        {
            /*
            uint Xdistance = Convert.ToUInt32(Math.Abs(state.position.X - furniture.Position.X));
            uint Ydistance = Convert.ToUInt32(Math.Abs(state.position.Y - furniture.Position.Y));

            if (Xdistance <= 15 && Ydistance <= 15)
            {
                return true;
            }

            return false;
             */

            float distance = Vector2.Distance(entity.Position, furniture.ObstaclePosition);
            if (distance < Avatar.CrashRadius + 10.0f)
            {
                return true;
            }

            return false;
            //return (distance < Avatar.CrashRadius + 20.0f);// + enemyType.CrashRadius);
        }

        public float GetLayerIndex(SteeringEntity entity, List<CFurniture> furniturelist)
        {
            float furnitureBasePosition, playerBasePosition;

            foreach (CFurniture furniture in furniturelist)
            {
                playerBasePosition = entity.Position.Y + entity.Height;
                furnitureBasePosition = furniture.Position.Y + furniture.Texture.Height;

                if (isInRange(entity, furniture))
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

                //if (playerBasePosition < furnitureBasePosition && state.position.X < furniture.ObstaclePosition.X)
                //{
                //    return furniture.layerIndex - 0.01f;
                //}
            }

            return 0.1f;
        }

        public void Draw(SpriteBatch batch, float TotalGameSeconds, SpriteFont font, List<CFurniture> furniturelist)
        {
            //SpriteBatch batch = spriteBatch;
            //batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            float layerIndex = GetLayerIndex(this.entity, furniturelist);

            if (this.status == ObjectStatus.Active)
            {
                // Produce animation
                //TankAnimation.Draw(batch, this.entity.Position, SpriteEffects.None);
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
                // Draw the powerup
                //batch.Draw(TZombie, zombie.position, null, Color.White,
                //0, TZombieOrigin, 0.5f, SpriteEffects.None, .6f);

                // Draw Score Bonus
                int score = 10;
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X + 1, this.entity.Position.Y + 1), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex + 0.1f);
                    batch.DrawString(font, score.ToString(), this.entity.Position, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                }

                // Draw the explosion
                //Explosion.Draw(batch, this.entity.Position, this.deathTimeTotalSeconds, TotalGameSeconds, this.deathTimeTotalSeconds, this.deathTimeTotalSeconds + .5);
            }

            //batch.End();
        }
    }

    public class Enemies
    {
        /*
        public EnemyState[] waves;
        public SeekerState[] seekers;
        public SeekerState[] zombies;
        public static EnemyInfo BasicInfo;
        public static EnemyInfo LooperInfo;
        public static EnemyInfo SeekerInfo;
        public static EnemyInfo ZombieInfo;
        public Random random;
        */

        // Zombies
        public ZombieState[] zombies;
        //public static EnemyInfo ZombieInfo;

        // Tanks
        public TankState[] tanks;
        //public static EnemyInfo TankInfo;

        public Random random;

        public float RadiansPerSecond = MathHelper.Pi * 4;
        public int ActiveEnemies;

        public Enemies()
        {
            //waves = new EnemyState[20];
            //seekers = new SeekerState[4];

            zombies = new ZombieState[20];
            tanks = new TankState[1];

            random = new Random();
        }

        public static void Initialize(Viewport view, ContentManager content)
        {
            /*
            BasicInfo.ScreenBounds = new Rectangle(view.X, view.Y, view.Width, 
                view.Height);
            BasicInfo.TimeToDie = 1.5f;   // 1 1/2 secs
            BasicInfo.CrashRadius = 20f;

            LooperInfo = BasicInfo;

            SeekerInfo = BasicInfo;
            SeekerInfo.TimeToDie = 10.0f;

            BasicInfo.XCurve = content.Load<Curve>("XCurve1");
            BasicInfo.YCurve = content.Load<Curve>("YCurve1");

            LooperInfo.XCurve = content.Load<Curve>("XCurve2");
            LooperInfo.YCurve = content.Load<Curve>("YCurve2");

            ZombieInfo = BasicInfo;
            ZombieInfo.TimeToDie = 10.0f;
            

            ZombieInfo.ScreenBounds = new Rectangle(view.X, view.Y, view.Width, view.Height);
            ZombieInfo.CrashRadius = 20f;
            ZombieInfo.TimeToDie = 10.0f;

            TankInfo.ScreenBounds = new Rectangle(view.X, view.Y, view.Width, view.Height);
            TankInfo.CrashRadius = 20f;
            TankInfo.TimeToDie = 10.0f;
            */
        }

        public void Reset()
        {
            /*
            for (int i = 0; i < waves.Length; i++)
            {
                waves[i].startTimeTotalSeconds = 0;
                waves[i].deathTimeTotalSeconds = 0;
                waves[i].status = ObjectStatus.Inactive;
                waves[i].TimeOnScreen = 0;
                waves[i].type = EnemyType.Looper;                
            }
            for (int j = 0; j < seekers.Length; j++)
            {
                seekers[j].deathTimeTotalSeconds = 0;
                seekers[j].position = Vector2.One * -40;
                seekers[j].speed = 0;
                seekers[j].status = ObjectStatus.Inactive;
            }

            for (int j = 0; j < zombies.Length; j++)
            {
                zombies[j].deathTimeTotalSeconds = 0;
                zombies[j].position = Vector2.One * -40;
                zombies[j].speed = 0;
                zombies[j].status = ObjectStatus.Inactive;
            }
             */
/*
            for (int j = 0; j < zombies.Length; j++)
            {
                zombies[j] = new ZombieState();
                zombies[j].deathTimeTotalSeconds = 0;
                zombies[j].entity = new SteeringEntity();
                zombies[j].entity.Position = Vector2.One * -40;
                zombies[j].speed = 0;
                zombies[j].status = ObjectStatus.Inactive;
            }
*/
            for (int j = 0; j < tanks.Length; j++)
            {
                tanks[j].deathTimeTotalSeconds = 0;
                tanks[j].position = Vector2.One * -40;
                tanks[j].speed = 0;
                tanks[j].status = ObjectStatus.Inactive;
            }
        }
/*
        private void SpawnBasicWave(float totalGameSeconds)
        {
            EnemyState spawn;

            spawn.status = ObjectStatus.Active;
            spawn.startTimeTotalSeconds = totalGameSeconds;
            spawn.deathTimeTotalSeconds = 0;
            spawn.type = EnemyType.Basic;
            spawn.invert = false;


            float BasicBaseTime = 4.5f; // 4 1/2 secs
            float MinimumTimeOnScreen = 1.0f;
            // every minute, we speed up the wave
            spawn.TimeOnScreen = BasicBaseTime - (totalGameSeconds / 60) * 0.6f;
            if (spawn.TimeOnScreen < MinimumTimeOnScreen)
                spawn.TimeOnScreen = MinimumTimeOnScreen;

            for (int i = 0; i < waves.Length; i++)
            {
                waves[i] = spawn;
                spawn.invert = !spawn.invert;
                spawn.startTimeTotalSeconds += 0.1f; // one-tenth of a second
            }
        }

        public void SpawnNextWave(float totalGameSeconds)
        {
            if (waves[0].type == EnemyType.Basic)
                SpawnLooperWave(totalGameSeconds);
            else
                SpawnBasicWave(totalGameSeconds);
        }

        private void SpawnLooperWave(float totalGameSeconds)
        {
            EnemyState spawn;

            spawn.status = ObjectStatus.Active;
            spawn.startTimeTotalSeconds = totalGameSeconds;
            spawn.deathTimeTotalSeconds = 0;
            spawn.type = EnemyType.Looper;
            spawn.invert = false;

            float LooperBaseTime = 6.0f; // 6 secs
            float MinimumTimeOnScreen = 1.0f;
            // every minute, we speed up the wave
            spawn.TimeOnScreen = LooperBaseTime - (totalGameSeconds / 60) * 0.6f;
            if (spawn.TimeOnScreen < MinimumTimeOnScreen)
                spawn.TimeOnScreen = MinimumTimeOnScreen;

            for (int i = 0; i < waves.Length; i++)
            {
                waves[i] = spawn;
                spawn.invert = !spawn.invert;
                spawn.startTimeTotalSeconds += 0.07f; // 7 one-hundreths of a second
            }
        }

        public void SpawnSeeker()
        {
            SeekerState spawn;
            spawn.position = Vector2.One * 100;
            spawn.status = ObjectStatus.Active;
            spawn.speed = 75.0f;
            spawn.angle = MathHelper.PiOver2;
            spawn.deathTimeTotalSeconds = 0;

            for (int i = 0; i < seekers.Length; i++)
            {
                if (seekers[i].status == ObjectStatus.Inactive)
                {
                    seekers[i] = spawn;
                    break;
                }
            }        
        }
*/

        public void SpawnZombie()
        {
            /*
            ZombieState spawn = new ZombieState();
            float RandomX;
            float RandomY;
            //spawn.position = Vector2.One * 100;
            spawn.status = ObjectStatus.Active;
            spawn.speed = 35.0f;
            spawn.angle = MathHelper.PiOver2;
            spawn.deathTimeTotalSeconds = 0;

            for (int i = 0; i < zombies.Length; i++)
            {
                if (zombies[i].status == ObjectStatus.Inactive)
                {
                    RandomX = random.Next(10,1270);
                    RandomY = random.Next(10, 710);
                    spawn.entity = new SteeringEntity();
                    spawn.entity.Position = new Vector2(RandomX, RandomY);
                    zombies[i] = spawn;
                    break;
                }
            }*/
        }

        public void SpawnTank()
        {
/*
            TankState spawn = new TankState();
            float RandomX;
            float RandomY;
            //spawn.position = Vector2.One * 100;
            spawn.status = ObjectStatus.Active;
            spawn.speed = 75.0f;
            spawn.angle = MathHelper.PiOver2;
            spawn.deathTimeTotalSeconds = 0;

            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i].status == ObjectStatus.Inactive)
                {
                    RandomX = random.Next(10, 1270);
                    RandomY = random.Next(10, 710);
                    spawn.position = new Vector2(RandomX, RandomY);
                    tanks[i] = spawn;
                    break;
                }
            }
 */
        }

        public void Update(float totalGameSeconds)
        {
            ActiveEnemies = 0;
            /*
            for (int i = 0; i < waves.Length; i++)
            {
                UpdateBasic(totalGameSeconds, ref waves[i]);
                //if (waves[i].status == ObjectStatus.Active)
                if (waves[i].status != ObjectStatus.Inactive)
                    ActiveEnemies++;
            } 
             */

            for (int i = 0; i < zombies.Length; i++)
            {
                UpdateZombies(totalGameSeconds, ref zombies[i]);
                if (zombies[i].status != ObjectStatus.Inactive)
                    ActiveEnemies++;
            } 
        }

        private static void UpdateZombies(float totalGameSeconds, ref ZombieState enemy)
        {
            switch (enemy.status)
            {
                case ObjectStatus.Inactive:
                    break;
                case ObjectStatus.Active:
                    // Reset enemies that went offscreen
                    /*
                    if ((enemy.startTimeTotalSeconds + enemy.TimeOnScreen) <
                        totalGameSeconds)
                    {
                        enemy.startTimeTotalSeconds = totalGameSeconds;
                    }*/
                    break;
                case ObjectStatus.Dying:
                    /*
                    if ((enemy.deathTimeTotalSeconds + ZombieInfo.TimeToDie) <
                        totalGameSeconds)
                    {
                        enemy.status = ObjectStatus.Inactive;
                    }*/
                    break;
                default:
                    break;
            }

        }


/*
        private static void UpdateBasic(float totalGameSeconds, ref EnemyState enemy)
        {
            switch (enemy.status)
            {
                case ObjectStatus.Inactive:
                    break;
                case ObjectStatus.Active:
                    // Reset enemies that went offscreen
                    if ((enemy.startTimeTotalSeconds + enemy.TimeOnScreen) <
                        totalGameSeconds)
                    {
                        enemy.startTimeTotalSeconds = totalGameSeconds;
                    }
                    break;
                case ObjectStatus.Dying:
                    if ((enemy.deathTimeTotalSeconds + ZombieInfo.TimeToDie) <
                        totalGameSeconds)
                    {
                        enemy.status = ObjectStatus.Inactive;
                    }
                    break;
                default:
                    break;
            }

        }

        public void DestroyEnemy(float totalGameSeconds, byte index)
        {
            waves[index].deathTimeTotalSeconds = totalGameSeconds;
            waves[index].status = ObjectStatus.Dying;
        }

        public void DestroySeeker(float totalGameSeconds, byte index)
        {
            seekers[index].deathTimeTotalSeconds = totalGameSeconds;
            seekers[index].status = ObjectStatus.Dying;
        }

        // Destroy the seeker without leaving a powerup
        public void CrashSeeker(float totalGameSeconds, byte index)
        {
            seekers[index].deathTimeTotalSeconds = totalGameSeconds;
            seekers[index].status = ObjectStatus.Inactive;
        }
*/
        public void DestroyZombie(float totalGameSeconds, byte index)
        {
            zombies[index].deathTimeTotalSeconds = totalGameSeconds;
            zombies[index].status = ObjectStatus.Dying;
        }

        // Destroy the seeker without leaving a powerup
        public void CrashZombie(float totalGameSeconds, byte index)
        {
            zombies[index].deathTimeTotalSeconds = totalGameSeconds;
            zombies[index].status = ObjectStatus.Inactive;
        }

        // Destroy the seeker without leaving a powerup
        public void CrashTank(float totalGameSeconds, byte index)
        {
            tanks[index].deathTimeTotalSeconds = totalGameSeconds;
            tanks[index].status = ObjectStatus.Inactive;
        }
/*
        public static EnemyInfo GetInfoForType(EnemyType type)
        {
            switch (type)
            {

                case EnemyType.Basic:
                    return BasicInfo;
                case EnemyType.Looper:
                    return LooperInfo;
                case EnemyType.Seeker:
                    return SeekerInfo;

                case EnemyType.Zombie:
                    return ZombieInfo;
                case EnemyType.Tank:
                    return TankInfo;
            }
            return ZombieInfo;
        }

        public static Vector2 GetPosition(float totalGameSeconds, ZombieState enemy)
        {
            float elapsed;
            if (enemy.deathTimeTotalSeconds == 0)
                elapsed = (totalGameSeconds - enemy.startTimeTotalSeconds);
            else
                elapsed = (enemy.deathTimeTotalSeconds - enemy.startTimeTotalSeconds);

            Vector2 pos;

            switch (enemy.type)
            {

                case EnemyType.Basic:
                    pos = GetPosition(elapsed / enemy.TimeOnScreen, BasicInfo);
                    if (enemy.invert)
                    {

                        pos.X = BasicInfo.ScreenBounds.Width - pos.X;
                    }
                    return pos;
                case EnemyType.Looper:
                    pos = GetPosition(elapsed / enemy.TimeOnScreen, LooperInfo);
                    if (enemy.invert)
                    {
                        pos.X = LooperInfo.ScreenBounds.Width - pos.X;
                    }
                    return pos;
                case EnemyType.Seeker:
                    break;

                case EnemyType.Zombie:
                    break;
                case EnemyType.Tank:
                    break;
                default:
                    break;
            }
            return Vector2.Zero;
        }

        public static Vector2 GetPosition(float position, EnemyInfo info)
        {
            Vector2 pos = Vector2.Zero;
            pos.Y = info.YCurve.Evaluate(position) * info.ScreenBounds.Height;
            pos.X = info.XCurve.Evaluate(position) * info.ScreenBounds.Width;
            return pos;
        }
*/


    }
}
