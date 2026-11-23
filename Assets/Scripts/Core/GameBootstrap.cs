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

        public bool IsReady { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public EnhanceService EnhanceService { get; private set; }

        private void Awake()
        {
            IsReady = GameDataValidator.Validate(_equipmentTable, _expTableSet);
            if (IsReady && _goldCostPerExp < 0)
            {
                Debug.LogError($"[Fatal] Gold cost per exp must be 0 or more: {_goldCostPerExp}");
                IsReady = false;
            }
            if (!IsReady)
            {
                Debug.LogError("Game data validation failed. Enhance screen is blocked.");
                return;
            }

            Inventory = new PlayerInventory(_equipmentTable, _expTableSet);
            Inventory.AddGold(_startGold);
            foreach (int equipmentId in _startEquipmentIds)
            {
                Inventory.TryAddEquipment(equipmentId, out EquipmentInstance equipment);
            }

            EnhanceService = new EnhanceService(_goldCostPerExp);
        }
    }
}
