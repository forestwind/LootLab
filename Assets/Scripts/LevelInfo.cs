namespace LootLab.Equipment
{
    public readonly struct LevelInfo
    {
        public int Level { get; }
        public int ExpInLevel { get; }
        public int ExpToNextLevel { get; }
        public bool IsMaxLevel { get; }

        public float Progress
        {
            get
            {
                if (IsMaxLevel)
                {
                    return 1f;
                }
                return (float)ExpInLevel / ExpToNextLevel;
            }
        }

        public LevelInfo(int level, int expInLevel, int expToNextLevel, bool isMaxLevel)
        {
            Level = level;
            ExpInLevel = expInLevel;
            ExpToNextLevel = expToNextLevel;
            IsMaxLevel = isMaxLevel;
        }
    }
}
