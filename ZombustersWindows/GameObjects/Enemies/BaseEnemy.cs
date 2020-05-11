using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using ZombustersWindows.GameObjects;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class BaseEnemy
    {
        public SteeringBehaviors behaviors;
        public SteeringEntity entity;

        public ObjectStatus status;
        public float deathTimeTotalSeconds;
        public float TimeOnScreen;
        public bool invert;
        public float speed;
        public float angle;
        public int playerChased;
        public int entityYOffset;

        public float lifecounter;
        public bool isLoosingLife;

        public Random random;
        public GunType currentgun;
        public float timer;
        public bool isInPlayerRange;

        public SpriteFont font;

#if DEBUG
        Texture2D PositionReference;
        SpriteFont DebugFont;
        bool collision = false;
#endif

        public BaseEnemy()
        {
        }

        public bool IsInRange(Player[] players)
        {
            foreach (Player player in players)
            {
                float distance = Vector2.Distance(entity.Position, player.avatar.position);
                if (distance < Avatar.CrashRadius + 20.0f)
                {
#if DEBUG
                    collision = true;
#endif
                    return true;
                }
            }
#if DEBUG
            collision = false;
#endif
            return false;
        }

        virtual public void Destroy(float totalGameSeconds, GunType currentgun)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Dying;
            this.currentgun = currentgun;
        }

        // Destroy the seeker without leaving a powerup
        virtual public void Crash(float totalGameSeconds)
        {
            this.deathTimeTotalSeconds = totalGameSeconds;
            this.status = ObjectStatus.Inactive;
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


        virtual public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>(@"menu\ArialMenuInfo");
#if DEBUG
            PositionReference = content.Load<Texture2D>(@"InGame/position_reference_temporal");
            DebugFont = content.Load<SpriteFont>(@"menu/ArialMenuInfo");
#endif
        }

        virtual public void Update(GameTime gameTime, MyGame game, List<BaseEnemy> enemyList)
        {
        }

        virtual public void Draw(SpriteBatch batch, float TotalGameSeconds, List<Furniture> furniturelist, GameTime gameTime)
        {
#if DEBUG
            batch.Draw(PositionReference, new Rectangle(Convert.ToInt32(this.entity.Position.X), Convert.ToInt32(this.entity.Position.Y), PositionReference.Width, PositionReference.Height),
            new Rectangle(0, 0, PositionReference.Width, PositionReference.Height), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            batch.DrawString(DebugFont, collision.ToString(), this.entity.Position, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
#endif
            if (this.status == ObjectStatus.Dying)
            {
                int score = 10;
                float layerIndex = GetLayerIndex(this.entity, furniturelist);
                if ((TotalGameSeconds < this.deathTimeTotalSeconds + .5) && (this.deathTimeTotalSeconds < TotalGameSeconds))
                {
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2 + 1, this.entity.Position.Y - entityYOffset - 1), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex);
                    batch.DrawString(font, score.ToString(), new Vector2(this.entity.Position.X - font.MeasureString(score.ToString()).X / 2, this.entity.Position.Y - entityYOffset), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerIndex - 0.1f);
                }
            }
        }
    }
}
