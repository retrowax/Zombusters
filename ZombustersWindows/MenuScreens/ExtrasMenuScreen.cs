using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class ExtrasMenuScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnStart;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar

        SpriteFont MenuHeaderFont;
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

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom);
#else
            titleBounds = new Rectangle(0, 0, 1280, 720);

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
#endif

            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

#if WINDOWS_PHONE
            menu = new MenuComponent(this.ScreenManager.Game, MenuHeaderFont);
#else
            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
#endif

            //bloom = new BloomComponent(this.ScreenManager.Game);

            //Initialize Main Menu
            menu.Initialize();

            //Adding Options text for the Menu
            menu.AddText("HowToPlayInGameString", "HTPMenuSupportString");  // How To Play
            menu.AddText("LeaderboardMenuString", "LeaderboardMMString");    // Leaderboard
            menu.AddText("CreditsMenuString", "CreditsMMString");            // Credits

            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            //menu.SelectedColor = new Color(198,34,40);
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            //menu.MenuShowMarketplace += new EventHandler<MenuSelection>(menu_ShowMarketPlace);

            //Posiciona el menu
            menu.CenterInXLeftMenu(view);
#if !WINDOWS_PHONE && !WINDOWS
            this.PresenceMode = GamerPresenceMode.AtMenu;
#endif

            //bloom.Visible = !bloom.Visible;

            base.Initialize();

            this.isBackgroundOn = true;
        }


        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            // If they hit B or Back, go back to Menu Screen
            ScreenManager.AddScreen(new MenuScreen());
        }


        void menu_MenuConfigSelected(Object sender, MenuSelection selection)
        {
            // If they hit Start, go to Configuration Screen
            //SignedInGamer gamer;
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.One);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(0);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Two);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(1);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Three);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(2);
                }
#endif
            }
            else if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.Start))
            {
#if !WINDOWS_PHONE && !WINDOWS
                gamer = NetworkSessionManager.FindGamer(PlayerIndex.Four);
                if (gamer != null)
                {
                    ((Game1)this.ScreenManager.Game).DisplayOptions(3);
                }
#endif
            }
            //((Game1)this.ScreenManager.Game).DisplayOptions();
        }

        

        void menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            
            switch (selection.Selection)
            {
                case 0: // How to Play
                    ((MyGame)this.ScreenManager.Game).DisplayHowToPlay();
                    break;

                case 1: // Leaderboards
                    ((MyGame)this.ScreenManager.Game).DisplayLeaderBoard();
                    break;

                case 2: // Credits
                    ((MyGame)this.ScreenManager.Game).DisplayCredits();
                    break;

                default:
                    break;
            }

        }

        public override void LoadContent()
        {
            //Button "A"
            btnA = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Button "Select"
            btnStart = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerStart");

            //Linea blanca separatoria
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");

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
#if !WINDOWS
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

            // Draw Menu
#if WINDOWS_PHONE
            menu.DrawPhone(gameTime);
            menu.DrawBackButtonMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width + 55, uiBounds.Y - 30), MenuInfoFont);
            if (Guide.IsTrialMode)
            {
                menu.DrawBuyNow(gameTime);
            }
#else
            menu.Draw(gameTime);
#endif

            // Draw Retrowax Logo
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);

#if !WINDOWS_PHONE
            // Draw Context Menu
            DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
#if XBOX
            // Draw "My Group" info
            ((Game1)this.ScreenManager.Game).gamerManager.Draw(menu, new Vector2(uiBounds.X + uiBounds.Width - 300, 90), this.ScreenManager.SpriteBatch, MenuInfoFont, false, null);
#endif
#else
            //Draw Context Menu
            DrawContextMenuWP(menu, selectPos, this.ScreenManager.SpriteBatch);
#endif
        }


        //Draw all the Selection buttons on the bottom of the menu
        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            string[] lines;

            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //MAIN MENU fade rotated
            batch.DrawString(MenuHeaderFont, Strings.MainMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);

            //Texto de CREATE GAME or FIND GAME
            batch.DrawString(MenuHeaderFont, Strings.ExtrasMenuString, MenuTitlePosition, Color.White);
            

            //Linea divisoria
            pos.X -= 40;
            pos.Y -= 270;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;

            pos.Y -= 115;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 115;

            //Texto de contexto de menu
            lines = Regex.Split(Strings.ResourceManager.GetString(menu.HelpText[menu.Selection]), "\r\n");
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            //Linea divisoria
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);

            // Draw context buttons
            menu.DrawMenuButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont, false, false, false);

            batch.End();
        }

#if WINDOWS_PHONE
        //Draw all the Selection buttons on the bottom of the menu
        private void DrawContextMenuWP(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            string[] lines;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 225);

            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //Texto de MENU PRINCIPAL
            batch.DrawString(MenuHeaderFont, Strings.ExtrasMenuString, MenuTitlePosition, Color.White);

            //Linea divisoria
            pos.X -= 40;
            pos.Y -= 270;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;

            pos.Y -= 115;
            batch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 115;

            //Texto de contexto de menu
            lines = Regex.Split(menu.HelpText[menu.Selection], "\r\n");
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            batch.End();
        }
#endif
    }

}
