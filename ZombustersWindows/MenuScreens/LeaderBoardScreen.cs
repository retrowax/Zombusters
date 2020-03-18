using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class LeaderBoardScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnB;
        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar
        Texture2D kbEsc;
        Texture2D submit_button;

        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        // Array to hold one page. Declare as a member, to avoid allocating a new one each time:
        TopScoreEntry[] page;
        int pageIndex;

        public LeaderBoardScreen()
        {
        }

        private MenuComponent menu;
        //private BloomComponent bloom;

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

            titleBounds = new Rectangle(115, 65, 1000, 323);

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);

            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);

            //Initialize Main Menu
            menu.Initialize();

            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            
            //Posiciona el menu
            menu.CenterInXLeftMenu(view);
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            this.PresenceMode = GamerPresenceMode.AtMenu;
#endif

            //bloom.Visible = !bloom.Visible;

            // Array to hold one page. Declare as a member, to avoid allocating a new one each time:
            page = new TopScoreEntry[10];
            pageIndex = 0;
            if (((MyGame)this.ScreenManager.Game).topScoreListContainer != null)
            {
                ((MyGame)this.ScreenManager.Game).topScoreListContainer.fillPageFromFullList(0, pageIndex, page);
            }

            base.Initialize();

            this.isBackgroundOn = true;
        }


        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            ExitScreen();
        }
        
        public override void LoadContent()
        {
#if WINDOWS
            //Key "Scape"
            kbEsc = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_esc");
#endif

            //Button "B"
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Linea blanca separatoria
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");

            submit_button = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/submit_button_mobile");

#if XBOX
            ((Game1)this.ScreenManager.Game).gamerManager.Load();
#endif

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Go Back Button
                    if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        // If they hit B or Back, go back to Menu Screen
                        ExitScreen();
                    }
                }
            }
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            // Menu Update
            if (!coveredByOtherScreen
#if !WINDOWS && !NETCOREAPP
                && !Guide.IsVisible
#endif
                )
            {
                menu.Update(gameTime);
            }

#if XBOX
            // Gamer Manager update
            if (((Game1)this.ScreenManager.Game).gamerManager.GamerListHasChanged())
            {
                ((Game1)this.ScreenManager.Game).gamerManager.Update();
            }
#endif

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;
            int count = 1;
            

#if WINDOWS_PHONE
            Vector2 pos = new Vector2(uiBounds.X + 60, uiBounds.Bottom);
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 225);
            Vector2 scorespos = new Vector2(selectPos.X, selectPos.Y - 235);
#else
            Vector2 pos = selectPos;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);
            Vector2 scorespos = new Vector2(selectPos.X, selectPos.Y - 265);
#endif

            base.Draw(gameTime);

            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Logo Menu
            this.ScreenManager.SpriteBatch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            // EXTRAS fade rotated
            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, Strings.ExtrasMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);

            // Texto de LeaderBoards
            this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, Strings.LeaderboardMenuString.ToUpper(), MenuTitlePosition, Color.White);

#if !WINDOWS_PHONE
            // GLOBAL / LOCAL / FRIENDS
            switch (pageIndex)
            {
                case 0:
                    this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, "GLOBAL", new Vector2(scorespos.X + 80 + MenuInfoFont.MeasureString("GLOBAL").X/2, scorespos.Y - 60),
                        new Color(255, 255, 255, 40), 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1.0f);
                    break;
                case 1:
                    this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, "LOCAL", new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                        new Color(255, 255, 255, 40), 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
                    break;
                case 2:
                    this.ScreenManager.SpriteBatch.DrawString(MenuHeaderFont, "FRIENDS", new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                        new Color(255, 255, 255, 40), 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
                    break;
            }

            // Score String
            this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.LeaderboardScoreString, new Vector2(scorespos.X + 165 - MenuInfoFont.MeasureString(Strings.LeaderboardScoreString).X, scorespos.Y - 30), Color.White);
#endif

            // LT - RT String
            //this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "<  LT", new Vector2(scorespos.X - 30, scorespos.Y - 30), Color.White);
            //this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "RT  >", new Vector2(scorespos.X + 390, scorespos.Y - 30), Color.White);

            // Linea divisoria
            pos.X -= 40;
            pos.Y -= 270;
            this.ScreenManager.SpriteBatch.Draw(lineaMenu, pos, Color.White);
            pos.Y += 270;

            // Display Leaderbord Entries
            foreach (TopScoreEntry entry in page)
            {
                if (entry != null)
                {
                    // do something with the entry here, to display it
                    this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, count.ToString() + ".", new Vector2(scorespos.X - MenuInfoFont.MeasureString(count.ToString()).X, scorespos.Y), Color.White);
                    this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, entry.Score.ToString(), new Vector2(scorespos.X + 165 - MenuInfoFont.MeasureString(entry.Score.ToString()).X, scorespos.Y), Color.White);
                    this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, entry.Gamertag, new Vector2(scorespos.X + 250, scorespos.Y), Color.White);
                    scorespos.Y += 25;
                    count++;
                }
            }

            //Linea divisoria
            pos.Y -= 15;
            this.ScreenManager.SpriteBatch.Draw(lineaMenu, pos, Color.White);

            // Leave Button
            if (((MyGame)this.ScreenManager.Game).player1.Options != InputMode.Touch)
            {
                if (((MyGame)this.ScreenManager.Game).player1.Options == InputMode.Keyboard)
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                    this.ScreenManager.SpriteBatch.Draw(kbEsc, new Vector2(158 + distanceBetweenButtonsText, 613), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                }
                else
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                    this.ScreenManager.SpriteBatch.Draw(btnB, new Vector2(158 + distanceBetweenButtonsText, 613), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.LeaveMenuString, new Vector2(158 + spaceBetweenButtonAndText + distanceBetweenButtonsText, 613 + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(MenuInfoFont.MeasureString(Strings.LeaveMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
            }
            else
            {
                Vector2 position = new Vector2(158, 613);
                this.ScreenManager.SpriteBatch.Draw(submit_button, position, Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.LeaveMenuString.ToUpper(),
                    new Vector2(position.X + 2 + submit_button.Width / 2 - MenuInfoFont.MeasureString(Strings.LeaveMenuString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.LeaveMenuString.ToUpper(),
                    new Vector2(position.X + submit_button.Width / 2 - MenuInfoFont.MeasureString(Strings.LeaveMenuString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            }

            this.ScreenManager.SpriteBatch.End();

            // Draw Retrowax Logo
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);

#if WINDOWS_PHONE
            menu.DrawBackButtonMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width + 55, uiBounds.Y - 30), MenuInfoFont);
            if (Guide.IsTrialMode)
            {
                menu.DrawBuyNow(gameTime);
            }
#endif
        }
    }

}
