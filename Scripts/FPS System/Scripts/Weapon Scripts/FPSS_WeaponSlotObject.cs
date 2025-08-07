using System.Collections;
using FMODUnity;
using UnityEngine;
using Unity.Cinemachine;

/* 
    First Person Controller Hierarchy:

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs) 
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                   
                    - POS_GUN_AUDIO
                    - 0_0_Ak-47 (Gun_AK47.cs)
                        - AK_47
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        <--- THIS SCRIPT
                    - 1_0_HandGun (Gun_HandGun.cs)
                        - HandGun
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 1_1_ShotGun (FPSS_WeaponSlotObject.cs)            <--- THIS SCRIPT
                    - 2_0_Knife (FPSS_WeaponSlotObject.cs)              <--- THIS SCRIPT
                    - 3_0_Grenade (FPSS_WeaponSlotObject.cs)            <--- THIS SCRIPT
                    - 3_1_FlashGrenade (FPSS_WeaponSlotObject.cs)       <--- THIS SCRIPT
                    - 3_2_SmokeGrenade (FPSS_WeaponSlotObject.cs)       <--- THIS SCRIPT
                    - 4_0_Unarmed (FPSS_WeaponSlotObject.cs)            <--- THIS SCRIPT
 */

#region FPSS_WeaponSlotObject
/// <summary>
/// Class representing a weapon slot object in the FPS system.
/// 
/// SHOULD NOT BE USING AS A BASE CLASS!!!
/// MAKE CHILD CLASSES FOR EACH WEAPON TYPE (GUN, MELEE, GRENADE, UNARMED)
/// 
/// </summary>
public class FPSS_WeaponSlotObject : MonoBehaviour
{
    [SerializeField] public WeaponRefID refID;
    [SerializeField] public WeaponHUDData HUDData;

    [Header("References")]
    [Space(10)]

    protected FPSS_ReticleSystem reticleSystem;
    private FPSS_WeaponHUD hud;

    [SerializeField] protected FPSS_Pool weaponPool;
    [SerializeField] protected CamShake camShake;
    [Tooltip("Intensity of camera shake, 0-1 (1 being the most intense)")]
    [SerializeField] protected float camShakeIntensity;
    [SerializeField] protected CinemachineCamera cam;
    [SerializeField] public Animator animator;
    [SerializeField] protected FirstPersonCamController camController;

    [SerializeField] private WeaponSlot weaponSlot;

    [Header("Objects")]
    [Space(10)]

    /* [SerializeField] public Sprite img_activeIcon;
    [SerializeField] public Sprite img_inactiveIcon; */

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

    [SerializeField] private float initTimeout = 10f;
    public bool initialized { get; private set; }

    void OnEnable()
    {
        //SBGDebug.LogInfo("Instantiated" , $"WeaponSlotObject: {HUDData.weaponDisplayName}");
        initialized = false;
        StartCoroutine(Init());
    }

    #region Init
    IEnumerator Init()
    {
        Debug.Log("FPS_WEAPONSLOTOBJECT | Initialization started");

        float initTime = Time.time;

        while (reticleSystem == null && Time.time - initTime <= initTimeout)
        {
            try
            {
                reticleSystem = GameObject.FindAnyObjectByType<FPSS_ReticleSystem>();
            }
            finally
            {
                //Debug.Log("FPSS_WEAPONSLOTOBJECT | Searching for ''Reticle System''");
            }

            yield return null;
        }

        while (hud == null && Time.time - initTime <= initTimeout)
        {
            try
            {
                hud = GameObject.FindAnyObjectByType<FPSS_WeaponHUD>();
            }
            finally
            {
                //Debug.Log("FPSS_WEAPONSLOTOBJECT | Searching for ''Weapon HUD''");
            }

            yield return null;
        }

        initialized = true;

        //SBGDebug.LogInfo($"Initialization time: {Time.time - initTime} seconds." , $"WeaponSlotObject: {HUDData.weaponDisplayName}" );


    }
    #endregion

    #region WEAPON ACTIONS
    public IEnumerator SetWeaponActive()
    {
        if (weaponPool == null)
        {
            //SBGDebug.LogError("'SetWeaponActive()' No Weapon pool found." , $"WeaponSlotObject: {HUDData.weaponDisplayName}");
            yield break;
        }

        if (FPSS_Main.Instance == null)
        {
            //SBGDebug.LogError("'SetWeaponActive()' No FPS_Main found." , $"WeaponSlotObject: {HUDData.weaponDisplayName}");
            yield break;
        }

        isActive = true;

        if (weaponObject != null)
        {
            weaponObject.SetActive(true);
        }

        if (weaponArms != null)
        {
            weaponArms.SetActive(true);
        }

        animator.SetTrigger("Arm");

        yield return new WaitForSeconds(armSpeed);

        animator.SetTrigger("Idle");

        weaponPool.currentWeaponSlot = weaponSlot;
        FPSS_Main.Instance.currentWeaponSlot = weaponSlot;

        if (hud != null)
        {
            hud.RefreshWeaponHUD();
        }
    }

    public IEnumerator SetWeaponInactive()
    {
        animator.SetTrigger("Disarm");

        yield return new WaitForSeconds(disarmSpeed);

        weaponObject.SetActive(false);
        weaponArms.SetActive(false);

        isActive = false;
    }

    /// <summary>
    /// Used to deactivate current weapon without actually switching, or unloading weapon. Use for locked interactions and maybe: cutescenes? dialog? spacial events?
    /// </summary>
    /// <param name="active"></param>
    public void SetCurrentWeaponActive(bool active)
    {
        if (!active)
        {
            StartCoroutine(SetWeaponInactive());
        }
        else
        {
            StartCoroutine(SetWeaponActive());
        }
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
            GameObject instantiatedDecal = Instantiate(impactDecal, hit.point, Quaternion.LookRotation(hit.normal));
            instantiatedDecal.transform.SetParent(hit.collider.gameObject.transform); // Set impactDecal as a child

            PlaySfx(surfaceInfo.sfx_Impact, hit.point, true);

            ParticleSystem impactEffect = surfaceInfo.impactEffect;
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    protected IEnumerator PlaySfxDelay(EventReference sfx, float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        PlaySfx(sfx, position);
    }
    #endregion

    #region AUDIO
    public void PlaySfx(EventReference eventRef, Vector3 position, bool surfaceSFX = false)
    {
        if (surfaceSFX == false)
        {
            RuntimeManager.PlayOneShotAttached(eventRef, pos_GunAudio.gameObject);
        }
        else
        {
            if (position == null) // if no position is provided, use the gun audio position
            {
                position = pos_GunAudio.position;
            }

            RuntimeManager.PlayOneShot(eventRef, position);
        }
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
