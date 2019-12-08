namespace ZombustersWindows.Subsystem_Managers
{
    public partial class SubLevel
    {
        public EnemiesCount enemies;
        public SubLevelType subLevel;

        public SubLevel(EnemiesCount enemies, SubLevelType subLevel)
        {
            this.enemies = enemies;
            this.subLevel = subLevel;
        }
    }
}
