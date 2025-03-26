using System.Collections.Generic;
using DungeonGame.Code.Helpers;
using DungeonGame.Code.Models;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Player class
/// </summary>
public class Player
{
    private Dictionary<string, Item> _equippedItems;
    private static readonly GameConstants _constants = GameConstants.Default;
        
    public Player()
    {
        _equippedItems = new Dictionary<string, Item>
        {
            { "weapon", null },
            { "shield", null },
            { "helmet", null },
            { "armor", null },
            { "amulet", null },
            { "ring", null },
            { "boots", null }
        };
    }

    public string Name { get; set; }

    public void EquipItem(Item item)
    {
        if (item != null && item.Slot != null && _equippedItems.ContainsKey(item.Slot.ToLower()))
        {
            _equippedItems[item.Slot.ToLower()] = item;
        }
    }
        
    public void UnequipItem(string slot)
    {
        if (_equippedItems.ContainsKey(slot.ToLower()))
        {
            _equippedItems[slot.ToLower()] = null;
        }
    }
        
    public Item GetEquippedItem(string slot)
    {
        return _equippedItems.ContainsKey(slot.ToLower()) ? _equippedItems[slot.ToLower()] : null;
    }
        
    public Dictionary<string, Item> GetEquippedItems()
    {
        return _equippedItems;
    }
        
    public PlayerStats CalculateStats()
    {
        // Base stats
        var stats = new PlayerStats
        {
            MaxHealth = _constants.PlayerDefenseFactor,
            Attack = _constants.BasePlayerAttack,
            Defense = _constants.BasePlayerDefense,
            Speed = _constants.BasePlayerSpeed
        };
            
        // Add equipment bonuses
        foreach (var item in _equippedItems.Values)
        {
            if (item != null)
            {
                // Basic power contribution
                stats.Attack += item.Power * 0.6f;
                stats.Defense += item.Power * 0.4f;
                    
                // Signature-based bonuses
                if (item.Signature != null)
                {
                    // Temperature (high = fire damage, low = ice defense)
                    if (item.Signature[0] > 0.5f) stats.Attack += item.Power * 0.2f;
                    if (item.Signature[0] < -0.5f) stats.Defense += item.Power * 0.2f;
                        
                    // Hardness (high = defense, low = speed)
                    if (item.Signature[1] > 0.5f) stats.Defense += item.Power * 0.3f;
                    if (item.Signature[1] < -0.5f) stats.Speed += item.Power * 0.3f;
                }
            }
        }
            
        return stats;
    }
}
