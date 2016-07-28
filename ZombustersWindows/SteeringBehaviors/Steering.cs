using System;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    /// <summary>
    /// Clase base de todos los behaviors. Debe ser usada como clase base, no puede inicializarse directamente
    /// </summary>
    public abstract class Steering
    {
        /// <summary>
        /// Tipo del steering concreto
        /// </summary>
        private BehaviorType _type;

        /// <summary>
        /// Tipo del steering concreto, se establece en el constructor
        /// </summary>
        public BehaviorType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Método que deberá ser sobreescrito en cada clase que herede de Steering
        /// </summary>
        virtual internal Vector2 CalculateSteering(SteeringEntity entity)
        {
            throw new Exception(@"El método SteeringLibrary.CalculateSteering() debe ser sobreescrito");
        }

        /// <summary>
        /// Inicializa la clase
        /// </summary>
        /// <param name="type">Tipo de steering</param>
        public Steering(BehaviorType type)
        {
            this._type = type;
        }
    }
}