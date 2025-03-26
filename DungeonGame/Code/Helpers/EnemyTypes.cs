using System.Collections.Generic;

namespace DungeonGame;

/// <summary>
/// Enemy types
/// </summary>
public static class EnemyTypes
{
    public static readonly Dictionary<string, EnemyTypeInfo> Types = new()
    {
        { "Goblin", new EnemyTypeInfo { Name = "Goblin", BaseHealth = 20, BaseDamage = 5 } },
        { "Skeleton", new EnemyTypeInfo { Name = "Skeleton", BaseHealth = 15, BaseDamage = 7 } },
        { "Slime", new EnemyTypeInfo { Name = "Slime", BaseHealth = 30, BaseDamage = 3 } },
        { "Troll", new EnemyTypeInfo { Name = "Troll", BaseHealth = 50, BaseDamage = 8 } },
        { "Ghost", new EnemyTypeInfo { Name = "Ghost", BaseHealth = 25, BaseDamage = 6 } }
    };
        
    public class EnemyTypeInfo
    {
        public string Name { get; set; }
        public float BaseHealth { get; set; }
        public float BaseDamage { get; set; }
    }
}