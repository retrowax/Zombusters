#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;

namespace ZombustersWindows
{
    public sealed class Explosion
    {
        private static Texture2D texture;
        public static Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        private Explosion() { }
        public static void Draw(SpriteBatch batch, Vector2 pos, float rotation, double totalSeconds, double startSeconds, double endSeconds)
        {
            // If the explosion is finished, or hasn't started yet, return
            if ((totalSeconds > endSeconds) || (startSeconds > totalSeconds))
                return;

            double duration = endSeconds - startSeconds;
            double time = totalSeconds - startSeconds;
            
            // The explosion gets bigger as time goes on
            float scale = (float) ((time / duration)*.5f) + .6f;

            Vector2 origin = new Vector2(texture.Height / 2);
            
            // The explosion gets brighter as time goes on
            float intensity = (float)(time / duration) * .5f;
            Vector4 colorval = new Vector4(0.75f + intensity);

            // The explosion also fades in and out
            colorval.W = (float)Math.Sin((time / duration) * MathHelper.Pi)+.2f;

            // The intensity and fade determines our tint color
            Color color = new Color(colorval);

            // Draw the texture with the appropriate scale and tint
            batch.Draw(texture, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0.5f);            
        }
    }

    public class ScrollingBackground
    {
        private Vector2 screenpos, origin, texturesize;
        private Texture2D mytexture;
        private int screenheight;
        public void Load(GraphicsDevice device, Texture2D backgroundTexture)
        {
            mytexture = backgroundTexture;
            screenheight = device.Viewport.Height;
            int screenwidth = device.Viewport.Width;
            // Set the origin so that we're drawing from the 
            // center of the top edge.
            origin = new Vector2(mytexture.Width / 2, 0);
            // Set the screen position to the center of the screen.
            screenpos = new Vector2(screenwidth / 2, screenheight / 2);
            // Offset to draw the second texture, when necessary.
            texturesize = new Vector2(0, mytexture.Height);
        }
        public void Update(float deltaY)
        {
            screenpos.Y += deltaY;
            screenpos.Y = screenpos.Y % mytexture.Height;
        }
        public void Draw(SpriteBatch batch, Color color)
        {
            // Draw the texture, if it is still onscreen.
            if (screenpos.Y < screenheight)
            {
                batch.Draw(mytexture, screenpos, null,
                     color, 0, origin, 1, SpriteEffects.None, 0f);
            }
            // Draw the texture a second time, behind the first,
            // to create the scrolling illusion.
            batch.Draw(mytexture, screenpos - texturesize, null,
                 color, 0, origin, 1, SpriteEffects.None, 0f);
        }
    }

    public class BackgroundScreen : GameScreen
    {
        ScrollingBackground stars;
        ScrollingBackground dust;
        Texture2D corner;
        Texture2D border;
        public bool isBackgroundOn;
        Texture2D background_title;
        Texture2D background_title_shadow;
        Texture2D background_title_scrolling0;
        Texture2D background_title_scrolling1;
        Texture2D background_title_scrolling2;
        Texture2D background_title_scrolling3;

        private readonly List<Texture2D> noisedMap = new List<Texture2D>(4);
        private readonly Random random = new Random(4);
        public Vector2 position = new Vector2(-200, 0);
        public Vector2 position2 = new Vector2(-200, 0);
        public bool directionRight = true;

        public float BackgroundDriftRatePerSec = 64.0f;
        int maxPosition = 0;
        int minPosition = -200;
        float velocity = 0.0015f;

        public BackgroundScreen()
        {
            stars = new ScrollingBackground();
            dust = new ScrollingBackground();

            // We need tap gestures to hit the buttons
            EnabledGestures = GestureType.Tap;
        }

        public override void Initialize()
        {
            this.isBackgroundOn = false;
            base.Initialize();
        }

        public override void LoadContent()
        {
            corner = this.ScreenManager.Game.Content.Load<Texture2D>("corner");
            border = this.ScreenManager.Game.Content.Load<Texture2D>("border");
            background_title = this.ScreenManager.Game.Content.Load<Texture2D>("background_title");
            background_title_shadow = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/background_title_shadow");
            background_title_scrolling0 = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/background_title_scrolling0");
            background_title_scrolling1 = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/background_title_scrolling1");
            background_title_scrolling2 = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/background_title_scrolling2");
            background_title_scrolling3 = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/background_title_scrolling3");
            for (int i = 0; i < 4; i++)
            {
                noisedMap.Add(new Texture2D(this.ScreenManager.GraphicsDevice, 512, 512, false, SurfaceFormat.Color));
            }

            noisedMap[0].SetData<Color>(CreateTexture.FillNoise(noisedMap[0].Width, noisedMap[0].Height, 0.5f));
            noisedMap[1].SetData<Color>(CreateTexture.FillNoise(noisedMap[1].Width, noisedMap[1].Height, 0.4f));
            noisedMap[2].SetData<Color>(CreateTexture.FillNoise(noisedMap[2].Width, noisedMap[2].Height, 0.6f));
            noisedMap[3].SetData<Color>(CreateTexture.FillNoise(noisedMap[3].Width, noisedMap[3].Height, 0.7f));
            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            ((MyGame)ScreenManager.Game).CheckIfControllerChanged(input);
            base.HandleInput(input);
        }

