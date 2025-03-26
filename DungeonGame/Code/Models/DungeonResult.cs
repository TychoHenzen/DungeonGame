using System.Collections.Generic;
using DungeonGame.Code.Entities;

namespace DungeonGame.Code.Models;

/// <summary>
/// Dungeon result
/// </summary>
public class DungeonResult
{
    public bool Success { get; set; }
    public bool Casualties { get; set; }
    public int Duration { get; set; } // minutes
    public List<Item> Loot { get; set; }
    public List<string> CombatLog { get; set; }
    public PlayerStats PlayerStats { get; set; }
    public CombatStats Stats { get; set; }
}