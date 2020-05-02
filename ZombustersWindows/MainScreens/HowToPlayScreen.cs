using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Localization;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class HowToPlayScreen : BackgroundScreen
    {
        Rectangle uiBounds;
        Rectangle titleBounds;
        Vector2 selectPos;

        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnX;
        Texture2D btnY;
        Texture2D btnStart;
        Texture2D btnLB;
        Texture2D btnRB;
        Texture2D btnDPad;
        Texture2D LeftThumbstick;
        Texture2D RightThumbstick;
        Texture2D submit_button;
        Texture2D kbEsc;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar
        Texture2D controller;
        Texture2D inputNormal;
        Texture2D HTPHeaderImage;

        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        private MenuComponent menu;

        public HowToPlayScreen()
        {
        }

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
            menu.Initialize();
            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);
            menu.MenuCanceled += new EventHandler<MenuSelection>(MenuCanceled);
            menu.CenterInXLeftMenu(view);
            base.Initialize();

            this.isBackgroundOn = true;
        }

        public override void LoadContent()
        {
            //Key "Scape"
            kbEsc = this.ScreenManager.Game.Content.Load<Texture2D>(@"Keyboard/key_esc");

            //Button "A"
            btnA = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            //Button "X"
            btnX = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonX");

            //Button "Y"
            btnY = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonY");

            //Button "Select"
            btnStart = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerStart");

            //Button "LB"
            btnLB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerLeftShoulder");

            //Button "RB"
            btnRB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerRightShoulder");

            //Button "DPad"
            btnDPad = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerDPad");

            //Left Thumbstick
            LeftThumbstick = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerLeftThumbstick");

            //Right Thumbstick
            RightThumbstick = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerRightThumbstick");

            //Linea blanca separatoria
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");

            //Controller
            controller = this.ScreenManager.Game.Content.Load<Texture2D>("controller");

            //Input Normal
            inputNormal = this.ScreenManager.Game.Content.Load<Texture2D>("InputText");

            submit_button = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/submit_button_mobile");

            // HTP Header Image
            HTPHeaderImage = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/lobby_header_image");

            base.LoadContent();
        }

        void MenuCanceled(Object sender, MenuSelection selection)
        {
            ExitScreen();
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
            if (!coveredByOtherScreen)
            {
                menu.Update(gameTime);
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

            if (((MyGame)this.ScreenManager.Game).currentInputMode == InputMode.GamePad)
            {
                DrawController(this.ScreenManager.SpriteBatch, uiBounds);
                DrawControllerSchemeDetails(this.ScreenManager.SpriteBatch);
            }

            this.ScreenManager.SpriteBatch.End();

            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
#if DEMO
            menu.DrawDemoWIPDisclaimer(this.ScreenManager.SpriteBatch);
#endif
            DrawContextMenu(selectPos, this.ScreenManager.SpriteBatch);
        }

        private Vector2 DrawController(SpriteBatch batch, Rectangle UIbounds)
        {
            Vector2 CenterTop = new Vector2(UIbounds.Width - 150,
                UIbounds.Height / 4 + UIbounds.Top);
            Vector2 origin = new Vector2(controller.Width / 2, controller.Height / 2);
            Vector2 textposorigin = new Vector2(CenterTop.X - controller.Width / 2 + 80, CenterTop.Y - controller.Height / 2 + 20);

            // Draw Controller
            batch.Draw(controller, CenterTop, null, Color.White, 0,
                origin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw Input Text Lines
            batch.Draw(inputNormal, CenterTop, null, Color.White, 0,
                    origin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw Input Text 
            batch.DrawString(MenuInfoFont, Strings.HTPChangeSongString, textposorigin, Color.White);
            batch.DrawString(MenuInfoFont, Strings.HTPMoveString, new Vector2(textposorigin.X + 25, textposorigin.Y + 38), Color.White);
            //batch.DrawString(MenuInfoFont, Strings.HTPTauntString, new Vector2(textposorigin.X + 20, textposorigin.Y + 132), Color.White);
            //batch.DrawString(MenuInfoFont, Strings.HTPGrenadeString, new Vector2(textposorigin.X + 440, textposorigin.Y), Color.White);
            batch.DrawString(MenuInfoFont, Strings.HTPChangeWeaponString, new Vector2(textposorigin.X + 410, textposorigin.Y + 26), Color.White);
            //batch.DrawString(MenuInfoFont, Strings.HTPReviveTeammateString, new Vector2(textposorigin.X + 400, textposorigin.Y + 84), Color.White);
            batch.DrawString(MenuInfoFont, Strings.HTPAimShootString, new Vector2(textposorigin.X + 420, textposorigin.Y + 128), Color.White);


            return CenterTop;
        }

        private void DrawControllerSchemeDetails(SpriteBatch batch)
        {
            Vector2 position = new Vector2(uiBounds.Center.X + 250, uiBounds.Center.Y - 25);
            int separationheight = Convert.ToInt32(MenuInfoFont.MeasureString(Strings.HTPMoveString).Y) + 10;

            // Change Song
            batch.DrawString(MenuInfoFont, Strings.HTPChangeSongString, position, Color.White);
            batch.Draw(btnLB, new Vector2(position.X - 75, position.Y - 3), null, Color.White, 0, Vector2.Zero, 0.28f, SpriteEffects.None, 1.0f);

            // Move
            batch.DrawString(MenuInfoFont, Strings.HTPMoveString, new Vector2(position.X, position.Y + separationheight), Color.White);
            batch.Draw(LeftThumbstick, new Vector2(position.X - 40, position.Y + separationheight - 3), null, Color.White, 0, Vector2.Zero, 0.17f, SpriteEffects.None, 1.0f);

            // Taunt
            //batch.DrawString(MenuInfoFont, Strings.HTPTauntString, new Vector2(position.X, position.Y + separationheight * 2), Color.White);
            //batch.Draw(btnDPad, new Vector2(position.X - 43, position.Y + separationheight * 2 - 3), null, Color.White, 0, Vector2.Zero, 0.17f, SpriteEffects.None, 1.0f);

            // Grenade
            //batch.DrawString(MenuInfoFont, Strings.HTPGrenadeString, new Vector2(position.X, position.Y + separationheight * 3), Color.White);
            //batch.Draw(btnRB, new Vector2(position.X - 75, position.Y + separationheight * 3 - 3), null, Color.White, 0, Vector2.Zero, 0.28f, SpriteEffects.None, 1.0f);

            // Change Weapon
            batch.DrawString(MenuInfoFont, Strings.HTPChangeWeaponString, new Vector2(position.X, position.Y + separationheight * 2), Color.White);
            batch.Draw(btnY, new Vector2(position.X - 40, position.Y + separationheight * 2 - 3), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);

            // Revive Teammate
            //batch.DrawString(MenuInfoFont, Strings.HTPReviveTeammateString.ToUpper(), new Vector2(position.X, position.Y + separationheight * 5), Color.White);
            //batch.Draw(btnX, new Vector2(position.X - 40, position.Y + separationheight * 5 - 3), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);

            // AIM + SHOOT
            batch.DrawString(MenuInfoFont, Strings.HTPAimShootString, new Vector2(position.X, position.Y + separationheight * 3), Color.White);
            batch.Draw(RightThumbstick, new Vector2(position.X - 40, position.Y + separationheight * 3 - 3), null, Color.White, 0, Vector2.Zero, 0.17f, SpriteEffects.None, 1.0f);

            // Pause
            batch.DrawString(MenuInfoFont, Strings.HTPPauseString, new Vector2(position.X, position.Y + separationheight * 4), Color.White);
            batch.Draw(btnStart, new Vector2(position.X - 40, position.Y + separationheight * 4 - 3), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);

        }

        private void DrawContextMenu(Vector2 pos, SpriteBatch batch)
        {
            string[] lines;

            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);
            batch.DrawString(MenuHeaderFont, Strings.ExtrasMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
            batch.DrawString(MenuHeaderFont, Strings.HowToPlayInGameString.ToUpper(), MenuTitlePosition, Color.White);
            batch.Draw(HTPHeaderImage, new Vector2(MenuTitlePosition.X, MenuTitlePosition.Y + 75), Color.White);

            pos.X -= 40;
            pos.Y -= 115;
            batch.Draw(lineaMenu, new Vector2(pos.X, pos.Y - 40), Color.White);

            contextMenuPosition = new Vector2(pos.X, pos.Y - 30);
            if (((MyGame)this.ScreenManager.Game).currentInputMode == InputMode.Keyboard)
            {
                lines = Regex.Split(Strings.PCExplanationString, "\r\n");
            }
            else
            {
                lines = Regex.Split(Strings.HTPExplanationString, "\r\n");
            }
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            pos.Y += 100;
            batch.Draw(lineaMenu, pos, Color.White);

            batch.End();
        }
    }
}
