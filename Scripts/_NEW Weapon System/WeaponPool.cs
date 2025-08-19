using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region WEAPON POOL
public class WeaponPool : MonoBehaviour
{
    private static WeaponPool _instance;
    public static WeaponPool Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("WeaponPool instance is not initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    // EVENTS
    [HideInInspector] public UnityEvent onWeaponActivationComplete;
    [HideInInspector] public UnityEvent onWeaponDeactivationComplete;

    // Gets filled during LoadWeaponInventory
    private List<WeaponData> weaponDataCache;

    // Assigned during weapon pool filling
    public FPSS_WeaponSlotObject primaryWSO { get; private set; }
    public WPO_Gun primaryWeaponComponent { get; private set; }
    public FPSS_WeaponSlotObject secondaryWSO { get; private set; }
    public WPO_Gun secondaryWeaponComponent { get; private set; }
    public FPSS_WeaponSlotObject meleeWSO { get; private set; }
    public FPSS_WeaponSlotObject utilityWSO { get; private set; }
    public FPSS_WeaponSlotObject unarmedWSO { get; private set; }

    private WeaponSlot defaultActiveSlot = WeaponSlot.Secondary; //SHOULD BE UNARMED BUT NEED TO IMPLEMENT IN UNITY FIRST
    public FPSS_WeaponSlotObject activeWSO { get; private set; } 
    public WeaponSlot activeSlot { get; private set; }  = WeaponSlot.Secondary; //SHOULD BE UNARMED BUT NEED TO IMPLEMENT IN UNITY FIRST

    private bool initialized = false;
    public bool poolFilled { get; private set; } = false;
    private bool isSwitching = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            StartCoroutine(Init());
        }
        else
        {
            Debug.LogWarning("Multiple instances of WeaponPool detected. Destroying this duplicate.");
            Destroy(this);
        }
    }

    #region Initialization
    private IEnumerator Init()
    {
        yield return new WaitUntil(() => WeaponInventory.inventoryLoaded);
        LoadWeaponInventory();
        yield return new WaitUntil(() => poolFilled);
        
        // Set the initial active weapon based on defaultActiveSlot
        yield return StartCoroutine(ActivateWeaponSlot(defaultActiveSlot));
        
        SubscribeToEvents();
        initialized = true;
    }

    private void LoadWeaponInventory()
    {
        weaponDataCache = WeaponInventory.GetCurrentWeaponDatas();

        FillWeaponPool();
    }

    private void FillWeaponPool()
    {
        bool primaryAssigned = false;
        bool secondaryAssigned = false;
        bool meleeAssigned = false;
        bool utilityAssigned = false;
        bool unarmedAssigned = false;

        foreach (WeaponData weaponData in weaponDataCache)
        {
            switch (weaponData.slot)
            {
                case WeaponSlot.Primary:
                    if (primaryAssigned) continue; // Skip if already assigned, but continue to process other weapons
                    var weaponInstance = Instantiate(weaponData.prefab);
                    if (weaponInstance == null)
                    {
                        SBGDebug.LogError($"Failed to instantiate prefab for {weaponData.displayName}. Check if the prefab is assigned correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    weaponInstance.transform.SetParent(transform);
                    weaponInstance.transform.localPosition = new Vector3(0, -0.6f, 0);
                    weaponInstance.transform.localRotation = Quaternion.Euler(0, 21, 0);
                    primaryWSO = weaponInstance.GetComponent<FPSS_WeaponSlotObject>();
                    primaryWSO.weaponData = weaponData;
                    if (primaryWSO == null)
                    {
                        SBGDebug.LogError($"Failed to get FPSS_WeaponSlotObject component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    primaryWeaponComponent = primaryWSO.GetComponent<WPO_Gun>();
                    if (primaryWeaponComponent == null)
                    {
                        SBGDebug.LogError($"Failed to get WPO_Gun component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    primaryAssigned = true;
                    break;
                case WeaponSlot.Secondary:
                    if (secondaryAssigned) continue;
                    var secondaryInstance = Instantiate(weaponData.prefab);
                    if (secondaryInstance == null)
                    {
                        SBGDebug.LogError($"Failed to instantiate prefab for {weaponData.displayName}. Check if the prefab is assigned correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    secondaryInstance.transform.SetParent(transform);
                    secondaryInstance.transform.localPosition = new Vector3(0, -0.6f, 0);
                    secondaryInstance.transform.localRotation = Quaternion.Euler(0, 21, 0);
                    secondaryWSO = secondaryInstance.GetComponent<FPSS_WeaponSlotObject>();
                    secondaryWSO.weaponData = weaponData;
                    if (secondaryWSO == null)
                    {
                        SBGDebug.LogError($"Failed to get FPSS_WeaponSlotObject component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    secondaryWeaponComponent = secondaryWSO.GetComponent<WPO_Gun>();
                    if (secondaryWeaponComponent == null)
                    {
                        SBGDebug.LogError($"Failed to get WPO_Gun component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    secondaryAssigned = true;
                    break;
                case WeaponSlot.Melee:
                    if (meleeAssigned) continue;
                    var meleeInstance = Instantiate(weaponData.prefab);
                    if (meleeInstance == null)
                    {
                        SBGDebug.LogError($"Failed to instantiate prefab for {weaponData.displayName}. Check if the prefab is assigned correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    meleeInstance.transform.SetParent(transform);
                    meleeInstance.transform.localPosition = new Vector3(0, -0.6f, 0);
                    meleeInstance.transform.localRotation = Quaternion.Euler(0, 21, 0);
                    meleeWSO = meleeInstance.GetComponent<FPSS_WeaponSlotObject>();
                    meleeWSO.weaponData = weaponData;
                    if (meleeWSO == null)
                    {
                        SBGDebug.LogError($"Failed to get FPSS_WeaponSlotObject component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    meleeAssigned = true;
                    break;
                case WeaponSlot.Utility:
                    if (utilityAssigned) continue;
                    var utilityInstance = Instantiate(weaponData.prefab);
                    if (utilityInstance == null)
                    {
                        SBGDebug.LogError($"Failed to instantiate prefab for {weaponData.displayName}. Check if the prefab is assigned correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    utilityInstance.transform.SetParent(transform);
                    utilityInstance.transform.localPosition = new Vector3(0, -0.6f, 0);
                    utilityInstance.transform.localRotation = Quaternion.Euler(0, 21, 0);
                    utilityWSO = utilityInstance.GetComponent<FPSS_WeaponSlotObject>();
                    utilityWSO.weaponData = weaponData;
                    if (utilityWSO == null)
                    {
                        SBGDebug.LogError($"Failed to get FPSS_WeaponSlotObject component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    utilityAssigned = true;
                    break;
                case WeaponSlot.Unarmed:
                    if (unarmedAssigned) continue;
                    var unarmedInstance = Instantiate(weaponData.prefab);
                    if (unarmedInstance == null)
                    {
                        SBGDebug.LogError($"Failed to instantiate prefab for {weaponData.displayName}. Check if the prefab is assigned correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    unarmedInstance.transform.SetParent(transform);
                    unarmedInstance.transform.localPosition = new Vector3(0, -0.6f, 0);
                    unarmedInstance.transform.localRotation = Quaternion.Euler(0, 21, 0);
                    unarmedWSO = unarmedInstance.GetComponent<FPSS_WeaponSlotObject>();
                    unarmedWSO.weaponData = weaponData;
                    if (unarmedWSO == null)
                    {
                        SBGDebug.LogError($"Failed to get FPSS_WeaponSlotObject component for {weaponData.displayName}. Check if the component is attached correctly.", "WeaponPool | FillWeaponPool");
                        continue;
                    }
                    unarmedAssigned = true;
                    break;
            }
        }

        poolFilled = true;
    }

    private static void SubscribeToEvents()
    {
        InputHandler.Instance.OnFireInput.AddListener(Instance.Fire);
        InputHandler.Instance.OnReloadInput.AddListener(Instance.Reload);

        InputHandler.Instance.OnSlot1Input.AddListener(() => Instance.SelectPrimary());
        InputHandler.Instance.OnSlot2Input.AddListener(() => Instance.SelectSecondary());
        InputHandler.Instance.OnMeleeInput.AddListener(() => Instance.SelectMelee());
        InputHandler.Instance.OnUtilityInput.AddListener(() => Instance.SelectUtility());
        InputHandler.Instance.OnUnarmedInput.AddListener(() => Instance.SelectUnarmed());

        InputHandler.Instance.OnSwapInput.AddListener(() => Instance.SwapPrimarySecondary());
    }
    #endregion

    #region Weapon Actions
    public void Fire()
    {
        Debug.Log("Fire");
        activeWSO.Fire();
    }

    public void Reload()
    {
        Debug.Log("Reload");
        activeWSO.Reload();
    }
    #endregion

    #region Weapon Selection
    private void SelectPrimary()
    {
        if (primaryWSO == null) return;
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
    }

    private void SelectSecondary()
    {
        if (secondaryWSO == null) return;
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Secondary));
    }

    private void SelectMelee()
    {
        if (meleeWSO == null) return;
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Melee));
    }

    private void SelectUtility()
    {
        if (utilityWSO == null) return;
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Utility));
    }

    private void SelectUnarmed()
    {
        if (unarmedWSO == null) return;
        StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Unarmed));
    }
    #endregion

    #region Weapon Switching
    private void SwapPrimarySecondary()
    {
        WeaponSlot targetSlot = activeSlot switch
        {
            WeaponSlot.Primary => WeaponSlot.Secondary,
            WeaponSlot.Secondary => WeaponSlot.Primary,
            _ => WeaponSlot.Primary
        };

        StartCoroutine(UpdateActiveWeaponSlot(targetSlot));
    }

    private IEnumerator UpdateActiveWeaponSlot(WeaponSlot slot)
    {
        if (isSwitching) yield break;

        isSwitching = true;

        bool switchSuccessful = true;
        try
        {
            activeWSO.SetWeaponInactive();
            ActivateWeaponSlot(slot);
        }
        catch (Exception e)
        {
            SBGDebug.LogError($"Failed to switch weapon slot: {e.Message}", "WeaponPool | UpdateActiveWeaponSlot");
            switchSuccessful = false;
        }
        finally
        {
            isSwitching = false;
        }

        if (switchSuccessful)
        {
            yield return StartCoroutine(ActivateWeaponSlot(slot));
        }
    }

    private IEnumerator ActivateWeaponSlot(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Primary:
                activeWSO = primaryWSO;
                activeSlot = WeaponSlot.Primary;
                break;
            case WeaponSlot.Secondary:
                activeWSO = secondaryWSO;
                activeSlot = WeaponSlot.Secondary;
                break;
            case WeaponSlot.Melee:
                activeWSO = meleeWSO;
                activeSlot = WeaponSlot.Melee;
                break;
            case WeaponSlot.Utility:
                activeWSO = utilityWSO;
                activeSlot = WeaponSlot.Utility;
                break;
            case WeaponSlot.Unarmed:
                activeWSO = unarmedWSO;
                activeSlot = WeaponSlot.Unarmed;
                break;
        }
        yield return activeWSO.SetWeaponActive();
        //FPSS_Main.Instance.currentWeaponSlot = slot; trying to remove FPSS_Main from codebase if possible
    }
    #endregion
}
#endregion