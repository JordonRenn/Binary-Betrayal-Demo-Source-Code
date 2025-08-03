using UnityEngine;

[System.Serializable]
public class Item : IItem
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemType itemType;
    [SerializeField] private int itemWeight;

    public string ItemId => itemId;
    public string Name => itemName;
    public string Description => itemDescription;
    public Sprite Icon => itemIcon;
    public ItemType Type => itemType;
    public int weight => itemWeight;

    public Item(string id, string name, string description, Sprite icon, ItemType type, int weight)
    {
        this.itemId = id;
        this.itemName = name;
        this.itemDescription = description;
        this.itemIcon = icon;
        this.itemType = type;
        this.itemWeight = weight;
    }
}
