using Microsoft.Xna.Framework;
using System;
using ZombustersWindows.GameObjects;

namespace ZombustersWindows
{
    public class GameplayHelper
    {
        public static float BULLET_SPEED = 400;

        /// <summary>
        /// Call this method to determine if a bullet hit an enemy
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectCollision(Vector4 bullet, Vector2 enemy, double totalGameSeconds)
        {

            Vector2 pos = FindBulletPosition(bullet, totalGameSeconds);
            if (Vector2.Distance(pos, enemy) < 30)
                return true;

            return false;
        }
        /// <summary>
        /// Call this method to determine if the enemy crashed into the player
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectCrash(Avatar player, Vector2 enemy)
        //EnemyInfo enemyType)
        {
            if (player.status == ObjectStatus.Active)
            {
                float distance = Vector2.Distance(player.position, enemy);
                return (distance < Avatar.CrashRadius + 20.0f);// + enemyType.CrashRadius);
            }
            return false;
        }

        public static Vector2 FindShotgunBulletPosition(Vector4 bullet, float angle, int pelletcount, double totalGameSeconds)
        {
            Vector2 pos = Vector2.Zero;
            float overAngle = 0;
            float pelletsSpreadRadians = MathHelper.ToRadians(2.5f);

            switch (pelletcount)
            {
                case 0:
                    overAngle = 0;
                    break;
                case 1:
                    overAngle += 0.25f;
                    break;
                case 2:
                    overAngle -= 0.25f;
                    break;
                default:
                    overAngle = 0;
                    break;
            }

            if (angle > -0.3925f && angle < 0.3925f) //NORTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 0.3925f && angle < 1.1775f) //NORTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(angle) + overAngle + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(angle) + overAngle - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 1.1775f && angle < 1.9625f) //EAST
            {
                pos.X = bullet.X + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (angle > 1.19625f && angle < 2.7275f) //SOUTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(angle) + overAngle + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(angle) + overAngle + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle > 2.7275f || angle < -2.7275f) //SOUTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle < -1.9625f && angle > -2.7275f) //SOUTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(angle) + overAngle - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(angle) + overAngle + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (angle < -1.1775f && angle > -1.9625f) //WEST
            {
                pos.X = bullet.X - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }

            else if (angle < -0.3925f && angle > -1.1775f) //NORTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(angle) + overAngle - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(angle) + overAngle - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }

            return pos;
        }

        public static Vector2 FindBulletPosition(Vector4 bullet, double totalGameSeconds)
        {
            Vector2 pos = Vector2.Zero;

            //pos.X = bullet.X;
            //pos.Y = bullet.Y - (bulletspeed * ((float)totalGameSeconds - bullet.Z));

            if (bullet.W > -0.3925f && bullet.W < 0.3925f) //NORTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 0.3925f && bullet.W < 1.1775f) //NORTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 1.1775f && bullet.W < 1.9625f) //EAST
            {
                pos.X = bullet.X + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (bullet.W > 1.19625f && bullet.W < 2.7275f) //SOUTH-EAST
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W > 2.7275f || bullet.W < -2.7275f) //SOUTH
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W < -1.9625f && bullet.W > -2.7275f) //SOUTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (bullet.W < -1.1775f && bullet.W > -1.9625f) //WEST
            {
                pos.X = bullet.X - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (bullet.W < -0.3925f && bullet.W > -1.1775f) //NORTH-WEST
            {
                pos.X = bullet.X - (float)Math.Sin(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }

            return pos;
        }

        public static float Move(float input, float ElapsedGameSeconds)
        {
            return input * ElapsedGameSeconds * Avatar.PixelsPerSecond;
        }

        public static float DistanceLineSegmentToPoint(Vector2 A, Vector2 B, Vector2 p)
        {
            //get the normalized line segment vector
            Vector2 v = B - A;
            v.Normalize();

            //determine the point on the line segment nearest to the point p
            float distanceAlongLine = Vector2.Dot(p, v) - Vector2.Dot(A, v);
            Vector2 nearestPoint;
            if (distanceAlongLine < 0)
            {
                //closest point is A
                nearestPoint = A;
            }
            else if (distanceAlongLine > Vector2.Distance(A, B))
            {
                //closest point is B
                nearestPoint = B;
            }
            else
            {
                //closest point is between A and B... A + d  * ( ||B-A|| )
                nearestPoint = A + distanceAlongLine * v;
            }

            //Calculate the distance between the two points
            float actualDistance = Vector2.Distance(nearestPoint, p);
            return actualDistance;

        }

        private static float AngleFromAxis(Vector2 axis, Vector2 vector)
        {
            float retval = AbsoluteAngle(vector) - AbsoluteAngle(axis);
            if (retval > MathHelper.Pi)
                return retval - MathHelper.TwoPi;
            if (retval < -MathHelper.Pi)
                return retval + MathHelper.TwoPi;

            return retval;
        }
        private static float AbsoluteAngle(Vector2 a)
        {
            return (float)Math.Atan2(a.Y, a.X);
        }
        private static Vector2 VectorFromAngle(float a)
        {
            return new Vector2((float)Math.Sin(a), (float)-Math.Cos(a));
        }
    }
}