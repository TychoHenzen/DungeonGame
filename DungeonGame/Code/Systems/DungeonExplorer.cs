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

using CombatTurn = (bool EnemyDefeated, float DamageDealt, float DamageTaken);

#endregion

/// <summary>
///     Handles automated dungeon exploration and combat
/// </summary>
public class DungeonExplorer
{
    private readonly Dungeon _dungeon;
    private readonly List<string> _explorationLog;
    private readonly Player _player;
    private float _currentHealth;
    private int _explorationSteps;

    private bool _isRunning;
    private PlayerStats _playerStats;

    public DungeonExplorer(Dungeon dungeon, Player player)
    {
        _dungeon = dungeon;
        _player = player;
        _explorationLog = [];
        _playerStats = _player.CalculateStats();
        _currentHealth = _playerStats.MaxHealth;
    }

    /// <summary>
    ///     Runs the dungeon exploration and returns the result
    /// </summary>
    public DungeonResult ExploreAutomatically()
    {
        _isRunning = true;
        _explorationSteps = 0;
        _explorationLog.Clear();

        // Set player starting position
        _dungeon.SetStartingPosition();

        // Apply affinity bonus based on dungeon signature
        CalculatePlayerStatsWithAffinity();

        _explorationLog.Add($"Entered dungeon with {_dungeon.Enemies.Count} enemies!");
        _explorationLog.Add(
            $"Player stats - HP: {(int)_playerStats.MaxHealth}, ATK: {(int)_playerStats.Attack}, DEF: {(int)_playerStats.Defense}, SPD: {(int)_playerStats.Speed}");

        // Run the exploration algorithm
        while (_isRunning && _explorationSteps < Constants.Dungeon.MaxExplorationSteps)
        {
            _explorationSteps++;

            // Check for enemies at current position
            var enemy = _dungeon.GetEnemyAt(_dungeon.PlayerX, _dungeon.PlayerY);
            if (enemy != null)
            {
                // Encounter enemy - engage in combat
                var combatResult = SimulateCombat(enemy);

                if (combatResult.EnemyDefeated)
                {
                    HandleEnemyDefeated(enemy);
                }
                else
                {
                    // Player was defeated - end dungeon run
                    _explorationLog.Add("PLAYER DEFEATED! Dungeon run failed!");
                    _isRunning = false;
                }
            }

            // If still exploring, move to next tile
            if (_isRunning)
            {
                MoveToNextTile();

                // Recover a bit between moves
                RecoverBetweenMoves();
            }
        }

        // Generate final dungeon result
        return CreateDungeonResult();
    }

    private void HandleEnemyDefeated(Enemy enemy)
    {
        // Add enemy to defeated list
        _dungeon.DefeatedEnemies.Add(enemy);

        // Generate and collect loot
        var loot = enemy.GenerateLoot();
        if (loot != null)
        {
            _dungeon.CollectedLoot.Add(loot);
            _explorationLog.Add($"Found {loot.Name} from defeated {enemy.Name}!");
        }
        else
        {
            _explorationLog.Add($"No loot found from defeated {enemy.Name}.");
        }

        // Check if all enemies are defeated
        if (_dungeon.AreAllEnemiesDefeated())
        {
            _explorationLog.Add("All enemies defeated! Dungeon cleared!");
            _isRunning = false;
        }
    }

    /// <summary>
    ///     Calculates player stats with affinity bonus applied
    /// </summary>
    private void CalculatePlayerStatsWithAffinity()
    {
        _playerStats = _player.CalculateStats();

        // Calculate signature affinity bonus
        var affinityBonus = CalculateAffinityBonus(_player.GetEquippedItems(), _dungeon.Signature);

        // Apply affinity bonus to stats
        _playerStats.Attack *= 1 + affinityBonus * Constants.Combat.AffinityAttackBonus;
        _playerStats.Defense *= 1 + affinityBonus * Constants.Combat.AffinityDefenseBonus;
        _playerStats.Speed *= 1 + affinityBonus * Constants.Combat.AffinitySpeedBonus;

        _explorationLog.Add($"Affinity bonus: {affinityBonus:P0}");
    }

