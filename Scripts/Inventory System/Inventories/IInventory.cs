using System.Collections.Generic;

/* 
INHERITANCE STRUCTURE:
IInventory
├── InventoryBase (abstract class)
│   ├── Inv_Container
│   ├── Inv_NPC
│   ├── Inv_Container
│   └── Inv_Player
InventoryData (class)
IInventoryExchange 
 */

public interface IInventory
{
    string InventoryId { get; }
    string Name { get; }
    InventoryType Type { get; }
    float Capacity { get; }
    Dictionary<IItem, int> Items { get; } // Key: ItemID, Value: Quantity

    bool HasItem(IItem item, int quantity);
    bool HasItemById(string itemId, int quantity);
    void AddItem(IItem item, int quantity);
    void RemoveItem(IItem item, int quantity);
    IItem[] GetItems();
    float totalWeight();
}