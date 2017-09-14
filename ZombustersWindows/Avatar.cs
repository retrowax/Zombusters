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
#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;

namespace ZombustersWindows
{

    public struct AvatarState
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
        public int currentgun;
        //public bool FiredGun;
    }

    public class ShotgunShell
    {
        public List<Vector3> Pellet; //perdigon
        public Vector2 Position, Direction, Velocity;
        public float Angle;

        static readonly float pelletsSpreadRadians = MathHelper.ToRadians(2.5f);
        const float initialSpeed = 640f;
        protected float radius = 1f;

        public ShotgunShell(Vector2 position, Vector2 direction, float angle, float totalgameseconds)
        {
            // initialize the graphics data
            this.Position = position;
            this.Angle = angle;
            //this.Angle = (float)Math.Acos(Vector2.Dot(Vector2.UnitY, direction));
            //if (direction.X > 0f)
            //{
            //    this.Angle *= -1f;
            //}

            this.Velocity =  initialSpeed * direction;

            // calculate the direction vectors for the second and third projectiles
            float rotation = (float)Math.Acos(Vector2.Dot(new Vector2(0f, -1f),
                direction));
            rotation *= (Vector2.Dot(new Vector2(0f, -1f),
                new Vector2(direction.Y, -direction.X)) > 0f) ? 1f : -1f;
            Vector2 Direction2 = new Vector2(
                 (float)Math.Sin(rotation - pelletsSpreadRadians),
                -(float)Math.Cos(rotation - pelletsSpreadRadians));
            Vector2 Direction3 = new Vector2(
                 (float)Math.Sin(rotation + pelletsSpreadRadians),
                -(float)Math.Cos(rotation + pelletsSpreadRadians));

            Pellet = new List<Vector3>();
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
        }
    }

    public class Avatar
    {
        // Network/Draw state information
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
        public int currentgun;
        public Vector2 accumMove;
        public Vector2 accumFire;

        public Player Player;
        public bool isReady;
        public bool IsPlaying
        {
            get { return status != ObjectStatus.Inactive; }
        }

        //PRUEBASS
        public SteeringEntity entity;
        public SteeringBehaviors behaviors;
        public const float VELOCIDAD_MAXIMA = 1.0f;
        public const float FUERZA_MAXIMA = 0.15f;
        public Vector2 ObjectAvoidCalc = Vector2.Zero;

        public static float CrashRadius = 20f;
        public static float RespawnTime = 5.0f;
        public static float PixelsPerSecond = 200;
        public static int bulletmax = 20;
        public static int pelletmax = 50;
        
        public Rectangle ScreenBounds;
        
        // Variables to control bullet behavior
        public int RateOfFire = 2; // per second
        private float LastShot;

        // FlameThrower Particle Effect
        public Vector2 FlameThrowerPosition;
        public float FlameThrowerAngle;
        public RotatedRectangle FlameThrowerRectangle;

        public bool isLoosingLife;
        /*
        public Vector2 FlameThrowerEffectPosition;
        private Renderer particleRenderer1, particleRenderer2;
        private ParticleEffect FlameThrowerEffect, FlameThrowerEffectYellow;
        private ConeEmitter FlameThrowerConeEmitter, FlameThrowerConeEmitterYellow;
        private LinearGravityModifier linearGravityModifier; */

        public Avatar()             
        {
            bullets = new List<Vector4>(bulletmax);
            shotgunbullets = new List<ShotgunShell>(pelletmax);
        }
      
               
        public void Initialize(Viewport view)
        {
            //PRUEBASS
            this.entity = new SteeringEntity();
            this.entity.Width = 28;
            this.entity.Height = 50;
            this.entity.Velocity = Vector2.Zero;
            this.entity.Position = Vector2.Zero;
            this.entity.BoundingRadius = 10.0f;
            this.entity.MaxSpeed = VELOCIDAD_MAXIMA;
            this.behaviors = new SteeringBehaviors(0.15f, CombinationType.prioritized);

            //ScreenBounds = new Rectangle(view.X + 50, 50, view.Width - 100, view.Height - 100);
            ScreenBounds = new Rectangle(view.X + 60, 60, view.Width - 60, view.Height - 55);
            position = new Vector2(ScreenBounds.Left + (ScreenBounds.Width / 2), ScreenBounds.Bottom - 60);

            FlameThrowerRectangle = new RotatedRectangle(new Rectangle((int)position.X, (int)position.Y, 88, 43), 0.0f);
            this.isLoosingLife = false;
        }

/*
        public void LoadFlameThrower(Game1 game)
        {

            // Efecto particulas de Humo
            FlameThrowerEffect = new ParticleEffect();

            linearGravityModifier = new LinearGravityModifier { Gravity = new Vector2(75, 0) };

            FlameThrowerConeEmitter = new ConeEmitter
                {
                    Budget = 500,
                    Term = 3f,

                    Enabled = true,
                    Name = "FlameThrowerEmitter",
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0, 0),
                    MinimumTriggerPeriod = 0.0f,
                    ReleaseColour = new VariableFloat3 { Value = new Vector3(1, 0.5f, 0), Variation = new Vector3(0, 0, 0) },
                    ReleaseImpulse = new Vector2(10, 0),
                    ReleaseOpacity = new VariableFloat { Value = 1f, Variation = 0f },
                    ReleaseQuantity = 1,
                    ReleaseRotation = new VariableFloat { Value = 0, Variation = MathHelper.Pi },
                    ReleaseSpeed = new VariableFloat { Value = 200, Variation = 100 },
                    ReleaseScale = new VariableFloat { Value = 0.1f, Variation = 1 },
                    ParticleTextureAssetName = "Flame2",
                    Direction = 0.1f,
                    ConeAngle = 0.1f,

                    Modifiers = new ModifierCollection
                    {
                        new DampingModifier { DampingCoefficient = 2 },
                        new ScaleModifier 
                        {
                            InitialScale = 1,
                            UltimateScale = 50,
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1.57f,
                            FinalRate = 0,
                        },
                        //new LinearGravityModifier { Gravity = new Vector2(75,0) },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 0.1f,
                            MiddleOpacity = 0.5f,
                            MiddlePosition = 0.1f,
                            FinalOpacity = 0,
                        },
                    },
                };

            FlameThrowerConeEmitter.Modifiers.Add(linearGravityModifier);

            //particleEffect = new ParticleEffect();
            //particleEffect = effect.DeepCopy();
            FlameThrowerEffect.Add(FlameThrowerConeEmitter);
            FlameThrowerEffect.Initialise();
            FlameThrowerEffect.LoadContent(game.Content);

            FlameThrowerEffectYellow = new ParticleEffect();

            FlameThrowerConeEmitterYellow = new ConeEmitter
                {
                    Budget = 500,
                    Term = 3f,

                    Enabled = true,
                    Name = "FlameThrowerEmitterYellow",
                    BlendMode = EmitterBlendMode.Alpha,
                    TriggerOffset = new Vector2(0, 0),
                    MinimumTriggerPeriod = 0.0f,
                    ReleaseColour = new VariableFloat3 { Value = new Vector3(1, 1, 0), Variation = new Vector3(0, 0, 0) },
                    ReleaseImpulse = new Vector2(0, 0),
                    ReleaseOpacity = new VariableFloat { Value = 1f, Variation = 0f },
                    ReleaseQuantity = 1,
                    ReleaseRotation = new VariableFloat { Value = 0, Variation = MathHelper.Pi },
                    ReleaseSpeed = new VariableFloat { Value = 200, Variation = 100 },
                    ReleaseScale = new VariableFloat { Value = 0.1f, Variation = 0.5f },
                    ParticleTextureAssetName = "Flame2",
                    Direction = 0.1f,
                    ConeAngle = 0.1f,

                    Modifiers = new ModifierCollection
                    {
                        new DampingModifier { DampingCoefficient = 3 },
                        new ScaleModifier 
                        {
                            InitialScale = 1,
                            UltimateScale = 20,
                        },
                        new RotationRateModifier
                        {
                            InitialRate = 1.57f,
                            FinalRate = 0,
                        },
                        //new LinearGravityModifier { Gravity = new Vector2(75,0) },
                        new OpacityInterpolatorModifier
                        {
                            InitialOpacity = 1.0f,
                            MiddleOpacity = 0.2f,
                            MiddlePosition = 0.2f,
                            FinalOpacity = 0,
                        },
                    },
                };

            FlameThrowerConeEmitter.Modifiers.Add(linearGravityModifier);

            //particleEffect = new ParticleEffect();
            //particleEffect = effect.DeepCopy();
            FlameThrowerEffectYellow.Add(FlameThrowerConeEmitterYellow);
            FlameThrowerEffectYellow.Initialise();
            FlameThrowerEffectYellow.LoadContent(game.Content);

            FlameThrowerEffectPosition = this.position;

            particleRenderer1 = new SpriteBatchRenderer
            {
                GraphicsDeviceService = game.graphics
            };
            particleRenderer1.LoadContent(game.Content);

            particleRenderer2 = new SpriteBatchRenderer
            {
                GraphicsDeviceService = game.graphics
            };
            particleRenderer2.LoadContent(game.Content);
        }

        public void setFlameThrower(Vector2 position, float angle)
        {
            this.FlameThrowerEffectPosition = position;

            FlameThrowerEffect.Clear();
            FlameThrowerConeEmitter.Direction = angle - MathHelper.ToRadians(90);
            if (angle < 0)
            {
                FlameThrowerConeEmitter.ReleaseImpulse = new Vector2(-10,0);
                FlameThrowerConeEmitter.Modifiers.Remove(linearGravityModifier);
                linearGravityModifier.Gravity = new Vector2(-75,0);
                FlameThrowerConeEmitter.Modifiers.Add(linearGravityModifier);
            }
            else
            {
                FlameThrowerConeEmitter.ReleaseImpulse = new Vector2(10, 0);
                FlameThrowerConeEmitter.Modifiers.Remove(linearGravityModifier);
                linearGravityModifier.Gravity = new Vector2(75, 0);
                FlameThrowerConeEmitter.Modifiers.Add(linearGravityModifier);
            }
            //ReleaseImpulse,Direction, LinearGravityModifier

            FlameThrowerEffect.Add(FlameThrowerConeEmitter);

            FlameThrowerEffectYellow.Clear();
            FlameThrowerConeEmitterYellow.Direction = angle - MathHelper.ToRadians(90);
            //FlameThrowerConeEmitterYellow.ConeAngle = MathHelper.WrapAngle(angle);
            FlameThrowerEffectYellow.Add(FlameThrowerConeEmitter);
        }

        public void DrawFlameThrower()
        {
            if (this.currentgun == 3 && this.ammo[3] > 0)
            {
                // Render Efecto Particulas
                particleRenderer1.RenderEffect(FlameThrowerEffect);
                particleRenderer2.RenderEffect(FlameThrowerEffectYellow);
            }
        }
*/

        public void setFlameThrower(Vector2 position, float angle)
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
            currentgun = 0;
            ammo = new List<int>(4);
            for (i = 0; i < 4; i++)
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
                        //state.lives--;
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
                //Ship.ClampPlayer(ref state, ScreenBounds);

                if (totalGameSeconds > deathTimeTotalSeconds + 
                    (Avatar.RespawnTime * 2))
                {
                    status = ObjectStatus.Active;
                }
            }

            this.ObjectAvoidCalc = this.behaviors.Update(gameTime, this.entity);
            //this.entity.Velocity += this.behaviors.Update(gameTime, this.entity);
            //this.entity.Velocity = VectorHelper.TruncateVector(this.entity.Velocity, this.entity.MaxSpeed / 1.5f);

            // Set presence info for Player
            //Player.SetPresenceValue(score);
/*
            // Particle Engine Update
            if (this.currentgun == 3 && this.ammo[3] > 0)
            {
                FlameThrowerEffect.Trigger(FlameThrowerEffectPosition);
                FlameThrowerEffectYellow.Trigger(FlameThrowerEffectPosition);
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                FlameThrowerEffect.Update(deltaSeconds);
                FlameThrowerEffectYellow.Update(deltaSeconds);
            }
 */
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
