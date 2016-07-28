using System;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    public static class Geometry
    {
        public static bool LineIntersection2D(Vector2 A,
                               Vector2 B,
                               Vector2 C,
                               Vector2 D,
                               ref float dist,
                               ref Vector2 point)
        {

            float rTop = (A.Y - C.Y) * (D.X - C.X) - (A.X - C.X) * (D.Y - C.Y);
            float rBot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

            float sTop = (A.Y - C.Y) * (B.X - A.X) - (A.X - C.X) * (B.Y - A.Y);
            float sBot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop / rBot;
            float s = sTop / sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = Vector2.Distance(A, B) * r;

                point = A + r * (B - A);

                return true;
            }

            else
            {
                dist = 0;

                return false;
            }
        }
    }
}
