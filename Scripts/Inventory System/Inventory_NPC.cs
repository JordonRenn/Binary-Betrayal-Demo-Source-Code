using System.Collections.Generic;

public class Inv_NPC : IInventory
{
    public string InventoryId { get; private set; }
    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public Dictionary<IItem, int> Items { get; private set; }

    public Inv_NPC(string inventoryId, string name, int capacity)
    {
        InventoryId = inventoryId;
        Name = name;
        Capacity = capacity;
        Items = new Dictionary<IItem, int>();
    }

    public bool HasItem(IItem item, int quantity)
    {
        return Items.ContainsKey(item) && Items[item] >= quantity;
    }

    public void AddItem(IItem item, int quantity)
    {
        if (Items.ContainsKey(item))
        {
            Items[item] += quantity;
        }
        else
        {
            Items[item] = quantity;
        }
    }

    public void RemoveItem(IItem item, int quantity)
    {
        if (HasItem(item, quantity))
        {
            Items[item] -= quantity;
            if (Items[item] <= 0)
            {
                Items.Remove(item);
            }
        }
    }

    public IItem[] GetItems()
    {
        return new List<IItem>(Items.Keys).ToArray();
    }

    public int totalWeight()
    {
        int total = 0;
        foreach (var item in Items)
        {
            total += item.Key.weight * item.Value;
        }
        return total;
    }
}