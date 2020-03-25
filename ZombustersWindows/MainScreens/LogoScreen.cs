using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace ZombustersWindows
{
    public class LogoScreen : BackgroundScreen {
        MyGame game;
        float logoFadeValue = 1;         // Current Value of the Fade for the Logo
        float logoFadeSpeed = 200.0f; // Speed at which the Fade will happen
        Vector2 viewportCentre;
        Vector2 logoCentre;         // The Centre of the Logo Texture
        float logoRotationAngle;  // The Rotation Angle of the Texture Image
        Texture2D logo;
        bool FadeOut = false;
        bool isPlayedSound = false;
        float timer;

        public LogoScreen() {
        }

        public override void Initialize() {
            base.Initialize();
            this.game = (MyGame)this.ScreenManager.Game;
            this.isBackgroundOn = true;
            viewportCentre = new Vector2(this.ScreenManager.GraphicsDevice.Viewport.Width / 2, this.ScreenManager.GraphicsDevice.Viewport.Height / 2);
            logoRotationAngle = 0.0f;
        }

        public override void LoadContent() {
            logo = this.ScreenManager.Game.Content.Load<Texture2D>("LogoRetroWax");
            logoCentre = new Vector2(logo.Width / 2, logo.Height / 2);
            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!isPlayedSound) {
                if (MediaPlayer.GameHasControl) {
                    game.audio.PlaySplashSound();
                    isPlayedSound = true;
                }
            }

            if (this.logoFadeValue < 254 && !FadeOut) {
                this.logoFadeValue = this.logoFadeValue + (timeDelta * this.logoFadeSpeed);
            } else {
                if (!FadeOut) {
                    this.logoFadeValue = 255;
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds; 
                     if(timer >= 1.0f) {
                         FadeOut = true;
                     }
                } else {
                    this.logoFadeValue = this.logoFadeValue - (timeDelta * this.logoFadeSpeed);
                }
            }

            if (logoFadeValue <= 0) {
                FinishLogo();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        void FinishLogo() {
            this.ScreenManager.AddScreen(new StartScreen());
            game.bloom.Visible = true;
            ExitScreen();
        }

        public Color FadeColor(Color baseColor, float FadeValue) {
            Color tempColor;
            tempColor = new Color(baseColor.R, baseColor.G, baseColor.B, (byte)FadeValue);
            return tempColor;
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin();
            Color c = new Color(new Vector4(255, 255, 255, 0));
            this.ScreenManager.GraphicsDevice.Clear(c);
            batch.Draw(logo, this.viewportCentre,null, FadeColor(Color.White, logoFadeValue), 
                logoRotationAngle, logoCentre,1.0f, SpriteEffects.None, 0.0f);
            batch.End();
        }
    }

}
