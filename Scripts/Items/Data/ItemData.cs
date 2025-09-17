using System;

#region ItemData
[Serializable]
public class ItemData
{
    public string itemId;
    public string name;
    public string description;
    public float value;
    public float weight;
    public ItemType itemType;
    public string subType;
    public string effectType;
    public float effectValue;
    public ItemRarity ItemRarity;
    public ItemViewLogicType ViewLogicType;
    public string iconId;

    public ItemData(string itemId, string name, string description, float value, float weight, ItemType itemType, string subType, string effectType, float effectValue, ItemRarity itemRarity, ItemViewLogicType viewLogicType, string iconId)
    {
        this.itemId = itemId;
        this.name = name;
        this.description = description;
        this.value = value;
        this.weight = weight;
        this.itemType = itemType;
        this.subType = subType;
        this.effectType = effectType;
        this.effectValue = effectValue;
        this.ItemRarity = itemRarity;
        this.ViewLogicType = viewLogicType;
        this.iconId = iconId;
    }
}
#endregion

#region Context Data
public class ItemContextData
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}
#endregion