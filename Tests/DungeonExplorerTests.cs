#region

using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Systems;

#endregion

namespace Tests;

[TestFixture]
public class DungeonExplorerTests
{
    [SetUp]
    public void Setup()
    {
        // Create a basic player for testing
        _player = new Player
        {
            Name = "TestPlayer"
        };

        // Create a basic weapon with a signature
        var weapon = new Item
        {
            Name = "Test Sword",
            Type = "Weapon",
            Slot = SlotType.Weapon,
            Signature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]),
            Attack = 10,
            Defense = 0,
            Speed = 5,
            Power = 10
        };

        // Equip items to player
        _player.EquipItem(weapon);

        // Create a simple dungeon for testing
        _dungeon = DungeonGenerator.GenerateDungeon(weapon.Signature);

        // Make sure the dungeon is small and simple for testing
        _dungeon.Width = 5;
        _dungeon.Height = 5;
        _dungeon.TileMap = new Tile[5, 5];

        // Create all passable tiles
        for (var x = 0; x < 5; x++)
        {
            for (var y = 0; y < 5; y++)
            {
                _dungeon.TileMap[x, y] = new Tile
                {
                    Type = "Stone",
                    X = x,
                    Y = y,
                    IsPassable = true,
                    Signature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f])
                };
            }
        }

        // Add a single weak enemy
        var enemy = new Enemy
        {
            Name = "Test Enemy",
            Type = "Goblin",
            Health = 10,
            Damage = 2,
            Signature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]),
            X = 2,
            Y = 2
        };

        _dungeon.Enemies.Clear();
        _dungeon.Enemies.Add(enemy);
    }

    private Player _player;
    private Dungeon _dungeon;

    [Test]
    public void ExploreAutomatically_WithStrongPlayer_ShouldSucceed()
    {
        // Arrange
        var explorer = new DungeonExplorer(_dungeon, _player);

        // Act
        var result = explorer.ExploreAutomatically();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Stats.EnemiesDefeated, Is.EqualTo(1));
        Assert.That(result.PlayerStats.RemainingHealth, Is.GreaterThan(0));
    }

    [Test]
    public void ExploreAutomatically_WithWeakPlayer_ShouldFail()
    {
        // Arrange
        // Make the enemy much stronger
        _dungeon.Enemies[0].Health = 100;
        _dungeon.Enemies[0].Damage = 50;

        var explorer = new DungeonExplorer(_dungeon, _player);

        // Act
        var result = explorer.ExploreAutomatically();

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.PlayerStats.RemainingHealth, Is.LessThanOrEqualTo(0));
    }

    [Test]
    public void ExploreAutomatically_EmptyDungeon_ShouldSucceedWithoutCombat()
    {
        // Arrange
        _dungeon.Enemies.Clear();
        var explorer = new DungeonExplorer(_dungeon, _player);

        // Act
        var result = explorer.ExploreAutomatically();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Stats.EnemiesDefeated, Is.EqualTo(0));
        Assert.That(result.Loot, Is.Not.Empty, "Should still get bonus loot for completing dungeon");
    }

    [Test]
    public void ExploreAutomatically_ShouldGenerateLoot_WhenSuccessful()
    {
        // Arrange
        var explorer = new DungeonExplorer(_dungeon, _player);

        // Act
        var result = explorer.ExploreAutomatically();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Loot, Is.Not.Empty);
        Assert.That(result.Loot.Count, Is.GreaterThanOrEqualTo(1), "Should at least get the legendary item");
    }
}
