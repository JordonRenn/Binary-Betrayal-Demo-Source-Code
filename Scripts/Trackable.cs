using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FOR ITEMS THAT WILL SHOW UP ON THE NAV COMPASS
/// </summary>

public class Trackable : MonoBehaviour
{
    [SerializeField] public Sprite compassIcon ;
    [HideInInspector] public Image compassImage;
    [SerializeField] public float compassDrawDistance ;
    [HideInInspector] public Vector2 position 
        {get {return new Vector2(transform.position.x, transform.position.z);}}

    void OnEnable()
    {
        GameMaster.Instance.allTrackables.Add(this);
    }

    void OnDestroy() 
    {
        GameMaster.Instance.allTrackables.Remove(this);
    }
}
