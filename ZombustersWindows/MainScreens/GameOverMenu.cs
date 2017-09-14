﻿using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZombustersWindows.Localization;

namespace ZombustersWindows.MainScreens
{
    public class GameOverMenu : GameScreen
    {
        MenuComponent menu;
        public event EventHandler<MenuSelection> GameOverMenuOptionSelected;

        public GameOverMenu()
        {
            EnabledGestures = GestureType.Tap;
            this.IsPopup = true;
        }

        public override void Initialize()
        {
            menu = new MenuComponent(this.ScreenManager.Game, this.ScreenManager.Font, this.ScreenManager.SpriteBatch);
            menu.AddText(Strings.GPRestartCurrentWaveString); //Restart from current Wave
            menu.AddText(Strings.GPRestartFromBeginningString); //Restart from beginning
            menu.AddText(Strings.GPReturnToMainMenuString); //Return to main menu
            menu.MenuOptionSelected += new EventHandler<MenuSelection>(menu_MenuOptionSelected);
            menu.MenuCanceled += new EventHandler<MenuSelection>(menu_MenuCanceled);
            //menu.MenuConfigSelected += new EventHandler<MenuSelection>(menu_MenuConfigSelected);
            menu.Initialize();
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            menu.CenterMenu(view);
            TransitionPosition = 0.5f;
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        void menu_MenuCanceled(Object sender, MenuSelection selection)
        {
            //MenuCanceled.Invoke(this, new MenuSelection(-1));
            //ExitScreen();
        }

        void menu_MenuOptionSelected(Object sender, MenuSelection selection)
        {
            GameOverMenuOptionSelected.Invoke(this, selection);
            ExitScreen();
        }

        public override void HandleInput(InputState input)
        {
            menu.HandleInput(input);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            this.ScreenManager.FadeBackBufferToBlack(127);
            menu.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}