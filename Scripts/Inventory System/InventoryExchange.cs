using System.Collections.Generic;

public class InventoryExchange
{
    private IInventory sourceInventory;
    private IInventory targetInventory;

    public InventoryExchange(IInventory source, IInventory target)
    {
        sourceInventory = source;
        targetInventory = target;
    }

    public bool ExchangeItem(IItem item, int quantity)
    {
        if (sourceInventory.HasItem(item, quantity))
        {
            sourceInventory.RemoveItem(item, quantity);
            targetInventory.AddItem(item, quantity);
            return true;
        }
        return false;
    }

    public void TransferAllItems()
    {
        foreach (var item in sourceInventory.GetItems())
        {
            int quantity = sourceInventory.Items[item];
            if (quantity > 0)
            {
                ExchangeItem(item, quantity);
            }
        }
    }
}