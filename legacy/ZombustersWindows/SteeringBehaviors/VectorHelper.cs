using System;
using Microsoft.Xna.Framework;

namespace ZombustersWindows
{
    public static class VectorHelper
    {
        public static Vector2 Normal(Vector2 line)
        {
            float x = line.X; float y = line.Y;
            Vector2 normal = new Vector2();

            if (x != 0)
            {
                normal.X = -y / x;
                normal.Y = 1 / (float)Math.Sqrt(normal.X * normal.X + 1);
                normal.Normalize();
            }

            else if (y != 0)
            {
                normal.Y = 0;
                normal.X = (line.Y < 0) ? 1 : -1;
            }
            else
            {
                normal.X = 1;
                normal.Y = 0;
            }

            if (x < 0)
            {
                normal *= -1;
            }

            return normal;
        }

        /// <summary>
        /// Limita la longitud de un vector en función de un valor dado
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="max">Longitud máxima</param>
        /// <returns></returns>
        public static Vector2 TruncateVector(Vector2 vector, float max)
        {
            if (vector.Length() > max)
            {
                vector.Normalize();

                vector *= max;
            }

            return vector;
        }

        /// <summary>
        /// Obtiene el ángulo en radianes entre dos vectores
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float GetAngle(Vector2 position, Vector2 target)
        {
            return (float)System.Math.Atan2(position.Y - target.Y, position.X - target.X);
        }

        /// <summary>
        /// Devuelve un vector perpendicular al enviado como parámetro
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 GetPerpendicular(Vector2 position)
        {
            return new Vector2(-position.Y, position.X);
        }

        public static Matrix Rotate(Matrix matTransform, Vector2 forward, Vector2 side)
        {
            Matrix mat = new Matrix();

            mat.M11 = forward.X; mat.M12 = forward.Y; mat.M13 = 0;
            mat.M21 = side.X; mat.M22 = side.Y; mat.M23 = 0;
            mat.M31 = 0; mat.M32 = 0; mat.M33 = 1;

            Matrix.Multiply(mat, matTransform);

            return mat;
        }

    }
}
