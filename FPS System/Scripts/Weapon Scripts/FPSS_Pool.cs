using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSS_Pool : MonoBehaviour
{
    public static FPSS_Pool Instance { get; private set; }
    private FPSS_Main main;

    [SerializeField] private GameObject[] primaryWeaponObjects;
    [SerializeField] private GameObject[] secondaryWeaponObjects;
    [SerializeField] private GameObject[] meleeWeaponObjects;
    [SerializeField] private GameObject[] utilityWeaponObjects;

    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> PrimaryObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> SecondaryObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> MeleeObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>(); 
    private Dictionary<WeaponRefID, FPSS_WeaponSlotObject> UtilityObjects = new Dictionary<WeaponRefID, FPSS_WeaponSlotObject>();

    public FPSS_WeaponSlotObject assignedPrimaryWPO {get; private set;}
    public FPSS_WeaponSlotObject assignedSecondaryWPO {get; private set;}
    public FPSS_WeaponSlotObject assignedMeleeWPO {get; private set;}
    public FPSS_WeaponSlotObject assignedUtilityWPO {get; private set;}

    public FPSS_WeaponSlotObject currentActiveWPO {get; private set;}

    //SUB STATES
    public bool isSwitching {get; private set;}
    public bool isReloading {get; private set;}

    [SerializeField] WeaponSlot defaultActiveSlot;              //TEMP FOR DEV/DEBUG --- CREATE SYSTEM TO SELECT EXTERNALLY
    [HideInInspector] public WeaponSlot currentWeaponSlot;

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private float initDelay = 0.2f;
    [SerializeField] private float initTimeout = 10f;
    public bool initialized  {get; private set;}

    #region Initialization
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        isReloading = false;
        isSwitching = false;
        initialized = false;

        foreach (GameObject wgo in  primaryWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            PrimaryObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in  secondaryWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            SecondaryObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in  meleeWeaponObjects)
        {
            var wso = wgo.GetComponent<FPSS_WeaponSlotObject>();
            MeleeObjects.Add(wso.refID, wso);
        }

        foreach (GameObject wgo in  utilityWeaponObjects)
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
        FPS_InputHandler.Instance.fireTriggered.AddListener(Fire);
        FPS_InputHandler.Instance.reloadTriggered.AddListener(Reload);
        
        FPS_InputHandler.Instance.activatePrimaryTriggered.AddListener(SelectPrimary);
        FPS_InputHandler.Instance.activateSecondaryTriggered.AddListener(SelectSecondary);
        //FPS_InputHandler.Instance.weaponSlotKnifeTriggered.AddListener(SelectMelee);          //update input handler before implementation
        //FPS_InputHandler.Instance.weaponSlotKnifeTriggered.AddListener(SelectUtility);        //update input handler before implementation

        FPS_InputHandler.Instance.swapTriggered.AddListener(SwapPrimarySecondary);

        Debug.Log("FPS_WEAPONPOOL | Subcribed to input events");
    }
    #endregion

    #region Weapon Actions
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
        if (currentWeaponSlot == WeaponSlot.Primary) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Primary));
        }
    }

    private void SelectSecondary()
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

    private void SelectMelee()
    {
        if (currentWeaponSlot == WeaponSlot.Melee) 
        {
            return;
        }
        else
        {
            StartCoroutine(UpdateActiveWeaponSlot(WeaponSlot.Melee));
        }
    }

    private void SelectUtility()
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
    #endregion



    #region Weapon Switching
    private void SwapPrimarySecondary()
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
    
    private IEnumerator UpdateActiveWeaponSlot(WeaponSlot slot)
    {
        if (isSwitching) yield break;
        isSwitching = true;
        WeaponSlot nextWeaponSlot = slot;

        switch (slot)
        {
            case WeaponSlot.Primary:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Primary));
                break;
            case WeaponSlot.Secondary:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Secondary));
                break;
            case WeaponSlot.Melee:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Melee));
                break;
            case WeaponSlot.Utility:
                yield return StartCoroutine(SwitchWeaponSlot(WeaponSlot.Utility));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }

        isSwitching = false;
        yield return null;
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