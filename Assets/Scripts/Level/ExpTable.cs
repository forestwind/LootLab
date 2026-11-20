using System;
using UnityEngine;

namespace LootLab.Level
{
    [Serializable]
    public class ExpTable
    {
        [SerializeField] private int _id;
        [Tooltip("Total exp required to reach level 2, 3, 4 ...")]
        [SerializeField] private int[] _totalExpForLevel = new int[0];

        public int Id => _id;

        public int MaxLevel
        {
            get
            {
                if (_totalExpForLevel == null)
                {
                    return 1;
                }
                return _totalExpForLevel.Length + 1;
            }
        }

        public int MaxTotalExp
        {
            get
            {
                if (_totalExpForLevel == null || _totalExpForLevel.Length == 0)
                {
                    return 0;
                }
                return _totalExpForLevel[_totalExpForLevel.Length - 1];
            }
        }

        public bool HasValidOrder()
        {
            if (_totalExpForLevel == null)
            {
                return true;
            }

            int previous = 0;
            foreach (int exp in _totalExpForLevel)
            {
                if (exp <= previous)
                {
                    return false;
                }
                previous = exp;
            }
            return true;
        }

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
