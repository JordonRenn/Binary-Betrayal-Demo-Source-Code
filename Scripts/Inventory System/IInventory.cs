using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    string InventoryId { get; }
    string Name { get; }
    int Capacity { get; }
    Dictionary<IItem, int> Items { get; } // Key: ItemID, Value: Quantity

    bool HasItem(IItem item, int quantity);
    void AddItem(IItem item, int quantity);
    void RemoveItem(IItem item, int quantity);
    IItem[] GetItems();
    int totalWeight();
}
