#region

using DungeonGame.Code.Core;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Models;
using Microsoft.Xna.Framework;

#endregion

namespace Tests;

/// <summary>
///     Test implementation of SignatureGame for unit testing
/// </summary>
public class TestSignatureGame : SignatureGame
{
    public TestSignatureGame()
    {
        // Initialize with defaults to avoid null reference exceptions
        TestPlayer = new Player();
        TestInventory = new Inventory(16);
        TestDungeon = new Dungeon();
        TestRunningDungeon = false;
        TestRunTimer = 0;
    }

    // Properties for testing
    public Player TestPlayer { get; set; }
    public Inventory TestInventory { get; set; }
    public Dungeon TestDungeon { get; set; }
    public DungeonResult TestDungeonResult { get; set; }
    public bool TestRunningDungeon { get; set; }
    public float TestRunTimer { get; set; }
    public bool UnlockNextDungeonSlotCalled { get; set; }
    public GameStateType CurrentStateType { get; private set; }

    // Override methods that require graphics
    protected override void Initialize()
    {
    }

    protected override void LoadContent()
    {
    }

    protected override void Update(GameTime gameTime)
    {
    }

    protected override void Draw(GameTime gameTime)
    {
    }

    // Hide base methods to use test properties instead
    public new Player GetPlayer()
    {
        return TestPlayer ?? base.GetPlayer();
    }

    public new Inventory GetInventory()
    {
        return TestInventory ?? base.GetInventory();
    }

    public new Dungeon GetCurrentDungeon()
    {
        return TestDungeon ?? base.GetCurrentDungeon();
    }

    public new DungeonResult GetDungeonResult()
    {
        return TestDungeonResult ?? base.GetDungeonResult();
    }

    public new bool IsRunningDungeon()
    {
        return TestRunningDungeon;
    }

    public new float GetRunTimer()
    {
        return TestRunTimer;
    }

    // Track method calls
    public new void UnlockNextDungeonSlot()
    {
        UnlockNextDungeonSlotCalled = true;
    }

    // Override ChangeState to track state changes
    public override void ChangeState(GameStateType stateType)
    {
        CurrentStateType = stateType;
        // Don't call base as it requires initialized _gameStates
    }

    // Initialize all needed objects for testing
    public void InitializeTestObjects()
    {
        // Set protected fields for testing
        _player = TestPlayer;
        _inventory = TestInventory;
        _currentDungeon = TestDungeon;
        _dungeonResult = TestDungeonResult;
        _runningDungeon = TestRunningDungeon;
        _runTimer = TestRunTimer;
        _dungeonSlotItems = new Item[3];
    }
}
