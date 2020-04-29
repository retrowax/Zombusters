using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.GameObjects;

namespace ZombustersWindows
{
    public class Avatar
    {
        public Vector2 position;
        public List<Vector4> bullets;
        public List<ShotgunShell> shotgunbullets;
        public ObjectStatus status;
        public int score;
        public int lives;
        public List<int> ammo;
        public int lifecounter;
        public float deathTimeTotalSeconds;
        public Color color;
        public byte character;
        public float shotAngle;
        public GunType currentgun;
        public Vector2 accumMove;
        public Vector2 accumFire;

        public Player Player;
        public bool isReady;
        public bool IsPlaying
        {
            get { return status != ObjectStatus.Inactive; }
        }

        public SteeringEntity entity;
        public SteeringBehaviors behaviors;
        public const float VELOCIDAD_MAXIMA = 1.0f;
        public const float FUERZA_MAXIMA = 0.15f;
        public Vector2 ObjectAvoidCalc = Vector2.Zero;

        public static float CrashRadius = 20f;
        public static float RespawnTime = 2.0f;
        public static float ImmuneTime = 8.0f;
        public static float PixelsPerSecond = 200;
        public static int bulletmax = 20;
        public static int pelletmax = 50;
        
        public Rectangle ScreenBounds;
        
        public int RateOfFire = 2; // per second
        private float LastShot;

        public Vector2 FlameThrowerPosition;
        public float FlameThrowerAngle;
        public RotatedRectangle FlameThrowerRectangle;

        public bool isLoosingLife;

        public Avatar(Color color)             
        {
            bullets = new List<Vector4>(bulletmax);
            shotgunbullets = new List<ShotgunShell>(pelletmax);
            this.color = color;
        }
      
               
        public void Initialize(Viewport view)
        {
            this.entity = new SteeringEntity
            {
                Width = 28,
                Height = 50,
                Velocity = Vector2.Zero,
                Position = Vector2.Zero,
                BoundingRadius = 10.0f,
                MaxSpeed = VELOCIDAD_MAXIMA
            };
            this.behaviors = new SteeringBehaviors(0.15f, CombinationType.prioritized);

            ScreenBounds = new Rectangle(view.X + 60, 60, view.Width - 60, view.Height - 55);
            position = new Vector2(ScreenBounds.Left + (ScreenBounds.Width / 2), ScreenBounds.Bottom - 60);

            FlameThrowerRectangle = new RotatedRectangle(new Rectangle((int)position.X, (int)position.Y, 88, 43), 0.0f);
            this.isLoosingLife = false;
        }

        public void SetFlameThrower(Vector2 position, float angle)
        {
            this.FlameThrowerPosition = position;
            this.FlameThrowerAngle = angle - MathHelper.ToRadians(90);

            this.FlameThrowerRectangle.ChangePosition(Convert.ToInt32(position.X), Convert.ToInt32(position.Y));
            this.FlameThrowerRectangle.Rotation = this.FlameThrowerAngle;
        }

        /// <summary>
        /// Resets ship state, sets a color, and deactivates ship.
        /// </summary>
        /// <param name="color"></param>
        public void Reset(Color color)
        {
            this.color = color;
            Reset();
        }        

        /// <summary>
        /// Resets ship state and deactivates ship.
        /// </summary>
        public void Reset()
        {            
            Restart();
            Deactivate();
        }

        /// <summary>
        /// Resets ship state without deactivating the ship.
        /// </summary>
        public void Restart()
        {
            int i;

            position.X = ScreenBounds.X + (ScreenBounds.Width / 2);
            position.Y = ScreenBounds.Y + (ScreenBounds.Height - 60);
            deathTimeTotalSeconds = 0;
            lives = 3;
            score = 0;
            lifecounter = 100;
            currentgun = GunType.pistol;
            ammo = new List<int>(Enum.GetNames(typeof(GunType)).Length);
            for (i = 0; i < Enum.GetNames(typeof(GunType)).Length; i++)
            {
                ammo.Add(0);
            }
            bullets.Clear();
            shotgunbullets.Clear();
            LastShot = 0;
        }

        /// <summary>
        /// Activates ship and assigns it to a player.
        /// </summary>
        /// <param name="player"></param>
        public void Activate(Player player)
        {
            player.IsPlaying = true;
            this.Player = player;
            this.status = ObjectStatus.Active;
        }

        /// <summary>
        /// Deactivates ship.
        /// </summary>
        public void Deactivate()
        {
            this.status = ObjectStatus.Inactive;
        }

        public Vector2 VerifyMove(Vector2 offset)
        {
            if ((status != ObjectStatus.Dying) &&
                (status != ObjectStatus.Inactive))
            {
                Vector2 pos = position + offset;
                pos.X = MathHelper.Clamp(pos.X, ScreenBounds.X, ScreenBounds.Width);
                pos.Y = MathHelper.Clamp(pos.Y, ScreenBounds.Y, ScreenBounds.Height);

                return pos - position;  // Return the distance allowed to travel
            }
            else
                return position;

        }


        public bool VerifyFire(float currentSec, int rateoffire)
        {
            if (VerifyFire(currentSec, LastShot, rateoffire))
            {
                LastShot = currentSec;
                return true;
            }
            return false;
        }

        private static bool VerifyFire(float currentSec, float lastShot, int RateOfFire)
        {
            if ((currentSec - lastShot) > (1f / RateOfFire))
                return true;
            return false;
        }


        public void DestroyShip(float totalGameSeconds)
        {
            deathTimeTotalSeconds = totalGameSeconds;
            status = ObjectStatus.Dying;
        }

        public void Update(GameTime gameTime, float totalGameSeconds)
        {
            if (status == ObjectStatus.Dying)
            {
                if ((totalGameSeconds > deathTimeTotalSeconds + Avatar.RespawnTime))
                {
                    if (lives > 0)
                    {
                        status = ObjectStatus.Immune;
                    }
                    else
                    {
                        status = ObjectStatus.Inactive;
                    }
                }
            }
            else if (status == ObjectStatus.Immune)
            {
                if (totalGameSeconds > deathTimeTotalSeconds + Avatar.ImmuneTime)
                {
                    status = ObjectStatus.Active;
                }
            }

            this.ObjectAvoidCalc = this.behaviors.Update(gameTime, this.entity);
        }

        public AvatarState State
        {
            get
            {
                AvatarState state;

                state.bullets = this.bullets;
                state.shotgunbullets = this.shotgunbullets;
                state.color = this.color;
                state.deathTimeTotalSeconds = this.deathTimeTotalSeconds;
                state.lives = this.lives;
                state.position = this.position;
                state.score = this.score;
                state.status = this.status;
                state.ammo = this.ammo;
                state.lifecounter = this.lifecounter;
                state.currentgun = this.currentgun;

                return state;
            }
            set
            {
                this.bullets = value.bullets;
                this.shotgunbullets = value.shotgunbullets;
                this.color = value.color;
                this.deathTimeTotalSeconds = value.deathTimeTotalSeconds;
                this.lives = value.lives;
                this.position = value.position;
                this.score = value.score;
                this.status = value.status;
                this.ammo = value.ammo;
                this.lifecounter = value.lifecounter;
                this.currentgun = value.currentgun;
            }
        }
    }
}
