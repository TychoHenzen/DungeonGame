using System.Collections.Generic;

namespace DungeonGame;

/// <summary>
/// Item types
/// </summary>
public static class ItemTypes
{
    public static readonly Dictionary<string, ItemTypeInfo> Types = new Dictionary<string, ItemTypeInfo>
    {
        { "Sword", new ItemTypeInfo { Name = "Sword", BasePower = 10, Slot = "weapon" } },
        { "Shield", new ItemTypeInfo { Name = "Shield", BasePower = 8, Slot = "shield" } },
        { "Helmet", new ItemTypeInfo { Name = "Helmet", BasePower = 6, Slot = "helmet" } },
        { "Armor", new ItemTypeInfo { Name = "Armor", BasePower = 12, Slot = "armor" } },
        { "Amulet", new ItemTypeInfo { Name = "Amulet", BasePower = 5, Slot = "amulet" } },
        { "Ring", new ItemTypeInfo { Name = "Ring", BasePower = 4, Slot = "ring" } },
        { "Boots", new ItemTypeInfo { Name = "Boots", BasePower = 7, Slot = "boots" } }
    };
        
    public class ItemTypeInfo
    {
        public string Name { get; set; }
        public int BasePower { get; set; }
        public string Slot { get; set; }
    }
}