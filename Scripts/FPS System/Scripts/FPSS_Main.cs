using UnityEngine;

/* 
    First Person Controller Hierarchy:

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)
            - FPS System (FPSS_Main.cs)                                 <--- THIS SCRIPT    
                - FPS_Interaction (FirstPersonInteraction.cs) 
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                   
                    - POS_GUN_AUDIO
                    - 0_0_Ak-47 (Gun_AK47.cs)
                        - AK_47
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        // Need to make "Gun_SniperRifle.cs"
                    - 1_0_HandGun (Gun_HandGun.cs)
                        - HandGun
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 1_1_ShotGun (FPSS_WeaponSlotObject.cs)            // Need to make "Gun_ShotGun.cs"
                    - 2_0_Knife (FPSS_WeaponSlotObject.cs)              // Need to make "Melee_Knife.cs"
                    - 3_0_Grenade (FPSS_WeaponSlotObject.cs)            // Need to make "Grenade.cs"
                    - 3_1_FlashGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "FlashGrenade.cs"
                    - 3_2_SmokeGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "SmokeGrenade.cs"
                    - 4_0_Unarmed (FPSS_WeaponSlotObject.cs)            // Need to make "Unarmed.cs"
 */

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
