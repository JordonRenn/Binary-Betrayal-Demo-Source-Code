using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using FMODUnity;

public class WPO_Gun : FPSS_WeaponSlotObject
{
    [Header("Weapon Properties")]
    [Space(10)]

    public string weaponName;
    //[SerializeField] private WeaponSlot weaponSlot;
    [SerializeField] public WeaponFireMode fireMode;
    [SerializeField] private bool isScoped;
    
    [Header("Ammunition")]
    [Space(10)]
    
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] protected float fireRate;
    [SerializeField] private float bulletSpeed;
    [SerializeField] protected float reticleFallOffSpeed;
    [SerializeField] public int clipSize;
    [SerializeField] public int currentClip;
    [SerializeField] private int maxAmmo = 69420;
    [SerializeField] public int currentAmmo;
    
    [Header("Shell")]
    [Space(10)]
    
    [SerializeField] private GameObject shellObject;
    [SerializeField] private float sfx_ShellDelay;

    [Header("Spread")]
    [Space(10)]

    [SerializeField] private float spread;
    [SerializeField] private float spreadPatternRandomization;
    [SerializeField] private Vector2[] spreadPattern;
    private int spreadPatternArrayLength;
    [SerializeField] private float spreadRecoveryRate;
    private int spreadIndex = 0;
    private float currentSpread = 0f;

    [Header("Position Targets")]
    [Space(10)]

    [SerializeField] private Transform pos_Muzzle;
    [SerializeField] private Transform pos_ShellEject;

    [Header("WEAPON SFX")]
    [Space(10)]

    [SerializeField] protected EventReference sfx_Fire;
    [SerializeField] protected EventReference sfx_Empty;
    [SerializeField] private EventReference sfx_ClipOut;
    [SerializeField] private float clipOutSFXDelay;
    [SerializeField] private EventReference sfx_ClipIn;
    [SerializeField] private float clipInSFXDelay;
    [SerializeField] private EventReference sfx_Slide;
    [SerializeField] private float slideSFXDelay;
    [SerializeField] private EventReference sfx_gun_shell;
    [SerializeField] private EventReference sfx_AdsLayerIn;

    [Header("VFX")] 
    [Space(10)] 

    [SerializeField] private GameObject bullettrail;
    [SerializeField] private MuzzleFlash mFlash;

    //SUB STATES
    protected bool isReloading = false;
    protected bool canReload = true;

    void Start()
    {
        spreadPatternArrayLength = spreadPattern.Length;
        ConstructSpreadPattern(); 
    }

    void Update()
    {
        if (!initialized) 
        {
            Debug.LogWarning("WEAPON SLOT OBJECT: Initializing...");
            return;
        }

        if (isActive)
        {
            CalculateSpread(); //find a way to get this the hell out of the Update loop
        }

        if (!inputHandler.FireInput) //can we also get this out of here?
        {
            spreadIndex = 0;
        }
    }

    protected void ConstructSpreadPattern()
    {
        Vector2[] randomizedSpreadPattern = new Vector2[spreadPatternArrayLength];
        for (int i = 0; i < spreadPatternArrayLength; i++)
        {
            spreadPattern[i] = spreadPattern[i] + new Vector2(Random.Range(-spreadPatternRandomization, spreadPatternRandomization), Random.Range(-spreadPatternRandomization, spreadPatternRandomization));
        }
    }

    protected void CalculateSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread - spreadRecoveryRate * Time.deltaTime, 0, spreadPatternArrayLength);
    }

    protected void ApplySpread()
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

    protected void FireHitScan()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, range))
        {
            Vector3 targetPos = hit.point;

            StartCoroutine(ApplyBulletVisualEffects(targetPos)); 

            CalculateDamage(hit.point);

            ApplyHitSurfaceEffects(ray, out hit);  
        }
    }

    public IEnumerator ReloadWeapon()
    {
        if (currentClip < clipSize)
        {
            animator.SetTrigger("Reload");
            isReloading = true;
            canReload = false;

            yield return new WaitForSeconds(clipOutSFXDelay);
            playSfx(sfx_ClipOut, pos_GunAudio.position);

            yield return new WaitForSeconds(clipInSFXDelay);
            playSfx(sfx_ClipIn, pos_GunAudio.position);
            currentClip = clipSize;

            yield return new WaitForSeconds(slideSFXDelay);
            playSfx(sfx_Slide, pos_GunAudio.position);

            animator.SetTrigger("Idle");
        }

        isReloading = false;
        canReload = true;
    }

    private IEnumerator ApplyBulletVisualEffects(Vector3 targetPos)
    {
        mFlash.Flash();
        
        GameObject bulletTrail = Instantiate(bullettrail, pos_Muzzle.position, Quaternion.identity);

        while (bulletTrail != null && Vector3.Distance(bulletTrail.transform.position, targetPos) > 0.1f)
        {
            bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, targetPos, bulletSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(bulletTrail);
    }

    public void EjectShell()
    {
        Debug.Log("Ejecting shell");

        StartCoroutine(PlaySfxDelay(sfx_gun_shell, sfx_ShellDelay, pos_GunAudio.position)); //??? Posittioning will need to get fixed, lazy temp fix
        GameObject shell = Instantiate(shellObject, pos_ShellEject.position, pos_ShellEject.rotation);
    }
}
