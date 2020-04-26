using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class SliderComponent : DrawableGameComponent
    {
        public int SliderUnits = 10;
        public Rectangle SliderArea = new Rectangle(0, 0, 80, 28);

        private int setting = 5;
        public int SliderSetting
        {
            get { return setting; }
            set
            {
                if (value > SliderUnits)
                    setting = SliderUnits;
                else if (value < 0)
                    setting = 0;
                else
                    setting = value;
            }
        }
        public Color UnsetColor = Color.DodgerBlue;
        public Color SetColor = Color.Cyan;

        SpriteBatch batch;
        Texture2D blank;
        Vector2 origin;
        private readonly MyGame game;
        SpriteFont MenuInfoFont;

        public SliderComponent(MyGame game, SpriteBatch batch)
            : base(game)
        {
            this.game = game;
            this.batch = batch;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public new void LoadContent()
        {
            blank = game.Content.Load<Texture2D>("whitepixel");
            origin = new Vector2(0, blank.Height);
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            if (batch == null)
                batch = new SpriteBatch(game.GraphicsDevice);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {            
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            if (this.game.player1.inputMode == InputMode.Touch)
            {
                batch.DrawString(MenuInfoFont, "-", new Vector2(SliderArea.Left + 20, SliderArea.Bottom - 25), Color.Yellow, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 1.0f);
            }
            int x = 0;
            for (int i = 1; i <= SliderUnits; i++)
            {
                float percent = (float)i / SliderUnits;
                int height = (int)(percent * SliderArea.Height);

                int gaps = SliderUnits - 1;
                int width = (int)(SliderArea.Width / (gaps + SliderUnits));

                x= ((i - 1) * 2 * width);

                Rectangle displayArea;
                if (this.game.player1.inputMode != InputMode.Touch)
                {
                    displayArea = new Rectangle(SliderArea.Left + x, SliderArea.Bottom, width, height);
                }
                else
                {
                    displayArea = new Rectangle(SliderArea.Left + 50 + x, SliderArea.Bottom, width, height);
                }

                if (i <= setting)
                    batch.Draw(blank, displayArea, null, SetColor, 0, origin, SpriteEffects.None, 1.0f);
                else
                    batch.Draw(blank, displayArea, null, UnsetColor, 0, origin, SpriteEffects.None, 1.0f);
                
            }
            if (this.game.player1.inputMode == InputMode.Touch)
            {
                batch.DrawString(MenuInfoFont, "+", new Vector2(SliderArea.Left + 70 + x, SliderArea.Bottom - 28), Color.Yellow, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 1.0f);
            }

            batch.End();
            base.Draw(gameTime);
        }
    }
}
