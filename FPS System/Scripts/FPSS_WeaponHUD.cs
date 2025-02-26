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
    private FPSS_Main main; // Used to see what weapon slot is currently active
    private FPSS_WeaponPool weaponPool; // Used to see what gun is currently assigned to each slot

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
        float elapsedTime = 0f;

        while (FPSS_Main.Instance == null || FPSS_WeaponPool.Instance == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("Weapon HUD initialization timed out: Main or WeaponPool instance not found.");
                yield break;
            }
            yield return null;
        }

        main = FPSS_Main.Instance;
        weaponPool = FPSS_WeaponPool.Instance;

        yield return new WaitForSeconds(initDelay);
        elapsedTime += initDelay;

        while (weaponPool.weaponPool[0] == null || weaponPool.weaponPool[1] == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("Weapon HUD initialization timed out: Weapon pool arrays not initialized.");
                yield break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(initDelay);
        elapsedTime += initDelay;

        while (weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex] == null || weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex] == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("Weapon HUD initialization timed out: Primary or Secondary weapon not assigned.");
                yield break;
            }
            yield return null;
        }

        CacheWeaponComponents();   

        yield return new WaitForSeconds(initDelay);

        initialized = true;

        Debug.Log($"WEAPON HUD: Initialization time: {elapsedTime} seconds. ({initDelay}s initialization delay applied)");

        primaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);
        secondaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);

        RefreshWeaponHUD();
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        if (!initialized) 
        {
            Debug.LogWarning("WEAPON HUD: Initializing...");
            return;
        }
    }

    private void UpdateAmmoCount()
    {
        primaryAmmoText.SetText($"{primaryWeaponComponent.currentClip} / {primaryWeaponComponent.clipSize}");
        secondaryAmmoText.SetText($"{secondaryWeaponComponent.currentClip} / {secondaryWeaponComponent.clipSize}");
    }

    private void CacheWeaponComponents()
    {
        primaryWeaponComponent = weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex].GetComponent<WPO_Gun>();
        secondaryWeaponComponent = weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex].GetComponent<WPO_Gun>();
    }

    /// <summary>
    /// Refreshes the weapon HUD to display the currently active weapon slot and its respective weapon; do not call to refresh Icon Sprites, use 'UpdateIconSprite' directly instead
    /// </summary>
    public void RefreshWeaponHUD()
    {
        Debug.Log("WEAPON HUD: Refreshing elements...");
        CacheWeaponComponents();            
        
        switch (main.currentWeaponSlot)
        {
            case WeaponSlot.Primary:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + main.currentWeaponSlot);
                
                primaryNameText.color = color_NameActiveText;
                primarySlotNumber.color = color_SlotActiveText;
                primaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.color = color_NameInactiveText;
                secondarySlotNumber.color = color_SlotInactiveText;
                secondaryAmmoText.color = color_AmmoInactiveText;
                
                primaryNameText.text = primaryWeaponComponent.weaponName;
                break;
            case WeaponSlot.Secondary:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + main.currentWeaponSlot);
                
                primaryNameText.color = color_NameInactiveText;
                primarySlotNumber.color = color_SlotInactiveText;
                primaryAmmoText.color = color_AmmoInactiveText;
                
                secondaryNameText.color = color_NameActiveText;
                secondarySlotNumber.color = color_SlotActiveText;
                secondaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.text = secondaryWeaponComponent.weaponName;
                break;
            case WeaponSlot.Unarmed:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + main.currentWeaponSlot);
                
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
        FPSS_WeaponSlotObject primaryWeaponObject = weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();
        FPSS_WeaponSlotObject secondaryWeaponObject =  weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();             

        if (main.currentWeaponSlot == WeaponSlot.Primary)
        {
            primaryIconImg.sprite = primaryWeaponObject.img_activeIcon;
            secondaryIconImg.sprite = secondaryWeaponObject.img_inactiveIcon;
        }
        else if (main.currentWeaponSlot == WeaponSlot.Secondary)
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

    public void Hide()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (!hidden)
        {
            // Move the entire HUD downwards to hide it
            rectTransform.DOAnchorPos(new Vector2(0, -80), 0);
            hidden = true;
        }
        else
        {
            // Move the entire HUD upwards to show it
            rectTransform.DOAnchorPos(new Vector2(0, 0), 0);
            hidden = false;
        }
    }
}