using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance {get; private set;}
    
    public List<Trackable> allTrackables = new List<Trackable>(); 

    [HideInInspector] public UnityEvent gm_PlayerSpawned;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
