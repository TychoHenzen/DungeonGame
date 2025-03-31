#region

using System;
using System.IO;
using System.Text.Json;

#endregion

namespace DungeonGame.Const;

public static class Constants
{
    // Cached JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public static GameConstants Game { get; } = Load<GameConstants>("GameConstants.json");
    public static CombatConstants Combat { get; } = Load<CombatConstants>("CombatConstants.json");
    public static DungeonConstants Dungeon { get; } = Load<DungeonConstants>("DungeonConstants.json");
    public static UIConstants UI { get; } = Load<UIConstants>("UIConstants.json");


    /// <summary>
    ///     Loads constants from a file, or returns defaults if file doesn't exist
    /// </summary>
    private static T Load<T>(string filePath) where T : new()
    {
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading constants: {ex.Message}");
        }

        var returned = new T();
        Save<T>(returned, filePath);
        return returned;
    }

    /// <summary>
    ///     Saves constants to a file
    /// </summary>
    private static bool Save<T>(T self, string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(self, _jsonOptions);
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
