#region

using System.Collections.Generic;
using DungeonGame.Code.Entities;

#endregion

namespace DungeonGame.Code.Models;

/// <summary>
///     Dungeon result
/// </summary>
public class DungeonResult
{
    public bool Success { get; set; }
    public bool Casualties { get; set; }
    public int Duration { get; set; } // minutes
    public IList<Item> Loot { get; set; }
    public IList<string> CombatLog { get; set; }
    public PlayerStats PlayerStats { get; set; }
    public CombatStats Stats { get; set; }
}
