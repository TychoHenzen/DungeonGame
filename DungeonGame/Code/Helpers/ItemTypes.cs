using System.Collections.Generic;
using DungeonGame.Code.Enums;

namespace DungeonGame.Code.Helpers;

/// <summary>
/// Item types
/// </summary>
public static class ItemTypes
{
    public static readonly Dictionary<SlotType, ItemTypeInfo> Types = new()
    {
        { SlotType.Weapon, new ItemTypeInfo { Name = "Sword", BasePower = 10, Slot = SlotType.Weapon } },
        { SlotType.Shield, new ItemTypeInfo { Name = "Shield", BasePower = 8, Slot = SlotType.Shield } },
        { SlotType.Helmet, new ItemTypeInfo { Name = "Helmet", BasePower = 6, Slot = SlotType.Helmet } },
        { SlotType.Armor, new ItemTypeInfo { Name = "Armor", BasePower = 12, Slot = SlotType.Armor } },
        { SlotType.Amulet, new ItemTypeInfo { Name = "Amulet", BasePower = 5, Slot = SlotType.Amulet } },
        { SlotType.Ring, new ItemTypeInfo { Name = "Ring", BasePower = 4, Slot = SlotType.Ring } },
        { SlotType.Boots, new ItemTypeInfo { Name = "Boots", BasePower = 7, Slot = SlotType.Boots } }
    };

    public class ItemTypeInfo
    {
        public string Name { get; set; }
        public int BasePower { get; set; }
        public SlotType Slot { get; set; }
    }
}