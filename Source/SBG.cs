using System;

namespace SBG
{
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
    [Obsolete("Use WeaponID instead")]
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
}