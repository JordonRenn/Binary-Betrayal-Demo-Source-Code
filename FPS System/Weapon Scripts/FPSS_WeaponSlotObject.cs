using System.Collections;
using FMODUnity;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

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
    
    private FPSS_WeaponPool weaponPool;
    private FPSS_Main main;
    private FPS_InputHandler inputHandler;
    [SerializeField] private FPSS_ReticleSystem reticleSystem;
    [SerializeField] private CamShake camShake;
    [Tooltip("Intensity of camera shake, 0-1 (1 being the most intense)")]
    [SerializeField] private float camShakeIntensity;
    [SerializeField] private Camera cam;
    public Animator animator { get; private set; } 
    private FPSS_PlayerCamController camController;

    [Header("Weapon Properties")]
    [Space(10)]

    public string weaponName;
    [SerializeField] private WeaponSlot weaponSlot;
    [SerializeField] public WeaponFireMode fireMode;
    [SerializeField] private bool isScoped;
    
    [Header("Objects")]
    [Space(10)]

    [SerializeField] public Sprite img_activeIcon;
    [SerializeField] public Sprite img_inactiveIcon;

    [SerializeField] private GameObject weaponObject;
    [SerializeField] private GameObject weaponArms;

    [SerializeField] private float armSpeed;
    [SerializeField] private float disarmSpeed;
    [SerializeField] private GameObject shellObject;
    [SerializeField] private float sfx_ShellDelay;
    
    [Header("Spread")]
    [Space(10)]
    
    [SerializeField] private float spread;
    [SerializeField] private float spreadPatternRandomization;
    [SerializeField] private Vector2[] spreadPattern;
    [SerializeField] private int spreadPatternArrayLength;
    [SerializeField] private float spreadRecoveryRate;
    private int spreadIndex = 0;
    private float currentSpread = 0f;
    [SerializeField] private string fireAnimStateName;
    
    [Header("Ammunition")]
    [Space(10)]
    
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private float fireRate;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float reticleFallOffSpeed;
    [SerializeField] public int clipSize;
    [SerializeField] public int currentClip;
    [SerializeField] private int maxAmmo = 69420;
    [SerializeField] public int currentAmmo;

    [Header("Range and Damage")]
    [Space(10)]
    
    [SerializeField] private int damagePerShot;
    [SerializeField] private float headshotMultiplier;
    [SerializeField] private float armourPenetration;
    [SerializeField] private float range;
    [SerializeField] private AnimationCurve damageFalloff;

    [Header("WEAPON SFX")]
    [Space(10)]

    [SerializeField] private EventReference sfx_Fire;
    [SerializeField] private EventReference sfx_Empty;
    [SerializeField] private EventReference sfx_ClipOut;
    [SerializeField] private float clipOutSFXDelay;
    [SerializeField] private EventReference sfx_ClipIn;
    [SerializeField] private float clipInSFXDelay;
    [SerializeField] private EventReference sfx_Slide;
    [SerializeField] private float slideSFXDelay;
    [SerializeField] private EventReference sfx_gun_shell;
    [SerializeField] private EventReference sfx_AdsLayerIn;

    [Header("Positio Targets")]
    [Space(10)]

    [SerializeField] private Transform pos_Muzzle;
    [SerializeField] private Transform pos_ShellEject;
    [SerializeField] private Transform pos_GunAudio;
    
    [Header("VFX")] 
    [Space(10)] 

    [SerializeField] private Sprite muzzleFlash;
    [SerializeField] private GameObject bullettrail;

    private bool isActive = false;
    private bool canFire = true;

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;
    [SerializeField] private float initDelay = 0.2f;
    [SerializeField] private float initTimeout = 10f;
    private bool initialized = false;

    public UnityEvent OnFire = new UnityEvent();
    public UnityEvent OnReload = new UnityEvent();
    
    #region Init
    void Start()
    {
        StartCoroutine(Init());
        ConstructSpreadPattern();
        OnFire.AddListener(Fire);
        OnReload.AddListener(() => StartCoroutine(Reload()));
    }

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

        while (reticleSystem == null || camController == null)
        {
            reticleSystem = GameObject.FindAnyObjectByType<FPSS_ReticleSystem>();
            camController = cam.GetComponent<FPSS_PlayerCamController>();

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initTimeout)
            {
                Debug.LogError("WEAPON SLOT OBJECT: Initialization timed out: ReticleSystem or CamController not found.");
                yield break;
            }
            yield return null;
        }

        initialized = true;
    }
    #endregion

    void Update()
    {
        if (!initialized) 
        {
            Debug.LogWarning("WEAPON SLOT OBJECT: Initializing...");
            return;
        }
        
        if (isActive)
        {
            CalculateSpread();
        }

        if (!inputHandler.FireInput)
        {
            spreadIndex = 0;
        }
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
    }

    public IEnumerator SetWeaponInactive()
    {
        animator.SetTrigger("Disarm");

        yield return new WaitForSeconds(disarmSpeed);
        
        weaponObject.SetActive(false);
        weaponArms.SetActive(false);

        isActive = false;
    }

    #region reloading
    public IEnumerator Reload()
    {
        if (currentClip < clipSize)
        {
            animator.SetTrigger("Reload");
            weaponPool.isReloading = true;
            weaponPool.canReload = false;

            yield return new WaitForSeconds(clipOutSFXDelay);
            playSfx(sfx_ClipOut);

            yield return new WaitForSeconds(clipInSFXDelay);
            playSfx(sfx_ClipIn);
            currentClip = clipSize;

            yield return new WaitForSeconds(slideSFXDelay);
            playSfx(sfx_Slide);

            animator.SetTrigger("Idle");
        }

        weaponPool.isReloading = false;
        weaponPool.canReload = true;
    }
    #endregion

    #region firing
    public void Fire()
    {
        if (isActive && !weaponPool.isReloading) 
        {
            StartCoroutine(FireBullet());
        }
    }

    private IEnumerator FireBullet()
    {
        if (currentClip > 0 && canFire)
        {
            canFire = false;
            
            playSfx(sfx_Fire);
            animator.Play(fireAnimStateName, -1, 0f);
            reticleSystem.GunFire(reticleFallOffSpeed);
            
            FireHitScan();
            
            ApplySpread();
            
            camShake.Shake(camShakeIntensity);

            currentClip--;

            if (fireMode == WeaponFireMode.BoltAction)
            {
                // TODO: Implement bolt action
            }

            yield return new WaitForSeconds(fireRate);
            canFire = true;

            if (fireMode == WeaponFireMode.Automatic && inputHandler.FireInput && !weaponPool.isReloading)
            {
                StartCoroutine(FireBullet());
            }
        }
        else
        {
            playSfx(sfx_Empty);
        }
    }

    private void FireHitScan()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, range))
        {
            Vector3 targetPos = hit.point;

            StartCoroutine(ApplyBulletVisualEffects(targetPos)); 

            CalculateDamage(hit.point, hit.normal);

            ApplyBulletSurfaceEffects(ray, out hit);  
        }
    }
    #endregion

    #region damage
    void CalculateDamage(Vector3 hitPoint, Vector3 hitNormal)
    {
        float distance = Vector3.Distance(cam.transform.position, hitPoint);
        float damageMultiplier = damageFalloff.Evaluate(distance / range);
        float damage = damagePerShot * damageMultiplier;
    }
    #endregion

    #region spread
    private void ConstructSpreadPattern()
    {
        Vector2[] randomizedSpreadPattern = new Vector2[spreadPatternArrayLength];
        for (int i = 0; i < spreadPatternArrayLength; i++)
        {
            spreadPattern[i] = spreadPattern[i] + new Vector2(Random.Range(-spreadPatternRandomization, spreadPatternRandomization), Random.Range(-spreadPatternRandomization, spreadPatternRandomization));
        }
    }

    private void CalculateSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread - spreadRecoveryRate * Time.deltaTime, 0, spreadPatternArrayLength);
    }

    private void ApplySpread()
    {
        if (spreadIndex >= spreadPatternArrayLength)
        {
            spreadIndex = 0;
        }

        Vector2 spreadOffset = spreadPattern[spreadIndex] * currentSpread;
        camController.ApplySpread(spreadOffset);

        currentSpread = Mathf.Clamp(currentSpread + spread, 0, spreadPatternArrayLength);
        spreadIndex++;
    }
    #endregion

    #region VFX    
    private void ApplyBulletSurfaceEffects(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, range))
        {
            int layerMask = hit.collider.gameObject.layer;
            SurfaceInfo surfaceInfo = hit.collider.gameObject.GetComponent<SurfaceInfo>();

            if (surfaceInfo == null)
            {
                return;
            }

            GameObject impactDecal = surfaceInfo.bulletHolePrefab;
            Instantiate(impactDecal, hit.point, Quaternion.LookRotation(hit.normal));

            playSfx(surfaceInfo.sfx_Impact, hit.transform);

            ParticleSystem impactEffect = surfaceInfo.impactEffect;
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    private IEnumerator ApplyBulletVisualEffects(Vector3 targetPos)
    {
        GameObject bulletTrail = Instantiate(bullettrail, pos_Muzzle.position, Quaternion.identity);

        while (bulletTrail != null && Vector3.Distance(bulletTrail.transform.position, targetPos) > 0.1f)
        {
            bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, targetPos, bulletSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(bulletTrail);
    }
    #endregion

    public void EjectShell()
    {
        Debug.Log("Ejecting shell");

        StartCoroutine(PlaySfxDelay(sfx_gun_shell, sfx_ShellDelay));
        GameObject shell = Instantiate(shellObject, pos_ShellEject.position, pos_ShellEject.rotation);
    }

    private IEnumerator PlaySfxDelay(EventReference sfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        playSfx(sfx);
    }

    #region AUDIO
    public void playSfx(EventReference eventRef, Transform position = null)
    {
        if (position == null)
        {
            position = pos_GunAudio;
        }
        RuntimeManager.PlayOneShot(eventRef, position.position);
    }
    #endregion
}
#endregion
