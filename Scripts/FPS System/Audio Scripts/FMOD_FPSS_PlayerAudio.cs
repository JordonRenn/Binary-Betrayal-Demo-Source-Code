using UnityEngine;
using FMODUnity;

public class FMOD_FPSS_PlayerAudio : MonoBehaviour
{
    private static FMOD_FPSS_PlayerAudio _instance;
    public static FMOD_FPSS_PlayerAudio Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(FMOD_FPSS_PlayerAudio)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private FPSS_Pool weaponPool;
    [SerializeField] private Transform pos_GunAudio;

    [Header("PLAYER SFX")] 
    [Space(10)]

    [SerializeField] public EventReference sfx_player_adsIn;            //default ads sfx
    [SerializeField] public EventReference sfx_player_adsOut;           //default ads sfx
    [SerializeField] public EventReference sfx_player_hurt;             //hurt
    [SerializeField] public EventReference sfx_player_exertion;         //exertion
    [SerializeField] public EventReference sfx_player_death;            //death

    

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateRequiredComponents();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    private void ValidateRequiredComponents()
    {
        if (!pos_GunAudio)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: Gun audio position transform is missing!");
        if (sfx_player_adsIn.Guid == System.Guid.Empty)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: ADS in sound effect not assigned!");
        if (sfx_player_adsOut.Guid == System.Guid.Empty)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: ADS out sound effect not assigned!");
        if (sfx_player_hurt.Guid == System.Guid.Empty)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: Player hurt sound effect not assigned!");
        if (sfx_player_exertion.Guid == System.Guid.Empty)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: Player exertion sound effect not assigned!");
        if (sfx_player_death.Guid == System.Guid.Empty)
            Debug.LogError($"{nameof(FMOD_FPSS_PlayerAudio)}: Player death sound effect not assigned!");
    }

    void Awake()
    {
        // Initialize as singleton and persist across scenes since audio needs to be maintained
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            ValidateRequiredComponents();
        }
    }

    void Start()
    {
        weaponPool = FPSS_Pool.Instance;
    }
    
    public void Fire()
    {
        Debug.Log("AUDIO: Playing 'Fire' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_fire, pos_GunAudio.position);
    }

    public void DryFire()
    {
        Debug.Log("AUDIO: Playing 'Gun_Empty' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_dryFire, pos_GunAudio.position);
    }

    public void ClipIn()
    {
        Debug.Log("AUDIO: Playing 'ClipIn' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_clipIn, pos_GunAudio.position);
    }

    public void ClipOut()
    {
        Debug.Log("AUDIO: Playing 'ClipOut' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_clipOut, pos_GunAudio.position);
    }
    
    public void Slide()
    {
        Debug.Log("AUDIO: Playing 'SlideIn' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_slide, pos_GunAudio.position);
    }

    public void Shell()
    {
        Debug.Log("AUDIO: Playing 'Shell' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_shell, pos_GunAudio.position);
        //weaponPool.currentWeaponSlotObject.audioComp.Shell();
    }

    public void ADSIn()
    {
        Debug.Log("AUDIO: Playing 'ADSIn' sfx");
        RuntimeManager.PlayOneShot(sfx_player_adsIn, pos_GunAudio.position);
    }

    public void ADSOut()
    {
        Debug.Log("AUDIO: Playing 'ADSOut' sfx");
        RuntimeManager.PlayOneShot(sfx_player_adsOut, pos_GunAudio.position);
    }

    public void ADSLayerIn()
    {
        Debug.Log("AUDIO: Playing 'ADSLayerIn' sfx");
        //RuntimeManager.PlayOneShot(sfx_gun_AdsLayerIn, pos_GunAudio.position);
    }
}
