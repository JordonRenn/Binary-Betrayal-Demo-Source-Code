using System.Collections;
using UnityEngine;
using FMODUnity;
public class DoorGeneric : Door
{
    [SerializeField] private Animator doorAnimator;

    [SerializeField] private bool alwaysOpenToExt = false;
    [SerializeField] private float unlockTime = 2f;

    [Header("Door and Lock States")]
    [Space(10)]

    [SerializeField] private DoorLockState defaultLockState;
    [SerializeField] private  DoorState defaultDoorState = DoorState.Closed;

    [Header("FMOD")]
    [Space(10)]

    [SerializeField] private EventReference sfx_Open;
    [SerializeField] private EventReference sfx_Close;
    [SerializeField] private EventReference sfx_Locked;
    [SerializeField] private EventReference sfx_Unlock;
    [SerializeField] private Transform audioPosition;

    //SUBSTATES
    private bool playerInside = false; //is player on the interior side of the door

    void Awake() 
    {
       doorState = defaultDoorState;
       doorLockState = defaultLockState;
    }
    
    public override void Interact()
    {
        if (doorState == DoorState.Closed)
        {
            StartCoroutine(OpenDoor());
        }
        else
        {
            CloseDoor();
        }
    }

    private void HandleDoorOpen(bool isPlayerInside)
    {
        if (isPlayerInside)
        {
            switch (doorLockState)
            {
                case DoorLockState.Unlocked:
                    OpenDoor(true);
                    doorState = DoorState.OpenExt;
                    break;
                case DoorLockState.Locked:
                    PlayLocked_SFX();
                    SBGDebug.LogInfo("Door is locked from both sides", "DOORGENERIC");
                    break;
                case DoorLockState.LockedInteriorOnly:
                    PlayLocked_SFX();
                    SBGDebug.LogInfo("Door is locked from the outside", "DOORGENERIC");
                    break;
            }
        }
        else
        {
            switch(doorLockState)
            {
                case DoorLockState.Unlocked:
                    SBGDebug.LogInfo("Door is opening", "DOORGENERIC");
                    if (alwaysOpenToExt)
                    {
                        OpenDoor(true);
                        doorState = DoorState.OpenExt;
                    }
                    else 
                    {
                        OpenDoor(false);
                        doorState = DoorState.OpenInt;
                    }
                    doorState = DoorState.OpenExt;
                    break;
                case DoorLockState.Locked:
                    PlayLocked_SFX();
                    SBGDebug.LogInfo("Door is locked from both sides", "DOORGENERIC");
                    break;
                case DoorLockState.LockedExteriorOnly:
                    PlayLocked_SFX();
                    SBGDebug.LogInfo("Door is locked from the inside", "DOORGENERIC");
                    break;
            }
        }
    }

    public override IEnumerator OpenDoor()
    {
        if (playerInside && doorLockState == DoorLockState.LockedExteriorOnly ||
            !playerInside && doorLockState == DoorLockState.LockedInteriorOnly)
        {
            PlayUnlock_SFX();
            yield return new WaitForSeconds(unlockTime);
            
            SBGDebug.LogInfo($"Door has been unlocked from the {(playerInside ? "inside" : "outside")}", "DOORGENERIC");
            doorLockState = DoorLockState.Unlocked;

            if (alwaysOpenToExt)
            {
                OpenDoor(true);
                doorState = DoorState.OpenExt;
            }
            else 
            {
                OpenDoor(!playerInside);
                doorState = playerInside ? DoorState.OpenExt : DoorState.OpenInt;
            }
        }
        else
        {
            HandleDoorOpen(playerInside);
        }
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
        SBGDebug.LogInfo("Door is closing", "DOORGENERIC");
        
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

    //TRIGGER TO DETECT PLAYER SIDE OF THE DOOR

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("playerObject"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("playerObject"))
        {
            playerInside = false;
        }
    }

    public void PlayOpen_SFX()
    {
        try
        {
            RuntimeManager.PlayOneShot(sfx_Open, audioPosition.position);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Error playing open sfx: {e}", "DOORGENERIC");
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
            SBGDebug.LogError($"Error playing close sfx: {e}", "DOORGENERIC");
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
            SBGDebug.LogError($"Error playing close sfx: {e}", "DOORGENERIC");
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
            SBGDebug.LogError($"Error playing close sfx: {e}", "DOORGENERIC");
        }
    }
}
