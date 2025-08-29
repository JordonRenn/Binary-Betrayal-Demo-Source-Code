using UnityEngine;
using FMODUnity;
using DG.Tweening;
using System.Collections;
using System;

/* 
Only use this class for Misc items that do not fit into other categories and need no extra properties.
 */

public class PickUpItem : SauceObject
{
    [Header("Pick-Up Item Properties")]
    [Space(10)]

    [SerializeField] protected string itemID;
    [SerializeField] protected string itemDisplayName;
    [SerializeField] protected string itemDescription;
    [SerializeField] protected GameObject item3dIcon;
    [SerializeField] protected Sprite itemInventoryIcon;
    
    // Default fallback values
    protected ItemType itemType = ItemType.Misc;
    protected float itemWeight = 1.0f;
    protected float itemValue = 0.0f;
    protected bool isUsable = false;
    protected bool isEquippable = false;
    protected bool isQuestItem = false;

    private protected LayerMask playerLayer;
    private const string PLAYER_LAYER_NAME = "playerObject";

    private protected EventReference sfx_PickUp;
    private const string SFX_REFERENCE = "event:/Player/player_PickUp";

    private protected Collider pickUpTrigger;

    protected float spinSpeed = 120f;
    protected float bobSpeed = 1.25f;
    protected float bobHeight = 0.25f;

    private Vector3 initialPosition;
    protected IItem item;
    private const float PLAYER_ANIMATION_RENDER_DISTANCE = 30f;
    private bool playerInRange = false;

    protected virtual void Start()
    {
        // Store the initial position of the item3dIcon for bobbing animation
        if (item3dIcon != null)
        {
            initialPosition = item3dIcon.transform.position;
        }

        // Create item for misc items using ItemFactory
        CreateItem();

        sfx_PickUp = EventReference.Find(SFX_REFERENCE);
        playerLayer = LayerMask.GetMask(PLAYER_LAYER_NAME);
        pickUpTrigger = GetComponent<Collider>();

        GameMaster.Instance?.globalTick.AddListener(() =>
        {
            playerInRange = CheckPlayerDistance();
        });
    }

    void Update()
    {
        if (playerInRange) AnimateObject();
    }

    private bool CheckPlayerDistance()
    {
        Vector3 playerPosition = GameMaster.Instance.playerObject.transform.position;
        return Vector3.Distance(transform.position, playerPosition) < PLAYER_ANIMATION_RENDER_DISTANCE;
    }

    protected void CreateItem()
    {
        // Try to create from database first
        if (ItemFactory.ItemExists(itemID))
        {
            item = ItemFactory.CreateItemFromDatabase(itemID, itemInventoryIcon);
        }
        else
        {
            SBGDebug.LogWarning($"Item with ID {itemID} does not exist in the database. Creating a fallback item.", $"PickUpItem | CreateItem");
            ManuallyCreateItem();
        }
    }

    protected virtual void ManuallyCreateItem()
    {
        // Create an ItemData instance using the new constructor
        ItemData itemData = new ItemData(
            itemID,
            itemDisplayName,
            itemDescription,
            itemValue,
            itemWeight,
            itemType,
            itemInventoryIcon,
            isUsable,
            isEquippable,
            isQuestItem
        );

        // Create the IItem from this ItemData
        item = ItemFactory.CreateItem(itemData);
        
        if (item == null)
        {
            SBGDebug.LogError($"Failed to create item {itemID} manually.", $"PickUpItem | ManuallyCreateItem");
        }
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
        Debug.Log($"Picking up {itemDisplayName}");
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
        if (item3dIcon != null)
        {
            item3dIcon.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.25f);
        }

        CreateInventoryItem();

        yield return new WaitForSeconds(0.125f);

        Destroy(gameObject);
    }

    private void CreateInventoryItem()
    {
        Debug.Log($"Creating inventory item: {itemDisplayName} (ID: {itemID})");

        // Check if InventoryManager exists
        if (InventoryManager.Instance != null)
        {
            // Check if player inventory exists, if not create a temporary one
            if (InventoryManager.Instance.GetPlayerInventory() == null)
            {
                SBGDebug.LogError($"Player inventory is null! Item not added to inventory.", $"PICK UP ITEM: {itemDisplayName}");
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
        if (item3dIcon != null)
        {
            // Rotate the item
            item3dIcon.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

            // Bob the item up and down relative to its initial position
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            item3dIcon.transform.position = new Vector3(
                initialPosition.x,
                initialPosition.y + bobOffset,
                initialPosition.z
            );
        }
    }
}
