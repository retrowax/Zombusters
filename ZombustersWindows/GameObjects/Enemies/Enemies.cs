using System;
using System.Collections.Generic;
using GameAnalyticsSDK.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.GameObjects;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class Enemies
    {
        public List<BaseEnemy> EnemiesList = new List<BaseEnemy>();
        public List<PowerUp> PowerUpList = new List<PowerUp>();

        public Texture2D livePowerUpTexture;
        private Texture2D extraLivePowerUpTexture;
        private Texture2D shotgunAmmoPowerUpTexture;
        private Texture2D machinegunAmmoPowerUpTexture;
        private Texture2D flamethrowerAmmoPowerUpTexture;
        private Texture2D immunePowerUpTexture;
        public Texture2D heart;
        public Texture2D shotgunammoUI;
        public Texture2D pistolammoUI;
        public Texture2D grenadeammoUI;
        public Texture2D flamethrowerammoUI;

        private readonly MyGame game;
        private Random random;

        public Enemies(ref MyGame myGame, ref Random gameRandom)
        {
            game = myGame;
            random = gameRandom;
        }

        public void LoadContent(ContentManager content)
        {
            foreach (BaseEnemy enemy in EnemiesList)
            {
                enemy.LoadContent(content);
            }

            PowerUpsLoad();
        }

        private void PowerUpsLoad()
        {
            livePowerUpTexture = game.Content.Load<Texture2D>(@"InGame/live_powerup");
            extraLivePowerUpTexture = game.Content.Load<Texture2D>(@"InGame/extralife_powerup");
            shotgunAmmoPowerUpTexture = game.Content.Load<Texture2D>(@"InGame/shotgun_ammo_powerup");
            machinegunAmmoPowerUpTexture = game.Content.Load<Texture2D>(@"InGame/machinegun_ammo_powerup");
            flamethrowerAmmoPowerUpTexture = game.Content.Load<Texture2D>(@"InGame/flamethrower_ammo_powerup");
            immunePowerUpTexture = game.Content.Load<Texture2D>(@"InGame/immune_ammo_powerup");
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
                        Zombie zombie = new Zombie(new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f, life, speed + subspeed, ref random);
                        zombie.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        zombie.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        zombie.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        EnemiesList.Add(zombie);
                        break;
                    case EnemyType.Tank:
                        Tank tank = new Tank(new Vector2(0, 0), new Vector2(RandomX, RandomY), 5.0f);
                        tank.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        tank.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        EnemiesList.Add(tank);
                        break;
                    case EnemyType.Rat:
                        Rat rat = new Rat(new Vector2(RandomX, RandomY), 5.0f, life, speed, ref random);
                        rat.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        rat.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        rat.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        EnemiesList.Add(rat);
                        break;
                    case EnemyType.Wolf:
                        Wolf wolf = new Wolf(new Vector2(RandomX, RandomY), 5.0f, life, speed, ref random);
                        wolf.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        wolf.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        wolf.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        EnemiesList.Add(wolf);
                        break;
                    case EnemyType.Minotaur:
                        Minotaur minotaur = new Minotaur(new Vector2(RandomX, RandomY), 5.0f, life, speed, ref random);
                        minotaur.behaviors.AddBehavior(new Pursuit(Arrive.Deceleration.fast, 50.0f));
                        minotaur.behaviors.AddBehavior(new ObstacleAvoidance(ref level.gameWorld, 15.0f));
                        minotaur.playerChased = numplayersIngame[this.random.Next(numplayersIngame.Count)];
                        EnemiesList.Add(minotaur);
                        break;
                }
                
            }
        }

        public void HandleCollisions(Player player, float totalGameSeconds)
        {
            HandleEnemiesCollisions(player, totalGameSeconds);
            HandlePowerUpCollisions(player);
        }

        private void HandleEnemiesCollisions(Player player, float totalGameSeconds)
        {
            for (int i = 0; i < EnemiesList.Count; i++)
            {
                BaseEnemy enemy = EnemiesList[i];
                if (enemy.status == ObjectStatus.Active)
                {
                    if (player.avatar.currentgun == GunType.flamethrower && player.avatar.ammo[(int)player.avatar.currentgun] > 0)
                    {
                        if (player.avatar.accumFire.Length() > .5)
                        {
                            if (player.avatar.FlameThrowerRectangle.Intersects(new Rectangle((int)enemy.entity.Position.X, (int)enemy.entity.Position.Y, 48, (int)enemy.entity.Height)))
                            {
                                if (enemy.lifecounter > 1.0f)
                                {
                                    enemy.lifecounter -= 0.2f;
                                    enemy.isLoosingLife = true;
                                }
                                else
                                {
                                    enemy.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                                    player.avatar.score += 10;
                                    game.audio.PlayZombieDying();

                                    if (player.avatar.score % 8000 == 0)
                                    {
                                        player.avatar.lives += 1;
                                    }

                                    if (PowerUpIsInRange(enemy.entity.Position, (int)enemy.entity.Width, (int)enemy.entity.Height))
                                    {
                                        SpawnPowerUp(enemy);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int l = 0; l < player.avatar.bullets.Count; l++)
                        {
                            if (GameplayHelper.DetectBulletCollision(player.avatar.bullets[l], enemy.entity.Position, totalGameSeconds))
                            {
                                if (enemy.lifecounter > 1.0f)
                                {
                                    enemy.lifecounter -= 1.0f;
                                    enemy.isLoosingLife = true;
                                    player.avatar.bullets.RemoveAt(l);
                                }
                                else
                                {
                                    enemy.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                                    player.avatar.score += 10;
                                    game.audio.PlayZombieDying();

                                    if (player.avatar.score % 8000 == 0)
                                    {
                                        player.avatar.lives += 1;
                                    }
                                    player.avatar.bullets.RemoveAt(l);
                                    if (PowerUpIsInRange(enemy.entity.Position, (int)enemy.entity.Width, (int)enemy.entity.Height))
                                    {
                                        SpawnPowerUp(enemy);
                                    }
                                }
                            }
                        }

                        for (int bulletCount = 0; bulletCount < player.avatar.shotgunbullets.Count; bulletCount++)
                        {
                            for (int pelletCount = 0; pelletCount < player.avatar.shotgunbullets[bulletCount].Pellet.Count; pelletCount++)
                            {
                                if (GameplayHelper.DetectPelletCollision(player.avatar.shotgunbullets[bulletCount].Pellet[pelletCount], enemy.entity.Position, player.avatar.shotgunbullets[bulletCount].Angle, pelletCount, totalGameSeconds))
                                {
                                    player.avatar.shotgunbullets[bulletCount].Pellet.RemoveAt(pelletCount);
                                    if (enemy.lifecounter > 1.0f)
                                    {
                                        enemy.lifecounter -= 1.0f;
                                        enemy.isLoosingLife = true;
                                    }
                                    else
                                    {
                                        enemy.Destroy(game.totalGameSeconds, player.avatar.currentgun);
                                        player.avatar.score += 10;
                                        game.audio.PlayZombieDying();

                                        if (player.avatar.score % 8000 == 0)
                                        {
                                            player.avatar.lives += 1;
                                        }

                                        if (PowerUpIsInRange(enemy.entity.Position, (int)enemy.entity.Width, (int)enemy.entity.Height))
                                        {
                                            SpawnPowerUp(enemy);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (GameplayHelper.DetectCrash(player.avatar, enemy.entity.Position))
                {
                    if (enemy.status == ObjectStatus.Active)
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
            int enemiesCount = 0;

            foreach (BaseEnemy enemy in EnemiesList)
            {
                if (enemy.status == ObjectStatus.Active)
                {
                    enemiesCount++;
                }
            }

            return enemiesCount;
        }

        public void Clear()
        {
            EnemiesList.Clear();
        }

        public void Update(ref GameTime gameTime, MyGame game)
        {
            foreach (BaseEnemy enemy in EnemiesList)
            {
                enemy.Update(gameTime, game, EnemiesList);
            }
        }

        public void UpdatePowerUps(ref GameTime gameTime, MyGame game)
        {
            foreach (PowerUp powerup in PowerUpList)
            {
                powerup.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float totalGameSeconds, List<Furniture> furnitureList, GameTime gameTime)
        {
            foreach (BaseEnemy enemy in EnemiesList)
            {
                enemy.Draw(spriteBatch, totalGameSeconds, furnitureList, gameTime);
            }
        }

        public void DrawPowerUps(SpriteBatch spriteBatch, GameTime gameTime)
        {
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

        private void SpawnPowerUp(BaseEnemy enemy)
        {
            bool isPowerUpAdded = false;
            if (this.random.Next(1, 14) == 8)
            {
                isPowerUpAdded = true;
                PowerUpType powerUpType = (PowerUpType)Enum.ToObject(typeof(PowerUpType), this.random.Next(0, Enum.GetNames(typeof(PowerUpType)).Length - 4));
                switch (powerUpType)
                {
                    case PowerUpType.live:
                        PowerUpList.Add(new PowerUp(livePowerUpTexture, heart, enemy.entity.Position, PowerUpType.live, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.live);
                        break;

                    case PowerUpType.machinegun:
                        PowerUpList.Add(new PowerUp(machinegunAmmoPowerUpTexture, pistolammoUI, enemy.entity.Position, PowerUpType.machinegun, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.machinegun);
                        break;

                    case PowerUpType.flamethrower:
                        PowerUpList.Add(new PowerUp(flamethrowerAmmoPowerUpTexture, flamethrowerammoUI, enemy.entity.Position, PowerUpType.flamethrower, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.flamethrower);
                        break;

                    case PowerUpType.shotgun:
                        PowerUpList.Add(new PowerUp(shotgunAmmoPowerUpTexture, shotgunammoUI, enemy.entity.Position, PowerUpType.shotgun, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.shotgun);
                        break;

                    case PowerUpType.grenade:
                        PowerUpList.Add(new PowerUp(grenadeammoUI, grenadeammoUI, enemy.entity.Position, PowerUpType.grenade, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.grenade);
                        break;

                    case PowerUpType.speedbuff:
                        PowerUpList.Add(new PowerUp(livePowerUpTexture, heart, enemy.entity.Position, PowerUpType.speedbuff, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.speedbuff);
                        break;

                    case PowerUpType.immunebuff:
                        PowerUpList.Add(new PowerUp(immunePowerUpTexture, immunePowerUpTexture, enemy.entity.Position, PowerUpType.immunebuff, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.immunebuff);
                        break;

                    case PowerUpType.extralife:
                        PowerUpList.Add(new PowerUp(extraLivePowerUpTexture, extraLivePowerUpTexture, enemy.entity.Position, PowerUpType.extralife, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.extralife);
                        break;

                    default:
                        PowerUpList.Add(new PowerUp(livePowerUpTexture, heart, enemy.entity.Position, PowerUpType.live, game.Content));
                        GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.live);
                        break;
                }
            }

            if (!isPowerUpAdded)
            {
                if (this.random.Next(1, 128) == 12)
                {
                    GameAnalytics.AddDesignEvent("PowerUps:Dropped", (int)PowerUpType.extralife);
                    PowerUpList.Add(new PowerUp(extraLivePowerUpTexture, extraLivePowerUpTexture, enemy.entity.Position, PowerUpType.extralife, game.Content));
                }
            }
        }
    }
}
