#region

using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Helpers;
using DungeonGame.Const;

#endregion

namespace DungeonGame.Code.Systems;

/// <summary>
///     Enhanced dungeon generator that creates a proper tile map
/// </summary>
public static class DungeonGenerator
{
    public static Dungeon GenerateDungeon(Signature itemSignature)
    {
        // Generate dungeon signature similar to item
        var dungeonSignature = Signature.CreateSimilar(itemSignature, 0.2f);

        var dungeon = new Dungeon
        {
            Signature = dungeonSignature,
            Difficulty = Random.Shared.Next(1, 4), // 1-3
            Length = Random.Shared.Next(3, 6), // 3-5 minutes (we'll keep this for progress tracking)
            Width = Constants.Dungeon.DefaultMapWidth,
            Height = Constants.Dungeon.DefaultMapHeight
        };

        // Generate tile map using Perlin-like noise based on signature
        GenerateTileMap(dungeon, dungeonSignature);

        // Populate dungeon with enemies
        PopulateEnemies(dungeon);

        return dungeon;
    }

    private static void GenerateTileMap(Dungeon dungeon, Signature signature)
    {
        dungeon.TileMap = new Tile[dungeon.Width, dungeon.Height];

        // Use temperature and wetness as primary factors for tile distribution
        var temperature = signature[0]; // First dimension
        var wetness = signature[2]; // Third dimension

        // Create basic noise for map generation
        var noiseMap = GenerateSimpleNoise(dungeon.Width, dungeon.Height);

        // Fill the map with tiles
        for (var x = 0; x < dungeon.Width; x++)
        {
            for (var y = 0; y < dungeon.Height; y++)
            {
                // Combine noise with signature to determine tile type
                var noiseValue = noiseMap[x, y];
                var adjustedTemp = temperature + noiseValue * 0.5f;
                var adjustedWet = wetness + noiseValue * 0.3f;

                // Generate tile signature from the base signature with noise influence
                var tileSignature = GenerateTileSignature(signature, noiseValue);

                // Determine tile type based on adjusted values
                var tileType = GetTileTypeForValues(adjustedTemp, adjustedWet, noiseValue);

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
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                // Generate a smooth noise value between -1 and 1
                noise[x, y] = (float)(Random.Shared.NextDouble() * 2 - 1);
            }
        }

        // Smooth the noise (simple box blur)
        var smoothedNoise = new float[width, height];
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                float sum = 0;
                var count = 0;

                // Average of neighbors
                for (var nx = Math.Max(0, x - 1); nx <= Math.Min(width - 1, x + 1); nx++)
                {
                    for (var ny = Math.Max(0, y - 1); ny <= Math.Min(height - 1, y + 1); ny++)
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

    private static Signature GenerateTileSignature(Signature baseSignature, float noiseValue)
    {
        var tileSignature = new float[Signature.Dimensions];

        for (var i = 0; i < Signature.Dimensions; i++)
        {
            // Vary the signature based on noise
            tileSignature[i] = Math.Clamp(baseSignature[i] + noiseValue * 0.4f, -1f, 1f);
        }

        return new Signature(tileSignature);
    }

    private static string GetTileTypeForValues(float temperature, float wetness, float noise)
    {
        // Determine tile type based on temperature and wetness with some randomness
        if (temperature > 0.5f && wetness < -0.3f)
        {
            return "Lava";
        }

        if (temperature < -0.5f && wetness > 0.3f)
        {
            return "Ice";
        }

        if (wetness > 0.5f)
        {
            return "Water";
        }

        if (temperature > 0 && wetness > 0)
        {
            return "Grass";
        }

        if (temperature > 0.3f && wetness < 0)
        {
            return "Sand";
        }

        if (noise > 0.5f) // Use noise for crystal distribution
        {
            return "Crystal";
        }

        if (noise < -0.5f) // Use noise for wood distribution
        {
            return "Wood";
        }

        return "Stone";
    }

    private static void EnsureTraversableMap(Dungeon dungeon)
    {
        // Create a simple path from top to bottom to ensure dungeon is traversable
        var pathX = dungeon.Width / 2;

        for (var y = 0; y < dungeon.Height; y++)
        {
            // Make this path passable
            dungeon.TileMap[pathX, y].IsPassable = true;

            // Set simple stone path
            dungeon.TileMap[pathX, y].Type = "Stone";

            // Randomly adjust path to make it more natural
            if (Random.Shared.Next(100) < 40 && y < dungeon.Height - 1)
            {
                pathX = Math.Clamp(pathX + Random.Shared.Next(-1, 2), 1, dungeon.Width - 2);
            }
        }
    }

    private static void PopulateEnemies(Dungeon dungeon)
    {
        // Clear existing enemies (in case this is a regeneration)
        dungeon.Enemies.Clear();

        // Determine number of enemies based on difficulty
        var enemyCount = 3 + dungeon.Difficulty * 2;

        // Create a list of open positions where enemies can spawn
        var openPositions = new List<(int x, int y)>();

        for (var x = 0; x < dungeon.Width; x++)
        {
            for (var y = 0; y < dungeon.Height; y++)
            {
                if (dungeon.TileMap[x, y].IsPassable)
                {
                    openPositions.Add((x, y));
                }
            }
        }

        // Shuffle the positions
        openPositions = openPositions.OrderBy(_ => Random.Shared.Next()).ToList();

        // Create enemies and place them on the map
        for (var i = 0; i < Math.Min(enemyCount, openPositions.Count); i++)
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

    private static Enemy GenerateEnemyForTile(Tile tile, Signature dungeonSignature)
    {
        // Get random enemy type
        var enemyTypes = EnemyTypes.Types.Values.ToList();
        var enemyType = enemyTypes[Random.Shared.Next(enemyTypes.Count)];

        // Create Signature objects
        Signature tileSignature;
        Signature dungeonSig;

        try
        {
            tileSignature = tile.Signature;
            dungeonSig = dungeonSignature;
        }
        catch (ArgumentException)
        {
            // Fallback to random signatures if invalid
            tileSignature = Signature.CreateRandom();
            dungeonSig = Signature.CreateRandom();
        }

        // Generate enemy signature similar to tile
        var enemySig = Signature.CreateSimilar(tileSignature, 0.3f);


        // Generate adjective for enemy name
        var adjective = string.Empty;

        for (var i = 0; i < Signature.Dimensions; i++)
        {
            if (enemySig[i] > 0.5f)
            {
                adjective = SignatureDimensions.HighDescriptors[i];
                break;
            }

            if (enemySig[i] < -0.5f)
            {
                adjective = SignatureDimensions.LowDescriptors[i];
                break;
            }
        }

        var enemyName = string.IsNullOrEmpty(adjective) ? enemyType.Name : $"{adjective} {enemyType.Name}";

        // Scale enemy stats based on signature similarity to dungeon
        var similarityFactor = 1 - enemySig.CalculateDistanceFrom(dungeonSig) / 4;

        return new Enemy
        {
            Name = enemyName,
            Type = enemyType.Name,
            Health = enemyType.BaseHealth * (0.8f + similarityFactor * 0.4f),
            Damage = enemyType.BaseDamage * (0.8f + similarityFactor * 0.4f),
            Signature = enemySig
        };
    }
}