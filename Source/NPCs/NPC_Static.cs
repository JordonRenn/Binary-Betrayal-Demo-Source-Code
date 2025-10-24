using UnityEngine;
using SBG;

namespace BinaryBetrayal.NPC
{
    public class NPC_Static : SauceObject, INPC
    {
        [SerializeField] private string npcId;
        [SerializeField] private string npcName;
        [SerializeField] private string description;

        public string NPCId => npcId;
        public string Name => npcName;
        public string Description => description;
        public Sprite Icon => nav_CompassIcon;
        public SauceObject SauceObject => this;

        // Compass marker properties are inherited from SauceObject
        public Sprite compassIcon => nav_CompassIcon;
        public float compassDrawDistance => nav_CompassDrawDistance;
    }
}