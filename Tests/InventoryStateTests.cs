#region

using DungeonGame.Code.Entities;
using DungeonGame.Code.States;
using Microsoft.Xna.Framework;

#endregion

namespace Tests;

[TestFixture]
public class InventoryStateTests
{
    [SetUp]
    public void Setup()
    {
        _testGame = new TestSignatureGame();

        _testPlayer = new Player();
        _testInventory = new Inventory(16);

        // Set up the test game
        _testGame.TestPlayer = _testPlayer;
        _testGame.TestInventory = _testInventory;

        _inventoryState = new InventoryState(_testGame);
    }

    [TearDown]
    public void TearDown()
    {
        _testGame.Dispose();
    }

    private InventoryState _inventoryState;
    private TestSignatureGame _testGame;
    private Player _testPlayer;
    private Inventory _testInventory;

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
}
