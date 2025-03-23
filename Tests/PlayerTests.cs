using System;
using NUnit.Framework;
using DungeonGame;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        private Player _player;
        
        [SetUp]
        public void Setup()
        {
            _player = new Player();
        }
        
        [Test]
        public void EquipItem_ValidItem_ShouldBeEquipped()
        {
            // Arrange
            var weapon = new Item
            {
                Name = "Test Sword",
                Type = "Sword",
                Slot = "weapon",
                Power = 10
            };
            
            // Act
            _player.EquipItem(weapon);
            
            // Assert
            Assert.That(_player.GetEquippedItem("weapon"), Is.EqualTo(weapon));
        }
        
        [Test]
        public void UnequipItem_EquippedItem_ShouldBeRemoved()
        {
            // Arrange
            var weapon = new Item
            {
                Name = "Test Sword",
                Type = "Sword",
                Slot = "weapon",
                Power = 10
            };
            _player.EquipItem(weapon);
            
            // Act
            _player.UnequipItem("weapon");
            
            // Assert
            Assert.That(_player.GetEquippedItem("weapon"), Is.Null);
        }
        
        [Test]
        public void CalculateStats_WithEquippedItems_ShouldIncreaseStats()
        {
            // Arrange
            var baseStats = _player.CalculateStats();
            
            var weapon = new Item
            {
                Name = "Test Sword",
                Type = "Sword",
                Slot = "weapon",
                Power = 10,
                Signature = new float[] { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }
            };
            
            // Act
            _player.EquipItem(weapon);
            var newStats = _player.CalculateStats();
            
            // Assert
            Assert.That(newStats.Attack, Is.GreaterThan(baseStats.Attack));
            Assert.That(newStats.Defense, Is.GreaterThan(baseStats.Defense));
        }
        
        [Test]
        public void GetEquippedItems_ShouldReturnAllSlots()
        {
            // Arrange - player already initialized
            
            // Act
            var items = _player.GetEquippedItems();
            
            // Assert
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count, Is.EqualTo(7)); // 7 equipment slots
            Assert.That(items.ContainsKey("weapon"), Is.True);
            Assert.That(items.ContainsKey("shield"), Is.True);
            Assert.That(items.ContainsKey("helmet"), Is.True);
            Assert.That(items.ContainsKey("armor"), Is.True);
            Assert.That(items.ContainsKey("amulet"), Is.True);
            Assert.That(items.ContainsKey("ring"), Is.True);
            Assert.That(items.ContainsKey("boots"), Is.True);
        }
    }
}
