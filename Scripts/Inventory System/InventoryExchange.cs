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

public static class InventoryExchange
{
    public static bool TransferSingleItem(IInventory sourceInventory, IInventory targetInventory, IItem item, int quantity)
    {
        if (sourceInventory.HasItem(item, quantity))
        {
            sourceInventory.RemoveItem(item, quantity);
            targetInventory.AddItem(item, quantity);
            return true;
        }
        return false;
    }

    public static void TransferAllItems(IInventory source, IInventory target)
    {
        foreach (var item in source.GetItems())
        {
            int quantity = source.Items[item];
            if (quantity > 0)
            {
                TransferSingleItem(source, target, item, quantity);
            }
        }
    }

    public static void ExchangeMultipleItems(IInventory source, IInventory target, List<IItem> sourceItemsToTransfer, List<IItem> targetItemsToTransfer)
    {
        foreach (var item in sourceItemsToTransfer)
        {
            int quantity = source.Items[item];
            if (source.HasItem(item, quantity))
            {
                TransferSingleItem(source, target, item, quantity);
            }
        }

        foreach (var item in targetItemsToTransfer)
        {
            int quantity = target.Items[item];
            if (target.HasItem(item, quantity))
            {
                TransferSingleItem(target, source, item, quantity);
            }
        }
    }

    public static void TransferItemsByType(IInventory source, IInventory target, ItemType itemType)
    {
        foreach (var item in source.GetItems())
        {
            if (item.Type == itemType)
            {
                int quantity = source.Items[item];
                if (quantity > 0)
                {
                    TransferSingleItem(source, target, item, quantity);
                }
            }
        }
    }
}