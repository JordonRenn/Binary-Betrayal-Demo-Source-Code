using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* 
    First Person Controller Hierarchy:

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs) 
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                   <--- THIS SCRIPT
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

public class FPSS_Pool : MonoBehaviour
{
    private static FPSS_Pool _instance;
    public static FPSS_Pool Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(FPSS_Pool)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private FPSS_Main main;

    [SerializeField] private GameObject[] primaryWeaponObjects;
    [SerializeField] private GameObject[] secondaryWeaponObjects;
    [SerializeField] private GameObject[] meleeWeaponObjects;
    [SerializeField] private GameObject[] utilityWeaponObjects;

    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> PrimaryObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> SecondaryObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> MeleeObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> UtilityObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();

    public FPSS_WeaponSlotObject assignedPrimaryWPO { get; private set; }
    public FPSS_WeaponSlotObject assignedSecondaryWPO { get; private set; }
    public FPSS_WeaponSlotObject assignedMeleeWPO { get; private set; }
    public FPSS_WeaponSlotObject assignedUtilityWPO { get; private set; }

    public FPSS_WeaponSlotObject currentActiveWPO { get; private set; }

    //SUB STATES
    public bool isSwitching { get; private set; }

    [SerializeField] WeaponSlot defaultActiveSlot;              //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
    [HideInInspector] public WeaponSlot currentWeaponSlot;

    // Events
    [Header("Weapon Events")]
    [Space(10)]
    public UnityEvent<WeaponSlot> onWeaponSwitchStarted;
    public UnityEvent<WeaponSlot> onWeaponSwitchCompleted;
    public UnityEvent<WeaponSlot> onWeaponSwitchFailed;


