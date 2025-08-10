using UnityEngine;

public class PUI_Key: PickUpItem
{
    [Header("Key Properties")]
    [Space(10)]

    [SerializeField] private KeyType keyType = KeyType.Key;
    [SerializeField] private ItemEffect_Key effectType = ItemEffect_Key.UnlockDoor;
    [SerializeField] private int effectValue = 1;

    protected override void CreateItem()
    {
        // Try to create from database first
        if (ItemFactory.ItemExists(objectID))
        {
            item = ItemFactory.CreateItemFromDatabase(objectID, itemInventoryIcon);
        }
        else
        {
            // Fallback to manual creation if not in database
            item = ItemFactory.CreateKeyItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, keyType);
        }
    }
}
