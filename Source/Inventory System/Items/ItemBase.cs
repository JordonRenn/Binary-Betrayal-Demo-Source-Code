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

    [System.Serializable]
    public abstract class ItemBase : IItem
    {
        public string ItemId { get; }
        public string Name { get; }
        public string Description { get; }
        public abstract ItemType Type { get; }
        public Sprite Icon { get; }
        public float Weight { get; }
        public ItemRarity Rarity { get; }
        public ItemViewLogicType ViewLogic { get; }

        protected ItemBase(string id, string name, string description, Sprite icon, ItemType type, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
        {
            ItemId = id;
            Name = name;
            Description = description;
            Icon = icon;
            Weight = weight;
            Rarity = rarity;
            ViewLogic = viewLogic;
        }
    }
}