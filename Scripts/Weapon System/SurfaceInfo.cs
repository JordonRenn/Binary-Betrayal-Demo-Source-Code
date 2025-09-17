using UnityEngine;
using FMODUnity;

public class SurfaceInfo : MonoBehaviour
{
    public SurfaceType surfaceType;
    public bool isPenetrable;
    public GameObject[] bulletHolePrefab;
    public ParticleSystem impactEffect;
    public EventReference sfx_Impact;
    public EventReference sfx_ImpactTail;
}