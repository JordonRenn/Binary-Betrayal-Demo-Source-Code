using UnityEngine;
using System;
using System.Collections.Generic;

public static class InventoryFactory
{
    public static IInventory CreateInventory(InventoryData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "InventoryData cannot be null.");

        // Determine inventory type using switch statement
        IInventory inventory;
        
        // Try to determine inventory type from explicit type first, then fallback to ID prefix
        switch (data.inventoryType)
        {
            case InventoryType.Player:
                inventory = CreatePlayerInventory(data.inventoryId, data.displayName, (int)data.maxWeight);
                break;
            case InventoryType.Container:
                inventory = CreateContainerInventory(data.inventoryId, data.displayName, (int)data.maxWeight);
                break;
            case InventoryType.NPC:
                inventory = CreateNPCInventory(data.inventoryId, data.displayName, (int)data.maxWeight);
                break;
            default:
                // Fall back to ID prefix detection if inventoryType isn't specified or recognized
                inventory = data.inventoryId switch
                {
                    string id when id.StartsWith("PLAYER_") => CreatePlayerInventory(data.inventoryId, data.displayName, (int)data.maxWeight),
                    string id when id.StartsWith("CONTAINER_") => CreateContainerInventory(data.inventoryId, data.displayName, (int)data.maxWeight),
                    string id when id.StartsWith("NPC_") => CreateNPCInventory(data.inventoryId, data.displayName, (int)data.maxWeight),
                    _ => throw new ArgumentException($"Unknown inventory type for ID: {data.inventoryId}")
                };
                break;
        }

        // Populate the inventory with items if available
        if (data.items != null)
        {
            foreach (var itemData in data.items)
            {
                var item = ItemFactory.CreateItemFromDatabase(itemData.itemId);
                if (item != null)
                {
                    // check if item already exists in inventory
                    if (!inventory.Items.ContainsKey(item))
                    {
                        inventory.AddItem(item, 1);
                    }
                    else
                    {
                        inventory.Items[item] += 1; // Increment quantity
                    }
                }
            }
        }
        else
        {
            SBGDebug.LogInfo("No items found in inventory data.", "InventoryFactory | CreateInventory");
        }

        return inventory;
    }
    
    public static InventoryData CreateInventoryDataFromContext (InventoryContextData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "InventoryContextData cannot be null.");

        var inventoryData = new InventoryData
        {
            inventoryId = data.InventoryId,
            // Map other fields as necessary
        };

        // Convert ItemContextData to ItemData using ItemFactory
        foreach (var ctxItem in data.Items)
        {
            var item = ItemFactory.CreateItemFromDatabase(ctxItem.ItemId) as ItemData;
            if (item != null)
            {
                inventoryData.items.Add(item);
            }
            else
            {
                SBGDebug.LogWarning($"ItemFactory could not create item for id: {ctxItem.ItemId}", "InventoryFactory");
            }
        }

        return inventoryData;
    }

    public static IInventory CreatePlayerInventory(string inventoryId, string name, int capacity)
    {
        return new Inv_Player(inventoryId, name, capacity);
    }

    public static IInventory CreateContainerInventory(string inventoryId, string name, int capacity)
    {
        return new Inv_Container(inventoryId, name, capacity);
    }

    public static IInventory CreateNPCInventory(string inventoryId, string name, int capacity)
    {
        return new Inv_NPC(inventoryId, name, capacity);
    }

    private static IItem ValidateAndCreate(Func<IInventory> inventorySupplier, Func<IItem> itemSupplier)
    {
        var inventory = inventorySupplier();
        var item = itemSupplier();

        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory), "Inventory cannot be null.");

        if (item == null)
            throw new ArgumentNullException(nameof(item), "Item cannot be null.");

        return item;
    }
}