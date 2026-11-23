using System;
using System.Collections.Generic;
using LootLab.Equipment;
using LootLab.Level;
using UnityEngine;

namespace LootLab.Player
{
    public class PlayerInventory
    {
        private readonly EquipmentTable _equipmentTable;
        private readonly ExpTableSet _expTableSet;
        private readonly List<EquipmentInstance> _equipments = new List<EquipmentInstance>();
        private readonly Dictionary<Guid, EquipmentInstance> _uidToEquipment = new Dictionary<Guid, EquipmentInstance>();

        public event Action EquipmentsChanged;
        public event Action GoldChanged;

        public int Gold { get; private set; }
        public IReadOnlyList<EquipmentInstance> Equipments => _equipments;

        public PlayerInventory(EquipmentTable equipmentTable, ExpTableSet expTableSet)
        {
            _equipmentTable = equipmentTable;
            _expTableSet = expTableSet;
        }

        public bool TryAddEquipment(int equipmentId, out EquipmentInstance equipment)
        {
            equipment = null;

            if (_equipmentTable == null || _expTableSet == null)
            {
                Debug.LogError("Player inventory has no data tables.");
                return false;
            }
            if (!_equipmentTable.TryGet(equipmentId, out EquipmentData data))
            {
                Debug.LogError($"Cannot add equipment: unknown equipment id {equipmentId}.");
                return false;
            }
            if (!_expTableSet.TryGet(data.ExpTableId, out ExpTable expTable))
            {
                Debug.LogError($"Cannot add equipment {equipmentId}: exp table {data.ExpTableId} not found.");
                return false;
            }

            Guid uid = Guid.NewGuid();
            if (_uidToEquipment.ContainsKey(uid))
            {
                Debug.LogError($"Cannot add equipment {equipmentId}: uid {uid} already exists.");
                return false;
            }
            if (!EquipmentInstance.TryCreate(uid, DateTime.UtcNow, data, expTable, 0, out equipment))
            {
                return false;
            }

            _equipments.Add(equipment);
            _uidToEquipment.Add(equipment.Uid, equipment);
            NotifyEquipmentsChanged();
            return true;
        }

        public bool TryGetEquipment(Guid uid, out EquipmentInstance equipment)
        {
            return _uidToEquipment.TryGetValue(uid, out equipment);
        }

        public bool RemoveEquipment(Guid uid)
        {
            if (!_uidToEquipment.TryGetValue(uid, out EquipmentInstance equipment))
            {
                return false;
            }

            _uidToEquipment.Remove(uid);
            _equipments.Remove(equipment);
            NotifyEquipmentsChanged();
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (amount > int.MaxValue - Gold)
            {
                Gold = int.MaxValue;
            }
            else
            {
                Gold += amount;
            }
            NotifyGoldChanged();
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot spend negative gold: {amount}");
                return false;
            }
            if (amount > Gold)
            {
                return false;
            }

            Gold -= amount;
            NotifyGoldChanged();
            return true;
        }

        private void NotifyEquipmentsChanged()
        {
            if (EquipmentsChanged != null)
            {
                EquipmentsChanged();
            }
        }

        private void NotifyGoldChanged()
        {
            if (GoldChanged != null)
            {
                GoldChanged();
            }
        }
    }
}
