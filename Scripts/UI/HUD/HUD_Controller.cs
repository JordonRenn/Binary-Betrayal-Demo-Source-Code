using UnityEngine;
using UnityEngine.Events;

public class HUD_Controller : MonoBehaviour
{
    public static HUD_Controller Instance { get; private set; }

    [SerializeField] private GameObject[] HUD_Status_Elements;
    [SerializeField] private GameObject[] HUD_Weapon_Elements;
    [SerializeField] private GameObject[] HUD_Utility_Elements;

    private FPSS_WeaponHUD hud_weapon;
    private NavCompass hud_compass;
    private FPSS_ReticleSystem hud_reticle;

    public bool hud_Hidden { get; private set; }
    private bool hud_weapon_Hidden;
    private bool hud_compass_Hidden;
    private bool hud_reticle_Hidden;

    public UnityEvent<bool> he_WeaponHidden;
    public UnityEvent<bool> he_CompassHidden;
    public UnityEvent<bool> he_ReticleHidden;

    void Awake()
    {
        InitializeComponents();
        Instance = this;
    }

    private void InitializeComponents()
    {
        hud_weapon = GetComponentInChildren<FPSS_WeaponHUD>();
        hud_compass = GetComponentInChildren<NavCompass>();
        hud_reticle = GetComponentInChildren<FPSS_ReticleSystem>();

        if (hud_weapon == null) Debug.LogError("FPSS_WeaponHUD not found as child component");
        if (hud_compass == null) Debug.LogError("NavCompass not found as child component");
        if (hud_reticle == null) Debug.LogError("FPSS_ReticleSystem not found as child component");
    }

    public void HideAllHUD(bool hide)
    {
        // Remove any existing listeners to prevent duplicates
        he_WeaponHidden.RemoveAllListeners();
        he_CompassHidden.RemoveAllListeners();
        he_ReticleHidden.RemoveAllListeners();

        he_WeaponHidden.AddListener((hidden) =>
        {
            hud_weapon_Hidden = hidden;
            CheckHUDVisibility();
        });
        he_CompassHidden.AddListener((hidden) =>
        {
            hud_compass_Hidden = hidden;
            CheckHUDVisibility();
        });
        he_ReticleHidden.AddListener((hidden) =>
        {
            hud_reticle_Hidden = hidden;
            CheckHUDVisibility();
        });

        HideWeaponHUD(hide);
        HideCompass(hide);
        HideReticle(hide);
    }

    private void CheckHUDVisibility()
    {
        if (hud_weapon_Hidden && hud_compass_Hidden && hud_reticle_Hidden)
        {
            hud_Hidden = true;
        }
        else if (!hud_weapon_Hidden && !hud_compass_Hidden && !hud_reticle_Hidden)
        {
            hud_Hidden = false;
        }
        else
        {
            return;
        }
    }

    public void HideWeaponHUD(bool hide)
    {
        hud_weapon.Hide(hide); // invokes "he_WeaponHidden"

        FPSS_ReticleSystem.Instance?.Hide(hide);

        foreach (GameObject element in HUD_Status_Elements)
        {
            element.SetActive(false);
        }
    }

    public void HideCompass(bool hide)
    {
        //hud_compass.Hide(hide); // (should) invokes "he_CompassHidden"
        // Temporarily invoke the event directly since compass hide is not implemented
        he_CompassHidden?.Invoke(hide);
    }

    public void HideReticle(bool hide)
    {
        //hud_reticle.Hide(hide); // (should) invokes "he_ReticleHidden"
        // Temporarily invoke the event directly since reticle hide is not implemented
        he_ReticleHidden?.Invoke(hide);
    }
}