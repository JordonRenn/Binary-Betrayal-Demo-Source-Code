using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Enum representing the different weapon slots.
/// </summary>
public enum WeaponSlot
{
    Primary,
    Secondary,
    Knife,
    Utility,
    Unarmed
}

#region FPSS_WeaponPool
/// <summary>
/// Class representing the weapon pool in the FPS system.
/// </summary>
public class FPSS_WeaponPool : MonoBehaviour
{
    public static FPSS_WeaponPool Instance { get; private set; }
    private FPS_InputHandler inputHandler;
    private FPSS_Main main;     
    
    [HideInInspector] public GameObject[][] weaponPool;
    private int poolSize = 5;

    [SerializeField] public GameObject[] primaryWeapons;
    [HideInInspector] public int assignedPrimaryWeaponIndex;
    [SerializeField] public GameObject[] secondaryWeapons;
    [HideInInspector] public int assignedSecondaryWeaponIndex;
    [SerializeField] public GameObject[] knifeWeapons;
    [HideInInspector] public int assignedKnifeWeaponIndex;
    [SerializeField] public GameObject[] utilityWeapons;
    [HideInInspector] public int assignedUtilityWeaponIndex;
    [SerializeField] public GameObject[] unarmedWeapons;
    [HideInInspector] public int assignedUnarmedWeaponIndex;

    public FPSS_WeaponSlotObject currentWeaponSlotObject;
    [HideInInspector] public WeaponSlot currentWeaponSlot;
    WeaponSlot nextWeaponSlot;

    //SUB STATES
    private bool isSwitching = false;
    public bool isReloading = false;
    public bool canReload = true;

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;
    [SerializeField] private float initDelay = 0.2f;
    [SerializeField] private float initTimeout = 10f;
    private bool initialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void SubscribeToEvents()
    {
        FPS_InputHandler.Instance.fireTriggered.AddListener(Fire);
        FPS_InputHandler.Instance.reloadTriggered.AddListener(Reload);
        
        FPS_InputHandler.Instance.activatePrimaryTriggered.AddListener(SelectPrimary);
        FPS_InputHandler.Instance.activateSecondaryTriggered.AddListener(SelectSecondary);
        //FPS_InputHandler.Instance.weaponSlotKnifeTriggered.AddListener(SelectKnife);

        FPS_InputHandler.Instance.swapTriggered.AddListener(SwapPrimarySecondary);
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    #region init
    IEnumerator Init()
    {
        float elapsedTime = 0f;
        
        weaponPool = new GameObject[poolSize][];

        while (FPSS_Main.Instance == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("Weapon Pool initialization timed out: Main instance not found.");
                yield break;
            }
            yield return null;
        }

        main = FPSS_Main.Instance;

        weaponPool[0] = primaryWeapons;
        weaponPool[1] = secondaryWeapons;
        weaponPool[2] = knifeWeapons;
        weaponPool[3] = utilityWeapons;
        weaponPool[4] = unarmedWeapons;

        currentWeaponSlot = WeaponSlot.Unarmed;
        main.currentWeaponSlot = currentWeaponSlot;
        currentWeaponSlotObject = weaponPool[(int)currentWeaponSlot][0].GetComponent<FPSS_WeaponSlotObject>();

        assignedPrimaryWeaponIndex = 0;
        assignedSecondaryWeaponIndex = 0;
        assignedKnifeWeaponIndex = 0;
        assignedUtilityWeaponIndex = 0;
        assignedUnarmedWeaponIndex = 0;

        yield return new WaitForSeconds(initDelay);
        elapsedTime += initDelay;

        while (weaponPool[0] == null || weaponPool[1] == null || weaponPool[2] == null || weaponPool[3] == null || weaponPool[4] == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("Weapon Pool initialization timed out: Weapon pool arrays not initialized.");
                yield break;
            }
            yield return null;
        }

        SubscribeToEvents();
        
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
        
        initialized = true;
        Debug.Log($"WEAPON POOL: Initialization time: {elapsedTime} seconds.");
    }
    #endregion

    #region Update Active Slot
    private IEnumerator UpdateActiveWeaponSlot(WeaponSlot slot)
    {
        if (isSwitching) yield break;
        isSwitching = true;
        nextWeaponSlot = slot;

        switch (slot)
        {
            case WeaponSlot.Primary:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Primary, currentWeaponSlot));
                break;
            case WeaponSlot.Secondary:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Secondary, currentWeaponSlot));
                break;
            case WeaponSlot.Knife:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Knife, currentWeaponSlot));
                break;
            case WeaponSlot.Utility:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Utility, currentWeaponSlot));
                break;
            case WeaponSlot.Unarmed:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Unarmed, currentWeaponSlot));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }

        isSwitching = false;
        yield return null;
    }

    private IEnumerator SwitchWeaponSlot(WeaponSlot nextSlot, WeaponSlot prevSlot)
    {
        yield return currentWeaponSlotObject.SetWeaponInactive();
        yield return ActivateWeaponSlot(nextSlot);
    }

    private IEnumerator ActivateWeaponSlot(WeaponSlot slot)
    {
        int weaponIndex = 0;
        switch (slot)
        {
            case WeaponSlot.Primary:
                weaponIndex = assignedPrimaryWeaponIndex;
                break;
            case WeaponSlot.Secondary:
                weaponIndex = assignedSecondaryWeaponIndex;
                break;
            case WeaponSlot.Knife:
                weaponIndex = assignedKnifeWeaponIndex;
                break;
            case WeaponSlot.Utility:
                weaponIndex = assignedUtilityWeaponIndex;
                break;
            case WeaponSlot.Unarmed:
                weaponIndex = assignedUnarmedWeaponIndex;
                break;
        }
        currentWeaponSlotObject = weaponPool[(int)slot][weaponIndex].GetComponent<FPSS_WeaponSlotObject>();
        yield return currentWeaponSlotObject.SetWeaponActive();
    }
    #endregion

    #region Weapon Select
    public void SelectPrimary()
    {
        if (currentWeaponSlot == WeaponSlot.Primary) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
        }
    }

    public void SelectSecondary()
    {
        if (currentWeaponSlot == WeaponSlot.Secondary) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Secondary));
        }   
    }

    public void SelectKnife()
    {
        if (currentWeaponSlot == WeaponSlot.Knife) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Knife));
        }
    }

    public void SelectUtility()
    {
        if (currentWeaponSlot == WeaponSlot.Utility) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Utility));
        }
    }

    public void SelectUnarmed()
    {
        if (currentWeaponSlot == WeaponSlot.Unarmed) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Unarmed));
        }
    }

    public void SwapPrimarySecondary()
    {
        if (currentWeaponSlot == WeaponSlot.Primary) 
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Secondary));
        }
        else if (currentWeaponSlot == WeaponSlot.Secondary) 
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
        }
    }
    #endregion

    #region Weapon Actions
    /* public void ReloadWeapon()
    {
        if (isReloading) return;
        if (!canReload) return;

        StartCoroutine(ReloadSequence());
    }

    private IEnumerator ReloadSequence()
    {
        yield return currentWeaponSlotObject.Reload();
        isReloading = false;
        canReload = true;
    } */

    /* public void FireWeapon()
    {
        currentWeaponSlotObject.Fire();
    } */

    public void Fire()
    {
        Debug.Log("Fire");
        currentWeaponSlotObject.Fire();
    }

    public void Reload()
    {
        Debug.Log("Reload");
        currentWeaponSlotObject.Reload();
    }
    #endregion
}
#endregion
