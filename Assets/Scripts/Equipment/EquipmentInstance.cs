using LootLab.Level;
using UnityEngine;

namespace LootLab.Equipment
{
    public class EquipmentInstance
    {
        private readonly ExpTable _expTable;

        public int Uid { get; }
        public EquipmentData Data { get; }
        public int TotalExp { get; private set; }

        public LevelInfo LevelInfo => _expTable.GetLevelInfo(TotalExp);
        public int Level => LevelInfo.Level;
        public bool IsMaxLevel => LevelInfo.IsMaxLevel;
        public int MaxTotalExp => _expTable.MaxTotalExp;

        public LevelInfo GetLevelInfoAt(int totalExp) => _expTable.GetLevelInfo(totalExp);

        private EquipmentInstance(int uid, EquipmentData data, ExpTable expTable, int totalExp)
        {
            Uid = uid;
            Data = data;
            _expTable = expTable;
            TotalExp = Mathf.Clamp(totalExp, 0, expTable.MaxTotalExp);
        }

        public static bool TryCreate(int uid, EquipmentData data, ExpTable expTable, int totalExp, out EquipmentInstance instance)
        {
            instance = null;

            if (uid <= 0)
            {
                Debug.LogError($"Invalid equipment uid: {uid}");
                return false;
            }
            if (data == null)
            {
                Debug.LogError($"Equipment {uid} has no data.");
                return false;
            }
            if (expTable == null)
            {
                Debug.LogError($"Equipment {uid} (data {data.Id}) has no exp table.");
                return false;
            }
            if (data.ExpTableId != expTable.Id)
            {
                Debug.LogError($"Equipment {uid} (data {data.Id}) uses exp table {data.ExpTableId}, got {expTable.Id}.");
                return false;
            }
            if (totalExp < 0 || totalExp > expTable.MaxTotalExp)
            {
                Debug.LogWarning($"Equipment {uid} (data {data.Id}) exp {totalExp} is out of range 0~{expTable.MaxTotalExp}. Clamped.");
            }

            instance = new EquipmentInstance(uid, data, expTable, totalExp);
            return true;
        }

        public void AddExp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int remaining = _expTable.MaxTotalExp - TotalExp;
            if (amount >= remaining)
            {
                TotalExp = _expTable.MaxTotalExp;
                return;
            }
            TotalExp += amount;
        }
    }
}
