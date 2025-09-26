using System;

namespace GlobalEvents
{   
    public static class Events
    {
        public static event Action Tick;
        internal static void RaiseTick()
        {
            Tick?.Invoke();
        }
    }

    #region System Events
    public static class SystemEvents
    {
        public static event Action GameMasterInitialized;

        internal static void RaiseGameMasterInitialized()
        {
            GameMasterInitialized?.Invoke();
            SBGDebug.LogEvent("GameMaster Initialized", nameof(SystemEvents));
        }
    }
    #endregion

    #region Sauce Object Events
    public static class SauceObjectEvents
    {
        /// <summary>
        /// Event triggered when an object is interacted with.
        /// Parameters: string (objectID)
        /// </summary>
        public static event Action<string> InteractionEvent;

        internal static void RaiseInteractionEvent(string objectId)
        {
            InteractionEvent?.Invoke(objectId);
            SBGDebug.LogEvent($"Object Interacted: {objectId}", nameof(SauceObject));
        }
    }
    #endregion

    #region Inventory Events
    public static class InventoryEvents
    {
        /// <summary>
        /// Event triggered when an item is added to the inventory.
        /// Parameters: InventoryType (inventory type), string (itemID), string (display name)
        /// </summary>
        public static event Action<InventoryType, string, string> ItemAdded;

        /// <summary>
        /// Event triggered when an item is removed from the inventory.
        /// Parameters: InventoryType (inventory type), string (itemID), string (display name)
        /// </summary>
        public static event Action<InventoryType, string, string> ItemRemoved;

        internal static void RaiseItemAdded(InventoryType type, string itemId, string displayName)
        {
            ItemAdded?.Invoke(type, itemId, displayName);
            SBGDebug.LogEvent($"Item Added: {itemId} ({displayName}) to {type} inventory", nameof(InventoryEvents));
        }

        internal static void RaiseItemRemoved(InventoryType type, string itemId, string displayName)
        {
            ItemRemoved?.Invoke(type, itemId, displayName);
            SBGDebug.LogEvent($"Item Removed: {itemId} ({displayName}) from {type} inventory", nameof(InventoryEvents));
        }
    }
    #endregion

    #region Dialogue Events
    public static class DialogueEvents
    {
        /// <summary>
        /// Event triggered when a specific dialogue is triggered.
        /// Parameters: string (dialogueID)
        /// </summary>
        public static event Action<string> DialogueTriggered;

        public static event Action DialogueStarted;

        public static event Action DialogueEnded;

        internal static void RaiseDialogueTriggered(string dialogueId)
        {
            DialogueTriggered?.Invoke(dialogueId);
            SBGDebug.LogEvent($"Dialogue Triggered: {dialogueId}", nameof(DialogueEvents));
        }

        internal static void RaiseDialogueStarted()
        {
            DialogueStarted?.Invoke();
            SBGDebug.LogEvent("Dialogue Started", nameof(DialogueEvents));
        }

        internal static void RaiseDialogueEnded()
        {
            DialogueEnded?.Invoke();
            SBGDebug.LogEvent("Dialogue Ended", nameof(DialogueEvents));
        }
    }
    #endregion

    #region  Weapon Events
    public static class WeaponEvents
    {
        /// <summary>
        /// Event triggered when a weapon is equipped.
        /// Parameters: WeaponSlot (slot), string (weaponID)
        /// </summary>
        public static event Action<WeaponSlot, string> WeaponEquipped;

        /// <summary>
        /// Event triggered when a weapon is unequipped.
        /// Parameters: WeaponSlot (slot), string (weaponID)
        /// </summary>
        public static event Action<WeaponSlot, string> WeaponUnequipped;

        /// <summary>
        /// Event triggered when ammo count changes.
        /// Parameters: WeaponSlot (slot), int (current ammo), int (reserve ammo)
        /// </summary>
        public static event Action<WeaponSlot, int, int> AmmoChanged;

        internal static void RaiseWeaponEquipped(WeaponSlot slot, string weaponId)
        {
            WeaponEquipped?.Invoke(slot, weaponId);
            SBGDebug.LogEvent($"Weapon Equipped: {weaponId} in slot {slot}", nameof(WeaponEvents));
        }

        internal static void RaiseWeaponUnequipped(WeaponSlot slot, string weaponId)
        {
            WeaponUnequipped?.Invoke(slot, weaponId);
            SBGDebug.LogEvent($"Weapon Unequipped: {weaponId} from slot {slot}", nameof(WeaponEvents));
        }

