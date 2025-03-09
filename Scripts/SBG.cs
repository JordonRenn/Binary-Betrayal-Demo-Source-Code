using UnityEngine;
using System;

//enums and structs and other global stuffs

#region General
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

public struct TargetFloatRange //create a range using floats
{
    public float minValue {get ; private set;}
    public float maxValue {get ; private set;}

    public TargetFloatRange(float min, float max)
    {
        this.minValue = min;
        this.maxValue = max;
    }
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
/// <summary>
/// Struct used to format dialog messages so they can be passed in one call
/// </summary>
public struct DialogMessage
{
    public string charName;
    public Sprite charAvatar;
    public string charMessage;
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

#region SBG Math
public static class SBGMath
{
    /// <summary>
    /// Gets a random float value within a specified range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive)</param>
    /// <param name="max">Maximum value (inclusive)</param>
    /// <returns>Random float between min and max</returns>
    public static float FloatRandomRange(float min, float max)
    {
        if (min > max)
        {
            Debug.LogWarning("FloatRandomRange: min value is greater than max, swapping values");
            (min, max) = (max, min);
        }
        
        return UnityEngine.Random.Range(min, max);
    }
}
#endregion