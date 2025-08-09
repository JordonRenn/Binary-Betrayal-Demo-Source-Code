using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* 
INHERITANCE STRUCTURE:
IItem
├── ItemBase (abstract class)
│   ├── Item Type Classes (e.g., Item_Misc, Item_Material, Item_Food, Item_Keys, Item_Quest)
│   └── ItemData (concrete implementation of IItem)
 */

/* 
HOW TO USE ItemFactory:
   RECOMMENDED:
   1. Use ItemFactory.CreateItemFromDatabase(itemId) - handles subtypes automatically from CSV
   2. For specific subtypes when needed, use specialized methods (CreateKeyItem, CreateMaterialItem, etc.)

   LEGACY SUPPORT:
   3. For basic items, create ItemData and call ItemFactory.CreateItem(itemData) 
      (uses default subtypes, only use when CSV is not appropriate)
 */

#region ItemMasterData
[System.Serializable]
public class ItemMasterData
{
    public string itemId;
    public string displayName;
    public string description;
    public string category;
    public string subtype;
    public string rarity;
    public string viewLogic;
    public int value;

    // Parsed enum properties
    public ItemType ItemType { get; private set; }
    public ItemRarity ItemRarity { get; private set; }
    public ItemViewLogicType ViewLogicType { get; private set; }

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
    }
}
#endregion

#region Item Factory Class
public class ItemFactory : MonoBehaviour
{
    [Header("Database Settings")]
    [SerializeField] private string csvFileName = "ItemMaster.csv";

