using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ZombustersWindows.GameObjects;

namespace ZombustersWindows
{
    public struct AvatarState
    {
        public Vector2 position;
        public List<Vector4> bullets;
        public List<ShotgunShell> shotgunbullets;
        public ObjectStatus status;
        public int score;
        public int lives;
        public List<int> ammo;
        public int lifecounter;
        public float deathTimeTotalSeconds;
        public Color color;
        public GunType currentgun;
    }
}
