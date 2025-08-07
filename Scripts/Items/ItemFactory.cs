using UnityEngine;
using System;

/* 
INHERITANCE STRUCTURE:
IItem
├── ItemBase (abstract class)
│   ├── ItemTypes (e.g., Item_Misc, Item_Material, Item_Food, Item_Keys, Item_Quest)
│   └── ItemData (concrete implementation of IItem)
 */

 /* 
 HOW TO USE ItemFactory:
    1. Create an ItemData instance with the required properties.
    2. Call ItemFactory.CreateItem(itemData) to create an IItem instance.
    3. For specialized items (keys, quests, phones), use the specific creation methods
       like ItemFactory.CreateKeyItem, ItemFactory.CreateQuestItem, etc.    
  */

public static class ItemFactory
{
    /// <summary>
    /// Creates an item instance from ItemData. For items requiring special properties 
    /// (KeyType, questId, phoneNumber), use the specialized creation methods instead.
    /// </summary>
    /// <param name="itemData">The item data containing basic item information</param>
    /// <returns>An IItem instance or null if creation fails</returns>
    public static IItem CreateItem(ItemData itemData)
    {
        try
        {
            if (itemData == null)
            {
                SBGDebug.LogError("Item data is null", "ItemFactory");
                return null;
            }

            return itemData.Type switch
            {
                ItemType.Misc => ValidateAndCreate(() => new Item_Misc(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight), "Misc"),
                ItemType.Material => ValidateAndCreate(() => new Item_Material(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight), "Material"),
                ItemType.Food => ValidateAndCreate(() => new Item_Food(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight), "Food"),
                ItemType.Keys => ValidateAndCreate(() => new Item_Keys(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, KeyType.Key), "Keys"),
                ItemType.Quest => ValidateAndCreate(() => new Item_Quest(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight, string.Empty), "Quest"),
                ItemType.Medical => ValidateAndCreate(() => new Item_Medical(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight), "Medical"),
                ItemType.Phone => ValidateAndCreate(() => new Item_Phone(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight, string.Empty), "Phone"),
                ItemType.Tools => ValidateAndCreate(() => new Item_Tools(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, itemData.weight), "Tools"),
                _ => throw new ArgumentOutOfRangeException($"Unsupported item type: {itemData.Type}")
            };
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    #region Specialized Item Creation Methods

    /// <summary>
    /// Creates a key item with specified key type
    /// </summary>
    public static IItem CreateKeyItem(string id, string name, string description, Sprite icon, KeyType keyType)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Keys(id, name, description, icon, null, keyType), "Keys");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create key item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    public static IItem CreateQuestItem(string id, string name, string description, Sprite icon, string questId, int stackSize = 1)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Quest(id, name, description, icon, null, stackSize, questId), "Quest");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create quest item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    public static IItem CreatePhoneItem(string id, string name, string description, Sprite icon, string phoneNumber, int stackSize = 1)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Phone(id, name, description, icon, null, stackSize, phoneNumber), "Phone");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create phone item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    public static IItem CreateStackableItem(ItemType itemType, string id, string name, string description, Sprite icon, int stackSize)
    {
        try
        {
            return itemType switch
            {
                ItemType.Material => ValidateAndCreate(() => new Item_Material(id, name, description, icon, null, stackSize), "Material"),
                ItemType.Food => ValidateAndCreate(() => new Item_Food(id, name, description, icon, null, stackSize), "Food"),
                ItemType.Medical => ValidateAndCreate(() => new Item_Medical(id, name, description, icon, null, stackSize), "Medical"),
                ItemType.Tools => ValidateAndCreate(() => new Item_Tools(id, name, description, icon, null, stackSize), "Tools"),
                ItemType.Misc => ValidateAndCreate(() => new Item_Misc(id, name, description, icon, null, stackSize), "Misc"),
                _ => throw new ArgumentOutOfRangeException($"Item type {itemType} is not stackable or not supported")
            };
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create stackable item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates and creates an item using the provided creation function
    /// </summary>
    private static IItem ValidateAndCreate(Func<IItem> createFunc, string itemTypeName)
    {
        IItem item = createFunc();
        if (item == null)
        {
            SBGDebug.LogError($"Failed to create {itemTypeName} item", "ItemFactory");
            return null;
        }

        // Basic validation
        if (string.IsNullOrEmpty(item.ItemId))
        {
            SBGDebug.LogError($"{itemTypeName} item has invalid ID", "ItemFactory");
            return null;
        }
        if (string.IsNullOrEmpty(item.Name))
        {
            SBGDebug.LogError($"{itemTypeName} item has invalid Name", "ItemFactory");
            return null;
        }
        if (item.Icon == null)
        {
            SBGDebug.LogWarning($"{itemTypeName} item '{item.Name}' has no icon assigned", "ItemFactory");
        }
        if (item.weight < 0)
        {
            SBGDebug.LogError($"{itemTypeName} item '{item.Name}' has negative weight", "ItemFactory");
            return null;
        }

        return item;
    }

    #endregion
}