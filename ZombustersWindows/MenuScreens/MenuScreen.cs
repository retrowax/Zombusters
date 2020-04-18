using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Localization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class MenuScreen : BackgroundScreen {
        MyGame game;
        Rectangle uiBounds;
        Vector2 selectPos;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;
        SpriteFont fontItalic;
        SpriteFont fontSmallItalic;
        private MenuComponent menu;

        public MenuScreen() {
            EnabledGestures = GestureType.Tap;
        }

        public override void Initialize() {
            game = (MyGame)this.ScreenManager.Game;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuList");
            fontItalic = game.Content.Load<SpriteFont>(@"menu\ArialMusic");
            fontSmallItalic = game.Content.Load<SpriteFont>(@"menu\ArialMusicItalic");

            menu = new MenuComponent(game, MenuListFont);
            //bloom = new BloomComponent(game);
            menu.Initialize();
            menu.AddText("WPPlayNewGame", "WPPlayNewGameMMString");
            menu.AddText("ExtrasMenuString", "ExtrasMMString");
            menu.AddText("SettingsMenuString", "ConfigurationString");
#if DEMO
            menu.AddText("ReviewMenuString", "ReviewMMString");
#endif
            //menu.AddText(Strings.ReviewMenuString, Strings.ReviewMMString);
            /*if (licenseInformation.IsTrial)
            {
                menu.AddText(Strings.UnlockFullGameMenuString").ToUpper(), Strings.UnlockFullGameMenuString"));
            }*/
            menu.AddText("QuitGame", "QuitGame");

            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, uiBounds.Bottom / 2);
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(OnMenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(OnMenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(OnMenuConfigSelected);
            //menu.MenuShowMarketplace += new EventHandler<MenuSelection>(OnShowMarketplace);
            menu.CenterInXLeftMenu(view);
            //bloom.Visible = !bloom.Visible;
            game.isInMenu = true;
            base.Initialize();
            this.isBackgroundOn = true;
        }

        void OnMenuCanceled(Object sender, MenuSelection selection) {

        }

        void OnMenuConfigSelected(Object sender, MenuSelection selection) {

        }

        void OnShowMarketplace(Object sender, MenuSelection selection) {

        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e) {
            game.Exit();
            // WARNING: This is a workaround because the game is not exiting correctly
            Environment.Exit(0);
        }

        void OnMenuOptionSelected(Object sender, MenuSelection selection) {
            switch (selection.Selection) {
                case 0:
                    game.BeginSelectPlayerScreen(false);
                    break;

                case 1:
                    game.DisplayExtrasMenu();
                    break;

                case 2:
                    game.DisplayOptions(0);
                    break;
#if DEMO
                case 3:
                    //await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=44468RetrowaxGames.Zombusters_rhyy9bbdeb2be"));
                    System.Diagnostics.Process.Start("http://google.com");
                    break;

                case 4:
                    ShowConfirmExitMessageBox();
                    break;
#else
                case 3:
                    ShowConfirmExitMessageBox();
                    break;
#endif
                default:
                    break;
            }
        }
        
        public override void LoadContent() {
            base.LoadContent();
        }

        public override void HandleInput(InputState input) {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if (game.currentGameState != GameState.Paused) {
                if (!coveredByOtherScreen) {
                    menu.Update(gameTime);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            if (game.currentGameState != GameState.Paused) {
                //menu.DrawBuyNow(gameTime);
                //menu.DrawSocialLogosMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width - 200, uiBounds.Height), MenuInfoFont);
                menu.Draw(gameTime);
                menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
#if DEMO
                menu.DrawDemoWIPDisclaimer(this.ScreenManager.SpriteBatch);
#endif
                menu.DrawContextMenu(selectPos, this.ScreenManager.SpriteBatch);
            } else {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
                this.ScreenManager.SpriteBatch.Draw(game.blackTexture, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused.ToUpper(), new Vector2(5, 5), Color.White);
                this.ScreenManager.SpriteBatch.End();
            }
        }

        private void ShowConfirmExitMessageBox()
        {
            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.QuitGame, Strings.ConfirmQuitGame);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox);
        }
    }
}
