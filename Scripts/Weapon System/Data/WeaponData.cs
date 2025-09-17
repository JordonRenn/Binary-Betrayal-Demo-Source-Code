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
