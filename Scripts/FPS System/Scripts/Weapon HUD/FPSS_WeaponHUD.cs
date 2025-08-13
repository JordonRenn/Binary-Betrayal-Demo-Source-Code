using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
                _instance = FindAnyObjectByType<FPSS_WeaponHUD>();
                if (_instance == null)
                {
                    Debug.LogWarning($"Attempting to access {nameof(FPSS_WeaponHUD)} before it is initialized. Returning null.");
                    return null;
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private GameObject player;
    private FPSS_Pool c_WeaponPool;
    private WPO_Gun primaryWeaponComponent;
    private WPO_Gun secondaryWeaponComponent;

    //private bool isHidden = false;

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
        // Primary weapon components
        if (primaryNameText == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Primary weapon name text component is missing!");
        if (primarySlotNumber == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Primary slot number text component is missing!");
        if (primaryAmmoText == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Primary ammo text component is missing!");
        if (primaryIconImg == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Primary weapon icon image component is missing!");

        // Secondary weapon components
        if (secondaryNameText == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Secondary weapon name text component is missing!");
        if (secondarySlotNumber == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Secondary slot number text component is missing!");
        if (secondaryAmmoText == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Secondary ammo text component is missing!");
        if (secondaryIconImg == null)
            Debug.LogError($"{nameof(FPSS_WeaponHUD)}: Secondary weapon icon image component is missing!");
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

    [Header("Scriptable Objects")]
    [Space(10)]

    [SerializeField] private WeaponHUDData[] weaponData;

    [Header("DEV OPTIONS")]
    [SerializeField] private bool debugMode; // Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f; // Used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f; // Initialization timeout
    private bool initialized = false; // Flag used to stop Update() from running before initialization is complete
    private bool hidden = false;

    #region Initialization
    void Awake()
    {
        // Initialize as singleton but don't persist across scenes (UI is scene-specific)
        if (this.InitializeSingleton(ref _instance) == this)
        {
            // Validate required components
            ValidateRequiredComponents();

            // Setup event listeners
            GameMaster.Instance.gm_PlayerSpawned.AddListener(GetPlayer);
            GameMaster.Instance.gm_WeaponPoolSpawned.AddListener(GetWeaponPool);

            SBGDebug.LogInfo("WeaponHUD initialized successfully", "FPSS_WeaponHUD");
        }
    }

    void Start()
    {
        StartCoroutine(Init());
        GameMaster.Instance.gm_WeaponHudSpawned.Invoke();
    }

    /// <summary>
    /// Initializes the weapon HUD by waiting for necessary instances and data to be available.
    /// </summary>
    IEnumerator Init()
    {
        Debug.Log("FPSS_WEAPONHUD | Initialization started");
        float initTime = Time.time;

        while (player == null && Time.time - initTime < initTimeout)
        {
            yield return null;
        }

        while (c_WeaponPool == null && Time.time - initTime < initTimeout)
        {
            yield return null;
        }

        yield return new WaitForSeconds(initDelay);

        CacheWeaponComponents();

        Debug.Log($"FPSS_WEAPONHUD | Initialization time: {Time.time - initTime} seconds.");

        primaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);
        secondaryWeaponComponent.AmmoChange.AddListener(UpdateAmmoCount);

        initialized = true;

        RefreshWeaponHUD();
        //GameMaster.Instance.gm_PlayerSpawned.AddListener(RefreshWeaponHUD);
    }

    void GetPlayer()
    {
        player = GameObject.FindWithTag("Player");
        GameMaster.Instance.gm_PlayerSpawned.RemoveListener(GetPlayer);
        Debug.Log("WEAPON HUD | Player object cached");
    }

    void GetWeaponPool()
    {
        c_WeaponPool = FPSS_Pool.Instance;
        GameMaster.Instance.gm_WeaponPoolSpawned.RemoveListener(GetWeaponPool);
        Debug.Log("WEAPON HUD | Weapon pool instance cached");
    }
    #endregion

    private void UpdateAmmoCount()
    {
        primaryAmmoText.SetText($"{primaryWeaponComponent.currentClip} / {primaryWeaponComponent.clipSize}");
        secondaryAmmoText.SetText($"{secondaryWeaponComponent.currentClip} / {secondaryWeaponComponent.clipSize}");
    }

    private void CacheWeaponComponents()
    {
        primaryWeaponComponent = c_WeaponPool.assignedPrimaryWPO.gameObject.GetComponent<WPO_Gun>();
        secondaryWeaponComponent = c_WeaponPool.assignedSecondaryWPO.gameObject.GetComponent<WPO_Gun>();
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

                UpdateHUDTextColors(0);

                primaryNameText.text = primaryWeaponComponent.HUDData.weaponDisplayName;
                break;
            case WeaponSlot.Secondary:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + FPSS_Main.Instance.currentWeaponSlot);

                UpdateHUDTextColors(1);

                secondaryNameText.text = secondaryWeaponComponent.HUDData.weaponDisplayName;
                break;
            case WeaponSlot.Melee:
                Debug.Log("WEAPON HUD: Current Weapon Slot: " + FPSS_Main.Instance.currentWeaponSlot);

                UpdateHUDTextColors(2);

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
        FPSS_WeaponSlotObject primaryWeaponObject = c_WeaponPool.assignedPrimaryWPO;
        FPSS_WeaponSlotObject secondaryWeaponObject = c_WeaponPool.assignedSecondaryWPO;

        if (FPSS_Main.Instance.currentWeaponSlot == WeaponSlot.Primary)
        {
            primaryIconImg.sprite = primaryWeaponObject.HUDData.weaponActiveImg;
            secondaryIconImg.sprite = secondaryWeaponObject.HUDData.weaponInactiveImg;
        }
        else if (FPSS_Main.Instance.currentWeaponSlot == WeaponSlot.Secondary)
        {
            primaryIconImg.sprite = primaryWeaponObject.HUDData.weaponInactiveImg;
            secondaryIconImg.sprite = secondaryWeaponObject.HUDData.weaponActiveImg;
        }
        else
        {
            primaryIconImg.sprite = primaryWeaponObject.HUDData.weaponInactiveImg;
            secondaryIconImg.sprite = secondaryWeaponObject.HUDData.weaponInactiveImg;
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
    }
    #endregion
}
#endregion