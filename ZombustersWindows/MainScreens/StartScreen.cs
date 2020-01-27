using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class StartScreen : BackgroundScreen {
        MyGame game;
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 startPos;
        Vector2 startOrigin;
        Vector2 rightsPos;
        Vector2 rightsOrigin;
        Texture2D title;
        SpriteFont font;
        PlayerIndex playerOne;
        bool Exiting = false;

        public StartScreen() {
            
        }

        public override void Initialize() {
            base.Initialize();
            this.game = (MyGame)this.ScreenManager.Game;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            titleBounds = new Rectangle(115, 65, 1000, 323);
            startPos = new Vector2(uiBounds.X + uiBounds.Width / 2,  uiBounds.Height * .75f);
            rightsPos = new Vector2(uiBounds.X + uiBounds.Width / 2, uiBounds.Height + 50);
            game.musicComponent.Enabled = true;
            game.musicComponent.StartPlayingMusic(6);
            game.isInMenu = false;
            this.isBackgroundOn = true;
        }

        public override void LoadContent() {
            title = game.Content.Load<Texture2D>("title");
            font = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            if (game.player1.Options == InputMode.GamePad) {
                startOrigin = font.MeasureString(Strings.PressStartString) / 2;
            } else if (game.player1.Options == InputMode.Keyboard) {
                startOrigin = font.MeasureString(Strings.PCPressAnyKeyString) / 2;
            }
            rightsOrigin = font.MeasureString(Strings.CopyRightString) / 2;
            base.LoadContent();
        }

        public override void HandleInput(InputState input) {
            if (!Exiting && InputManager.CheckPlayerOneStart(out playerOne, input)) {
                Exiting = true;
                game.TrySignIn(true, FinishStart);
            }
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        void FinishStart(Object sender, EventArgs e) {
            MessageBoxScreen confirmExitMessageBox =new MessageBoxScreen(Strings.AutosaveTitleMenuString, Strings.AutosaveStartMenuString, false);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            confirmExitMessageBox.Cancelled += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox);
        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e) {
            game.InitializeMain(playerOne);
            this.ScreenManager.AddScreen(new MenuScreen());
            ExitScreen();
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            SpriteBatch batch = this.ScreenManager.SpriteBatch;
            batch.Begin();
            batch.Draw(title, titleBounds, Color.White);
            float interval = 2.0f; // Two second interval
            float value = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * interval);
            value = (value + 1) / 2;  // Shift the sine wave into positive 
            Color color = new Color(value, value, value, value);
            if (game.player1.Options == InputMode.GamePad) {
                batch.DrawString(font, Strings.PressStartString, startPos, color, 0, startOrigin, 1.0f, SpriteEffects.None, 0.1f);
            } else if (game.player1.Options == InputMode.Keyboard) {
                batch.DrawString(font, Strings.PCPressAnyKeyString, startPos, color, 0, startOrigin, 1.0f, SpriteEffects.None, 0.1f);
            }
            batch.DrawString(font, Strings.CopyRightString, rightsPos, Color.White, 0, rightsOrigin, 1.0f, SpriteEffects.None, 0.1f);
            batch.End();
        }
    }
}
