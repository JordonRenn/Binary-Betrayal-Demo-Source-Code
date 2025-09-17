using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public string inventoryId;
    public string displayName;
    public float maxWeight;
    public InventoryType inventoryType;
    public List<IItem> items;

    public InventoryData()
    {
        items = new List<IItem>();
    }
}

public class InventoryContextData
{
    public string InventoryId { get; set; }
    public List<ItemContextData> Items { get; set; }

    public InventoryContextData()
    {
        Items = new List<ItemContextData>();
    }
}