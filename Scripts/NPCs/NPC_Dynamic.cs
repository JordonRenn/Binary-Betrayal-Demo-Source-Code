using UnityEngine;

public class NPC_Dynamic : Interactable, INPC
{
    [SerializeField] private string npcId;
    [SerializeField] private string npcName;
    [SerializeField] private string description;
    [SerializeField] private Sprite trackerIcon;
    [SerializeField] private Trackable trackable;

    public string NPCId => npcId;
    public string Name => npcName;
    public string Description => description;
    public Sprite Icon => trackerIcon;
    public Trackable Trackable => trackable;

    // Required compass marker fields from Trackable
    public Sprite compassIcon => trackerIcon;
    public float compassDrawDistance => trackable?.compassDrawDistance ?? 20f;
}