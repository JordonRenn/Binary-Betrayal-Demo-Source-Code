using UnityEngine;

public class PUI_Food : PickUpItem
{
    [Header("Food Properties")]
    [Space(10)]

    [SerializeField] private Item_FoodType foodType;
    [SerializeField] private ItemEffect_Food effectType = ItemEffect_Food.BoostHealth;
    [SerializeField] private int effectValue = 15;

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
            item = ItemFactory.CreateFoodItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, foodType);
        }
    }
}