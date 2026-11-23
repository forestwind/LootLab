using LootLab.Level;
using UnityEngine;

namespace LootLab.Enhance
{
    public enum EnhanceError
    {
        Uninitialized,      // 초기화되지 않음 (default가 성공으로 판정되지 않도록 0번에 둠)
        None,              // 강화 가능
        NoTarget,           // 강화할 장비 없음
        TargetMaxLevel,     // 강화할 장비가 이미 최대 레벨
        NoMaterials,        // 재료 없음
        MaterialIsTarget,   // 강화할 장비를 재료로 선택함
        DuplicateMaterial,  // 같은 재료를 두 번 선택함
        InvalidMaterial,    // 재료가 비어 있거나 재료 경험치가 0
        NotOwned,           // 대상이나 재료가 인벤토리에 없음
        NotEnoughGold,      // 골드 부족
    }

    public readonly struct EnhancePreview
    {
        public EnhanceError Error { get; }
        public int TotalMaterialExp { get; }
        public int AppliedExp { get; }
        public int WastedExp { get; }
        public int GoldCost { get; }
        public LevelInfo BeforeLevelInfo { get; }
        public LevelInfo AfterLevelInfo { get; }

        public bool IsValid => Error == EnhanceError.None;
        public bool HasWastedExp => WastedExp > 0;
        public int LevelsGained => AfterLevelInfo.Level - BeforeLevelInfo.Level;

        private EnhancePreview(EnhanceError error, int totalMaterialExp, int appliedExp, int wastedExp, int goldCost, LevelInfo beforeLevelInfo, LevelInfo afterLevelInfo)
        {
            Error = error;
            TotalMaterialExp = totalMaterialExp;
            AppliedExp = appliedExp;
            WastedExp = wastedExp;
            GoldCost = goldCost;
            BeforeLevelInfo = beforeLevelInfo;
            AfterLevelInfo = afterLevelInfo;
        }

        public static EnhancePreview Success(int totalMaterialExp, int appliedExp, int wastedExp, int goldCost, LevelInfo beforeLevelInfo, LevelInfo afterLevelInfo)
        {
            return new EnhancePreview(EnhanceError.None, totalMaterialExp, appliedExp, wastedExp, goldCost, beforeLevelInfo, afterLevelInfo);
        }

        // 실패 시 LevelInfo는 default (Level 0). UI는 IsValid를 먼저 확인할 것.
        public static EnhancePreview Fail(EnhanceError error)
        {
            if (error == EnhanceError.None)
            {
                Debug.LogError("EnhancePreview.Fail called with EnhanceError.None.");
                error = EnhanceError.Uninitialized;
            }
            return new EnhancePreview(error, 0, 0, 0, 0, default, default);
        }
    }
}
