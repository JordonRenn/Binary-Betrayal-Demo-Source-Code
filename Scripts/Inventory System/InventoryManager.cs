using UnityEngine;

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

    [Header("Player Inventory")]
    private IInventory playerInventory;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance) == this)
        {
            // Initialize any default values if needed
        }
    }

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
    /// Check if player has a specific item
    /// </summary>
    public bool PlayerHasItem(IItem item, int quantity = 1)
    {
        return playerInventory?.HasItem(item, quantity) ?? false;
    }

    /// <summary>
    /// Add item to player inventory
    /// </summary>
    public void AddItemToPlayer(IItem item, int quantity = 1)
    {
        if (playerInventory != null)
        {
            playerInventory.AddItem(item, quantity);
            GameMaster.Instance?.gm_InventoryItemAdded?.Invoke();
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
}
