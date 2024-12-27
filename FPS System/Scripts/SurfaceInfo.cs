using UnityEngine;
using FMODUnity;

//used as a "struct" you can attach to objects to define their surface type

public enum SurfaceType
{
    Concrete,
    Metal,
    Wood,
    Glass,
    Flesh,
    None
}

public class SurfaceInfo : MonoBehaviour
{
    public SurfaceType surfaceType;         //This will be used to store the surface type
    public bool isPenetrable;               //This will be used to check if the surface is penetrable
    public GameObject bulletHolePrefab;     //This will be used to store the bullet
    public ParticleSystem impactEffect;     //This will be used to store the impact effect
    public EventReference sfx_Impact;       //This will be used to store the impact sound
    public EventReference sfx_ImpactTail;   //This will be used to store the impact sound tail
}
