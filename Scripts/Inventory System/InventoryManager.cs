using UnityEngine;
using System.IO;
using System;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(InventoryManager)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    //[Header("Player Inventory")]
    public IInventory playerInventory { get; private set; } //player's inventory
    public bool inventoryLoaded { get; private set; } = false;

    private IInventory connectedInventory; //inventory of the currently interacted NPC or container

    [Header("Developer Options")]
    [Space(10)]
    private const string JSON_INVENTORY_FILE_PATH = "Inventories/";
    private bool GenerateDummyInventory = true;
    private bool GenerateEmptyInventory = false;
    /* private bool testParsing = true; */

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            /* if (testParsing)
            {
                TestJsonParsing();
            } */
        }
    }

    /* private void TestJsonParsing()
    {
        var jsonAsset = new TextAsset("{\"inventoryId\":\"test\",\"items\":[]}");
        var result = InventoryJsonParser.ParseInventoryJsonData(jsonAsset);
        if (result == null || result.InventoryId != "test")
        {
            SBGDebug.LogError("SimdJson parser test failed!", "InventoryManager");
        }
        else
        {
            SBGDebug.LogInfo("SimdJson parser test succeeded.", "InventoryManager");
        }
    } */

    void Start()
    {
        if (GenerateDummyInventory)
        {
            GenerateDummyInventories();
        }
        else if (GenerateEmptyInventory)
        {
            SetPlayerInventory(new Inv_Player("PlayerInventory", "Player Inventory", 100));
            // Debug.Log("Generated empty player inventory.");

            inventoryLoaded = true; // I guess? might break some stuff.. idk
        }
        else
        {
            SBGDebug.LogWarning("HEY DUMBASS, YOU NEED TO LOAD A GOD DAMN INVENTORY FOR THE PLAYER, OR THEY WON'T HAVE ONE. FIX IT.", "InventoryManager | Start");
        }
    }

    #region Player Inventory
    /// <summary>
    /// Sets the player's inventory
    /// </summary>
    public void SetPlayerInventory(IInventory inventory)
    {
        playerInventory = null;
        playerInventory = inventory;
        // Debug.Log($"Player inventory set: {inventory.Name} with {inventory.GetItems().Length} unique item types");
        inventoryLoaded = true;
    }

    /// <summary>
    /// Gets the player's inventory
    /// </summary>
    public IInventory GetPlayerInventory()
    {
        return playerInventory;
    }

    /// <summary>
    /// Add item to player inventory
    /// </summary>
    public void AddItemToPlayer(IItem item, int quantity = 1)
    {
        if (playerInventory != null)
        {
            // Debug.Log($"Adding item to player inventory: {item.Name} x{quantity}");
            playerInventory.AddItem(item, quantity);
            // SBGDebug.LogInfo($"Item {item.Name} added to inventory. Total count: {quantity}", "InventoryManager");
        }
        else
        {
            SBGDebug.LogError($"Cannot add item {item.Name} - player inventory is null!", "InventoryManager | AddItemToPlayer");
        }
    }

    /// <summary>
    /// Remove item from player inventory
    /// </summary>
    public void RemoveItemFromPlayer(IItem item, int quantity = 1)
    {
        if (playerInventory != null)
        {
            playerInventory.RemoveItem(item, quantity);
            //GameMaster.Instance?.gm_ItemRemoved?.Invoke(InventoryType.Player, item.ItemId, item.Name);
        }
    }

    public void GenerateDummyInventories()
    {
        var playerInventory = LoadInventoryFromJSON("DummyInventoryData");

        if (playerInventory != null)
        {
            SetPlayerInventory(playerInventory);
            // Debug.Log("Dummy inventory loaded from JSON.");
        }
        else
        {
            SBGDebug.LogError("Failed to load dummy inventory from JSON.", "InventoryManager | GenerateDummyInventories");
        }
    }
    #endregion

    #region JSON Loading
    public static IInventory LoadInventoryFromJSON(string inventoryId)
    {
        try
        {
            // SBGDebug.LogInfo($"Begin attempt to load inventory from JSON: {inventoryId}", "InventoryManager | LoadInventoryFromJSON");

            string filePath = Path.Combine(Application.streamingAssetsPath, JSON_INVENTORY_FILE_PATH, $"{inventoryId}.json");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON file not found at: {filePath}");
                return null;
            }

            // Read file as bytes and create TextAsset for the parser
            byte[] fileBytes = File.ReadAllBytes(filePath);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                Debug.LogError($"Failed to read file or file is empty: {filePath}");
                return null;
            }

            TextAsset jsonAsset = new TextAsset(System.Text.Encoding.UTF8.GetString(fileBytes));
            if (jsonAsset == null)
            {
                Debug.LogError($"Failed to create TextAsset from file bytes: {filePath}");
                return null;
            }

            // Use our SimdJson parser
            var inventoryContextData = InventoryJsonParser.ParseInventoryJsonData(jsonAsset);
            if (inventoryContextData == null)
            {
                Debug.LogError($"Failed to parse inventory data for {inventoryId}");
                return null;
            }

            var loadedContextInventory = InventoryFactory.CreateInventoryDataFromContext(inventoryContextData);
            if (loadedContextInventory == null)
            {
                Debug.LogError($"InventoryFactory.CreateInventoryDataFromContext returned null for {inventoryId}");
                return null;
            }

            // SBGDebug.LogInfo($"Successfully loaded inventory: {inventoryContextData.InventoryId} with {inventoryContextData.Items?.Count ?? 0} items", "InventoryManager | LoadInventoryFromJSON");

            var loadedInventory = InventoryFactory.CreateInventory(loadedContextInventory);
            if (loadedInventory == null)
            {
                Debug.LogError($"InventoryFactory.CreateInventory returned null for {inventoryId}");
                return null;
            }

            return loadedInventory;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading inventory from JSON: {e.Message}");
            return null;
        }
    }
    #endregion

    public static InventoryData ConvertContextToInventoryData(InventoryContextData context)
    {
        var inventoryData = new InventoryData
        {
            inventoryId = context.InventoryId,
            // Set other fields as needed
        };

        // Convert ItemContextData to IItem and add to inventoryData.items
        if (context.Items != null)
        {
            foreach (var ctxItem in context.Items)
            {
                var item = ItemFactory.GetItemFromDatabase(ctxItem.ItemId);
                if (item != null)
                {
                    for (int i = 0; i < ctxItem.Quantity; i++)
                    {
                        inventoryData.items.Add(item);
                    }
                }
            }
        }

        return inventoryData;
    }

    public void ConnectInventory(IInventory inventory)
    {
        connectedInventory = inventory;
        // Debug.Log($"Connected to inventory: {inventory.Name}");
        //GameMaster.Instance?.gm_InventoryConnected?.Invoke();
    }
}