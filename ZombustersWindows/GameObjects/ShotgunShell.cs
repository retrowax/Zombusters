using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class ShotgunShell
    {
        public List<Vector4> Pellet;
        public Vector2 Position, Direction, Velocity;
        public float Angle;

        static readonly float pelletsSpreadRadians = MathHelper.ToRadians(2.5f);
        const float initialSpeed = 640f;
        protected float radius = 1f;

        public ShotgunShell(Vector2 position, Vector2 direction, float angle, float totalgameseconds)
        {
            // initialize the graphics data
            this.Position = position;
            this.Angle = angle;
            //this.Angle = (float)Math.Acos(Vector2.Dot(Vector2.UnitY, direction));
            //if (direction.X > 0f)
            //{
            //    this.Angle *= -1f;
            //}

            this.Velocity =  initialSpeed * direction;

            // calculate the direction vectors for the second and third projectiles
            float rotation = (float)Math.Acos(Vector2.Dot(new Vector2(0f, -1f),
                direction));
            rotation *= (Vector2.Dot(new Vector2(0f, -1f),
                new Vector2(direction.Y, -direction.X)) > 0f) ? 1f : -1f;
            Vector2 Direction2 = new Vector2(
                 (float)Math.Sin(rotation - pelletsSpreadRadians),
                -(float)Math.Cos(rotation - pelletsSpreadRadians));
            Vector2 Direction3 = new Vector2(
                 (float)Math.Sin(rotation + pelletsSpreadRadians),
                -(float)Math.Cos(rotation + pelletsSpreadRadians));

            Pellet = new List<Vector4>
            {
                new Vector4(GetLeftPellet(Position, angle), totalgameseconds, angle),
                new Vector4(this.Position, totalgameseconds, angle),
                new Vector4(GetRightPellet(Position, angle), totalgameseconds, angle)
            };
        }

        private Vector2 GetLeftPellet(Vector2 position, float angle)
        {
            if (angle > Angles.NORTH[0] && angle < Angles.NORTH[1])
            {
                return new Vector2(position.X - 5, position.Y);
            }
            else if (angle > Angles.NORTH_EAST[0] && angle < Angles.NORTH_EAST[1])
            {
                return new Vector2(position.X, position.Y - 5);
            }
            else if (angle > Angles.EAST[0] && angle < Angles.EAST[1])
            {
                return new Vector2(position.X , position.Y -5);
            }
            else if (angle > Angles.SOUTH_EAST[0] && angle < Angles.SOUTH_EAST[1])
            {
                return new Vector2(position.X + 5, position.Y -5);
            }
            else if (angle > Angles.SOUTH[0] && angle < Angles.SOUTH[1])
            {
                return new Vector2(position.X + 5, position.Y);
            }
            else if (angle > Angles.SOUTH_WEST[0] && angle < Angles.SOUTH_WEST[1])
            {
                return new Vector2(position.X, position.Y + 5);
            }
            else if (angle > Angles.WEST[0] && angle < Angles.WEST[1])
            {
                return new Vector2(position.X, position.Y - 5);
            }
            else if (angle > Angles.NORTH_WEST[0] && angle < Angles.NORTH_WEST[1])
            {
                return new Vector2(position.X, position.Y - 5);
            }
            return position;
        }

        private Vector2 GetRightPellet(Vector2 position, float angle)
        {
            if (angle > Angles.NORTH[0] && angle < Angles.NORTH[1])
            {
                return new Vector2(position.X + 5, position.Y);
            }
            else if (angle > Angles.NORTH_EAST[0] && angle < Angles.NORTH_EAST[1])
            {
                return new Vector2(position.X + 5, position.Y + 5);
            }
            else if (angle > Angles.EAST[0] && angle < Angles.EAST[1])
            {
                return new Vector2(position.X, position.Y + 5);
            }
            else if (angle > Angles.SOUTH_EAST[0] && angle < Angles.SOUTH_EAST[1])
            {
                return new Vector2(position.X + 5, position.Y + 5);
            }
            else if (angle > Angles.SOUTH[0] && angle < Angles.SOUTH[1])
            {
                return new Vector2(position.X - 5, position.Y);
            }
            else if (angle > Angles.SOUTH_WEST[0] && angle < Angles.SOUTH_WEST[1])
            {
                return new Vector2(position.X - 5, position.Y - 5);
            }
            else if (angle > Angles.WEST[0] && angle < Angles.WEST[1])
            {
                return new Vector2(position.X - 5, position.Y - 5);
            }
            else if (angle > Angles.NORTH_WEST[0] && angle < Angles.NORTH_WEST[1])
            {
                return new Vector2(position.X, position.Y - 5);
            }
            return position;
        }
    }
}
