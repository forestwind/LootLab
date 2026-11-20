namespace LootLab.Level
{
    public readonly struct LevelInfo
    {
        public int Level { get; }
        public int CurrentLevelExp { get; }
        public int CurrentLevelMaxExp { get; }
        public bool IsMaxLevel { get; }

        public float Progress
        {
            get
            {
                if (IsMaxLevel || CurrentLevelMaxExp <= 0)
                {
                    return 1f;
                }
                return (float)CurrentLevelExp / CurrentLevelMaxExp;
            }
        }

        public LevelInfo(int level, int currentLevelExp, int currentLevelMaxExp, bool isMaxLevel)
        {
            Level = level;
            CurrentLevelExp = currentLevelExp;
            CurrentLevelMaxExp = currentLevelMaxExp;
            IsMaxLevel = isMaxLevel;
        }
    }
}
