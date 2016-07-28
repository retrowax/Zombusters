using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    class StoryTellingScreen : GameScreen
    {
        public StoryTellingScreen()
        {

        }

        public override void Initialize()
        {
           
            base.Initialize();
        }

        public override void LoadContent()
        {            
            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
        }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
                    
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            this.ScreenManager.FadeBackBufferToBlack(127);

            base.Draw(gameTime);
        }
    }
}