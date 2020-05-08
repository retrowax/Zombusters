using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.GameObjects;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class Enemies
    {
        public List<TankState> Tanks = new List<TankState>();
        public List<ZombieState> Zombies = new List<ZombieState>();
        public List<Rat> Rats = new List<Rat>();
        public List<Wolf> Wolfs = new List<Wolf>();
        public List<Minotaur> Minotaurs = new List<Minotaur>();
        public List<PowerUp> PowerUpList = new List<PowerUp>();
        public Texture2D livePowerUp, extraLivePowerUp, shotgunAmmoPowerUp, machinegunAmmoPowerUp, flamethrowerAmmoPowerUp, immunePowerUp, heart, shotgunammoUI, pistolammoUI, grenadeammoUI, flamethrowerammoUI;

        private readonly MyGame game;
        private readonly Random random = new Random(16);

        public Enemies(ref MyGame myGame)
        {
            game = myGame;
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

            foreach (Wolf wolf in Wolfs)
            {
                wolf.LoadContent(content);
            }

            foreach (Minotaur minotaur in Minotaurs)
            {
                minotaur.LoadContent(content);
            }

            PowerUpsLoad();
        }

        private void PowerUpsLoad()
        {
            livePowerUp = game.Content.Load<Texture2D>(@"InGame/live_powerup");
            extraLivePowerUp = game.Content.Load<Texture2D>(@"InGame/extralife_powerup");
            shotgunAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/shotgun_ammo_powerup");
            machinegunAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/machinegun_ammo_powerup");
            flamethrowerAmmoPowerUp = game.Content.Load<Texture2D>(@"InGame/flamethrower_ammo_powerup");
            immunePowerUp = game.Content.Load<Texture2D>(@"InGame/immune_ammo_powerup");
            heart = game.Content.Load<Texture2D>(@"InGame/GUI/heart");
            shotgunammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/shotgunammo");
            pistolammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/pistolammo");
            grenadeammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/grenadeammo");
            flamethrowerammoUI = game.Content.Load<Texture2D>(@"InGame/GUI/flamethrowerammo");
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
                    case EnemyType.Wolf:
                        Wolf wolf = new Wolf(new Vector2(RandomX, RandomY), 5.0f, 1);
                        wolf.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        wolf.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        wolf.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        Wolfs.Add(wolf);
                        break;
                    case EnemyType.Minotaur:
                        Minotaur minotaur = new Minotaur(new Vector2(RandomX, RandomY), 5.0f, 1);
                        minotaur.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        minotaur.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        minotaur.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        Minotaurs.Add(minotaur);
                        break;
                }
                
            }
        }

        public void HandleCollisions(Player player, float totalGameSeconds)
        {
            HandleZombieCollisions(player, totalGameSeconds);
            HandleTankCollisions(player, totalGameSeconds);
            HandleRatCollisions(player, totalGameSeconds);
            HandleWolfCollisions(player, totalGameSeconds);
            HandleMinotaurCollisions(player, totalGameSeconds);
            HandlePowerUpCollisions(player);
        }

        private void HandleTankCollisions(Player player, float totalGameSeconds)
        {
            for (int i = 0; i < Tanks.Count; i++)
            {
                TankState tank = Tanks[i];
                if (tank.status == ObjectStatus.Active)
                {
                    for (int l = 0; l < player.avatar.bullets.Count; l++)
                    {
                        if (GameplayHelper.DetectCollision(player.avatar.bullets[l], tank.entity.Position, totalGameSeconds))
                        {
                            tank.DestroyTank(totalGameSeconds);
                            player.avatar.score += 10;
                            game.audio.PlayZombieDying();

                            if (player.avatar.score % 8000 == 0)
                            {
                                player.avatar.lives += 1;
                            }
                            player.avatar.bullets.RemoveAt(l);
                        }
                    }
                }
            }
        }

        private void HandleRatCollisions(Player player, float totalGameSeconds)
        {
            foreach (Rat rat in Rats)
            {
                if (rat.status == ObjectStatus.Active)
                {
                    for (int l = 0; l < player.avatar.bullets.Count; l++)
                    {
                        if (GameplayHelper.DetectCollision(player.avatar.bullets[l], rat.entity.Position, totalGameSeconds))
                        {
                            rat.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                            player.avatar.score += 10;
                            game.audio.PlayZombieDying();

                            if (player.avatar.score % 8000 == 0)
                            {
                                player.avatar.lives += 1;
                            }
                            player.avatar.bullets.RemoveAt(l);
                        }
                    }
                }
            }
        }

        private void HandleWolfCollisions(Player player, float totalGameSeconds)
        {
            foreach (Wolf wolf in Wolfs)
            {
                if (wolf.status == ObjectStatus.Active)
                {
                    for (int l = 0; l < player.avatar.bullets.Count; l++)
                    {
                        if (GameplayHelper.DetectCollision(player.avatar.bullets[l], wolf.entity.Position, totalGameSeconds))
                        {
                            wolf.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                            player.avatar.score += 10;
                            game.audio.PlayZombieDying();

                            if (player.avatar.score % 8000 == 0)
                            {
                                player.avatar.lives += 1;
                            }
                            player.avatar.bullets.RemoveAt(l);
                        }
                    }
                }
            }
        }

        private void HandleMinotaurCollisions(Player player, float totalGameSeconds)
        {
            foreach (Minotaur minotaur in Minotaurs)
            {
                if (minotaur.status == ObjectStatus.Active)
                {
                    for (int l = 0; l < player.avatar.bullets.Count; l++)
                    {
                        if (GameplayHelper.DetectCollision(player.avatar.bullets[l], minotaur.entity.Position, totalGameSeconds))
                        {
                            minotaur.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                            player.avatar.score += 10;
                            game.audio.PlayZombieDying();

                            if (player.avatar.score % 8000 == 0)
                            {
                                player.avatar.lives += 1;
                            }
                            player.avatar.bullets.RemoveAt(l);
                        }
                    }
                }
            }
        }

        private void HandleZombieCollisions(Player player, float totalGameSeconds)
        {
            for (int i = 0; i < Zombies.Count; i++)
            {
                ZombieState zombie = Zombies[i];
                if (zombie.status == ObjectStatus.Active)
                {
                    if (player.avatar.currentgun == GunType.flamethrower && player.avatar.ammo[(int)player.avatar.currentgun] > 0)
                    {
                        if (player.avatar.accumFire.Length() > .5)
                        {
                            if (player.avatar.FlameThrowerRectangle.Intersects(new Rectangle((int)zombie.entity.Position.X, (int)zombie.entity.Position.Y, 48, (int)zombie.entity.Height)))
                            {
                                if (zombie.lifecounter > 1.0f)
                                {
                                    zombie.lifecounter -= 0.2f;
                                    zombie.isLoosingLife = true;
                                }
                                else
                                {
                                    zombie.DestroyZombie(game.totalGameSeconds, player.avatar.currentgun);
                                    player.avatar.score += 10;
                                    game.audio.PlayZombieDying();

                                    if (player.avatar.score % 8000 == 0)
                                    {
                                        player.avatar.lives += 1;
                                    }

                                    if (PowerUpIsInRange(zombie.entity.Position, zombie.ZombieTexture.Width, zombie.ZombieTexture.Height))
                                    {
                                        SpawnPowerUp(zombie);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int l = 0; l < player.avatar.bullets.Count; l++)
                        {
                            if (GameplayHelper.DetectCollision(player.avatar.bullets[l], zombie.entity.Position, totalGameSeconds))
                            {
                                if (zombie.lifecounter > 1.0f)
                                {
                                    zombie.lifecounter -= 1.0f;
                                    zombie.isLoosingLife = true;
                                    player.avatar.bullets.RemoveAt(l);
                                }
                                else
                                {
                                    zombie.DestroyZombie(game.totalGameSeconds, player.avatar.currentgun);
                                    player.avatar.score += 10;
                                    game.audio.PlayZombieDying();

                                    if (player.avatar.score % 8000 == 0)
                                    {
                                        player.avatar.lives += 1;
                                    }
                                    player.avatar.bullets.RemoveAt(l);
                                    if (PowerUpIsInRange(zombie.entity.Position, zombie.ZombieTexture.Width, zombie.ZombieTexture.Height))
                                    {
                                        SpawnPowerUp(zombie);
                                    }
                                }
                            }
                        }

                        for (int bulletCount = 0; bulletCount < player.avatar.shotgunbullets.Count; bulletCount++)
                        {
                            for (int pelletCount = 0; pelletCount < player.avatar.shotgunbullets[bulletCount].Pellet.Count; pelletCount++)
                            {
                                if (GameplayHelper.DetectCollision(player.avatar.shotgunbullets[bulletCount].Pellet[pelletCount], zombie.entity.Position, totalGameSeconds))
                                {
                                    player.avatar.shotgunbullets[bulletCount].Pellet.RemoveAt(pelletCount);
                                    if (zombie.lifecounter > 1.0f)
                                    {
                                        zombie.lifecounter -= 1.0f;
                                        zombie.isLoosingLife = true;
                                    }
                                    else
                                    {
                                        zombie.DestroyZombie(game.totalGameSeconds, player.avatar.currentgun);
                                        player.avatar.score += 10;
                                        game.audio.PlayZombieDying();

                                        if (player.avatar.score % 8000 == 0)
                                        {
                                            player.avatar.lives += 1;
                                        }

                                        if (PowerUpIsInRange(zombie.entity.Position, zombie.ZombieTexture.Width, zombie.ZombieTexture.Height))
                                        {
                                            SpawnPowerUp(zombie);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (GameplayHelper.DetectCrash(player.avatar, zombie.entity.Position))
                {
                    if (zombie.status == ObjectStatus.Active)
                    {
                        if (player.avatar.lifecounter <= 0)
                        {
                            player.Destroy();
                            player.avatar.lifecounter = 100;
                        }
                        else
                        {
                            player.avatar.isLoosingLife = true;
                            player.avatar.lifecounter -= 1;
                        }
                    }
                }
            }
        }

        private void HandlePowerUpCollisions(Player player)
        {
            foreach (PowerUp powerUp in PowerUpList)
            {
                if (powerUp.status == ObjectStatus.Active)
                {
                    if (GameplayHelper.DetectCrash(player.avatar, powerUp.Position))
                    {
                        if (powerUp.powerUpType == PowerUpType.extralife)
                        {
                            if (player.avatar.lives < 9)
                            {
                                player.avatar.lives++;
                            }
                            powerUp.status = ObjectStatus.Dying;
                        }

                        if (powerUp.powerUpType == PowerUpType.live)
                        {
                            if (player.avatar.lifecounter < 100)
                            {
                                player.avatar.lifecounter += powerUp.Value;

                                if (player.avatar.lifecounter > 100)
                                {
                                    player.avatar.lifecounter = 100;
                                }
                            }

                            powerUp.status = ObjectStatus.Dying;
                        }

                        if (powerUp.powerUpType == PowerUpType.machinegun)
                        {
                            player.avatar.ammo[(int)GunType.machinegun] += powerUp.Value;
                            powerUp.status = ObjectStatus.Dying;

                        }

                        if (powerUp.powerUpType == PowerUpType.shotgun)
                        {
                            player.avatar.ammo[(int)GunType.shotgun] += powerUp.Value;
                            powerUp.status = ObjectStatus.Dying;

                        }

                        if (powerUp.powerUpType == PowerUpType.grenade)
                        {
                            player.avatar.ammo[(int)GunType.grenade] += powerUp.Value;
                            powerUp.status = ObjectStatus.Dying;

                        }

                        if (powerUp.powerUpType == PowerUpType.flamethrower)
                        {
                            player.avatar.ammo[(int)GunType.flamethrower] += powerUp.Value;
                            powerUp.status = ObjectStatus.Dying;

                        }

                        if (powerUp.powerUpType == PowerUpType.speedbuff || powerUp.powerUpType == PowerUpType.immunebuff)
                        {
                            //player. += powerup.Value;
                            powerUp.status = ObjectStatus.Dying;
                        }
                    }
                }
            }
        }

        public int Count()
        {
            return Zombies.Count + Tanks.Count + Rats.Count + Wolfs.Count + Minotaurs.Count;
        }

        public void Clear()
        {
            Zombies.Clear();
            Tanks.Clear();
            Rats.Clear();
            Wolfs.Clear();
            Minotaurs.Clear();
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

            foreach (Wolf wolf in Wolfs)
            {
                wolf.Update(gameTime, game, Wolfs);
            }

            foreach (Minotaur minotaur in Minotaurs)
            {
                minotaur.Update(gameTime, game, Minotaurs);
            }

            foreach (PowerUp powerup in PowerUpList)
            {
                powerup.Update(gameTime);
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

            foreach (Wolf wolf in Wolfs)
            {
                wolf.Draw(spriteBatch, totalGameSeconds, furnitureList, gameTime);
            }

            foreach (Minotaur minotaur in Minotaurs)
            {
                minotaur.Draw(spriteBatch, totalGameSeconds, furnitureList, gameTime);
            }

            foreach (PowerUp powerup in PowerUpList)
            {
                powerup.Draw(spriteBatch, gameTime);
            }
        }

        private bool PowerUpIsInRange(Vector2 position, int width, int height)
        {
            Rectangle ScreenBounds;
            ScreenBounds = new Rectangle(game.GraphicsDevice.Viewport.X + 60, 60, game.GraphicsDevice.Viewport.Width - 60, game.GraphicsDevice.Viewport.Height - 55);
            if (ScreenBounds.Intersects(new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), width, height)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SpawnPowerUp(ZombieState zombie)
        {
            if (this.random.Next(1, 16) == 8)
            {
                PowerUpType powerUpType = (PowerUpType)Enum.ToObject(typeof(PowerUpType), this.random.Next(0, Enum.GetNames(typeof(PowerUpType)).Length));
                switch (powerUpType)
                {
                    case PowerUpType.live:
                        PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUpType.live));
                        break;

                    case PowerUpType.machinegun:
                        PowerUpList.Add(new PowerUp(machinegunAmmoPowerUp, pistolammoUI, zombie.entity.Position, PowerUpType.machinegun));
                        break;

                    case PowerUpType.flamethrower:
                        PowerUpList.Add(new PowerUp(flamethrowerAmmoPowerUp, flamethrowerammoUI, zombie.entity.Position, PowerUpType.flamethrower));
                        break;

                    case PowerUpType.extralife:
                        PowerUpList.Add(new PowerUp(extraLivePowerUp, extraLivePowerUp, zombie.entity.Position, PowerUpType.extralife));
                        break;

                    case PowerUpType.shotgun:
                        PowerUpList.Add(new PowerUp(shotgunAmmoPowerUp, shotgunammoUI, zombie.entity.Position, PowerUpType.shotgun));
                        break;

                    case PowerUpType.grenade:
                        PowerUpList.Add(new PowerUp(grenadeammoUI, grenadeammoUI, zombie.entity.Position, PowerUpType.grenade));
                        break;

                    case PowerUpType.speedbuff:
                        PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUpType.speedbuff));
                        break;

                    case PowerUpType.immunebuff:
                        PowerUpList.Add(new PowerUp(immunePowerUp, immunePowerUp, zombie.entity.Position, PowerUpType.immunebuff));
                        break;

                    default:
                        PowerUpList.Add(new PowerUp(livePowerUp, heart, zombie.entity.Position, PowerUpType.live));
                        break;
                }
            }
        }
    }
}
