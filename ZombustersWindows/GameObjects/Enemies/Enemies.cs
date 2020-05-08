using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class Enemies
    {
        public List<TankState> Tanks = new List<TankState>();
        public List<ZombieState> Zombies = new List<ZombieState>();
        public List<Rat> Rats = new List<Rat>();

        private Random random = new Random(16);

        public Enemies()
        {

        }

        public void LoadContent(ContentManager content)
        {
            foreach (ZombieState zombie in Zombies)
            {
                zombie.LoadContent(content);
            }

            foreach (TankState tank in Tanks)
            {
                tank.LoadContent(content);
            }

            foreach (Rat rat in Rats)
            {
                rat.LoadContent(content);
            }
        }

        public void InitializeEnemy(int quantity, EnemyType enemyType, Level level, int subLevelIndex, float life, float speed, List<int> numplayersIngame)
        {
            int i, RandomX, RandomY;
            int RandomSpawnZone;
            int howManySpawnZones = 4;

            for (i = 0; i < quantity; i++)
            {
                RandomSpawnZone = this.random.Next(0, howManySpawnZones - 1);
                RandomX = this.random.Next(Convert.ToInt32(level.ZombieSpawnZones[RandomSpawnZone].X), Convert.ToInt32(level.ZombieSpawnZones[RandomSpawnZone].Y));
                RandomY = this.random.Next(Convert.ToInt32(level.ZombieSpawnZones[RandomSpawnZone].Z), Convert.ToInt32(level.ZombieSpawnZones[RandomSpawnZone].W));
                float subspeed = subLevelIndex / 10;
                switch (enemyType)
                {
                    case EnemyType.Zombie:
                        ZombieState zombie = new ZombieState(new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f, life, speed + subspeed);
                        zombie.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        zombie.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        zombie.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        Zombies.Add(zombie);
                        break;
                    case EnemyType.Tank:
                        TankState tank = new TankState(new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f);
                        tank.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        tank.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        Tanks.Add(tank);
                        break;
                    case EnemyType.Rat:
                        Rat rat = new Rat(new Vector2(RandomX, RandomY), 5.0f, 1);
                        rat.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        rat.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        rat.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        Rats.Add(rat);
                        break;
                }
                
            }
        }

        public void Clear()
        {
            Zombies.Clear();
            Tanks.Clear();
            Rats.Clear();
        }

        public void Update(ref GameTime gameTime, MyGame game)
        {
            foreach (ZombieState zombie in Zombies)
            {
                zombie.Update(gameTime, game, Zombies);
            }

            foreach (TankState tank in Tanks)
            {
                tank.Update(gameTime, game);
            }

            foreach (Rat rat in Rats)
            {
                rat.Update(gameTime, game, Rats);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float totalGameSeconds, List<Furniture> furnitureList, GameTime gameTime)
        {
            foreach (ZombieState zombie in Zombies)
            {
                zombie.Draw(spriteBatch, totalGameSeconds, furnitureList, gameTime);
            }

            foreach (TankState tank in Tanks)
            {
                tank.Draw(spriteBatch, totalGameSeconds, furnitureList);
            }

            foreach (Rat rat in Rats)
            {
                rat.Draw(spriteBatch, totalGameSeconds, furnitureList, gameTime);
            }
        }
    }
}