    private static ItemFactory _instance;
    public static ItemFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemFactory>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ItemFactory");
                    _instance = go.AddComponent<ItemFactory>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, ItemMasterData> itemDatabase = new Dictionary<string, ItemMasterData>();
    private bool isLoaded = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItemDatabase();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads the item database from the CSV file
    /// </summary>
    public void LoadItemDatabase()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);

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

            // Skip header line and comments
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                try
                {
                    ItemMasterData itemData = ParseCsvLine(line);
                    if (itemData != null && !string.IsNullOrEmpty(itemData.itemId))
                    {
                        itemData.ParseEnums();
                        itemDatabase[itemData.itemId] = itemData;
                    }
                }
                catch (Exception ex)
                {
                    SBGDebug.LogError($"Error parsing line {i + 1}: {line}. Error: {ex.Message}", "ItemFactory");
                }
            }

            isLoaded = true;
            SBGDebug.LogInfo($"Loaded {itemDatabase.Count} items from ItemMaster.csv", "ItemFactory");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to load ItemMaster.csv: {ex.Message}", "ItemFactory");
        }
    }

    /// <summary>
    /// Parses a single CSV line into ItemMasterData
    /// </summary>
    private ItemMasterData ParseCsvLine(string line)
    {
        string[] values = SplitCsvLine(line);

        if (values.Length < 8)
        {
            SBGDebug.LogWarning($"CSV line has insufficient columns: {line}", "ItemFactory");
            return null;
        }

        ItemMasterData item = new ItemMasterData
        {
            itemId = values[0].Trim(),
            displayName = values[1].Trim(),
            description = values[2].Trim(),
            category = values[3].Trim(),
            subtype = values[4].Trim(),
            rarity = values[5].Trim(),
            viewLogic = values[6].Trim(),
            value = int.TryParse(values[7].Trim(), out int val) ? val : 0
        };

        return item;
    }

    /// <summary>
    /// Splits a CSV line handling commas within quotes
    /// </summary>
    private string[] SplitCsvLine(string line)
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

    #region Public Database API

    /// <summary>
    /// Checks if an item exists in the database
    /// </summary>
    public static bool ItemExists(string itemId)
    {
        return Instance.itemDatabase.ContainsKey(itemId);
    }

    /// <summary>
    /// Gets item master data by ID
    /// </summary>
    public static ItemMasterData GetItemData(string itemId)
    {
        return Instance.itemDatabase.GetValueOrDefault(itemId);
    }

    /// <summary>
    /// Gets all items of a specific type
    /// </summary>
    public static List<ItemMasterData> GetItemsByType(ItemType itemType)
    {
        List<ItemMasterData> items = new List<ItemMasterData>();

        foreach (var item in Instance.itemDatabase.Values)
        {
            if (item.ItemType == itemType)
            {
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// Gets all items that are quest items
    /// </summary>
    public static List<ItemMasterData> GetQuestItems()
    {
        List<ItemMasterData> items = new List<ItemMasterData>();

        foreach (var item in Instance.itemDatabase.Values)
        {
            if (item.ItemRarity == ItemRarity.Quest)
            {
                items.Add(item);
            }
        }

        return items;
    }

    #endregion

    #region Item Creation API

    /// <summary>
    /// Creates an item from the database (RECOMMENDED METHOD)
    /// This is the primary way to create items as it handles subtypes correctly from CSV data
    /// </summary>
    public static IItem CreateItemFromDatabase(string itemId, Sprite icon = null)
    {
        if (!Instance.isLoaded)
        {
            SBGDebug.LogError("ItemFactory database not loaded yet", "ItemFactory");
            return null;
        }

        if (!ItemExists(itemId))
        {
            SBGDebug.LogError($"Item '{itemId}' not found in database", "ItemFactory");
            return null;
        }

        ItemMasterData data = GetItemData(itemId);

        try
        {
            return data.ItemType switch
            {
                ItemType.Misc => CreateItem(CreateItemData(data, icon)),
                ItemType.Material => CreateMaterialFromData(data, icon),
                ItemType.Food => CreateFoodFromData(data, icon),
                ItemType.Keys => CreateKeyFromData(data, icon),
                ItemType.Quest => CreateQuestFromData(data, icon),
                ItemType.Medical => CreateMedicalFromData(data, icon),
                ItemType.Phone => CreatePhoneFromData(data, icon),
                ItemType.Tools => CreateToolFromData(data, icon),
                _ => null
            };
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create item '{itemId}': {ex.Message}", "ItemFactory");
            return null;
        }
    }

    /// <summary>
    /// Creates an item instance from ItemData. 
    /// NOTE: This method uses default subtypes for specialized items. 
    /// For proper subtype handling, use CreateItemFromDatabase() instead.
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
                ItemType.Misc => ValidateAndCreate(() => new Item_Misc(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null), "Misc"),
                ItemType.Material => ValidateAndCreate(() => new Item_Material(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_MaterialType.MetalScraps), "Material"),
                ItemType.Food => ValidateAndCreate(() => new Item_Food(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_FoodType.Snack), "Food"),
                ItemType.Keys => ValidateAndCreate(() => new Item_Keys(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, KeyType.Key), "Keys"),
                ItemType.Quest => ValidateAndCreate(() => new Item_Quest(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_QuestType.Main, string.Empty), "Quest"),
                ItemType.Medical => ValidateAndCreate(() => new Item_Medical(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_MedicalType.FirstAidKit), "Medical"),
                ItemType.Phone => ValidateAndCreate(() => new Item_Phone(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_PhoneType.Number, string.Empty), "Phone"),
                ItemType.Tools => ValidateAndCreate(() => new Item_Tools(itemData.ItemId, itemData.Name, itemData.Description, itemData.Icon, null, Item_ToolType.KeyJammer), "Tools"),
                _ => throw new ArgumentOutOfRangeException($"Unsupported item type: {itemData.Type}")
            };
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    #endregion

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

    /// <summary>
    /// Creates a material item with specified material type
    /// </summary>
    public static IItem CreateMaterialItem(string id, string name, string description, Sprite icon, Item_MaterialType materialType)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Material(id, name, description, icon, null, materialType), "Material");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create material item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    /// <summary>
    /// Creates a food item with specified food type
    /// </summary>
    public static IItem CreateFoodItem(string id, string name, string description, Sprite icon, Item_FoodType foodType)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Food(id, name, description, icon, null, foodType), "Food");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create food item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    /// <summary>
    /// Creates a medical item with specified medical type
    /// </summary>
    public static IItem CreateMedicalItem(string id, string name, string description, Sprite icon, Item_MedicalType medicalType)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Medical(id, name, description, icon, null, medicalType), "Medical");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create medical item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    /// <summary>
    /// Creates a tool item with specified tool type
    /// </summary>
    public static IItem CreateToolItem(string id, string name, string description, Sprite icon, Item_ToolType toolType)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Tools(id, name, description, icon, null, toolType), "Tools");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create tool item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    public static IItem CreateQuestItem(string id, string name, string description, Sprite icon, Item_QuestType questType, string questId)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Quest(id, name, description, icon, null, questType, questId), "Quest");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create quest item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    public static IItem CreatePhoneItem(string id, string name, string description, Sprite icon, Item_PhoneType phoneType, string phoneNumber)
    {
        try
        {
            return ValidateAndCreate(() => new Item_Phone(id, name, description, icon, null, phoneType, phoneNumber), "Phone");
        }
        catch (Exception ex)
        {
            SBGDebug.LogError($"Failed to create phone item: {ex.Message}", "ItemFactory");
            return null;
        }
    }

    #endregion

    #region Private Helper Methods

    private static ItemData CreateItemData(ItemMasterData data, Sprite icon)
    {
        return new ItemData(
            data.itemId,
            data.displayName,
            data.description,
            icon,
            data.ItemType,
            0, // You might want to add weight to your CSV
            data.ItemRarity,
            data.ViewLogicType
        );
    }

    private static IItem CreateMaterialFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_MaterialType>(data.subtype, out Item_MaterialType materialType))
        {
            return CreateMaterialItem(data.itemId, data.displayName, data.description, icon, materialType);
        }
        SBGDebug.LogWarning($"Unknown material subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreateFoodFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_FoodType>(data.subtype, out Item_FoodType foodType))
        {
            return CreateFoodItem(data.itemId, data.displayName, data.description, icon, foodType);
        }
        SBGDebug.LogWarning($"Unknown food subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreateKeyFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<KeyType>(data.subtype, out KeyType keyType))
        {
            return CreateKeyItem(data.itemId, data.displayName, data.description, icon, keyType);
        }
        SBGDebug.LogWarning($"Unknown key subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreateQuestFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_QuestType>(data.subtype, out Item_QuestType questType))
        {
            return CreateQuestItem(data.itemId, data.displayName, data.description, icon, questType, "");
        }
        SBGDebug.LogWarning($"Unknown quest subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreateMedicalFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_MedicalType>(data.subtype, out Item_MedicalType medicalType))
        {
            return CreateMedicalItem(data.itemId, data.displayName, data.description, icon, medicalType);
        }
        SBGDebug.LogWarning($"Unknown medical subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreatePhoneFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_PhoneType>(data.subtype, out Item_PhoneType phoneType))
        {
            return CreatePhoneItem(data.itemId, data.displayName, data.description, icon, phoneType, "");
        }
        SBGDebug.LogWarning($"Unknown phone subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

    private static IItem CreateToolFromData(ItemMasterData data, Sprite icon)
    {
        if (Enum.TryParse<Item_ToolType>(data.subtype, out Item_ToolType toolType))
        {
            return CreateToolItem(data.itemId, data.displayName, data.description, icon, toolType);
        }
        SBGDebug.LogWarning($"Unknown tool subtype '{data.subtype}' for item '{data.itemId}'", "ItemFactory");
        return null;
    }

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
#endregion