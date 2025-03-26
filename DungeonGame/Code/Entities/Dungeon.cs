using System;
using System.Collections.Generic;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Enhanced dungeon class with tile map support
/// </summary>
public class Dungeon
{
    // Original properties
    public float[] Signature { get; set; }
    
    /// <summary>
    /// Gets a Signature object from the raw signature values
    /// </summary>
    /// <returns>A Signature object, or null if the signature is invalid</returns>
    public Signature GetSignatureObject()
    {
        if (Signature == null)
            return null;
            
        try
        {
            return new Signature(Signature);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
    public List<Tile> Tiles { get; set; }
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
    public bool IsExplored { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new dungeon
    /// </summary>
    public Dungeon()
    {
        Tiles = new List<Tile>();
        Enemies = new List<Enemy>();
        DefeatedEnemies = new List<Enemy>();
        CollectedLoot = new List<Item>();
        
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
    public bool IsTilePassable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
            
        return TileMap[x, y].IsPassable;
    }
    
    /// <summary>
    /// Moves the player in the specified direction if possible
    /// </summary>
    public bool MovePlayer(int deltaX, int deltaY)
    {
        int newX = PlayerX + deltaX;
        int newY = PlayerY + deltaY;
        
        if (IsTilePassable(newX, newY))
        {
            PlayerX = newX;
            PlayerY = newY;
            return true;
        }
        
        return false;
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
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
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
    
    /// <summary>
    /// Adds an enemy to the defeated list and collects its loot
    /// </summary>
    public void DefeatEnemy(Enemy enemy)
    {
        if (!DefeatedEnemies.Contains(enemy))
        {
            DefeatedEnemies.Add(enemy);
            
            // Generate and collect loot
            Item loot = enemy.GenerateLoot();
            if (loot != null)
            {
                CollectedLoot.Add(loot);
            }
        }
    }
    
    /// <summary>
    /// Gets a list of possible moves from the current position
    /// </summary>
    public List<(int x, int y)> GetPossibleMoves()
    {
        var moves = new List<(int x, int y)>();
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };
        
        for (int i = 0; i < 4; i++)
        {
            int newX = PlayerX + dx[i];
            int newY = PlayerY + dy[i];
            
            if (IsTilePassable(newX, newY))
            {
                moves.Add((newX, newY));
            }
        }
        
        return moves;
    }
    
    /// <summary>
    /// Gets the current tile the player is standing on
    /// </summary>
    public Tile GetCurrentTile()
    {
        if (PlayerX >= 0 && PlayerX < Width && PlayerY >= 0 && PlayerY < Height)
        {
            return TileMap[PlayerX, PlayerY];
        }
        
        return null;
    }
    
    /// <summary>
    /// Marks a point on the map as visited
    /// </summary>
    public void MarkVisited(int x, int y)
    {
        // This could be expanded to track fog of war/exploration
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            // Future implementation could track visited tiles
        }
    }
}
