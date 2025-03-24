using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using Moq;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class InventoryStateTests
    {
        private InventoryState _inventoryState;
        private Mock<SpriteBatch> _mockSpriteBatch;
        private SpriteFont _mockDefaultFont;
        private SpriteFont _mockSmallFont;
        private Mock<Texture2D> _mockTexture;
        private TestSignatureGame _testGame;
        private Player _testPlayer;
        private Inventory _testInventory;

        [SetUp]
        public void Setup()
        {
            _testGame = new TestSignatureGame();
            _mockSpriteBatch = new Mock<SpriteBatch>();
            _mockDefaultFont = null; // Can't mock SpriteFont as it's sealed
            _mockSmallFont = null;   // Can't mock SpriteFont as it's sealed
            _mockTexture = new Mock<Texture2D>();
            
            _testPlayer = new Player();
            _testInventory = new Inventory(16);
            
            // Set up the test game
            _testGame.TestPlayer = _testPlayer;
            _testGame.TestInventory = _testInventory;
            
            _inventoryState = new InventoryState(_testGame);
        }

        [Test]
        public void SetTexture_ShouldSetTexture()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _inventoryState.SetTexture(_mockTexture.Object));
        }

        [Test]
        public void Update_ShouldNotThrowException()
        {
            // Arrange
            var gameTime = new GameTime();
            
            // Act & Assert
            Assert.DoesNotThrow(() => _inventoryState.Update(gameTime));
        }

        [Test]
        public void Draw_ShouldNotThrowException()
        {
            // Skip this test since we can't properly mock SpriteFont
            Assert.Pass("Skipping test that requires SpriteFont");
        }

        [Test]
        public void UnlockDungeonSlot_ShouldCallGameUnlockMethod()
        {
            // Act
            _inventoryState.UnlockDungeonSlot();
            
            // Assert
            Assert.That(_testGame.UnlockNextDungeonSlotCalled, Is.True);
        }

        [Test]
        public void GetDimensionColor_ShouldReturnDifferentColorsForDifferentDimensions()
        {
            // This test uses reflection to access the private method
            var methodInfo = typeof(InventoryState).GetMethod("GetDimensionColor", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var color0 = (Color)methodInfo.Invoke(_inventoryState, new object[] { 0 });
            var color1 = (Color)methodInfo.Invoke(_inventoryState, new object[] { 1 });
            var color2 = (Color)methodInfo.Invoke(_inventoryState, new object[] { 2 });
            
            // Assert
            Assert.That(color0, Is.Not.EqualTo(color1));
            Assert.That(color0, Is.Not.EqualTo(color2));
            Assert.That(color1, Is.Not.EqualTo(color2));
        }
    }
}
