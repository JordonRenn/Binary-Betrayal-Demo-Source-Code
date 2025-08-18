using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponHUDData", menuName = "Scriptable Objects/WeaponHUDData")]
[Obsolete("Use WeaponData instead")]
public class WeaponHUDData : ScriptableObject
{
    [SerializeField] public string weaponDisplayName;
    [SerializeField] public Sprite weaponActiveImg;
    [SerializeField] public Sprite weaponInactiveImg;
}
