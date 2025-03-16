using UnityEngine;

public class CompassIcon : MonoBehaviour
{
    public NavCompass compass;

    public void DestroyIcon()
    {
        //
        GameObject.Destroy(this.gameObject);
    }
}
