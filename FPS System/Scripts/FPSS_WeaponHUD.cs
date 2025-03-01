using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Manages the weapon HUD, displaying the current weapon information.
/// </summary>
public class FPSS_WeaponHUD : MonoBehaviour
{
    public static FPSS_WeaponHUD Instance { get; private set; }
    
    //private FPSS_Main FPSS_Main.Instance; // Used to see what weapon slot is currently active
    //private FPSS_WeaponPool weaponPool; // Used to see what gun is currently assigned to each slot
    private GameObject player;

    private WPO_Gun primaryWeaponComponent;
    private WPO_Gun secondaryWeaponComponent;
    
    [Header("Primary Weapon Objects")]
    [Space(10)] 

    [SerializeField] private TMP_Text primaryNameText;
    [SerializeField] private TMP_Text primarySlotNumber; //DO NOT CHANGE THIS TEXT, only color
    [SerializeField] private TMP_Text primaryAmmoText;
    [SerializeField] private Image primaryIconImg;

    [Header("Secondary Weapon Objects")]
    [Space(10)]

    [SerializeField] private TMP_Text secondaryNameText;
    [SerializeField] private TMP_Text secondarySlotNumber; //DO NOT CHANGE THIS TEXT, only color 
    [SerializeField] private TMP_Text secondaryAmmoText;
    [SerializeField] private Image secondaryIconImg;

    [Header("Text Colors")]
    [Space(10)]

    [SerializeField] private Color color_NameActiveText;
    [SerializeField] private Color color_NameInactiveText;
    [SerializeField] private Color color_SlotActiveText;
    [SerializeField] private Color color_SlotInactiveText;
    [SerializeField] private Color color_AmmoActiveText;
    [SerializeField] private Color color_AmmoInactiveText;

    [Header("DEV OPTIONS")]
    [SerializeField] private bool debugMode; // Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f; // Used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f; // Initialization timeout
    private bool initialized = false; // Flag used to stop Update() from running before initialization is complete
    private bool hidden = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    void Start()
    {
        StartCoroutine(Init());
    }

