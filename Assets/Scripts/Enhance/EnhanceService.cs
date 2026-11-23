using System.Collections.Generic;
using LootLab.Equipment;
using LootLab.Player;
using UnityEngine;

namespace LootLab.Enhance
{
    public class EnhanceService
    {
        private readonly EnhanceCalculator _calculator;

        public EnhanceService(int goldCostPerExp)
        {
            _calculator = new EnhanceCalculator(goldCostPerExp);
        }

        public EnhancePreview Preview(EquipmentInstance target, IReadOnlyList<EquipmentInstance> materials)
        {
            return _calculator.Preview(target, materials);
        }

        public bool TryEnhance(PlayerInventory inventory, EquipmentInstance target, IReadOnlyList<EquipmentInstance> materials, out EnhancePreview result)
        {
            if (inventory == null)
            {
                Debug.LogError("Cannot enhance: inventory is null.");
                result = EnhancePreview.Fail(EnhanceError.NotOwned);
                return false;
            }

            result = _calculator.Preview(target, materials);
            if (!result.IsValid)
            {
                return false;
            }

            if (!inventory.Contains(target))
            {
                result = EnhancePreview.Fail(EnhanceError.NotOwned);
                return false;
            }
            foreach (EquipmentInstance material in materials)
            {
                if (!inventory.Contains(material))
                {
                    result = EnhancePreview.Fail(EnhanceError.NotOwned);
                    return false;
                }
            }
            if (inventory.Gold < result.GoldCost)
            {
                result = EnhancePreview.Fail(EnhanceError.NotEnoughGold);
                return false;
            }

            if (!inventory.TrySpendGold(result.GoldCost))
            {
                Debug.LogError($"Cannot enhance: failed to spend {result.GoldCost} gold after check.");
                result = EnhancePreview.Fail(EnhanceError.NotEnoughGold);
                return false;
            }
            target.AddExp(result.AppliedExp);
            inventory.RemoveEquipments(materials);
            return true;
        }
    }
}
