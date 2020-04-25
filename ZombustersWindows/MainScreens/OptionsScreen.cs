using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class OptionsScreen : BackgroundScreen
    {
        readonly MyGame game;
        MenuComponent menu;
        SliderComponent volumeSlider;
        SliderComponent musicSlider;
        LanguageComponent languageComponent;
        Vector2 playerStringLoc;
        Vector2 selectPos;

        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        OptionsState state;
        Rectangle uiBounds;
        InputMode displayMode;
       

        public OptionsScreen(MyGame game, OptionsState state)
        {
            this.state = state;
            this.game = game;
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            uiBounds = GetTitleSafeArea();
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            menu = new MenuComponent(game, MenuListFont);
            menu.Initialize();
            //menu.AddText("Player Control Scheme");
            menu.AddText("SoundEffectVolumeString");
            menu.AddText("MusicVolumeString");
            menu.AddText("ChangeLanguageOption");
            menu.AddText("FullScreenMenuString");
            menu.AddText("SaveAndExitString");

            menu.MenuOptionSelected += new EventHandler<MenuSelection>(MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(MenuCancelled);

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

            volumeSlider = new SliderComponent(game, this.ScreenManager.SpriteBatch);
            volumeSlider.Initialize();
            Rectangle tempExtent = menu.GetExtent(0);  // volume menu item
            volumeSlider.SliderArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);
            volumeSlider.SliderUnits = 10;
            volumeSlider.SliderSetting = (int) (state.FXLevel*volumeSlider.SliderUnits);
            volumeSlider.SetColor = Color.Cyan;
            volumeSlider.UnsetColor = Color.DodgerBlue;

            musicSlider = new SliderComponent(game, this.ScreenManager.SpriteBatch);
            musicSlider.Initialize();
            tempExtent = menu.GetExtent(1);
            musicSlider.SliderArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);
            musicSlider.SliderUnits = 10;
            musicSlider.SliderSetting = (int)(state.MusicLevel*musicSlider.SliderUnits);
            musicSlider.SetColor = Color.Cyan;
            musicSlider.UnsetColor = Color.DodgerBlue;

            languageComponent = new LanguageComponent(game, ScreenManager.SpriteBatch);
            languageComponent.Initialize();
            languageComponent.languageArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);

            tempExtent = menu.GetExtent(0);
            playerStringLoc = new Vector2(menu.uiBounds.Right + 20, tempExtent.Top);
            //this.PresenceMode = GamerPresenceMode.ConfiguringSettings;
            base.Initialize();
            this.isBackgroundOn = true;
        }

        void MenuCancelled(Object sender, MenuSelection selection)
        {
            ExitScreen();
        }

        void MenuOptionSelected(Object sender, MenuSelection selection)
        {
            if (menu.Selection == 2) 
            {
                state.locale = languageComponent.SwitchLanguage();
            }
            else if (menu.Selection == 3)
            {
                ToggleFullScreen();
            }
            else if (menu.Selection == 4)  // Save and Exit
            {
                ExitScreen();
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                {
                    game.SetOptions(state, this.game.player1);
                }
                else if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.A))
                {
                    game.SetOptions(state, this.game.player2);
                }
                else if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.A))
                {
                    game.SetOptions(state, this.game.player3);
                }
                else if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.A))
                {
                    game.SetOptions(state, this.game.player4);
                }
                game.SetOptions(state, this.game.player1);
            }
        }
        
        public override void LoadContent()
        {
            volumeSlider.LoadContent();
            musicSlider.LoadContent();
            languageComponent.LoadContent();
            base.LoadContent();
        }

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
                case 0:
                    HandleFXAudio(input, game.player1.Controller);
                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
                    {
                        HandleFXAudio(input, game.player1.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                    {
                        HandleFXAudio(input, game.player2.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                    {
                        HandleFXAudio(input, game.player3.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                    {
                        HandleFXAudio(input, game.player4.Controller);
                    }
                    volumeSlider.UnsetColor = menu.SelectedColor;
                    break;
                case 1:
                    HandleMusic(input, game.player1.Controller);
                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
                    {
                        HandleMusic(input, game.player1.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                    {
                        HandleMusic(input, game.player2.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                    {
                        HandleMusic(input, game.player3.Controller);
                    }
                    if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                    {
                        HandleMusic(input, game.player4.Controller);
                    }
                    musicSlider.UnsetColor = menu.SelectedColor;
                    break;
                default:
                    break;
            }

            // Let the menu handle input regarding selection change
            // and the A/B/Back buttons:
            if (this.game.player1.inputMode != InputMode.Touch)
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
                        game.SetOptions(state, this.game.player1);
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

        private void ToggleFullScreen()
        {
            Resolution.SetVirtualResolution(game.VIRTUAL_RESOLUTION_WIDTH, game.VIRTUAL_RESOLUTION_HEIGHT);
            if (state.FullScreenMode)
            {
                Resolution.SetResolution(game.VIRTUAL_RESOLUTION_WIDTH, game.VIRTUAL_RESOLUTION_HEIGHT, false);
                state.FullScreenMode = false;
                game.graphics.IsFullScreen = true;
            }
            else
            {
                int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Resolution.SetResolution(screenWidth, screenHeight, true);
                state.FullScreenMode = true;
                game.graphics.IsFullScreen = false;
            }
            this.game.graphics.ToggleFullScreen();
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
            languageComponent.Draw(gameTime);
            menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
            menu.DrawContextMenu(selectPos, this.ScreenManager.SpriteBatch);
#if DEMO
            menu.DrawDemoWIPDisclaimer(this.ScreenManager.SpriteBatch);
#endif
        }
    }
}
