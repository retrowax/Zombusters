using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    public class Arrive : Steering
    {
        public enum Deceleration
        {
            slow = 3,
            normal = 2,
            fast = 1
        };

        public Vector2? Target;
        private Deceleration _deceleration;
        private float _decelerationBeginsAtDistance;

        /// <summary>
        /// Implementación del cálculo de steering par el comportamiento de tipo Seek
        /// </summary>
        /// <param name="entity">Datos básicos de la entidad</param>
        /// <returns></returns>
        internal override Vector2 CalculateSteering(SteeringEntity entity)
        {
            if (Target.HasValue)
            {
                // Vector desde la posición de la entidad hasta el target
                Vector2 ToTarget = Target.Value - entity.Position;

                // Distancia al target
                float distance = ToTarget.Length();

                Vector2 DesiredVelocity;

                // Bajamos la velocidad cuando está dentro del área de "pánico"
                if (distance <= _decelerationBeginsAtDistance)
                {
                    // Suavizamos la deceleración
                    const float DecelerationTweaker = 90.3f;

                    // Calculamos la velocidad necesaria para alcanzar el target dada la desaceleración
                    float speed = distance / ((float)_deceleration * DecelerationTweaker);

                    // La velocidad no puede superar el máximo establecido
                    speed = MathHelper.Min(speed, entity.MaxSpeed);

                    // Utilizamos los valores calculados 
                    DesiredVelocity = ToTarget * speed / distance;
                }
                else
                {
                    // Seek
                    DesiredVelocity = Vector2.Normalize(Target.Value - entity.Position)
                             * entity.MaxSpeed;
                }

                return (DesiredVelocity - entity.Velocity);
            }
            else
            {
                throw new Exception("Debe indicarse el target para el steering de tipo Arrive");
            }
        }

        public Arrive(Deceleration deceleration, float decelerationBeginsAtDistance)
            : base(BehaviorType.arrive)
        {
            this._deceleration = deceleration;
            this._decelerationBeginsAtDistance = decelerationBeginsAtDistance;
            Target = null;
        }


    }
}