using System;
using UnityEngine;

namespace LootLab.Equipment
{
    [Serializable]
    public class EquipmentData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EquipmentGrade _grade;
        [SerializeField] private Sprite _icon;
        [Tooltip("Exp given when used as enhance material")]
        [SerializeField, Min(0)] private int _materialExp;
        [SerializeField] private int _expTableId;

        public int Id => _id;
        public string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    return string.Empty;
                }
                return _displayName;
            }
        }
        public EquipmentGrade Grade => _grade;
        public Sprite Icon => _icon;
        public int MaterialExp => _materialExp;
        public int ExpTableId => _expTableId;
    }
}
