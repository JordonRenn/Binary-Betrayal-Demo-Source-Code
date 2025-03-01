using UnityEngine;

public class PUI_Key: PickUpItem
{
    [SerializeField] private int keyID = 0000;

    public override void PickUp() 
    {
        Debug.Log($"Picked up {ItemName} | KeyID: {keyID}");
        PlaySFX(sfx_PickUp);
        Destroy(gameObject);
    }


}
