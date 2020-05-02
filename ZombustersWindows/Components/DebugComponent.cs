#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
#endregion

namespace ZombustersWindows
{
    /// <summary>
    /// A message box screen, used to display "debug" messages.
    /// </summary>
    public class DebugInfoComponent : DrawableGameComponent
    {
        readonly MyGame game;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont, smallspriteFont;
        bool stopUpdatingText = false;
        readonly InputState input = new InputState();

        ScrollingTextManager mText;
        String previousNetDebugText = "";
        public String NetDebugText = "";

        public DebugInfoComponent(MyGame game)
            : base(game)
        {
            this.game = game;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo"); //content.Load<SpriteFont>(@"menu/ArialMenuInfo");
            smallspriteFont = game.Content.Load<SpriteFont>(@"menu\ArialMusicItalic");

            //Create a Textblock object. This object will calculate the line wraps needs for the 
            //block of text and position the lines of text for scrolling through the display area
            mText = new ScrollingTextManager(new Rectangle(0, 280, 500, 400), smallspriteFont, NetDebugText);
        }

        public virtual void HandleInput(InputState input) { }

        public void HandleDebugInput(InputState input)
        {
            if (input.IsNewButtonPress(Buttons.X))
            {
                if (stopUpdatingText == false)
                    stopUpdatingText = true;
                else
                    stopUpdatingText = false;


                return;
            }

            this.HandleInput(input);
        }


        public override void Update(GameTime gameTime)
        {
            input.Update();
            this.HandleDebugInput(input);

            //Update the TextBlock. This will allow the lines of text to scroll.
            if (NetDebugText != previousNetDebugText)
            {
                mText = new ScrollingTextManager(new Rectangle(781, 322, 500, 400), smallspriteFont, NetDebugText);
                previousNetDebugText = NetDebugText;
            }

            if (stopUpdatingText == false)
                mText.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            int count = 0;
            Vector2 position = new Vector2(32, 64);
            spriteBatch.Begin();

#if XBOX
            spriteBatch.DrawString(spriteFont, "Signed In Gamers: " + Gamer.SignedInGamers.Count.ToString(), position, Color.White);
            if (mygame.gamerManager.getOnlinePlayerGamertag() != null)
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Online Gamertags: " + mygame.gamerManager.getOnlinePlayerGamertag().Count.ToString(), position, Color.White);
            }

            if (mygame.gamerManager.getOnlinePlayerIcons() != null)
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Online Icons: " + mygame.gamerManager.getOnlinePlayerIcons().Count.ToString(), position, Color.White);
            }
#endif

#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            if (mygame.networkSessionManager.networkSession != null)
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "BPS Received: " + mygame.networkSessionManager.networkSession.BytesPerSecondReceived.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "BPS Sent: " + mygame.networkSessionManager.networkSession.BytesPerSecondSent.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Private Slots: " + mygame.networkSessionManager.networkSession.PrivateGamerSlots.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Max session Gamers: " + mygame.networkSessionManager.networkSession.MaxGamers.ToString(), position, Color.White);
            }
#endif

            //position = new Vector2(position.X, position.Y + 32);
#if DEMO
            spriteBatch.DrawString(smallspriteFont, "Demo: ON", new Vector2(position.X + 1, position.Y + 1), Color.Black);
            spriteBatch.DrawString(smallspriteFont, "Demo: ON", position, Color.White);
#else
            spriteBatch.DrawString(smallspriteFont, "Demo: OFF", new Vector2(position.X + 1, position.Y + 1), Color.Black);
            spriteBatch.DrawString(smallspriteFont, "Demo: OFF", position, Color.White);
#endif
            count = 0;
            position = new Vector2(position.X, position.Y + 22);
            foreach (Player player in game.players)
            {
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "Name: " + player.Name, new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "Name: " + player.Name, position, Color.White);
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "Levels" + count.ToString() + ": " + player.levelsUnlocked.ToString(), new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "Levels" + count.ToString() + ": " + player.levelsUnlocked.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "Avatar Status: " + player.avatar.status.ToString(), new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "Avatar Status: " + player.avatar.status.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "Input: " + player.inputMode.ToString(), new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "Input: " + player.inputMode.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "IsPlaying: " + player.IsPlaying.ToString(), new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "IsPlaying: " + player.IsPlaying.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 22);
                spriteBatch.DrawString(smallspriteFont, "IsReady: " + player.isReady.ToString(), new Vector2(position.X + 1, position.Y + 1), Color.Black);
                spriteBatch.DrawString(smallspriteFont, "IsReady: " + player.isReady.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 22);


#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                if (avatar.Player.Container != null)
                {
                    position = new Vector2(position.X, position.Y + 32);
                    spriteBatch.DrawString(spriteFont, "Contner Dispsd: " + avatar.Player.Container.IsDisposed.ToString(), position, Color.White);
                }
#endif
                count++;
            }

            //Now draw the text to be drawn on the screen
            mText.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}
