using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class DummyInventoryGenerator
{
    private static Sprite defaultIcon = null; // We'll use null for testing purposes
    private const string JSON_FILE_PATH = "Testing/DummyInventoryData.json";

    public static void GenerateDummyInventories()
    {
        Debug.Log("=== GENERATING FRESH DUMMY INVENTORY FROM JSON ===");
        
        var playerInventory = LoadPlayerInventoryFromJSON();
        
        if (playerInventory != null)
        {
            // Clear any existing inventory and store the new one
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SetPlayerInventory(playerInventory);
                Debug.Log("=== DUMMY INVENTORY SUCCESSFULLY LOADED AND SET ===");
            }
            else
            {
                Debug.LogError("InventoryManager.Instance is null - cannot set player inventory!");
            }
        }
        else
        {
            Debug.LogError("Failed to load player inventory from JSON!");
        }
    }

    private static IInventory LoadPlayerInventoryFromJSON()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, JSON_FILE_PATH);
            
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
                // Parse the item type from string
                if (System.Enum.TryParse<ItemType>(itemData.type, out ItemType itemType))
                {
                    var item = new Item(
                        itemData.itemId,
                        itemData.name,
                        itemData.description,
                        defaultIcon, // Using null for now, can be enhanced later
                        itemType,
                        itemData.weight
                    );

                    playerInventory.AddItem(item, itemData.quantity);
                }
                else
                {
                    Debug.LogWarning($"Unknown item type: {itemData.type} for item {itemData.name}");
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

    // Data structures for JSON parsing
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

    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public string name;
        public string description;
        public string type;
        public int weight;
        public int quantity;
    }
}
