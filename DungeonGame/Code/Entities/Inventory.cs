using System.Collections.Generic;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Inventory class
/// </summary>
public class Inventory
{
    public List<Item> Items { get; private set; }
    public int Capacity { get; private set; }
        
    public Inventory(int capacity)
    {
        Items = new List<Item>();
        Capacity = capacity;
    }
        
    public bool AddItem(Item item)
    {
        if (Items.Count < Capacity)
        {
            Items.Add(item);
            return true;
        }
            
        return false;
    }
        
    public bool RemoveItem(Item item)
    {
        return Items.Remove(item);
    }
}