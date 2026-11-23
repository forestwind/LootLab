using System;
using LootLab.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LootLab.UI
{
    /// <summary>
    /// 장비 칸 하나의 표시(아이콘, 이름, 레벨, 등급 테두리, 선택 표시)와 클릭 알림
    /// </summary>
    public class EquipmentSlotView : MonoBehaviour
    {
        private static readonly Color EmptyFrameColor = new Color32(0x4A, 0x4A, 0x56, 0xFF);
        private static readonly Color CommonColor = new Color32(0x9A, 0xA0, 0xA6, 0xFF);
        private static readonly Color UncommonColor = new Color32(0x4C, 0xAF, 0x50, 0xFF);
        private static readonly Color RareColor = new Color32(0x3F, 0x8E, 0xF0, 0xFF);
        private static readonly Color EpicColor = new Color32(0xA2, 0x59, 0xE6, 0xFF);
        private static readonly Color LegendaryColor = new Color32(0xF3, 0x9C, 0x2B, 0xFF);

        [SerializeField] private Image _frameImage;
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _selectedMark;

        private bool _hasReferences;

        public EquipmentInstance Equipment { get; private set; }
        public bool IsEmpty => Equipment == null;

        public event Action<EquipmentSlotView> Clicked;

        private void Awake()
        {
            _hasReferences = _frameImage != null && _button != null && _iconImage != null
                && _nameText != null && _levelText != null && _selectedMark != null;
            if (!_hasReferences)
            {
                Debug.LogError($"EquipmentSlotView on '{name}' has missing references. This slot is disabled.", this);
                return;
            }

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        public void SetEquipment(EquipmentInstance equipment)
        {
            if (equipment == null)
            {
                SetEmpty();
                return;
            }
            if (!_hasReferences)
            {
                return;
            }

            Equipment = equipment;
            Sprite icon = equipment.Data.Icon;
            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;
            _nameText.text = equipment.Data.DisplayName;
            _levelText.text = equipment.IsMaxLevel ? "Lv MAX" : $"Lv {equipment.Level}";
            _frameImage.color = GetGradeColor(equipment.Data.Grade);
        }

        public void SetEmpty()
        {
            Equipment = null;
            if (!_hasReferences)
            {
                return;
            }

            _iconImage.sprite = null;
            _iconImage.enabled = false;
            _nameText.text = string.Empty;
            _levelText.text = string.Empty;
            _frameImage.color = EmptyFrameColor;
            _selectedMark.SetActive(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (!_hasReferences)
            {
                return;
            }
            _selectedMark.SetActive(isSelected);
        }

        private void OnButtonClicked()
        {
            if (Clicked != null)
            {
                Clicked(this);
            }
        }

        private static Color GetGradeColor(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Common: return CommonColor;
                case EquipmentGrade.Uncommon: return UncommonColor;
                case EquipmentGrade.Rare: return RareColor;
                case EquipmentGrade.Epic: return EpicColor;
                case EquipmentGrade.Legendary: return LegendaryColor;
                default:
                    Debug.LogWarning($"Unknown equipment grade: {grade}");
                    return CommonColor;
            }
        }
    }
}
