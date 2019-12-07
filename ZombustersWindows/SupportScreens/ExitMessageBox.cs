#region File Description
//-----------------------------------------------------------------------------
// ExitMessageBox.cs
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Localization;
#endregion

namespace ZombustersWindows
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class MessageBoxScreen : GameScreen
    {
        #region Fields
        Texture2D kbEnter;
        Texture2D kbEsc;
        Texture2D btnA;
        Texture2D btnB;
        Texture2D lineaMenu;
        Texture2D SaveTexture;
        Texture2D submit_button;

        string title;
        string message;
        bool includeButtons;

        #endregion

        #region Events

        public event EventHandler<PlayerIndexEventArgs> Accepted;
        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public MessageBoxScreen(string title, string message)
            : this(title, message, true)
        { }


        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.
        /// </summary>
        public MessageBoxScreen(string title, string message, bool includeButtons)
        {
            //if (includeUsageText)
                //this.message = message + "Press A to Exit, Press B to cancel";
            //else
                //this.message = message;

            this.title = title;
            this.message = message;
            this.includeButtons = includeButtons;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            // We need tap gestures to hit the buttons
            EnabledGestures = GestureType.Tap;
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            //Key "Enter"
            kbEnter = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>(@"Keyboard/key_enter");

            //Key "Scape"
            kbEsc = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>(@"Keyboard/key_esc");

            //Button "A"
            btnA = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>("xboxControllerButtonA");

            //Button "B"
            btnB = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>("xboxControllerButtonB");

            //Linea blanca separatoria
            lineaMenu = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>(@"menu/linea_menu");

            // Save Texture
            SaveTexture = ((MyGame)this.ScreenManager.Game).Content.Load<Texture2D>(@"menu/SaveAnimation");

            submit_button = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/submit_button_mobile");

#if !WINDOWS_PHONE && !WINDOWS
            if (!includeButtons)
            {
                ((Game1)this.ScreenManager.Game).storageDeviceManager.isStartScreen = true;
            }
#endif
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    if (includeButtons)
                    {
                        if ((gesture.Position.X >= 515 && gesture.Position.X <= 798) &&
                            (gesture.Position.Y >= 523 && gesture.Position.Y <= 555))
                        {
                            // Raise the accepted event, then exit the message box.
                            if (Accepted != null)
                                Accepted(this, new PlayerIndexEventArgs(PlayerIndex.One));
                        }

                        if ((gesture.Position.X >= 846 && gesture.Position.X <= 1130) &&
                            (gesture.Position.Y >= 523 && gesture.Position.Y <= 555))
                        {
                            // Raise the cancelled event, then exit the message box.
                            if (Cancelled != null)
                                Cancelled(this, new PlayerIndexEventArgs(PlayerIndex.One));
                        }
                    }
                    else
                    {
                        // Raise the accepted event, then exit the message box.
                        if (Accepted != null)
                            Accepted(this, new PlayerIndexEventArgs(PlayerIndex.One));
                    }

                    

                    ExitScreen();
                }
            }

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            for (byte i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        playerIndex = PlayerIndex.One;
                        break;
                    case 1:
                        playerIndex = PlayerIndex.Two;
                        break;
                    case 2:
                        playerIndex = PlayerIndex.Three;
                        break;
                    case 3:
                        playerIndex = PlayerIndex.Four;
                        break;
                    default:
                        playerIndex = PlayerIndex.One;
                        break;
                }

                if (input.IsMenuSelect(playerIndex))
                {
                    // Raise the accepted event, then exit the message box.
                    if (Accepted != null)
                        Accepted(this, new PlayerIndexEventArgs(playerIndex));

                    ExitScreen();
                }
                else if (input.IsMenuCancel(playerIndex))
                {
                    // Raise the cancelled event, then exit the message box.
                    if (Cancelled != null)
                        Cancelled(this, new PlayerIndexEventArgs(playerIndex));

                    ExitScreen();
                }
            }
        }


        #endregion


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
#if !WINDOWS_PHONE && !WINDOWS
            // Save Animation Update
            if (!includeButtons)
            {
                ((Game1)this.ScreenManager.Game).storageDeviceManager.Update(gameTime);
            }
#endif

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }




        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            string[] lines;
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;
            Vector2 buttonsPossition = new Vector2(textPosition.X, textPosition.Y + 40);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, (int)TransitionAlpha);

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;
            float pulsate = (float)Math.Sin(time * 6) + 1;
            float scale = 2 + pulsate * 0.10f;

            spriteBatch.Begin();

