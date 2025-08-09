using UnityEngine;
using System.Collections;

/* 
INHERITANCE HIERARCHY:
MonoBehaviour
    |
    +-- SauceObject                                 // Nav Tracking + Interactablity
        |
        +-- Door (abstract class)
            |
            +-- DoorGeneric (concrete class)        // Just Open/Close logic + Player Position Consideration
                |
                +-- DoorLockable (concrete class)   // Adds Lock/Unlock logic
 */
#region Door Lockable
public class DoorLockable : DoorGeneric
{
    [Header("Key Properties")]
    [Space(10)]

    [Tooltip("If true, a key is required and cannot be lockpicked. If false, the door can be lockpicked.")]
    [SerializeField] private bool isKeyRequired = false;
    [SerializeField] private string keyId = "";
    [SerializeField] private KeyType keyType = KeyType.Key;
    private float unlockTime = 1f;

    [Header("Lock State")]
    [Space(10)]

    [SerializeField] private DoorLockState defaultLockState;

    private const string FILE_DIALOGUE_ID_KEYNEEDED = "door_locked_keyNeeded_firstTime";
    private const string FILE_DIALOGUE_ID_STILLLOCKED = "door_locked_stillLocked";
    private const string FILE_DIALOGUE_ID_CANLOCKPICK = "door_locked_canLockPick_firstTime";
    private const string FILE_DIALOGUE_ID_HASKEY = "door_locked_hasKey_firstTime";

    private LockPickingQuickTimeEvent qte_LockPick;
    private bool hasBeenInteractedWith = false;
    private bool playerHasKey = false;

    void Awake()
    {
        doorState = defaultDoorState;
        doorLockState = defaultLockState;

        qte_LockPick = GetComponentInChildren<LockPickingQuickTimeEvent>();
        if (qte_LockPick != null)
        {
            //
        }
    }

    #region Interaction
    public override void Interact()
    {
        // If door is open, close it
        if (doorState != DoorState.Closed)
        {
            CloseDoor();
            return;
        }

        // If door is unlocked, open it
        if (doorLockState == DoorLockState.Unlocked)
        {
            HandleDoorOpen();
            return;
        }

        // Door is locked - handle locked door interaction
        HandleLockedDoorInteraction();
    }

    private void HandleLockedDoorInteraction()
    {
        if (!hasBeenInteractedWith)
        {
            HandleFirstTimeInteraction();
        }
        else
        {
            HandleSubsequentInteraction();
        }
    }

    private void HandleFirstTimeInteraction()
    {
        hasBeenInteractedWith = true;

        if (isKeyRequired)
        {
            var dialogueVariation = CheckKey() ?
                LockedDoorDialogueVariation.LockedHasKey :
                LockedDoorDialogueVariation.LockedKeyNeeded;

            StartCoroutine(DoorLockDialogueSequence(dialogueVariation));
        }
        else
        {
            StartCoroutine(DoorLockDialogueSequence(LockedDoorDialogueVariation.LockedCanLockPick));
        }
    }

    private void HandleSubsequentInteraction()
    {
        if (isKeyRequired)
        {
            CheckKey();
            if (playerHasKey)
            {
                HandleDoorOpen();
            }
            // If key is required but player doesn't have it, do nothing
        }
        else
        {
            // Key not required, allow lock picking
            if (qte_LockPick != null && qte_LockPick.IsPickable())
            {
                SBGDebug.LogInfo("Triggering LockPick QTE", "DoorLockable");
                qte_LockPick.Interact();
            }
        }
    }
    #endregion

    #region Dialogue
    // Only used first  time the door is interacted with
    public IEnumerator DoorLockDialogueSequence(LockedDoorDialogueVariation v)
    {
        // Lock input during dialogue to prevent interference
        FPS_InputHandler.Instance.SetInputState(InputState.LockedInteraction);

        switch (v)
        {
            case LockedDoorDialogueVariation.LockedKeyNeeded:
                DialogueBox.Instance.LoadDialogue(FILE_DIALOGUE_ID_KEYNEEDED);
                break;
            case LockedDoorDialogueVariation.LockedCanLockPick:
                DialogueBox.Instance.LoadDialogue(FILE_DIALOGUE_ID_CANLOCKPICK);
                break;
            case LockedDoorDialogueVariation.LockedHasKey:
                DialogueBox.Instance.LoadDialogue(FILE_DIALOGUE_ID_HASKEY);
                break;
        }

        // Open the dialogue box to display the loaded dialogue
        DialogueBox.Instance.OpenDialogueBox();

        // Wait until the dialogue ends, then trigger appropriate action
        bool dialogueEnded = false;
        void OnDialogueEnded()
        {
            dialogueEnded = true;
            GameMaster.Instance.gm_DialogueEnded.RemoveListener(OnDialogueEnded);

            // Handle different dialogue variations
            if (v == LockedDoorDialogueVariation.LockedHasKey)
            {
                FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);

                // Player has key, unlock and open the door
                LockDoor(false); // Unlock the door
                HandleDoorOpen(); // Open the door
            }
            else if (v == LockedDoorDialogueVariation.LockedCanLockPick && qte_LockPick != null && qte_LockPick.IsPickable())
            {
                // Only trigger lock picking for the LockedCanLockPick variation
                SBGDebug.LogInfo("Triggering LockPick QTE after dialogue", "DoorLockable");
                qte_LockPick.Interact();
            }
            else
            {
                // For LockedKeyNeeded, just restore input state without triggering lock picking
                FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
            }
        }
        GameMaster.Instance.gm_DialogueEnded.AddListener(OnDialogueEnded);

        // Wait until dialogueEnded is true
        yield return new WaitUntil(() => dialogueEnded);
    }
    #endregion


    #region Public Methods
    public override void LockDoor(bool locked)
    {
        if (locked)
        {
            doorLockState = DoorLockState.Locked;
            // Trigger locked event for objectives
            GameMaster.Instance.objective_DoorLocked?.Invoke(objectDisplayName, keyId);
        }
        else
        {
            PlayUnlock_SFX();
            doorLockState = DoorLockState.Unlocked;
            // Trigger unlocked event for objectives
            GameMaster.Instance.objective_DoorUnlocked?.Invoke(objectDisplayName, keyId);
        }
    }
    
    public bool CheckKey()
    {
        if (isKeyRequired)
        {
            var id = "key_" + keyId;
            if (InventoryManager.Instance.playerInventory.HasItemById(id, 1))
            {
                playerHasKey = true;
                SBGDebug.LogInfo($"Key {id} found in inventory", $"class: DoorLockable | object: {objectDisplayName}");
                return true;
            }
            else
            {
                SBGDebug.LogInfo($"Key {id} not found in inventory", $"class: DoorLockable | {objectDisplayName}");
            }
        }
        return playerHasKey;
    }
    
    public bool LockCheck() // for other scripts to check if door is locked
    {
        return doorLockState == DoorLockState.Locked || doorLockState == DoorLockState.LockedExteriorOnly || doorLockState == DoorLockState.LockedInteriorOnly;
    }
    #endregion
}
#endregion