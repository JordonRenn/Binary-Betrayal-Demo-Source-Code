using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

#region FPSS_WeaponHUD
/// <summary>
/// Manages the weapon HUD, displaying the current weapon information.
/// </summary>
public class FPSS_WeaponHUD : MonoBehaviour
{
    private static FPSS_WeaponHUD _instance;
    public static bool IsInitialized => _instance != null && _instance.initialized;
    public static FPSS_WeaponHUD Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("WeaponHUD instance is not initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

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

    private GameObject player;
    private WPO_Gun primaryWeaponComponent;
    private WPO_Gun secondaryWeaponComponent;

    private bool initialized = false; // Flag used to stop Update() from running before initialization is complete
    private float initTimeout = 10f;
    private bool hidden = false;

    #region Initialization
    void Awake()
    {
        if (this.InitializeSingleton(ref _instance) == this)
        {
            //GameMaster.Instance.gm_PlayerSpawned.AddListener(GetPlayer);
            //GameMaster.Instance.gm_WeaponPoolSpawned.AddListener(GetWeaponPool);
            GameMaster.Instance.gm_WeaponHudSpawned.Invoke();

            StartCoroutine(Init());
        }
    }

    /// <summary>
    /// Initializes the weapon HUD by waiting for necessary instances and data to be available.
    /// </summary>
    IEnumerator Init()
    {
        float initTime = Time.time;

        // add any hard conditionals here

        bool allDependenciesLoaded = false;
        while (!allDependenciesLoaded && Time.time - initTime <= initTimeout)
        {
            bool gameMasterReady = GameMaster.Instance != null;
            bool poolReady = WeaponPool.Instance != null;

            allDependenciesLoaded = gameMasterReady && poolReady;

            if (!allDependenciesLoaded)
                yield return null;
        }

        if (allDependenciesLoaded)
        {
            yield return new WaitUntil(() => GameMaster.Instance.playerObject != null);
            player = GameMaster.Instance.playerObject;

            yield return new WaitUntil(() => WeaponPool.Instance.poolFilled);
            
            // Additional wait to ensure weapon components are fully initialized
            yield return new WaitUntil(() => 
                WeaponPool.Instance.primaryWeaponComponent != null && 
                WeaponPool.Instance.secondaryWeaponComponent != null);
            
            // Wait for activeWSO to be set in WeaponPool
            yield return new WaitUntil(() => WeaponPool.Instance.activeWSO != null);
            
            // Give Unity one more frame to ensure everything is initialized
            yield return null;
            
            Debug.Log("WEAPON HUD: All weapon components verified, caching now...");
            CacheWeaponComponents();
            RefreshWeaponHUD();
        }
        else
        {
            Debug.LogError("WeaponHUD initialization timed out. Please check dependencies.");
        }

        initialized = true;
    }
    #endregion

    private void UpdateAmmoCount()
    {
        if (primaryWeaponComponent != null)
        {
            primaryAmmoText.SetText($"{primaryWeaponComponent.currentClip} / {primaryWeaponComponent.clipSize}");
        }
        else
        {
            primaryAmmoText.SetText("");
        }

        if (secondaryWeaponComponent != null)
        {
            secondaryAmmoText.SetText($"{secondaryWeaponComponent.currentClip} / {secondaryWeaponComponent.clipSize}");
        }
        else
        {
            secondaryAmmoText.SetText("");
        }
    }

    private void CacheWeaponComponents()
    {
        primaryWeaponComponent = WeaponPool.Instance.primaryWeaponComponent;
        secondaryWeaponComponent = WeaponPool.Instance.secondaryWeaponComponent;

        if (primaryWeaponComponent != null) primaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);
        if (secondaryWeaponComponent != null) secondaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);
    }

    /// <summary>
    /// Refreshes the weapon HUD to display the currently active weapon slot and its respective weapon; do not call to refresh Icon Sprites, use 'UpdateIconSprite' directly instead
    /// </summary>
    public void RefreshWeaponHUD()
    {
        Debug.Log("WEAPON HUD: Refreshing elements...");
        CacheWeaponComponents();

        switch (WeaponPool.Instance.activeSlot)
        {
            case WeaponSlot.Primary:
                Hide(false);
                UpdateHUDTextColors(0);
                primaryNameText.text = primaryWeaponComponent.weaponData.displayName;
                break;
            case WeaponSlot.Secondary:
                Hide(false);
                UpdateHUDTextColors(1);
                secondaryNameText.text = secondaryWeaponComponent.weaponData.displayName;
                break;
            case WeaponSlot.Melee:
                Hide(true);
                primaryNameText.text = "";
                secondaryNameText.text = "";
                break;
            case WeaponSlot.Utility:
                Hide(true);
                primaryNameText.text = "";
                secondaryNameText.text = "";
                break;
            case WeaponSlot.Unarmed:
                Hide(true);
                primaryNameText.text = "";
                secondaryNameText.text = "";
                break;
        }

        UpdateIconSprites();
    }

    /// <summary>
    /// Updates the icon sprites for the weapon slots. Call when the WeaponSlot is assigned a different weapon, not when the WeaponSlot is changed.
    /// </summary>
    void UpdateIconSprites()
    {
        FPSS_WeaponSlotObject primaryWeaponObject = WeaponPool.Instance.primaryWSO;
        FPSS_WeaponSlotObject secondaryWeaponObject = WeaponPool.Instance.secondaryWSO;

        if (WeaponPool.Instance.activeSlot == WeaponSlot.Primary)
        {
            primaryIconImg.sprite = primaryWeaponObject.weaponData.weaponActiveImg;
            secondaryIconImg.sprite = secondaryWeaponObject.weaponData.weaponInactiveImg;
        }
        else if (WeaponPool.Instance.activeSlot == WeaponSlot.Secondary)
        {
            if (primaryWeaponObject != null)
            {
                primaryIconImg.sprite = primaryWeaponObject.weaponData.weaponInactiveImg;
            }
            else
            {
                primaryIconImg.sprite = null; // or a default inactive sprite
            }
            if (secondaryWeaponObject != null)
            {
                secondaryIconImg.sprite = secondaryWeaponObject.weaponData.weaponActiveImg;
            }
            else
            {
                secondaryIconImg.sprite = null; // or a default inactive sprite
            }
        }
        else
        {
            primaryIconImg.sprite = primaryWeaponObject.weaponData.weaponInactiveImg;
            secondaryIconImg.sprite = secondaryWeaponObject.weaponData.weaponInactiveImg;
        }
    }

    /// <summary>
    /// 0 = Primary active; 1 = Secondary active; 2 = both inactive
    /// </summary>
    /// <param name="slot"></param>
    private void UpdateHUDTextColors(int slot)
    {
        if (slot == 0) //primary active
        {
            primaryNameText.color = color_NameActiveText;
            primarySlotNumber.color = color_SlotActiveText;
            primaryAmmoText.color = color_AmmoActiveText;

            secondaryNameText.color = color_NameInactiveText;
            secondarySlotNumber.color = color_SlotInactiveText;
            secondaryAmmoText.color = color_AmmoInactiveText;
        }
        else if (slot == 1) //secondary active
        {
            primaryNameText.color = color_NameInactiveText;
            primarySlotNumber.color = color_SlotInactiveText;
            primaryAmmoText.color = color_AmmoInactiveText;

            secondaryNameText.color = color_NameActiveText;
            secondarySlotNumber.color = color_SlotActiveText;
            secondaryAmmoText.color = color_AmmoActiveText;
        }
        else //unarmed?? why not just hide HUD?
        {
            primaryNameText.color = color_NameInactiveText;
            primarySlotNumber.color = color_SlotInactiveText;
            primaryAmmoText.color = color_AmmoInactiveText;

            secondaryNameText.color = color_NameInactiveText;
            secondarySlotNumber.color = color_SlotInactiveText;
            secondaryAmmoText.color = color_AmmoInactiveText;
        }
    }

    #region Hide
    public void Hide(bool hide)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (hide && !hidden)
        {
            // Move the entire HUD downwards to hide it
            rectTransform.DOAnchorPos(new Vector2(0, -80), 0)
                .OnComplete(() =>
                {
                    HUD_Controller.Instance?.he_WeaponHidden.Invoke(true);
                });
            hidden = true;
        }
        else if (!hide && hidden)
        {
            // Move the entire HUD upwards to show it
            rectTransform.DOAnchorPos(new Vector2(0, 0), 0)
                .OnComplete(() =>
                {
                    HUD_Controller.Instance?.he_WeaponHidden.Invoke(false);
                });
            hidden = false;
        }
        else
        {
            return;
        }
    }
    #endregion
}
#endregion