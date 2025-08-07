using UnityEngine;

/* 
INHERITANCE STRUCTURE:
IItem
├── ItemBase (abstract class)
│   ├── ItemTypes (e.g., Item_Misc, Item_Material, Item_Food, Item_Keys, Item_Quest)
│   └── ItemData (concrete implementation of IItem)
 */

[System.Serializable]
class Item_Misc : ItemBase
{
    public override ItemType Type => ItemType.Misc;

    public Item_Misc(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize)
        : base(id, name, description, icon, ItemType.Misc, 1)
    {
        // Additional initialization for misc items if needed
    }
}

[System.Serializable]
class Item_Material : ItemBase
{
    public override ItemType Type => ItemType.Material;

    public Item_Material(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize)
        : base(id, name, description, icon, ItemType.Material, stackSize)
    {
        // Additional initialization for material items if needed
    }
}

[System.Serializable]
class Item_Food : ItemBase
{
    public override ItemType Type => ItemType.Food;

    public Item_Food(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize)
        : base(id, name, description, icon, ItemType.Food, stackSize)
    {
        // Additional initialization for food items if needed
    }
}

[System.Serializable]
class Item_Keys : ItemBase
{
    public override ItemType Type => ItemType.Keys;
    public KeyType KeyType { get; private set; }

    public Item_Keys(string id, string name, string description, Sprite icon, GameObject modelPrefab, KeyType keyType)
        : base(id, name, description, icon, ItemType.Keys, 1)
    {
        KeyType = keyType;
        // Additional initialization for key items if needed
    }
}

[System.Serializable]
class Item_Quest : ItemBase
{
    public override ItemType Type => ItemType.Quest;
    public string QuestID { get; private set; }

    public Item_Quest(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize, string questId)
        : base(id, name, description, icon, ItemType.Quest, stackSize)
    {
        QuestID = questId;
        // Additional initialization for quest items if needed
    }
}

[System.Serializable]
class Item_Medical : ItemBase
{
    public override ItemType Type => ItemType.Medical;

    public Item_Medical(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize)
        : base(id, name, description, icon, ItemType.Medical, stackSize)
    {
        // Additional initialization for medical items if needed
    }
}

[System.Serializable]
class Item_Phone : ItemBase
{
    public override ItemType Type => ItemType.Phone;
    public string PhoneNumber { get; private set; }

    public Item_Phone(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize, string phoneNumber)
        : base(id, name, description, icon, ItemType.Phone, stackSize)
    {
        PhoneNumber = phoneNumber;
        // Additional initialization for phone items if needed
    }
}

[System.Serializable]
class Item_Tools : ItemBase
{
    public override ItemType Type => ItemType.Tools;

    public Item_Tools(string id, string name, string description, Sprite icon, GameObject modelPrefab, int stackSize)
        : base(id, name, description, icon, ItemType.Tools, stackSize)
    {
        // Additional initialization for tool items if needed
    }
}