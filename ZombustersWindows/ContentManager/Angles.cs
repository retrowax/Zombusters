namespace ZombustersWindows.Subsystem_Managers
{
    public static class Angles
    {
        public static readonly float[] NORTH = { -0.3925f, 0.3925f };
        public static readonly float[] NORTH_EAST = { 0.3925f, 1.1775f };
        public static readonly float[] EAST = { 1.1775f, 1.9625f };
        public static readonly float[] SOUTH_EAST = { 1.19625f, 2.7275f };
        public static readonly float[] SOUTH = { 2.7275f, -2.7275f };
        public static readonly float[] SOUTH_WEST = { -1.9625f, -2.7275f };
        public static readonly float[] WEST = { -1.1775f, -1.9625f };
        public static readonly float[] NORTH_WEST = { -0.3925f, -1.1775f };

        public static bool IsNorth(float angle)
        {
            if (angle > NORTH[0] && angle < NORTH[1])
            {
                return true;
            } else
            {
                return false;
            }
        }

        public static bool IsNorthEast(float angle)
        {
            if (angle > Angles.NORTH_EAST[0] && angle < Angles.NORTH_EAST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsEast(float angle)
        {
            if (angle > Angles.EAST[0] && angle < Angles.EAST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsSouthEast(float angle)
        {
            if (angle > Angles.SOUTH_EAST[0] && angle < Angles.SOUTH_EAST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsSouth(float angle)
        {
            if (angle > Angles.SOUTH[0] || angle < Angles.SOUTH[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsSouthWest(float angle)
        {
            if (angle < Angles.SOUTH_WEST[0] && angle > Angles.SOUTH_WEST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsWest(float angle)
        {
            if (angle < Angles.WEST[0] && angle > Angles.WEST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsNorthWest(float angle)
        {
            if (angle < Angles.NORTH_WEST[0] && angle > Angles.NORTH_WEST[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
