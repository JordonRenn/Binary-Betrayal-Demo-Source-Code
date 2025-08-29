using System;
using System.Collections.Generic;
using UnityEngine;

#region ItemData
[Serializable]
public class ItemData
{
    public string itemId;
    public string name;
    public string description;
    public float value;
    public float weight;
    public ItemType itemType;
    public Sprite icon;
    public bool isUsable;
    public bool isEquippable;
    public bool isQuestItem;
    public Dictionary<string, float> attributes;

    public ItemData(string itemId, string name, string description, float value, float weight, ItemType itemType, Sprite icon, bool isUsable = false, bool isEquippable = false, bool isQuestItem = false)
    {
        this.itemId = itemId;
        this.name = name;
        this.description = description;
        this.value = value;
        this.weight = weight;
        this.itemType = itemType;
        this.icon = icon;
        this.isUsable = isUsable;
        this.isEquippable = isEquippable;
        this.isQuestItem = isQuestItem;
        attributes = new Dictionary<string, float>();
    }
}
#endregion

public class ItemContextData
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}

#region ItemMasterData
[System.Serializable]
public class ItemMasterData
{
    // Core properties - match ItemData naming where possible
    public string itemId;
    public string displayName;
    public string description;
    public float weight;
    public float value;
    public string category;
    public string subtype;
    public string effectType;
    public string effectValue;
    public string rarity;
    public string viewLogic;
    public string iconPath;
    public int quantity = 1;
    public int maxStack = 99;
    
    // Boolean flags (stored as strings in CSV, parsed to booleans)
    public string isUsableStr = "false";
    public string isEquippableStr = "false";
    public string isQuestItemStr = "false";
    
    // Parsed properties
    public ItemType ItemType { get; private set; }
    public ItemRarity ItemRarity { get; private set; }
    public ItemViewLogicType ViewLogicType { get; private set; }
    
    // Boolean properties parsed from strings
    public bool isUsable { get; private set; }
    public bool isEquippable { get; private set; }
    public bool isQuestItem { get; private set; }
    
    // Additional data
    public Dictionary<string, float> attributes;

    public ItemMasterData()
    {
        attributes = new Dictionary<string, float>();
    }

    /// <summary>
    /// Parses all enum and boolean values from their string representations
    /// </summary>
    public void ParseEnums()
    {
        // Parse ItemType
        if (!Enum.TryParse<ItemType>(this.category, true, out ItemType itemType))
        {
            Debug.LogWarning($"Invalid ItemType '{this.category}' for item '{this.itemId}', defaulting to Misc");
            this.ItemType = ItemType.Misc;
        }
        else
        {
            this.ItemType = itemType;
        }

        // Parse ItemRarity
        if (!Enum.TryParse<ItemRarity>(this.rarity, true, out ItemRarity itemRarity))
        {
            Debug.LogWarning($"Invalid ItemRarity '{this.rarity}' for item '{this.itemId}', defaulting to Common");
            this.ItemRarity = ItemRarity.Common;
        }
        else
        {
            this.ItemRarity = itemRarity;
        }

        // Parse ItemViewLogicType
        if (!Enum.TryParse<ItemViewLogicType>(this.viewLogic, true, out ItemViewLogicType viewLogicType))
        {
            Debug.LogWarning($"Invalid ViewLogicType '{this.viewLogic}' for item '{this.itemId}', defaulting to Static");
            this.ViewLogicType = ItemViewLogicType.Static;
        }
        else
        {
            this.ViewLogicType = viewLogicType;
        }
        
        // Parse boolean flags
        this.isUsable = ParseBool(isUsableStr, false);
        this.isEquippable = ParseBool(isEquippableStr, false);
        this.isQuestItem = ParseBool(isQuestItemStr, false) || this.ItemRarity == ItemRarity.Quest;
    }
    
    /// <summary>
    /// Helper method to parse boolean values from strings
    /// </summary>
    private bool ParseBool(string value, bool defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
            
        if (bool.TryParse(value.Trim().ToLower(), out bool result))
            return result;
            
        return value.Trim().ToLower() == "1" || 
               value.Trim().ToLower() == "yes" || 
               value.Trim().ToLower() == "true" || 
               value.Trim().ToLower() == "y";
    }
    
    /// <summary>
    /// Converts this ItemMasterData to an ItemData instance
    /// </summary>
    public ItemData ToItemData(Sprite icon = null)
    {
        var itemData = new ItemData(
            itemId,
            displayName,
            description, 
            value,
            weight,
            ItemType,
            icon,
            isUsable,
            isEquippable,
            isQuestItem
        );
        
        // Copy over attributes if any
        if (attributes != null && attributes.Count > 0)
        {
            foreach (var kvp in attributes)
            {
                itemData.attributes[kvp.Key] = kvp.Value;
            }
        }
        
        return itemData;
    }
}
#endregion