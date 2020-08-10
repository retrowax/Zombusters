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
        protected float radius = 1f;

        public ShotgunShell(Vector2 position, Vector2 direction, float angle, float totalgameseconds)
        {
            this.Position = position;
            this.Angle = angle;

            Pellet = new List<Vector4>
            {
                new Vector4(this.Position, totalgameseconds, angle),
                new Vector4(this.Position, totalgameseconds, angle),
                new Vector4(this.Position, totalgameseconds, angle)
            };
        }
    }
}
