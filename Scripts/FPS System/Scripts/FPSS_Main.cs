using UnityEngine;

/// <summary>
/// Main script for the FPS system. Used to access all other scripts in the FPS system.
/// </summary>
public class FPSS_Main : MonoBehaviour
{
    private static FPSS_Main _instance;
    public static FPSS_Main Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(FPSS_Main)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [HideInInspector] public WeaponSlot currentWeaponSlot; //Current weapon slot

    /// <summary>
    /// Initializes the main script.
    /// </summary>
    void Awake()
    {
        // Initialize as singleton and persist across scenes since it manages core FPS functionality
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            Debug.Log("FPS_MAIN | Initialized successfully");
            ValidateRequiredComponents();
        }
    }

    /// <summary>
    /// Starts the initialization of the main script.
    /// </summary>
    void Start()
    {
        GameMaster.Instance.gm_FPSMainSpawned.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateRequiredComponents();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    private void ValidateRequiredComponents()
    {
        // Add any component validations needed for FPSS_Main
        // For now, just ensure we're not in an invalid state
        if (currentWeaponSlot < WeaponSlot.Primary || currentWeaponSlot > WeaponSlot.Utility)
        {
            Debug.LogWarning($"{nameof(FPSS_Main)}: Invalid weapon slot state detected. Resetting to Primary.");
            currentWeaponSlot = WeaponSlot.Primary;
        }
    }
}
