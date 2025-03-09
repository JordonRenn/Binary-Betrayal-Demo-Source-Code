using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance {get; private set;}
    
    public List<Trackable> allTrackables = new List<Trackable>(); 

    //OBJECT INSTANTIATION

    //Player Objects
    [HideInInspector] public UnityEvent gm_PlayerSpawned;
    [HideInInspector] public UnityEvent gm_WeaponHudSpawned;
    [HideInInspector] public UnityEvent gm_ReticleSystemSpawned;
    [HideInInspector] public UnityEvent gm_WeaponPoolSpawned;
    [HideInInspector] public UnityEvent gm_FPSMainSpawned;

    //Level Objects
    //
    //
    //

    //Game Play Events
    [HideInInspector] public UnityEvent gm_GamePaused ;
    [HideInInspector] public UnityEvent gm_GameUnpaused;
    [HideInInspector] public UnityEvent gm_ReturnToMainMenu;

    //tick event
    [HideInInspector] public UnityEvent globalTick;


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
