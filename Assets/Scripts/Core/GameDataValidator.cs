using LootLab.Equipment;
using LootLab.Level;
using UnityEngine;

namespace LootLab.Core
{
    public static class GameDataValidator
    {
        public static bool Validate(EquipmentTable equipmentTable, ExpTableSet expTableSet)
        {
            if (equipmentTable == null)
            {
                Debug.LogError("[Fatal] Equipment table is not assigned.");
                return false;
            }
            if (expTableSet == null)
            {
                Debug.LogError("[Fatal] Exp table set is not assigned.");
                return false;
            }
            if (equipmentTable.Ids.Count == 0)
            {
                Debug.LogError("[Fatal] Equipment table has no valid equipment.", equipmentTable);
                return false;
            }

            int usableCount = 0;
            foreach (int id in equipmentTable.Ids)
            {
                equipmentTable.TryGet(id, out EquipmentData data);
                if (!expTableSet.TryGet(data.ExpTableId, out ExpTable expTable))
                {
                    Debug.LogError($"Equipment {id} uses missing exp table {data.ExpTableId}. It cannot be used.", equipmentTable);
                    continue;
                }
                usableCount++;
            }

            if (usableCount == 0)
            {
                Debug.LogError("[Fatal] No equipment has a valid exp table.", equipmentTable);
                return false;
            }
            return true;
        }
    }
}
