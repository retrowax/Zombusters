using Microsoft.Xna.Framework;
using GameStateManagement;

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