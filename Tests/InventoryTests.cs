using System;
using NUnit.Framework;
using DungeonGame;
using DungeonGame.Code.Entities;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        private Inventory _inventory;
        
        [SetUp]
        public void Setup()
        {
            _inventory = new Inventory(5); // 5 slots for testing
        }
        
        [Test]
        public void AddItem_WithinCapacity_ShouldReturnTrue()
        {
            // Arrange
            var item = new Item { Name = "Test Item" };
            
            // Act
            bool result = _inventory.AddItem(item);
            
            // Assert
            Assert.That(result, Is.True);
            Assert.That(_inventory.Items, Contains.Item(item));
            Assert.That(_inventory.Items.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void AddItem_BeyondCapacity_ShouldReturnFalse()
        {
            // Arrange
            for (int i = 0; i < 5; i++) // Fill inventory to capacity
            {
                _inventory.AddItem(new Item { Name = $"Item {i}" });
            }
            
            var extraItem = new Item { Name = "Extra Item" };
            
            // Act
            bool result = _inventory.AddItem(extraItem);
            
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
            bool result = _inventory.RemoveItem(item);
            
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
            bool result = _inventory.RemoveItem(differentItem);
            
            // Assert
            Assert.That(result, Is.False);
            Assert.That(_inventory.Items, Contains.Item(item));
            Assert.That(_inventory.Items.Count, Is.EqualTo(1));
        }
    }
}