        internal static void RaiseAmmoChanged(WeaponSlot slot, int currentAmmo, int reserveAmmo)
        {
            AmmoChanged?.Invoke(slot, currentAmmo, reserveAmmo);
            SBGDebug.LogEvent($"Ammo Changed: Slot {slot}, Current Ammo: {currentAmmo}, Reserve Ammo: {reserveAmmo}", nameof(WeaponEvents));
        }
    }
    #endregion

    #region Level Events
    public static class LevelEvents
    {
        /* /// <summary>
        /// Event triggered when a level is loaded.
        /// Parameters: string (door ID), DoorLockState (new state)
        /// </summary>
        public static event Action<string, DoorLockState> DoorLockStateChanged; */

        /// <summary>
        /// Event triggered when a prop is destroyed.
        /// Parameters: string (prop ID)
        /// </summary>
        public static event Action<string> PropDestroyed;

        /// <summary>
        /// Event triggered when the player controller is instantiated.
        /// </summary>
        public static event Action PlayerControllerInstantiated;

        /* internal static void RaiseDoorLockStateChanged(string doorId, DoorLockState state)
        {
            DoorLockStateChanged?.Invoke(doorId, state);
            SBGDebug.LogEvent($"Door Lock State Changed: {doorId} to state {state}", nameof(LevelEvents));
        } */

        internal static void RaisePropDestroyed(string propId)
        {
            PropDestroyed?.Invoke(propId);
            SBGDebug.LogEvent($"Prop Destroyed: {propId}", nameof(LevelEvents));
        }

        internal static void RaisePlayerControllerInstantiated()
        {
            PlayerControllerInstantiated?.Invoke();
            SBGDebug.LogEvent("Player Controller Instantiated", nameof(LevelEvents));
        }
    }
    #endregion

    #region UI Events
    public static class UIEvents
    {
        /// <summary>
        /// Event triggered when the HUD is hidden.
        /// Params: bool (isHidden)
        /// </summary>
        public static event Action<bool> HUDHidden;

        /// <summary>
        /// Event triggered when the inventory is closed.
        /// Params: bool (isClosed)
        /// </summary>
        public static event Action<bool> InventoryClosed;

        /// <summary>
        /// Event triggered when the dialogue is closed.
        /// Params: bool (isClosed)
        /// </summary>
        public static event Action<bool> DialogueClosed;

        /// <summary>
        /// Event triggered when the pause menu is closed.
        /// Params: bool (isClosed)
        /// </summary>
        public static event Action<bool> PauseMenuClosed;

        /// <summary>
        /// Event triggered when the journal is closed.
        /// Params: bool (isClosed)
        /// </summary>
        public static event Action<bool> JournalClosed;

        /// <summary>
        /// Event triggered when the player stats menu is closed.
        /// Params: bool (isClosed)
        /// </summary>
        public static event Action<bool> PlayerStatsClosed;

        internal static void RaiseHUDHidden(bool isHidden)
        {
            HUDHidden?.Invoke(isHidden);
            SBGDebug.LogEvent($"HUD Hidden: {isHidden}", nameof(UIEvents));
        }

        internal static void RaiseInventoryClosed(bool isClosed)
        {
            InventoryClosed?.Invoke(isClosed);
            SBGDebug.LogEvent($"Inventory Closed: {isClosed}", nameof(UIEvents));
        }

        internal static void RaiseDialogueClosed(bool isClosed)
        {
            DialogueClosed?.Invoke(isClosed);
            SBGDebug.LogEvent($"Dialogue Closed: {isClosed}", nameof(UIEvents));
        }

        internal static void RaisePauseMenuClosed(bool isClosed)
        {
            PauseMenuClosed?.Invoke(isClosed);
            SBGDebug.LogEvent($"Pause Menu Closed: {isClosed}", nameof(UIEvents));
        }

        internal static void RaiseJournalClosed(bool isClosed)
        {
            JournalClosed?.Invoke(isClosed);
            SBGDebug.LogEvent($"Journal Closed: {isClosed}", nameof(UIEvents));
        }

        internal static void RaisePlayerStatsClosed(bool isClosed)
        {
            PlayerStatsClosed?.Invoke(isClosed);
            SBGDebug.LogEvent($"Player Stats Closed: {isClosed}", nameof(UIEvents));
        }
    }
    #endregion

    #region Phone Events
    public static class PhoneEvents
    {
        /// <summary>
        /// Event triggered when a phone call is made.
        /// Parameters: string (objectID), string (phone number), PhoneCallEvent (event type)
        /// </summary>
        public static event Action<string, string, PhoneCallEvent> PhoneCallMade;

