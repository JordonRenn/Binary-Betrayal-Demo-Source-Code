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

    private void Awake()
    {
        weaponPrimaryIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("WeaponPrimaryIcon");
        weaponSecondaryIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("WeaponSecondaryIcon");
        weaponMeleeIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("WeaponMeleeIcon");
        weaponUtilityIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("WeaponUtilityIcon");
        weaponUnarmedIcon = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("WeaponUnarmedIcon");

        currentAmmo = GetComponent<UIDocument>().rootVisualElement.Q<Label>("WeaponAmmoCurrentMag");
        reserveAmmo = GetComponent<UIDocument>().rootVisualElement.Q<Label>("WeaponAmmoTotal");
        magSize = GetComponent<UIDocument>().rootVisualElement.Q<Label>("WeaponAmmoMagCapacity");
    }
}
