using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;

#region Inventory Menu
public class _InventoryMenu : MonoBehaviour
{
    private UIDocument document;

    private MultiColumnListView listViewFavorites;
    private MultiColumnListView listViewMisc;
    private MultiColumnListView listViewMaterials;
    private MultiColumnListView listViewMedical;
    private MultiColumnListView listViewFood;
    private MultiColumnListView listViewKeys;
    private MultiColumnListView listViewDocuments;
    private MultiColumnListView listViewPhone;
    private MultiColumnListView listViewTools;

    private Dictionary<ItemType, List<InventoryListItem>> itemsByType = new Dictionary<ItemType, List<InventoryListItem>>();
    private List<InventoryListItem> favorites = new List<InventoryListItem>();

    #region Column Generation
    private void Awake()
    {
        document = GetComponent<UIDocument>();

        // Initialize lists for each item type
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            itemsByType[type] = new List<InventoryListItem>();
        }

        var root = document.rootVisualElement;

        listViewFavorites = root.Q<MultiColumnListView>("Favorites-ListView");
        listViewMisc = root.Q<MultiColumnListView>("Misc-ListView");
        listViewMaterials = root.Q<MultiColumnListView>("Materials-ListView");
        listViewMedical = root.Q<MultiColumnListView>("Medical-ListView");
        listViewFood = root.Q<MultiColumnListView>("Food-ListView");
        listViewKeys = root.Q<MultiColumnListView>("Keys-ListView");
        listViewDocuments = root.Q<MultiColumnListView>("Documents-ListView");
        listViewPhone = root.Q<MultiColumnListView>("Phone-ListView");
        listViewTools = root.Q<MultiColumnListView>("Tools-ListView");

        var listViews = new List<MultiColumnListView>
        {
            listViewFavorites,
            listViewMisc,
            listViewMaterials,
            listViewMedical,
            listViewFood,
            listViewKeys,
            listViewDocuments,
            listViewPhone,
            listViewTools
        };

        foreach (var listView in listViews)
        {
            GenerateColumns(listView);
        }
    }

    private void GenerateColumns(MultiColumnListView listView)
    {
        // Get the appropriate item type for this list view
        ItemType listType = GetListViewType(listView);
        var typeItems = itemsByType[listType];

        listView.columns.Clear();

        // Enable sorting
        listView.sortingMode = ColumnSortingMode.Default;

        // Item name
        var nameColumn = new Column
        {
            title = "Item",
            name = "ItemName",
            width = Length.Percent(80),
            makeCell = () => new Label(),
            bindCell = (element, i) =>
            {
                if (element is Label label && i >= 0 && i < typeItems.Count)
                {
                    label.text = typeItems[i].Item.Name;
                }
            },
            comparison = (a, b) => string.Compare(typeItems[a].Item.Name, typeItems[b].Item.Name)
        };
        listView.columns.Add(nameColumn);

        // Item weight
        var weightColumn = new Column
        {
            title = "Weight",
            name = "ItemWeight",
            width = Length.Percent(10),
            makeCell = () => new Label(),
            bindCell = (element, i) =>
            {
                if (element is Label label && i >= 0 && i < typeItems.Count)
                {
                    float totalWeight = typeItems[i].Item.Weight * typeItems[i].Quantity;
                    string format = totalWeight % 1 == 0 ? "F0" : "F1";
                    label.text = totalWeight.ToString(format);
                }
            },
            comparison = (a, b) =>
            {
                float weightA = typeItems[a].Item.Weight * typeItems[a].Quantity;
                float weightB = typeItems[b].Item.Weight * typeItems[b].Quantity;
                return weightA.CompareTo(weightB);
            }
        };
        listView.columns.Add(weightColumn);

        // Item quantity
        var quantityColumn = new Column
        {
            title = "Quantity",
            name = "ItemQuantity",
            width = Length.Percent(10),
            makeCell = () => new Label(),
            bindCell = (element, i) =>
            {
                if (element is Label label && i >= 0 && i < typeItems.Count)
                {
                    label.text = typeItems[i].Quantity.ToString();
                }
            },
            comparison = (a, b) => typeItems[a].Quantity.CompareTo(typeItems[b].Quantity)
        };
        listView.columns.Add(quantityColumn);

        // Optional: Add debug logging for sorting changes
        listView.columnSortingChanged += () =>
        {
            SBGDebug.LogInfo($"Sorting changed for {listView.name}", "InventoryMenu");
            foreach (var sortedColumn in listView.sortedColumns)
            {
                SBGDebug.LogInfo($"Column {sortedColumn.columnName} sorted {sortedColumn.direction}", "InventoryMenu");
            }
        };

        listView.itemsSource = typeItems;
        listView.Rebuild();
    }

    private ItemType GetListViewType(MultiColumnListView listView)
    {
        return listView.name switch
        {
            "Misc-ListView" => ItemType.Misc,
            "Materials-ListView" => ItemType.Material,
            "Medical-ListView" => ItemType.Medical,
            "Food-ListView" => ItemType.Food,
            "Keys-ListView" => ItemType.Keys,
            "Documents-ListView" => ItemType.Document,
            "Phone-ListView" => ItemType.Phone,
            "Tools-ListView" => ItemType.Tools,
            _ => ItemType.Misc // Default case
        };
    }
    #endregion

    private void Start()
    {
        PopulateListViews();
    }

    #region List Population
    private async void PopulateListViews()
    {
        await Task.Run(() => new WaitUntil(() => InventoryManager.Instance.inventoryLoaded));

        var playerInventory = InventoryManager.Instance.GetPlayerInventory();
        var allItems = playerInventory.InventoryListViewItems;

        // Clear all existing items
        foreach (var list in itemsByType.Values)
        {
            list.Clear();
        }

        // Sort items into their respective type lists
        foreach (var item in allItems)
        {
            itemsByType[item.Item.Type].Add(item);
        }

        // Rebuild all list views to show the new data
        var listViews = new List<MultiColumnListView>
        {
            listViewMisc,
            listViewMaterials,
            listViewMedical,
            listViewFood,
            listViewKeys,
            listViewDocuments,
            listViewPhone,
            listViewTools
        };

        foreach (var listView in listViews)
        {
            if (listView != null)
            {
                listView.Rebuild();
            }
        }
    }
    #endregion
}

public struct InventoryListItem
{
    public IItem Item;
    public int Quantity;
}
#endregion