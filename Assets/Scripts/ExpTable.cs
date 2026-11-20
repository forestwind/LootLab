using System;
using UnityEngine;

namespace LootLab.Equipment
{
    [Serializable]
    public class ExpTable
    {
        [SerializeField] private int _id;
        [Tooltip("Total exp required to reach level 2, 3, 4 ...")]
        [SerializeField] private int[] _totalExpForLevel = new int[0];

        public int Id => _id;
        public int MaxLevel => _totalExpForLevel.Length + 1;

        public LevelInfo GetLevelInfo(int totalExp)
        {
            if (totalExp < 0)
            {
                totalExp = 0;
            }

            int level = 1;
            while (level < MaxLevel && totalExp >= _totalExpForLevel[level - 1])
            {
                level++;
            }

            if (level == MaxLevel)
            {
                return new LevelInfo(level, 0, 0, true);
            }

            int levelStartExp = 0;
            if (level > 1)
            {
                levelStartExp = _totalExpForLevel[level - 2];
            }
            int nextLevelExp = _totalExpForLevel[level - 1];

            return new LevelInfo(level, totalExp - levelStartExp, nextLevelExp - levelStartExp, false);
        }
    }
}
