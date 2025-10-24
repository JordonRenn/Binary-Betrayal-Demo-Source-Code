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
    class Item_Misc : ItemBase
    {
        public override ItemType Type => ItemType.Misc;

        public Item_Misc(string id, string name, string description, Sprite icon, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Misc, weight, rarity, viewLogic)
        {
            // Additional initialization for misc items if needed
        }
    }

    [System.Serializable]
    class Item_Material : ItemBase
    {
        public override ItemType Type => ItemType.Material;
        public Item_MaterialType MaterialType { get; }
        public ItemEffect_Material EffectType { get; }
        public int EffectValue { get; }

        public Item_Material(string id, string name, string description, Sprite icon, Item_MaterialType materialType, ItemEffect_Material effectType, int effectValue, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Material, weight, rarity, viewLogic)
        {
            MaterialType = materialType;
            EffectType = effectType;
            EffectValue = effectValue;
        }
    }

    [System.Serializable]
    class Item_Food : ItemBase
    {
        public override ItemType Type => ItemType.Food;
        public Item_FoodType FoodType { get; }
        public ItemEffect_Food EffectType { get; }
        public int EffectValue { get; }

        public Item_Food(string id, string name, string description, Sprite icon, Item_FoodType foodType, ItemEffect_Food effectType, int effectValue, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Food, weight, rarity, viewLogic)
        {
            FoodType = foodType;
            EffectType = effectType;
            EffectValue = effectValue;
        }
    }

    [System.Serializable]
    class Item_Keys : ItemBase
    {
        public override ItemType Type => ItemType.Keys;
        public KeyType KeyType { get; }
        public ItemEffect_Key EffectType { get; }

        public Item_Keys(string id, string name, string description, Sprite icon, KeyType keyType, ItemEffect_Key effectType, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Keys, weight, rarity, viewLogic)
        {
            KeyType = keyType;
            EffectType = effectType;
        }
    }

    [System.Serializable]
    class Item_Quest : ItemBase
    {
        public override ItemType Type => ItemType.Quest;
        public Item_QuestType QuestType { get; }
        public string QuestID { get; }
        public ItemEffect_Quest EffectType { get; }

        public Item_Quest(string id, string name, string description, Sprite icon, Item_QuestType questType, string questId, ItemEffect_Quest effectType, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Quest, weight, rarity, viewLogic)
        {
            QuestType = questType;
            QuestID = questId;
            EffectType = effectType;
        }
    }

    [System.Serializable]
    class Item_Medical : ItemBase
    {
        public override ItemType Type => ItemType.Medical;
        public Item_MedicalType MedicalType { get; }
        public ItemEffect_Medical EffectType { get; }
        public int EffectValue { get; }

        public Item_Medical(string id, string name, string description, Sprite icon, Item_MedicalType medicalType, ItemEffect_Medical effectType, int effectValue, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Medical, weight, rarity, viewLogic)
        {
            MedicalType = medicalType;
            EffectType = effectType;
            EffectValue = effectValue;
        }
    }

    [System.Serializable]
    class Item_Phone : ItemBase
    {
        public override ItemType Type => ItemType.Phone;
        public Item_PhoneType PhoneType { get; }
        public string PhoneNumber { get; }
        public ItemEffect_Phone EffectType { get; }

        public Item_Phone(string id, string name, string description, Sprite icon, Item_PhoneType phoneType, string phoneNumber, ItemEffect_Phone effectType, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Phone, weight, rarity, viewLogic)
        {
            PhoneType = phoneType;
            PhoneNumber = phoneNumber;
            EffectType = effectType;
        }
    }

    [System.Serializable]
    class Item_Tools : ItemBase
    {
        public override ItemType Type => ItemType.Tools;
        public Item_ToolType ToolType { get; }
        public ItemEffect_Tool EffectType { get; }

        public Item_Tools(string id, string name, string description, Sprite icon, Item_ToolType toolType, ItemEffect_Tool effectType, float weight, ItemRarity rarity, ItemViewLogicType viewLogic)
            : base(id, name, description, icon, ItemType.Tools, weight, rarity, viewLogic)
        {
            ToolType = toolType;
            EffectType = effectType;
        }
    }
}