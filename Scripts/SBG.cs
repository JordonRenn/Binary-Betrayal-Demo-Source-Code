#region General

public enum Language
{
    English,
    Spanish,
    French
}

public enum SurfaceType
{
    Concrete,
    Dirt,
    Grass,
    Metal,
    Wood,
    Glass,
    Flesh,
    Plastic,
    None
}
#endregion

#region Phone
public enum PhoneCallEvent
{
    Incoming,
    Outgoing
}
#endregion

#region Graphics
public enum VolumeType //post process volume
{
    PauseMenu,
    LockPick,
    Cutscene,
    Default
}
#endregion


#region Menus
/// <summary>
/// Enum representing the main menu state
/// </summary>
public enum MainMenuState 
{ 
    Main, //showing logo
    Play, //showing play button
    Settings, //showing settings button
    Credits, //showing credits button
    StartingGame //showing static, transitioning to game
}

/// <summary>
/// Enum representing the settings menu state
/// </summary>
public enum SettingsMenuState
{
    NotDisplayed,
    Gameplay,
    Video,
    Audio,
    Controls,
    Credits
}

public enum PauseMenuState
{
    NotDisplayed,
    Main,
    Settings
}
#endregion


#region Scenes
public enum SceneName
{
    MainMenu,
    C01_01,
    C01_02,
    C01_03,
    C01_04,
    EndCredits,
    Dev_1 //scene used for testing systems and mechanics
}
#endregion


#region Weapons
/// <summary>
/// Enum representing the weapon ref IDs.
/// </summary>
public enum WeaponRefID
{
    Rifle,
    Sniper,
    Handgun,
    Shotgun,
    Knife,
    Grenade,
    SmokeGrenade,
    FlashGrenade,
    Unarmed,
}

/// <summary>
/// Enum representing the fire mode of the weapon.
/// </summary>
public enum WeaponFireMode
{
    Automatic,
    SemiAutomatic,
    BoltAction
}

/// <summary>
/// Enum representing the different weapon slots.
/// </summary>
public enum WeaponSlot
{
    Primary,
    Secondary,
    Melee,
    Utility
}
#endregion


#region Doors
public enum DoorState
{
    OpenExt, //door swung open to the outside
    OpenInt, //door swung open to the inside
    Closed
}

public enum DoorLockState
{
    LockedInteriorOnly,
    LockedExteriorOnly,
    Locked,
    Unlocked
}

public enum LockedDoorDialogueVariation
{
    LockedKeyNeeded,
    LockedCanLockPick,
    LockedHasKey,
    StillLocked

}

public enum LockDifficulty
{
    Easy,
    Moderate,
    Hard
}
#endregion


#region Notifications
public enum NotificationType
{
    Normal,
    Warning,
    Error,
    Reward,
}

/// <summary>
/// Struct used to format notification system messages so they can be passed in one call
/// </summary>
public struct Notification
{
    public string message;
    public NotificationType type;

    public Notification(string _message, NotificationType _type)
    {
        message = _message;
        type = _type;
    }
}
#endregion

#region Questing
public enum QuestState
{
    Incomplete,
    InProgress,
    Complete
}

public enum QuestType
{
    Main,
    Side,
    Secret,
    Encounter
}

public enum ObjectiveType
{
    Talk,
    Collect,
    DoorLock,
    PhoneCall,
    Kill,
    Explore,
    UseItem,
    Interact
}

public enum ObjectiveStatus
{
    InProgress,
    CompletedSuccess,
    CompletedFailure,
    Abandoned
}

public enum RewardType {
    Item,
    Experience,
    Currency,
    Skill,
    Reputation
}

public enum TargetType {
    NPC,
    Item,
    Location,
    Interactable
}
#endregion

#region Inventory
public enum InventoryType
{
    Player,
    Container,
    NPC
}
public enum ItemType
{
    Misc, //used as default fallback
    Material,
    Food,
    Keys,
    Quest,
    Medical,
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
#endregion