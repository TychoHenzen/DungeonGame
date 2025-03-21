namespace DungeonGame;

/// <summary>
/// Program entry point
/// </summary>
public static class Program
{
    public static void Main()
    {
        using (var game = new DungeonGame.SignatureGame())
            game.Run();
    }
}