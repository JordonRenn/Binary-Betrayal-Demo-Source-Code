using UnityEngine;

public class DoorLock : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Interact()
    {
        Debug.Log("Interacting with door lock");
    }
}
