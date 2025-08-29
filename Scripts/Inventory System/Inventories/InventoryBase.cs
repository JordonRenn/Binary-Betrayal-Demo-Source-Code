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

public abstract class InventoryBase : IInventory
{
    public string InventoryId { get; protected set; }
    public string Name { get; protected set; }
    public float Capacity { get; protected set; }
    public Dictionary<IItem, int> Items { get; protected set; }
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
        float newWeight = totalWeight() + (item.weight * quantity);
        if (newWeight > Capacity)
        {
            //if player inventory, show error notification
            if (InventoryManager.Instance.playerInventory.Type == InventoryType.Player)
            {
                Notification notification = new Notification
                {
                    message = "You are trying to add too many items to your inventory.",
                    type = NotificationType.Warning
                };
                NotificationSystem.Instance.DisplayNotification(notification);

                // eventually add encumberance logic here
                // continue on with adding item
            }
            else
            {
                throw new System.InvalidOperationException("Cannot add item: Inventory is full.");
            }
        }

        if (Items.ContainsKey(item))
        {
            Items[item] += quantity;
        }
        else
        {
            Items[item] = quantity;
        }

        if (NotificationSystem.Instance != null)
        {
            Notification notification = new Notification
            {
                message = $"Added {quantity}x {item.Name} to your inventory.",
                type = NotificationType.Normal
            };
            NotificationSystem.Instance.DisplayNotification(notification);
        }

        if (GameMaster.Instance != null)
        {
            GameMaster.Instance?.oe_ItemAdded?.Invoke(Type, item.ItemId, item.Name);
        }

        SBGDebug.LogInfo($"Item added: {item.Name} x{quantity}. Total in inventory: {Items[item]}", $"class: InventoryBase | inventoryId:  {InventoryId}");

        // Optionally, you can also handle item-specific logic here
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

            GameMaster.Instance?.oe_ItemRemoved?.Invoke(Type, item.ItemId, item.Name);
        }

        if (NotificationSystem.Instance != null)
        {
            Notification notification = new Notification
            {
                message = $"Removed {quantity}x {item.Name} from your inventory.",
                type = NotificationType.Normal
            };
            NotificationSystem.Instance.DisplayNotification(notification);
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
            total += item.Key.weight * item.Value;
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