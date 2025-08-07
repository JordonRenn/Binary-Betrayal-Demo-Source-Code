using UnityEngine;
using System;

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

 /* 
 HOW TO USE:
 1. Create an instance of InventoryData with the required parameters.
 2. Call InventoryFactory.CreateInventory(data) to create the inventory.
  */

public static class InventoryFactory
{
    public static IInventory CreateInventory(InventoryData data)
    {
        if (data.Items == null)
            throw new ArgumentNullException(nameof(data.Items), "Items dictionary cannot be null.");

        // Determine inventory type based on InventoryId prefix
        return data.InventoryId switch
        {
            string id when id.StartsWith("PLAYER_") => CreatePlayerInventory(data.InventoryId, data.Name, data.Capacity),
            string id when id.StartsWith("CONTAINER_") => CreateContainerInventory(data.InventoryId, data.Name, data.Capacity),
            string id when id.StartsWith("NPC_") => CreateNPCInventory(data.InventoryId, data.Name, data.Capacity),
            _ => throw new ArgumentException($"Unknown inventory type for ID: {data.InventoryId}")
        };
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