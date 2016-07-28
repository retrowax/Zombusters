using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombustersWindows
{
    /// <summary>
    /// Representa el "mundo" en el que se moverán las entidades Steering
    /// </summary>
    public class GameWorld
    {

        #region Atributos

        /// <summary>
        /// Indica si se dibujarán o no los obstáculos
        /// </summary>
        private bool _renderObstacles;

        /// <summary>
        /// Indica si se dibujarán o no las paredes
        /// </summary>
        private bool _renderWalls;

        /// <summary>
        /// Paredes que "cierran" a la entidad
        /// </summary>
        private List<Wall> _walls;

        /// <summary>
        /// Lista de obstáculos que se encuentran en el mundo
        /// </summary>
        private List<Circle> _obstacles;

        /// <summary>
        /// Lista de obstáculos que "podrían" colisionar con la ruta de
        /// los agentes, se regenera en cada ciclo de ejecución
        /// </summary>
        private List<Circle> _taggedObstacles;

        #endregion

        #region Propiedades

        /// <summary>
        /// Getter público de walls
        /// </summary>
        public List<Wall> Walls
        {
            get
            {
                return _walls;
            }
        }

        /// <summary>
        /// Getter público de obstáculos
        /// </summary>
        public List<Circle> Obstacles
        {
            get
            {
                return _obstacles;
            }
        }

        /// <summary>
        /// Getter público de obstáculos en el rango
        /// </summary>
        public List<Circle> TaggedCircleObstacles
        {
            get
            {
                return _taggedObstacles;
            }
        }

        #endregion

        #region Metodos

        /// <summary>
        /// Reservamos los obstáculos que están al alcance de la entidad
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="range"></param>
        internal void TagObstaclesWithinViewRange(SteeringEntity entity, float detectionBoxLength)
        {
            this._taggedObstacles.Clear();

            foreach (Circle obstaculo in this._obstacles)
            {
                // Obtenemos el vector de la entidad al círculo
                Vector2 to = obstaculo.Center - entity.Position;

                // Se tienen en cuenta los ranges de las dos entidades
                float range = detectionBoxLength + obstaculo.Radius;

                // Si el obstáculo está en el alcance de la entidad, la añadimos a la lista de
                // entidades preseleccioadas (_taggedObstacles)
                if (to.LengthSquared() < range * range)
                {
                    this._taggedObstacles.Add(obstaculo);
                }
            }
        }

        /// <summary>
        /// Dibjuar el world por pantalla (sólo se usaría para debugar, normalmente 
        /// no nos interesará dibujar las primitivas, el renderizado "real" será el de los sprites)
        /// </summary>
        /// <param name="gameTime">Tiempo del juego</param>
        /// <param name="spriteBatch">Spritebatch ya inicializado</param>
        public void Draw(SpriteBatch batch, GameTime gameTime, SpriteBatch spriteBatch)
        {
            //SpriteBatch batch = spriteBatch;
            //batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (_renderWalls)
                foreach (Wall pared in _walls)
                {
                    pared.Draw(gameTime, batch);
                }

            if (_renderObstacles)
                foreach (Circle circulo in _obstacles)
                {
                    circulo.Draw(batch);
                }

            //batch.End();
        }

        #endregion

        /// <summary>
        /// Inicialización del "mundo". Normalmente el render estará establecido a false (sólo usar para debug),
        /// el render del mundo "bonito", con sus sprites y demás no corresponde a SteeringLibrary
        /// </summary>
        /// <param name="renderWalls"></param>
        /// <param name="renderObstacles"></param>
        public GameWorld(bool renderWalls, bool renderObstacles)
        {
            this._taggedObstacles = new List<Circle>();
            this._obstacles = new List<Circle>();
            this._walls = new List<Wall>();

            this._renderWalls = renderWalls;
            this._renderObstacles = renderObstacles;
        }

    }
}
