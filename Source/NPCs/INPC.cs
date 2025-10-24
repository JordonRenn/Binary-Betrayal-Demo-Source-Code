using UnityEngine;
using SBG;

namespace BinaryBetrayal.NPC
{
    public interface INPC
    {
        string NPCId { get; }
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }
        SauceObject SauceObject { get; }
    }
}