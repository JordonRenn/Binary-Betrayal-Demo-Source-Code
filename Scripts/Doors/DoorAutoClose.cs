using System.Collections;
using UnityEngine;

public class DoorAutoClose : MonoBehaviour
{
    private IDoor door;
    [SerializeField] private float autoCloseTime = 3f;

    void Awake()
    {
        door = GetComponentInParent<IDoor>();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"object {other} has left the door auto close trigger");
        
        if (other.gameObject.layer == LayerMask.NameToLayer("playerObject"))
        {
            Debug.Log("Player has left the door auto close trigger");
            StartCoroutine(AutoClose());
        }
    }
    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseTime);
        try 
        {
            door.CloseDoor();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error closing door: {e}");
        }
    }
}
