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
            long totalMaterialExpSum = 0;
            foreach (EquipmentInstance material in materials)
            {
                if (material == null)
                {
                    Debug.LogError("Enhance material list has an empty entry. Filter empty slots before calling Preview.");
                    return EnhancePreview.Fail(EnhanceError.InvalidMaterial);
                }
                if (material.Uid == target.Uid)
                {
                    return EnhancePreview.Fail(EnhanceError.MaterialIsTarget);
                }
                if (!materialUids.Add(material.Uid))
                {
                    return EnhancePreview.Fail(EnhanceError.DuplicateMaterial);
                }
                if (material.Data.MaterialExp <= 0)
                {
                    return EnhancePreview.Fail(EnhanceError.InvalidMaterial);
                }
                totalMaterialExpSum += material.Data.MaterialExp;
            }

            int totalMaterialExp = (int)Math.Min(totalMaterialExpSum, int.MaxValue);
            int remainingExpToMax = target.MaxTotalExp - target.TotalExp;
            int appliedExp = Math.Min(totalMaterialExp, remainingExpToMax);
            int wastedExp = totalMaterialExp - appliedExp;
            int goldCost = (int)Math.Min((long)totalMaterialExp * _goldCostPerExp, int.MaxValue);

            LevelInfo beforeLevelInfo = target.LevelInfo;
            LevelInfo afterLevelInfo = target.GetLevelInfoAt(target.TotalExp + appliedExp);

            return EnhancePreview.Success(totalMaterialExp, appliedExp, wastedExp, goldCost, beforeLevelInfo, afterLevelInfo);
        }
    }
}