#if WINDOWS_PHONE
            textPosition.X -= 100;
            textPosition.Y -= 50;

            // Draw the message box text.
            spriteBatch.DrawString(font, title, textPosition, color);

            // Linea blanca
            textPosition.Y += 30;
            spriteBatch.Draw(lineaMenu, textPosition, Color.White);

            // Draw the message box text.
            textPosition.Y += 15;
            //spriteBatch.DrawString(font, Strings.ConfirmReturnMainMenuMMString, textPosition, color);
            lines = Regex.Split(message, "\r\n");
            foreach (string line in lines)
            {
                spriteBatch.DrawString(font, line.Replace("	", ""), textPosition, color);
                textPosition.Y += 20;
            }

            // Linea blanca
            textPosition.Y += 100;
            spriteBatch.Draw(lineaMenu, textPosition, Color.White);

            spriteBatch.DrawString(font, "OK", new Vector2(buttonsPossition.X + spaceBetweenButtonAndText + 2, buttonsPossition.Y + 6), Color.Black, 0, Vector2.Zero, scale - 0.5f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, "OK", new Vector2(buttonsPossition.X + spaceBetweenButtonAndText, buttonsPossition.Y + 4), Color.White, 0, Vector2.Zero, scale - 0.5f, SpriteEffects.None, 1.0f);
            
            distanceBetweenButtonsText = Convert.ToInt32(font.MeasureString("Ok").X) + spaceBetweenButtonAndText + spaceBetweenButtons + 100;

            spriteBatch.DrawString(font, Strings.CancelString.ToUpper(), new Vector2(buttonsPossition.X + spaceBetweenButtonAndText + distanceBetweenButtonsText + 2, buttonsPossition.Y + 6), Color.Black, 0, Vector2.Zero, scale - 0.5f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, Strings.CancelString.ToUpper(), new Vector2(buttonsPossition.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, buttonsPossition.Y + 4), Color.White, 0, Vector2.Zero, scale - 0.5f, SpriteEffects.None, 1.0f);
            

#else
            // Draw the message box text.
            spriteBatch.DrawString(font, title, textPosition, color);

            // Linea blanca
            textPosition.Y += 30;
            spriteBatch.Draw(lineaMenu, textPosition, Color.White);

            // Draw the message box text.
            textPosition.Y += 15;
            //spriteBatch.DrawString(font, Strings.ConfirmReturnMainMenuMMString, textPosition, color);
            lines = Regex.Split(message, "\r\n");
            foreach (string line in lines)
            {
                spriteBatch.DrawString(font, line.Replace("	", ""), textPosition, color);
                textPosition.Y += 20;
            }

#if !WINDOWS
            // Save Animation Texture
            if (!includeButtons)
            {
                ((Game1)this.ScreenManager.Game).storageDeviceManager.Draw(spriteBatch, gameTime, new Vector2(textPosition.X, textPosition.Y + 20), true);
                //spriteBatch.Draw(SaveTexture, new Vector2(textPosition.X, textPosition.Y + 20) , Color.White);
            }
#endif

            // Linea blanca
            textPosition.Y += 100;
            spriteBatch.Draw(lineaMenu, textPosition, Color.White);

            
            // Show Gamer Card Button
            buttonsPossition = new Vector2(textPosition.X, textPosition.Y + 10);

            if (((MyGame)this.ScreenManager.Game).player1.Options != InputMode.Touch)
            {
                if (((MyGame)this.ScreenManager.Game).player1.Options == InputMode.Keyboard)
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                    spriteBatch.Draw(kbEnter, buttonsPossition, null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                }
                else
                {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                    spriteBatch.Draw(btnA, buttonsPossition, null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }

                spriteBatch.DrawString(font, "Ok", new Vector2(buttonsPossition.X + spaceBetweenButtonAndText, buttonsPossition.Y + 4), Color.White);
                distanceBetweenButtonsText = Convert.ToInt32(font.MeasureString("Ok").X) + spaceBetweenButtonAndText + spaceBetweenButtons;

                if (includeButtons)
                {
                    // Leave Button
                    if (((MyGame)this.ScreenManager.Game).player1.Options == InputMode.Keyboard)
                    {
                        spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                        spriteBatch.Draw(kbEsc, new Vector2(buttonsPossition.X + distanceBetweenButtonsText, buttonsPossition.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    }
                    else
                    {
                        spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                        spriteBatch.Draw(btnB, new Vector2(buttonsPossition.X + distanceBetweenButtonsText, buttonsPossition.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                    }

                    spriteBatch.DrawString(font, Strings.CancelString, new Vector2(buttonsPossition.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, buttonsPossition.Y + 4), Color.White);
                    distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(font.MeasureString(Strings.CancelString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                }
            }
            else
            {
                // OK
                spriteBatch.Draw(submit_button, buttonsPossition, Color.White);
                spriteBatch.DrawString(font, "OK", new Vector2(buttonsPossition.X + 2 + submit_button.Width / 2 - font.MeasureString("OK").X / 2, buttonsPossition.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                spriteBatch.DrawString(font, "OK", new Vector2(buttonsPossition.X + submit_button.Width / 2 - font.MeasureString("OK").X / 2, buttonsPossition.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                if (includeButtons)
                {
                    // Cancel
                    spriteBatch.Draw(submit_button, new Vector2(buttonsPossition.X + 332, buttonsPossition.Y), Color.White);
                    spriteBatch.DrawString(font, Strings.CancelString.ToUpper(),
                        new Vector2(buttonsPossition.X + 332 + submit_button.Width / 2 - font.MeasureString(Strings.CancelString.ToUpper()).X / 2, buttonsPossition.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                    spriteBatch.DrawString(font, Strings.CancelString.ToUpper(),
                        new Vector2(buttonsPossition.X + 330 + submit_button.Width / 2 - font.MeasureString(Strings.CancelString.ToUpper()).X / 2, buttonsPossition.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                }
            }
#endif

            spriteBatch.End();
        }


        #endregion
    }
}
