using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using ZombustersWindows.Localization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows.MainScreens
{
    public class GameOverMenu : GameScreen
    {
        MenuComponent menu;
        public event EventHandler<MenuSelection> GameOverMenuOptionSelected;
        Texture2D gameover;
        Texture2D coffinMeme;
        Vector2 gameoverOrigin;
        Rectangle uiBounds;

        public GameOverMenu()
        {
            EnabledGestures = GestureType.Tap;
            this.IsPopup = true;
        }

        public override void Initialize()
        {
            menu = new MenuComponent(this.ScreenManager.Game, this.ScreenManager.Font, this.ScreenManager.SpriteBatch);
            menu.AddText("GPRestartCurrentWaveString"); //Restart from current Wave
            menu.AddText("GPRestartFromBeginningString"); //Restart from beginning
            menu.AddText("GPReturnToMainMenuString"); //Return to main menu
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(SelectOption);
            menu.MenuCanceled += new EventHandler<MenuSelection>(CancelMenu);
            //menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            menu.Initialize();
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            uiBounds = GetTitleSafeArea();
            menu.CenterMenu(view);
            TransitionPosition = 0.5f;
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            coffinMeme = this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/coffin_meme");
            gameover = this.ScreenManager.Game.Content.Load<Texture2D>(@"InGame/gameover");
            gameoverOrigin = new Vector2(gameover.Width / 2, gameover.Height / 2);
        }

        void CancelMenu(Object sender, MenuSelection selection)
        {
            //MenuCanceled.Invoke(this, new MenuSelection(-1));
            //ExitScreen();
        }

        void SelectOption(Object sender, MenuSelection selection)
        {
            GameOverMenuOptionSelected.Invoke(this, selection);
            ExitScreen();
        }

        public override void HandleInput(InputState input)
        {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            this.ScreenManager.FadeBackBufferToBlack(127);
            menu.Draw(gameTime);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
            this.ScreenManager.SpriteBatch.Draw(gameover, new Vector2(uiBounds.X + uiBounds.Width / 2 - 204, uiBounds.Y + 254), null, Color.Black, 0, gameoverOrigin, 1.0f, SpriteEffects.None, 1.0f);
            this.ScreenManager.SpriteBatch.Draw(gameover, new Vector2(uiBounds.X + uiBounds.Width / 2 - 200, uiBounds.Y + 250), null, Color.AntiqueWhite, 0, gameoverOrigin, 1.0f, SpriteEffects.None, 1.0f);
            this.ScreenManager.SpriteBatch.Draw(coffinMeme, new Vector2(uiBounds.X + uiBounds.Width / 2 - 75, uiBounds.Y + uiBounds.Height / 2 + 100), null, Color.White, 0, gameoverOrigin, 1.0f, SpriteEffects.None, 1.0f);
            this.ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        public Rectangle GetTitleSafeArea()
        {
            PresentationParameters pp =
                this.ScreenManager.GraphicsDevice.PresentationParameters;
            Rectangle retval =
                new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

            int offsetx = (pp.BackBufferWidth + 9) / 10;
            int offsety = (pp.BackBufferHeight + 9) / 10;

            retval.Inflate(-offsetx, -offsety);  // Deflate the rectangle
            return retval;
        }
    }
}
