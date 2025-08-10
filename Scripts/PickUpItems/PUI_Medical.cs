using UnityEngine;

public class PUI_Medical : PickUpItem
{
    [Header("Medical Properties")]
    [Space(10)]

    [SerializeField] private Item_MedicalType medicalType;
    [SerializeField] private ItemEffect_Medical effectType = ItemEffect_Medical.Heal;
    [SerializeField] private int effectValue = 50;

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
            item = ItemFactory.CreateMedicalItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, medicalType);
        }
    }
}