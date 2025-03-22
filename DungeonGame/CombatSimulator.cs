using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Combat simulator that handles dungeon run simulations
/// </summary>
public class CombatSimulator
{
    private readonly Random _random = new Random();
    
    // Constants to replace magic numbers
    private const int MAX_COMBAT_ROUNDS = 20;
    private const float PLAYER_RECOVERY_PERCENT = 0.15f;
    private const float AFFINITY_ATTACK_BONUS = 0.3f;
    private const float AFFINITY_DEFENSE_BONUS = 0.3f;
    private const float AFFINITY_SPEED_BONUS = 0.2f;
    private const float PLAYER_SPEED_ADVANTAGE_FACTOR = 0.5f;
    private const float ENEMY_DEFENSE_FACTOR = 0.2f;
    private const float PLAYER_DEFENSE_FACTOR = 0.5f;
    private const float LOW_HEALTH_THRESHOLD = 0.3f;

    /// <summary>
    /// Helper class to reduce parameter count in methods
    /// </summary>
    private class DungeonRunData
    {
        public Dungeon Dungeon { get; set; }
        public bool Success { get; set; }
        public float CurrentHealth { get; set; }
        public PlayerStats PlayerStats { get; set; }
        public float TotalDamageDealt { get; set; }
        public float TotalDamageTaken { get; set; }
        public int EnemiesDefeated { get; set; }
        public ICollection<string> CombatLog { get; set; }
    }
    
    /// <summary>
    /// Simulates a complete dungeon run with the given player and dungeon
    /// </summary>
    /// <param name="player">The player participating in the dungeon run</param>
    /// <param name="dungeon">The dungeon to be explored</param>
    /// <returns>A DungeonResult containing the outcome and details of the run</returns>
    public DungeonResult SimulateDungeonRun(Player player, Dungeon dungeon)
    {
        // Combat log to track events
        var combatLog = new List<string>();

        // Calculate player stats and apply affinity bonus
        var playerStats = CalculatePlayerStatsWithAffinity(player, dungeon);
        float currentHealth = playerStats.MaxHealth;

        // Initialize combat tracking variables
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        int enemiesDefeated = 0;

        // Clone the enemy list for modification during combat
        var enemies = dungeon.Enemies.ToList();
        bool success = true;

        // Start combat simulation
        combatLog.Add($"Entered dungeon with {enemies.Count} enemies!");
        combatLog.Add(
            $"Player stats - HP: {(int)playerStats.MaxHealth}, ATK: {(int)playerStats.Attack}, DEF: {(int)playerStats.Defense}, SPD: {(int)playerStats.Speed}");

        // Fight each enemy in sequence
        foreach (var enemy in enemies)
        {
            var combatResult = SimulateCombat(enemy, playerStats, ref currentHealth, combatLog);

            // Update combat stats
            totalDamageDealt += combatResult.DamageDealt;
            totalDamageTaken += combatResult.DamageTaken;

            if (combatResult.EnemyDefeated)
            {
                enemiesDefeated++;
            }

            // Check if player was defeated
            if (currentHealth <= 0)
            {
                combatLog.Add("PLAYER DEFEATED! Dungeon run failed!");
                success = false;
                break;
            }

            // Recovery between fights
            RecoverBetweenFights(ref currentHealth, playerStats, combatLog);
        }

        // Generate summary and result
        var runData = new DungeonRunData
        {
            Dungeon = dungeon,
            Success = success,
            CurrentHealth = currentHealth,
            PlayerStats = playerStats,
            TotalDamageDealt = totalDamageDealt,
            TotalDamageTaken = totalDamageTaken,
            EnemiesDefeated = enemiesDefeated,
            CombatLog = combatLog
        };
        return CreateDungeonResult(runData);
    }

    /// <summary>
    /// Calculates player stats with affinity bonus applied
    /// </summary>
    private PlayerStats CalculatePlayerStatsWithAffinity(Player player, Dungeon dungeon)
    {
        var playerStats = player.CalculateStats();

        // Calculate signature affinity bonus
        float affinityBonus = CalculateAffinityBonus(player.GetEquippedItems(), dungeon.Signature);

        // Apply affinity bonus to stats
        playerStats.Attack *= (1 + affinityBonus * AFFINITY_ATTACK_BONUS);
        playerStats.Defense *= (1 + affinityBonus * AFFINITY_DEFENSE_BONUS);
        playerStats.Speed *= (1 + affinityBonus * AFFINITY_SPEED_BONUS);

        return playerStats;
    }

