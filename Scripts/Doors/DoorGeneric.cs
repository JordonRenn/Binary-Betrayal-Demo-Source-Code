using System.Collections;
using UnityEngine;
using FMODUnity;

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

MORE INFO:

    - Open/Close SFX are triggered within the animation clips.
 */

[RequireComponent(typeof(Animator))]
public class DoorGeneric : Door
{
    [Header("Door Generic Properties")]
    [Space(10)]

    [SerializeField] private bool alwaysOpenToExt = false;
    /* [SerializeField] private float unlockTime = 2f; */
    [SerializeField] protected DoorState defaultDoorState = DoorState.Closed;

    [Header("Audio References")]
    [Space(10)]

    [SerializeField] private EventReference sfx_Open;
    [SerializeField] private EventReference sfx_Close;
    [SerializeField] private EventReference sfx_Locked;
    [SerializeField] private EventReference sfx_Unlock;
    [SerializeField] private Transform audioPosition;

    private GameObject player;
    private Animator doorAnimator;

    void Awake()
    {
        doorState = defaultDoorState;
    }

    void Start()
    {
        player = GameMaster.Instance.playerObject;
        if (player == null)
        {
            SBGDebug.LogWarning("Player object not found in GameMaster on instanciation, will try again when interacted with ", "DoorGeneric");
        }

        doorAnimator = GetComponent<Animator>();
        if (doorAnimator == null)   
        {
            SBGDebug.LogError("Animator component missing from DoorGeneric", "DoorGeneric");
        }
    }

    #region Door Actions
    public override void Interact()
    {
        SBGDebug.LogInfo("Door Interacted with", "DoorGeneric");

        if (doorState == DoorState.Closed)
        {
            HandleDoorOpen();
        }
        else
        {
            CloseDoor();
        }
    }

    protected void HandleDoorOpen()
    {
        bool isPlayerInside = IsPlayerInside();

        if (alwaysOpenToExt)
        {
            // Always open to exterior regardless of player position
            OpenDoor(true);
            doorState = DoorState.OpenExt;
        }
        else
        {
            // Open away from player (default behavior)
            if (isPlayerInside)
            {
                // Player is inside, open to exterior (away from player)
                OpenDoor(true);
                doorState = DoorState.OpenExt;
            }
            else
            {
                // Player is outside, open to interior (away from player)
                OpenDoor(false);
                doorState = DoorState.OpenInt;
            }
        }
    }

    public override IEnumerator OpenDoor()
    {
        HandleDoorOpen();
        return null;
    }

    private void OpenDoor(bool exterior)
    {
        if (exterior)
        {
            doorAnimator.SetTrigger("OpenExt");
            doorState = DoorState.OpenExt;
        }
        else
        {
            doorAnimator.SetTrigger("OpenInt");
            doorState = DoorState.OpenInt;
        }
    }

    public override void CloseDoor()
    {
        //play close sound
        SBGDebug.LogInfo("Door is closing", "DoorGeneric");

        if (doorState == DoorState.OpenExt)
        {
            doorAnimator.SetTrigger("CloseExt");
            doorState = DoorState.Closed;
        }
        else
        {
            doorAnimator.SetTrigger("CloseInt");
            doorState = DoorState.Closed;
        }
    }
    #endregion

    #region Player Locator
    private bool IsPlayerInside()
    {
        if (player == null)
        {
            player = GameMaster.Instance.playerObject;
            if (player == null)
            {
                SBGDebug.LogWarning("Player object not found in GameMaster", "DoorGeneric");
                return false;
            }
        }

        if (player != null)
        {
            // Convert player's world position to door's local space
            Vector3 playerLocalPosition = transform.InverseTransformPoint(player.transform.position);

            if (playerLocalPosition.x > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // If player is still null for some reason, return false
        return false;
    }

    #region Audio
    public void PlayOpen_SFX()
    {
        try
        {
            RuntimeManager.PlayOneShot(sfx_Open, audioPosition.position);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Error playing open sfx: {e}", "DoorGeneric");
        }
    }

    public void PlayClose_SFX()
    {
        try
        {
            RuntimeManager.PlayOneShot(sfx_Close, audioPosition.position);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Error playing close sfx: {e}", "DoorGeneric");
        }
    }

    public void PlayLocked_SFX()
    {
        try
        {
            RuntimeManager.PlayOneShot(sfx_Locked, audioPosition.position);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Error playing locked sfx: {e}", "DoorGeneric");
        }
    }

    public void PlayUnlock_SFX()
    {
        try
        {
            RuntimeManager.PlayOneShot(sfx_Unlock, audioPosition.position);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Error playing unlock sfx: {e}", "DoorGeneric");
        }
    }
    #endregion
}
#endregion