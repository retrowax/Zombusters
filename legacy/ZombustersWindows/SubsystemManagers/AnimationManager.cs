#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ZombustersWindows.Subsystem_Managers
{
    class Animation
    {
        #region Fields
        // The texture with animation frames
        Texture2D animationTexture;
        // The size and structure of whole frames sheet in animationTexture. The animationTexture could
        // hold animaton sequence organized in multiple rows and multiple columns, that's why animation 
        // engine should know how the frames are organized inside a frames sheet
        Point sheetSize;
        // Amount of time between frames
        TimeSpan frameInterval;
        // Time passed since last frame
        TimeSpan nextFrame;

        // Current frame in the animation sequence
        public Point currentFrame;
        // The size of single frame inside the animationTexture
        public Point frameSize;

        //RetroTrax Animation variables
        public Boolean reverse;
        public Boolean stop;
        private List<int> startAnimSequence, endAnimSequence;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor of an animation class
        /// </summary>
        /// <param name="frameSheet">Texture with animation frames sheet</param>
        /// <param name="size">Single frame size</param>
        /// <param name="frameSheetSize">The whole frame sheet size</param>
        /// <param name="interval">Interval between progressing to the next frame</param>
        public Animation(Texture2D frameSheet, Point size, Point frameSheetSize, TimeSpan interval)
        {
            animationTexture = frameSheet;
            frameSize = size;
            sheetSize = frameSheetSize;
            frameInterval = interval;

            reverse = false;
            stop = false;
            //startAnimSequence = new List<int>(new int[] { 7,6,5,4,3,2,1,0,1 });
            //endAnimSequence = new List<int>(new int[] { 0,1,2,3,4,5,6,7,7 });
            endAnimSequence = new List<int>(new int[] { 7, 6, 5, 4, 3, 2, 1, 0, 1 });
            startAnimSequence = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 7 });
        }
        #endregion

        #region Update and Render
        /// <summary>
        /// Updates the animaton progress
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="progressed">Returns true if animation were progressed; in such case 
        /// caller could updated the position of the animated character</param>
        public bool Update(GameTime gameTime)
        {
            bool progressed;

            // Check is it is a time to progress to the next frame
            if (nextFrame >= frameInterval)
            {
                // Progress to the next frame in the row
                currentFrame.X++;
                // If reached end of the row advance to the next row 
                // and start form the first frame there
                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;
                    currentFrame.Y++;
                }
                // If reached last row in the frame sheet jump to the first row again - produce endless loop
                if (currentFrame.Y >= sheetSize.Y)
                    currentFrame.Y = 0;

                // Reset interval for next frame
                progressed = true;
                nextFrame = TimeSpan.Zero;
            }
            else
            {
                // Wait for the next frame 
                nextFrame += gameTime.ElapsedGameTime;
                progressed = false;
            }

            return progressed;
        }

        public bool UpdateLogoTrax(GameTime gameTime)
        {
            bool progressed;

            if (stop == false)
            {
                // Check is it is a time to progress to the next frame
                if (nextFrame >= frameInterval)
                {
                    // Progress to the next frame in the row
                    if (reverse)
                        currentFrame.X--;
                    else
                        currentFrame.X++;
                    // If reached end of the row advance to the next row 
                    // and start form the first frame there
                    if (currentFrame.X >= sheetSize.X)
                    {
                        currentFrame.X = sheetSize.X;
                        reverse = true;
                        //stop = true;
                    }

                    if (currentFrame.X <= 0)
                    {
                        currentFrame.X = 0;
                        reverse = false;
                    }

                    // If reached last row in the frame sheet jump to the first row again - produce endless loop
                    if (currentFrame.Y >= sheetSize.Y)
                        currentFrame.Y = 0;

                    // Reset interval for next frame
                    progressed = true;
                    nextFrame = TimeSpan.Zero;
                }
                else
                {
                    // Wait for the next frame 
                    nextFrame += gameTime.ElapsedGameTime;
                    progressed = false;
                }

            }
            else
            {
                progressed = false;
            }

            return progressed;
        }

        /// <summary>
        /// Rendering of the animation
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch in which current frame will be rendered</param>
        /// <param name="position">The position of current frame</param>
        /// <param name="spriteEffect">SpriteEffect to apply on current frame</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffect, float layerDepth, float angle, Color color)
        {
            Draw(spriteBatch, position, 1.0f, spriteEffect, layerDepth, angle, color);
        }

        /// <summary>
        /// Rendering of the animation
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch in which current frame will be rendered</param>
        /// <param name="position">The position of the current frame</param>
        /// <param name="scale">Scale factor to apply on the current frame</param>
        /// <param name="spriteEffect">SpriteEffect to apply on the current frame</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects spriteEffect, float layerDepth, float angle, Color color)
        {
            if (spriteEffect == SpriteEffects.FlipHorizontally)
            {
                spriteBatch.Draw(animationTexture, new Vector2(position.X - frameSize.X/2, position.Y), new Rectangle(
                      frameSize.X * currentFrame.X,
                      frameSize.Y * currentFrame.Y,
                      frameSize.X,
                      frameSize.Y),
                      color, angle, Vector2.Zero, scale, spriteEffect, layerDepth);
            }
            else
            {
                spriteBatch.Draw(animationTexture, position, new Rectangle(
                      frameSize.X * currentFrame.X,
                      frameSize.Y * currentFrame.Y,
                      frameSize.X,
                      frameSize.Y),
                      color, angle, Vector2.Zero, scale, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Rendering of the animation
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch in which current frame will be rendered</param>
        /// <param name="position">The position of the current frame</param>
        /// <param name="scale">Scale factor to apply on the current frame</param>
        /// <param name="spriteEffect">SpriteEffect to apply on the current frame</param>
        public void DrawLogoTrax(SpriteBatch spriteBatch, Vector2 position, float scale, SpriteEffects spriteEffect)
        {
            int cframe = new int();

            if (reverse)
                cframe = endAnimSequence[currentFrame.X];
            else
                cframe = startAnimSequence[currentFrame.X];

            spriteBatch.Draw(animationTexture, position, new Rectangle(
                  frameSize.X * cframe,
                  frameSize.Y * currentFrame.Y,
                  frameSize.X,
                  frameSize.Y),
                  Color.White, 0f, Vector2.Zero, scale, spriteEffect, 0);
        }
        #endregion
    }
}
