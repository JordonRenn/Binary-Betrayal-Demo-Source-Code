using UnityEngine;

[CreateAssetMenu(fileName = "WeaponHUDData", menuName = "Scriptable Objects/WeaponHUDData")]
public class WeaponHUDData : ScriptableObject
{
    [SerializeField] public string weaponDisplayName;
    [SerializeField] public Sprite weaponActiveImg;
    [SerializeField] public Sprite weaponInactiveImg;
}
