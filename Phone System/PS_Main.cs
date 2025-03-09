using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;

public class PS_Main : Interactable
{
    private FPSS_WeaponHUD c_WeaponHud;
    private FPSS_ReticleSystem c_ReticleSystem;
    private GameObject playerObj;
    private CharacterMovement characterMovement;

    [SerializeField] private string phoneID = "";

    [Header("Phone System Components")]
    [Space(10)]
    
    [SerializeField] private GameObject fauxArms;
    [SerializeField] private CinemachineCamera c_PhoneCam;
    [SerializeField] private PS_Keypad c_Keypad;
    [SerializeField] private Collider interactCollider;
    [SerializeField] private Transform playerTeleportPoint;
    [SerializeField] private Animator payphoneAnimator;

    [Header("FMOD")]
    [Space(10)]

    [SerializeField] public EventReference phonePickupSound;
    [SerializeField] public EventReference phoneHangupSound;
    [SerializeField] public EventReference phoneHookSound;

    [Header("Dev Options")]
    [Space(10)]

    [SerializeField] private float initDelay = 0.1f;
    [SerializeField] private float initTimeout = 10f;

    //substates
    private bool usingPhone = false;
    private bool initialized = false;

    #region Initialization
    void Awake()
    {
        Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Instantiated");

        GameMaster.Instance.gm_PlayerSpawned.AddListener(_GetPlayer);
        GameMaster.Instance.gm_ReticleSystemSpawned.AddListener(_GetReticle);
        GameMaster.Instance.gm_WeaponHudSpawned.AddListener(_GetWeaponHUD);
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Initialization started");

        //yield return new WaitForSeconds(initDelay);

        float initTime = Time.time;

        while (playerObj == null && Time.time - initTime < initTimeout) // PLAYER OBJECT
        {
            //playerObj = GameObject.FindWithTag("Player");
            Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Searching for PLAYER OBJECT");
            yield return null;
        }

        while (c_ReticleSystem == null && Time.time - initTime < initTimeout) // RETICLE SYSTEM
        {
            //c_ReticleSystem = FindFirstObjectByType<FPSS_ReticleSystem>();
            Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Searching for RETICLE SYSTEM");
            yield return null;
        }

        while (c_WeaponHud == null && Time.time - initTime < initTimeout) // WEAPON HUD
        {
            //c_WeaponHud = FindFirstObjectByType<FPSS_WeaponHUD>();
            Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Searching for WEAPON HUD");
            yield return null;
        }

        initialized = true;
        Debug.Log($"PAY PHONE | {this.gameObject.transform.position} | Initialization COMPLETE");
    }

    private void _GetPlayer()
    {
        playerObj = GameObject.FindWithTag("Player");
        characterMovement = playerObj.GetComponent<CharacterMovement>();
    }

    private void _GetReticle()
    {
        c_ReticleSystem = FindFirstObjectByType<FPSS_ReticleSystem>();
    }

    private void _GetWeaponHUD()
    {
        c_WeaponHud = FindFirstObjectByType<FPSS_WeaponHUD>();
    }
    #endregion

    public override void Interact()
    {
        if (!usingPhone && initialized)
        {
           StartCoroutine(ActivePhone());
        }
    }

    /// <summary>
    /// Activates the phone system
    /// </summary>/
    
    private IEnumerator ActivePhone()
    {
        usingPhone = true;

        FPSS_Pool.Instance.currentActiveWPO.SetCurrentWeaponActive(false);
        characterMovement.moveDisabled = true; //needed to stop Update loop from running so controller can be disabled so player can teleport
        FPS_InputHandler.Instance.SetInputState(InputState.LockedInteraction);
        FPS_InputHandler.Instance.lint_CancelTriggered.AddListener(DeactivePhone);
        interactCollider.enabled = false;

        UI_Master.Instance.HideAllHUD();

        yield return new WaitForSeconds(0.2f);
        
        c_PhoneCam.Priority = 300;

        yield return new WaitForSeconds(0.5f);
        
        fauxArms.SetActive(true);
        payphoneAnimator.SetTrigger("payphone_pickup");
        StartCoroutine(TeleportPlayer());

        yield return new WaitForSeconds(0.45f);

        RuntimeManager.PlayOneShot(phonePickupSound, transform.position);

        yield return new WaitForSeconds(0.85f); 

        c_Keypad.enabled = true;
    }

    private void DeactivePhone()
    {
        StartCoroutine(DeactivePhoneRoutine());
    }

    /// <summary>
    /// Deactivates the phone system
    /// </summary>
    
    private IEnumerator DeactivePhoneRoutine()
    {
        c_Keypad.enabled = false;
        FPS_InputHandler.Instance.lint_CancelTriggered.RemoveListener(DeactivePhone);

        payphoneAnimator.SetTrigger("payphone_hangup");

        yield return new WaitForSeconds(0.85f);
        
        RuntimeManager.PlayOneShot(phoneHangupSound, transform.position);

        yield return new WaitForSeconds(0.45f);
        
        fauxArms.SetActive(false);
        
        c_PhoneCam.Priority = 0;
        
        FirstPersonCamController.Instance.AllowOverride(false);
        
        interactCollider.enabled = true;
        usingPhone = false;

        FPSS_Pool.Instance.currentActiveWPO.SetCurrentWeaponActive(true);

        yield return new WaitForSeconds(0.2f); //allow time for weapon to be re-enabled
        
        characterMovement.moveDisabled = false; 
        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);

        UI_Master.Instance.ShowAllHUD();
    }

    /// <summary>
    /// Dials the number on the phone
    /// </summary>
    
    public void DialNumber(string number)
    {
        Debug.Log("Dialed number: " + number);
        // Dummy method for now
    }

    
    /// <summary>
    /// Teleports the player to position of the phone
    /// </summary>
    private IEnumerator TeleportPlayer()
    {
        
        FirstPersonCamController.Instance.AllowOverride(true);
        
        CharacterController characterController = playerObj.GetComponent<CharacterController>();
        Rigidbody playerRigidbody = playerObj.GetComponent<Rigidbody>();

        FirstPersonCamController.Instance.SetRotation(new Vector2(playerTeleportPoint.transform.eulerAngles.x, playerTeleportPoint.transform.eulerAngles.y + 90f));

        if (characterController != null)
        {
            //characterController.enabled = false; // Disable the CharacterController to change the position
            yield return new WaitForSeconds(0.05f);
            playerObj.transform.position = playerTeleportPoint.position;
            yield return new WaitForSeconds(0.05f);
            //characterController.enabled = true; // Re-enable the CharacterController
        }
        else
        {
            playerObj.transform.position = playerTeleportPoint.position;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero; // Reset the Rigidbody velocity
        }

        FirstPersonCamController.Instance.AllowOverride(false);

        Debug.Log("Player teleported to: " + playerTeleportPoint.position);
    }   
}
