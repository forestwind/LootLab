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
        [SerializeField, Min(0)] private int _materialExp;

        public int Id => _id;
        public string DisplayName => _displayName;
        public EquipmentGrade Grade => _grade;
        public Sprite Icon => _icon;
        public int MaterialExp => _materialExp;
    }
}
