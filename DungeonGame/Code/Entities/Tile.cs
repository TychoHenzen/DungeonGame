#region

using DungeonGame.Code.Enums;

#endregion

namespace DungeonGame.Code.Entities;

/// <summary>
///     Tile class
/// </summary>
public class Tile
{
    public TileType Type { get; set; }
    public Signature Signature { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsPassable { get; set; }
}