    /// <summary>
    /// Simulates combat between player and an enemy
    /// </summary>
    private (bool EnemyDefeated, float DamageDealt, float DamageTaken) SimulateCombat(
        Enemy enemy, PlayerStats playerStats, ref float currentHealth, ICollection<string> combatLog)
    {
        // Log enemy encounter
        combatLog.Add($"Encountered {enemy.Name}! (HP: {(int)enemy.Health}, DMG: {(int)enemy.Damage})");

        float enemyHealth = enemy.Health;
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        int rounds = 0;

        // Combat rounds
        bool combatEnded = false;
        while (!combatEnded && rounds < MAX_COMBAT_ROUNDS)
        {
            rounds++;

            // Determine if player attacks first based on speed
            bool playerFirst = playerStats.Speed >= enemy.Damage * PLAYER_SPEED_ADVANTAGE_FACTOR;

            // Process combat round
            var roundResult = ProcessCombatRound(playerFirst, playerStats, enemy, ref enemyHealth, ref currentHealth, combatLog);
            totalDamageDealt += roundResult.damageDealt;
            totalDamageTaken += roundResult.damageTaken;
            
            // Check if combat ended
            combatEnded = enemyHealth <= 0 || currentHealth <= 0;
        }

        // Handle timeout case
        if (rounds >= MAX_COMBAT_ROUNDS && enemyHealth > 0)
        {
            combatLog.Add("- Combat taking too long, moving on...");
        }
        
        // Report enemy defeat if applicable
        if (enemyHealth <= 0)
        {
            combatLog.Add($"- {enemy.Name} was defeated!");
        }

        return (enemyHealth <= 0, totalDamageDealt, totalDamageTaken);
    }

