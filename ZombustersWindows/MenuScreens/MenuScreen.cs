using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    // Used by the Menu event handlers to get the menu selection.
    public class MenuSelection : EventArgs {
        private int m_selection;

        public MenuSelection(int s) {
            m_selection = s;
        }

        public int Selection {
            get { return m_selection; }
            set { m_selection = value; }
        }
    }

    public class MenuComponent : DrawableGameComponent {
        private SpriteBatch batch;
        private List<string> MenuItems;
        public List<string> HelpText;
        private List<int> ItemHeights;
        private List<int> ItemWidths;
        //public Vector2 TopLeft;
        public Rectangle uiBounds;
        public SpriteFont Font;
        public Color SelectedColor = Color.White;
        public Color UnselectedColor = Color.LightGray;
        public int Selection = 0;
        public PlayerIndex Controller;
        Texture2D lineaTextoMenu; //Linea de fondo del texto de las opciones
        Texture2D logoRetrowaxMenu;
        Texture2D kbEnter;
        Texture2D kbSpace;
        Texture2D kbEsc;
        Texture2D kbLeft;
        Texture2D kbRight;
        Texture2D btnA;
        Texture2D btnB;
        Texture2D btnX;
        Texture2D btnY;
        Texture2D btnStart;
        Texture2D btnLB;
        Texture2D btnRB;
        Texture2D btnDPad;
        Texture2D submit_button;
        Texture2D goBackButton;
        Texture2D arrow;
        Texture2D facebookLogo;
        Texture2D twitterLogo;
        Texture2D googleLogo;
        Texture2D buyNow;
        public event EventHandler<MenuSelection> MenuOptionSelected;
        public event EventHandler<MenuSelection> MenuCanceled;
        public event EventHandler<MenuSelection> MenuConfigSelected;
        public event EventHandler<MenuSelection> MenuShowMarketplace;

        public MenuComponent(Game game) : base(game) {
            this.MenuItems = new List<string>();
            this.HelpText = new List<string>();
            this.ItemHeights = new List<int>();
            this.ItemWidths = new List<int>();
        }

        public MenuComponent(Game game, SpriteFont font) : this(game) {
            this.Font = font;
        }

        public MenuComponent(Game game, SpriteFont font, SpriteBatch batch) : this(game, font) {
            this.batch = batch;
        }

        public MenuComponent(Game game, SpriteFont font, SpriteBatch batch, Rectangle bounds) : this(game, font, batch) {
            this.uiBounds = bounds;
        }

        public int Count {
            get { return MenuItems.Count; }
        }

        public void Clear() {
            MenuItems.Clear();
            HelpText.Clear();
            ItemHeights.Clear();
            ItemWidths.Clear();
            Selection = 0;
        }

        public void AddText(string menu, string help) {
            MenuItems.Add(menu);
            HelpText.Add(help);
            Vector2 result = Font.MeasureString(menu);
            ItemHeights.Add(RoundUp(result.Y));
            ItemWidths.Add(RoundUp(result.X + 250));
        }

        public void UpdateText(string menu, string help, int position) {
            MenuItems[position] = menu;
            HelpText[position] = help;
            Vector2 result = Font.MeasureString(menu);
            ItemHeights[position] = RoundUp(result.Y);
            ItemWidths[position] = RoundUp(result.X + 250);
        }

        public void AddText(string menu) {
            AddText(menu, "");
        }

        public void RemoveAt(int index) {
            MenuItems.RemoveAt(index);
            HelpText.RemoveAt(index);
            ItemHeights.RemoveAt(index);
            ItemWidths.RemoveAt(index);
        }

        public void RemoveAll() {
            MenuItems.Clear();
            HelpText.Clear();
            ItemHeights.Clear();
            ItemWidths.Clear();
        }

        protected override void LoadContent() {
            if (batch == null)
                batch = new SpriteBatch(GraphicsDevice);

            kbEnter = this.Game.Content.Load<Texture2D>(@"Keyboard/key_enter");
            kbSpace = this.Game.Content.Load<Texture2D>(@"Keyboard/key_space");
            kbEsc = this.Game.Content.Load<Texture2D>(@"Keyboard/key_esc");
            kbLeft = this.Game.Content.Load<Texture2D>(@"Keyboard/key_left");
            kbRight = this.Game.Content.Load<Texture2D>(@"Keyboard/key_right");
            btnA = this.Game.Content.Load<Texture2D>("xboxControllerButtonA");
            btnB = this.Game.Content.Load<Texture2D>("xboxControllerButtonB");
            btnX = this.Game.Content.Load<Texture2D>("xboxControllerButtonX");
            btnY = this.Game.Content.Load<Texture2D>("xboxControllerButtonY");
            btnStart = this.Game.Content.Load<Texture2D>("xboxControllerStart");
            btnLB = this.Game.Content.Load<Texture2D>("xboxControllerLeftShoulder");
            btnRB = this.Game.Content.Load<Texture2D>("xboxControllerRightShoulder");
            btnDPad = this.Game.Content.Load<Texture2D>("xboxControllerDPad");
            submit_button = this.Game.Content.Load<Texture2D>(@"menu/submit_button_mobile");
            arrow = this.Game.Content.Load<Texture2D>(@"InGame/SelectPlayer/arrowLeft");
            facebookLogo = this.Game.Content.Load<Texture2D>(@"menu/facebook-64x64");
            twitterLogo = this.Game.Content.Load<Texture2D>(@"menu/twitter-64x64");
            googleLogo = this.Game.Content.Load<Texture2D>(@"menu/google-64x64");
            goBackButton = this.Game.Content.Load<Texture2D>(@"menu/goBackButton");
            buyNow = this.Game.Content.Load<Texture2D>(@"menu/buynow");
            lineaTextoMenu = this.Game.Content.Load<Texture2D>(@"menu/linea_texto_menu");
            logoRetrowaxMenu = this.Game.Content.Load<Texture2D>(@"menu/logo_retrowax_menu");
            base.LoadContent();            
        }

        public void HandleInput(InputState input) {
            for (int i = 0; i < input.GetCurrentGamePadStates().Length; i++) {
                if (input.IsNewKeyPress(Keys.Enter)) {
                    this.Controller = (PlayerIndex)i;
                }
                if (input.GetCurrentGamePadStates()[i].IsButtonDown(Buttons.A)) {
                    this.Controller = (PlayerIndex)i;
                }
            }

            if (input.IsNewKeyPress(Keys.Escape) || input.IsNewKeyPress(Keys.Back)) {
                MenuCanceled.Invoke(this, new MenuSelection(-1));
                return;
            }

            if (input.IsNewKeyPress(Keys.Enter) || input.IsNewKeyPress(Keys.Space)) {
                if (MenuOptionSelected != null)
                    MenuOptionSelected(this, new MenuSelection(Selection));
                return;
            }

            if (input.IsNewKeyPress(Keys.Down)) {
                Selection++;
            }

            if (input.IsNewKeyPress(Keys.Up)) {
                Selection--;
            }

            if (input.IsNewButtonPress(Buttons.B) || input.IsNewButtonPress(Buttons.Back)) {
                MenuCanceled.Invoke(this, new MenuSelection(-1));
                return;
            }

            if (input.IsNewButtonPress(Buttons.A)) {
                if (MenuOptionSelected != null)
                    MenuOptionSelected(this, new MenuSelection(Selection));
                return;
            }

            Vector2 current = new Vector2(uiBounds.X + (uiBounds.Width / 2), uiBounds.Y);
            for (int i = 0; i < MenuItems.Count; i++) {
                foreach (GestureSample gesture in input.GetGestures()) {
                    if (gesture.GestureType == GestureType.Tap) {
                        if ((gesture.Position.X >= current.X && gesture.Position.X <= (current.X + lineaTextoMenu.Width)) &&
                            (gesture.Position.Y >= current.Y && gesture.Position.Y <= (current.Y + lineaTextoMenu.Height)))
                        {
                            if (MenuOptionSelected != null)
                                MenuOptionSelected(this, new MenuSelection(i));
                        }

                        if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                            (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                        {
                            MenuCanceled.Invoke(this, new MenuSelection(-1));
                        }
                    }
                }
                current.Y += ItemHeights[i];
            }

            if (input.IsNewButtonPress(Buttons.DPadDown) || input.IsNewButtonPress(Buttons.LeftThumbstickDown)) {
                Selection++;
            }

            if (input.IsNewButtonPress(Buttons.DPadUp) || input.IsNewButtonPress(Buttons.LeftThumbstickUp)) {
                Selection--;
            }

            if (Selection >= MenuItems.Count)
                Selection -= MenuItems.Count;

            if (Selection < 0)
                Selection += MenuItems.Count;
        }

        private static int RoundUp(float value) {
            int retval = (int)value;
            if (value > retval)
                retval++;
            return retval;
        }

        public Rectangle Extents {
            get {
                int width = 0;
                int height = 0;
                for (int i = 0; i < MenuItems.Count; i++) {
                    width = Math.Max(width, ItemWidths[i]);
                    height += ItemHeights[i];
                }
                return new Rectangle(uiBounds.X, uiBounds.Y, width, height);
            }
        }

        public Rectangle GetExtent(int index) {
            int totalheight = 0;
            for (int i = 0; i < index; i++) {
                totalheight += ItemHeights[i];
            }
            return new Rectangle(uiBounds.X, uiBounds.Y + totalheight, 
                ItemWidths[index], ItemHeights[index]);
        }

        private Viewport CreateViewport() {
            Viewport view = new Viewport(); // create a new viewport
            view.X = uiBounds.X + (uiBounds.Width / 2);       // using our UIBounds
            view.Y = uiBounds.Y;
            view.Width = uiBounds.Width;
            view.Height = uiBounds.Height;
            return view;
        }

        public void CenterMenu(Viewport view) {
            Vector2 centerView = new Vector2(view.X + (view.Width / 2), view.Y + (view.Height / 2));
            Rectangle bounds = Extents;
            bounds.X = 0;
            bounds.Y = 0;
            bounds.Offset((int)(centerView.X - (bounds.Width / 2)), (int)(centerView.Y - (bounds.Height / 2)));
            this.uiBounds = bounds;            
        }

        public void CenterInXMenu(Viewport view) {
            Vector2 centerView = new Vector2(view.X + (view.Width / 2), view.Y + (view.Height / 2));
            Rectangle bounds = Extents;
            bounds.X = 0;
            bounds.Y = 0;
            bounds.Offset((int)(centerView.X - (bounds.Width / 2)) - 100, (int)(centerView.Y + centerView.Y /4));
            this.uiBounds = bounds;
        }

        public void CenterInXLeftMenu(Viewport view) {
            Vector2 centerView = new Vector2(view.X, view.Y + (view.Height / 2));
            Rectangle bounds = Extents;
            bounds.X = 0;
            bounds.Y = 0;
            bounds.Offset((int)(centerView.X - (bounds.Width / 2)) + 150, (int)(centerView.Y));
            this.uiBounds = bounds;
        }

        private Vector2 GetTopLeft() {
            int selectheight = ItemHeights[Selection];
            int aboveheight = 0;
            int belowheight = 0;
            for (int i = 0; i < Selection; i++) {
                aboveheight += ItemHeights[i];
            }

            for (int i = Selection + 1; i < ItemHeights.Count; i++) {
                belowheight += ItemHeights[i];
            
            }
            int totalheight = selectheight + aboveheight + belowheight;

            if (aboveheight + (selectheight / 2) <= uiBounds.Height / 2) {
                return new Vector2(0, 0);
            } else if (belowheight + (selectheight / 2) < uiBounds.Height / 2) {
                return new Vector2(0, uiBounds.Height - totalheight);
            } else {
                int temp = aboveheight + (selectheight / 2);
                return new Vector2(0, uiBounds.Height / 2 - temp);
            }
        }

        public override void Draw(GameTime gameTime) {
            if (Count == 0)
                return;

            Vector2 current = GetTopLeft();
            Viewport oldv = Game.GraphicsDevice.Viewport;  // cache the current viewport
            Game.GraphicsDevice.Viewport = CreateViewport();  // set viewport to our UIBounds
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            float interval = 3.0f;
            float value = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds * interval);
            value = (value + 1) / 2;
            Color color = new Color(value, value, value, value);
            for (int i = 0; i < MenuItems.Count; i++) {
                if (Selection == i) {
                    if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                        batch.DrawString(Font, MenuItems[i], new Vector2(current.X + 5, current.Y + 1), SelectedColor);
                        batch.Draw(lineaTextoMenu, current, color);
                    } else {
                        if (MenuItems[i] != Strings.SaveAndExitString) {
                            batch.DrawString(Font, MenuItems[i], new Vector2(current.X + 5, current.Y + 1), SelectedColor);
                        }
                    }
                } else {
                    if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                        batch.DrawString(Font, MenuItems[i], new Vector2(current.X + 5, current.Y + 1), UnselectedColor);
                    } else {
                        if (MenuItems[i] != Strings.SaveAndExitString) {
                            batch.DrawString(Font, MenuItems[i], new Vector2(current.X + 5, current.Y + 1), UnselectedColor);
                        }
                    }
                }
                current.Y += ItemHeights[i];
            }
            batch.End();
            GraphicsDevice.Viewport = oldv;  // return to the old viewport
            base.Draw(gameTime);
        }

        public void DrawBuyNow(GameTime gameTime) {
            /*if (licenseInformation.IsTrial)
            {
                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // Arrow Left
                batch.Draw(buyNow, new Vector2(-25, -25), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                batch.End();
            }*/
        }

        public void DrawDreamBuildPlayDisclaimer(SpriteBatch batch, SpriteFont fontBig, SpriteFont fontSmall)
        {
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            string build = "'Dream Build Play' Build v1.0";
            string disclaimer = "This demo does not represent the final features or quality of the software.";
            batch.DrawString(fontBig, build,
                    new Vector2(65, 65), Color.White);
            batch.DrawString(fontSmall, disclaimer,
                    new Vector2(65, 65 + fontBig.MeasureString(build).Y + 2), Color.White);
            batch.End();
        }

        public void DrawLogoRetrowaxMenu(SpriteBatch batch, Vector2 position, SpriteFont MenuFont) {
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            batch.Draw(logoRetrowaxMenu, position, Color.White);
            /*if (licenseInformation.IsTrial) 
            { 
                batch.DrawString(MenuFont, Strings.TrialModeMenuString").ToUpper(), 
                    new Vector2(position.X + logoRetrowaxMenu.Width/2 - MenuFont.MeasureString(Strings.TrialModeMenuString").ToUpper()).X/2, position.Y + logoRetrowaxMenu.Height), Color.White);
            }*/
            batch.End();
        }

        public void DrawMenuButtons(SpriteBatch batch, Vector2 position, SpriteFont Font, Boolean inLobby, Boolean isHost, Boolean inMainMenu) {
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;
            if (inLobby == true) {
                if (isHost) {
                    if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                        spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                        batch.Draw(kbEnter, position, null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    } else {
                        spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                        batch.Draw(btnA, position, null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                    }
                    batch.DrawString(Font, Strings.StartGameMenuString, new Vector2(position.X + spaceBetweenButtonAndText, position.Y + 4), Color.White);
                    distanceBetweenButtonsText = Convert.ToInt32(Font.MeasureString(Strings.StartGameMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                }

                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                    batch.Draw(kbEsc, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                    batch.Draw(btnB, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.LeaveMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.LeaveMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                spaceBetweenButtonAndText = Convert.ToInt32(btnX.Width * 0.33f) + 5;
                batch.Draw(btnX, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.InviteMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.InviteMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                spaceBetweenButtonAndText = Convert.ToInt32(btnY.Width * 0.33f) + 5;
                batch.Draw(btnY, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.ShowGamerCardMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.ShowGamerCardMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                spaceBetweenButtonAndText = Convert.ToInt32(btnRB.Width * 0.28f) + 5;
                batch.Draw(btnRB, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.28f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.PartyMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.PartyMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
            } else {
                if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                    if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                        spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                        batch.Draw(kbEnter, position, null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    } else {
                        spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                        batch.Draw(btnA, position, null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                    }
                    batch.DrawString(Font, Strings.SelectString, new Vector2(position.X + spaceBetweenButtonAndText, position.Y + 4), Color.White);
                    distanceBetweenButtonsText = Convert.ToInt32(Font.MeasureString(Strings.SelectString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                }

                if (!inMainMenu) {
                    if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                        if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                            spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                            batch.Draw(kbEsc, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                        } else if (((MyGame)this.Game).player1.Options == InputMode.GamePad) {
                            spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                            batch.Draw(btnB, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                        }
                        batch.DrawString(Font, Strings.BackString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                        distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.BackString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                    } else {
                        batch.Draw(submit_button, position, Color.White);
                        batch.DrawString(Font, Strings.BackString.ToUpper(),
                            new Vector2(position.X + 2 + submit_button.Width / 2 - Font.MeasureString(Strings.BackString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        batch.DrawString(Font, Strings.BackString.ToUpper(),
                            new Vector2(position.X + submit_button.Width / 2 - Font.MeasureString(Strings.BackString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                    }
                }
            }
        }

        public void DrawSelectPlayerButtons(SpriteBatch batch, Vector2 position, SpriteFont Font, Boolean canStartGame, Boolean isHost) {
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;

            if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbSpace.Width * 0.7f) + 5;
                    batch.Draw(kbSpace, position, null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                    batch.Draw(btnA, position, null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.SwitchReadyMenuString, new Vector2(position.X + spaceBetweenButtonAndText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = Convert.ToInt32(Font.MeasureString(Strings.SwitchReadyMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;

                if (canStartGame && isHost) {
                    if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                        spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                        batch.Draw(kbEnter, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    } else {
                        spaceBetweenButtonAndText = Convert.ToInt32(btnStart.Width * 0.33f) + 5;
                        batch.Draw(btnStart, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                    }
                    batch.DrawString(Font, Strings.StartGameMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                    distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.StartGameMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                }

                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                    batch.Draw(kbEsc, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                    batch.Draw(btnB, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.LeaveMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.LeaveMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;

                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbLeft.Width * 0.7f) + Convert.ToInt32(kbRight.Width * 0.7f) + 5;
                    batch.Draw(kbLeft, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    batch.Draw(kbRight, new Vector2(position.X + Convert.ToInt32(kbLeft.Width * 0.7f) + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnDPad.Width * 0.17f) + 5;
                    batch.Draw(btnDPad, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.17f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.ChangeCharacterMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.ChangeCharacterMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
            } else {
                batch.Draw(submit_button, position, Color.White);
                batch.DrawString(Font, Strings.BackString.ToUpper(),
                    new Vector2(position.X + 2 + submit_button.Width / 2 - Font.MeasureString(Strings.BackString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.BackString.ToUpper(),
                    new Vector2(position.X + submit_button.Width / 2 - Font.MeasureString(Strings.BackString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.Draw(submit_button, new Vector2(position.X + 332, position.Y), Color.White);
                batch.DrawString(Font, Strings.ToggleReadyMenuString.ToUpper(),
                    new Vector2(position.X + 332 + submit_button.Width / 2 - Font.MeasureString(Strings.ToggleReadyMenuString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.ToggleReadyMenuString.ToUpper(),
                    new Vector2(position.X + 330 + submit_button.Width / 2 - Font.MeasureString(Strings.ToggleReadyMenuString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                if (canStartGame) {
                    batch.Draw(submit_button, new Vector2(position.X + 662, position.Y), Color.White);
                    batch.DrawString(Font, Strings.StartGameMenuString.ToUpper(),
                        new Vector2(position.X + 662 + submit_button.Width / 2 - Font.MeasureString(Strings.StartGameMenuString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                    batch.DrawString(Font, Strings.StartGameMenuString.ToUpper(),
                        new Vector2(position.X + 660 + submit_button.Width / 2 - Font.MeasureString(Strings.StartGameMenuString.ToUpper()).X / 2, position.Y + 7), Color.Yellow, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                }
            }
        }

        public void DrawOptionsMenuButtons(SpriteBatch batch, Vector2 position, SpriteFont Font) {
            int distanceBetweenButtonsText = 0;
            int spaceBetweenButtonAndText = 0;
            int spaceBetweenButtons = 30;

            if (((MyGame)this.Game).player1.Options != InputMode.Touch) {
                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEnter.Width * 0.7f) + 5;
                    batch.Draw(kbEnter, position, null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnA.Width * 0.33f) + 5;
                    batch.Draw(btnA, position, null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.SelectString, new Vector2(position.X + spaceBetweenButtonAndText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = Convert.ToInt32(Font.MeasureString(Strings.SelectString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbEsc.Width * 0.7f) + 5;
                    batch.Draw(kbEsc, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnB.Width * 0.33f) + 5;
                    batch.Draw(btnB, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.33f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.BackWithoutSavingString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.BackWithoutSavingString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
                if (((MyGame)this.Game).player1.Options == InputMode.Keyboard) {
                    spaceBetweenButtonAndText = Convert.ToInt32(kbLeft.Width * 0.7f) + Convert.ToInt32(kbRight.Width * 0.7f) + 5;
                    batch.Draw(kbLeft, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                    batch.Draw(kbRight, new Vector2(position.X + Convert.ToInt32(kbLeft.Width * 0.7f) + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                } else {
                    spaceBetweenButtonAndText = Convert.ToInt32(btnDPad.Width * 0.17f) + 5;
                    batch.Draw(btnDPad, new Vector2(position.X + distanceBetweenButtonsText, position.Y), null, Color.White, 0, Vector2.Zero, 0.17f, SpriteEffects.None, 1.0f);
                }
                batch.DrawString(Font, Strings.ChangeSoundVolumeMenuString, new Vector2(position.X + spaceBetweenButtonAndText + distanceBetweenButtonsText, position.Y + 4), Color.White);
                distanceBetweenButtonsText = distanceBetweenButtonsText + Convert.ToInt32(Font.MeasureString(Strings.ChangeSoundVolumeMenuString).X) + spaceBetweenButtonAndText + spaceBetweenButtons;
            } else {
                batch.Draw(submit_button, position, Color.White);
                batch.DrawString(Font, Strings.SaveAndExitString.ToUpper(),
                    new Vector2(position.X + 2 + submit_button.Width / 2 - Font.MeasureString(Strings.SaveAndExitString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.SaveAndExitString.ToUpper(),
                    new Vector2(position.X + submit_button.Width / 2 - Font.MeasureString(Strings.SaveAndExitString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.Draw(submit_button, new Vector2(position.X + 332, position.Y), Color.White);
                batch.DrawString(Font, Strings.BackWithoutSavingString.ToUpper(),
                    new Vector2(position.X + 332 + submit_button.Width / 2 - Font.MeasureString(Strings.BackWithoutSavingString.ToUpper()).X / 2, position.Y + 9), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                batch.DrawString(Font, Strings.BackWithoutSavingString.ToUpper(),
                    new Vector2(position.X + 330 + submit_button.Width / 2 - Font.MeasureString(Strings.BackWithoutSavingString.ToUpper()).X / 2, position.Y + 7), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            }
        }

        public void DrawSocialLogosMenu(SpriteBatch batch, Vector2 position, SpriteFont MenuFont) {
            Vector2 iconPos = new Vector2(position.X + 25, position.Y + 10);
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            batch.Draw(facebookLogo, iconPos, Color.White);
            batch.Draw(twitterLogo, new Vector2(iconPos.X + facebookLogo.Width + 15, iconPos.Y), Color.White);
            //batch.Draw(googleLogo, new Vector2(iconPos.X + facebookLogo.Width + twitterLogo.Width + 30, iconPos.Y), Color.White);
            batch.End();
        }

        public void DrawBackButtonMenu(SpriteBatch batch, Vector2 position, SpriteFont MenuFont) {
            Vector2 iconPos = new Vector2(position.X + 25, position.Y + 10);
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            batch.Draw(goBackButton, iconPos, Color.White);
            batch.DrawString(MenuFont, Strings.BackString.ToUpper(), new Vector2(iconPos.X - 10 - MenuFont.MeasureString(Strings.BackString.ToUpper()).X, iconPos.Y + 15), Color.White);
            batch.End();
        }
    }

    public class MenuScreen : BackgroundScreen {
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
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);
            uiBounds = GetTitleSafeArea();
            titleBounds = new Rectangle(0, 0, 1280, 720);
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");
            fontItalic = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMusic");
            fontSmallItalic = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMusicItalic");
            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
            //bloom = new BloomComponent(this.ScreenManager.Game);
            menu.Initialize();
            menu.AddText(Strings.WPPlayNewGame.ToUpper(), Strings.WPPlayNewGameMMString);
            menu.AddText(Strings.ExtrasMenuString, Strings.ExtrasMMString);
            menu.AddText(Strings.SettingsMenuString, Strings.ConfigurationString);
            menu.AddText(Strings.ReviewMenuString, Strings.ReviewMMString);
            /*if (licenseInformation.IsTrial)
            {
                menu.AddText(Strings.UnlockFullGameMenuString").ToUpper(), Strings.UnlockFullGameMenuString"));
            }*/
            menu.AddText(Strings.QuitGame.ToUpper(), Strings.QuitGame + ".");

            menu.uiBounds = menu.Extents;
            menu.uiBounds.Offset(uiBounds.X, 300);
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            menu.MenuShowMarketplace += new EventHandler<MenuSelection>(menu_ShowMarketPlace);
            menu.CenterInXLeftMenu(view);
            //bloom.Visible = !bloom.Visible;
            ((MyGame)this.ScreenManager.Game).isInMenu = true;
            base.Initialize();
            this.isBackgroundOn = true;
        }

        void menu_MenuCanceled(Object sender, MenuSelection selection) {
            // If they hit B or Back, go back to Start Screen
            //ExitScreen();
            //ScreenManager.AddScreen(new StartScreen((Game1)ScreenManager.Game));
        }

        void menu_MenuConfigSelected(Object sender, MenuSelection selection) {

        }

        void menu_ShowMarketPlace(Object sender, MenuSelection selection) {

        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e) {
            this.ScreenManager.Game.Exit();
        }

        void menu_MenuOptionSelected(Object sender, MenuSelection selection) {
            switch (selection.Selection) {
                case 0:
                    ((MyGame)this.ScreenManager.Game).BeginSelectPlayerScreen(false);
                    break;
                case 1:
                    ((MyGame)this.ScreenManager.Game).DisplayExtrasMenu();
                    break;

                case 2:
                    ((MyGame)this.ScreenManager.Game).DisplayOptions(0);
                    break;

                case 3:
                    //await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=44468RetrowaxGames.Zombusters_rhyy9bbdeb2be"));
                    break;

                case 4:
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
            lineaMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/linea_menu");
            logoMenu = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/logo_menu");
            base.LoadContent();
        }

        public override void HandleInput(InputState input) {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if (((MyGame)this.ScreenManager.Game).currentGameState != GameState.Paused) {
                if (!coveredByOtherScreen) {
                    menu.Update(gameTime);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            if (((MyGame)this.ScreenManager.Game).currentGameState != GameState.Paused) {
                menu.DrawBuyNow(gameTime);
                //menu.DrawSocialLogosMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width - 200, uiBounds.Height), MenuInfoFont);
                menu.Draw(gameTime);
                menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
                //menu.DrawDreamBuildPlayDisclaimer(this.ScreenManager.SpriteBatch, fontItalic, fontSmallItalic);
                DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
            } else {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                this.ScreenManager.SpriteBatch.Draw(((MyGame)this.ScreenManager.Game).blackTexture, new Vector2(0, 0), Color.White);
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
            lines = Regex.Split(menu.HelpText[menu.Selection], "\r\n");
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
