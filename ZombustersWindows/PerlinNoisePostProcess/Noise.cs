using System;

namespace ZombustersWindows
{
    public static class c3DPerlinNoise
    {
        private static Random rand;
        private static int[] ms_p = new int[512];

        static c3DPerlinNoise()
        {
            Init();
        }

        public static void Init()
        {
            rand = new Random(42);

            int nbVals = (1 << 8);
            int[] ms_perm = new int[nbVals];

            // set values as "unused"
            for (int i = 0; i < nbVals; i++)
            {
                ms_perm[i] = -1;
            }

            for (int i = 0; i < nbVals; i++)
            {
                // for each value, find an empty spot, and place it in it
                while (true)
                {
                    // generate rand # with max a nbvals
                    int p = rand.Next() % nbVals;
                    if (ms_perm[p] == -1)
                    {
                        ms_perm[p] = i;
                        break;
                    }
                }
            }

            // Assign the values in the temporary 256 array to the 512 permuation array.
            for (int i = 0; i < nbVals; i++)
                ms_p[nbVals + i] = ms_p[i] = ms_perm[i];
        }

        private static double _fade(double t)
        {
            return (t * t * t * (t * (t * 6 - 15) + 10));
        }

        private static double _lerp(double t, double a, double b)
        {
            return (a + t * (b - a));
        }

        static double grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public static double noise(double x, double y, double z)
        {
            int X = (int)x & 255,
                Y = (int)y & 255,
                Z = (int)z & 255;

            x -= Math.Floor(x);
            y -= Math.Floor(y);
            z -= Math.Floor(z);

            double u = _fade(x);
            double v = _fade(y);
            double w = _fade(z);

            int A = ms_p[X    ] + Y, AA = ms_p[A] + Z, AB = ms_p[A + 1] + Z;
            int B = ms_p[X + 1] + Y, BA = ms_p[B] + Z, BB = ms_p[B + 1] + Z;

            return _lerp(w, _lerp(v, _lerp(u, grad(ms_p[AA], x, y, z),
                                              grad(ms_p[BA], x - 1, y, z)),
                                     _lerp(u, grad(ms_p[AB], x, y - 1, z),
                                              grad(ms_p[BB], x - 1, y - 1, z))),
                            _lerp(v, _lerp(u, grad(ms_p[AA + 1], x, y, z - 1),
                                              grad(ms_p[BA + 1], x - 1, y, z - 1)),
                                     _lerp(u, grad(ms_p[AB + 1], x, y - 1, z - 1),
                                              grad(ms_p[BB + 1], x - 1, y - 1, z - 1))));

        }

        public static double ridge(double h, float offset)
        {
            h = Math.Abs(h);
            h = offset - h;
            h = h * h;
            return h;
        }

        public static double RidgedMF(double x, double y, double z, int octaves, float lacunarity, float gain, float offset)
        {
            double sum = 0;
            float amplitude = 0.5f;
            float frequency = 1.0f;
            double prev = 1.0f;

            for (int i = 0; i < octaves; i++)
            {
                double n = ridge(noise(x * frequency, y * frequency, z * frequency), offset);
                sum += n * amplitude * prev;
                prev = n;
                frequency *= lacunarity;
                amplitude *= gain;
            }

            return sum;
        }

        public static double fBm(double x, double y, double z, int octaves, float lacunarity, float gain)
        {
            double frequency = 1.0f;
            double amplitude = 0.5f;
            double sum = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                sum += noise(x * frequency, y * frequency, z * frequency) * amplitude;
                frequency *= lacunarity;
                amplitude *= gain;
            }
            return sum;
        }
    }
}