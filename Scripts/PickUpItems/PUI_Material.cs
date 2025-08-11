using UnityEngine;

public class PUI_Material : PickUpItem
{
    [Header("Material Properties")]
    [Space(10)]

    [SerializeField] private Item_MaterialType materialType;
    [SerializeField] private ItemEffect_Material effectType = ItemEffect_Material.CraftMetal;
    [SerializeField] private int effectValue = 10;

    protected override void ManuallyCreateItem()
    {
        item = ItemFactory.CreateMaterialItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, materialType);
    }
}