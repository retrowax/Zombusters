#region File Description
//-----------------------------------------------------------------------------
// DebugInfoPortlet.cs
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE && !NETCOREAPP
using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
#endregion

namespace ZombustersWindows
{
    /// <summary>
    /// A message box screen, used to display "debug" messages.
    /// </summary>
    public class DebugInfoPortlet : DrawableGameComponent
    {
        MyGame mygame;
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont, smallspriteFont;
        bool stopUpdatingText = false;
        InputState input = new InputState();

        ScrollingTextManager mText;
        String previousNetDebugText = "";
        public String NetDebugText = "";

        public DebugInfoPortlet(MyGame game)
            : base(game)
        {
            content = game.Content;
            mygame = game;
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>(@"menu\ArialMenuInfo"); //content.Load<SpriteFont>(@"menu/ArialMenuInfo");
            smallspriteFont = content.Load<SpriteFont>(@"menu\ArialMusicItalic");

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

#if !WINDOWS_PHONE
            //Update the TextBlock. This will allow the lines of text to scroll.
            if (NetDebugText != previousNetDebugText)
            {
                mText = new ScrollingTextManager(new Rectangle(781, 322, 500, 400), smallspriteFont, NetDebugText);
                previousNetDebugText = NetDebugText;
            }

            if (stopUpdatingText == false)
                mText.Update(gameTime);
#endif
        }


        public override void Draw(GameTime gameTime)
        {
            int count = 0;

            Vector2 position = new Vector2(32, 64);

            spriteBatch.Begin();

            //spriteBatch.DrawString(spriteFont, "Signed In Gamers: " + Gamer.SignedInGamers.Count.ToString(), position, Color.White);

#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            if (mygame.networkSessionManager.networkSession != null)
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "NET Gamers: " + mygame.networkSessionManager.networkSession.AllGamers.Count.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "LocalNetworkGamers: " + mygame.networkSessionManager.networkSession.LocalGamers.Count.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "RemoteNetworkGamers: " + mygame.networkSessionManager.networkSession.RemoteGamers.Count.ToString(), position, Color.White);
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "IsHost: " + mygame.networkSessionManager.networkSession.LocalGamers[0].IsHost.ToString(), position, Color.White);
            }
#endif

#if XBOX
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

            /*if (Guide.IsTrialMode == true)
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Trial Mode On", position, Color.White);
            }
            else*/
            {
                position = new Vector2(position.X, position.Y + 32);
                spriteBatch.DrawString(spriteFont, "Trial Mode Off", position, Color.White);
            }

            count = 0;
            foreach (Avatar avatar in mygame.currentPlayers)
            {
                if (avatar.Player != null)
                {
#if !WINDOWS_PHONE
                    position = new Vector2(position.X, position.Y + 32);
                    spriteBatch.DrawString(spriteFont, "Name: " + avatar.Player.Name, position, Color.White);
#endif
                    position = new Vector2(position.X, position.Y + 32);
                    spriteBatch.DrawString(spriteFont, "ULevels" + count.ToString() + ": " + avatar.Player.levelsUnlocked.ToString(), position, Color.White);

#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
                    if (avatar.Player.Container != null)
                    {
                        position = new Vector2(position.X, position.Y + 32);
                        spriteBatch.DrawString(spriteFont, "Contner Dispsd: " + avatar.Player.Container.IsDisposed.ToString(), position, Color.White);
                    }
#endif
                }
                count++;
            }

            //Now draw the text to be drawn on the screen
            mText.Draw(spriteBatch);

            spriteBatch.End();
        }
    }







    /// <summary>
    /// A message box screen, used to display framerate.
    /// </summary>
    public class FrameRateCounterBis : DrawableGameComponent
    {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;


        public FrameRateCounterBis(Game game)
            : base(game)
        {
            content = game.Content;
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>(@"menu\ArialMenuInfo"); //content.Load<SpriteFont>(@"menu/ArialMenuInfo");
        }


        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, fps, new Vector2(33, 33), Color.Black);
            spriteBatch.DrawString(spriteFont, fps, new Vector2(32, 32), Color.White);

            spriteBatch.End();
        }
    }
}