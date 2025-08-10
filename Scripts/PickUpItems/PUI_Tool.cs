using UnityEngine;

public class PUI_Tool : PickUpItem
{
    [Header("Tool Properties")]
    [Space(10)]

    [SerializeField] private Item_ToolType toolType;
    [SerializeField] private ItemEffect_Tool effectType = ItemEffect_Tool.DisableCamera;
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
            item = ItemFactory.CreateToolItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, toolType);
        }
    }
}