using UnityEngine;
using FMODUnity;
using DG.Tweening;
using System.Collections;

/* 
Only use this class for Misc items that do not fit into other categories and need no extra properties.
 */

public class PickUpItem : SauceObject
{
    [Header("Item Properties")]
    [Space(10)]

    [SerializeField] protected string itemDescription;
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected ItemRarity itemRarity;
    [SerializeField] protected ItemViewLogicType itemViewLogic;
    [SerializeField] protected int weight = 0;
    [SerializeField] protected GameObject itemPrefab;

    [Header("Item Components")]
    [Space(10)]

    [SerializeField] protected Collider pickUpTrigger;
    [SerializeField] protected LayerMask playerLayer;

    [Header("SFX")]
    [Space(10)]

    [SerializeField] protected EventReference sfx_PickUp;

    /* [Header("Animation")]
    [Space(10)] */

    /* [SerializeField] */
    protected float spinSpeed = 120f;
    /* [SerializeField] */
    protected float bobSpeed = 1.25f;
    /* [SerializeField] */
    protected float bobHeight = 0.25f;

    private Vector3 initialPosition;

    void Start()
    {
        // Store the initial position of the itemPrefab for bobbing animation
        if (itemPrefab != null)
        {
            initialPosition = itemPrefab.transform.position;
        }
    }

    void Update()
    {
        AnimateObject();
    }

    protected void OnTriggerEnter(Collider c)
    {
        Debug.Log($"Trigger Entered by: {c.gameObject.name} | Layer: {LayerMask.LayerToName(c.gameObject.layer)}");

        if ((playerLayer.value & (1 << c.gameObject.layer)) != 0)
        {
            PickUp();
        }
    }

    public virtual void PickUp()
    {
        Debug.Log($"Picking up {objectDisplayName}");
        StartCoroutine(PickUpRoutine());
    }

    private IEnumerator PickUpRoutine()
    {
        if (NavCompass.Instance != null)
        {
            NavCompass.Instance.RemoveCompassMarker(this);
        }
        // Play pick-up sound effect
        PlaySFX(sfx_PickUp);

        // Play pick-up animation
        if (itemPrefab != null)
        {
            itemPrefab.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.25f);
        }

        CreateInventoryItem();
        
        GameMaster.Instance?.objective_ItemCollected?.Invoke(objectID, objectDisplayName);

        yield return new WaitForSeconds(0.125f);

        Destroy(gameObject);
    }

    private void CreateInventoryItem()
    {
        Debug.Log($"Creating inventory item: {objectDisplayName} (ID: {objectID}, Type: {itemType}, Weight: {weight})");

        // Create the item data directly
        IItem item = new ItemData(objectID, objectDisplayName, itemDescription, null, itemType, weight, itemRarity, itemViewLogic);

        // Check if InventoryManager exists
        if (InventoryManager.Instance != null)
        {
            // Check if player inventory exists, if not create a temporary one
            if (InventoryManager.Instance.GetPlayerInventory() == null)
            {
                SBGDebug.LogError($"Player inventory is null! Item not added to inventory.", $"PICK UP ITEM: {objectDisplayName}");
                return; // Exit if no inventory is available
            }

            Debug.Log($"Adding item to player inventory: {item.Name}");
            InventoryManager.Instance.AddItemToPlayer(item, 1);
        }
        else
        {
            Debug.LogError("InventoryManager.Instance is null! Cannot add item to inventory.");
        }
    }

    protected void PlaySFX(EventReference path)
    {
        RuntimeManager.PlayOneShot(path, transform.position);
    }

    void AnimateObject()
    {
        if (itemPrefab != null)
        {
            // Rotate the item
            itemPrefab.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

            // Bob the item up and down relative to its initial position
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            itemPrefab.transform.position = new Vector3(
                initialPosition.x,
                initialPosition.y + bobOffset,
                initialPosition.z
            );
        }
    }
}
