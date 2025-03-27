namespace DungeonGame.Const;

public class DungeonConstants
{
    // Dungeon constants
    public int DefaultMapWidth { get; init; } = 20;
    public int DefaultMapHeight { get; init; } = 15;
    public int MaxExplorationSteps { get; init; } = 100;
    public int DefaultDungeonSlots { get; init; } = 3;
    public int InitialUnlockedDungeonSlots { get; init; } = 1;
    public float DungeonSimulationDelay { get; init; } = 3.0f;
}