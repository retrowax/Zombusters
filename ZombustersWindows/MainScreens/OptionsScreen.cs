using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{

    public class SliderComponent : DrawableGameComponent
    {
        public int SliderUnits = 10;
        public Rectangle SliderArea = new Rectangle(0, 0, 80, 28);

        private int setting = 5;
        public int SliderSetting
        {
            get { return setting; }
            set
            {
                if (value > SliderUnits)
                    setting = SliderUnits;
                else if (value < 0)
                    setting = 0;
                else
                    setting = value;
            }
        }
        public Color UnsetColor = Color.DodgerBlue;
        public Color SetColor = Color.Cyan;


        SpriteBatch batch;

        public SliderComponent(MyGame game, SpriteBatch batch)
            : base(game)
        {
            this.game = game;
            this.batch = batch;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        Texture2D blank;        
        Vector2 origin;
        private MyGame game;
        SpriteFont MenuInfoFont;
        public new void LoadContent()
        {
            blank = Game.Content.Load<Texture2D>("whitepixel");
            origin = new Vector2(0, blank.Height);
            MenuInfoFont = this.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            if (batch == null)
                batch = new SpriteBatch(Game.GraphicsDevice);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {            
            //batch.Begin(SpriteBlendMode.AlphaBlend);
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (this.game.Main.Options == InputMode.Touch)
            {
                batch.DrawString(MenuInfoFont, "-", new Vector2(SliderArea.Left + 20, SliderArea.Bottom - 25), Color.Yellow, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 1.0f);
            }
            int x = 0;
            for (int i = 1; i <= SliderUnits; i++)
            {
                float percent = (float)i / SliderUnits;
                int height = (int)(percent * SliderArea.Height);

                int gaps = SliderUnits - 1;
                int width = (int)(SliderArea.Width / (gaps + SliderUnits));

                x= ((i - 1) * 2 * width);

                Rectangle displayArea;
                if (this.game.Main.Options != InputMode.Touch)
                {
                    displayArea = new Rectangle(SliderArea.Left + x, SliderArea.Bottom, width, height);
                }
                else
                {
                    displayArea = new Rectangle(SliderArea.Left + 50 + x, SliderArea.Bottom, width, height);
                }

                if (i <= setting)
                    batch.Draw(blank, displayArea, null, SetColor, 0, origin, SpriteEffects.None, 1.0f);
                else
                    batch.Draw(blank, displayArea, null, UnsetColor, 0, origin, SpriteEffects.None, 1.0f);
                
            }
            if (this.game.Main.Options == InputMode.Touch)
            {
                batch.DrawString(MenuInfoFont, "+", new Vector2(SliderArea.Left + 70 + x, SliderArea.Bottom - 28), Color.Yellow, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 1.0f);
            }

            batch.End();
            base.Draw(gameTime);
        }
    }

    public enum InputMode
    {
        Keyboard,
        GamePad,
        Touch
    }
    public struct OptionsState
    {
        public InputMode Player;
        public float FXLevel;
        public float MusicLevel;        
    }

    public delegate void OptionsSet(OptionsState state);

    public class OptionsScreen : BackgroundScreen
    {
        MenuComponent menu;
        SliderComponent volumeSlider;
        SliderComponent musicSlider;
        Vector2 playerStringLoc;
        Vector2 selectPos;

        Texture2D logoMenu;
        Texture2D lineaMenu;  //Linea de 1px para separar

        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        OptionsState state;
        MyGame Game;
        Rectangle uiBounds;

        public OptionsScreen(MyGame game, OptionsState state)
        {
            this.state = state;
            this.Game = game;
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            uiBounds = GetTitleSafeArea();
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuHeaderFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            menu = new MenuComponent(this.ScreenManager.Game, MenuListFont);
            menu.Initialize();
            //menu.AddText("Player Control Scheme");
            menu.AddText(Strings.SoundEffectVolumeString);
            menu.AddText(Strings.MusicVolumeString);
            menu.AddText(Strings.FullScreenMenuString);
            menu.AddText(Strings.SaveAndExitString);

            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCancelled);

            // The menu should be situated in the bottom half of the screen, centered
            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            //menu.uiBounds.Offset(
            //    uiBounds.X + uiBounds.Width / 2 - menu.uiBounds.Width - 5,
            //    uiBounds.Height / 2 + (menu.uiBounds.Height / 2));

            //Posiciona el menu
            menu.CenterInXLeftMenu(view);

            //menu.SelectedColor = Color.DodgerBlue;
            //menu.UnselectedColor = Color.LightBlue;

            volumeSlider = new SliderComponent(this.Game, this.ScreenManager.SpriteBatch);
            volumeSlider.Initialize();
            Rectangle tempExtent = menu.GetExtent(0);  // volume menu item
            volumeSlider.SliderArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);
            volumeSlider.SliderUnits = 10;
            volumeSlider.SliderSetting = (int) (state.FXLevel*volumeSlider.SliderUnits);
            volumeSlider.SetColor = Color.Cyan;
            volumeSlider.UnsetColor = Color.DodgerBlue;

            musicSlider = new SliderComponent(this.Game, this.ScreenManager.SpriteBatch);
            musicSlider.Initialize();
            tempExtent = menu.GetExtent(1);
            musicSlider.SliderArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);
            musicSlider.SliderUnits = 10;
            musicSlider.SliderSetting = (int)(state.MusicLevel*musicSlider.SliderUnits);
            musicSlider.SetColor = Color.Cyan;
            musicSlider.UnsetColor = Color.DodgerBlue;

            tempExtent = menu.GetExtent(0);
            playerStringLoc = new Vector2(menu.uiBounds.Right + 20, tempExtent.Top);
            //this.PresenceMode = GamerPresenceMode.ConfiguringSettings;
            base.Initialize();
            this.isBackgroundOn = true;
        }

        void menu_MenuCancelled(Object sender, MenuSelection selection)
        {
            ExitScreen();
        }

        void menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            if (menu.Selection == 2)
            {
                this.Game.graphics.ToggleFullScreen();
            }
            else if (menu.Selection == 3)  // Save and Exit
            {
                ExitScreen();
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                {
                    Game.SetOptions(state, this.Game.Main);
                }
                else if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.A))
                {
                    Game.SetOptions(state, this.Game.Player2);
                }
                else if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.A))
                {
                    Game.SetOptions(state, this.Game.Player3);
                }
                else if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.A))
                {
                    Game.SetOptions(state, this.Game.Player4);
                }
                Game.SetOptions(state, this.Game.Main);
            }
        }
        
        public override void LoadContent()
        {
            //Linea blanca separatoria
            lineaMenu = this.Game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = this.Game.Content.Load<Texture2D>(@"menu/logo_menu");

            volumeSlider.LoadContent();
            musicSlider.LoadContent();
            base.LoadContent();
        }

        InputMode displayMode;

        public override void HandleInput(InputState input)
        {
            // If they press Back or B, exit
            if (input.IsNewButtonPress(Buttons.Back) ||
                input.IsNewButtonPress(Buttons.B))
                ExitScreen();

            if (input.IsNewKeyPress(Keys.Escape) || input.IsNewKeyPress(Keys.Back))
            {
                ExitScreen();
            }

            musicSlider.UnsetColor = menu.UnselectedColor;
            volumeSlider.UnsetColor = menu.UnselectedColor;
            switch (menu.Selection)
            {
                //case 0:
                //    HandlePlayer(input, Game.Main.Controller);
                //    break;
                case 0:
                    HandleFXAudio(input, Game.Main.Controller);
                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
                    {
                        HandleFXAudio(input, Game.Main.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                    {
                        HandleFXAudio(input, Game.Player2.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                    {
                        HandleFXAudio(input, Game.Player3.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                    {
                        HandleFXAudio(input, Game.Player4.Controller);
                    }
                    volumeSlider.UnsetColor = menu.SelectedColor;
                    break;
                case 1:
                    HandleMusic(input, Game.Main.Controller);
                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
                    {
                        HandleMusic(input, Game.Main.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                    {
                        HandleMusic(input, Game.Player2.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                    {
                        HandleMusic(input, Game.Player3.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                    {
                        HandleMusic(input, Game.Player4.Controller);
                    }
                    musicSlider.UnsetColor = menu.SelectedColor;
                    break;
                default:
                    break;
            }

            // Let the menu handle input regarding selection change
            // and the A/B/Back buttons:
            if (this.Game.Main.Options != InputMode.Touch)
            {
                menu.HandleInput(input);
            }

            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Volume -
                    if ((gesture.Position.X >= 415 && gesture.Position.X <= 523) &&
                        (gesture.Position.Y >= 351 && gesture.Position.Y <= 382))
                    {
                        volumeSlider.SliderSetting--;
                        volumeSlider.UnsetColor = menu.SelectedColor;
                    }

                    // Volume +
                    if ((gesture.Position.X >= 524 && gesture.Position.X <= 629) &&
                        (gesture.Position.Y >= 351 && gesture.Position.Y <= 382))
                    {
                        volumeSlider.SliderSetting++;
                        volumeSlider.UnsetColor = menu.SelectedColor;
                    }

                    // Music -
                    if ((gesture.Position.X >= 415 && gesture.Position.X <= 523) &&
                        (gesture.Position.Y >= 384 && gesture.Position.Y <= 428))
                    {
                        musicSlider.SliderSetting--;
                        musicSlider.UnsetColor = menu.SelectedColor;
                    }

                    // Music +
                    if ((gesture.Position.X >= 524 && gesture.Position.X <= 629) &&
                        (gesture.Position.Y >= 384 && gesture.Position.Y <= 428))
                    {
                        musicSlider.SliderSetting++;
                        musicSlider.UnsetColor = menu.SelectedColor;
                    }

                    // Save and Exit
                    if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        ExitScreen();
                        Game.SetOptions(state, this.Game.Main);
                    }

                    // Exit without Saving
                    if ((gesture.Position.X >= 490 && gesture.Position.X <= 774) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        ExitScreen();
                    }

                    //// BUY NOW!!
                    //if ((gesture.Position.X >= 0 && gesture.Position.X <= 90) &&
                    //    (gesture.Position.Y >= 0 && gesture.Position.Y <= 90))
                    //{
                    //    if (Guide.IsTrialMode)
                    //    {
                    //        Guide.ShowMarketplace(PlayerIndex.One);
                    //    }
                    //}
                }
            }

            base.HandleInput(input);
        }
        private void HandlePlayer(InputState input, PlayerIndex index)
        {
            if (input.IsNewButtonPress(Buttons.LeftThumbstickRight, index) ||
                input.IsNewButtonPress(Buttons.DPadRight, index))
            {
                state.Player++;
                if (state.Player > InputMode.GamePad)
                    state.Player = InputMode.Keyboard;                
            }
            else if (input.IsNewButtonPress(Buttons.LeftThumbstickLeft, index) ||
                input.IsNewButtonPress(Buttons.DPadLeft, index))
            {
                state.Player--;
                if (state.Player < InputMode.Keyboard)
                    state.Player = InputMode.GamePad;                    
            }
            displayMode = state.Player;
        }
        private void HandleFXAudio(InputState input, PlayerIndex index)
        {
            if (input.IsNewButtonPress(Buttons.LeftThumbstickRight, index) ||
                input.IsNewButtonPress(Buttons.DPadRight, index))
            {
                volumeSlider.SliderSetting++;
            }
            else if (input.IsNewButtonPress(Buttons.LeftThumbstickLeft, index) ||
                input.IsNewButtonPress(Buttons.DPadLeft, index))
            {
                volumeSlider.SliderSetting--;
            }

            if (input.IsNewKeyPress(Keys.Right))
            {
                volumeSlider.SliderSetting++;
            }
            else if (input.IsNewKeyPress(Keys.Left))
            {
                volumeSlider.SliderSetting--;
            }
        }
        private void HandleMusic(InputState input, PlayerIndex index)
        {
            if (input.IsNewButtonPress(Buttons.LeftThumbstickRight, index) ||
                input.IsNewButtonPress(Buttons.DPadRight, index))
            {
                musicSlider.SliderSetting++;
            }
            else if (input.IsNewButtonPress(Buttons.LeftThumbstickLeft, index) ||
                input.IsNewButtonPress(Buttons.DPadLeft, index))
            {
                musicSlider.SliderSetting--;
            }

            if (input.IsNewKeyPress(Keys.Right))
            {
                musicSlider.SliderSetting++;
            }
            else if (input.IsNewKeyPress(Keys.Left))
            {
                musicSlider.SliderSetting--;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            state.FXLevel = volumeSlider.SliderSetting / 
                (float)musicSlider.SliderUnits;
            state.MusicLevel = musicSlider.SliderSetting / 
                (float)musicSlider.SliderUnits;
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            menu.Draw(gameTime);
            volumeSlider.Draw(gameTime);
            musicSlider.Draw(gameTime);
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
            DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
        }

        //Draw all the Selection buttons on the bottom of the menu
        private void DrawContextMenu(MenuComponent menu, Vector2 pos, SpriteBatch batch)
        {
            string[] lines;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //Logo Menu
            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);

            //Texto de OPCIONES
            batch.DrawString(MenuHeaderFont, Strings.SettingsMenuString, MenuTitlePosition, Color.White);

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

            //Linea divisoria
            pos.Y -= 15;
            batch.Draw(lineaMenu, pos, Color.White);


            // Draw context buttons
            menu.DrawOptionsMenuButtons(batch, new Vector2(pos.X + 10, pos.Y + 10), MenuInfoFont);

            batch.End();
        }
    }
}
