using UnityEngine;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(MeshRenderer))]

public class FPS_GunPrefab : MonoBehaviour
{
    [SerializeField] private int weaponSlot;
    public Transform pos_L_Grip;
    public Transform pos_R_Grip;
    public Transform pos_Muzzle;
    public Transform pos_ShellEject;

    [SerializeField] public GameObject objShell;
    [SerializeField] public ParticleSystem particleMuzzleFlash;

    public int ammo = 100;

    public GameObject shellPrefab;

    void Start()
    {
        Debug.Log($"Starting ammo count is {ammo}");
    }

    public bool Fire()
    {
        Debug.Log($"There is {ammo} bullets available");
        
        if (ammo > 0)
        {
            EjectShell();
            MuzzleFlash();
            FireAnimation();
            return true;
        }
        else
        {
            return false;
        }
    }

    void EjectShell()
    {
        //
    }

    void MuzzleFlash()
    {
        //
    }

    void FireAnimation()
    {
        //
    }
}
