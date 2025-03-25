using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class SignatureGameTests
    {
        private TestSignatureGame _game;
        private Item _testItem;

        [SetUp]
        public void Setup()
        {
            _game = new TestSignatureGame();
            _testItem = new Item();
            _testItem.Name = "Test Item";
            _testItem.Type = "weapon";
            
            // Initialize the player and inventory in the test game
            _game.InitializeTestObjects();
        }

        [TearDown]
        public void TearDown()
        {
            _game.Dispose();
        }

        [Test]
        public void ChangeState_ShouldUpdateCurrentState()
        {
            // Act
            _game.ChangeState(GameStateType.Inventory);
            
            // Assert - We can verify the state was changed
            Assert.That(_game.CurrentStateType, Is.EqualTo(GameStateType.Inventory));
        }

        [Test]
        public void GetSetDungeonSlotItem_ShouldWorkCorrectly()
        {
            // Act
            _game.SetDungeonSlotItem(_testItem, 0);
            
            // Assert
            Assert.That(_game.GetDungeonSlotItem(0), Is.EqualTo(_testItem));
        }

        [Test]
        public void ClearDungeonSlot_ShouldRemoveItem()
        {
            // Arrange
            _game.SetDungeonSlotItem(_testItem, 0);
            
            // Act
            _game.ClearDungeonSlot(0);
            
            // Assert
            Assert.That(_game.GetDungeonSlotItem(0), Is.Null);
        }

        [Test]
        public void GetPlayer_ShouldReturnPlayerInstance()
        {
            // Act
            var player = _game.GetPlayer();
            
            // Assert
            Assert.That(player, Is.Not.Null);
            Assert.That(player, Is.InstanceOf<Player>());
        }

        [Test]
        public void GetInventory_ShouldReturnInventoryInstance()
        {
            // Act
            var inventory = _game.GetInventory();
            
            // Assert
            Assert.That(inventory, Is.Not.Null);
            Assert.That(inventory, Is.InstanceOf<Inventory>());
        }
    }
}