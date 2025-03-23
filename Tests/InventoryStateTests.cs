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
        private Mock<SpriteFont> _mockDefaultFont;
        private Mock<SpriteFont> _mockSmallFont;
        private Mock<Texture2D> _mockTexture;
        private Mock<SignatureGame> _mockGame;
        private Player _testPlayer;
        private Inventory _testInventory;

        [SetUp]
        public void Setup()
        {
            _mockGame = new Mock<SignatureGame>();
            _mockSpriteBatch = new Mock<SpriteBatch>();
            _mockDefaultFont = new Mock<SpriteFont>();
            _mockSmallFont = new Mock<SpriteFont>();
            _mockTexture = new Mock<Texture2D>();
            
            _testPlayer = new Player();
            _testInventory = new Inventory(16);
            
            _mockGame.Setup(g => g.GetPlayer()).Returns(_testPlayer);
            _mockGame.Setup(g => g.GetInventory()).Returns(_testInventory);
            
            _inventoryState = new InventoryState(_mockGame.Object);
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
            // Act & Assert
            Assert.DoesNotThrow(() => _inventoryState.Draw(
                _mockSpriteBatch.Object, 
                _mockDefaultFont.Object, 
                _mockSmallFont.Object));
        }

        [Test]
        public void UnlockDungeonSlot_ShouldCallGameUnlockMethod()
        {
            // Act
            _inventoryState.UnlockDungeonSlot();
            
            // Assert
            _mockGame.Verify(g => g.UnlockNextDungeonSlot(), Times.Once);
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