    /// <summary>
    ///     Simulates combat between player and an enemy
    /// </summary>
    private CombatTurn SimulateCombat(Enemy enemy)
    {
        // Log enemy encounter
        _explorationLog.Add($"Encountered {enemy.Name}! (HP: {(int)enemy.Health}, DMG: {(int)enemy.Damage})");

        var enemyHealth = enemy.Health;
        float totalDamageDealt = 0;
        float totalDamageTaken = 0;
        var rounds = 0;

        // Combat rounds
        while (enemyHealth > 0 && _currentHealth > 0)
        {
            rounds++;

            if (PlayTurn(enemy,
                    ref enemyHealth,
                    ref totalDamageDealt,
                    ref totalDamageTaken,
                    out var simulateCombat))
            {
                return simulateCombat;
            }

            // Prevent infinite loops
            if (rounds > Constants.Combat.MaxCombatRounds)
            {
                _explorationLog.Add("- Combat taking too long, moving on...");
                break;
            }
        }

        return (enemyHealth <= 0, totalDamageDealt, totalDamageTaken);
    }

    private bool PlayTurn(Enemy enemy, ref float enemyHealth, ref float totalDamageDealt, ref float totalDamageTaken,
        out CombatTurn simulateCombat)
    {
        // Determine if player attacks first based on speed
        var playerFirst = _playerStats.Speed >= enemy.Damage * Constants.Combat.PlayerSpeedAdvantageFactor;

        // Player's first attack if going first
        if (playerFirst)
        {
            var damage = CalculatePlayerDamage(_playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name);
            totalDamageDealt += damage;

            // Check if enemy defeated
            if (enemyHealth <= 0)
            {
                _explorationLog.Add($"- {enemy.Name} was defeated!");
                simulateCombat = (true, totalDamageDealt, totalDamageTaken);
                return true;
            }
        }

        // Enemy attack
        var enemyDamage = CalculateEnemyDamage(enemy, _playerStats);
        ApplyDamageToPlayer(ref _currentHealth, enemyDamage, enemy.Name);
        totalDamageTaken += enemyDamage;

        // Check if player defeated
        if (_currentHealth <= 0)
        {
            simulateCombat = (false, totalDamageDealt, totalDamageTaken);
            return true;
        }

        // Player's second attack if going second
        if (!playerFirst)
        {
            var damage = CalculatePlayerDamage(_playerStats, enemy);
            ApplyDamageToEnemy(ref enemyHealth, damage, enemy.Name);
            totalDamageDealt += damage;

            // Check if enemy defeated
            if (enemyHealth <= 0)
            {
                _explorationLog.Add($"- {enemy.Name} was defeated!");
                simulateCombat = (true, totalDamageDealt, totalDamageTaken);
                return true;
            }
        }

        return false;
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
    private void ApplyDamageToEnemy(ref float enemyHealth, float damage, string enemyName)
    {
        enemyHealth -= damage;
        _explorationLog.Add(
            $"- Player attacks for {(int)damage} damage! {enemyName} has {Math.Max(0, (int)enemyHealth)} HP left.");
    }

    /// <summary>
    ///     Applies damage to player and logs result
    /// </summary>
    private void ApplyDamageToPlayer(ref float playerHealth, float damage, string enemyName)
    {
        playerHealth -= damage;
        _explorationLog.Add(
            $"- {enemyName} attacks for {(int)damage} damage! Player has {Math.Max(0, (int)playerHealth)} HP left.");
    }

    /// <summary>
    ///     Handles health recovery between moves
    /// </summary>
    private void RecoverBetweenMoves()
    {
        var recovery = _playerStats.MaxHealth * Constants.Combat.PlayerRecoveryPercent;
        _currentHealth = Math.Min(_playerStats.MaxHealth, _currentHealth + recovery);
    }

    /// <summary>
    ///     Moves the player to the next tile using a simple algorithm
    /// </summary>
    private void MoveToNextTile()
    {
        // Find the closest unexplored enemy
        var remainingEnemies = _dungeon.Enemies
            .Where(e => !_dungeon.DefeatedEnemies.Contains(e))
            .ToList();

        if (remainingEnemies.Count == 0)
        {
            // No more enemies - we're done
            _isRunning = false;
            return;
        }

        // Find the closest enemy
        var target = remainingEnemies.OrderBy(e =>
            Math.Abs(e.X - _dungeon.PlayerX) + Math.Abs(e.Y - _dungeon.PlayerY)
        ).First();

        // Move towards target
        var deltaX = Math.Sign(target.X - _dungeon.PlayerX);
        var deltaY = Math.Sign(target.Y - _dungeon.PlayerY);

        // Try moving horizontally or vertically
        var moved = false;
        if (deltaX != 0)
        {
            moved = _dungeon.MovePlayer(deltaX, 0);
        }

        if (!moved && deltaY != 0)
        {
            moved = _dungeon.MovePlayer(0, deltaY);
        }

        // If still can't move, try other directions
        if (!moved)
        {
            // Try all directions randomly
            var directions = new List<(int dx, int dy)>
            {
                (1, 0), (-1, 0), (0, 1), (0, -1)
            }.OrderBy(_ => Random.Shared.Next()).ToList();

            foreach (var (dx, dy) in directions)
            {
                if (_dungeon.MovePlayer(dx, dy))
                {
                    moved = true;
                    break;
                }
            }
        }

        if (moved)
        {
            var tile = _dungeon.TileMap[_dungeon.PlayerX, _dungeon.PlayerY];
            _explorationLog.Add($"Moved to {tile.Type} tile at ({_dungeon.PlayerX}, {_dungeon.PlayerY})");
        }
        else
        {
            _explorationLog.Add("Player is stuck! Cannot move to any adjacent tile.");
        }
    }

    /// <summary>
    ///     Creates the final dungeon result
    /// </summary>
    private DungeonResult CreateDungeonResult()
    {
        var success = _currentHealth > 0 && _dungeon.AreAllEnemiesDefeated();
        var enemiesDefeated = _dungeon.DefeatedEnemies.Count;
        float totalDamageDealt = 0; // We'd need to track this more explicitly
        var totalDamageTaken = _playerStats.MaxHealth - _currentHealth;

        // End of dungeon summary
        if (success)
        {
            _explorationLog.Add($"DUNGEON CLEARED! Defeated {enemiesDefeated}/{_dungeon.Enemies.Count} enemies.");
        }

        // Define casualties based on remaining health
        var casualties = success && _currentHealth / _playerStats.MaxHealth < Constants.Combat.LowHealthThreshold;
        if (casualties)
        {
            _explorationLog.Add("Barely survived with heavy injuries!");
        }

        // Determine final loot
        var loot = _dungeon.CollectedLoot.ToList();

        // Add bonus loot for completing dungeon
        if (success)
        {
            var bonusLoot = ItemGenerator.GenerateItemWithSignature(_dungeon.Signature);
            bonusLoot.Name = "Legendary " + bonusLoot.Name; // Mark it as special
            loot.Add(bonusLoot);
            _explorationLog.Add($"Found a legendary treasure: {bonusLoot.Name}!");
        }

        // Create and return dungeon result
        return new DungeonResult
        {
            Success = success,
            Casualties = casualties,
            Duration = _explorationSteps, // Use steps instead of minutes
            Loot = loot,
            CombatLog = _explorationLog,
            PlayerStats = new PlayerStats
            {
                Attack = (int)_playerStats.Attack,
                Defense = (int)_playerStats.Defense,
                Speed = (int)_playerStats.Speed,
                MaxHealth = (int)_playerStats.MaxHealth,
                RemainingHealth = (int)_currentHealth
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
    ///     Calculates the affinity bonus based on how well the player's equipment matches the dungeon's signature
    /// </summary>
    private static float CalculateAffinityBonus(Dictionary<SlotType, Item?> equippedItems, Signature dungeonSignature)
    {
        float affinitySum = 0;
        var itemCount = 0;

        foreach (var item in equippedItems.Values
                     .Where(item => item is not null))
        {
            try
            {
                var similarity = item.Signature.CalculateSimilarityWith(dungeonSignature);
                affinitySum += similarity;
                itemCount++;
            }
            catch (ArgumentException)
            {
                // Skip items with invalid signatures
            }
        }

        return itemCount > 0 ? affinitySum / itemCount : 0;
    }
}
