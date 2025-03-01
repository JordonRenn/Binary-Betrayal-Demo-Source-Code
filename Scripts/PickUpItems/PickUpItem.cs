using UnityEngine;
using FMODUnity;
using DG.Tweening;

public class PickUpItem : Trackable
{
    [SerializeField] protected int itemID;
    
    [Header("Item Properties")]
    [Space(10)]
    
    [SerializeField] protected string itemName;
    [SerializeField] protected string itemDescription;
    [SerializeField] protected Sprite itemIcon;

    [Header("Item Components")]
    [Space(10)]

    [SerializeField] protected Collider pickUpTrigger;
    [SerializeField] protected LayerMask playerLayer;

    [Header("SFX")]
    [Space(10)]

    [SerializeField] protected EventReference sfx_PickUp;

    [Header("Animation")]
    [Space(10)]

    [SerializeField] protected float spinSpeed = 180f;
    [SerializeField] protected float bobSpeed = 0.5f;
    [SerializeField] protected float bobHeight = 0.5f;

    public int ItemID { get => itemID; }
    public string ItemName { get => itemName; }
    public string ItemDescription { get => itemDescription; }
    public Sprite ItemIcon { get => itemIcon; }

    void Update () 
    {
        AnimateObject();
    }
    
    protected void OnCollisionEnter(Collision c) 
    {
        if (c.gameObject.layer == playerLayer) {
            PickUp();
        }
    }

    public virtual void PickUp() 
    {
        Debug.Log($"Picked up {ItemName}");
        PlaySFX(sfx_PickUp);
        Destroy(gameObject);
    }

    protected void PlaySFX(EventReference path) 
    {
        RuntimeManager.PlayOneShot(path, transform.position);
    }

    void AnimateObject() 
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight, transform.position.z);
    }
}
