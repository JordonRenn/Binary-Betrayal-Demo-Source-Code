using System.Collections;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Enum representing the fire mode of the weapon.
/// </summary>
public enum WeaponFireMode
{
    Automatic,
    SemiAutomatic,
    BoltAction
}

#region FPSS_WeaponSlotObject
/// <summary>
/// Class representing a weapon slot object in the FPS system.
/// </summary>
public class FPSS_WeaponSlotObject : MonoBehaviour
{
    [Header("References")]
    [Space(10)]
    
    protected FPSS_WeaponPool weaponPool;
    private FPSS_Main main;
    protected FPS_InputHandler inputHandler;
    protected FPSS_ReticleSystem reticleSystem;
    private FPSS_WeaponHUD hud;


    protected CamShake camShake;
    [Tooltip("Intensity of camera shake, 0-1 (1 being the most intense)")]
    [SerializeField] protected float camShakeIntensity;
    [SerializeField] protected Camera cam;
    public Animator animator { get; private set; } 
    protected FPSS_PlayerCamController camController;

    [SerializeField] private WeaponSlot weaponSlot;
    
    [Header("Objects")]
    [Space(10)]

    [SerializeField] public Sprite img_activeIcon;
    [SerializeField] public Sprite img_inactiveIcon;

    [SerializeField] private GameObject weaponObject;
    [SerializeField] private GameObject weaponArms;

    [SerializeField] private float armSpeed;
    [SerializeField] private float disarmSpeed;
    [SerializeField] protected string fireAnimStateName;

    [Header("Range and Damage")]
    [Space(10)]
    
    [SerializeField] private int damagePerShot;
    [SerializeField] private float headshotMultiplier;
    [SerializeField] private float armourPenetration;
    [SerializeField] protected float range;
    [SerializeField] private AnimationCurve damageFalloff;

    [SerializeField] protected Transform pos_GunAudio;

    protected bool isActive = false;
    protected bool canFire = true;

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;
    [SerializeField] private float initDelay = 0.2f;
    [SerializeField] private float initTimeout = 10f;
    public bool initialized  { get; private set; }
    
    void Awake()
    {
        initialized = false;
        StartCoroutine(Init());
    }

    #region Init
    IEnumerator Init()
    {
        float elapsedTime = 0;

        animator = GetComponent<Animator>();

        while (FPSS_WeaponPool.Instance == null || FPSS_Main.Instance == null || FPS_InputHandler.Instance == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("WEAPON SLOT OBJECT: Initialization timed out: Main or WeaponPool instance not found.");
                yield break;
            }
            yield return null;
        }

        weaponPool = FPSS_WeaponPool.Instance;
        main = FPSS_Main.Instance;
        inputHandler = FPS_InputHandler.Instance;

        yield return new WaitForSeconds(initDelay);
        elapsedTime += initDelay;

        while (reticleSystem == null || camController == null || cam == null || hud == null)
        {
            cam = Camera.main;
            reticleSystem = GameObject.FindAnyObjectByType<FPSS_ReticleSystem>();
            camController = cam.GetComponent<FPSS_PlayerCamController>();
            camShake = cam.GetComponent<CamShake>();
            hud = GameObject.FindAnyObjectByType<FPSS_WeaponHUD>();

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("WEAPON SLOT OBJECT: Initialization timed out: ReticleSystem or CamController not found.");
                yield break;
            }
            yield return null;
        }

        initialized = true;

        Debug.Log($"WEAPON SLOT OBJECT: Initialization time: {elapsedTime} seconds.");
    }
    #endregion

    #region WEAPON ACTIONS
    public IEnumerator SetWeaponActive()
    {
        if (weaponObject == null || weaponArms == null || animator == null || weaponPool == null || main == null)
        {
            Debug.LogError("WEAPON SLOT OBJECT: SetWeaponActive failed due to uninitialized objects.");
            yield break;
        }

        isActive = true;
        
        weaponObject.SetActive(true);
        weaponArms.SetActive(true);

        animator.SetTrigger("Arm");

        yield return new WaitForSeconds(armSpeed);

        animator.SetTrigger("Idle");
        
        weaponPool.currentWeaponSlot = weaponSlot;
        main.currentWeaponSlot = weaponSlot;
        hud.RefreshWeaponHUD();
        
    }

    public IEnumerator SetWeaponInactive()
    {
        animator.SetTrigger("Disarm");

        yield return new WaitForSeconds(disarmSpeed);
        
        weaponObject.SetActive(false);
        weaponArms.SetActive(false);

        isActive = false;
    }
    #endregion

    #region damage
    protected void CalculateDamage(Vector3 hitPoint)
    {
        float distance = Vector3.Distance(cam.transform.position, hitPoint);
        float damageMultiplier = damageFalloff.Evaluate(distance / range);
        float damage = damagePerShot * damageMultiplier;
    }
    #endregion

    #region VFX    
    protected void ApplyHitSurfaceEffects(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, range))
        {
            int layerMask = hit.collider.gameObject.layer;
            SurfaceInfo surfaceInfo = hit.collider.gameObject.GetComponent<SurfaceInfo>();

            if (surfaceInfo == null)
            {
                return;
            }

            GameObject impactDecal = surfaceInfo.bulletHolePrefab[Random.Range(0, surfaceInfo.bulletHolePrefab.Length)];
            Instantiate(impactDecal, hit.point, Quaternion.LookRotation(hit.normal));

            playSfx(surfaceInfo.sfx_Impact, hit.point);

            ParticleSystem impactEffect = surfaceInfo.impactEffect;
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    protected IEnumerator PlaySfxDelay(EventReference sfx, float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        playSfx(sfx, position);
    }
    #endregion

    #region AUDIO
    public void playSfx(EventReference eventRef, Vector3 position) //fix this
    {
        if (position == null)
        {
            position = pos_GunAudio.position;
        }
        RuntimeManager.PlayOneShotAttached(eventRef, pos_GunAudio.gameObject);
    }
    #endregion

    #region virtual methods
    public virtual void Fire()
    {
        //
    }

    public virtual void Reload()
    {
        //
    }
    #endregion
}
#endregion
