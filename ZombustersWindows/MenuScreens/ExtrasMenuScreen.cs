using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using GameAnalyticsSDK.Net;

namespace ZombustersWindows
{
    public class ExtrasMenuScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Vector2 selectPos;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        private MenuComponent menu;

        public ExtrasMenuScreen()
        {
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

#if WINDOWS_PHONE
            titleBounds = new Rectangle(0, 0, 800, 480);
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom);
#else
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
#endif
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

#if WINDOWS_PHONE
            menu = new MenuComponent(this.ScreenManager.Game, MenuHeaderFont);
#else
            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
#endif

            //bloom = new BloomComponent(this.ScreenManager.Game);
            menu.Initialize();
            menu.AddText("HowToPlayInGameString", "HTPMenuSupportString");  // How To Play
            menu.AddText("LeaderboardMenuString", "LeaderboardMMString");    // Leaderboard
            menu.AddText("CreditsMenuString", "CreditsMMString");            // Credits
            menu.AddText("StoreMenuString", "StoreMMString");

            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);

            menu.MenuOptionSelected += new EventHandler<MenuSelection>(Menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(Menu_MenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(Menu_MenuConfigSelected);

            menu.CenterInXLeftMenu(view);
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            this.PresenceMode = GamerPresenceMode.AtMenu;
#endif

            //bloom.Visible = !bloom.Visible;

            base.Initialize();

            this.isBackgroundOn = true;
        }


        void Menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            // If they hit B or Back, go back to Menu Screen
            ScreenManager.AddScreen(new MenuScreen());
        }


        void Menu_MenuConfigSelected(Object sender, MenuSelection selection)
        {
            // If they hit Start, go to Configuration Screen
            //SignedInGamer gamer;
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.One);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(0);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Two);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(1);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Three);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(2);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Four);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(3);
                }
#endif
            }
            //((Game1)this.ScreenManager.Game).DisplayOptions();
        }

        

        void Menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            
            switch (selection.Selection)
            {
                case 0: // How to Play
                    ((MyGame)this.ScreenManager.Game).DisplayHowToPlay();
                    break;

                case 1: // Leaderboards
                    ((MyGame)this.ScreenManager.Game).DisplayLeaderBoard();
                    break;

                case 2:
                    ShowMarketPlace();
                    break;

                default:
                    break;
            }

        }

        void ShowMarketPlace()
        {
            GameAnalytics.AddDesignEvent("ScreenView:Extras:Marketplace:View");
            System.Diagnostics.Process.Start("https://www.redbubble.com/es/people/retrowax/shop");
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            // Menu update
            if (!coveredByOtherScreen
#if !WINDOWS && !NETCOREAPP
                && !Guide.IsVisible
#endif
                )

            {
                menu.Update(gameTime);
            }

#if XBOX
            // Gamer Manager Update
            if (((Game1)this.ScreenManager.Game).gamerManager.GamerListHasChanged())
            {
                ((Game1)this.ScreenManager.Game).gamerManager.Update();
            }
#endif

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            menu.Draw(gameTime);
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
#if DEMO
            menu.DrawDemoWIPDisclaimer(this.ScreenManager.SpriteBatch);
#endif
            menu.DrawContextMenu(selectPos, this.ScreenManager.SpriteBatch);

#if XBOX
            // Draw "My Group" info
            ((Game1)this.ScreenManager.Game).gamerManager.Draw(menu, new Vector2(uiBounds.X + uiBounds.Width - 300, 90), this.ScreenManager.SpriteBatch, MenuInfoFont, false, null);
#endif
#if WINDOWS_PHONE
            menu.DrawPhone(gameTime);
            menu.DrawBackButtonMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width + 55, uiBounds.Y - 30), MenuInfoFont);
            if (Guide.IsTrialMode)
            {
                menu.DrawBuyNow(gameTime);
            }
            menu.DrawContextMenuWP(menu, selectPos, this.ScreenManager.SpriteBatch);
#endif
        }
    }

}
