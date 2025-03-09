using UnityEngine;
using FMODUnity;

public class FMOD_FPSS_PlayerAudio : MonoBehaviour
{
    public static FMOD_FPSS_PlayerAudio Instance {get ; private set;}
    private FPSS_Pool weaponPool;
    [SerializeField] private Transform pos_GunAudio;

    [Header("PLAYER SFX")] 
    [Space(10)]

    [SerializeField] public EventReference sfx_player_adsIn;            //default ads sfx
    [SerializeField] public EventReference sfx_player_adsOut;           //default ads sfx
    [SerializeField] public EventReference sfx_player_hurt;             //hurt
    [SerializeField] public EventReference sfx_player_exertion;         //exertion
    [SerializeField] public EventReference sfx_player_death;            //death

    

    void Awake()
    {
        if (Instance != null && Instance != this) //Singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
