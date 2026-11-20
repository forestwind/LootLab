using System.Collections.Generic;
using UnityEngine;

namespace LootLab.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentTable", menuName = "LootLab/Equipment Table")]
    public class EquipmentTable : ScriptableObject
    {
        [SerializeField] private List<EquipmentData> _items = new List<EquipmentData>();

        private Dictionary<int, EquipmentData> _idToItem;

        public IReadOnlyList<EquipmentData> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<EquipmentData>();
                }
                return _items;
            }
        }

        public bool TryGet(int id, out EquipmentData data)
        {
            if (_idToItem == null)
            {
                BuildLookup();
            }
            return _idToItem.TryGetValue(id, out data);
        }

        private void BuildLookup()
        {
            _idToItem = new Dictionary<int, EquipmentData>();
            if (_items == null)
            {
                return;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                EquipmentData item = _items[i];
                if (item == null)
                {
                    Debug.LogError($"Equipment table has an empty entry at index {i}", this);
                    continue;
                }
                if (item.Id <= 0)
                {
                    Debug.LogError($"Invalid equipment id {item.Id} at index {i}", this);
                    continue;
                }
                if (_idToItem.ContainsKey(item.Id))
                {
                    Debug.LogError($"Duplicate equipment id: {item.Id}", this);
                    continue;
                }
                _idToItem.Add(item.Id, item);
            }
        }

        private void OnValidate()
        {
            _idToItem = null;
        }
    }
}
