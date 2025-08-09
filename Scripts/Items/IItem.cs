using UnityEngine;

/* 
INHERITANCE STRUCTURE:
IItem
├── ItemBase (abstract class)
│   ├── Item Type Classes (e.g., Item_Misc, Item_Material, Item_Food, Item_Keys, Item_Quest)
│   └── ItemData (concrete implementation of IItem)
 */

public interface IItem
{
    string ItemId { get; }
    string Name { get; }
    string Description { get; }
    Sprite Icon { get; }
    ItemType Type { get; }
    int weight { get; }
    ItemRarity Rarity { get; }
    ItemViewLogicType ViewLogic { get; }
}

public class ItemData : IItem
{
    public string ItemId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Sprite Icon { get; private set; }
    public ItemType Type { get; private set; }
    public int weight { get; private set; }
    public ItemRarity Rarity { get; private set; }
    public ItemViewLogicType ViewLogic { get; private set; }

    public ItemData(string itemId, string name, string description, Sprite icon, ItemType type, int weight, ItemRarity rarity, ItemViewLogicType viewLogic)
    {
        ItemId = itemId;
        Name = name;
        Description = description;
        Icon = icon;
        Type = type;
        this.weight = weight;
        Rarity = rarity;
        ViewLogic = viewLogic;
    }
}