using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Data")]
    [Space(10)]

    [SerializeField] public WeaponID ID;
    [SerializeField] public string displayName;
    [SerializeField] public GameObject prefab;
    [SerializeField] public WeaponSlot slot;

    [Header("Weapon Images")]
    [Space(10)]

    [SerializeField] public Sprite weaponActiveImg;
    [SerializeField] public Sprite weaponInactiveImg;
}

#region ENUMS
public enum WeaponID
{
    Rifle,
    Sniper,
    Handgun,
    Shotgun,
    Knife,
    Grenade,
    None, //used for unarmed
}

/// <summary>
/// Enum representing the fire mode of the weapon.
/// </summary>
public enum WeaponFireMode
{
    Automatic,
    SemiAutomatic,
    BoltAction
}

/// <summary>
/// Enum representing the different weapon slots.
/// </summary>
public enum WeaponSlot
{
    Primary,
    Secondary,
    Melee,
    Utility,
    Unarmed
}

#endregion