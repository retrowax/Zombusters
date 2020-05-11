using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Localization;
using Microsoft.Xna.Framework.Input;
using GameAnalyticsSDK.Net;

namespace ZombustersWindows
{
    public class DemoEndingScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Texture2D btnB;
        Texture2D btnA;
        Texture2D Title;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;
        Texture2D submit_button;
        Texture2D kbEsc;
        Texture2D kbEnter;
        private MenuComponent menu;
        //private BloomComponent bloom;
        ScrollingTextManager mText;
        String Texto;

        public DemoEndingScreen()
        {
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            titleBounds = new Rectangle(115, 65, 1000, 323);
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");
            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
            menu.Initialize();
            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);
            menu.MenuCanceled += new EventHandler<MenuSelection>(Menu_MenuCanceled);
            menu.CenterInXLeftMenu(view);
            System.IO.Stream stream = TitleContainer.OpenStream(@"LevelXMLs\Credits.txt");
            System.IO.StreamReader sreader = new System.IO.StreamReader(stream);
            Texto = sreader.ReadToEnd();
            sreader.Dispose();
            stream.Dispose();
            //bloom.Visible = !bloom.Visible;
            base.Initialize();
            this.isBackgroundOn = true;

            GameAnalytics.AddDesignEvent("ScreenView:Extras:DemoEnding:View");
        }

        void Menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            ExitScreen();
        }

        public override void LoadContent()
        {
            Title = this.ScreenManager.Game.Content.Load<Texture2D>("title");
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");
            btnA = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonA");
            kbEsc = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_esc");
            kbEnter = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_enter");
            submit_button = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/submit_button_mobile");
            mText = new ScrollingTextManager(new Rectangle(0, 280, 1280, 440), MenuInfoFont, Texto);
            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            foreach (GestureSample gesture in input.GetGestures())
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        ExitScreen();
                    }
                }
            }
            if (input.IsNewKeyPress(Keys.Enter) || input.IsNewButtonPress(Buttons.A))
            {
                GameAnalytics.AddDesignEvent("Demo:EndingScreen:AddToWishlist");
                System.Diagnostics.Process.Start("https://store.steampowered.com/app/1272300/Zombusters/");
            }
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen)
            {
                menu.Update(gameTime);
            }
            mText.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;
            base.Draw(gameTime);
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
            this.ScreenManager.SpriteBatch.Draw(Title, titleBounds, Color.White);

            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.CreditsThanksForPlayingString, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.CreditsThanksForPlayingString).X / 2 - 60, uiBounds.Center.Y - 50), Color.White, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1);

            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.DemoEndScreenLine1, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.DemoEndScreenLine1).X / 2, uiBounds.Center.Y), Color.White);
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.DemoEndScreenLine2, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.DemoEndScreenLine2).X / 2, uiBounds.Center.Y + 25), Color.White);
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.DemoEndScreenLine3, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.DemoEndScreenLine3).X / 2, uiBounds.Center.Y + 75), Color.White);
            this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.DemoEndScreenLine4, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.DemoEndScreenLine4).X / 2, uiBounds.Center.Y + 125), Color.White);

            if (((MyGame)this.ScreenManager.Game).currentInputMode != InputMode.Touch)
            {
                if (((MyGame)this.ScreenManager.Game).currentInputMode == InputMode.Keyboard)
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

                if (((MyGame)this.ScreenManager.Game).currentInputMode == InputMode.Keyboard)
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                    this.ScreenManager.SpriteBatch.Draw(kbEnter, new Vector2(158 + distanceBetweenButtonsText, 613), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                }
                else
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                    this.ScreenManager.SpriteBatch.Draw(btnA, new Vector2(158 + distanceBetweenButtonsText, 613), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, Strings.WishListGameMenu, new Vector2(158 + spaceBetweenButtonAndText + distanceBetweenButtonsText, 613 + 4), Color.White);
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
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
        }
    }
}
