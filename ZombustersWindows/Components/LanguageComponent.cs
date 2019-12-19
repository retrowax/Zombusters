using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ZombustersWindows
{
    public class LanguageComponent : DrawableGameComponent
    {
        SpriteBatch batch;
        Texture2D blank;
        private readonly MyGame game;
        SpriteFont MenuInfoFont;
        public Rectangle languageArea = new Rectangle(0, 0, 80, 28);
        private Vector2 offset = new Vector2(-3, 11);

        readonly List<string> languages = new List<string>
        {
            "en-US",
            "fr-FR",
            "it-IT",
            "gr-GR",
            "es-ES"
        };

        public LanguageComponent(MyGame game, SpriteBatch batch)
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
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            if (batch == null)
                batch = new SpriteBatch(game.GraphicsDevice);
            base.LoadContent();
        }

        public string SwitchLanguage()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(languages[3]);
            return languages[3];
        }

        public override void Draw(GameTime gameTime)
        {            
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            batch.DrawString(MenuInfoFont, currentCulture.DisplayName, new Vector2(languageArea.Left + offset.X, languageArea.Bottom + offset.Y), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

            batch.End();
            base.Draw(gameTime);
        }
    }
}