        public Rectangle GetTitleSafeArea()
        {
            PresentationParameters pp = 
                this.ScreenManager.GraphicsDevice.PresentationParameters;
            Rectangle retval = 
                new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
     
            int offsetx = (pp.BackBufferWidth + 9) / 10;
            int offsety = (pp.BackBufferHeight + 9) / 10;

            retval.Inflate(-offsetx, -offsety);  // Deflate the rectangle
            return retval;
        }

        public Rectangle GetTitleSafeRect(Rectangle view)
        {
            // Return the part of the incoming rectangle that is within
            // the title safe area
            return Rectangle.Intersect(view, GetTitleSafeArea());
        }

        public void DrawBorder(SpriteBatch batch, Rectangle uiBounds, Color color)
        {
            Vector2 TopLeft = new Vector2(uiBounds.Left, uiBounds.Top);
            Vector2 BottomRight = new Vector2(uiBounds.Right, uiBounds.Bottom);
            Vector2 BottomLeft = new Vector2(uiBounds.Left, uiBounds.Bottom);
            Vector2 TopRight = new Vector2(uiBounds.Right, uiBounds.Top);

            int cornerSize = 20;
            Rectangle LeftBorder = new Rectangle(uiBounds.Left - 1, 
                uiBounds.Top + cornerSize, 3, uiBounds.Height - cornerSize * 2);
            Rectangle UpperBorder = new Rectangle(uiBounds.Left + cornerSize, 
                uiBounds.Top + 2, 3, uiBounds.Width - cornerSize * 2);
            // Draw corners
            batch.Draw(corner, TopLeft, color);
            batch.Draw(corner, TopRight, null, color, MathHelper.PiOver2, 
                Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            batch.Draw(corner, BottomRight, null, color, MathHelper.Pi, 
                Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            batch.Draw(corner, BottomLeft, null, color, MathHelper.PiOver2 * 3, 
                Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

            // Draw connective border
            batch.Draw(border, LeftBorder, null, color, 0, Vector2.Zero, 
                SpriteEffects.None, 1.0f);
            LeftBorder.X = uiBounds.Right - 2;
            batch.Draw(border, LeftBorder, null, color, 0, Vector2.Zero, 
                SpriteEffects.None, 1.0f);
            batch.Draw(border, UpperBorder, null, color, -MathHelper.PiOver2, 
                Vector2.Zero, SpriteEffects.None, 1.0f);
            UpperBorder.Y = uiBounds.Bottom + 1;
            batch.Draw(border, UpperBorder, null, color, -MathHelper.PiOver2, 
                Vector2.Zero, SpriteEffects.None, 1.0f);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            if (isBackgroundOn)
            {
                float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

                //Scrolling Effect
                if (directionRight == true)
                {
                    // BKG 1
                    if (position.X <= maxPosition)
                    {
                        position.X += (velocity * 2) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        directionRight = false;
                        position.X -= (velocity * 2) * gameTime.ElapsedGameTime.Milliseconds;
                    }

                    // BKG 2
                    if (position2.X <= maxPosition)
                    {
                        position2.X += (velocity) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        position2.X -= (velocity) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else
                {
                    // BKG 1
                    if (position.X >= minPosition)
                    {
                        position.X -= (velocity * 2) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        directionRight = true;
                        position.X += (velocity * 2) * gameTime.ElapsedGameTime.Milliseconds;
                    }

                    // BKG 2
                    if (position2.X <= maxPosition)
                    {
                        position2.X -= (velocity) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        position2.X += (velocity) * gameTime.ElapsedGameTime.Milliseconds;
                    }
                }

                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                this.ScreenManager.SpriteBatch.Draw(background_title, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.Draw(background_title_scrolling2, new Vector2(position2.X, position2.Y), Color.White);
                this.ScreenManager.SpriteBatch.Draw(background_title_scrolling3, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.Draw(background_title_scrolling1, new Vector2(position.X, position.Y), Color.White);

                if (((MyGame)this.ScreenManager.Game).isInMenu)
                {
                    this.ScreenManager.SpriteBatch.Draw(background_title_shadow, new Vector2(0, 0), Color.White);
                }

                // Perlin Noise effect draw
                this.ScreenManager.SpriteBatch.Draw(noisedMap[random.Next(0, 3)], new Rectangle(0, 0, 1280, 720), new Color(255, 255, 255, 12));
                this.ScreenManager.SpriteBatch.End();
            }
            else
            {
                this.ScreenManager.GraphicsDevice.Clear(Color.Black);
            }

            // Draw the Storage Device Icon
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            this.ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
