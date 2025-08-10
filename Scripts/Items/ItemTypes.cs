using UnityEngine;

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

    public Item_Misc(string id, string name, string description, Sprite icon, GameObject modelPrefab, int weight = 0)
        : base(id, name, description, icon, ItemType.Misc, weight, ItemRarity.Common, ItemViewLogicType.Static)
    {
        // Additional initialization for misc items if needed
    }
}

[System.Serializable]
class Item_Material : ItemBase
{
    public override ItemType Type => ItemType.Material;
    public Item_MaterialType MaterialType { get; private set; }
    public ItemEffect_Material EffectType { get; private set; }
    public int EffectValue { get; private set; }

    public Item_Material(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_MaterialType materialType, ItemEffect_Material effectType, int effectValue, int weight = 0)
        : base(id, name, description, icon, ItemType.Material, weight, ItemRarity.Common, ItemViewLogicType.Static)
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
    public Item_FoodType FoodType { get; private set; }
    public ItemEffect_Food EffectType { get; private set; }
    public int EffectValue { get; private set; }

    public Item_Food(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_FoodType foodType, ItemEffect_Food effectType, int effectValue, int weight = 0)
        : base(id, name, description, icon, ItemType.Food, weight, ItemRarity.Common, ItemViewLogicType.Consumable)
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
    public KeyType KeyType { get; private set; }
    public ItemEffect_Key EffectType { get; private set; }

    public Item_Keys(string id, string name, string description, Sprite icon, GameObject modelPrefab, KeyType keyType, ItemEffect_Key effectType, int weight = 0)
        : base(id, name, description, icon, ItemType.Keys, weight, ItemRarity.Common, ItemViewLogicType.Static)
    {
        KeyType = keyType;
        EffectType = effectType;
    }
}

[System.Serializable]
class Item_Quest : ItemBase
{
    public override ItemType Type => ItemType.Quest;
    public Item_QuestType QuestType { get; private set; }
    public string QuestID { get; private set; }
    public ItemEffect_Quest EffectType { get; private set; }

    public Item_Quest(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_QuestType questType, string questId, ItemEffect_Quest effectType = ItemEffect_Quest.None, int weight = 0)
        : base(id, name, description, icon, ItemType.Quest, weight, ItemRarity.Quest, ItemViewLogicType.Static)
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
    public Item_MedicalType MedicalType { get; private set; }
    public ItemEffect_Medical EffectType { get; private set; }
    public int EffectValue { get; private set; }

    public Item_Medical(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_MedicalType medicalType, ItemEffect_Medical effectType, int effectValue, int weight = 0)
        : base(id, name, description, icon, ItemType.Medical, weight, ItemRarity.Common, ItemViewLogicType.Consumable)
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
    public Item_PhoneType PhoneType { get; private set; }
    public string PhoneNumber { get; private set; }
    public ItemEffect_Phone EffectType { get; private set; }

    public Item_Phone(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_PhoneType phoneType, string phoneNumber, ItemEffect_Phone effectType = ItemEffect_Phone.None, int weight = 0)
        : base(id, name, description, icon, ItemType.Phone, weight, ItemRarity.Common, ItemViewLogicType.Usable)
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
    public Item_ToolType ToolType { get; private set; }
    public ItemEffect_Tool EffectType { get; private set; }

    public Item_Tools(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_ToolType toolType, ItemEffect_Tool effectType, int weight = 0)
        : base(id, name, description, icon, ItemType.Tools, weight, ItemRarity.Common, ItemViewLogicType.Usable)
    {
        ToolType = toolType;
        EffectType = effectType;
    }
}