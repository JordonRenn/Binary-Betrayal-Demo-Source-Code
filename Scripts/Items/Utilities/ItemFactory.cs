using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region ItemFactory
// rename to "ItemFactory" when complete to replace old class
public static class ItemFactory
{
    private const string FILE_ITEM_MASTER_CSV = "ItemMaster.csv";
    private const string PATH_ICON_FOLDER = "ItemIcons/"; // in streaming assets folder / iconId = png file name / default ID = "item"
    public static bool databaseLoaded { get; private set; } = false;
    private static Dictionary<string, IItem> itemDatabase = new Dictionary<string, IItem>(); // items by ID

    #region DataBase
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadItemDatabase()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, FILE_ITEM_MASTER_CSV);
            if (!File.Exists(filePath))
            {
                SBGDebug.LogError($"ItemMaster CSV file not found at: {filePath}", "ItemFactory");
                return;
            }
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                SBGDebug.LogError("ItemMaster CSV file is empty or has no data rows", "ItemFactory");
                return;
            }
            itemDatabase.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                try
                {
                    IItem item = ParseCsvLineToIItem(line);
                    if (item != null && !string.IsNullOrEmpty(item.ItemId))
                    {
                        itemDatabase[item.ItemId] = item;
                    }
                }
                catch (Exception ex)
                {
                    SBGDebug.LogError($"Error parsing line {i + 1}: {line}. Error: {ex.Message}", "ItemFactory | LoadItemDatabase");
                }
            }
            databaseLoaded = true;
            // SBGDebug.LogInfo($"Loaded {itemDatabase.Count} items from ItemMaster.csv", "ItemFactory | LoadItemDatabase");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to load ItemMaster.csv: {ex.Message}", "ItemFactory | LoadItemDatabase");
        }
    }
    #endregion

    #region Helper Methods
    private static IItem ParseCsvLineToIItem(string line)
    {
        string[] values = SplitCsvLine(line);
        if (values.Length < 11)
            return null;
        string itemId = values[0].Trim();
        string name = values[1].Trim();
        string description = values[2].Trim();
        float weight = float.TryParse(values[3].Trim(), out float w) ? w : 0;
        ItemType itemType = ParseItemType(values[4].Trim());
        string subType = values[5].Trim();
        string effectType = values[6].Trim();
        float effectValue = float.TryParse(values[7].Trim(), out float ev) ? ev : 0;
        ItemRarity itemRarity = ParseItemRarity(values[8].Trim());
        ItemViewLogicType viewLogicType = ParseItemViewLogicType(values[9].Trim());
        string iconId = values[10].Trim();

        switch (itemType)
        {
            case ItemType.Misc:
                return new Item_Misc(itemId, name, description, LoadSprite(iconId), weight, itemRarity, viewLogicType);
            case ItemType.Material:
                return new Item_Material(itemId, name, description, LoadSprite(iconId), ParseMaterialType(subType), ParseMaterialEffect(effectType), (int)effectValue, weight, itemRarity, viewLogicType);
            case ItemType.Food:
                return new Item_Food(itemId, name, description, LoadSprite(iconId), ParseFoodType(subType), ParseFoodEffect(effectType), (int)effectValue, weight, itemRarity, viewLogicType);
            case ItemType.Keys:
                return new Item_Keys(itemId, name, description, LoadSprite(iconId), ParseKeyType(subType), ParseKeyEffect(effectType), weight, itemRarity, viewLogicType);
            case ItemType.Quest:
                return new Item_Quest(itemId, name, description, LoadSprite(iconId), ParseQuestType(subType), itemId, ParseQuestEffect(effectType), weight, itemRarity, viewLogicType);
            case ItemType.Medical:
                return new Item_Medical(itemId, name, description, LoadSprite(iconId), ParseMedicalType(subType), ParseMedicalEffect(effectType), (int)effectValue, weight, itemRarity, viewLogicType);
            case ItemType.Phone:
                return new Item_Phone(itemId, name, description, LoadSprite(iconId), ParsePhoneType(subType), effectType, ParsePhoneEffect(effectType), weight, itemRarity, viewLogicType);
            case ItemType.Tools:
                return new Item_Tools(itemId, name, description, LoadSprite(iconId), ParseToolType(subType), ParseToolEffect(effectType), weight, itemRarity, viewLogicType);
            default:
                SBGDebug.LogError($"Unknown item type: {itemType}", "ItemFactory | ParseCsvLineToIItem");
                return null;
        }
    }

    private static Sprite LoadSprite(string iconId)
    {
        var path = Application.streamingAssetsPath + "/" + PATH_ICON_FOLDER + iconId + ".png";
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            path = Application.streamingAssetsPath + "/" + PATH_ICON_FOLDER + "item.png";
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    private static ItemType ParseItemType(string value)
    {
        if (Enum.TryParse<ItemType>(value, true, out ItemType itemType))
        {
            return itemType;
        }
        Debug.LogWarning($"Invalid ItemType '{value}', defaulting to Misc");
        return ItemType.Misc;
    }

    private static ItemRarity ParseItemRarity(string value)
    {
        if (Enum.TryParse<ItemRarity>(value, true, out ItemRarity itemRarity))
        {
            return itemRarity;
        }
        Debug.LogWarning($"Invalid ItemRarity '{value}', defaulting to Common");
        return ItemRarity.Common;
    }

    private static ItemViewLogicType ParseItemViewLogicType(string value)
    {
        if (Enum.TryParse<ItemViewLogicType>(value, true, out ItemViewLogicType viewLogicType))
        {
            return viewLogicType;
        }
        Debug.LogWarning($"Invalid ViewLogicType '{value}', defaulting to Static");
        return ItemViewLogicType.Static;
    }
    #endregion

    #region Item Creation
    public static IItem GetItemFromDatabase(string itemId)
    {
        if (!databaseLoaded)
        {
            SBGDebug.LogError("Item database not loaded. Call LoadItemDatabase() first.", "ItemFactory");
            return null;
        }
        if (!itemDatabase.ContainsKey(itemId))
        {
            SBGDebug.LogError($"Item '{itemId}' not found in database.", "ItemFactory");
            return null;
        }

    var item = itemDatabase[itemId];
    // If items are mutable, consider returning a deep copy here
    return item;
    }

    // Helper methods to parse subtypes and effects for each item type
    private static Item_MaterialType ParseMaterialType(string value)
    {
        Enum.TryParse<Item_MaterialType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Material ParseMaterialEffect(string value)
    {
        Enum.TryParse<ItemEffect_Material>(value, true, out var result);
        return result;
    }
    private static Item_FoodType ParseFoodType(string value)
    {
        Enum.TryParse<Item_FoodType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Food ParseFoodEffect(string value)
    {
        Enum.TryParse<ItemEffect_Food>(value, true, out var result);
        return result;
    }
    private static KeyType ParseKeyType(string value)
    {
        Enum.TryParse<KeyType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Key ParseKeyEffect(string value)
    {
        Enum.TryParse<ItemEffect_Key>(value, true, out var result);
        return result;
    }
    private static Item_QuestType ParseQuestType(string value)
    {
        Enum.TryParse<Item_QuestType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Quest ParseQuestEffect(string value)
    {
        Enum.TryParse<ItemEffect_Quest>(value, true, out var result);
        return result;
    }
    private static Item_MedicalType ParseMedicalType(string value)
    {
        Enum.TryParse<Item_MedicalType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Medical ParseMedicalEffect(string value)
    {
        Enum.TryParse<ItemEffect_Medical>(value, true, out var result);
        return result;
    }
    private static Item_PhoneType ParsePhoneType(string value)
    {
        Enum.TryParse<Item_PhoneType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Phone ParsePhoneEffect(string value)
    {
        Enum.TryParse<ItemEffect_Phone>(value, true, out var result);
        return result;
    }
    private static Item_ToolType ParseToolType(string value)
    {
        Enum.TryParse<Item_ToolType>(value, true, out var result);
        return result;
    }
    private static ItemEffect_Tool ParseToolEffect(string value)
    {
        Enum.TryParse<ItemEffect_Tool>(value, true, out var result);
        return result;
    }
    #endregion
}
#endregion