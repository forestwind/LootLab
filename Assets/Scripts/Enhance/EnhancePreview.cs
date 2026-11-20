using LootLab.Level;

namespace LootLab.Enhance
{
    public enum EnhanceError
    {
        None,               // 강화 가능
        NoTarget,           // 강화할 장비 없음
        TargetMaxLevel,     // 강화할 장비가 이미 최대 레벨
        NoMaterials,        // 재료 없음
        MaterialIsTarget,   // 강화할 장비를 재료로 선택함
        DuplicateMaterial,  // 같은 재료를 두 번 선택함
    }

    public readonly struct EnhancePreview
    {
        public EnhanceError Error { get; }
        public int GainedExp { get; }
        public int WastedExp { get; }
        public int Cost { get; }
        public LevelInfo BeforeLevelInfo { get; }
        public LevelInfo AfterLevelInfo { get; }

        public bool IsValid => Error == EnhanceError.None;
        public bool HasWastedExp => WastedExp > 0;
        public int LevelsGained => AfterLevelInfo.Level - BeforeLevelInfo.Level;

        public EnhancePreview(EnhanceError error, int gainedExp, int wastedExp, int cost, LevelInfo beforeLevelInfo, LevelInfo afterLevelInfo)
        {
            Error = error;
            GainedExp = gainedExp;
            WastedExp = wastedExp;
            Cost = cost;
            BeforeLevelInfo = beforeLevelInfo;
            AfterLevelInfo = afterLevelInfo;
        }

        // 실패 시 LevelInfo는 default (Level 0). UI는 IsValid를 먼저 확인할 것.
        public static EnhancePreview Fail(EnhanceError error)
        {
            return new EnhancePreview(error, 0, 0, 0, default, default);
        }
    }
}
