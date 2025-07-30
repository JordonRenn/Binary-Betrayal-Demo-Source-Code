using UnityEngine;

//used to make make creating interactable classes easier to interact with

public class Interactable : MonoBehaviour
{
    public string objectName;

    public virtual void Interact()
    {
        //override this method in inherited classes 
    }

    public string ShowObjectInfo()
    {
        return objectName;
    }
}
