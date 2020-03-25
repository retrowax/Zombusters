using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;

namespace ZombustersWindows
{
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
