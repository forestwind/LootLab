using System.Collections.Generic;
using LootLab.Core;
using LootLab.Enhance;
using LootLab.Equipment;
using LootLab.Level;
using LootLab.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LootLab.UI
{
    /// <summary>
    /// 강화 화면: 목록에서 대상·재료를 고르고, 미리보기를 보여 주고, 강화를 요청
    /// </summary>
    public class EnhanceScreen : MonoBehaviour
    {
        private static readonly Color ErrorColor = new Color32(0xFF, 0x6B, 0x6B, 0xFF);
        private static readonly Color SuccessColor = new Color32(0x7C, 0xD6, 0x7C, 0xFF);

        [Header("Selection")]
        [SerializeField] private EquipmentListView _listView;
        [SerializeField] private EquipmentSlotView _targetSlot;
        [SerializeField] private EquipmentSlotView[] _materialSlots;

        [Header("Preview")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _currentFill;
        [SerializeField] private Image _previewFill;
        [SerializeField] private TMP_Text _expText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private Button _enhanceButton;

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
            _enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);

            _listView.Bind(_inventory);
            _inventory.EquipmentsChanged += OnEquipmentsChanged;
            _inventory.GoldChanged += OnGoldChanged;
            RefreshGold();
            RefreshSelection();
        }

        private void OnEnable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged += OnEquipmentsChanged;
                _inventory.GoldChanged += OnGoldChanged;
                RemoveNotOwned();
                RefreshGold();
                RefreshSelection();
            }
        }

        private void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.EquipmentsChanged -= OnEquipmentsChanged;
                _inventory.GoldChanged -= OnGoldChanged;
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

        private void OnGoldChanged()
        {
            RefreshGold();
            RefreshPreview();
        }

        private void OnEnhanceButtonClicked()
        {
            // 강화 도중 UI가 _materials를 비우므로 로직에는 스냅샷을 넘김
            List<EquipmentInstance> materialsSnapshot = new List<EquipmentInstance>(_materials);
            if (_enhanceService.TryEnhance(_target, materialsSnapshot, out EnhancePreview result))
            {
                ShowMessage($"Enhanced! Lv {result.BeforeLevelInfo.Level} -> Lv {result.AfterLevelInfo.Level}", SuccessColor);
            }
            else
            {
                RefreshPreview();
                ShowMessage(GetErrorMessage(result.Error), ErrorColor);
            }
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
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            _enhanceButton.interactable = false;
            _costText.text = "Cost 0";

            if (_target == null)
            {
                _levelText.text = string.Empty;
                _expText.text = string.Empty;
                SetGauge(0f);
                ShowMessage(GetErrorMessage(EnhanceError.NoTarget), ErrorColor);
                return;
            }

            LevelInfo current = _target.LevelInfo;
            _levelText.text = $"Lv {current.Level}";
            _expText.text = FormatExp(current);
            SetGauge(current.Progress);

            EnhancePreview preview = _enhanceService.Preview(_target, _materials);
            if (!preview.IsValid)
            {
                ShowMessage(GetErrorMessage(preview.Error), ErrorColor);
                return;
            }

            _levelText.text = $"Lv {preview.BeforeLevelInfo.Level} -> Lv {preview.AfterLevelInfo.Level}";
            _expText.text = $"{FormatExp(current)}  (+{preview.AppliedExp})";
            _costText.text = $"Cost {preview.GoldCost}";

            if (_inventory.Gold < preview.GoldCost)
            {
                ShowMessage(GetErrorMessage(EnhanceError.NotEnoughGold), ErrorColor);
                return;
            }

            ShowMessage(string.Empty, ErrorColor);
            _enhanceButton.interactable = true;
        }

        private void RefreshGold()
        {
            _goldText.text = $"Gold {_inventory.Gold}";
        }

        // 이중 게이지(강화 후 예상분)는 7단계에서 추가. 지금은 두 겹 모두 현재 값
        private void SetGauge(float progress)
        {
            _currentFill.fillAmount = progress;
            _previewFill.fillAmount = progress;
        }

        private void ShowMessage(string message, Color color)
        {
            _messageText.text = message;
            _messageText.color = color;
        }

        private static string FormatExp(LevelInfo info)
        {
            return info.IsMaxLevel ? "MAX" : $"{info.CurrentLevelExp} / {info.CurrentLevelMaxExp}";
        }

        private static string GetErrorMessage(EnhanceError error)
        {
            switch (error)
            {
                case EnhanceError.NoTarget: return "Select a target";
                case EnhanceError.TargetMaxLevel: return "Target is already max level";
                case EnhanceError.NoMaterials: return "Select materials";
                case EnhanceError.NotOwned: return "Equipment not found in inventory";
                case EnhanceError.NotEnoughGold: return "Not enough gold";
                default: return $"Cannot enhance ({error})";
            }
        }

        private bool HasReferences()
        {
            if (_listView == null || _targetSlot == null || _materialSlots == null || _materialSlots.Length == 0)
            {
                return false;
            }
            if (_levelText == null || _currentFill == null || _previewFill == null || _expText == null
                || _costText == null || _messageText == null || _goldText == null || _enhanceButton == null)
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
