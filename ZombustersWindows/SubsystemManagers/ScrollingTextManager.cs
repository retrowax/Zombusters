using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombustersWindows.Subsystem_Managers
{
    class ScrollingTextManager
    {
        //This will store the display are for the block of text
        private Rectangle displayArea;

        //This is the font that will be used to draw the text
        private readonly SpriteFont font;

        //This is the lines of text that make up the text block
        private List<TextLine> textLines;

        //Indica si ya se han mostrado todas las lineas de los creditos
        public bool endOfLines;

        //Create a new TextBlock object
        public ScrollingTextManager(Rectangle theArea, SpriteFont theFont, string theText)
        {
            displayArea = theArea;
            font = theFont;
            endOfLines = false;

            CalculateTextDisplay(theText);
        }

        //Calculate the line lengths and position the lines for scrolling
        private void CalculateTextDisplay(string theText)
        {
            textLines = new List<TextLine>();
            
            string aTextLine = string.Empty;
            string aNewWord = string.Empty;

            int aYPosition = displayArea.Y + displayArea.Height;

            foreach (char theChar in theText.ToCharArray())
            {
                if (font.MeasureString(aTextLine + aNewWord + theChar).Length() > displayArea.Width || theChar == '\n')
                {
                    textLines.Add(new TextLine(new Vector2(displayArea.X, aYPosition), aTextLine));
                    aYPosition += font.LineSpacing;
                    aTextLine = string.Empty;
                }

                aNewWord += theChar;
                if (theChar == ' ' || theChar == '\r' || theChar == '\n')
                {
                    aTextLine += aNewWord;
                    aNewWord = string.Empty;
                }
            }

            textLines.Add(new TextLine(new Vector2(displayArea.X, aYPosition), aTextLine + aNewWord));           
        }


        //Scroll the lines up through the display rectangle
        private double mScrollDelay = 0.05f;
        public void Update(GameTime theGameTime)
        {
            mScrollDelay -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mScrollDelay < 0)
            {
                mScrollDelay = 0.05f;

                foreach (TextLine theTextLine in textLines)
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
            foreach (TextLine theTextLine in textLines)
            {
                if (theTextLine.Position.Y + font.LineSpacing <= displayArea.Y + displayArea.Height)
                {
                    if (theTextLine.Position.Y > displayArea.Y) 
                    {
                        theBatch.DrawString(font, theTextLine.Text, new Vector2(displayArea.Center.X - font.MeasureString(theTextLine.Text).X / 2, theTextLine.Position.Y), Color.White);
                    }

                    if ((countLine == textLines.Count - 1) && theTextLine.Position.Y < displayArea.Y)
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