    /// <summary>
    /// Initializes the weapon HUD by waiting for necessary instances and data to be available.
    /// </summary>
    IEnumerator Init()
    {
        float initTime = Time.time;

        while (player == null && Time.time - initTime < initTimeout)
        {
            //Debug.Log("WEAPON HUD | looking for Player");

            player = GameObject.FindWithTag("Player");
            yield return null;
        }

        while (FPSS_WeaponPool.Instance.weaponPool[0] == null || FPSS_WeaponPool.Instance.weaponPool[1] == null)
        {
            if (Time.time - initTime < initTimeout)
            {
                Debug.LogError("WEAPON HUD | Initialization timed out: Weapon pool arrays not initialized.");
                yield break;
            }
            yield return null;
        }

        while (FPSS_WeaponPool.Instance.weaponPool[0][FPSS_WeaponPool.Instance.assignedPrimaryWeaponIndex] == null || FPSS_WeaponPool.Instance.weaponPool[1][FPSS_WeaponPool.Instance.assignedSecondaryWeaponIndex] == null)

        {
            if (Time.time - initTime < initTimeout)
            {
                Debug.LogError("WEAPON HUD | Initialization timed out: Primary or Secondary weapon not assigned.");
                yield break;
            }
            yield return null;
        }

        CacheWeaponComponents();

        Debug.Log($"WEAPON HUD: Initialization time: {Time.time - initTime} seconds.");

        primaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);
        secondaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);

        initialized = true;

        //yield return new WaitForSeconds(initDelay);

        RefreshWeaponHUD();
        //GameMaster.Instance.gm_PlayerSpawned.AddListener(RefreshWeaponHUD);
    }

    private void UpdateAmmoCount()
    {
        primaryAmmoText.SetText($"{primaryWeaponComponent.currentClip} / {primaryWeaponComponent.clipSize}");
        secondaryAmmoText.SetText($"{secondaryWeaponComponent.currentClip} / {secondaryWeaponComponent.clipSize}");
    }

    private void CacheWeaponComponents()
    {
        primaryWeaponComponent = FPSS_WeaponPool.Instance.weaponPool[0][FPSS_WeaponPool.Instance.assignedPrimaryWeaponIndex].GetComponent<WPO_Gun>();
        secondaryWeaponComponent = FPSS_WeaponPool.Instance.weaponPool[1][FPSS_WeaponPool.Instance.assignedSecondaryWeaponIndex].GetComponent<WPO_Gun>();
    }

    /// <summary>
    /// Refreshes the weapon HUD to display the currently active weapon slot and its respective weapon; do not call to refresh Icon Sprites, use 'UpdateIconSprite' directly instead
    /// </summary>
    public void RefreshWeaponHUD()
    {
        Debug.Log("WEAPON HUD: Refreshing elements...");
        CacheWeaponComponents();            
        
        switch (FPSS_Main.Instance.currentWeaponSlot)
        {
            case WeaponSlot.Primary:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + FPSS_Main.Instance.currentWeaponSlot);
                
                primaryNameText.color = color_NameActiveText;
                primarySlotNumber.color = color_SlotActiveText;
                primaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.color = color_NameInactiveText;
                secondarySlotNumber.color = color_SlotInactiveText;
                secondaryAmmoText.color = color_AmmoInactiveText;
                
                primaryNameText.text = primaryWeaponComponent.weaponName;
                break;
            case WeaponSlot.Secondary:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + FPSS_Main.Instance.currentWeaponSlot);
                
                primaryNameText.color = color_NameInactiveText;
                primarySlotNumber.color = color_SlotInactiveText;
                primaryAmmoText.color = color_AmmoInactiveText;
                
                secondaryNameText.color = color_NameActiveText;
                secondarySlotNumber.color = color_SlotActiveText;
                secondaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.text = secondaryWeaponComponent.weaponName;
                break;
            case WeaponSlot.Unarmed:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + FPSS_Main.Instance.currentWeaponSlot);
                
                primaryNameText.color = color_NameInactiveText;
                primarySlotNumber.color = color_SlotInactiveText;
                primaryAmmoText.color = color_AmmoInactiveText;
                
                secondaryNameText.color = color_NameInactiveText;
                secondarySlotNumber.color = color_SlotInactiveText;
                secondaryAmmoText.color = color_AmmoInactiveText;
                
                primaryNameText.text = "Unarmed";
                secondaryNameText.text = "Unarmed";
                break;
        }

        UpdateIconSprites();
    }

    /// <summary>
    /// Updates the icon sprites for the weapon slots. Call when the WeaponSlot is assigned a different weapon, not when the WeaponSlot is changed.
    /// </summary>
    void UpdateIconSprites()
    {
        FPSS_WeaponSlotObject primaryWeaponObject = FPSS_WeaponPool.Instance.weaponPool[0][FPSS_WeaponPool.Instance.assignedPrimaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();
        FPSS_WeaponSlotObject secondaryWeaponObject =  FPSS_WeaponPool.Instance.weaponPool[1][FPSS_WeaponPool.Instance.assignedSecondaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();             

        if (FPSS_Main.Instance.currentWeaponSlot == WeaponSlot.Primary)
        {
            primaryIconImg.sprite = primaryWeaponObject.img_activeIcon;
            secondaryIconImg.sprite = secondaryWeaponObject.img_inactiveIcon;
        }
        else if (FPSS_Main.Instance.currentWeaponSlot == WeaponSlot.Secondary)
        {
            primaryIconImg.sprite = primaryWeaponObject.img_inactiveIcon;
            secondaryIconImg.sprite = secondaryWeaponObject.img_activeIcon;
        }
        else
        {
            primaryIconImg.sprite = primaryWeaponObject.img_inactiveIcon;
            secondaryIconImg.sprite = secondaryWeaponObject.img_inactiveIcon;
        }
    }

    public void Hide(bool hide)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        
        if (hide && !hidden)
        {
            // Move the entire HUD downwards to hide it
            rectTransform.DOAnchorPos(new Vector2(0, -80), 0);
            hidden = true;
        }
        else if (!hide && hidden)
        {
            // Move the entire HUD upwards to show it
            rectTransform.DOAnchorPos(new Vector2(0, 0), 0);
            hidden = false;
        }
    }
}