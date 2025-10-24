using System;
using System.Collections.Generic;
using SBG.InventorySystem.Items;

namespace SBG.InventorySystem
{
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

    [Serializable]
    public class InventoryContextData
    {
        public string InventoryId { get; set; }
        public List<ItemContextData> Items { get; set; }

        public InventoryContextData()
        {
            Items = new List<ItemContextData>();
        }
    }

    public enum InventoryType
    {
        Player,
        Container,
        NPC
    }
}