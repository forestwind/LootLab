using LootLab.Enhance;
using LootLab.Equipment;
using LootLab.Level;
using LootLab.Player;
using UnityEngine;

namespace LootLab.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private EquipmentTable _equipmentTable;
        [SerializeField] private ExpTableSet _expTableSet;

        [Header("Enhance")]
        [SerializeField, Min(0)] private int _goldCostPerExp = 1;

        [Header("Test Start Data")]
        [SerializeField, Min(0)] private int _startGold = 10000;
        [SerializeField] private int[] _startEquipmentIds = { 1001, 1001, 1002, 1003, 1004, 1005, 1006 };

        private PlayerInventory _inventory;

        private void Awake()
        {
            bool isValid = GameDataValidator.Validate(_equipmentTable, _expTableSet);
            if (isValid && _goldCostPerExp < 0)
            {
                Debug.LogError($"[Fatal] Gold cost per exp must be 0 or more: {_goldCostPerExp}");
                isValid = false;
            }
            if (!isValid)
            {
                Debug.LogError("Game data validation failed. Enhance screen is blocked.");
                GameContext.Clear();
                return;
            }

            _inventory = new PlayerInventory(_equipmentTable, _expTableSet);
            _inventory.AddGold(_startGold);
            foreach (int equipmentId in _startEquipmentIds)
            {
                _inventory.TryAddEquipment(equipmentId, out EquipmentInstance equipment);
            }

            EnhanceService enhanceService = new EnhanceService(_inventory, _goldCostPerExp);
            GameContext.Register(_inventory, enhanceService);
        }

        private void OnDestroy()
        {
            if (_inventory != null && GameContext.Inventory == _inventory)
            {
                GameContext.Clear();
            }
        }
    }
}
