using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEvents;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance 
    {
        get
        {
            if (_instance == null)
            {
                SBGDebug.LogWarning($"Attempting to access before initialization.", "GameMaster");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    public List<SauceObject> allSauceObjects = new List<SauceObject>();
    public List<SauceObject> allTrackableSauceObjects = new List<SauceObject>();
    public GameObject playerObject; //reference assigned when player is instantiated

    void Awake() 
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            SBGDebug.LogVerbose("GameMaster instance initialized.", "GameMaster | Awake");
            DontDestroyOnLoad(this.gameObject);

            SystemEvents.RaiseGameMasterInitialized();
        }
    }
} 
