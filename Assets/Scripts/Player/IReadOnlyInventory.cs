using System;
using System.Collections.Generic;
using LootLab.Equipment;

namespace LootLab.Player
{
    /// <summary>
    /// UI가 인벤토리를 읽기 위한 인터페이스. 변경은 각 서비스(예: EnhanceService)를 통해서만 한다.
    /// </summary>
    public interface IReadOnlyInventory
    {
        /// <summary>보유 장비 목록 (획득 순서)</summary>
        IReadOnlyList<EquipmentInstance> Equipments { get; }

        /// <summary>보유 골드</summary>
        int Gold { get; }

        /// <summary>장비가 추가·삭제되거나 강화로 바뀌었을 때 발생</summary>
        event Action EquipmentsChanged;

        /// <summary>골드가 바뀌었을 때 발생</summary>
        event Action GoldChanged;

        /// <summary>이 인벤토리가 가진 장비인지 확인 (같은 객체인지까지 비교)</summary>
        bool Contains(EquipmentInstance equipment);

        /// <summary>Uid로 보유 장비를 찾음. 없으면 false</summary>
        bool TryGetEquipment(Guid uid, out EquipmentInstance equipment);
    }
}
