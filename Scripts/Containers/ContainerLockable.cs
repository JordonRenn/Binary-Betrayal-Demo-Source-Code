using UnityEngine;
using System.Collections;
using GlobalEvents;
using BinaryBetrayal.InputManagement;

#region Container Lockable
public class ContainerLockable : ContainerBase
{
    [Header("Key Properties")]
    [Space(10)]

    [SerializeField] private bool isKeyRequired = false;
    [SerializeField] private string keyId = "";
    [SerializeField] private DoorLockState lockState = DoorLockState.Locked;

    private const string FILE_DIALOGUE_ID_KEYNEEDED = "container_locked_keyNeeded_firstTime";
    private const string FILE_DIALOGUE_ID_STILLLOCKED = "container_locked_stillLocked";
    private const string FILE_DIALOGUE_ID_CANLOCKPICK = "container_locked_canLockPick_firstTime";
    private const string FILE_DIALOGUE_ID_HASKEY = "container_locked_hasKey_firstTime";


    private LockPickingQuickTimeEvent qte_LockPick;
    private bool hasBeenInteractedWith = false;

    void Awake()
    {
        qte_LockPick = GetComponentInChildren<LockPickingQuickTimeEvent>();
        if (qte_LockPick == null)
        {
            Debug.LogError("LockPickingQuickTimeEvent component is missing on the ContainerLockable object.");
        }
    }

    #region Interaction
    public override void Interact()
    {
        // GameMaster.Instance?.oe_InteractionEvent?.Invoke(this.objectID);
        SauceObjectEvents.RaiseInteractionEvent(this.objectID);


        if (lockState == DoorLockState.Unlocked)
        {
            HandleContainerOpen();
        }
        else
        {
            HandleLockedContainerInteraction();
        }
    }

    void HandleLockedContainerInteraction()
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

    void HandleFirstTimeInteraction()
    {
        hasBeenInteractedWith = true;

        if (isKeyRequired)
        {
            var dialogueVariation = CheckKey() ?
                LockedDoorDialogueVariation.LockedHasKey :
                LockedDoorDialogueVariation.LockedKeyNeeded;

            StartCoroutine(ContainerLockDialogueSequence(dialogueVariation));
        }
        else
        {
            StartCoroutine(ContainerLockDialogueSequence(LockedDoorDialogueVariation.LockedCanLockPick));
        }
    }

    void HandleSubsequentInteraction()
    {
        if (isKeyRequired)
        {
            if (CheckKey())
            {
                HandleContainerOpen();
            }
        }
        else
        {
            if (qte_LockPick != null && qte_LockPick.IsPickable())
            {
                qte_LockPick.Interact();
            }
        }
    }
    #endregion

    #region Actions
    void HandleContainerOpen()
    {
        // Open the Inventory menu and Display this container's contents
    }
    #endregion

    #region Dialogue
    public IEnumerator ContainerLockDialogueSequence(LockedDoorDialogueVariation v)
    {
        // Lock input during dialogue to prevent interference
        InputSystem.SetInputState(InputState.Focus);

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
            SBGDebug.LogError("DialogueDisplayController.Instance is null", "ContainerLockable");
            InputSystem.SetInputState(InputState.FirstPerson);
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
                InputSystem.SetInputState(InputState.FirstPerson);

                HandleContainerOpen();
            }
            else if (v == LockedDoorDialogueVariation.LockedCanLockPick && qte_LockPick != null && qte_LockPick.IsPickable())
            {
                qte_LockPick.Interact();
            }
            else
            {
                InputSystem.SetInputState(InputState.FirstPerson);
            }
        }
        // GameMaster.Instance.gm_DialogueEnded.AddListener(OnDialogueEnded);
        DialogueEvents.DialogueEnded += OnDialogueEnded;

        yield return new WaitUntil(() => dialogueEnded);
    }
    #endregion

    #region Helper Methods
    public bool CheckKey()
    {
        if (isKeyRequired)
        {
            var id = "key_" + keyId;
            if (InventoryManager.Instance.playerInventory.HasItemById(id, 1))
            {
                // SBGDebug.LogInfo($"Key {id} found in inventory", $"class: DoorLockable | object: {objectDisplayName}");
                return true;
            }
            else
            {
                // SBGDebug.LogInfo($"Key {id} not found in inventory", $"class: DoorLockable | {objectDisplayName}");
            }
        }
        return false;
    }
    #endregion
}
#endregion