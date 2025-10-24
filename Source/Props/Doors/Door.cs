using UnityEngine;
using System.Collections;
using SBG;

namespace BinaryBetrayal.Props
{
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

    //public class Door : Interactable
    public abstract class Door : SauceObject
    {
        [HideInInspector] public DoorLockState doorLockState;

        [HideInInspector] public DoorState doorState;

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
}