    /// <summary>
    /// Processes a single round of combat
    /// </summary>
    private (float damageDealt, float damageTaken) ProcessCombatRound(
        bool playerFirst, PlayerStats playerStats, Enemy enemy, 
        ref float enemyHealth, ref float currentHealth, ICollection<string> combatLog)
    {
        float damageDealt = 0;
        float damageTaken = 0;

        // Player's first attack if going first
        if (playerFirst)
        {
            float damage = CalculatePlayerDamage(playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
            damageDealt += damage;

            // Check if enemy defeated
            if (enemyHealth <= 0)
            {
                return (damageDealt, damageTaken);
            }
        }

        // Enemy attack
        float enemyDamage = CalculateEnemyDamage(enemy, playerStats);
        ApplyDamageToPlayer(ref currentHealth, enemyDamage, enemy.Name, combatLog);
        damageTaken += enemyDamage;

        // Check if player defeated
        if (currentHealth <= 0)
        {
            return (damageDealt, damageTaken);
        }

        // Player's second attack if going second
        if (!playerFirst)
        {
            float damage = CalculatePlayerDamage(playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
            damageDealt += damage;
        }

        return (damageDealt, damageTaken);
    }

    /// <summary>
    /// Calculates damage dealt by player with randomness
    /// </summary>
    private float CalculatePlayerDamage(PlayerStats playerStats, Enemy enemy)
    {
        // Calculate player damage with variance (80-120% damage)
        float damageVariance = 0.8f + (float)_random.NextDouble() * 0.4f;
        float baseDamage = Math.Max(1, playerStats.Attack - enemy.Damage * ENEMY_DEFENSE_FACTOR);
        return (float)Math.Round(baseDamage * damageVariance);
    }

    /// <summary>
    /// Calculates damage dealt by enemy with randomness
    /// </summary>
    private float CalculateEnemyDamage(Enemy enemy, PlayerStats playerStats)
    {
        // Enemy damage with variance (90-110% damage)
        float enemyDamageVariance = 0.9f + (float)_random.NextDouble() * 0.2f;
        float enemyBaseDamage = Math.Max(1, enemy.Damage - playerStats.Defense * PLAYER_DEFENSE_FACTOR);
        return (float)Math.Round(enemyBaseDamage * enemyDamageVariance);
    }

    /// <summary>
    /// Applies damage to enemy and logs result
    /// </summary>
    private static void ApplyDamageToEnemy(ref float enemyHealth, float damage, string enemyName, ICollection<string> combatLog)
    {
        enemyHealth -= damage;
        combatLog.Add(
            $"- Player attacks for {(int)damage} damage! {enemyName} has {Math.Max(0, (int)enemyHealth)} HP left.");
    }

    /// <summary>
    /// Applies damage to player and logs result
    /// </summary>
    private static void ApplyDamageToPlayer(ref float playerHealth, float damage, string enemyName, ICollection<string> combatLog)
    {
        playerHealth -= damage;
        combatLog.Add(
            $"- {enemyName} attacks for {(int)damage} damage! Player has {Math.Max(0, (int)playerHealth)} HP left.");
    }

    /// <summary>
    /// Handles health recovery between fights
    /// </summary>
    private void RecoverBetweenFights(ref float currentHealth, PlayerStats playerStats, ICollection<string> combatLog)
    {
        float recovery = playerStats.MaxHealth * PLAYER_RECOVERY_PERCENT;
        currentHealth = Math.Min(playerStats.MaxHealth, currentHealth + recovery);
        combatLog.Add($"Recovered {(int)recovery} HP. Current HP: {(int)currentHealth}");
    }

    /// <summary>
    /// Creates the final dungeon result
    /// </summary>
    private DungeonResult CreateDungeonResult(DungeonRunData data)
    {
        // End of dungeon summary
        if (data.Success)
        {
            data.CombatLog.Add($"DUNGEON CLEARED! Defeated {data.EnemiesDefeated}/{data.Dungeon.Enemies.Count} enemies.");
            data.CombatLog.Add($"Total damage dealt: {(int)data.TotalDamageDealt}, Total damage taken: {(int)data.TotalDamageTaken}");
        }

        // Define casualties based on remaining health
        bool casualties = data.Success && (data.CurrentHealth / data.PlayerStats.MaxHealth < LOW_HEALTH_THRESHOLD);
        if (casualties)
        {
            data.CombatLog.Add("Barely survived with heavy injuries!");
        }

        // Generate loot based on success and dungeon signature
        var loot = GenerateLoot(data.Success, casualties, data.Dungeon, data.CombatLog);

        // Create and return dungeon result
        return new DungeonResult
        {
            Success = data.Success,
            Casualties = casualties,
            Duration = data.Dungeon.Length,
            Loot = loot,
            CombatLog = data.CombatLog,
            PlayerStats = new PlayerStats
            {
                Attack = (int)data.PlayerStats.Attack,
                Defense = (int)data.PlayerStats.Defense,
                Speed = (int)data.PlayerStats.Speed,
                MaxHealth = (int)data.PlayerStats.MaxHealth,
                RemainingHealth = (int)data.CurrentHealth
            },
            Stats = new CombatStats
            {
                EnemiesDefeated = data.EnemiesDefeated,
                TotalDamageDealt = (int)data.TotalDamageDealt,
                TotalDamageTaken = (int)data.TotalDamageTaken
            }
        };
    }

    /// <summary>
    /// Generates loot for the dungeon run
    /// </summary>
    private List<Item> GenerateLoot(bool success, bool casualties, Dungeon dungeon, ICollection<string> combatLog)
    {
        var loot = new List<Item>();
        if (success)
        {
            int lootCount = _random.Next(1, 4) + (casualties ? 0 : 2);
            for (int i = 0; i < lootCount; i++)
            {
                loot.Add(ItemGenerator.GenerateItemWithSignature(dungeon.Signature));
            }

            combatLog.Add($"Found {lootCount} items!");
        }

        return loot;
    }

    /// <summary>
    /// Calculates the affinity bonus based on how well the player's equipment matches the dungeon's signature
    /// </summary>
    private float CalculateAffinityBonus(Dictionary<string, Item> equippedItems, float[] dungeonSignature)
    {
        float affinitySum = 0;
        int itemCount = 0;

        foreach (var item in equippedItems.Values)
        {
            if (item != null)
            {
                float similarity = 1 - SignatureDistance(item.Signature, dungeonSignature) / 4f;
                affinitySum += similarity;
                itemCount++;
            }
        }

        return itemCount > 0 ? affinitySum / itemCount : 0;
    }

    /// <summary>
    /// Calculates Euclidean distance between two signatures
    /// </summary>
    private float SignatureDistance(float[] sig1, float[] sig2)
    {
        float sumSquaredDiffs = 0;

        for (int i = 0; i < sig1.Length; i++)
        {
            sumSquaredDiffs += (sig1[i] - sig2[i]) * (sig1[i] - sig2[i]);
        }

        return (float)Math.Sqrt(sumSquaredDiffs);
    }
}
