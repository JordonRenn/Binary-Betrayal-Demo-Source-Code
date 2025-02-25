using System.Collections;
using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class DoorGeneric : Interactable, IDoor
{
    [SerializeField] private Animator doorAnimator;
    
    [SerializeField] private bool int_Locked = false;       //locked when trying to pass from the inside
    [SerializeField] private bool ext_Locked = false;       //locked when trying to pass from the outside

    [SerializeField] private bool alwaysOpenToExt = false;
    [SerializeField] private float doorSpeed = 1f;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private  DoorState defaultDoorState = DoorState.Closed;

    [SerializeField] private EventReference sfx_Open;
    [SerializeField] private EventReference sfx_Close;
    [SerializeField] private Transform audioPosition;
    [SerializeField] private float unlockTime = 0.4f;

    private bool playerInside = false; //is player on the interior side of the door
    public DoorState doorState {get ; private set;}

    public enum DoorState
    {
        OpenExt, //door swung open to the outside
        OpenInt, //door swung open to the inside
        Closed
    }

    void Awake() 
    {
       doorState = defaultDoorState;
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

    public IEnumerator OpenDoor()
    {
        if (playerInside) //opening from the inside
        {
            if (int_Locked)
            {
                //play locked sound
                Debug.Log("Door is locked from the outside");
                yield break;
            }
            else if (ext_Locked && !int_Locked)
            {
                //play unlock sound
                ext_Locked = false;
                Debug.Log("Door has been unlocked from the inside");

                try
                {
                    NotificationSystem.Instance.DisplayNotification(new Notification("Door unlocked", NotificationType.Normal));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"DOORGENERIC.CS | Error: {e}");
                }

                yield return new WaitForSeconds(unlockTime);
            }
            
            //play open sound
            Debug.Log("Door is opening");
            doorState = DoorState.OpenExt;
            doorAnimator.SetTrigger("OpenExt");
        }
        else //openinh from the outside
        {
            if (ext_Locked)
            {
                //play locked sound
                Debug.Log("Door is locked from the inside");
                yield break;
            }
            else if (int_Locked && !ext_Locked)
            {
                //play unlock sound
                int_Locked = false;
                Debug.Log("Door has been unlocked from the outside");

                try
                {
                    NotificationSystem.Instance.DisplayNotification(new Notification("Door unlocked", NotificationType.Normal));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"DOORGENERIC.CS | Error: {e}");
                }

                yield return new WaitForSeconds(unlockTime);
            }

            //play open sound
            Debug.Log("Door is opening");
            if (alwaysOpenToExt)
            {
                doorState = DoorState.OpenExt;
                doorAnimator.SetTrigger("OpenExt");
            }
            else
            {
                doorState = DoorState.OpenInt;
                doorAnimator.SetTrigger("OpenInt");
            }
        }
    }

    public void CloseDoor()
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

    public void LockDoor(bool locked)
    {
        int_Locked = locked;
        ext_Locked = locked;
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
            Debug.LogError($"Error playing open sfx: {e}");
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
            Debug.LogError($"Error playing close sfx: {e}");
        }
    }
}
