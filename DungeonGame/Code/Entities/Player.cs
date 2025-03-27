#region

using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Models;
using DungeonGame.Const;

#endregion

namespace DungeonGame.Code.Entities;

/// <summary>
///     Player class
/// </summary>
public class Player
{
    private readonly Dictionary<SlotType, Item?> _equippedItems = new()
    {
        { SlotType.Weapon, null },
        { SlotType.Shield, null },
        { SlotType.Helmet, null },
        { SlotType.Armor, null },
        { SlotType.Amulet, null },
        { SlotType.Ring, null },
        { SlotType.Boots, null }
    };

    public string Name { get; set; }

    public void EquipItem(Item item)
    {
        if (_equippedItems.ContainsKey(item.Slot))
        {
            _equippedItems[item.Slot] = item;
        }
    }

    public void UnequipItem(SlotType slot)
    {
        if (_equippedItems.ContainsKey(slot))
        {
            _equippedItems[slot] = null;
        }
    }

    public Item GetEquippedItem(SlotType slot)
    {
        return _equippedItems.GetValueOrDefault(slot, null);
    }

    public Dictionary<SlotType, Item?> GetEquippedItems()
    {
        return _equippedItems;
    }

    public PlayerStats CalculateStats()
    {
        // Base stats
        var stats = new PlayerStats
        {
            MaxHealth = Constants.Game.BasePlayerHealth,
            Attack = Constants.Game.BasePlayerAttack,
            Defense = Constants.Game.BasePlayerDefense,
            Speed = Constants.Game.BasePlayerSpeed
        };

        // Add equipment bonuses
        foreach (var item in _equippedItems.Values
                     .Where(item => item?.Signature != null))
        {
            // Basic power contribution
            stats.Attack += item.Power * 0.6f;
            stats.Defense += item.Power * 0.4f;

            // Signature-based bonuses

            // Temperature (high = fire damage, low = ice defense)
            if (item.Signature[0] > Constants.Game.SignatureHighThreshold)
            {
                stats.Attack += item.Power * 0.2f;
            }

            if (item.Signature[0] < Constants.Game.SignatureLowThreshold)
            {
                stats.Defense += item.Power * 0.2f;
            }

            // Hardness (high = defense, low = speed)
            if (item.Signature[1] > Constants.Game.SignatureHighThreshold)
            {
                stats.Defense += item.Power * 0.3f;
            }

            if (item.Signature[1] < Constants.Game.SignatureLowThreshold)
            {
                stats.Speed += item.Power * 0.3f;
            }
        }

        return stats;
    }
}