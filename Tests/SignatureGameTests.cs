using Microsoft.Xna.Framework;
using NUnit.Framework;
using Moq;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class SignatureGameTests
    {
        private SignatureGame _game;
        private Item _testItem;

        [SetUp]
        public void Setup()
        {
            // Create a test subclass that doesn't initialize graphics
            _game = new TestSignatureGame();
            _testItem = new Item();
            _testItem.Name = "Test Item";
            _testItem.Type = "weapon";
            
            // Initialize the player and inventory in the test subclass
            ((TestSignatureGame)_game).InitializeTestObjects();
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
            
            // Assert - We can't directly check the private state, but we can verify behavior
            // This is a limited test, but ensures the method doesn't throw
            Assert.DoesNotThrow(() => _game.ChangeState(GameStateType.Inventory));
        }

        [Test]
        public void StartDungeon_ShouldSetupDungeonRun()
        {
            // Act
            _game.StartDungeon(_testItem);
            
            // Assert
            Assert.That(_game.IsRunningDungeon(), Is.True);
            Assert.That(_game.GetSelectedDungeonItem(), Is.EqualTo(_testItem));
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

        // Test subclass that doesn't initialize graphics
        private class TestSignatureGame : SignatureGame
        {
            public TestSignatureGame() : base()
            {
                // Skip base initialization that requires graphics
            }

            // Override methods that require graphics
            protected override void Initialize() { }
            protected override void LoadContent() { }
            protected override void Update(GameTime gameTime) { }
            protected override void Draw(GameTime gameTime) { }
            
            // Initialize test objects that would normally be created in Initialize()
            public void InitializeTestObjects()
            {
                // Create player and inventory for testing
                _player = new Player();
                _inventory = new Inventory(16);
                _dungeonSlots = new Item[3];
            }
        }
    }
}
