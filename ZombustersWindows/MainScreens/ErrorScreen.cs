using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;

namespace ZombustersWindows
{
    public class ErrorScreen : GameScreen
    {
        SpriteBatch batch;
        SpriteFont font;
        Vector2 center;
        public string error;

        public ErrorScreen(string error)
        {
            this.error = error;
            this.IsPopup = true;
        }
        
        public override void LoadContent()
        {
            batch = new SpriteBatch(this.ScreenManager.GraphicsDevice);
            font = this.ScreenManager.Font;

            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            center = new Vector2(view.Width / 2, view.Height / 2);

            Vector2 fontwidth = font.MeasureString(error);
            center.X -= fontwidth.X / 2;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            this.ScreenManager.FadeBackBufferToBlack(255);
            batch.Begin();
            batch.DrawString(font, error, center, Color.White);
            batch.End();
            base.Draw(gameTime);
        }
    }
}
