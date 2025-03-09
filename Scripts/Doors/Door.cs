using UnityEngine;
using System.Collections;

public class Door : Interactable, IDoor
{
    [HideInInspector] public DoorLockState doorLockState ;

    [HideInInspector] public DoorState doorState ;

    public virtual IEnumerator OpenDoor()
    {
        return null;
    }

    public virtual void CloseDoor()
    {
        //
    }

    public virtual void LockDoor(bool locked)
    {
        //
    }
}
