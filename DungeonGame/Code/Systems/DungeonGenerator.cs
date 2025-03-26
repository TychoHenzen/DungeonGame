using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Enhanced dungeon generator that creates a proper tile map
/// </summary>
public static class DungeonGenerator
{
    private static readonly Random _random = new();
    private const int DefaultMapWidth = 20;
    private const int DefaultMapHeight = 15;
    
    public static Dungeon GenerateDungeon(float[] itemSignature)
    {
        // Generate dungeon signature similar to item
        float[] dungeonSignature = ItemGenerator.GenerateSimilarSignature(itemSignature, 0.2f);
        
        var dungeon = new Dungeon
        {
            Signature = dungeonSignature,
            Difficulty = _random.Next(1, 4), // 1-3
            Length = _random.Next(3, 6), // 3-5 minutes (we'll keep this for progress tracking)
            Width = DefaultMapWidth,
            Height = DefaultMapHeight
        };
        
        // Generate tile map using Perlin-like noise based on signature
        GenerateTileMap(dungeon, dungeonSignature);
        
        // Populate dungeon with enemies
        PopulateEnemies(dungeon);
        
        return dungeon;
    }
    
    private static void GenerateTileMap(Dungeon dungeon, float[] signature)
    {
        dungeon.TileMap = new Tile[dungeon.Width, dungeon.Height];
        
        // Use temperature and wetness as primary factors for tile distribution
        float temperature = signature[0]; // First dimension
        float wetness = signature[2];     // Third dimension
        
        // Create basic noise for map generation
        var noiseMap = GenerateSimpleNoise(dungeon.Width, dungeon.Height);
        
        // Fill the map with tiles
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                // Combine noise with signature to determine tile type
                float noiseValue = noiseMap[x, y];
                float adjustedTemp = temperature + noiseValue * 0.5f;
                float adjustedWet = wetness + noiseValue * 0.3f;
                
                // Generate tile signature from the base signature with noise influence
                float[] tileSignature = GenerateTileSignature(signature, noiseValue);
                
                // Determine tile type based on adjusted values
                string tileType = GetTileTypeForValues(adjustedTemp, adjustedWet, noiseValue);
                
                // Create the tile
                dungeon.TileMap[x, y] = new Tile
                {
                    Type = tileType,
                    Signature = tileSignature,
                    X = x,
                    Y = y,
                    IsPassable = tileType != "Lava" && tileType != "Water" // Simple passability
                };
                
                // Add to tile list for compatibility with existing code
                dungeon.Tiles.Add(dungeon.TileMap[x, y]);
            }
        }
        
        // Create pathways to ensure dungeon is traversable
        EnsureTraversableMap(dungeon);
    }
    
    private static float[,] GenerateSimpleNoise(int width, int height)
    {
        var noise = new float[width, height];
        
        // Simple implementation of noise (this could be replaced with proper Perlin noise)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Generate a smooth noise value between -1 and 1
                noise[x, y] = (float)(_random.NextDouble() * 2 - 1);
            }
        }
        
        // Smooth the noise (simple box blur)
        var smoothedNoise = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sum = 0;
                int count = 0;
                
                // Average of neighbors
                for (int nx = Math.Max(0, x - 1); nx <= Math.Min(width - 1, x + 1); nx++)
                {
                    for (int ny = Math.Max(0, y - 1); ny <= Math.Min(height - 1, y + 1); ny++)
                    {
                        sum += noise[nx, ny];
                        count++;
                    }
                }
                
                smoothedNoise[x, y] = sum / count;
            }
        }
        
        return smoothedNoise;
    }
    
    private static float[] GenerateTileSignature(float[] baseSignature, float noiseValue)
    {
        float[] tileSignature = new float[baseSignature.Length];
        
        for (int i = 0; i < baseSignature.Length; i++)
        {
            // Vary the signature based on noise
            tileSignature[i] = Math.Clamp(baseSignature[i] + noiseValue * 0.4f, -1f, 1f);
        }
        
        return tileSignature;
    }
    
    private static string GetTileTypeForValues(float temperature, float wetness, float noise)
    {
        // Determine tile type based on temperature and wetness with some randomness
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
        else if (noise > 0.5f) // Use noise for crystal distribution
            return "Crystal";
        else if (noise < -0.5f) // Use noise for wood distribution
            return "Wood";
        else
            return "Stone";
    }
    
    private static void EnsureTraversableMap(Dungeon dungeon)
    {
        // Create a simple path from top to bottom to ensure dungeon is traversable
        int pathX = dungeon.Width / 2;
        
        for (int y = 0; y < dungeon.Height; y++)
        {
            // Make this path passable
            dungeon.TileMap[pathX, y].IsPassable = true;
            
            // Set simple stone path
            dungeon.TileMap[pathX, y].Type = "Stone";
            
            // Randomly adjust path to make it more natural
            if (_random.Next(100) < 40 && y < dungeon.Height - 1)
            {
                pathX = Math.Clamp(pathX + _random.Next(-1, 2), 1, dungeon.Width - 2);
            }
        }
    }
    
    private static void PopulateEnemies(Dungeon dungeon)
    {
        // Clear existing enemies (in case this is a regeneration)
        dungeon.Enemies.Clear();
        
        // Determine number of enemies based on difficulty
        int enemyCount = 3 + dungeon.Difficulty * 2;
        
        // Create a list of open positions where enemies can spawn
        var openPositions = new List<(int x, int y)>();
        
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                if (dungeon.TileMap[x, y].IsPassable)
                {
                    openPositions.Add((x, y));
                }
            }
        }
        
        // Shuffle the positions
        openPositions = openPositions.OrderBy(_ => _random.Next()).ToList();
        
        // Create enemies and place them on the map
        for (int i = 0; i < Math.Min(enemyCount, openPositions.Count); i++)
        {
            var position = openPositions[i];
            var tile = dungeon.TileMap[position.x, position.y];
            
            // Generate enemy for this tile
            var enemy = GenerateEnemyForTile(tile, dungeon.Signature);
            
            // Set position
            enemy.X = position.x;
            enemy.Y = position.y;
            
            // Add to dungeon
            dungeon.Enemies.Add(enemy);
        }
    }
    
    private static Enemy GenerateEnemyForTile(Tile tile, float[] dungeonSignature)
    {
        // Get random enemy type
        var enemyTypes = EnemyTypes.Types.Values.ToList();
        var enemyType = enemyTypes[_random.Next(enemyTypes.Count)];
        
        // Create Signature objects
        Signature tileSignature;
        Signature dungeonSig;
        
        try
        {
            tileSignature = new Signature(tile.Signature);
            dungeonSig = new Signature(dungeonSignature);
        }
        catch (ArgumentException)
        {
            // Fallback to random signatures if invalid
            tileSignature = Signature.CreateRandom(_random);
            dungeonSig = Signature.CreateRandom(_random);
        }
            
        // Generate enemy signature similar to tile
        var enemySig = Signature.CreateSimilar(tileSignature, 0.3f, _random);
        float[] enemySignature = enemySig.GetValues();
            
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
        float similarityFactor = 1 - enemySig.CalculateDistanceFrom(dungeonSig) / 4;
            
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
        return SignatureHelper.CalculateDistance(sig1, sig2);
    }
}
