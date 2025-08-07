using System.Collections.Generic;

public class Inv_Player : IInventory
{
    public string InventoryId { get; private set; }
    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public Dictionary<IItem, int> Items { get; private set; }

    public Inv_Player(string inventoryId, string name, int capacity)
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
        if (NotificationSystem.Instance != null)
        {
            Notification notification = new Notification
            {
                message = $"Added {quantity}x {item.Name} to your inventory.",
                type = NotificationType.Normal
            };

            NotificationSystem.Instance.DisplayNotification(notification);
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

    public int totalWeight()
    {
        int total = 0;
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