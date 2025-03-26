using System;
using NUnit.Framework;
using DungeonGame;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;

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
                Slot = SlotType.Weapon,
                Power = 10
            };
            
            // Act
            _player.EquipItem(weapon);
            
            // Assert
            Assert.That(_player.GetEquippedItem(SlotType.Weapon), Is.EqualTo(weapon));
        }
        
        [Test]
        public void UnequipItem_EquippedItem_ShouldBeRemoved()
        {
            // Arrange
            var weapon = new Item
            {
                Name = "Test Sword",
                Type = "Sword",
                Slot = SlotType.Weapon,
                Power = 10
            };
            _player.EquipItem(weapon);
            
            // Act
            _player.UnequipItem(SlotType.Weapon);
            
            // Assert
            Assert.That(_player.GetEquippedItem(SlotType.Weapon), Is.Null);
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
                Slot = SlotType.Weapon,
                Power = 10,
                Signature = new Signature([ 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f ])
            };
            
            // Act
            _player.EquipItem(weapon);
            var newStats = _player.CalculateStats();
            
            // Assert
            Assert.That(newStats.Attack, Is.GreaterThan(baseStats.Attack));
            Assert.That(newStats.Defense, Is.GreaterThan(baseStats.Defense));
        }
        
        [TestCase(SlotType.Weapon, true)]
        [TestCase(SlotType.Shield, true)]
        [TestCase(SlotType.Helmet, true)]
        [TestCase(SlotType.Armor, true)]
        [TestCase(SlotType.Amulet, true)]
        [TestCase(SlotType.Ring, true)]
        [TestCase(SlotType.Boots, true)]
        [TestCase(SlotType.Selected, false)]
        public void GetEquippedItems_ShouldReturnAllSlots(SlotType slot, bool isContained)
        {
            // Arrange - player already initialized
            
            // Act
            var items = _player.GetEquippedItems();
            
            // Assert
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count, Is.EqualTo(7)); // 7 equipment slots
            Assert.That(items.ContainsKey(slot), Is.EqualTo(isContained));
        }
    }
}
