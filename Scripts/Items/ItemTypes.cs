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

    public Item_Misc(string id, string name, string description, Sprite icon, GameObject modelPrefab)
        : base(id, name, description, icon, ItemType.Misc, 0, ItemRarity.Common, ItemViewLogicType.Static)
    {
        // Additional initialization for misc items if needed
    }
}

[System.Serializable]
class Item_Material : ItemBase
{
    public override ItemType Type => ItemType.Material;
    public Item_MaterialType MaterialType { get; private set; }

    public Item_Material(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_MaterialType materialType)
        : base(id, name, description, icon, ItemType.Material, 0 , ItemRarity.Common, ItemViewLogicType.Static)
    {
        MaterialType = materialType;
        // Additional initialization for material items if needed
    }
}

[System.Serializable]
class Item_Food : ItemBase
{
    public override ItemType Type => ItemType.Food;
    public Item_FoodType FoodType { get; private set; }

    public Item_Food(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_FoodType foodType)
        : base(id, name, description, icon, ItemType.Food, 0 , ItemRarity.Common, ItemViewLogicType.Consumable)
    {
        FoodType = foodType;
        // Additional initialization for food items if needed
    }
}

[System.Serializable]
class Item_Keys : ItemBase
{
    public override ItemType Type => ItemType.Keys;
    public KeyType KeyType { get; private set; }

    public Item_Keys(string id, string name, string description, Sprite icon, GameObject modelPrefab, KeyType keyType)
        : base(id, name, description, icon, ItemType.Keys, 0 , ItemRarity.Common, ItemViewLogicType.Static)
    {
        KeyType = keyType;
        // Additional initialization for key items if needed
    }
}

[System.Serializable]
class Item_Quest : ItemBase
{
    public override ItemType Type => ItemType.Quest;
    public Item_QuestType QuestType { get; private set; }
    public string QuestID { get; private set; }

    public Item_Quest(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_QuestType questType, string questId)
        : base(id, name, description, icon, ItemType.Quest, 0 , ItemRarity.Common, ItemViewLogicType.Static)
    {
        QuestType = questType;
        QuestID = questId;
        // Additional initialization for quest items if needed
    }
}

[System.Serializable]
class Item_Medical : ItemBase
{
    public override ItemType Type => ItemType.Medical;
    public Item_MedicalType MedicalType { get; private set; }

    public Item_Medical(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_MedicalType medicalType)
        : base(id, name, description, icon, ItemType.Medical, 0 , ItemRarity.Common, ItemViewLogicType.Consumable)
    {
        MedicalType = medicalType;
    }
}

[System.Serializable]
class Item_Phone : ItemBase
{
    public override ItemType Type => ItemType.Phone;
    public Item_PhoneType PhoneType { get; private set; }
    public string PhoneNumber { get; private set; }

    public Item_Phone(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_PhoneType phoneType, string phoneNumber)
        : base(id, name, description, icon, ItemType.Phone, 0 , ItemRarity.Common, ItemViewLogicType.Usable)
    {
        PhoneType = phoneType;
        PhoneNumber = phoneNumber;
        // Additional initialization for phone items if needed
    }
}

[System.Serializable]
class Item_Tools : ItemBase
{
    public override ItemType Type => ItemType.Tools;
    public Item_ToolType ToolType { get; private set; }

    public Item_Tools(string id, string name, string description, Sprite icon, GameObject modelPrefab, Item_ToolType toolType)
        : base(id, name, description, icon, ItemType.Tools, 0 , ItemRarity.Common, ItemViewLogicType.Usable)
    {
        ToolType = toolType;
    }
}