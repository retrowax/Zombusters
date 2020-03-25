using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class MenuScreen : BackgroundScreen {
        MyGame game;
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;
        Texture2D logoMenu;
        Texture2D lineaMenu;
        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;
        SpriteFont fontItalic, fontSmallItalic;
        private MenuComponent menu;

        public MenuScreen() {
            EnabledGestures = GestureType.Tap;
        }

        public override void Initialize() {
            game = (MyGame)this.ScreenManager.Game;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            titleBounds = new Rectangle(0, 0, 1280, 720);
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuHeaderFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
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
            //menu.AddText(Strings.ReviewMenuString, Strings.ReviewMMString);
            /*if (licenseInformation.IsTrial)
            {
                menu.AddText(Strings.UnlockFullGameMenuString").ToUpper(), Strings.UnlockFullGameMenuString"));
            }*/
            menu.AddText("QuitGame", "QuitGame");

            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);
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

                /*case 3:
                    //await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=44468RetrowaxGames.Zombusters_rhyy9bbdeb2be"));
                    break;*/

                case 3:
                    /*if (licenseInformation.IsTrial)
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:PDP?PFN=44468RetrowaxGames.Zombusters_rhyy9bbdeb2be"));
                    }*/

                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(Strings.QuitGame, Strings.ConfirmQuitGame);
                    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
                    ScreenManager.AddScreen(confirmExitMessageBox);
                    break;
                default:
                    break;
            }
        }
        
        public override void LoadContent() {
            lineaMenu = game.Content.Load<Texture2D>(@"menu/linea_menu");
            logoMenu = game.Content.Load<Texture2D>(@"menu/logo_menu");
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
                menu.DrawBuyNow(gameTime);
                //menu.DrawSocialLogosMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width - 200, uiBounds.Height), MenuInfoFont);
                menu.Draw(gameTime);
                menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
                //menu.DrawDreamBuildPlayDisclaimer(this.ScreenManager.SpriteBatch, fontItalic, fontSmallItalic);
                DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
            } else {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                this.ScreenManager.SpriteBatch.Draw(game.blackTexture, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused.ToUpper(), new Vector2(5, 5), Color.White);
                this.ScreenManager.SpriteBatch.End();
            }
        }

        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch) {
            string[] lines;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);
            batch.DrawString(MenuHeaderFont, Strings.MainMenuString, MenuTitlePosition, Color.White);
            pos.X -= 40;
            pos.Y -= 270;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;
            /*if (licenseInformation.IsTrial)
            {
                pos.Y -= 105;
                batch.Draw(lineaMenu, pos, Color.White);
                pos.Y += 105;
            }
            else*/
            {
                pos.Y -= 115;
                batch.Draw(lineaMenu, pos, Color.White);
                pos.Y += 115;
            }
            lines = Regex.Split(Strings.ResourceManager.GetString(menu.HelpText[menu.Selection]), "\r\n");
            foreach (string line in lines) {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);
            menu.DrawMenuButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont, false, false, true);
            batch.End();
        }
    }
}
