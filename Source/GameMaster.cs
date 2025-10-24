using System.Collections.Generic;
using UnityEngine;
using BinaryBetrayal.GlobalEvents;

namespace SBG
{
    public static class GameMaster
    {
        public static List<SauceObject> allSauceObjects = new List<SauceObject>();
        public static List<SauceObject> allTrackableSauceObjects = new List<SauceObject>();
        public static GameObject playerObject; //reference assigned when player is instantiated

        static GameMaster()
        {
            /* SystemEvents.RaiseGameMasterInitialized(); */
        }
    }
}