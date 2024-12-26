using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region FPSS_WeaponHUD
public class FPSS_WeaponHUD : MonoBehaviour
{
    private FPSS_Main main; //used to see what weapon slot is currently active
    private FPSS_WeaponPool weaponPool; //used to see what gun is currently assigned to each slot
    
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
    [Space(10)]
    
    [SerializeField] private bool debugMode;            //Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f;    //used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f;   //initialization timeout
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Init());
    }

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

        yield return new WaitForSeconds(initDelay);

        initialized = true;

        RefreshWeaponHUD();
    }

    void Update()
    {
        if (!initialized) 
        {
            Debug.LogWarning("WEAPON HUD: Initializing...");
            return;
        }
        
        primaryAmmoText.SetText($"{weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>().currentClip} / {weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>().clipSize}");
        secondaryAmmoText.SetText($"{weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>().currentClip} / {weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>().clipSize}");
    }

    #region Update Functions
    /// <summary>
    /// Refreshes the weapon HUD to display the currently active weapon slot and its respective weapon; do not call to refresh Icon Sprites, use 'UpdateIconSprite' directly instead
    /// </summary>
    public void RefreshWeaponHUD() //do not call to refresh Icon Sprites, use 'UpdateIconSprite' directly instead
    {
        FPSS_WeaponSlotObject primaryWeaponObject = weaponPool.weaponPool[0][weaponPool.assignedPrimaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();
        FPSS_WeaponSlotObject secondaryWeaponObject =  weaponPool.weaponPool[1][weaponPool.assignedSecondaryWeaponIndex].GetComponent<FPSS_WeaponSlotObject>();             
        
        switch (main.currentWeaponSlot)
        {
            case WeaponSlot.Primary:
                primaryNameText.color = color_NameActiveText;
                primarySlotNumber.color = color_SlotActiveText;
                primaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.color = color_NameInactiveText;
                secondarySlotNumber.color = color_SlotInactiveText;
                secondaryAmmoText.color = color_AmmoInactiveText;
                
                primaryNameText.text = primaryWeaponObject.weaponName;
                //primaryAmmoText.text = weaponPool.primaryWeaponSlot.currentAmmo.ToString(); //no ammo, yet
                break;
            case WeaponSlot.Secondary:
                primaryNameText.color = color_NameInactiveText;
                primarySlotNumber.color = color_SlotInactiveText;
                primaryAmmoText.color = color_AmmoInactiveText;
                
                secondaryNameText.color = color_NameActiveText;
                secondarySlotNumber.color = color_SlotActiveText;
                secondaryAmmoText.color = color_AmmoActiveText;
                
                secondaryNameText.text = secondaryWeaponObject.weaponName;
                //secondaryAmmoText.text = weaponPool.secondaryWeaponSlot.currentAmmo.ToString(); //no ammo, yet
                break;
            case WeaponSlot.Unarmed:
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

    //implement this function in the future, once weapon slot assignment is implemented
    void UpdateIconSprites() //call when weapon slot is assigned different weapon, not when active weapon slot is changed
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
    #endregion
}
#endregion