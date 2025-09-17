using UnityEngine;
using System.Collections;
using GlobalEvents;

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
        // GameMaster.Instance?.oe_InteractionEvent?.Invoke(this.objectID);
        SauceObjectEvents.RaiseInteractionEvent(this.objectID);

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
            if (CheckKey())
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
        InputHandler.Instance.SetInputState(InputState.Focus);

        string dialogueId = "";
        switch (v)
        {
            case LockedDoorDialogueVariation.LockedKeyNeeded:
                dialogueId = FILE_DIALOGUE_ID_KEYNEEDED;
                break;
            case LockedDoorDialogueVariation.LockedCanLockPick:
                dialogueId = FILE_DIALOGUE_ID_CANLOCKPICK;
                break;
            case LockedDoorDialogueVariation.LockedHasKey:
                dialogueId = FILE_DIALOGUE_ID_HASKEY;
                break;
            case LockedDoorDialogueVariation.StillLocked:
                dialogueId = FILE_DIALOGUE_ID_STILLLOCKED;
                break;
        }

        // Start dialogue using new system
        if (DialogueDisplayController.Instance != null)
        {
            DialogueDisplayController.Instance.StartDialogue(dialogueId);
        }
        else
        {
            SBGDebug.LogError("DialogueDisplayController.Instance is null", "DoorLockable");
            InputHandler.Instance.SetInputState(InputState.FirstPerson);
            yield break;
        }

        bool dialogueEnded = false;
        void OnDialogueEnded()
        {
            dialogueEnded = true;
            // GameMaster.Instance.gm_DialogueEnded.RemoveListener(OnDialogueEnded);
            DialogueEvents.DialogueEnded -= OnDialogueEnded;

            if (v == LockedDoorDialogueVariation.LockedHasKey)
            {
                InputHandler.Instance.SetInputState(InputState.FirstPerson);

                LockDoor(false);
                HandleDoorOpen();
            }
            else if (v == LockedDoorDialogueVariation.LockedCanLockPick && qte_LockPick != null && qte_LockPick.IsPickable())
            {
                SBGDebug.LogInfo("Triggering LockPick QTE after dialogue", "DoorLockable");
                qte_LockPick.Interact();
            }
            else
            {
                InputHandler.Instance.SetInputState(InputState.FirstPerson);
            }
        }
        // GameMaster.Instance.gm_DialogueEnded.AddListener(OnDialogueEnded);
        DialogueEvents.DialogueEnded += OnDialogueEnded;

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
            // GameMaster.Instance.oe_DoorLockEvent?.Invoke(objectID, DoorLockState.Locked);
            DoorEvents.RaiseDoorLockStateChanged(this.objectID, DoorLockState.Locked);

        }
        else
        {
            PlayUnlock_SFX();
            doorLockState = DoorLockState.Unlocked;
            // Trigger unlocked event for objectives
            // GameMaster.Instance.oe_DoorLockEvent?.Invoke(objectID, DoorLockState.Unlocked);
            DoorEvents.RaiseDoorLockStateChanged(this.objectID, DoorLockState.Unlocked);
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
        return false;
    }
    
    public bool LockCheck() // for other scripts to check if door is locked
    {
        return doorLockState == DoorLockState.Locked || doorLockState == DoorLockState.LockedExteriorOnly || doorLockState == DoorLockState.LockedInteriorOnly;
    }
    #endregion
}
#endregion