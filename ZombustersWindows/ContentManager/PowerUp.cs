using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.ContentManagement;

namespace ZombustersWindows.Subsystem_Managers
{

    public class PowerUp
    {
        public Texture2D Texture, UITexture;
        public Vector2 Position;
        public int Value;
        public ObjectStatus status;
        public PowerUpType powerUpType;
        public float buffTimer;

        private float timer;
        private float dyingtimer;
        private Color color;

        public PowerUp(Texture2D texture, Texture2D uitexture, Vector2 position, PowerUpType type)
        {
            this.Texture = texture;
            this.UITexture = uitexture;
            this.Position = position;
            this.status = ObjectStatus.Active;
            this.color = Color.White;
            this.powerUpType = type;

            if (this.powerUpType == PowerUpType.extralife)
            {
                Value = 1;
            }

            if (this.powerUpType == PowerUpType.live)
            {
                Value = 30;
            }

            if (this.powerUpType == PowerUpType.shotgun)
            {
                Value = 50;
            }

            if (this.powerUpType == PowerUpType.machinegun)
            {
                Value = 50;
            }

            if (this.powerUpType == PowerUpType.flamethrower)
            {
                Value = 50;
            }

            if (this.powerUpType == PowerUpType.grenade)
            {
                Value = 10;
            }

            if (this.powerUpType == PowerUpType.speedbuff)
            {
                // Timer
                buffTimer = 20.0f;
            }

            if (this.powerUpType == PowerUpType.immunebuff)
            {
                // Timer
                buffTimer = 20.0f;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this.status == ObjectStatus.Active)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 20.0f)
                {
                    this.status = ObjectStatus.Inactive;
                    timer = 0;
                }
            }

            if (this.status == ObjectStatus.Dying)
            {
                dyingtimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (dyingtimer >= 1.5f)
                {
                    this.status = ObjectStatus.Inactive;
                    dyingtimer = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, SpriteFont font)
        {
            String textToShow;
            Vector2 texturePosition;
            Vector2 startPosition;
            SpriteBatch batch = spriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            if (this.status == ObjectStatus.Active)
            {
                if (timer >= 10.0f)
                {
                    float interval = 10.0f; // Two second interval
                    float value = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds
                        * interval);
                    value = (value + 1) / 2;  // Shift the sine wave into positive 
                    // territory, then back to 0-1 range
                    this.color = new Color(value, value, value, value);
                }
                else
                {
                    this.color = Color.White;
                }

                batch.Draw(this.Texture, this.Position, this.color);
            }

            if (this.status == ObjectStatus.Dying)
            {
                if (this.powerUpType == PowerUpType.live)
                {
                    this.color = Color.Red;
                }

                if (this.powerUpType == PowerUpType.shotgun)
                {
                    this.color = Color.LightYellow;
                }

                if (this.powerUpType == PowerUpType.machinegun)
                {
                    this.color = Color.LightYellow;
                }

                if (this.powerUpType == PowerUpType.flamethrower)
                {
                    this.color = Color.LightYellow;
                }

                if (this.powerUpType == PowerUpType.grenade)
                {
                    this.color = Color.LightYellow;
                }

                if (this.powerUpType == PowerUpType.speedbuff)
                {
                    this.color = new Color(95, 172, 226, 255);
                }

                if (this.powerUpType == PowerUpType.immunebuff)
                {
                    this.color = Color.BlueViolet;
                }

                if (this.powerUpType == PowerUpType.immunebuff || this.powerUpType == PowerUpType.speedbuff)
                {
                    textToShow = "+ " + Convert.ToInt32(this.buffTimer).ToString() + "s";
                }
                else
                {
                    textToShow = "+ " + this.Value.ToString();
                }

                startPosition = new Vector2(this.Position.X - (font.MeasureString(textToShow).X / 2), this.Position.Y);
                texturePosition = new Vector2(startPosition.X + font.MeasureString(textToShow).X + 2, startPosition.Y);

                batch.DrawString(font, textToShow, new Vector2(startPosition.X + 1, startPosition.Y + 1), Color.Black);
                batch.DrawString(font, textToShow, startPosition, color);
                batch.Draw(this.UITexture, texturePosition, Color.White);
            }

            batch.End();
        }
    }
}
