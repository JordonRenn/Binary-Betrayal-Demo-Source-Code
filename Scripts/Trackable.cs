using UnityEngine;

/// <summary>
/// FOR ITEMS THAT WILL SHOW UP ON THE NAV COMPASS
/// </summary>

public class Trackable : MonoBehaviour
{
    [SerializeField] protected Sprite compassIcon {get; private set;}
    [SerializeField] protected float compassDrawDistance  {get; private set;}
}
