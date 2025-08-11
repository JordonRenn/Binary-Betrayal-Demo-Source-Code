using UnityEngine;

public class PUI_Quest : PickUpItem
{
    [Header("Quest Properties")]
    [Space(10)]

    [SerializeField] private Item_QuestType questType;
    [SerializeField] private string questID = "";
    [SerializeField] private ItemEffect_Quest effectType = ItemEffect_Quest.None;
    [SerializeField] private int effectValue = 1;

    protected override void ManuallyCreateItem()
    {
        item = ItemFactory.CreateQuestItem(objectID, objectDisplayName, itemDescription, itemInventoryIcon, questType, questID);
    }
}