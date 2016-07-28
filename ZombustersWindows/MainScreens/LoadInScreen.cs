using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZombustersWindows.MainScreens
{
    public class LoadInScreen : BackgroundScreen
    {
        public event EventHandler ScreenFinished;
        public int paneCount = 1;
        public bool onlineOnly = true;

        public LoadInScreen()
        {
        }

        public LoadInScreen(int paneCount, bool onlineOnly)
        {
            this.paneCount = paneCount;
            this.onlineOnly = onlineOnly;
        }

        public override void Initialize()
        {
            this.IsPopup = false;
            base.Initialize();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (ScreenFinished != null)
            {
                ScreenFinished.Invoke(this, null);
            }
            ExitScreen();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
