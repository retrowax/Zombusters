using Microsoft.Xna.Framework;
using System;
using ZombustersWindows.GameObjects;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    public class GameplayHelper
    {
        public static float BULLET_SPEED = 400;
        public static float PELLET_SPEED = 700;
        public static float PELLET_SPREAD_ANGLE_90 = 0.09f;
        public static float PELLET_SPREAD_ANGLE_45 = 1.2f;

        /// <summary>
        /// Call this method to determine if a bullet hit an enemy
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectBulletCollision(Vector4 bullet, Vector2 enemy, double totalGameSeconds)
        {

            Vector2 pos = FindBulletPosition(bullet, totalGameSeconds);
            if (Vector2.Distance(pos, enemy) < 30)
                return true;

            return false;
        }
        /// <summary>
        /// Call this method to determine if a pellet hit an enemy
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="enemy"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static bool DetectPelletCollision(Vector4 bullet, Vector2 enemy, float angle, int pelletcount, double totalGameSeconds)
        {
            Vector2 pos = FindShotgunBulletPosition(bullet, angle, pelletcount, totalGameSeconds);
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

            if (Angles.IsNorth(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    case 1:
                        pos.X = bullet.X;
                        break;
                    case 2:
                        pos.X = bullet.X + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    default:
                        pos.X = bullet.X;
                        break;
                }
                
                pos.Y = bullet.Y - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsNorthEast(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 1:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 2:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        break;
                    default:
                        break;
                }
            }
            else if (Angles.IsEast(angle))
            {
                pos.X = bullet.X + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                switch (pelletcount)
                {
                    case 0:
                        pos.Y = bullet.Y - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    case 1:
                        pos.Y = bullet.Y;
                        break;
                    case 2:
                        pos.Y = bullet.Y + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    default:
                        pos.Y = bullet.Y;
                        break;
                }
            }
            else if (Angles.IsSouthEast(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 1:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 2:
                        pos.X = bullet.X + (float)Math.Sin(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        break;
                    default:
                        break;
                }
            }
            else if (Angles.IsSouth(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    case 1:
                        pos.X = bullet.X;
                        break;
                    case 2:
                        pos.X = bullet.X + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    default:
                        pos.X = bullet.X;
                        break;
                }
                pos.Y = bullet.Y + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsSouthWest(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        break;
                    case 1:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 2:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        pos.Y = bullet.Y + (float)Math.Cos(angle) + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    default:
                        break;
                }
            }
            else if (Angles.IsWest(angle))
            {
                pos.X = bullet.X - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                switch (pelletcount)
                {
                    case 0:
                        pos.Y = bullet.Y - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    case 1:
                        pos.Y = bullet.Y;
                        break;
                    case 2:
                        pos.Y = bullet.Y + (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_90;
                        break;
                    default:
                        pos.Y = bullet.Y;
                        break;
                }
            }
            else if (Angles.IsNorthWest(angle))
            {
                switch (pelletcount)
                {
                    case 0:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        break;
                    case 1:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    case 2:
                        pos.X = bullet.X - (float)Math.Sin(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z)) * PELLET_SPREAD_ANGLE_45;
                        pos.Y = bullet.Y - (float)Math.Cos(angle) - (PELLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                        break;
                    default:
                        break;
                }
            }

            return pos;
        }

        public static Vector2 FindBulletPosition(Vector4 bullet, double totalGameSeconds)
        {
            Vector2 pos = Vector2.Zero;

            if (Angles.IsNorth(bullet.W))
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsNorthEast(bullet.W))
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y - (float)Math.Cos(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsEast(bullet.W))
            {
                pos.X = bullet.X + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (Angles.IsSouthEast(bullet.W))
            {
                pos.X = bullet.X + (float)Math.Sin(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsSouth(bullet.W))
            {
                pos.X = bullet.X;
                pos.Y = bullet.Y + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsSouthWest(bullet.W))
            {
                pos.X = bullet.X - (float)Math.Sin(bullet.W) - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y + (float)Math.Cos(bullet.W) + (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
            }
            else if (Angles.IsWest(bullet.W))
            {
                pos.X = bullet.X - (BULLET_SPEED * ((float)totalGameSeconds - bullet.Z));
                pos.Y = bullet.Y;
            }
            else if (Angles.IsNorthWest(bullet.W))
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