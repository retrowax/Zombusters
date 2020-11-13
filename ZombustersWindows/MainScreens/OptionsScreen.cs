using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using ZombustersWindows.Subsystem_Managers;
using GameAnalyticsSDK.Net;

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

        Rectangle uiBounds;
        InputMode displayMode;
        Player player;
       
        public OptionsScreen(MyGame game, Player player)
        {
            this.player = player;
            this.game = game;
        }

        public override void Initialize()
        {
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            uiBounds = GetTitleSafeArea();
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);
            MenuInfoFont = game.Content.Load<SpriteFont>(@"Menu\ArialMenuInfo");
            MenuListFont = game.Content.Load<SpriteFont>(@"Menu\ArialMenuList");

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
            volumeSlider.SliderSetting = (int) (player.optionsState.FXLevel*volumeSlider.SliderUnits);
            volumeSlider.SetColor = Color.Cyan;
            volumeSlider.UnsetColor = Color.DodgerBlue;

            musicSlider = new SliderComponent(game, this.ScreenManager.SpriteBatch);
            musicSlider.Initialize();
            tempExtent = menu.GetExtent(1);
            musicSlider.SliderArea = new Rectangle(menu.uiBounds.Right + 20, tempExtent.Top + 4, 120, tempExtent.Height - 10);
            musicSlider.SliderUnits = 10;
            musicSlider.SliderSetting = (int)(player.optionsState.MusicLevel*musicSlider.SliderUnits);
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
                player.optionsState.locale = languageComponent.SwitchLanguage();
            }
            else if (menu.Selection == 3)
            {
                ToggleFullScreen();
            }
            else if (menu.Selection == 4)  // Save and Exit
            {
                ExitScreen();
                game.SetOptions(player.optionsState, player);
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
                    HandleFXAudio(input, player.playerIndex);
                    volumeSlider.UnsetColor = menu.SelectedColor;
                    break;
                case 1:
                    HandleMusic(input, player.playerIndex);
                    musicSlider.UnsetColor = menu.SelectedColor;
                    break;
                default:
                    break;
            }

            // Let the menu handle input regarding selection change
            // and the A/B/Back buttons:
            if (player.inputMode != InputMode.Touch)
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
                        game.SetOptions(player.optionsState, player);
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
            if (player.optionsState.FullScreenMode)
            {
                Resolution.SetResolution(game.VIRTUAL_RESOLUTION_WIDTH, game.VIRTUAL_RESOLUTION_HEIGHT, false);
                player.optionsState.FullScreenMode = false;
                game.graphics.IsFullScreen = true;
                GameAnalytics.AddDesignEvent("ScreenView:Options:FullScreen:false");
            }
            else
            {
                int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Resolution.SetResolution(screenWidth, screenHeight, true);
                player.optionsState.FullScreenMode = true;
                game.graphics.IsFullScreen = false;
                GameAnalytics.AddDesignEvent("ScreenView:Options:FullScreen:true");
            }
            this.game.graphics.ToggleFullScreen();
        }

        private void HandlePlayer(InputState input, PlayerIndex index)
        {
            if (input.IsNewButtonPress(Buttons.LeftThumbstickRight, index) ||
                input.IsNewButtonPress(Buttons.DPadRight, index))
            {
                player.optionsState.Player++;
                if (player.optionsState.Player > InputMode.GamePad)
                    player.optionsState.Player = InputMode.Keyboard;                
            }
            else if (input.IsNewButtonPress(Buttons.LeftThumbstickLeft, index) ||
                input.IsNewButtonPress(Buttons.DPadLeft, index))
            {
                player.optionsState.Player--;
                if (player.optionsState.Player < InputMode.Keyboard)
                    player.optionsState.Player = InputMode.GamePad;                    
            }
            displayMode = player.optionsState.Player;
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

            GameAnalytics.AddDesignEvent("ScreenView:Options:VolumeSFX", volumeSlider.SliderSetting);
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

            GameAnalytics.AddDesignEvent("ScreenView:Options:VolumeMusic", musicSlider.SliderSetting);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            player.optionsState.FXLevel = volumeSlider.SliderSetting / 
                (float)musicSlider.SliderUnits;
            player.optionsState.MusicLevel = musicSlider.SliderSetting / 
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
