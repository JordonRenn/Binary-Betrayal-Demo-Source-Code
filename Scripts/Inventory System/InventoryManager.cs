using UnityEngine;
using System.IO;

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

    private IInventory connectedInventory; //inventory of the currently interacted NPC or container

    [Header("Developer Options")]
    [Space(10)]
    private const string JSON_INVENTORY_FILE_PATH = "Inventories/";
    [SerializeField] private bool GenerateDummyInventory = false;
    [SerializeField] private bool GenerateEmptyInventory = false;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            // Initialize any default values if needed
        }
    }

    void Start()
    {
        if (GenerateDummyInventory)
        {
            GenerateDummyInventories();
        }
        else if (GenerateEmptyInventory)
        {
            SetPlayerInventory(new Inv_Player("PlayerInventory", "Player Inventory", 100));
            Debug.Log("Generated empty player inventory.");
        }
    }

    #region Player Inventory
    /// <summary>
    /// Sets the player's inventory
    /// </summary>
    public void SetPlayerInventory(IInventory inventory)
    {
        // Clear any existing inventory reference
        playerInventory = null;

        // Set the new inventory
        playerInventory = inventory;
        Debug.Log($"Player inventory set: {inventory.Name} with {inventory.GetItems().Length} unique item types");
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
            Debug.Log($"Adding item to player inventory: {item.Name} x{quantity}");
            playerInventory.AddItem(item, quantity);
            SBGDebug.LogInfo($"Item {item.Name} added to inventory. Total count: {quantity}", "InventoryManager");
            //GameMaster.Instance?.gm_ItemAdded?.Invoke(InventoryType.Player, item.ItemId, item.Name);
        }
        else
        {
            Debug.LogError($"Cannot add item {item.Name} - player inventory is null!");
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
            Debug.Log("Dummy inventory loaded from JSON.");
        }
        else
        {
            Debug.LogError("Failed to load dummy inventory from JSON.");
        }
    }
    #endregion

    #region JSON Loading
    public static IInventory LoadInventoryFromJSON(string inventoryId)
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, JSON_INVENTORY_FILE_PATH, $"{inventoryId}.json");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON file not found at: {filePath}");
                return null;
            }

            string jsonContent = File.ReadAllText(filePath);

            var inventoryData = JsonUtility.FromJson<InventoryData>(jsonContent);

            // Create the player inventory
            var loadedInventory = InventoryFactory.CreateInventory(inventoryData);

            return loadedInventory;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading inventory from JSON: {e.Message}");
            return null;
        }
    }
    #endregion

    public void ConnectInventory(IInventory inventory)
    {
        connectedInventory = inventory;
        Debug.Log($"Connected to inventory: {inventory.Name}");
        //GameMaster.Instance?.gm_InventoryConnected?.Invoke();
    }
}