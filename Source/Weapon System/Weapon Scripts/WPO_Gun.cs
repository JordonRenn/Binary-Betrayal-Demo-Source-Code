using BinaryBetrayal.InputManagement;
using BinaryBetrayal.CameraControl;
using SBG.VisualEffects;

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using FMODUnity;

[RequireComponent(typeof(WeaponStimEmitter))]
public class WPO_Gun : FPSS_WeaponSlotObject
{
    [Header("Gun Properties")]
    [Space(10)]

    [SerializeField] public WeaponFireMode fireMode;
    private WeaponStimEmitter emitter;
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
    [SerializeField] public EventReference sfx_ClipOut;
    [SerializeField] public EventReference sfx_ClipIn;
    [SerializeField] public EventReference sfx_Slide;
    [SerializeField] public EventReference sfx_Grab;
    [SerializeField] public EventReference sfx_Handling_1;
    [SerializeField] public EventReference sfx_Handling_2;
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
        emitter = GetComponent<WeaponStimEmitter>();
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

        if (!InputSystem.FireInput) //Reset spread pattern when not firing
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
        EjectShell();

        Transform camTransform = cam.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit, range))
        {
            Vector3 targetPos = hit.point;

            StartCoroutine(ApplyBulletVisualEffects(targetPos));

            emitter.GunFireStimulus(pos_Muzzle.position, CalculateDamage(hit.point), hit);

            // CalculateDamage(hit.point);

            ApplyHitSurfaceEffects(ray, out hit);
        }

        AmmoChange.Invoke();
    }

    public IEnumerator ReloadWeapon()
    {
        var waitTime = reloadTime;

        if (currentClip < clipSize)
        {
            animator.Play("Reload", 0, 0f);
            isReloading = true;
            canReload = false;

            yield return new WaitForSeconds(0.1f);

            try
            {
                // Get the currently playing animation clip info
                if (animator != null)
                {
                    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        waitTime = clipInfo[0].clip.length;
                        SBGDebug.LogInfo($"Reload animation length: {waitTime}s for {weaponData.displayName}", "WPO_Gun | ReloadWeapon");
                    }
                }
            }
            catch (System.Exception ex)
            {
                SBGDebug.LogWarning($"Couldn't get reload animation length, using default: {ex.Message}", "WPO_Gun | ReloadWeapon");
            }

            // Wait for the animation to complete
            yield return new WaitForSeconds(waitTime);

            currentClip = clipSize;

            animator.Play("Idle", 0, 0f);
        }

        AmmoChange.Invoke();

        isReloading = false;
        canReload = true;
    }

    private IEnumerator ApplyBulletVisualEffects(Vector3 targetPos)
    {
        //mFlash.Flash();

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
        // Debug.Log("Ejecting shell");

        StartCoroutine(PlaySfxDelay(sfx_gun_shell, sfx_ShellDelay, pos_GunAudio.position)); //??? Posittioning will need to get fixed, lazy temp fix
        GameObject shell = Instantiate(shellObject, pos_ShellEject.position, pos_ShellEject.rotation);
    }

    #region AUDIO
    public void PlaySfx(EventReference eventRef)
    {
        RuntimeManager.PlayOneShotAttached(eventRef, pos_GunAudio.gameObject);
    }
    #endregion
}
