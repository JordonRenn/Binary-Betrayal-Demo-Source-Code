using UnityEngine;
using System.IO;

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
    public Inv_Player playerInventory { get; private set; } //player's inventory

    private IInventory connectedInventory; //inventory of the currently interacted NPC or container

    [Header("Developer Options")]
    [Space(10)]
    private const string JSON_DUMMY_INVENTORY_FILE_PATH = "Testing/DummyInventoryData.json";
    [SerializeField] private bool GenerateDummyInventory = false;
    [SerializeField] private bool GenerateEmptyInventory = false;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            // Initialize any default values if needed
        }
    }

    void Start ()
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

    /// <summary>
    /// Sets the player's inventory
    /// </summary>
    public void SetPlayerInventory(Inv_Player inventory)
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
            GameMaster.Instance?.gm_InventoryItemAdded?.Invoke();
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
            GameMaster.Instance?.gm_InventoryItemRemoved?.Invoke();
        }
    }

    public void GenerateDummyInventories()
    {
        var playerInventory = LoadDummyInventoryFromJSON();

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

    private static Inv_Player LoadDummyInventoryFromJSON()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, JSON_DUMMY_INVENTORY_FILE_PATH);

            Debug.Log($"Loading inventory from: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON file not found at: {filePath}");
                return null;
            }

            string jsonContent = File.ReadAllText(filePath);
            Debug.Log($"JSON file loaded, content length: {jsonContent.Length} characters");

            var inventoryData = JsonUtility.FromJson<InventoryData>(jsonContent);

            if (inventoryData?.playerInventory == null)
            {
                Debug.LogError("Invalid JSON structure - playerInventory not found");
                return null;
            }

            // Create the player inventory
            var playerInventory = new Inv_Player(
                inventoryData.playerInventory.inventoryId,
                inventoryData.playerInventory.name,
                inventoryData.playerInventory.capacity
            );

            // Add all items from JSON
            foreach (var itemData in inventoryData.playerInventory.items)
            {
                if (itemData != null)
                {
                    var item = new ItemData(
                        itemData.ItemId,
                        itemData.Name,
                        itemData.Description,
                        itemData.Icon,
                        itemData.Type,
                        itemData.weight
                    );
                    playerInventory.AddItem(item, 1); // Assuming quantity is 1 for each item
                }
                else
                {
                    SBGDebug.LogWarning("Found null item in DUMMY JSON DATA, skipping.", "InventoryManager");
                }
            }

            Debug.Log($"Loaded player inventory with {playerInventory.GetItems().Length} different item types");
            return playerInventory;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading inventory from JSON: {e.Message}");
            return null;
        }
    }
}

[System.Serializable]
public class InventoryData
{
    public PlayerInventoryData playerInventory;
}

[System.Serializable]
public class PlayerInventoryData
{
    public string inventoryId;
    public string name;
    public int capacity;
    public ItemData[] items;
}