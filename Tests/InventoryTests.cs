#region

using DungeonGame.Code.Entities;

#endregion

namespace Tests;

[TestFixture]
public class InventoryTests
{
    [SetUp]
    public void Setup()
    {
        _inventory = new Inventory(5); // 5 slots for testing
    }

    private Inventory _inventory;

    [Test]
    public void AddItem_WithinCapacity_ShouldReturnTrue()
    {
        // Arrange
        var item = new Item { Name = "Test Item" };

        // Act
        var result = _inventory.AddItem(item);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_inventory.Items, Contains.Item(item));
        Assert.That(_inventory.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddItem_BeyondCapacity_ShouldReturnFalse()
    {
        // Arrange
        for (var i = 0; i < 5; i++) // Fill inventory to capacity
        {
            _inventory.AddItem(new Item { Name = $"Item {i}" });
        }

        var extraItem = new Item { Name = "Extra Item" };

        // Act
        var result = _inventory.AddItem(extraItem);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_inventory.Items, Does.Not.Contain(extraItem));
        Assert.That(_inventory.Items.Count, Is.EqualTo(5));
    }

    [Test]
    public void RemoveItem_ExistingItem_ShouldReturnTrue()
    {
        // Arrange
        var item = new Item { Name = "Test Item" };
        _inventory.AddItem(item);

        // Act
        var result = _inventory.RemoveItem(item);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_inventory.Items, Does.Not.Contain(item));
        Assert.That(_inventory.Items.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveItem_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var item = new Item { Name = "Test Item" };
        var differentItem = new Item { Name = "Different Item" };
        _inventory.AddItem(item);

        // Act
        var result = _inventory.RemoveItem(differentItem);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_inventory.Items, Contains.Item(item));
        Assert.That(_inventory.Items.Count, Is.EqualTo(1));
    }
}
