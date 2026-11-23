using System;
using System.Collections.Generic;
using LootLab.Equipment;
using LootLab.Player;
using UnityEngine;

namespace LootLab.UI
{
    /// <summary>
    /// 인벤토리 장비 목록을 칸으로 표시하고, 칸 클릭을 장비 단위로 알림
    /// </summary>
    public class EquipmentListView : MonoBehaviour
    {
        [SerializeField] private EquipmentSlotView _slotPrefab;
        [SerializeField] private Transform _content;

        private readonly List<EquipmentSlotView> _slots = new List<EquipmentSlotView>();
        private readonly HashSet<EquipmentInstance> _selected = new HashSet<EquipmentInstance>();
        private IReadOnlyInventory _inventory;

        public event Action<EquipmentInstance> EquipmentClicked;

        public void Bind(IReadOnlyInventory inventory)
        {
            if (_slotPrefab == null || _content == null)
            {
                Debug.LogError($"EquipmentListView on '{name}' has missing references.", this);
                return;
            }
            if (inventory == null)
            {
                Debug.LogError("EquipmentListView.Bind: inventory is null.", this);
                return;
            }

            if (_inventory != null)
            {
                _inventory.EquipmentsChanged -= Refresh;
            }
            _inventory = inventory;
            if (isActiveAndEnabled)
            {
                _inventory.EquipmentsChanged += Refresh;
            }
            Refresh();
        }

        public void SetSelection(IEnumerable<EquipmentInstance> selected)
        {
            _selected.Clear();
            if (selected != null)
            {
                foreach (EquipmentInstance equipment in selected)
                {
                    if (equipment != null)
                    {
                        _selected.Add(equipment);
                    }
                }
            }

            foreach (EquipmentSlotView slot in _slots)
            {
                if (!slot.IsEmpty)
                {
                    slot.SetSelected(_selected.Contains(slot.Equipment));
                }
            }
        }

        private void OnEnable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged += Refresh;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            IReadOnlyList<EquipmentInstance> equipments = _inventory.Equipments;
            while (_slots.Count < equipments.Count)
            {
                EquipmentSlotView slot = Instantiate(_slotPrefab, _content);
                slot.Clicked += OnSlotClicked;
                _slots.Add(slot);
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                EquipmentSlotView slot = _slots[i];
                if (i < equipments.Count)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetEquipment(equipments[i]);
                    slot.SetSelected(_selected.Contains(equipments[i]));
                }
                else
                {
                    slot.SetEmpty();
                    slot.gameObject.SetActive(false);
                }
            }
        }

        private void OnSlotClicked(EquipmentSlotView slot)
        {
            if (slot.IsEmpty)
            {
                return;
            }
            if (EquipmentClicked != null)
            {
                EquipmentClicked(slot.Equipment);
            }
        }
    }
}
