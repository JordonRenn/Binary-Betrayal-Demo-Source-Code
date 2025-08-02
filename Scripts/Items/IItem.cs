using UnityEngine;

public interface IItem
{
    string ItemId { get; }
    string Name { get; }
    string Description { get; }
    Sprite Icon { get; }
    ItemType Type { get; }
    int weight { get; }
}
