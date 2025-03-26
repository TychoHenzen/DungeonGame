using System;
using System.IO;
using System.Text.Json;

namespace DungeonGame.Code.Helpers;

/// <summary>
/// Contains game constants that can be serialized to disk
/// </summary>
public sealed class GameConstants
{
    // Cached JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    // Player constants
    public float BasePlayerHealth { get; init; } = 100f;
    public float BasePlayerAttack { get; init; } = 10f;
    public float BasePlayerDefense { get; init; } = 5f;
    public float BasePlayerSpeed { get; init; } = 10f;

    // Combat constants
    public float PlayerRecoveryPercent { get; init; } = 0.15f;
    public float AffinityAttackBonus { get; init; } = 0.5f;
    public float AffinityDefenseBonus { get; init; } = 0.3f;
    public float AffinitySpeedBonus { get; init; } = 0.2f;
    public float PlayerSpeedAdvantageFactor { get; init; } = 0.5f;
    public float EnemyDefenseFactor { get; init; } = 0.2f;
    public float PlayerDefenseFactor { get; init; } = 0.5f;
    public float LowHealthThreshold { get; init; } = 0.3f;
    public int MaxCombatRounds { get; init; } = 20;

    // Dungeon constants
    public int DefaultMapWidth { get; init; } = 20;
    public int DefaultMapHeight { get; init; } = 15;
    public int MaxExplorationSteps { get; init; } = 100;

    // Inventory constants
    public int DefaultInventoryCapacity { get; init; } = 20;
    public int DefaultDungeonSlots { get; init; } = 3;
    public int InitialUnlockedDungeonSlots { get; init; } = 1;

    // Graphics & UI constants
    public int DefaultScreenWidth { get; init; } = 1280;
    public int DefaultScreenHeight { get; init; } = 720;
    
    // Timing constants
    public float DungeonSimulationDelay { get; init; } = 3.0f;
    
    // Signature constants
    public float SignatureHighThreshold { get; init; } = 0.5f;
    public float SignatureLowThreshold { get; init; } = -0.5f;
    public float DefaultSignatureVariance { get; init; } = 0.2f;
    
    // Item generation constants
    public int MinLootCount { get; init; } = 1;
    public int MaxLootCount { get; init; } = 4;

    public static GameConstants Get { get; } = Load("GameConstants.json");

    /// <summary>
    /// Loads constants from a file, or returns defaults if file doesn't exist
    /// </summary>
    public static GameConstants Load(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<GameConstants>(json, _jsonOptions) ?? new GameConstants();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading constants: {ex.Message}");
        }

        var returned = new GameConstants();
        returned.Save(filePath);
        return returned;
    }

    /// <summary>
    /// Saves constants to a file
    /// </summary>
    public bool Save(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(this, _jsonOptions);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving constants: {ex.Message}");
            return false;
        }
    }
}