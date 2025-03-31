#region

using System.Collections.Generic;
using DungeonGame.Code.Entities;

#endregion

namespace DungeonGame.Code.Models;

/// <summary>
///     Helper class to reduce parameter count in methods
/// </summary>
public class DungeonRunData
{
    public Dungeon Dungeon { get; set; }
    public bool Success { get; set; }
    public float CurrentHealth { get; set; }
    public PlayerStats PlayerStats { get; set; }
    public float TotalDamageDealt { get; set; }
    public float TotalDamageTaken { get; set; }
    public int EnemiesDefeated { get; set; }
    public IList<string> CombatLog { get; set; }
}
