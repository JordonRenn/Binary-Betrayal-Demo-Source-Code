using UnityEngine;

/* 
INHERITANCE STRUCTURE:
IItem
├── ItemBase (abstract class)
│   ├── Item Type Classes (e.g., Item_Misc, Item_Material, Item_Food, Item_Keys, Item_Quest)
│   └── ItemData (concrete implementation of IItem)
 */

[System.Serializable]
public abstract class ItemBase : IItem
{
    public string ItemId { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public abstract ItemType Type { get; }
    public Sprite Icon { get; protected set; }
    public float weight { get; protected set; }
    public virtual ItemRarity Rarity { get; protected set; }
    public virtual ItemViewLogicType ViewLogic { get; protected set; }

    protected ItemBase(string id, string name, string description, Sprite icon, ItemType type = ItemType.Misc, float weight = 0, ItemRarity rarity = ItemRarity.Common, ItemViewLogicType viewLogic = ItemViewLogicType.Static)
    {
        ItemId = id;
        Name = name;
        Description = description;
        Icon = icon;
        this.weight = weight;
        Rarity = rarity;
        ViewLogic = viewLogic;
    }
}
