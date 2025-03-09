using UnityEngine;

/* 
use "base.Start()" in inherited classes if Start() gets ovverridden;
 */

public class Ticked : MonoBehaviour
{
    void Start()
    {
        GameMaster.Instance.globalTick.AddListener(TickedUpdate);
    }

    protected virtual void TickedUpdate()
    {
        //
    }
}
