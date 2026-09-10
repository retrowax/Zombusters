#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
}
