using System.Collections.Generic;
using LootLab.Core;
using LootLab.Enhance;
using LootLab.Equipment;
using LootLab.Player;
using UnityEngine;

namespace LootLab.UI
{
    /// <summary>
    /// 강화 화면: 목록에서 대상·재료를 고르고, 미리보기를 보여 주고, 강화를 요청
    /// </summary>
    public class EnhanceScreen : MonoBehaviour
    {
        [SerializeField] private EquipmentListView _listView;
        [SerializeField] private EquipmentSlotView _targetSlot;
        [SerializeField] private EquipmentSlotView[] _materialSlots;

        private readonly List<EquipmentInstance> _materials = new List<EquipmentInstance>();
        private IReadOnlyInventory _inventory;
        private EnhanceService _enhanceService;
        private EquipmentInstance _target;

        private void Start()
        {
            if (!HasReferences())
            {
                Debug.LogError($"EnhanceScreen on '{name}' has missing references.", this);
                return;
            }
            if (!GameContext.IsReady)
            {
                Debug.LogError("EnhanceScreen: GameContext is not ready.", this);
                return;
            }

            _inventory = GameContext.Inventory;
            _enhanceService = GameContext.EnhanceService;

            _listView.EquipmentClicked += OnListEquipmentClicked;
            _targetSlot.Clicked += OnTargetSlotClicked;
            foreach (EquipmentSlotView slot in _materialSlots)
            {
                slot.Clicked += OnMaterialSlotClicked;
            }

            _listView.Bind(_inventory);
            _inventory.EquipmentsChanged += OnEquipmentsChanged;
            RefreshSelection();
        }

        private void OnEnable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged += OnEquipmentsChanged;
                RemoveNotOwned();
                RefreshSelection();
            }
        }

        private void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged -= OnEquipmentsChanged;
            }
        }

        private void OnListEquipmentClicked(EquipmentInstance equipment)
        {
            if (equipment == _target)
            {
                _target = null;
            }
            else if (_materials.Contains(equipment))
            {
                _materials.Remove(equipment);
            }
            else if (_target == null)
            {
                _target = equipment;
            }
            else if (_materials.Count < _materialSlots.Length)
            {
                _materials.Add(equipment);
            }
            RefreshSelection();
        }

        private void OnTargetSlotClicked(EquipmentSlotView slot)
        {
            _target = null;
            RefreshSelection();
        }

        private void OnMaterialSlotClicked(EquipmentSlotView slot)
        {
            if (slot.IsEmpty)
            {
                return;
            }
            _materials.Remove(slot.Equipment);
            RefreshSelection();
        }

        private void OnEquipmentsChanged()
        {
            RemoveNotOwned();
            RefreshSelection();
        }

        // 강화 등으로 인벤토리에서 사라진 장비를 선택에서 뺌
        private void RemoveNotOwned()
        {
            if (_target != null && !_inventory.Contains(_target))
            {
                _target = null;
            }
            _materials.RemoveAll(material => !_inventory.Contains(material));
        }

        private void RefreshSelection()
        {
            _targetSlot.SetEquipment(_target);
            for (int i = 0; i < _materialSlots.Length; i++)
            {
                if (i < _materials.Count)
                {
                    _materialSlots[i].SetEquipment(_materials[i]);
                }
                else
                {
                    _materialSlots[i].SetEmpty();
                }
            }

            List<EquipmentInstance> selected = new List<EquipmentInstance>(_materials);
            if (_target != null)
            {
                selected.Add(_target);
            }
            _listView.SetSelection(selected);
        }

        private bool HasReferences()
        {
            if (_listView == null || _targetSlot == null || _materialSlots == null || _materialSlots.Length == 0)
            {
                return false;
            }
            foreach (EquipmentSlotView slot in _materialSlots)
            {
                if (slot == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
