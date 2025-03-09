using System.Collections;
using UnityEngine;
using FMODUnity;
public class DoorGeneric : Door
{
    [SerializeField] private Animator doorAnimator;
    
    [SerializeField] private bool int_Locked = false;       //locked when trying to pass from the inside
    [SerializeField] private bool ext_Locked = false;       //locked when trying to pass from the outside

    [SerializeField] private bool alwaysOpenToExt = false;
    [SerializeField] private float doorSpeed = 1f;
    [SerializeField] private float unlockTime = 2f;

    [SerializeField] private LayerMask playerLayer;

    [Header("Door and Lock States")]
    [Space(10)]

    [SerializeField] private DoorLockState defaultLockState;
    public new DoorLockState doorLockState {get; private set;}
    
    [SerializeField] private  DoorState defaultDoorState = DoorState.Closed;
    public new DoorState doorState {get ; private set;}

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
                    Debug.Log("DOORGENERIC | Door is locked from both sides");
                    break;
                case DoorLockState.LockedInteriorOnly:
                    PlayLocked_SFX();
                    Debug.Log("DOORGENERIC | Door is locked from the outside");
                    break;
            }
        }
        else
        {
            switch(doorLockState)
            {
                case DoorLockState.Unlocked:
                    Debug.Log("DOORGENERIC | Door is opening");
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
                    Debug.Log("DOORGENERIC | Door is locked from both sides");
                    break;
                case DoorLockState.LockedExteriorOnly:
                    PlayLocked_SFX();
                    Debug.Log("DOORGENERIC | Door is locked from the inside");
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
            
            Debug.Log($"DOORGENERIC | Door has been unlocked from the {(playerInside ? "inside" : "outside")}");
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
        Debug.Log("Door is closing");
        
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
            Debug.LogError($"DOORGENERIC | Error playing open sfx: {e}");
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
            Debug.LogError($"DOORGENERIC | Error playing close sfx: {e}");
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
            Debug.LogError($"DOORGENERIC | Error playing close sfx: {e}");
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
            Debug.LogError($"DOORGENERIC | Error playing close sfx: {e}");
        }
    }
}
