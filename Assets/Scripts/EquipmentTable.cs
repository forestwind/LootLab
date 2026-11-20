using System.Collections.Generic;
using UnityEngine;

namespace LootLab.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentTable", menuName = "LootLab/Equipment Table")]
    public class EquipmentTable : ScriptableObject
    {
        [SerializeField] private List<EquipmentData> _items = new List<EquipmentData>();

        private Dictionary<int, EquipmentData> _idToItem;

        public IReadOnlyList<EquipmentData> Items => _items;

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
            _idToItem = new Dictionary<int, EquipmentData>(_items.Count);
            foreach (EquipmentData item in _items)
            {
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
