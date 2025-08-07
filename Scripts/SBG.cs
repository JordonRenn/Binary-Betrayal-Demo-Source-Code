using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

//enums and structs and other global stuffs

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

public enum LockDifficulty
{
    Easy,
    Moderate,
    Hard
}

public enum KeyType
{
    Key,
    Keycard,
    Code
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


#region Dialog
[Serializable]
public class DialogueData
{
    public string dialogueId;
    public List<DialogueEntry> entries;
}

[Serializable]
public class DialogueEntry
{
    public string characterName;
    public string avatarPath;
    public string message;
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

[System.Serializable]
public class QuestData {
    public int quest_id;
    public QuestType type;
    public string title;
    public string description;
    public Objective[] objectives;
    public int[] prerequisiteQuestIds; // For quest chains
    public QuestReward[] rewards;
    public bool isRepeatable;
    public float timeLimit;
    public QuestState state { get; private set; } = QuestState.Incomplete;
    
    public float Progress => 
        objectives?.Length > 0 
            ? objectives.Average(o => o.Progress)
            : 0f;
            
    public void UpdateState()
    {
        if (state == QuestState.Complete) return;
        
        if (objectives == null || objectives.Length == 0)
        {
            state = QuestState.Complete;
            return;
        }
        
        bool allComplete = objectives.All(o => o.IsCompleted);
        state = allComplete ? QuestState.Complete :
               objectives.Any(o => o.Progress > 0) ? QuestState.InProgress :
               QuestState.Incomplete;
    }
}

[System.Serializable]
public class Objective
{
    public int objective_id;
    public ObjectiveType type;
    public string[] item;  // For collect objectives
    public int quantity;
    public int currentQuantity;
    public string[] enemy; // For kill objectives
    public string dialogID; // For talk objectives
    public ObjectiveTarget[] target;  // Use a specific type (e.g., NPC or item) later
    public string message;
    public bool IsCompleted { get; private set; }

    public float Progress =>
        quantity > 0 ? Mathf.Clamp01((float)currentQuantity / quantity) :
        IsCompleted ? 1f : 0f;

    public void MarkAsComplete()
    {
        IsCompleted = true;
    }

    public void UpdateProgress(int newCount)
    {
        currentQuantity = Mathf.Min(newCount, quantity);
        if (currentQuantity >= quantity)
            MarkAsComplete();
    }
}

public enum ObjectiveStatus
{
    InProgress,
    CompletedSuccess,
    CompletedFailure,
    Abandoned
}

[System.Serializable]
public class ObjectiveTarget
{
    public string targetId;
    public Vector3 position;
    public TargetType type;
    public bool isInteracted;
    
    // Additional data can be stored in a serialized format
    public string data;
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public string itemId;
    public int quantity;
    public int experiencePoints;
    public float currency;
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

// can be used to change logic of how items are displayed in the inventory
public enum ItemViewLogicType
{
    Static,     //item is static, does not change
    Consumable, //item can be consumed/used
    Usable      //item can be used in some way
}

public enum ItemRarity
{
    Ordanary,   //white
    Common,     //blue
    Uncommon,   //green
    Rare,       //purple
    Legendary,  //Red
    Quest       //Gold
}
#endregion