using System;                    // OperationCanceledException (취소 예외)
using System.Collections.Generic;
using System.Threading;          // CancellationTokenSource, CancellationToken (취소 버튼과 신호선)
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

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float _fillDurationPerLevel = 0.3f;

        private readonly List<EquipmentInstance> _materials = new List<EquipmentInstance>();
        private IReadOnlyInventory _inventory;
        private EnhanceService _enhanceService;
        private EquipmentInstance _target;

        // 연출을 멈출 때 누르는 "취소 버튼". null이 아니면 연출 중이라는 뜻으로도 씀
        private CancellationTokenSource _animationCancellation;

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
            // Awaitable은 꺼져도 계속 돌기 때문에 여기서 직접 취소 신호를 보냄
            StopAnimation();
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
                // 연출 시작 (끝날 때까지 기다리지 않음)
                PlayEnhanceAnimation(result);
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
            // 연출 중에는 게이지와 레벨 글자를 연출이 맡음. 끝나면 다시 호출됨
            if (_animationCancellation != null)
            {
                return;
            }
            _enhanceButton.interactable = false;
            _costText.text = "Cost 0";

            if (_target == null)
            {
                _levelText.text = string.Empty;
                _expText.text = string.Empty;
                SetGauge(0f, 0f);
                ShowMessage(GetErrorMessage(EnhanceError.NoTarget), ErrorColor);
                return;
            }

            LevelInfo current = _target.LevelInfo;
            _levelText.text = $"Lv {current.Level}";
            _expText.text = FormatExp(current);
            SetGauge(current.Progress, current.Progress);

            EnhancePreview preview = _enhanceService.Preview(_target, _materials);
            if (!preview.IsValid)
            {
                ShowMessage(GetErrorMessage(preview.Error), ErrorColor);
                return;
            }

            // 레벨이 오르면 이번 레벨 게이지를 넘어서므로 노란색을 꽉 채움
            float previewProgress = preview.LevelsGained > 0 ? 1f : preview.AfterLevelInfo.Progress;
            SetGauge(current.Progress, previewProgress);
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

        // 실제 강화(골드·경험치·재료 삭제)는 TryEnhance에서 이미 끝났고, 연출은 보여 주기만 함
        //
        // async void:
        // - 시작만 시키고 아무도 기다리지 않는 진입점
        // - 안쪽 함수들은 async Awaitable로 만들어서 await로 기다림
        // - 여기서 처리 못 한 예외는 Unity 콘솔에 오류로 찍힘
        private async void PlayEnhanceAnimation(EnhancePreview result)
        {
            // 이전 연출이 남아 있으면 먼저 취소. 덮어쓰면 이전 연출을 멈출 방법이 사라짐
            StopAnimation();
            
            // Cancel을 통해 await에서 중단
            CancellationTokenSource cancellation = new CancellationTokenSource();
            _animationCancellation = cancellation;
            _enhanceButton.interactable = false;
            ShowMessage(string.Empty, SuccessColor);

            try
            {
                // await: 연출이 끝날 때까지 대기
                await AnimateGaugeAsync(result.BeforeLevelInfo, result.AfterLevelInfo, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // 취소되면 기다리던 await에서 이 예외가 발생
                // 화면이 꺼져서 연출만 멈춘 것이므로 아무것도 하지 않고 종료
                // return해도 아래 finally는 실행됨
                return;
            }
            finally
            {
                // finally는 정상 종료든 취소든 항상 실행됨 → 상태 정리를 여기 한 곳에 모음
                // 코루틴은 꺼지면 그 줄에서 그냥 버려져서 이런 정리 코드가 실행되지 않음
                cancellation.Dispose();

                // StopAnimation이 이미 null로 비웠거나 새 연출이 시작됐을 수 있으므로 "내가 만든 취소 버튼"일 때만 비움
                if (_animationCancellation == cancellation)
                {
                    _animationCancellation = null;
                }
            }

            RefreshPreview();
            ShowMessage($"Enhanced! Lv {result.BeforeLevelInfo.Level} -> Lv {result.AfterLevelInfo.Level}", SuccessColor);
        }

        // 레벨이 오를 때마다 게이지를 끝까지 채우고 비운 뒤, 마지막 레벨의 진행률까지 채움
        //
        // async Awaitable: Unity 6(2023.1~)의 "기다릴 수 있는 함수"
        // 값을 돌려주려면 async Awaitable<int>처럼 씀 (코루틴은 값 반환 불가)
        private async Awaitable AnimateGaugeAsync(LevelInfo before, LevelInfo after, CancellationToken token)
        {
            int level = before.Level;
            float from = before.Progress;
            _levelText.text = $"Lv {level}";

            while (level < after.Level)
            {
                // 이번 레벨 게이지가 다 찰 때까지 기다렸다가 다음 줄(레벨 올리기)로 감
                await FillGaugeAsync(from, 1f, token);
                level++;
                from = 0f;
                _levelText.text = $"Lv {level}";
            }
            await FillGaugeAsync(from, after.Progress, token);
        }

        private async Awaitable FillGaugeAsync(float from, float to, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < _fillDurationPerLevel)
            {
                float progress = Mathf.Lerp(from, to, elapsed / _fillDurationPerLevel);
                SetGauge(progress, progress);

                // 다음 프레임까지 대기 = 코루틴의 yield return null
                // token이 취소되면 여기서 OperationCanceledException이 발생해 위로 전달됨
                await Awaitable.NextFrameAsync(token);
                // 게임 시간 기준: 일시정지(timeScale 0) 중에는 연출도 멈췄다가 풀리면 이어짐
                elapsed += Time.deltaTime;
            }
            SetGauge(to, to);
        }

        // 취소 버튼을 누름. 실제로 멈추는 건 연출 쪽 await에서 일어남
        private void StopAnimation()
        {
            if (_animationCancellation != null)
            {
                _animationCancellation.Cancel();
                _animationCancellation = null;
            }
        }

        private void RefreshGold()
        {
            _goldText.text = $"Gold {_inventory.Gold}";
        }

        // 파란색은 현재 경험치, 뒤의 노란색은 강화 후 예상 경험치
        private void SetGauge(float currentProgress, float previewProgress)
        {
            _currentFill.fillAmount = currentProgress;
            _previewFill.fillAmount = previewProgress;
        }

        private void ShowMessage(string message, Color color)
        {
            _messageText.text = message;
            _messageText.color = color;
        }

        private static string FormatExp(LevelInfo info)
        {
            if (info.IsMaxLevel || info.CurrentLevelMaxExp <= 0)
            {
                return "MAX";
            }
            // 정수 나눗셈으로 내림. 반올림하면 99.7%가 100%로 보여 레벨업으로 오해할 수 있음
            long percent = (long)info.CurrentLevelExp * 100 / info.CurrentLevelMaxExp;
            return $"{info.CurrentLevelExp} / {info.CurrentLevelMaxExp} ({percent}%)";
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
