using System.Collections.Generic;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Inventory class
/// </summary>
public class Inventory
{
    public List<Item> Items { get; }
    public int Capacity { get; }
        
    public Inventory(int capacity)
    {
        Items = [];
        Capacity = capacity;
    }
        
    public bool AddItem(Item item)
    {
        if (Items.Count >= Capacity)
        {
            return false;
        }

        Items.Add(item);
        return true;

    }
        
    public bool RemoveItem(Item item)
    {
        return Items.Remove(item);
    }
}