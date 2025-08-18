using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using FMODUnity;

public class WPO_Gun : FPSS_WeaponSlotObject
{
    [Header("Gun Properties")]
    [Space(10)]

    [SerializeField] public WeaponFireMode fireMode;
    [SerializeField] private bool hasScope;
    
    [Header("Ammunition")]
    [Space(10)]
    
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] protected float fireRate;
    [SerializeField] private float bulletSpeed = 300f;
    [SerializeField] protected float reticleFallOffSpeed = 2;
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

    [SerializeField] private float spread = 3; //additional randomization applied
    [SerializeField] private float spreadPatternRandomization = 0.015f; //small amount of randomization to be applied to the predefined spread pattern array values at start up
    [SerializeField] private Vector2[] spreadPattern;
    [SerializeField] private float spreadRecoveryRate = 0.25f;

    private int spreadPatternArrayLength;
    private int spreadIndex = 0;
    private float currentSpread = 0f;

    [Header("Effect Transforms")]
    [Space(10)]

    [SerializeField] private Transform pos_Muzzle;
    [SerializeField] private Transform pos_ShellEject;

    [Header("Weapon SFX")]
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

    [Header("VFX")] 
    [Space(10)] 

    [SerializeField] private GameObject bullettrail;
    [SerializeField] private MuzzleFlash mFlash;

    //SUB STATES
    protected bool isReloading = false;
    protected bool canReload = true;

    public UnityEvent AmmoChange = new UnityEvent();

    void Awake()
    {
        //base.Awake();
        spreadPatternArrayLength = spreadPattern.Length;
        ConstructSpreadPattern(); 
    }

    void Update()
    {
        if (!initialized) 
        {
            //Debug.LogWarning("WEAPON SLOT OBJECT: Initializing...");
            return;
        }

        if (isActive && (fireMode == WeaponFireMode.Automatic || fireMode == WeaponFireMode.SemiAutomatic))
        {
            CalculateSpread(); //find a way to get this the hell out of the Update loop
        }

        if (!InputHandler.Instance.FireInput) //seems jank but works, i guess..
        {
            spreadIndex = 0;
        }
    }

    protected void ConstructSpreadPattern()
    {
        for (int i = 0; i < spreadPatternArrayLength; i++)
        {
            float randomX = 1f + UnityEngine.Random.Range(-spreadPatternRandomization, spreadPatternRandomization);
            float randomY = 1f + UnityEngine.Random.Range(-spreadPatternRandomization, spreadPatternRandomization);
            spreadPattern[i] = new Vector2(
                spreadPattern[i].x * randomX,
                spreadPattern[i].y * randomY
            );
            //Debug.Log($"Spread Index {i} = {spreadPattern[i]}");
        }
    }

    protected void CalculateSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread - (spreadRecoveryRate * Time.deltaTime), spread, spreadPatternArrayLength);
    }

    protected void ApplySpread()
    {
        if (spreadIndex >= spreadPatternArrayLength)
        {
            spreadIndex = 0;
        }

        Vector2 spreadOffset = spreadPattern[spreadIndex] * currentSpread;

        FirstPersonCamController.Instance.ApplySpread(spreadOffset);

        currentSpread = Mathf.Clamp(currentSpread + spread, spread, spreadPatternArrayLength);
        spreadIndex++;
    }

    protected void FireHitScan()
    {
        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));     //old

        Transform camTransform = cam.transform;                             //new
        Ray ray = new Ray(camTransform.position, camTransform.forward);     //new

        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, range))
        {
            Vector3 targetPos = hit.point;

            StartCoroutine(ApplyBulletVisualEffects(targetPos)); 

            CalculateDamage(hit.point);

            ApplyHitSurfaceEffects(ray, out hit);  
        }

        AmmoChange.Invoke();
    }

    public IEnumerator ReloadWeapon()
    {
        if (currentClip < clipSize)
        {
            animator.SetTrigger("Reload");
            isReloading = true;
            canReload = false;

            yield return new WaitForSeconds(clipOutSFXDelay);
            PlaySfx(sfx_ClipOut, pos_GunAudio.position);

            yield return new WaitForSeconds(clipInSFXDelay);
            PlaySfx(sfx_ClipIn, pos_GunAudio.position);
            currentClip = clipSize;

            yield return new WaitForSeconds(slideSFXDelay);
            PlaySfx(sfx_Slide, pos_GunAudio.position);

            animator.SetTrigger("Idle");
        }

        AmmoChange.Invoke();

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
