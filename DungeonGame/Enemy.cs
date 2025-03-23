using System;

namespace DungeonGame;

/// <summary>
/// Enemy class
/// </summary>
public class Enemy
{
    public string Name { get; set; }
    public string Type { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
    public float[] Signature { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
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
    
    /// <summary>
    /// Generates loot when the enemy is defeated
    /// </summary>
    /// <returns>An item as loot, or null if no loot is generated</returns>
    public Item GenerateLoot()
    {
        // Simple implementation - 50% chance to drop an item
        if (System.Random.Shared.NextDouble() > 0.5)
        {
            // Generate an item with a signature similar to the enemy's
            return ItemGenerator.GenerateItemWithSignature(Signature, 0.2f);
        }
        
        return null;
    }
}