    public bool initialized { get; private set; }

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
        if (primaryWeaponObjects == null || primaryWeaponObjects.Length == 0)
            Debug.LogError($"{nameof(FPSS_Pool)}: Primary weapon objects array is missing!");
        if (secondaryWeaponObjects == null || secondaryWeaponObjects.Length == 0)
            Debug.LogError($"{nameof(FPSS_Pool)}: Secondary weapon objects array is missing!");
        if (meleeWeaponObjects == null || meleeWeaponObjects.Length == 0)
            Debug.LogError($"{nameof(FPSS_Pool)}: Melee weapon objects array is missing!");
        if (utilityWeaponObjects == null || utilityWeaponObjects.Length == 0)
            Debug.LogError($"{nameof(FPSS_Pool)}: Utility weapon objects array is missing!");
    }

    #region Initialization
    private void Awake()
    {
        // Initialize as singleton and persist across scenes since weapons need to be maintained
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            // Validate required arrays
            ValidateRequiredComponents();
        }

        isSwitching = false;
        initialized = false;

        foreach (GameObject wgo in primaryWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            PrimaryObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in secondaryWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            SecondaryObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in meleeWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            MeleeObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in utilityWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            UtilityObjects.Add(wso.refID, wso);
        }
    }

    void Start()
    {
        AssignPrimaryWPO(WeaponRefID.Rifle);        //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
        AssignSecondaryWPO(WeaponRefID.Handgun);    //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
        AssignMeleeWPO(WeaponRefID.Unarmed);        //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
        AssignUtilityWPO(WeaponRefID.Unarmed);      //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
        currentActiveWPO = assignedMeleeWPO;        //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY

        SubscribeToEvents();
        GameMaster.Instance.gm_WeaponPoolSpawned.Invoke();

        StartCoroutine(UpdateActiveWeaponSlot(defaultActiveSlot));
    }
    #endregion

    #region WPO Assignment
    void AssignPrimaryWPO(WeaponRefID id)
    {
        assignedPrimaryWPO = PrimaryObjects[id];
    }

    void AssignSecondaryWPO(WeaponRefID id)
    {
        assignedSecondaryWPO = SecondaryObjects[id];
    }

    void AssignMeleeWPO(WeaponRefID id)
    {
        assignedMeleeWPO = MeleeObjects[id];
    }

    void AssignUtilityWPO(WeaponRefID id)
    {
        assignedUtilityWPO = UtilityObjects[id];
    }
    #endregion

    #region Event Subcription
    void SubscribeToEvents()
    {
        InputHandler.Instance.OnFireInput.AddListener(Fire);
        InputHandler.Instance.OnReloadInput.AddListener(Reload);

        InputHandler.Instance.OnSlot1Input.AddListener(SelectPrimary);
        InputHandler.Instance.OnSlot2Input.AddListener(SelectSecondary);
        InputHandler.Instance.OnMeleeInput.AddListener(SelectMelee);        
        // probably will be renamed to Tools or something different idfk 
        //InputHandler.Instance.OnEquipmentInput.AddListener(SelectUtility);
        //InputHandler.Instance.OnUnarmedInput.AddListener(SelectUnarmed);

        InputHandler.Instance.OnSwapInput.AddListener(SwapPrimarySecondary);

        onWeaponSwitchStarted.AddListener((slot) =>
        {
            Debug.Log($"Starting weapon switch to {slot}");
            // do stuff..
        });

        onWeaponSwitchCompleted.AddListener((slot) =>
        {
            Debug.Log($"Completed weapon switch to {slot}");
            // do stuff..
        });

        onWeaponSwitchFailed.AddListener((slot) =>
        {
            Debug.Log($"Failed to switch to {slot}");
            // do stuff..
        });

        Debug.Log("FPS_WEAPONPOOL | Subcribed to input events");
    }
    #endregion

    #region Weapon Actions

    private bool CanSwitchToWeapon(WeaponSlot targetSlot)
    {
        if (isSwitching)
        {
            onWeaponSwitchFailed?.Invoke(targetSlot);
            Debug.Log($"Cannot switch weapons: Already switching");
            return false;
        }

        if (currentWeaponSlot == targetSlot)
        {
            Debug.Log($"Already using {targetSlot} weapon");
            return false;
        }

        return true;
    }

    public void Fire()
    {
        Debug.Log("Fire");
        currentActiveWPO.Fire();
    }

    public void Reload()
    {
        Debug.Log("Reload");
        currentActiveWPO.Reload();
    }
    #endregion

    #region Weapon Selection
    private void SelectPrimary()
    {
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
    }

    private void SelectSecondary()
    {
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Secondary));
    }

    private void SelectMelee()
    {
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Melee));
    }

    private void SelectUtility()
    {
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Utility));
    }
    #endregion



    #region Weapon Switching
    private void SwapPrimarySecondary()
    {
        WeaponSlot targetSlot = currentWeaponSlot switch
        {
            WeaponSlot.Primary => WeaponSlot.Secondary,
            WeaponSlot.Secondary => WeaponSlot.Primary,
            _ => WeaponSlot.Primary
        };

        StartCoroutine(UpdateActiveWeaponSlot(targetSlot));
    }

    private IEnumerator UpdateActiveWeaponSlot(WeaponSlot slot)
    {
        if (!CanSwitchToWeapon(slot)) yield break;

        isSwitching = true;
        onWeaponSwitchStarted?.Invoke(slot);

        bool switchSuccessful = true;
        try
        {
            // Start the switching process
            currentActiveWPO.SetWeaponInactive();
            ActivateWeaponSlot(slot);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error switching weapons: {e.Message}");
            onWeaponSwitchFailed?.Invoke(slot);
            switchSuccessful = false;
        }
        finally
        {
            isSwitching = false;
        }

        if (switchSuccessful)
        {
            yield return StartCoroutine(SwitchWeaponSlot(slot));
            onWeaponSwitchCompleted?.Invoke(slot);
        }
    }

    private IEnumerator SwitchWeaponSlot(WeaponSlot nextSlot)
    {
        yield return currentActiveWPO.SetWeaponInactive();
        yield return ActivateWeaponSlot(nextSlot);
    }

    private IEnumerator ActivateWeaponSlot(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Primary:
                currentActiveWPO = assignedPrimaryWPO;
                break;
            case WeaponSlot.Secondary:
                currentActiveWPO = assignedSecondaryWPO;
                break;
            case WeaponSlot.Melee:
                currentActiveWPO = assignedMeleeWPO;
                break;
            case WeaponSlot.Utility:
                currentActiveWPO = assignedUtilityWPO;
                break;
        }
        yield return currentActiveWPO.SetWeaponActive();
        FPSS_Main.Instance.currentWeaponSlot = slot;
    }
    #endregion
}