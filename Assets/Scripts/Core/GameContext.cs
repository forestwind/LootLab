using LootLab.Enhance;
using LootLab.Player;
using UnityEngine;

namespace LootLab.Core
{
    /// <summary>
    /// 게임 시작 시 준비된 인벤토리와 강화 서비스를 담아 두고, UI가 꺼내 쓰는 전역 입구
    /// </summary>
    public static class GameContext
    {
        public static bool IsReady { get; private set; }
        public static IReadOnlyInventory Inventory { get; private set; }
        public static EnhanceService EnhanceService { get; private set; }

        public static void Register(IReadOnlyInventory inventory, EnhanceService enhanceService)
        {
            if (inventory == null || enhanceService == null)
            {
                Debug.LogError("GameContext.Register: inventory or enhance service is null.");
                Clear();
                return;
            }
            if (IsReady)
            {
                Debug.LogWarning("GameContext is already registered. Replacing with new instances.");
            }

            Inventory = inventory;
            EnhanceService = enhanceService;
            IsReady = true;
        }

        public static void Clear()
        {
            Inventory = null;
            EnhanceService = null;
            IsReady = false;
        }

        // 플레이를 시작할 때마다 이전 플레이의 값을 비움
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayStart()
        {
            Clear();
        }
    }
}
