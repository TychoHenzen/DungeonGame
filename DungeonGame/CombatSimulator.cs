using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Combat simulator that handles dungeon run simulations
/// </summary>
public class CombatSimulator
{
    private readonly Random _random = new();

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
    /// Simulates a complete dungeon run with the given player and dungeon
    /// </summary>
    /// <param name="player">The player participating in the dungeon run</param>
    /// <param name="dungeon">The dungeon to be explored</param>
    /// <param name="selectedItem">The item selected for the dungeon run</param>
    /// <returns>A DungeonResult containing the outcome and details of the run</returns>
    public DungeonResult SimulateDungeonRun(Player player, Dungeon dungeon, Item selectedItem = null)
    {
        // Combat log to track events
        var combatLog = new List<string>();

        // Calculate player stats and apply affinity bonus
        var playerStats = CalculatePlayerStatsWithAffinity(player, dungeon, selectedItem);
        float currentHealth = playerStats.MaxHealth;

        // Initialize combat tracking variables
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        int enemiesDefeated = 0;

        // Clone the enemy list for modification during combat
        var enemies = dungeon.Enemies.ToList();
        bool success = true;
        bool playerDefeated = false;

        // Start combat simulation
        combatLog.Add($"Entered dungeon with {enemies.Count} enemies!");
        combatLog.Add(
            $"Player stats - HP: {(int)playerStats.MaxHealth}, ATK: {(int)playerStats.Attack}, DEF: {(int)playerStats.Defense}, SPD: {(int)playerStats.Speed}");

        // Fight each enemy in sequence
        int enemyIndex = 0;
        while (enemyIndex < enemies.Count && !playerDefeated)
        {
            var enemy = enemies[enemyIndex];
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
                playerDefeated = true;
            }
            else
            {
                // Recovery between fights
                RecoverBetweenFights(ref currentHealth, playerStats, combatLog);
                enemyIndex++;
            }
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
    private static PlayerStats CalculatePlayerStatsWithAffinity(Player player, Dungeon dungeon, Item selectedItem = null)
    {
        var playerStats = player.CalculateStats();

        // Calculate signature affinity bonus
        var equippedItems = player.GetEquippedItems();
        
        // Add selected item to the calculation if provided
        if (selectedItem != null)
        {
            var tempItems = new Dictionary<string, Item>(equippedItems);
            tempItems["selected"] = selectedItem;
            equippedItems = tempItems;
        }
        
        float affinityBonus = CalculateAffinityBonus(equippedItems, dungeon.Signature);

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
            var roundResult = ProcessCombatRound(playerFirst, playerStats, enemy, ref enemyHealth, ref currentHealth,
                combatLog);
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
        bool combatContinues = true;

        // Player's first attack if going first
        if (playerFirst && combatContinues)
        {
            float damage = CalculatePlayerDamage(playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
            damageDealt += damage;

            // Check if enemy defeated
            combatContinues = enemyHealth > 0;
        }

        // Enemy attack if combat continues
        if (combatContinues)
        {
            float enemyDamage = CalculateEnemyDamage(enemy, playerStats);
            ApplyDamageToPlayer(ref currentHealth, enemyDamage, enemy.Name, combatLog);
            damageTaken += enemyDamage;

            // Check if player defeated
            combatContinues = currentHealth > 0;
        }

        // Player's second attack if going second and combat continues
        if (!playerFirst && combatContinues)
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
    private static void ApplyDamageToEnemy(ref float enemyHealth, float damage, string enemyName,
        ICollection<string> combatLog)
    {
        enemyHealth -= damage;
        combatLog.Add(
            $"- Player attacks for {(int)damage} damage! {enemyName} has {Math.Max(0, (int)enemyHealth)} HP left.");
    }

    /// <summary>
    /// Applies damage to player and logs result
    /// </summary>
    private static void ApplyDamageToPlayer(ref float playerHealth, float damage, string enemyName,
        ICollection<string> combatLog)
    {
        playerHealth -= damage;
        combatLog.Add(
            $"- {enemyName} attacks for {(int)damage} damage! Player has {Math.Max(0, (int)playerHealth)} HP left.");
    }

    /// <summary>
    /// Handles health recovery between fights
    /// </summary>
    private static void RecoverBetweenFights(ref float currentHealth, PlayerStats playerStats,
        ICollection<string> combatLog)
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
            data.CombatLog.Add(
                $"DUNGEON CLEARED! Defeated {data.EnemiesDefeated}/{data.Dungeon.Enemies.Count} enemies.");
            data.CombatLog.Add(
                $"Total damage dealt: {(int)data.TotalDamageDealt}, Total damage taken: {(int)data.TotalDamageTaken}");
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
                if (dungeon.Signature != null)
                {
                    loot.Add(ItemGenerator.GenerateItemWithSignature(dungeon.Signature));
                }
                else
                {
                    loot.Add(ItemGenerator.GenerateRandomItem());
                }
            }

            combatLog.Add($"Found {lootCount} items!");
        }

        return loot;
    }

    /// <summary>
    /// Calculates the affinity bonus based on how well the player's equipment matches the dungeon's signature
    /// </summary>
    private static float CalculateAffinityBonus(Dictionary<string, Item> equippedItems, float[] dungeonSignatureValues)
    {
        if (dungeonSignatureValues == null)
            return 0;
            
        var dungeonSignature = new Signature(dungeonSignatureValues);
        float affinitySum = 0;
        int itemCount = 0;

        foreach (var item in equippedItems.Values)
        {
            if (item != null && item.Signature != null)
            {
                try
                {
                    var itemSignature = new Signature(item.Signature);
                    float similarity = itemSignature.CalculateSimilarityWith(dungeonSignature);
                    affinitySum += similarity;
                    itemCount++;
                }
                catch (ArgumentException)
                {
                    // Skip items with invalid signatures
                    continue;
                }
            }
        }

        return itemCount > 0 ? affinitySum / itemCount : 0;
    }
}
