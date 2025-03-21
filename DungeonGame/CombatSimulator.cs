using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Combat simulator
/// </summary>
public class CombatSimulator
{
    private Random _random = new Random();
        
    public DungeonResult SimulateDungeonRun(Player player, Dungeon dungeon)
    {
        // Calculate player stats
        var playerStats = player.CalculateStats();
        float currentHealth = playerStats.MaxHealth;
            
        // Calculate signature affinity bonus
        float affinityBonus = CalculateAffinityBonus(player.GetEquippedItems(), dungeon.Signature);
            
        // Apply affinity bonus to stats
        playerStats.Attack *= (1 + affinityBonus * 0.3f);
        playerStats.Defense *= (1 + affinityBonus * 0.3f);
        playerStats.Speed *= (1 + affinityBonus * 0.2f);
            
        // Combat log
        var combatLog = new List<string>();
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        int enemiesDefeated = 0;
            
        // Clone the enemy list for modification during combat
        var enemies = dungeon.Enemies.ToList();
        bool success = true;
            
        // Combat simulation
        combatLog.Add($"Entered dungeon with {enemies.Count} enemies!");
        combatLog.Add($"Player stats - HP: {(int)playerStats.MaxHealth}, ATK: {(int)playerStats.Attack}, DEF: {(int)playerStats.Defense}, SPD: {(int)playerStats.Speed}");
            
        // Fight each enemy
        foreach (var enemy in enemies)
        {
            // Log enemy encounter
            combatLog.Add($"Encountered {enemy.Name}! (HP: {(int)enemy.Health}, DMG: {(int)enemy.Damage})");
                
            float enemyHealth = enemy.Health;
            int rounds = 0;
                
            // Combat rounds
            while (enemyHealth > 0 && currentHealth > 0)
            {
                rounds++;
                    
                // Player attacks first if faster
                if (playerStats.Speed >= enemy.Damage * 0.5f)
                {
                    // Calculate player damage with variance
                    float damageVariance = 0.8f + (float)_random.NextDouble() * 0.4f; // 80-120% damage
                    float baseDamage = Math.Max(1, playerStats.Attack - enemy.Damage * 0.2f);
                    float damage = (float)Math.Round(baseDamage * damageVariance);
                        
                    // Apply damage
                    enemyHealth -= damage;
                    totalDamageDealt += damage;
                        
                    // Log attack
                    combatLog.Add($"- Player attacks for {(int)damage} damage! {enemy.Name} has {Math.Max(0, (int)enemyHealth)} HP left.");
                        
                    // Check if enemy defeated
                    if (enemyHealth <= 0)
                    {
                        combatLog.Add($"- {enemy.Name} was defeated!");
                        enemiesDefeated++;
                        break;
                    }
                }
                    
                // Enemy attacks
                float enemyDamageVariance = 0.9f + (float)_random.NextDouble() * 0.2f; // 90-110% damage
                float enemyBaseDamage = Math.Max(1, enemy.Damage - playerStats.Defense * 0.5f);
                float enemyDamage = (float)Math.Round(enemyBaseDamage * enemyDamageVariance);
                    
                // Apply damage
                currentHealth -= enemyDamage;
                totalDamageTaken += enemyDamage;
                    
                // Log enemy attack
                combatLog.Add($"- {enemy.Name} attacks for {(int)enemyDamage} damage! Player has {Math.Max(0, (int)currentHealth)} HP left.");
                    
                // Player attacks second if slower
                if (playerStats.Speed < enemy.Damage * 0.5f && currentHealth > 0)
                {
                    // Calculate player damage with variance
                    float damageVariance = 0.8f + (float)_random.NextDouble() * 0.4f; // 80-120% damage
                    float baseDamage = Math.Max(1, playerStats.Attack - enemy.Damage * 0.2f);
                    float damage = (float)Math.Round(baseDamage * damageVariance);
                        
                    // Apply damage
                    enemyHealth -= damage;
                    totalDamageDealt += damage;
                        
                    // Log attack
                    combatLog.Add($"- Player attacks for {(int)damage} damage! {enemy.Name} has {Math.Max(0, (int)enemyHealth)} HP left.");
                        
                    // Check if enemy defeated
                    if (enemyHealth <= 0)
                    {
                        combatLog.Add($"- {enemy.Name} was defeated!");
                        enemiesDefeated++;
                        break;
                    }
                }
                    
                // Prevent infinite loops
                if (rounds > 20)
                {
                    combatLog.Add("- Combat taking too long, moving on...");
                    break;
                }
            }
                
            // Check player health
            if (currentHealth <= 0)
            {
                combatLog.Add("PLAYER DEFEATED! Dungeon run failed!");
                success = false;
                break;
            }
                
            // Recovery between fights
            float recovery = playerStats.MaxHealth * 0.15f;
            currentHealth = Math.Min(playerStats.MaxHealth, currentHealth + recovery);
            combatLog.Add($"Recovered {(int)recovery} HP. Current HP: {(int)currentHealth}");
        }
            
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