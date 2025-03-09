using UnityEngine;

/// <summary>
/// Main script for the FPS system. Used to access all other scripts in the FPS system.
/// </summary>
public class FPSS_Main : MonoBehaviour
{
    public static FPSS_Main Instance {get ; private set;}
    [HideInInspector] public WeaponSlot currentWeaponSlot; //Current weapon slot

    /// <summary>
    /// Initializes the main script.
    /// </summary>
    void Awake()
    {
        Debug.Log("FPS_MAIN | Instantiated");
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Starts the initialization of the main script.
    /// </summary>
    void Start()
    {
        GameMaster.Instance.gm_FPSMainSpawned.Invoke();
    }
}
