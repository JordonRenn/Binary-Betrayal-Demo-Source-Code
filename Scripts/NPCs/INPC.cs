using UnityEngine;

public interface INPC
{
    string NPCId { get; }
    string Name { get; }
    string Description { get; }
    Sprite Icon { get; }
    Trackable Trackable { get; }
}
