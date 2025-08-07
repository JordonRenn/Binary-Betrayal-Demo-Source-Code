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

public class DoorLockable : DoorGeneric
{
    [Header("Key Properties")]
    [Space(10)]

    [SerializeField] private bool isKeyRequired = false;
    [SerializeField] private string keyId = "";
    [SerializeField] private KeyType keyType = KeyType.Key;
    [SerializeField] private float unlockTime = 2f;

    [Header("Door and Lock States")]
    [Space(10)]

    [SerializeField] private DoorLockState defaultLockState;

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

    public override void Interact()
    {
        if (doorState == DoorState.Closed)
        {
            if (doorLockState == DoorLockState.Unlocked)
            {
                StartCoroutine(OpenDoor());
            }

            if (doorLockState == DoorLockState.Locked)
            {
                if (hasBeenInteractedWith)
                {
                    if (isKeyRequired)
                    {
                        CheckKey();
                    }
                    if (playerHasKey)
                    {
                        StartCoroutine(OpenDoor());
                    }
                    else
                    {
                        if (qte_LockPick != null && qte_LockPick.IsPickable())
                        {
                            SBGDebug.LogInfo("Triggering LockPick QTE", "DOORGENERIC");
                            qte_LockPick.Interact();
                        }
                    }
                }
                else if (!hasBeenInteractedWith)
                {
                    hasBeenInteractedWith = true;
                    if (isKeyRequired)
                    {
                        if (CheckKey())
                        {
                            StartCoroutine(DoorLockDialogueSequence(LockedDoorDialogueVariation.LockedHasKey));
                        }
                        else
                        {
                            StartCoroutine(DoorLockDialogueSequence(LockedDoorDialogueVariation.LockedKeyNeeded));
                        }
                    }
                    else
                    {
                        StartCoroutine(DoorLockDialogueSequence(LockedDoorDialogueVariation.LockedCanLockPick));
                    }
                }
            }
        }
        else
        {
            CloseDoor();
        }
    }

    private bool CheckKey()
    {
        if (isKeyRequired)
        {
            var id = "key_" + keyId;
            if (InventoryManager.Instance.playerInventory.HasItemById(id, 1))
            {
                playerHasKey = true;
                SBGDebug.LogInfo($"Key {id} found in inventory", $"class: DoorGeneric | object: {objectDisplayName}");
                return true;
            }
            else
            {
                SBGDebug.LogInfo($"Key {id} not found in inventory", $"class: DoorGeneric | {objectDisplayName}");
            }
        }
        return playerHasKey;
    }

    // Only used first  time the door is interacted with
    public IEnumerator DoorLockDialogueSequence(LockedDoorDialogueVariation v)
    {
        switch (v)
        {
            case LockedDoorDialogueVariation.LockedKeyNeeded:
                DialogueBox.Instance.LoadDialogue("door_locked_keyNeeded_firstTime");
                break;
            case LockedDoorDialogueVariation.LockedCanLockPick:
                DialogueBox.Instance.LoadDialogue("door_locked_canLockPick_firstTime");
                break;
            case LockedDoorDialogueVariation.LockedHasKey:
                DialogueBox.Instance.LoadDialogue("door_locked_hasKey_firstTime");
                break;
        }

        // Wait until the dialogue ends, then trigger lockpick interaction
        bool dialogueEnded = false;
        void OnDialogueEnded()
        {
            dialogueEnded = true;
            GameMaster.Instance.gm_DialogueEnded.RemoveListener(OnDialogueEnded);
            if (qte_LockPick != null && qte_LockPick.IsPickable())
            {
                SBGDebug.LogInfo("Triggering LockPick QTE after dialogue", "DOORGENERIC");
                qte_LockPick.Interact();
            }
            {
                qte_LockPick.Interact();
            }
        }
        GameMaster.Instance.gm_DialogueEnded.AddListener(OnDialogueEnded);

        // Wait until dialogueEnded is true
        yield return new WaitUntil(() => dialogueEnded);


    }

    public override void LockDoor(bool locked)
    {
        //int_Locked = locked;
        //ext_Locked = locked;

        if (locked)
        {
            doorLockState = DoorLockState.Locked;
        }
        else
        {
            PlayUnlock_SFX();
            doorLockState = DoorLockState.Unlocked;
        }
    }
}
