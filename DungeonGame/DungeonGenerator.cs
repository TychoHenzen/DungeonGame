using System;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Dungeon generator
/// </summary>
public static class DungeonGenerator
{
    private static readonly Random _random = new();
        
    public static Dungeon GenerateDungeon(float[] itemSignature)
    {
        // Generate dungeon signature similar to item
        float[] dungeonSignature = ItemGenerator.GenerateSimilarSignature(itemSignature, 0.2f);
            
        var dungeon = new Dungeon
        {
            Signature = dungeonSignature,
            Difficulty = _random.Next(1, 4), // 1-3
            Length = _random.Next(3, 6) // 3-5 minutes
        };
            
        // Generate room tiles based on dungeon signature
        for (int i = 0; i < 5; i++) // 5 rooms per dungeon
        {
            float[] tileSignature = ItemGenerator.GenerateSimilarSignature(dungeonSignature, 0.4f);
                
            // Find the tile type that's most similar to this signature
            string tileType = GetTileTypeForSignature(tileSignature);
                
            dungeon.Tiles.Add(new Tile
            {
                Type = tileType,
                Signature = tileSignature
            });
        }
            
        // Generate enemies based on room tiles
        foreach (var tile in dungeon.Tiles)
        {
            dungeon.Enemies.Add(GenerateEnemyForTile(tile, dungeonSignature));
        }
            
        return dungeon;
    }
        
    private static string GetTileTypeForSignature(float[] signature)
    {
        // This is a simplified version - in a full implementation, 
        // we could match specific signature properties to tile types
        string[] tileTypes = { "Stone", "Water", "Lava", "Ice", "Grass", "Sand", "Crystal", "Wood" };
            
        // For now, use basic random selection with some weighting based on signature
        float temperature = signature[0]; // First dimension is temperature
        float wetness = signature[2];     // Third dimension is wetness
            
        // Weighted random selection based on signature
        if (temperature > 0.5f && wetness < -0.3f)
            return "Lava";
        else if (temperature < -0.5f && wetness > 0.3f)
            return "Ice";
        else if (wetness > 0.5f)
            return "Water";
        else if (temperature > 0 && wetness > 0)
            return "Grass";
        else if (temperature > 0.3f && wetness < 0)
            return "Sand";
        else if (signature[7] > 0.5f) // Resonance
            return "Crystal";
        else if (signature[1] < -0.2f) // Hardness (soft)
            return "Wood";
        else
            return "Stone";
    }
        
    private static Enemy GenerateEnemyForTile(Tile tile, float[] dungeonSignature)
    {
        // Get random enemy type
        var enemyTypes = EnemyTypes.Types.Values.ToList();
        var enemyType = enemyTypes[_random.Next(enemyTypes.Count)];
            
        // Generate enemy signature similar to tile
        float[] enemySignature = ItemGenerator.GenerateSimilarSignature(tile.Signature, 0.3f);
            
        // Generate adjective for enemy name
        string adjective = string.Empty;
            
        for (int i = 0; i < enemySignature.Length; i++)
        {
            if (enemySignature[i] > 0.5f)
            {
                adjective = SignatureDimensions.HighDescriptors[i];
                break;
            }
            else if (enemySignature[i] < -0.5f)
            {
                adjective = SignatureDimensions.LowDescriptors[i];
                break;
            }
        }
            
        string enemyName = string.IsNullOrEmpty(adjective) ? enemyType.Name : $"{adjective} {enemyType.Name}";
            
        // Scale enemy stats based on signature similarity to dungeon
        float similarityFactor = 1 - SignatureDistance(enemySignature, dungeonSignature) / 4;
            
        return new Enemy
        {
            Name = enemyName,
            Type = enemyType.Name,
            Health = enemyType.BaseHealth * (0.8f + similarityFactor * 0.4f),
            Damage = enemyType.BaseDamage * (0.8f + similarityFactor * 0.4f),
            Signature = enemySignature
        };
    }
        
    private static float SignatureDistance(float[] sig1, float[] sig2)
    {
        float sumSquaredDiffs = 0;
            
        for (int i = 0; i < sig1.Length; i++)
        {
            sumSquaredDiffs += (sig1[i] - sig2[i]) * (sig1[i] - sig2[i]);
        }
            
        return (float)Math.Sqrt(sumSquaredDiffs);
    }
}