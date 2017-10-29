using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
//using Microsoft.Xna.Framework.Storage;
#endif
using GameStateManagement;

namespace ZombustersWindows.Subsystem_Managers
{
    class ScrollingTextManager
    {
        //This will store the display are for the block of text
        private Rectangle mDisplayArea;

        //This is the font that will be used to draw the text
        private SpriteFont mFont;

        //This is the lines of text that make up the text block
        private List<TextLine> mTextLines;

        //Indica si ya se han mostrado todas las lineas de los creditos
        public bool endOfLines;

        //Create a new TextBlock object
        public ScrollingTextManager(Rectangle theArea, SpriteFont theFont, string theText)
        {
            mDisplayArea = theArea;
            mFont = theFont;
            endOfLines = false;

            CalculateTextDisplay(theText);
        }

        //Calculate the line lengths and position the lines for scrolling
        private void CalculateTextDisplay(string theText)
        {
            mTextLines = new List<TextLine>();
            
            string aTextLine = string.Empty;
            string aNewWord = string.Empty;

            int aYPosition = mDisplayArea.Y + mDisplayArea.Height;

            foreach (char theChar in theText.ToCharArray())
            {
                if (mFont.MeasureString(aTextLine + aNewWord + theChar).Length() > mDisplayArea.Width || theChar == '\n')
                {
                    mTextLines.Add(new TextLine(new Vector2(mDisplayArea.X, aYPosition), aTextLine));
                    aYPosition += mFont.LineSpacing;
                    aTextLine = string.Empty;
                }

                aNewWord += theChar;
                if (theChar == ' ' || theChar == '\r' || theChar == '\n')
                {
                    aTextLine += aNewWord;
                    aNewWord = string.Empty;
                }
            }

            mTextLines.Add(new TextLine(new Vector2(mDisplayArea.X, aYPosition), aTextLine + aNewWord));           
        }


        //Scroll the lines up through the display rectangle
        private double mScrollDelay = 0.05f;
        public void Update(GameTime theGameTime)
        {
            mScrollDelay -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mScrollDelay < 0)
            {
                mScrollDelay = 0.05f;

                foreach (TextLine theTextLine in mTextLines)
                {
                    theTextLine.Position.Y -= 1;
                }
            }
        }

        //Draw the TextBlock
        public void Draw(SpriteBatch theBatch)
        {
            int countLine = 0;
            //Cycle through all of the lines and if they are within the display area, draw them to the screen
            foreach (TextLine theTextLine in mTextLines)
            {
                if (theTextLine.Position.Y + mFont.LineSpacing <= mDisplayArea.Y + mDisplayArea.Height)
                {
                    if (theTextLine.Position.Y > mDisplayArea.Y) 
                    {
                        theBatch.DrawString(mFont, theTextLine.Text, new Vector2(mDisplayArea.Center.X - mFont.MeasureString(theTextLine.Text).X / 2, theTextLine.Position.Y), Color.White);
                    }

                    if ((countLine == mTextLines.Count - 1) && theTextLine.Position.Y < mDisplayArea.Y)
                    {
                        endOfLines = true;
                    }
                }

                countLine++;
            }
        }
    }

    class TextLine
    {
        public Vector2 Position;
        public string Text;

        public TextLine(Vector2 thePosition, string theText)
        {
            Position = thePosition;
            Text = theText;
        }
    }
}
