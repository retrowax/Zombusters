using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    public class ShotgunShell
    {
        public List<Vector3> Pellet;
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

            Pellet = new List<Vector3>();
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
            Pellet.Add(new Vector3(this.Position, totalgameseconds));
        }
    }
}
