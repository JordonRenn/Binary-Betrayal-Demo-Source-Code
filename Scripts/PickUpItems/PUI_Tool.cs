using UnityEngine;

public class PUI_Tool : PickUpItem
{
    [Header("Tool Properties")]
    [Space(10)]

    [SerializeField] private Item_ToolType toolType;
    [SerializeField] private ItemEffect_Tool effectType = ItemEffect_Tool.DisableCamera;
    [SerializeField] private int effectValue = 1;

    protected override void ManuallyCreateItem()
    {
        item = ItemFactory.CreateToolItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, toolType);
    }
}