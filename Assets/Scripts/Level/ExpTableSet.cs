using System.Collections.Generic;
using UnityEngine;

namespace LootLab.Level
{
    [CreateAssetMenu(fileName = "ExpTableSet", menuName = "LootLab/Exp Table Set")]
    public class ExpTableSet : ScriptableObject
    {
        [SerializeField] private List<ExpTable> _tables = new List<ExpTable>();

        private Dictionary<int, ExpTable> _idToTable;

        public IReadOnlyList<ExpTable> Tables
        {
            get
            {
                if (_tables == null)
                {
                    _tables = new List<ExpTable>();
                }
                return _tables;
            }
        }

        public bool TryGet(int id, out ExpTable table)
        {
            if (_idToTable == null)
            {
                BuildLookup();
            }
            return _idToTable.TryGetValue(id, out table);
        }

        private void BuildLookup()
        {
            _idToTable = new Dictionary<int, ExpTable>();
            if (_tables == null)
            {
                return;
            }

            for (int i = 0; i < _tables.Count; i++)
            {
                ExpTable table = _tables[i];
                if (table == null)
                {
                    Debug.LogError($"Exp table set has an empty entry at index {i}", this);
                    continue;
                }
                if (table.Id <= 0)
                {
                    Debug.LogError($"Invalid exp table id {table.Id} at index {i}", this);
                    continue;
                }
                if (_idToTable.ContainsKey(table.Id))
                {
                    Debug.LogError($"Duplicate exp table id: {table.Id}", this);
                    continue;
                }
                if (!table.HasValidOrder())
                {
                    Debug.LogError($"Exp table {table.Id} must be in ascending order", this);
                    continue;
                }
                if (table.MaxLevel <= 1)
                {
                    Debug.LogError($"Exp table {table.Id} has no levels", this);
                    continue;
                }
                _idToTable.Add(table.Id, table);
            }
        }

        private void OnValidate()
        {
            _idToTable = null;
        }
    }
}
