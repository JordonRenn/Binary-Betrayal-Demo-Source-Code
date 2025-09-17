using UnityEngine;
using UnityEngine.UIElements;

public class WeaponDisplayController : MonoBehaviour
{
    private VisualElement weaponPrimaryIcon;
    private VisualElement weaponSecondaryIcon;
    private VisualElement weaponMeleeIcon;
    private VisualElement weaponUtilityIcon;
    private VisualElement weaponUnarmedIcon;

    private Label currentAmmo;
    private Label reserveAmmo;
    private Label magSize;

    private const string ELEMENT_NAME_ICON_PRIMARY = "WeaponPrimaryIcon";
    private const string ELEMENT_NAME_ICON_SECONDARY = "WeaponSecondaryIcon";
    private const string ELEMENT_NAME_ICON_MELEE = "WeaponMeleeIcon";
    private const string ELEMENT_NAME_ICON_UTILITY = "WeaponUtilityIcon";
    private const string ELEMENT_NAME_ICON_UNARMED = "WeaponUnarmedIcon";
    private const string ELEMENT_NAME_AMMO_CURRENT = "WeaponAmmoCurrentMag";
    private const string ELEMENT_NAME_AMMO_RESERVE = "WeaponAmmoTotal";
    private const string ELEMENT_NAME_AMMO_MAG = "WeaponAmmoMagCapacity";

    private void Awake()
    {
        weaponPrimaryIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(ELEMENT_NAME_ICON_PRIMARY);
        weaponSecondaryIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(ELEMENT_NAME_ICON_SECONDARY);
        weaponMeleeIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(ELEMENT_NAME_ICON_MELEE);
        weaponUtilityIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(ELEMENT_NAME_ICON_UTILITY);
        weaponUnarmedIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(ELEMENT_NAME_ICON_UNARMED);

        currentAmmo = GetComponent<UIDocument>().rootVisualElement.Q<Label>(ELEMENT_NAME_AMMO_CURRENT);
        reserveAmmo = GetComponent<UIDocument>().rootVisualElement.Q<Label>(ELEMENT_NAME_AMMO_RESERVE);
        magSize = GetComponent<UIDocument>().rootVisualElement.Q<Label>(ELEMENT_NAME_AMMO_MAG);

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // GameMaster.Instance?.oe_WeaponEquipped.AddListener(OnWeaponEquipped);    // can data binding make this irrelevant?
        // GameMaster.Instance?.we_WeaponChanged.AddListener(OnWeaponChanged);      // can data binding make this irrelevant?
        // GameMaster.Instance?.we_WeaponReload.AddListener(OnWeaponReload);        // can data binding make this irrelevant?
    }
    
    private void OnDestroy()
    {
        // GameMaster.Instance?.oe_WeaponEquipped?.RemoveListener(OnWeaponEquipped);
        // GameMaster.Instance?.we_WeaponChanged?.RemoveListener(OnWeaponChanged);
        // GameMaster.Instance?.we_WeaponReload?.RemoveListener(OnWeaponReload);
    }
}
