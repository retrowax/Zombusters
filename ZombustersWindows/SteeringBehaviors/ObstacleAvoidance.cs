using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ZombustersWindows
{
    public class ObstacleAvoidance : Steering
    {
        #region Atributos

        private float _minDetectionBoxLength;
        private GameWorld _world;
        private const float BRAKING_WEIGHT = 0.2f;
        private float nearestObstacle;
        private Vector2 nearestSteering;
        public Circle obstacle;
        private const float WALL_DETECTION_FEELER_LENGTH = 10.0f;
        public Vector2[] feelers;
        //private SteeringEntity zombieobstacle;

        #endregion

        #region Metodos

        /// <summary>
        /// Genera sensores de colisiones contra objetos de tipo WALL
        /// </summary>
        /// <param name="entity"></param>
        private void CreateFeelers(SteeringEntity entity)
        {
            feelers = new Vector2[3];
            Matrix rotMatrix;
            Vector2 temp;
            Vector2 direction = entity.Velocity * 1.5f;

            // Feeler apuntando hacia delante
            feelers[0] = entity.Position + WALL_DETECTION_FEELER_LENGTH * direction;

            // Feeler a la izquierda
            temp = direction;
            rotMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-45));
            temp = Vector2.Transform(temp, rotMatrix);

            feelers[1] = entity.Position + WALL_DETECTION_FEELER_LENGTH / 1.5f * temp;

            // Feeler a la derecha
            temp = direction;
            rotMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(45));
            temp = Vector2.Transform(temp, rotMatrix);

            feelers[2] = entity.Position + WALL_DETECTION_FEELER_LENGTH / 1.5f * temp;
        }

        /// <summary>
        /// Calcula el steering necesario para evitar colisiones con obstáculos de tipo lineal (paredes)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Vector2 CalculateSteeringWalls(SteeringEntity entity)
        {
            this.CreateFeelers(entity);

            float DistToThisIP = 0.0f;
            float DistToClosestIP = float.MaxValue;

            //índice a la lista de paredes
            Wall ClosestWall = null;

            Vector2 SteeringForce = Vector2.Zero,
                point = Vector2.Zero,         //contiene info temporal
                ClosestPoint = Vector2.Zero;  //contiene el punto de intersección más cercano

            // Examinamos cada sensor
            for (uint flr = 0; flr < this.feelers.Length; ++flr)
            {
                // Comprobamos cada pared del mundo
                foreach (Wall wall in this._world.Walls)
                {
                    if (Geometry.LineIntersection2D(entity.Position,
                                feelers[flr],
                                wall.From,
                                wall.To,
                                ref DistToThisIP,
                                ref point))
                    {
                        // si es el más cercano lo guardamos
                        if (DistToThisIP < DistToClosestIP)
                        {
                            DistToClosestIP = DistToThisIP;

                            ClosestWall = wall;

                            ClosestPoint = point;
                        }
                    }
                }

                // si se ha detectado un punto de intersección, calculamos la fuerza que alejará al agente
                if (ClosestWall != null)
                {
                    // Calcular por que distancia la posición proyectada del agente
                    // sobrepasará la pared
                    Vector2 OverShoot = this.feelers[flr] - ClosestPoint;

                    // Crear una fuerza hacia la dirección de la normal de la pared
                    // con la magnitud calculada previamente
                    //SteeringForce = ClosestWall.Normal * OverShoot.Length();
                    Vector2 closest = VectorHelper.Normal(ClosestPoint);

                    SteeringForce += closest * OverShoot.Length();

                    if (entity.Velocity.X < 0.0f && SteeringForce.X < 0.0f)
                        SteeringForce.X *= -1;

                    if (entity.Velocity.Y > 0.0f && SteeringForce.Y > 0.0f)
                        SteeringForce.Y *= -1;

                    if (Vector2.Distance(entity.Position, ClosestPoint) <= 5.0f)
                    {
                        Vector2 helper = SteeringForce;

                        helper = VectorHelper.TruncateVector(helper, 10.0f);

                        entity.Position += helper;
                    }

                    break;
                }
            }
            return SteeringForce;
        }

        /// <summary>
        /// Calcula el steering necesario para evitar colisiones con obstáculos de tipo circular
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Vector2 CalculateSteeringCircles(SteeringEntity entity)
        {
            if (this._world.TaggedCircleObstacles != null)
                this._world.TaggedCircleObstacles.Clear();

            // minimum distance to obstacle before avoidance is required
            float minDistanceToCollision = 0.050f * entity.Speed;
            float detectionBoxLength;

            // La caja de detección es proporcional a la velocidad del agente
            detectionBoxLength = this._minDetectionBoxLength +
                (entity.Speed / entity.MaxSpeed) * this._minDetectionBoxLength;

            // 1 - Buscamos obstáculos dentro del alcance del detectionBox
            this._world.TagObstaclesWithinViewRange(entity, detectionBoxLength);

            this.nearestObstacle = float.MaxValue;
            this.nearestSteering = Vector2.Zero;

            // Analizamos sólo los obstáculos dentro del alcance
            foreach (Circle obstacle in this._world.TaggedCircleObstacles)
            {
                // distancia mínima al obstáculo para iniciar la separación del obstáculo
                float minDistanceToCenter = minDistanceToCollision + obstacle.Radius;
                // distancia de contacto
                float totalRadius = obstacle.Radius + entity.BoundingRadius;
                // centro del obstáculo relativo a la posición del vehículo
                Vector2 localOffset = obstacle.Center - entity.Position;

                float forwardComponent = Vector2.Dot(localOffset, entity.Heading);
                Vector2 forwardOffset = forwardComponent * entity.Heading;

                // offset del forward al centro del obstáculo
                Vector2 offForwardOffset = localOffset - forwardOffset;

                // test para ver si el obstáculo es "pisado" por el agente
                bool inCylinder = offForwardOffset.Length() < totalRadius;
                bool nearby = forwardComponent < minDistanceToCenter;
                bool inFront = forwardComponent > 0;

                // si se cumple una de las condiciones, alejamos del centro del obstáculo
                if (inCylinder || inFront || nearby)
                {
                    float length = (offForwardOffset * -1).Length();

                    if (length < nearestObstacle)
                    {
                        nearestObstacle = length;
                        nearestSteering = offForwardOffset * -1;
                        this.obstacle = obstacle;
                    }
                }
            }

            this.EnforceNonPenetrationConstraint(entity, this.obstacle);

            return nearestSteering;
        }

        /// <summary>
        /// Calcula la fuerza steering necesaria para evitar colisiones con obstáculos de tipo circular y lineal
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal override Vector2 CalculateSteering(SteeringEntity entity)
        {
            Vector2 force = Vector2.Zero;

            force += CalculateSteeringCircles(entity);
            force += CalculateSteeringWalls(entity);
            //force += CalculateSteeringEntities(entity);

            return force;
        }

        /// <summary>
        /// Impide que se solapen los agentes con los obstáculos (el weight puede llegar a producir esta situación)
        /// </summary>
        /// <param name="positionEntity"></param>
        /// <param name="obstacle"></param>
        private void EnforceNonPenetrationConstraint(SteeringEntity entity, Circle obstacle)
        {
            if (obstacle != null)
            {
                //calculate the distance between the positions of the entities
                Vector2 ToEntity = entity.Position - obstacle.Center;

                float DistFromEachOther = ToEntity.Length();

                //if this distance is smaller than the sum of their radii then this
                //entity must be moved away in the direction parallel to the
                //ToEntity vector   
                float AmountOfOverLap = entity.BoundingRadius + obstacle.Radius -
                                         DistFromEachOther;

                if (AmountOfOverLap >= 0)
                {
                    //move the entity a distance away equivalent to the amount of overlap.
                    entity.Position = (entity.Position + (ToEntity / DistFromEachOther) * AmountOfOverLap);
                }
            }
        }

        //private void EnforceNonPenetrationConstraint(SteeringEntity entity, Wall obstacle)
        //{
        //    if (obstacle != null)
        //    {
        //        //calculate the distance between the positions of the entities
        //        Vector2 ToEntity = entity.Position - obstacle.Center;

        //        float DistFromEachOther = ToEntity.Length();

        //        //if this distance is smaller than the sum of their radii then this
        //        //entity must be moved away in the direction parallel to the
        //        //ToEntity vector   
        //        float AmountOfOverLap = entity.BoundingRadius + obstacle.Radius -
        //                                 DistFromEachOther;

        //        if (AmountOfOverLap >= 0)
        //        {
        //            //move the entity a distance away equivalent to the amount of overlap.
        //            entity.Position = (entity.Position + (ToEntity / DistFromEachOther) * AmountOfOverLap);
        //        }
        //    }
        //}

        #endregion

        #region Constructor

        /// <summary>
        /// Inicialización mediante constructor
        /// </summary>
        /// <param name="world">Referencia al mundo en el que se encuentra el behavior</param>
        /// <param name="minDetectionBoxLength">Longitud mínima que deberá tener la caja de detección de intersecciones</param>
        public ObstacleAvoidance(ref GameWorld world, float minDetectionBoxLength)
            : base(BehaviorType.obstacle_avoidance)
        {
            this._world = world;
            this._minDetectionBoxLength = minDetectionBoxLength;

        }

        #endregion

    }
}