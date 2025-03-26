// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace DungeonGame.Const;

/// <summary>
/// Contains game constants that can be serialized to disk
/// </summary>
public sealed class GameConstants
{
    // Player constants
    internal float BasePlayerHealth { get; init; } = 100f;
    internal float BasePlayerAttack { get; init; } = 10f;
    internal float BasePlayerDefense { get; init; } = 5f;
    internal float BasePlayerSpeed { get; init; } = 10f;
    public int DefaultInventoryCapacity { get; init; } = 20;
    
    
    // Signature constants
    public float SignatureHighThreshold { get; init; } = 0.5f;
    public float SignatureLowThreshold { get; init; } = -0.5f;
    public float DefaultSignatureVariance { get; init; } = 0.2f;
    


}