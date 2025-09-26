using System.Collections.Generic;
using GlobalEvents;

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

public abstract class InventoryBase : IInventory
{
    public string InventoryId { get; protected set; }
    public string Name { get; protected set; }
    public float Capacity { get; protected set; }
    public Dictionary<IItem, int> Items { get; protected set; }
    public List<InventoryListItem> InventoryListViewItems { get; protected set; } = new List<InventoryListItem>();
    public InventoryType Type { get; protected set; }

    protected InventoryBase(string inventoryId, string name, float capacity, InventoryType type)
    {
        InventoryId = inventoryId;
        Name = name;
        Capacity = capacity;
        Items = new Dictionary<IItem, int>();
        Type = type;
    }

    public bool HasItem(IItem item, int quantity)
    {
        return Items.ContainsKey(item) && Items[item] >= quantity;
    }

    // call from inventory manager, not directly
    public void AddItem(IItem item, int quantity)
    {
        try
        {
            if (Items.ContainsKey(item))
            {
                Items[item] += quantity;
                // Update existing item in list view items
                var existingItem = InventoryListViewItems.Find(x => x.Item.ItemId == item.ItemId);
                if (existingItem.Item != null)
                {
                    InventoryListViewItems.Remove(existingItem);
                    InventoryListViewItems.Add(new InventoryListItem { Item = item, Quantity = Items[item] });
                }
            }
            else
            {
                Items[item] = quantity;
                // Add new item to list view items
                InventoryListViewItems.Add(new InventoryListItem { Item = item, Quantity = quantity });
            }

            if (GameMaster.Instance != null)
            {
                // GameMaster.Instance?.oe_ItemAdded?.Invoke(Type, item.ItemId, item.Name);
                InventoryEvents.RaiseItemAdded(Type, item.ItemId, item.Name);
            }

            // SBGDebug.LogInfo($"Item added: {item.Name} x{quantity}. Total in inventory: {Items[item]}", $"class: InventoryBase | inventoryId:  {InventoryId}");

            // Optionally, you can also handle item-specific logic here
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogError($"Exception in InventoryBase.AddItem: {ex.Message}\n{ex.StackTrace}", "InventoryBase | AddItem");
            throw;
        }
    }

    public void RemoveItem(IItem item, int quantity)
    {
        if (HasItem(item, quantity))
        {
            Items[item] -= quantity;
            // Update or remove from list view items
            var existingItem = InventoryListViewItems.Find(x => x.Item.ItemId == item.ItemId);
            if (existingItem.Item != null)
            {
                InventoryListViewItems.Remove(existingItem);
                if (Items[item] > 0)
                {
                    InventoryListViewItems.Add(new InventoryListItem { Item = item, Quantity = Items[item] });
                }
            }

            if (Items[item] <= 0)
            {
                Items.Remove(item);
            }

            // GameMaster.Instance?.oe_ItemRemoved?.Invoke(Type, item.ItemId, item.Name);
            InventoryEvents.RaiseItemRemoved(Type, item.ItemId, item.Name);
        }
    }

    public IItem[] GetItems()
    {
        return new List<IItem>(Items.Keys).ToArray();
    }

    public float totalWeight()
    {
        float total = 0;
        foreach (var item in Items)
        {
            total += item.Key.Weight * item.Value;
        }
        return total;
    }

    public IItem GetItemById(string itemId)
    {
        foreach (var item in Items.Keys)
        {
            if (item.ItemId == itemId)
            {
                return item;
            }
        }
        return null;
    }

    public bool HasItemById(string itemId, int quantity = 1)
    {
        IItem item = GetItemById(itemId);
        return item != null && Items[item] >= quantity;
    }

    public int GetItemQuantityById(string itemId)
    {
        IItem item = GetItemById(itemId);
        return item != null ? Items[item] : 0;
    }
}