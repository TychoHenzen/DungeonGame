using System;
using System.Collections.Generic;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Enhanced dungeon class with tile map support
/// </summary>
public class Dungeon
{
    // Original properties
    public Signature Signature { get; set; }
    
    public List<Tile> Tiles { get; }
    public List<Enemy> Enemies { get; set; }
    public int Difficulty { get; set; } // 1-3
    public int Length { get; set; } // In minutes (used for progress tracking)
    
    // New properties for tile map
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] TileMap { get; set; }
    
    // Track exploration state
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public List<Enemy> DefeatedEnemies { get; set; }
    public List<Item> CollectedLoot { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new dungeon
    /// </summary>
    public Dungeon()
    {
        Tiles = [];
        Enemies = [];
        DefeatedEnemies = [];
        CollectedLoot = [];
        
        // Default starting position in the center top
        PlayerX = 0;
        PlayerY = 0;
    }
    
    /// <summary>
    /// Gets the enemy at the specified position, or null if none
    /// </summary>
    public Enemy GetEnemyAt(int x, int y)
    {
        return Enemies.Find(e => e.X == x && e.Y == y && !DefeatedEnemies.Contains(e));
    }
    
    /// <summary>
    /// Checks if a tile is passable
    /// </summary>
    private bool IsTilePassable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return false;
        }

        return TileMap[x, y].IsPassable;
    }
    
    /// <summary>
    /// Moves the player in the specified direction if possible
    /// </summary>
    public bool MovePlayer(int deltaX, int deltaY)
    {
        var newX = PlayerX + deltaX;
        var newY = PlayerY + deltaY;

        if (!IsTilePassable(newX, newY))
        {
            return false;
        }

        PlayerX = newX;
        PlayerY = newY;
        return true;

    }
    
    /// <summary>
    /// Checks if all enemies are defeated
    /// </summary>
    public bool AreAllEnemiesDefeated()
    {
        return DefeatedEnemies.Count >= Enemies.Count;
    }
    
    /// <summary>
    /// Sets the starting position for the player
    /// </summary>
    public void SetStartingPosition()
    {
        // Find a passable tile near the top
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (TileMap[x, y].IsPassable && GetEnemyAt(x, y) == null)
                {
                    PlayerX = x;
                    PlayerY = y;
                    return;
                }
            }
        }
        
        // Fallback to center
        PlayerX = Width / 2;
        PlayerY = 0;
    }
    
}
