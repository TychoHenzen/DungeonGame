#region

using System;

#endregion

namespace DungeonGame.Code.Core;

/// <summary>
///     Program entry point
/// </summary>
file static class Program
{
    [STAThread]
    public static void Main()
    {
        using var game = new SignatureGame();
        game.Run();
    }
}
