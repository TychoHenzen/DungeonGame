using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using Moq;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class DungeonStateTests
    {
        private DungeonState _dungeonState;
        private Mock<SpriteBatch> _mockSpriteBatch;
        private Mock<SpriteFont> _mockDefaultFont;
        private Mock<SpriteFont> _mockSmallFont;
        private Mock<Texture2D> _mockTexture;
        private Mock<SignatureGame> _mockGame;
        private Dungeon _testDungeon;
        private Player _testPlayer;

        [SetUp]
        public void Setup()
        {
            _mockGame = new Mock<SignatureGame>();
            _mockSpriteBatch = new Mock<SpriteBatch>();
            _mockDefaultFont = new Mock<SpriteFont>();
            _mockSmallFont = new Mock<SpriteFont>();
            _mockTexture = new Mock<Texture2D>();
            
            _testPlayer = new Player();
            _testDungeon = new Dungeon();
            
            _mockGame.Setup(g => g.GetPlayer()).Returns(_testPlayer);
            _mockGame.Setup(g => g.GetCurrentDungeon()).Returns(_testDungeon);
            _mockGame.Setup(g => g.IsRunningDungeon()).Returns(true);
            _mockGame.Setup(g => g.GetRunTimer()).Returns(10f);
            
            _dungeonState = new DungeonState(_mockGame.Object);
        }

        [Test]
        public void SetTexture_ShouldSetTexture()
        {
            // Act
            _dungeonState.SetTexture(_mockTexture.Object);
            
            // Assert - Since the texture is private, we can only test indirectly
            // This is a limited test, but at least verifies the method doesn't throw
            Assert.DoesNotThrow(() => _dungeonState.SetTexture(_mockTexture.Object));
        }

        [Test]
        public void Update_ShouldNotThrowException()
        {
            // Arrange
            var gameTime = new GameTime();
            
            // Act & Assert
            Assert.DoesNotThrow(() => _dungeonState.Update(gameTime));
        }

        [Test]
        public void Draw_ShouldNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _dungeonState.Draw(
                _mockSpriteBatch.Object, 
                _mockDefaultFont.Object, 
                _mockSmallFont.Object));
        }

        [Test]
        public void GetTileColor_ShouldReturnDifferentColorsForDifferentTiles()
        {
            // This test uses reflection to access the private method
            var methodInfo = typeof(DungeonState).GetMethod("GetTileColor", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var forestColor = (Color)methodInfo.Invoke(_dungeonState, new object[] { "forest" });
            var desertColor = (Color)methodInfo.Invoke(_dungeonState, new object[] { "desert" });
            var mountainColor = (Color)methodInfo.Invoke(_dungeonState, new object[] { "mountain" });
            
            // Assert
            Assert.That(forestColor, Is.Not.EqualTo(desertColor));
            Assert.That(forestColor, Is.Not.EqualTo(mountainColor));
            Assert.That(desertColor, Is.Not.EqualTo(mountainColor));
        }
    }
}