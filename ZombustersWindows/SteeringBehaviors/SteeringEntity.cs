using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    public class SteeringEntity
    {
        private Vector2 _velocity;
        private Vector2 _position;
        private Vector2 _previousPosition;
        private float _maxSpeed;
        private float _boundingRadius;
        private float _width;
        private float _height;

        /// <summary>
        /// Ángulo hacia el cual "mira" la entidad
        /// </summary>
        public float Angle
        {
            get
            {
                return VectorHelper.GetAngle(_previousPosition, _position);
            }
        }

        public float Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        /// <summary>
        /// Radio del círculo bounding
        /// </summary>
        public float BoundingRadius
        {
            get { return _boundingRadius; }
            set { _boundingRadius = value; }
        }

        /// <summary>
        /// Vector velocidad
        /// </summary>
        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        /// <summary>
        /// Orientación de la entidad
        /// </summary>
        public Vector2 Heading
        {
            get
            {
                return new Vector2(
                    (float)Math.Abs(Math.Cos(this.Angle)), (float)Math.Abs(Math.Sin(this.Angle)));
            }
        }

        /// <summary>
        /// Perpendicular al vector heading
        /// </summary>
        public Vector2 Side
        {
            get
            {
                return VectorHelper.GetPerpendicular(this.Heading);
            }
        }

        /// <summary>
        /// Velocidad
        /// </summary>
        public float Speed
        {
            get { return _velocity.Length(); }
        }

        /// <summary>
        /// Posición de la entidad en el "mundo 2d"
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _previousPosition = _position;
                _position = value;
            }
        }

        public Vector2 PreviousPosition
        {
            get { return _previousPosition; }
        }

        /// <summary>
        /// Velocidad máxima de la entidad
        /// </summary>
        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

    }
}