using System.Collections.Generic;
using LootLab.Equipment;
using LootLab.Player;
using UnityEngine;

namespace LootLab.Enhance
{
    /// <summary>
    /// 강화 미리보기와 실제 강화 적용을 담당
    /// </summary>
    public class EnhanceService
    {
        private readonly PlayerInventory _inventory;
        private readonly EnhanceCalculator _calculator;

        public EnhanceService(PlayerInventory inventory, int goldCostPerExp)
        {
            _inventory = inventory;
            _calculator = new EnhanceCalculator(goldCostPerExp);
        }

        public EnhancePreview Preview(EquipmentInstance target, IReadOnlyList<EquipmentInstance> materials)
        {
            return _calculator.Preview(target, materials);
        }

        /// <summary>
        /// 검사를 모두 통과하면 골드 사용, 경험치 추가, 재료 삭제를 한 번에 적용. 실패하면 아무것도 바꾸지 않음
        /// </summary>
        public bool TryEnhance(EquipmentInstance target, IReadOnlyList<EquipmentInstance> materials, out EnhancePreview result)
        {
            if (_inventory == null)
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

            if (!_inventory.Contains(target))
            {
                result = EnhancePreview.Fail(EnhanceError.NotOwned);
                return false;
            }
            foreach (EquipmentInstance material in materials)
            {
                if (!_inventory.Contains(material))
                {
                    result = EnhancePreview.Fail(EnhanceError.NotOwned);
                    return false;
                }
            }
            if (_inventory.Gold < result.GoldCost)
            {
                result = EnhancePreview.Fail(EnhanceError.NotEnoughGold);
                return false;
            }

            if (!_inventory.TrySpendGold(result.GoldCost))
            {
                Debug.LogError($"Cannot enhance: failed to spend {result.GoldCost} gold after check.");
                result = EnhancePreview.Fail(EnhanceError.NotEnoughGold);
                return false;
            }
            target.AddExp(result.AppliedExp);
            _inventory.RemoveEquipments(materials);
            return true;
        }
    }
}
