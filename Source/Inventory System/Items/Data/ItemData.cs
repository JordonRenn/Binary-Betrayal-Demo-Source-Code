using System;

namespace SBG.InventorySystem
{
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

    public enum ItemType
    {
        Misc, //used as default fallback
        Material,
        Food,
        Keys,
        Quest,
        Medical,
        Document,
        Phone,
        Tools
    }

    public enum Item_MaterialType
    {
        MetalScraps,
        PlasticScraps,
        CircuitBoards,
        Wires,
        Cloth,
    }

    public enum Item_FoodType
    {
        CannedFood,
        Drink,
        Snack,
        Ration
    }

    public enum KeyType
    {
        Key,
        Keycard,
        Code
    }

    public enum Item_ToolType
    {
        KeyJammer,
        CameraJammer,
        AlarmJammer,
        RadioJammer
    }

    public enum Item_MedicalType
    {
        Supplement,
        Bandage,
        Painkillers,
        FirstAidKit
    }

    public enum Item_PhoneType
    {
        Number
    }

    public enum Item_QuestType
    {
        Main,
        Side,
        Secret,
        Hidden
    }

    public enum ItemEffect_Material
    {
        CraftMetal,
        CraftPlastic,
        CraftElectronics,
        CraftCloth
    }

    public enum ItemEffect_Food
    {
        RestoreHealth,              // fully restores health
        BoostHealth,                // incremental restoration of health
        BoostStamina,               // incremental restoration of stamina
        BoostHealthRegen,           // temporarily increases health regeneration
        BoostMaxHealth,             // temporarily increases max health
        BoostStaminaRegen,          // temporarily increases stamina regeneration
        BoostSpeed                  // temporarily increases speed
    }

    public enum ItemEffect_Key
    {
        UnlockDoor,
        UnlockContainer
    }

    // FUTURE FEATURES
    public enum ItemEffect_Tool
    {
        DisableCamera,
        DisableAlarm,
        DisableRadio,
        DisableKeypad
    }

    public enum ItemEffect_Medical
    {
        Heal,
        StopBleeding,
        CurePoison,
        CureDisease,
        Painkiller,
        BoostHealthRegen,
        BoostMaxHealth
    }

    // TBD
    public enum ItemEffect_Phone
    {
        None
    }

    // TBD
    public enum ItemEffect_Quest
    {
        None
    }

    // can be used to change logic of how items are displayed in the inventory
    public enum ItemViewLogicType
    {
        Static,     //item is static, does not change
        Consumable, //item can be consumed/used
        Usable      //item can be used in some way
    }

    public enum ItemRarity
    {
        Ordinary,   //white
        Common,     //blue
        Uncommon,   //green
        Rare,       //purple
        Legendary,  //Red
        Quest       //Gold
    }
}