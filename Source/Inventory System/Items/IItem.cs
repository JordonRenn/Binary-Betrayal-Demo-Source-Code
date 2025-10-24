using UnityEngine;

namespace SBG.InventorySystem.Items
{
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
        float Weight { get; }
        ItemRarity Rarity { get; }
        ItemViewLogicType ViewLogic { get; }
    }
}