using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class AchievementsScreen : BackgroundScreen
    {
        private List<AchievementDisplay> _achievements;
        private SpriteFont _titleFont;
        private Texture2D _buttonB;

        private SpriteFont _textFont;

        private Vector2 _iconLoc;
        private Vector2 _nameLoc;
        private Vector2 _pointsLoc;
        private Vector2 _dateLoc;

        private Player player;

        private int _topIndex;

        private const int MaxAchievementsDisplayed = 6;

        public AchievementsScreen(Player currentPlayer)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _iconLoc = new Vector2(10, 100);
            _nameLoc = new Vector2(90, 100);
            _pointsLoc = new Vector2(525, 100);
            _dateLoc = new Vector2(675, 100);

            _topIndex = 0;

            this.player = currentPlayer;
        }

        public override void Initialize()
        {
            _achievements = new AchievementManager(ScreenManager.Game, player).Achievements();
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            this.PresenceMode = GamerPresenceMode.ConfiguringSettings;
#endif

            base.Initialize();

            this.isBackgroundOn = true;
        }

        void menu_MenuCancelled(Object sender, MenuSelection selection)
        {
            ExitScreen();
        }
        

        public override void LoadContent()
        {
            _titleFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            _textFont = this.ScreenManager.Game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            _buttonB = this.ScreenManager.Game.Content.Load<Texture2D>("xboxControllerButtonB");

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            // If they press Back or B, exit
            if (input.IsNewButtonPress(Buttons.Back) ||
                input.IsNewButtonPress(Buttons.B))
                ExitScreen();


            //check for scrolling
            if (input.IsNewButtonPress(Buttons.DPadUp))
            {
                _topIndex++;
                _topIndex = (int)MathHelper.Clamp(_topIndex, 0, _achievements.Count - 1 - MaxAchievementsDisplayed);
            }

            if (input.IsNewButtonPress(Buttons.DPadDown))
            {
                _topIndex--;
                _topIndex = (int)MathHelper.Clamp(_topIndex, 0, _achievements.Count - 1 - MaxAchievementsDisplayed);
            }

            base.HandleInput(input);
        }
       
        
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 textPosition;
            Color color = new Color(0, 255, 0, (int)TransitionAlpha);

            base.Draw(gameTime);
            
            SpriteBatch batch = this.ScreenManager.SpriteBatch;

            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);


            //draw title
            Vector2 titleSize = _titleFont.MeasureString("Achievements");
            textPosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - titleSize.X / 2, 5);

            batch.DrawString(_titleFont, "Achievements", textPosition, color);

            color = new Color(255, 255, 255, (int)TransitionAlpha);

            int numDisplayed = (int)MathHelper.Clamp(_achievements.Count, 0, MaxAchievementsDisplayed);

            //draw headers
            batch.DrawString(_textFont, "Name", _nameLoc, Color.White);
            batch.DrawString(_textFont, "Points", _pointsLoc, Color.White);
            batch.DrawString(_textFont, "Date Unlocked", _dateLoc, Color.White);

            //loop through list and draw each item
            for (int i = _topIndex; i < numDisplayed; i++)
            {
                batch.Draw(_achievements[i].Icon, _iconLoc + new Vector2(0, (i + 1) * 75), Color.White);
                batch.DrawString(_textFont, _achievements[i].Name, _nameLoc + new Vector2(0, (i + 1) * 75), Color.White);
                batch.DrawString(_textFont, _achievements[i].Points, _pointsLoc + new Vector2(0, (i + 1) * 75), Color.White);
                batch.DrawString(_textFont, _achievements[i].DateUnlocked, _dateLoc + new Vector2(0, (i + 1) * 75), Color.White);
            }

            batch.Draw(_buttonB, new Rectangle(800, 700, 48, 48), Color.White);
            batch.DrawString(ScreenManager.Font, "Back", new Vector2(855, 700), Color.White);

            
            batch.End();

            if (TransitionPosition > 0)
                this.ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }
    }
}
