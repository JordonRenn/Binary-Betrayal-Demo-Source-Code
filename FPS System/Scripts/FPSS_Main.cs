using UnityEngine;

/// <summary>
/// Main script for the FPS system. Used to access all other scripts in the FPS system.
/// </summary>
public class FPSS_Main : MonoBehaviour
{
    public static FPSS_Main Instance {get ; private set;}
    
    [SerializeField] private FPSS_WeaponPool weaponPool;
    private FPS_InputHandler input;
    private FPSS_Interaction interaction;
    /* [HideInInspector] */ public WeaponSlot currentWeaponSlot; //Current weapon slot

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;            //Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f;    //used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f;   //initialization timeout
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    /// <summary>
    /// Initializes the main script.
    /// </summary>
    void Awake()
    {
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
        input = FPS_InputHandler.Instance;
        weaponPool = FPSS_WeaponPool.Instance;
        interaction = FPSS_Interaction.Instance;
    }
}
