using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;

#region Inventory Menu
public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private GameObject inventoryCanvasGameObject;

    [Header("Panel References")]
    [SerializeField] private GameObject itemTypePanel;     // Left panel
    [SerializeField] private GameObject itemListPanel;     // Middle panel
    [SerializeField] private GameObject itemDetailsPanel;  // Right panel

    [Header("Type List")]
    [SerializeField] private Transform typeListContent;    // Content transform for type buttons
    [SerializeField] private GameObject typeButtonPrefab;  // Prefab for type buttons

    [Header("Item List")]
    [SerializeField] private Transform itemListContent;    // Content transform for item buttons
    [SerializeField] private GameObject itemButtonPrefab;  // Prefab for item buttons

    [Header("Item Details")]
    //[SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemWeightText;

    private IInventory currentInventory;
    private Dictionary<ItemType, List<IItem>> itemsByType = new Dictionary<ItemType, List<IItem>>();
    private ItemType currentType;
    private IItem currentItem;

    public bool isOpen { get; private set; }
    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    #region Initialization
    private void Start()
    {
        // Subscribe to inventory change events if you have any
        SetupLayoutGroups();
        inventoryCanvasGameObject.SetActive(false);
    }

    private void SetupLayoutGroups()
    {
        // Ensure the content areas have proper layout groups
        SetupVerticalLayoutGroup(typeListContent);
        SetupVerticalLayoutGroup(itemListContent);
    }

    private void SetupVerticalLayoutGroup(Transform content)
    {
        if (content == null) return;

        // Add VerticalLayoutGroup if it doesn't exist
        var layoutGroup = content.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        // Configure the layout group
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = 5f;

        // Set padding
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);

        // Add ContentSizeFitter if it doesn't exist
        var sizeFitter = content.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();
        }

        // Configure the size fitter
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void Initialize(IInventory inventory)
    {
        currentInventory = inventory;
        Debug.Log($"InventoryMenu initialized with inventory: {(inventory != null ? inventory.Name : "null")}");
        if (inventory != null)
        {
            Debug.Log($"Inventory contains {inventory.GetItems().Length} unique item types");
        }
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (currentInventory == null)
        {
            Debug.LogWarning("Current inventory is null, cannot refresh.");
            return;
        }

        Debug.Log($"Refreshing inventory: {currentInventory.Name}");

        // Clear existing items
        itemsByType.Clear();

        // Group items by type
        foreach (var item in currentInventory.GetItems())
        {
            if (!itemsByType.ContainsKey(item.Type))
            {
                itemsByType[item.Type] = new List<IItem>();
            }
            itemsByType[item.Type].Add(item);
        }

        Debug.Log($"Grouped items into {itemsByType.Count} categories");
        foreach (var kvp in itemsByType)
        {
            Debug.Log($"Category {kvp.Key}: {kvp.Value.Count} items");
        }

        // Refresh UI
        RefreshItemTypeList();

        // Select "All Items" by default
        SelectItemType(null);
    }
    #endregion

    #region Type List Mgmt
    private void RefreshItemTypeList()
    {
        // Clear existing type buttons
        foreach (Transform child in typeListContent)
        {
            Destroy(child.gameObject);
        }

        // Add "All Items" button
        CreateItemTypeButton("All Items", null);

        // Add buttons for each type that has items
        foreach (var type in itemsByType.Keys)
        {
            CreateItemTypeButton(type.ToString(), type);
        }
    }

    private void CreateItemTypeButton(string typeName, ItemType? type)
    {
        if (typeListContent == null)
        {
            Debug.LogError("typeListContent is null! Make sure it's assigned in the inspector.");
            return;
        }

        if (typeButtonPrefab == null)
        {
            Debug.LogError("typeButtonPrefab is null! Make sure it's assigned in the inspector.");
            return;
        }

        var buttonObj = Instantiate(typeButtonPrefab, typeListContent);
        var button = buttonObj.GetComponent<Button>();
        var typeButtonUI = buttonObj.GetComponent<TypeButtonUI>();

        if (button == null)
        {
            Debug.LogError("Type button prefab doesn't have a Button component!");
            return;
        }

        // Check if the prefab has TypeButtonUI component
        if (typeButtonUI == null)
        {
            Debug.LogWarning("Type button prefab doesn't have TypeButtonUI component! Adding one...");
            typeButtonUI = buttonObj.AddComponent<TypeButtonUI>();
        }

        // Use TypeButtonUI to set the type name
        typeButtonUI.SetType(typeName);

        button.onClick.AddListener(() => SelectItemType(type));

        // Store the type with the button for reference
        buttonObj.AddComponent<TypeButtonData>().Type = type;

        Debug.Log($"Created type button: {typeName}");
    }

    private void SelectItemType(ItemType? type)
    {
        currentType = type ?? ItemType.Misc; // Default to Weapon if null (All Items)

        // Update type button highlighting
        UpdateItemTypeButtonHighlighting(type);

        // Refresh item list
        RefreshItemList(type);
    }

    private void UpdateItemTypeButtonHighlighting(ItemType? selectedType)
    {
        foreach (Transform child in typeListContent)
        {
            var buttonData = child.GetComponent<TypeButtonData>();
            var typeButtonUI = child.GetComponent<TypeButtonUI>();

            if (buttonData != null && typeButtonUI != null)
            {
                bool isSelected = (buttonData.Type == selectedType);
                typeButtonUI.SetHighlighted(isSelected);
            }
        }
    }
    #endregion

    #region Item List Mgmt
    private void RefreshItemList(ItemType? type)
    {
        // Clear existing item buttons
        foreach (Transform child in itemListContent)
        {
            Destroy(child.gameObject);
        }

        // Get items to display
        IEnumerable<IItem> itemsToShow;
        if (type == null)
        {
            // Show all items
            itemsToShow = itemsByType.Values.SelectMany(x => x);
        }
        else
        {
            // Show items of selected type
            itemsToShow = itemsByType.ContainsKey(type.Value) ? itemsByType[type.Value] : Enumerable.Empty<IItem>();
        }

        // Create buttons for items
        foreach (var item in itemsToShow)
        {
            CreateItemButton(item);
        }

        // Select first item by default
        var firstItem = itemsToShow.FirstOrDefault();
        if (firstItem != null)
        {
            SelectItem(firstItem);
        }
        else
        {
            ClearItemDetails();
        }
    }

    private void CreateItemButton(IItem item)
    {
        if (itemListContent == null)
        {
            Debug.LogError("itemListContent is null! Make sure it's assigned in the inspector.");
            return;
        }

        if (itemButtonPrefab == null)
        {
            Debug.LogError("itemButtonPrefab is null! Make sure it's assigned in the inspector.");
            return;
        }

        var buttonObj = Instantiate(itemButtonPrefab, itemListContent);
        var button = buttonObj.GetComponent<Button>();
        var itemButtonUI = buttonObj.GetComponent<ItemButtonUI>();

        if (button == null)
        {
            Debug.LogError("Item button prefab doesn't have a Button component!");
            return;
        }

        // Check if the prefab has ItemButtonUI component
        if (itemButtonUI == null)
        {
            Debug.LogWarning("Item button prefab doesn't have ItemButtonUI component! Adding one...");
            itemButtonUI = buttonObj.AddComponent<ItemButtonUI>();
        }

        // Use ItemButtonUI to set the item
        itemButtonUI.SetItem(item);

        button.onClick.AddListener(() => SelectItem(item));

        // Store the item with the button for reference
        buttonObj.AddComponent<ItemButtonData>().Item = item;

        Debug.Log($"Created item button: {item.Name}");
    }

    private void SelectItem(IItem item)
    {
        currentItem = item;

        // Update item button highlighting
        UpdateItemButtonHighlighting(item);

        // Update details panel
        UpdateItemDetails(item);
    }

    private void UpdateItemButtonHighlighting(IItem selectedItem)
    {
        foreach (Transform child in itemListContent)
        {
            var buttonData = child.GetComponent<ItemButtonData>();
            var itemButtonUI = child.GetComponent<ItemButtonUI>();

            if (buttonData != null && itemButtonUI != null)
            {
                bool isSelected = (buttonData.Item == selectedItem);
                itemButtonUI.SetHighlighted(isSelected);
            }
        }
    }

    private void UpdateItemDetails(IItem item)
    {
        if (item == null)
        {
            ClearItemDetails();
            return;
        }

        //itemIcon.sprite = item.Icon;
        itemNameText.text = item.Name;
        itemDescriptionText.text = item.Description;
        itemTypeText.text = $"Type: {item.Type}";
        itemWeightText.text = $"Weight: {item.Weight}";
    }

    private void ClearItemDetails()
    {
        //itemIcon.sprite = null;
        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemTypeText.text = "";
        itemWeightText.text = "";
    }

    #region Public Methods
    public void ShowInventory()
    {
        inventoryCanvasGameObject.SetActive(true);
        OnOpen?.Invoke();
        isOpen = true;
    }

    public void HideInventory()
    {
        inventoryCanvasGameObject.SetActive(false);
        OnClose?.Invoke();
        isOpen = false;
    }
    #endregion
}
#endregion

#region Helper Components
// Helper component to store type data with buttons
public class TypeButtonData : MonoBehaviour
{
    public ItemType? Type { get; set; }
}

// Helper component to store item data with buttons
public class ItemButtonData : MonoBehaviour
{
    public IItem Item { get; set; }
}


#endregion
#endregion