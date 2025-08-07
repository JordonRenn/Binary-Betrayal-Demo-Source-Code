using UnityEngine;

public interface IItem
{
    string ItemId { get; }
    string Name { get; }
    string Description { get; }
    Sprite Icon { get; }
    ItemType Type { get; }
    int weight { get; }
}

public class ItemData : IItem
{
    public string ItemId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Sprite Icon { get; private set; }
    public ItemType Type { get; private set; }
    public int weight { get; private set; }

    public ItemData(string itemId, string name, string description, Sprite icon, ItemType type, int weight)
    {
        ItemId = itemId;
        Name = name;
        Description = description;
        Icon = icon;
        Type = type;
        this.weight = weight;
    }
}