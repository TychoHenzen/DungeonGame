using System;

namespace DungeonGame;

/// <summary>
/// Item class
/// </summary>
public class Item
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Slot { get; set; }
    public int Power { get; set; }
    public float[] Signature { get; set; }
    public Guid Id { get; private set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }

    public Item()
    {
        Id = Guid.NewGuid();
    }
}