using System.Collections.Generic;

namespace DungeonGame;

/// <summary>
/// Dungeon class
/// </summary>
public class Dungeon
{
    public float[] Signature { get; set; }
    public List<Tile> Tiles { get; set; }
    public List<Enemy> Enemies { get; set; }
    public int Difficulty { get; set; } // 1-3
    public int Length { get; set; } // In minutes
        
    public Dungeon()
    {
        Tiles = new List<Tile>();
        Enemies = new List<Enemy>();
    }
}