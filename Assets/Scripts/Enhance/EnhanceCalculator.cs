using System;
using System.Collections.Generic;
using LootLab.Equipment;
using LootLab.Level;
using UnityEngine;

namespace LootLab.Enhance
{
    public class EnhanceCalculator
    {
        private readonly int _goldCostPerExp;

        public EnhanceCalculator(int goldCostPerExp)
        {
            if (goldCostPerExp < 0)
            {
                Debug.LogError($"Invalid gold cost per exp: {goldCostPerExp}. Using 0.");
                goldCostPerExp = 0;
            }
            _goldCostPerExp = goldCostPerExp;
        }

        public EnhancePreview Preview(EquipmentInstance target, IReadOnlyList<EquipmentInstance> materials)
        {
            if (target == null)
            {
                return EnhancePreview.Fail(EnhanceError.NoTarget);
            }
            if (target.IsMaxLevel)
            {
                return EnhancePreview.Fail(EnhanceError.TargetMaxLevel);
            }
            if (materials == null || materials.Count == 0)
            {
                return EnhancePreview.Fail(EnhanceError.NoMaterials);
            }

            HashSet<Guid> materialUids = new HashSet<Guid>();
            long totalGainedExp = 0;
            foreach (EquipmentInstance material in materials)
            {
                if (material == null)
                {
                    Debug.LogError("Enhance material list has an empty entry.");
                    continue;
                }
                if (material.Uid == target.Uid)
                {
                    return EnhancePreview.Fail(EnhanceError.MaterialIsTarget);
                }
                if (!materialUids.Add(material.Uid))
                {
                    return EnhancePreview.Fail(EnhanceError.DuplicateMaterial);
                }
                totalGainedExp += material.Data.MaterialExp;
            }

            if (materialUids.Count == 0)
            {
                return EnhancePreview.Fail(EnhanceError.NoMaterials);
            }

            int gainedExp = (int)Math.Min(totalGainedExp, int.MaxValue);
            int remainingExpToMax = target.MaxTotalExp - target.TotalExp;
            int appliedExp = Math.Min(gainedExp, remainingExpToMax);
            int wastedExp = gainedExp - appliedExp;
            int cost = (int)Math.Min((long)gainedExp * _goldCostPerExp, int.MaxValue);

            LevelInfo beforeLevelInfo = target.LevelInfo;
            LevelInfo afterLevelInfo = target.GetLevelInfoAt(target.TotalExp + appliedExp);

            return EnhancePreview.Success(gainedExp, wastedExp, cost, beforeLevelInfo, afterLevelInfo);
        }
    }
}