        internal static void RaisePhoneCallMade(string objectId, string phoneNumber, PhoneCallEvent callEvent)
        {
            PhoneCallMade?.Invoke(objectId, phoneNumber, callEvent);
            SBGDebug.LogEvent($"Phone Call Made: {phoneNumber} ({callEvent}) by {objectId}", nameof(PhoneEvents));
        }
    }
    #endregion

    #region Door Events
    public static class DoorEvents
    {
        /// <summary>
        /// Event triggered when a door lock state changes.
        /// Parameters: string (objectID), DoorLockState (lock state)
        /// </summary>
        public static event Action<string, DoorLockState> DoorLockStateChanged;

        internal static void RaiseDoorLockStateChanged(string objectId, DoorLockState lockState)
        {
            DoorLockStateChanged?.Invoke(objectId, lockState);
            SBGDebug.LogEvent($"Door Lock State Changed: {objectId} to state {lockState}", nameof(DoorEvents));
        }
    }
    #endregion

    #region NPC Events
    public static class NPCEvents
    {
        /// <summary>
        /// Event triggered when an NPC is interacted with.
        /// Parameters: string (objectID)
        /// </summary>
        public static event Action<string> NPCInteracted;

        internal static void RaiseNPCInteracted(string objectId)
        {
            NPCInteracted?.Invoke(objectId);
            SBGDebug.LogEvent($"NPC Interacted: {objectId}", nameof(NPCEvents));
        }
    }
    #endregion

    #region Quest Events
    public static class QuestEvents
    {
        /// <summary>
        /// Event triggered when an interaction event occurs.
        /// Parameters: string (questID), string (quest name)
        /// </summary>
        public static event Action<string, string> StartQuest;

        public static event Action<string> CompleteQuest;

        public static event Action<string> FailQuest;

        public static event Action<string> CompleteObjective;

        internal static void RaiseStartQuest(string questId, string questName)
        {
            StartQuest?.Invoke(questId, questName);
            SBGDebug.LogEvent($"Quest Started: {questName} ({questId})", nameof(QuestEvents));
        }

        internal static void RaiseCompleteQuest(string questId)
        {
            CompleteQuest?.Invoke(questId);
            SBGDebug.LogEvent($"Quest Completed: {questId}", nameof(QuestEvents));
        }

        internal static void RaiseFailQuest(string questId)
        {
            FailQuest?.Invoke(questId);
            SBGDebug.LogEvent($"Quest Failed: {questId}", nameof(QuestEvents));
        }

        internal static void RaiseCompleteObjective(string objectiveId)
        {
            CompleteObjective?.Invoke(objectiveId);
            SBGDebug.LogEvent($"Objective Completed: {objectiveId}", nameof(QuestEvents));
        }
        #endregion
    }

    #region Config Events
    public static class ConfigEvents
    {
        /// <summary>
        /// Event triggered when settings are changed.
        /// </summary>
        public static event Action SettingsChanged;

        /// <summary>
        /// Event triggered when audio settings are changed.
        /// </summary>
        public static event Action AudioSettingsChanged;

        /// <summary>
        /// Event triggered when video settings are changed.
        /// </summary>
        public static event Action VideoSettingsChanged;

        /// <summary>
        /// Event triggered when control settings are changed.
        /// </summary>
        public static event Action ControlSettingsChanged;

        /// <summary>
        /// Event triggered when gameplay settings are changed.
        /// </summary>
        public static event Action GameplaySettingsChanged;

        /// <summary>
        /// Event triggered when language settings are changed.
        /// </summary>
        public static event Action LanguageSettingsChanged;

        internal static void RaiseSettingsChanged()
        {
            SettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Settings Changed", nameof(ConfigEvents));
        }

        internal static void RaiseAudioSettingsChanged()
        {
            AudioSettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Audio Settings Changed", nameof(ConfigEvents));
        }

        internal static void RaiseVideoSettingsChanged()
        {
            VideoSettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Video Settings Changed", nameof(ConfigEvents));
        }

        internal static void RaiseControlSettingsChanged()
        {
            ControlSettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Control Settings Changed", nameof(ConfigEvents));
        }

        internal static void RaiseGameplaySettingsChanged()
        {
            GameplaySettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Gameplay Settings Changed", nameof(ConfigEvents));
        }

        internal static void RaiseLanguageSettingsChanged()
        {
            LanguageSettingsChanged?.Invoke();
            SBGDebug.LogEvent($"Language Settings Changed", nameof(ConfigEvents));
        }
    }
    #endregion
}