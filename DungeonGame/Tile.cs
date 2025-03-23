namespace DungeonGame;

/// <summary>
/// Tile class
/// </summary>
public class Tile
{
    public string Type { get; set; }
    public float[] Signature { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsPassable { get; set; }
    
    /// <summary>
    /// Gets a Signature object from the raw signature values
    /// </summary>
    /// <returns>A Signature object, or null if the signature is invalid</returns>
    public Signature GetSignatureObject()
    {
        if (Signature == null)
            return null;
            
        try
        {
            return new Signature(Signature);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
