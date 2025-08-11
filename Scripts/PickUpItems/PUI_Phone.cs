using UnityEngine;

public class PUI_Phone : PickUpItem
{
    [Header("Phone Properties")]
    [Space(10)]

    [SerializeField] private Item_PhoneType phoneType;
    [SerializeField] private string phoneNumber = "";
    [SerializeField] private ItemEffect_Phone effectType = ItemEffect_Phone.None;
    [SerializeField] private int effectValue = 1;

    protected override void ManuallyCreateItem()
    {
        item = ItemFactory.CreatePhoneItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, phoneType, phoneNumber);
    }
}