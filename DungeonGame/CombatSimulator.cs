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
        var playerStats = CalculatePlayerStatsWithAffinity(player, dungeon, combatLog);
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
        combatLog.Add($"Player stats - HP: {(int)playerStats.MaxHealth}, ATK: {(int)playerStats.Attack}, DEF: {(int)playerStats.Defense}, SPD: {(int)playerStats.Speed}");
        
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
        return CreateDungeonResult(
            player, 
            dungeon, 
            success, 
            currentHealth, 
            playerStats, 
            totalDamageDealt, 
            totalDamageTaken, 
            enemiesDefeated, 
            combatLog);
    }
    
    /// <summary>
    /// Calculates player stats with affinity bonus applied
    /// </summary>
    private PlayerStats CalculatePlayerStatsWithAffinity(Player player, Dungeon dungeon, List<string> combatLog)
    {
        var playerStats = player.CalculateStats();
        
        // Calculate signature affinity bonus
        float affinityBonus = CalculateAffinityBonus(player.GetEquippedItems(), dungeon.Signature);
        
        // Apply affinity bonus to stats
        playerStats.Attack *= (1 + affinityBonus * 0.3f);
        playerStats.Defense *= (1 + affinityBonus * 0.3f);
        playerStats.Speed *= (1 + affinityBonus * 0.2f);
        
        return playerStats;
    }
    
    /// <summary>
    /// Simulates combat between player and an enemy
    /// </summary>
    private (bool EnemyDefeated, float DamageDealt, float DamageTaken) SimulateCombat(
        Enemy enemy, PlayerStats playerStats, ref float currentHealth, List<string> combatLog)
    {
        // Log enemy encounter
        combatLog.Add($"Encountered {enemy.Name}! (HP: {(int)enemy.Health}, DMG: {(int)enemy.Damage})");
        
        float enemyHealth = enemy.Health;
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        int rounds = 0;
        
        // Combat rounds
        while (enemyHealth > 0 && currentHealth > 0)
        {
            rounds++;
            
            // Determine if player attacks first based on speed
            bool playerFirst = playerStats.Speed >= enemy.Damage * 0.5f;
            
            // Player's first attack if going first
            if (playerFirst)
            {
                float damage = CalculatePlayerDamage(playerStats, enemy);
                ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
                totalDamageDealt += damage;
                
                // Check if enemy defeated
                if (enemyHealth <= 0)
                {
                    combatLog.Add($"- {enemy.Name} was defeated!");
                    return (true, totalDamageDealt, totalDamageTaken);
                }
            }
            
            // Enemy attack
            float enemyDamage = CalculateEnemyDamage(enemy, playerStats);
            ApplyDamageToPlayer(ref currentHealth, enemyDamage, enemy.Name, combatLog);
            totalDamageTaken += enemyDamage;
            
            // Check if player defeated
            if (currentHealth <= 0)
            {
                return (false, totalDamageDealt, totalDamageTaken);
            }
            
            // Player's second attack if going second
            if (!playerFirst)
            {
                float damage = CalculatePlayerDamage(playerStats, enemy);
                ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name, combatLog);
                totalDamageDealt += damage;
                
                // Check if enemy defeated
                if (enemyHealth <= 0)
                {
                    combatLog.Add($"- {enemy.Name} was defeated!");
                    return (true, totalDamageDealt, totalDamageTaken);
                }
            }
            
            // Prevent infinite loops
            if (rounds > 20)
            {
                combatLog.Add("- Combat taking too long, moving on...");
                break;
            }
        }
        
        return (enemyHealth <= 0, totalDamageDealt, totalDamageTaken);
    }
    
    /// <summary>
    /// Calculates damage dealt by player with randomness
    /// </summary>
    private float CalculatePlayerDamage(PlayerStats playerStats, Enemy enemy)
    {
        // Calculate player damage with variance (80-120% damage)
        float damageVariance = 0.8f + (float)_random.NextDouble() * 0.4f;
        float baseDamage = Math.Max(1, playerStats.Attack - enemy.Damage * 0.2f);
        return (float)Math.Round(baseDamage * damageVariance);
    }
    
    /// <summary>
    /// Calculates damage dealt by enemy with randomness
    /// </summary>
    private float CalculateEnemyDamage(Enemy enemy, PlayerStats playerStats)
    {
        // Enemy damage with variance (90-110% damage)
        float enemyDamageVariance = 0.9f + (float)_random.NextDouble() * 0.2f;
        float enemyBaseDamage = Math.Max(1, enemy.Damage - playerStats.Defense * 0.5f);
        return (float)Math.Round(enemyBaseDamage * enemyDamageVariance);
    }
    
    /// <summary>
    /// Applies damage to enemy and logs result
    /// </summary>
    private void ApplyDamageToEnemy(ref float enemyHealth, float damage, string enemyName, List<string> combatLog)
    {
        enemyHealth -= damage;
        combatLog.Add($"- Player attacks for {(int)damage} damage! {enemyName} has {Math.Max(0, (int)enemyHealth)} HP left.");
    }
    
    /// <summary>
    /// Applies damage to player and logs result
    /// </summary>
    private void ApplyDamageToPlayer(ref float playerHealth, float damage, string enemyName, List<string> combatLog)
    {
        playerHealth -= damage;
        combatLog.Add($"- {enemyName} attacks for {(int)damage} damage! Player has {Math.Max(0, (int)playerHealth)} HP left.");
    }
    
    /// <summary>
    /// Handles health recovery between fights
    /// </summary>
    private void RecoverBetweenFights(ref float currentHealth, PlayerStats playerStats, List<string> combatLog)
    {
        float recovery = playerStats.MaxHealth * 0.15f;
        currentHealth = Math.Min(playerStats.MaxHealth, currentHealth + recovery);
        combatLog.Add($"Recovered {(int)recovery} HP. Current HP: {(int)currentHealth}");
    }
    
    /// <summary>
    /// Creates the final dungeon result
    /// </summary>
    private DungeonResult CreateDungeonResult(
        Player player,
        Dungeon dungeon,
        bool success,
        float currentHealth,
        PlayerStats playerStats,
        float totalDamageDealt,
        float totalDamageTaken,
        int enemiesDefeated,
        List<string> combatLog)
    {
        // End of dungeon summary
        if (success)
        {
            combatLog.Add($"DUNGEON CLEARED! Defeated {enemiesDefeated}/{dungeon.Enemies.Count} enemies.");
            combatLog.Add($"Total damage dealt: {(int)totalDamageDealt}, Total damage taken: {(int)totalDamageTaken}");
        }
        
        // Define casualties based on remaining health
        bool casualties = success && (currentHealth / playerStats.MaxHealth < 0.3f);
        if (casualties)
        {
            combatLog.Add("Barely survived with heavy injuries!");
        }
        
        // Generate loot based on success and dungeon signature
        var loot = GenerateLoot(success, casualties, dungeon, combatLog);
        
        // Create and return dungeon result
        return new DungeonResult
        {
            Success = success,
            Casualties = casualties,
            Duration = dungeon.Length,
            Loot = loot,
            CombatLog = combatLog,
            PlayerStats = new PlayerStats
            {
                Attack = (int)playerStats.Attack,
                Defense = (int)playerStats.Defense,
                Speed = (int)playerStats.Speed,
                MaxHealth = (int)playerStats.MaxHealth,
                RemainingHealth = (int)currentHealth
            },
            Stats = new CombatStats
            {
                EnemiesDefeated = enemiesDefeated,
                TotalDamageDealt = (int)totalDamageDealt,
                TotalDamageTaken = (int)totalDamageTaken
            }
        };
    }
    
    /// <summary>
    /// Generates loot for the dungeon run
    /// </summary>
    private List<Item> GenerateLoot(bool success, bool casualties, Dungeon dungeon, List<string> combatLog)
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