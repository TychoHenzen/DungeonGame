#region

using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Models;
using DungeonGame.Const;

#endregion

namespace DungeonGame.Code.Systems;

#region

using Turn = (float damageDealt, float damageTaken);

#endregion

/// <summary>
///     Combat simulator that handles dungeon run simulations
/// </summary>
public class CombatSimulator
{
    /// <summary>
    ///     Simulates a complete dungeon run with the given player and dungeon
    /// </summary>
    /// <param name="player">The player participating in the dungeon run</param>
    /// <param name="dungeon">The dungeon to be explored</param>
    /// <param name="selectedItem">The item selected for the dungeon run</param>
    /// <returns>A DungeonResult containing the outcome and details of the run</returns>
    public DungeonResult SimulateDungeonRun(Player player, Dungeon dungeon)
    {
        return SimulateDungeonRun(player, dungeon, null);
    }

    /// <summary>
    ///     Simulates a complete dungeon run with the given player and dungeon
    /// </summary>
    /// <param name="player">The player participating in the dungeon run</param>
    /// <param name="dungeon">The dungeon to be explored</param>
    /// <param name="selectedItem">The item selected for the dungeon run</param>
    /// <returns>A DungeonResult containing the outcome and details of the run</returns>
    public DungeonResult SimulateDungeonRun(Player player, Dungeon dungeon, Item? selectedItem)
    {
        // Combat log to track events
        var combatLog = new List<string>();

        // Calculate player stats and apply affinity bonus
        var playerStats = CalculatePlayerStatsWithAffinity(player, dungeon, selectedItem);
        var currentHealth = playerStats.MaxHealth;

        // Initialize combat tracking variables
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        var enemiesDefeated = 0;

        // Clone the enemy list for modification during combat
        var enemies = dungeon.Enemies.ToList();
        var success = true;
        var playerDefeated = false;

        // Start combat simulation
        combatLog.Add($"Entered dungeon with {enemies.Count} enemies!");
        combatLog.Add(
            $"Player stats - HP: {(int)playerStats.MaxHealth}, ATK: {(int)playerStats.Attack}, DEF: {(int)playerStats.Defense}, SPD: {(int)playerStats.Speed}");

        // Fight each enemy in sequence
        var enemyIndex = 0;
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
    ///     Calculates player stats with affinity bonus applied
    /// </summary>
    private static PlayerStats CalculatePlayerStatsWithAffinity(Player player, Dungeon dungeon,
        Item? selectedItem = null)
    {
        var playerStats = player.CalculateStats();

        // Calculate signature affinity bonus
        var equippedItems = player.GetEquippedItems();

        // Add selected item to the calculation if provided
        if (selectedItem != null)
        {
            var tempItems = new Dictionary<SlotType, Item?>(equippedItems)
            {
                [SlotType.Selected] = selectedItem
            };

            equippedItems = tempItems;
        }

        var affinityBonus = CalculateAffinityBonus(equippedItems, dungeon.Signature);

        // Apply affinity bonus to stats
        playerStats.Attack *= 1 + affinityBonus * Constants.Combat.AffinityAttackBonus;
        playerStats.Defense *= 1 + affinityBonus * Constants.Combat.AffinityDefenseBonus;
        playerStats.Speed *= 1 + affinityBonus * Constants.Combat.AffinitySpeedBonus;

        return playerStats;
    }

    /// <summary>
    ///     Simulates combat between player and an enemy
    /// </summary>
    private (bool EnemyDefeated, float DamageDealt, float DamageTaken) SimulateCombat(
        Enemy enemy, PlayerStats playerStats, ref float currentHealth, ICollection<string> combatLog)
    {
        // Log enemy encounter
        combatLog.Add($"Encountered {enemy.Name}! (HP: {(int)enemy.Health}, DMG: {(int)enemy.Damage})");

        var enemyHealth = enemy.Health;
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        var rounds = 0;

        // Combat rounds
        var combatEnded = false;
        while (!combatEnded && rounds < Constants.Combat.MaxCombatRounds)
        {
            rounds++;

            // Determine if player attacks first based on speed
            var playerFirst = playerStats.Speed >= enemy.Damage * Constants.Combat.PlayerSpeedAdvantageFactor;

            // Process combat round
            var roundResult = ProcessCombatRound(playerFirst, playerStats, enemy, ref enemyHealth, ref currentHealth,
                combatLog);

            totalDamageDealt += roundResult.damageDealt;
            totalDamageTaken += roundResult.damageTaken;

            // Check if combat ended
            combatEnded = enemyHealth <= 0 || currentHealth <= 0;
        }

        // Handle timeout case
        if (rounds >= Constants.Combat.MaxCombatRounds && enemyHealth > 0)
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
    ///     Processes a single round of combat
    /// </summary>
    private static Turn ProcessCombatRound(
        bool playerFirst, PlayerStats playerStats, Enemy enemy,
        ref float enemyHealth, ref float currentHealth, ICollection<string> combatLog)
    {
        float damageDealt = 0;
        float damageTaken = 0;
        var combatContinues = true;

        // Player's first attack if going first
        if (playerFirst && combatContinues)
        {
            var damage = CalculatePlayerDamage(playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
            damageDealt += damage;

            // Check if enemy defeated
            combatContinues = enemyHealth > 0;
        }

        // Enemy attack if combat continues
        if (combatContinues)
        {
            var enemyDamage = CalculateEnemyDamage(enemy, playerStats);
            ApplyDamageToPlayer(ref currentHealth, enemyDamage, enemy.Name, combatLog);
            damageTaken += enemyDamage;

            // Check if player defeated
            combatContinues = currentHealth > 0;
        }

        // Player's second attack if going second and combat continues
        if (!playerFirst && combatContinues)
        {
            var damage = CalculatePlayerDamage(playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
            damageDealt += damage;
        }

        return (damageDealt, damageTaken);
    }

    /// <summary>
    ///     Calculates damage dealt by player with randomness
    /// </summary>
    private static float CalculatePlayerDamage(PlayerStats playerStats, Enemy enemy)
    {
        // Calculate player damage with variance (80-120% damage)
        var damageVariance = 0.8f + (float)Random.Shared.NextDouble() * 0.4f;
        var baseDamage = Math.Max(1, playerStats.Attack - enemy.Damage * Constants.Combat.EnemyDefenseFactor);
        return (float)Math.Round(baseDamage * damageVariance);
    }

    /// <summary>
    ///     Calculates damage dealt by enemy with randomness
    /// </summary>
    private static float CalculateEnemyDamage(Enemy enemy, PlayerStats playerStats)
    {
        // Enemy damage with variance (90-110% damage)
        var enemyDamageVariance = 0.9f + (float)Random.Shared.NextDouble() * 0.2f;
        var enemyBaseDamage = Math.Max(1, enemy.Damage - playerStats.Defense * Constants.Combat.PlayerDefenseFactor);
        return (float)Math.Round(enemyBaseDamage * enemyDamageVariance);
    }

    /// <summary>
    ///     Applies damage to enemy and logs result
    /// </summary>
    private static void ApplyDamageToEnemy(ref float enemyHealth, float damage, string enemyName,
        ICollection<string> combatLog)
    {
        enemyHealth -= damage;
        combatLog.Add(
            $"- Player attacks for {(int)damage} damage! {enemyName} has {Math.Max(0, (int)enemyHealth)} HP left.");
    }

    /// <summary>
    ///     Applies damage to player and logs result
    /// </summary>
    private static void ApplyDamageToPlayer(ref float playerHealth, float damage, string enemyName,
        ICollection<string> combatLog)
    {
        playerHealth -= damage;
        combatLog.Add(
            $"- {enemyName} attacks for {(int)damage} damage! Player has {Math.Max(0, (int)playerHealth)} HP left.");
    }

    /// <summary>
    ///     Handles health recovery between fights
    /// </summary>
    private static void RecoverBetweenFights(ref float currentHealth, PlayerStats playerStats,
        ICollection<string> combatLog)
    {
        var recovery = playerStats.MaxHealth * Constants.Combat.PlayerRecoveryPercent;
        currentHealth = Math.Min(playerStats.MaxHealth, currentHealth + recovery);
        combatLog.Add($"Recovered {(int)recovery} HP. Current HP: {(int)currentHealth}");
    }

    /// <summary>
    ///     Creates the final dungeon result
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
        var casualties = data.Success &&
                         data.CurrentHealth / data.PlayerStats.MaxHealth < Constants.Combat.LowHealthThreshold;

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
    ///     Generates loot for the dungeon run
    /// </summary>
    private static List<Item> GenerateLoot(bool success, bool casualties, Dungeon dungeon,
        ICollection<string> combatLog)
    {
        var loot = new List<Item>();
        if (!success)
        {
            return loot;
        }

        var lootCount = Random.Shared.Next(Constants.Combat.MinLootCount,
            Constants.Combat.MaxLootCount) + (casualties ? 0 : 2);

        for (var i = 0; i < lootCount; i++)
        {
            loot.Add(ItemGenerator.GenerateItemWithSignature(dungeon.Signature));
        }

        combatLog.Add($"Found {lootCount} items!");

        return loot;
    }

    /// <summary>
    ///     Calculates the affinity bonus based on how well the player's equipment matches the dungeon's signature
    /// </summary>
    private static float CalculateAffinityBonus(Dictionary<SlotType, Item?> equippedItems, Signature dungeonSignature)
    {
        float affinitySum = 0;
        var itemCount = 0;

        foreach (var item in equippedItems.Values
                     .Where(item => item is not null))
        {
            var similarity = item.Signature.CalculateSimilarityWith(dungeonSignature);
            // Use a weight factor for the similarity
            affinitySum += similarity * Constants.Combat.AffinityScaleFactor;
            itemCount++;
        }

        return itemCount > 0
            ? affinitySum / itemCount
            : 0;
    }
}
