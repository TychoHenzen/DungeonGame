#region

using System;
using DungeonGame.Code.Systems;

#endregion

namespace DungeonGame.Code.Entities;

/// <summary>
///     Enemy class
/// </summary>
public class Enemy
{
    public string Name { get; set; }
    public string Type { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
    public Signature Signature { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    /// <summary>
    ///     Generates loot when the enemy is defeated
    /// </summary>
    /// <returns>An item as loot, or null if no loot is generated</returns>
    public Item? GenerateLoot()
    {
        // Simple implementation - 50% chance to drop an item
        if (Random.Shared.NextDouble() <= 0.5)
        {
            return null;
        }

        return ItemGenerator.GenerateItemWithSignature(Signature);
    }
}