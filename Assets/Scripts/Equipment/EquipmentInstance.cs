using System;
using LootLab.Level;
using UnityEngine;

namespace LootLab.Equipment
{
    public class EquipmentInstance
    {
        private readonly ExpTable _expTable;

        public Guid Uid { get; }
        public DateTime AcquiredAtUtc { get; }
        public EquipmentData Data { get; }
        public int TotalExp { get; private set; }

        public string ShortUid => Uid.ToString("N").Substring(0, 8);
        public LevelInfo LevelInfo => _expTable.GetLevelInfo(TotalExp);
        public int Level => LevelInfo.Level;
        public bool IsMaxLevel => LevelInfo.IsMaxLevel;
        public int MaxTotalExp => _expTable.MaxTotalExp;

        public LevelInfo GetLevelInfoAt(int totalExp) => _expTable.GetLevelInfo(totalExp);

        private EquipmentInstance(Guid uid, DateTime acquiredAtUtc, EquipmentData data, ExpTable expTable, int totalExp)
        {
            Uid = uid;
            AcquiredAtUtc = acquiredAtUtc;
            Data = data;
            _expTable = expTable;
            TotalExp = Mathf.Clamp(totalExp, 0, expTable.MaxTotalExp);
        }

        public static bool TryCreate(Guid uid, DateTime acquiredAtUtc, EquipmentData data, ExpTable expTable, int totalExp, out EquipmentInstance instance)
        {
            instance = null;

            if (uid == Guid.Empty)
            {
                Debug.LogError("Equipment uid is empty.");
                return false;
            }

            string shortUid = uid.ToString("N").Substring(0, 8);
            if (acquiredAtUtc.Kind != DateTimeKind.Utc)
            {
                Debug.LogError($"Equipment {shortUid} acquired time must be UTC, got {acquiredAtUtc.Kind}.");
                return false;
            }
            if (data == null)
            {
                Debug.LogError($"Equipment {shortUid} has no data.");
                return false;
            }
            if (expTable == null)
            {
                Debug.LogError($"Equipment {shortUid} (data {data.Id}) has no exp table.");
                return false;
            }
            if (data.ExpTableId != expTable.Id)
            {
                Debug.LogError($"Equipment {shortUid} (data {data.Id}) uses exp table {data.ExpTableId}, got {expTable.Id}.");
                return false;
            }
            if (totalExp < 0 || totalExp > expTable.MaxTotalExp)
            {
                Debug.LogWarning($"Equipment {shortUid} (data {data.Id}) exp {totalExp} is out of range 0~{expTable.MaxTotalExp}. Clamped.");
            }

            instance = new EquipmentInstance(uid, acquiredAtUtc, data, expTable, totalExp);
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
