using System.Collections;
using FMODUnity;
using UnityEngine;
using Unity.Cinemachine;
using BinaryBetrayal.CameraControl;


/* 
    First Person Controller Hierarchy:

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs) 
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                   
                    - POS_GUN_AUDIO
                    - 0_0_Ak-47 (Gun_AK47.cs)
                        - Hands_AK_47 (Weapon and Arm meshes)
                            - AK_47
                                - MuzzleFlash (MuzzleFlash.cs)
                    - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        <--- THIS SCRIPT
                    - 1_0_HandGun (Gun_HandGun.cs)
                        - Hands_HandGun (Weapon and Arm meshes)
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
/// SHOULD NOT BE USED ON OBJECTS
/// MAKE CHILD CLASSES FOR EACH WEAPON TYPE (GUN, MELEE, GRENADE, UNARMED)
/// 
/// </summary>
public abstract class FPSS_WeaponSlotObject : MonoBehaviour
{
    [Header("Weapon Slot Object Properties")]
    [Space(10)]

    [SerializeField] public WeaponData weaponData; // set by WeaponPool on instantiation

    [Tooltip("Animated arms and weapon mesh, should be a single object, with animator , with both arms and weapon mesh as children")]
    [SerializeField] public GameObject firstPersonMeshObject; // set by WeaponPool on instantiation

    [Tooltip("Default, fallback value")]
    [SerializeField] private float armSpeed = 0.23f; // target time -- TODO -> figure out how to extract from anim clips automatically
    [Tooltip("Default, fallback value")]
    [SerializeField] private float disarmSpeed = 0.1f; // target time -- TODO -> figure out how to extract from anim clips automatically
    [Tooltip("Default, fallback value")]
    [SerializeField] protected float reloadTime = 1f; // target time -- TODO -> figure out how to extract from anim clips automatically   

    [Header("Range and Damage")]
    [Space(10)]

    [SerializeField] private int baseDamage;
    [SerializeField] private AnimationCurve damageFalloff;
    [Tooltip("Intensity of camera shake, 0-1 (1 being the most intense)")]
    [SerializeField] protected float camShakeIntensity;
    [SerializeField] protected float range;


    [Header("Audio Positioning")]
    [Space(10)]

    [SerializeField] protected Transform pos_GunAudio;

    // Private References
    protected CinemachineCamera cam;
    [HideInInspector] public Animator animator;
    protected CamShake camShake;

    protected bool isActive = false;
    protected bool canFire = true;

    [Header("DEV OPTIONS")]
    [Space(10)]

    [SerializeField] private float initTimeout = 10f;
    public bool initialized { get; private set; }

    void OnEnable()
    {
        initialized = false;
        StartCoroutine(Init());
    }

    #region Init
    IEnumerator Init()
    {
        float initTime = Time.time;

        if (firstPersonMeshObject == null)
        {
            SBGDebug.LogError($"Weapon Slot Object {weaponData.displayName} does not have a First Person Mesh Object assigned.", $"FPSS_WeaponSlotObject ({weaponData.displayName}) | Initialization");
            yield break;
        }

        bool allDependenciesLoaded = false;
        while (!allDependenciesLoaded && Time.time - initTime <= initTimeout)
        {
            bool poolReady = WeaponPool.Instance != null;
            bool camControllerReady = FirstPersonCamController.Instance != null;

            allDependenciesLoaded =  poolReady && camControllerReady;

            if (!allDependenciesLoaded)
                yield return null;
        }

        if (allDependenciesLoaded)
        {
            initialized = true;
            
            camShake = FirstPersonCamController.Instance.gameObject.GetComponent<CamShake>();
            if (camShake == null) SBGDebug.LogError($"CamShake component not found on FirstPersonCamController", "FPSS_WeaponSlotObject | Initialization");

            cam = FirstPersonCamController.Instance.gameObject.GetComponent<CinemachineCamera>();
            if (cam == null) SBGDebug.LogError($"CinemachineCamera component not found on FirstPersonCamController", "FPSS_WeaponSlotObject | Initialization");

            animator = firstPersonMeshObject.GetComponentInChildren<Animator>();
            if (animator == null) SBGDebug.LogError($"Animator component not found on First Person Mesh Object", "FPSS_WeaponSlotObject | Initialization");
        }
        else
        {
            SBGDebug.LogError($"Initialization timed out for weapon {weaponData.displayName}. Some dependencies were not loaded.", "FPSS_WeaponSlotObject | Initialization");
        }
    }
    #endregion

    #region WEAPON ACTIONS
    public IEnumerator SetWeaponActive()
    {
        isActive = true;
        firstPersonMeshObject?.SetActive(true);

        // Play the Arm animation from the beginning with no blending
        float waitTime = armSpeed; // Default fallback time
        
        try
        {
            animator?.Play("Arm", 0, 0f);
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogError($"Error playing Arm animation: {ex.Message}", "FPSS_WeaponSlotObject | SetWeaponActive");
        }
        
        // Wait a tiny bit for the animation to actually start
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
                    // SBGDebug.LogInfo($"Arm animation length: {waitTime}s for {weaponData.displayName}", "FPSS_WeaponSlotObject");
                }
            }
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogWarning($"Couldn't get animation length, using default: {ex.Message}", "FPSS_WeaponSlotObject | SetWeaponActive");
        }
        
        // Wait for the animation to complete
        yield return new WaitForSeconds(waitTime);

        // Play the Idle animation from the beginning
        try
        {
            animator?.Play("Idle", 0, 0f);
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogError($"Error playing Idle animation: {ex.Message}", "FPSS_WeaponSlotObject | SetWeaponActive");
        }
        
        /* FPSS_WeaponHUD.Instance?.RefreshWeaponHUD(); */
        WeaponPool.Instance?.onWeaponActivationComplete.Invoke();
    }

    public IEnumerator SetWeaponInactive()
    {
        // Play the Disarm animation from the beginning with no blending
        float waitTime = disarmSpeed; // Default fallback time
        
        try
        {
            animator?.Play("Disarm", 0, 0f);
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogError($"Error playing Disarm animation: {ex.Message}", "FPSS_WeaponSlotObject | SetWeaponInactive");
        }
        
        // Wait a tiny bit for the animation to actually start
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
                    // SBGDebug.LogInfo($"Disarm animation length: {waitTime}s for {weaponData.displayName}", "FPSS_WeaponSlotObject");
                }
            }
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogWarning($"Couldn't get disarm animation length, using default: {ex.Message}", "FPSS_WeaponSlotObject | SetWeaponInactive");
        }
        
        // Wait for the animation to complete
        yield return new WaitForSeconds(waitTime);
        
        firstPersonMeshObject?.SetActive(false);
        WeaponPool.Instance?.onWeaponDeactivationComplete.Invoke();

        isActive = false;

        if (!firstPersonMeshObject)
        {
            // SBGDebug.LogInfo("Deactivated First Person Mesh Object", $"WeaponSlotObject: {weaponData.displayName}");
        }
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
    protected float CalculateDamage(Vector3 hitPoint)
    {
        float distance = Vector3.Distance(cam.transform.position, hitPoint);
        float falloffMultiplier = damageFalloff.Evaluate(distance / range); // ???? wtf 
        float damage = baseDamage * falloffMultiplier;

        return damage;
    }
    #endregion

    #region VFX    
    protected void ApplyHitSurfaceEffects(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, range))
        {
            int layerMask = hit.collider.gameObject.layer;
            SurfaceInfo surfaceInfo = hit.collider.gameObject.GetComponent<SurfaceInfo>();

            if (surfaceInfo == null) return;

            GameObject impactDecal = surfaceInfo.bulletHolePrefab[UnityEngine.Random.Range(0, surfaceInfo.bulletHolePrefab.Length)];
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
