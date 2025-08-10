using UnityEngine;

public class PUI_Material : PickUpItem
{
    [Header("Material Properties")]
    [Space(10)]

    [SerializeField] private Item_MaterialType materialType;
    [SerializeField] private ItemEffect_Material effectType = ItemEffect_Material.CraftMetal;
    [SerializeField] private int effectValue = 10;

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
            item = ItemFactory.CreateMaterialItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, materialType);
        }
    }
}