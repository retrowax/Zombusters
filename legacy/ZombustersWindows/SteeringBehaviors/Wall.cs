using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombustersWindows
{
    /// <summary>
    /// Objeto que representa una pared del "mundo" en que se encuentran las entidades steering
    /// </summary>
    public class Wall
    {
        protected Vector2 vA, vB, vN;
        private Primitive2D primitive;

        public Vector2 From
        {
            get
            {
                return vA;
            }
        }

        public Vector2 To
        {
            get
            {
                return vB;
            }
        }

        public Vector2 Normal
        {
            get
            {
                return vN;
            }
        }

        /// <summary>
        /// Calcula la normal del muro  
        /// </summary>
        protected void CalculateNormal()
        {
            Vector2 temp = Vector2.Normalize(this.vB - this.vA);

            vN.X = -temp.Y;
            vN.Y = temp.X;
        }

        /// <summary>
        /// Inicialización gráfica de la primitiva 2D
        /// </summary>
        /// <param name="graphics">Dispositivo gráfico</param>
        private void InitializeGraphics(GraphicsDevice graphics)
        {
            primitive = new Primitive2D(graphics);
        }

        /// <summary>
        /// Prepara la primitiva 2D para su uso en el draw
        /// </summary>
        private void InitializePrimitives()
        {
            primitive.AddVector(vA);
            primitive.AddVector(vB);
        }

        /// <summary>
        /// Constructor que autocalcula la normal
        /// </summary>
        /// <param name="vA">Vector origen</param>
        /// <param name="vB">Vector destino</param>
        /// <param name="graphics"></param>
        public Wall(Vector2 vA, Vector2 vB, GraphicsDevice graphics)
        {
            this.vA = vA;
            this.vB = vB;

            this.CalculateNormal();
            this.InitializeGraphics(graphics);
        }

        /// <summary>
        /// Constructor que incluye la normal
        /// </summary>
        /// <param name="vA">Vector origen</param>
        /// <param name="vB">Vector destino</param>
        /// <param name="vN">Vector normal</param>
        /// <param name="graphics"></param>
        public Wall(Vector2 vA, Vector2 vB, Vector2 vN, GraphicsDevice graphics)
        {
            this.vA = vA;
            this.vB = vB;
            this.vN = vN;

            this.InitializeGraphics(graphics);
        }

        /// <summary>
        /// Dibjuar el wall por pantalla (sólo se usaría para debugar, normalmente 
        /// no nos interesará dibujar las primitivas)
        /// </summary>
        /// <param name="gameTime">Tiempo del juego</param>
        /// <param name="spriteBatch">Spritebatch ya inicializado</param>
        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (primitive.CountVectors == 0)
                this.InitializePrimitives();

            primitive.Render(spriteBatch);
        }
    }

}
