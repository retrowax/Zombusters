using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class DemoEndingScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;
        Texture2D btnB;
        Texture2D Title;
        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;
        Texture2D submit_button;
        Texture2D kbEsc;
        private MenuComponent menu;
        //private BloomComponent bloom;
        ScrollingTextManager mText;
        String Texto;
        bool canLeaveScreen;

        public DemoEndingScreen()
        {
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            titleBounds = new Rectangle(115, 65, 1000, 323);
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");
            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
            menu.Initialize();
            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            menu.CenterInXLeftMenu(view);
            System.IO.Stream stream = TitleContainer.OpenStream(@"LevelXMLs\Credits.txt");
            System.IO.StreamReader sreader = new System.IO.StreamReader(stream);
            Texto = sreader.ReadToEnd();
            sreader.Dispose();
            stream.Dispose();
            //bloom.Visible = !bloom.Visible;
            base.Initialize();
            this.isBackgroundOn = true;
        }

        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            if (canLeaveScreen == true)
            {
                ExitScreen();
            }
        }

        public override void LoadContent()
        {
            Title = this.ScreenManager.Game.Content.Load<Texture2D>("title");
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");
            kbEsc = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_esc");
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
                        if (canLeaveScreen == true)
                        {
                            ExitScreen();
                        }
                    }
                }
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
            if (mText.endOfLines == true)
            {
                canLeaveScreen = true;
            }
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
            mText.Draw(this.ScreenManager.SpriteBatch);
            if (canLeaveScreen)
            {
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
            }

            if (mText.endOfLines)
            {
                this.ScreenManager.SpriteBatch.DrawString(MenuListFont, Strings.CreditsThanksForPlayingString, new Vector2(uiBounds.Center.X - MenuListFont.MeasureString(Strings.CreditsThanksForPlayingString).X / 2, uiBounds.Center.Y + 50), Color.White);
            }
            this.ScreenManager.SpriteBatch.End();
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
        }
    }
}
