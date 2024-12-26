using System;
using UnityEngine;
using FMODUnity;

public class FPSS_InteractableObject : MonoBehaviour
{
    private enum InteractionType
    {
        None,
        Pickup,
        Interact
    }

    private InteractionType currentInteractionType;
    
    public void AttemptInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);

        //switch expression
        switch (currentInteractionType)
        {
            case InteractionType.Pickup:
                Pickup();
                break;
            case InteractionType.Interact:
                InteractWith();
                break;
            case InteractionType.None:
                Debug.LogError("No interaction type assigned.");
                break;
            default:
                Debug.LogError("Invalid interaction type.");
                break;
        }

    }

    private void InteractWith()
    {
        throw new NotImplementedException();
    }

    private void Pickup()
    {
        throw new NotImplementedException();
    }
}
