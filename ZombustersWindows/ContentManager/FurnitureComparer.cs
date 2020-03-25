using System.Collections.Generic;

namespace ZombustersWindows.Subsystem_Managers
{
    public class FurnitureComparer : IComparer<Furniture>
    {
        public int Compare(Furniture x, Furniture y)
        {
            if ((x.Position.Y + x.Texture.Height) == (y.Position.Y + y.Texture.Height)) return 0;
            return ((x.Position.Y + x.Texture.Height) > (y.Position.Y + y.Texture.Height)) ? 1 : -1;
        }
    }
}
