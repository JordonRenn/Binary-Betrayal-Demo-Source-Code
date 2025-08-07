using System.Collections.Generic;

/* 
INHERITANCE STRUCTURE:
IInventory
├── InventoryBase (abstract class)
│   ├── Inv_Container
│   ├── Inv_NPC
│   ├── Inv_Container
│   └── Inv_Player
InventoryData (struct)
IInventoryExchange 
 */

public interface IInventory
{
    string InventoryId { get; }
    string Name { get; }
    InventoryType Type { get; }
    int Capacity { get; }
    Dictionary<IItem, int> Items { get; } // Key: ItemID, Value: Quantity

    bool HasItem(IItem item, int quantity);
    bool HasItemById(string itemId, int quantity);
    void AddItem(IItem item, int quantity);
    void RemoveItem(IItem item, int quantity);
    IItem[] GetItems();
    int totalWeight();
}

public struct InventoryData
{
    public string InventoryId;
    public string Name;
    public int Capacity;
    public Dictionary<IItem, int> Items;
    public InventoryType Type;

    public InventoryData(string inventoryId, string name, int capacity, Dictionary<IItem, int> items, InventoryType type)
    {
        InventoryId = inventoryId;
        Name = name;
        Capacity = capacity;
        Items = items;
        Type = type;
    }
}