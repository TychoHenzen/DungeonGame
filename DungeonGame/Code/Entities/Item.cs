using System;
using DungeonGame.Code.Enums;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Item class
/// </summary>
public class Item
{
    public string Name { get; set; }
    public string Type { get; set; }
    public SlotType Slot { get; set; }
    public int Power { get; set; }
    public Signature Signature { get; set; }
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}
