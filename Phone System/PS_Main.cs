using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;

public class PS_Main : Interactable
{
    private FPSS_WeaponHUD c_WeaponHud;
    private FPSS_ReticleSystem c_ReticleSystem;
    private GameObject playerObj;
    private CharacterMovement characterMovement;

    private Dictionary<string, string> phoneNumbers = new Dictionary<string, string>
    {
        { "1234567", "test_dialogue" },
        { "4206969",  "phone_4206969_01" },
    };

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
    //[SerializeField] public EventReference phoneHookSound;
    [SerializeField] public EventReference phoneRingbackTone;
    [SerializeField] public float rickBackLength = 3f;
    [SerializeField] public EventReference phoneInvalidCall;
    [SerializeField] public float invalidCallLength = 3f;

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
        SBGDebug.LogInfo($"PAY PHONE | {this.gameObject.transform.position} | Instantiated", "PS_Main");

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
        SBGDebug.LogInfo($"PAY PHONE | {this.gameObject.transform.position} | Initialization started", "PS_Main");

        //yield return new WaitForSeconds(initDelay);

        float initTime = Time.time;

        while (playerObj == null && Time.time - initTime < initTimeout) // PLAYER OBJECT
        {
            SBGDebug.LogDebug($"PAY PHONE | {this.gameObject.transform.position} | Searching for PLAYER OBJECT", "PS_Main");
            yield return null;
        }

        while (c_ReticleSystem == null && Time.time - initTime < initTimeout) // RETICLE SYSTEM
        {
            SBGDebug.LogDebug($"PAY PHONE | {this.gameObject.transform.position} | Searching for RETICLE SYSTEM", "PS_Main");
            yield return null;
        }

        while (c_WeaponHud == null && Time.time - initTime < initTimeout) // WEAPON HUD
        {
            SBGDebug.LogDebug($"PAY PHONE | {this.gameObject.transform.position} | Searching for WEAPON HUD", "PS_Main");
            yield return null;
        }

        initialized = true;
        SBGDebug.LogInfo($"PAY PHONE | {this.gameObject.transform.position} | Initialization COMPLETE", "PS_Main");
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

    #region Phone Management
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

        // Only play hangup animation and sound if we haven't already
        if (usingPhone)
        {
            payphoneAnimator.SetTrigger("payphone_hangup");
            yield return new WaitForSeconds(0.85f);

            RuntimeManager.StudioSystem.getBus("bus:/Pay Phone", out FMOD.Studio.Bus phoneBank);
            phoneBank.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);

            RuntimeManager.PlayOneShot(phoneHangupSound, transform.position);
        }

        // Clean up any ongoing dialogue
        DialogueBox.Instance.CloseDialogueBox();

        yield return new WaitForSeconds(0.45f);

        // Reset visual and camera states
        fauxArms.SetActive(false);
        c_PhoneCam.Priority = 0;
        FirstPersonCamController.Instance.AllowOverride(false);

        // Re-enable interaction and reset states
        interactCollider.enabled = true;
        usingPhone = false;

        // Re-enable weapon
        FPSS_Pool.Instance.currentActiveWPO.SetCurrentWeaponActive(true);

        yield return new WaitForSeconds(0.2f); //allow time for weapon to be re-enabled

        // Re-enable movement and input
        characterMovement.moveDisabled = false;
        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);

        UI_Master.Instance.ShowAllHUD();
    }

    private void SetPhoneState(bool active)
    {
        usingPhone = active;
        c_Keypad.enabled = active;
        interactCollider.enabled = !active;
        fauxArms.SetActive(active);
        
        if (!active)
        {
            RuntimeManager.StudioSystem.getBus("bus:/Pay Phone", out FMOD.Studio.Bus phoneBank);
            phoneBank.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
    #endregion

    #region Calling
    /// <summary>
    /// Attempts to dial a number on the phone
    /// </summary>
    /// <param name="number">The phone number to dial</param>
    public void AttemptCall(string number)
    {
        if (!ValidatePhoneNumber(number))
        {
            StartCoroutine(InvalidCall(number));
            return;
        }

        c_Keypad.DisableKeys();
        StartCoroutine(ConnectingCall(number));
    }

    private bool ValidatePhoneNumber(string number)
    {
        if (string.IsNullOrEmpty(number) || number.Length != 7)
        {
            SBGDebug.LogError($"Invalid number format: {number}", "PS_Main");
            return false;
        }
        
        return NumberLookup(number);
    }

    private bool NumberLookup(string number)
    {
        if (!phoneNumbers.TryGetValue(number, out string dialogueId))
        {
            SBGDebug.LogError($"Number {number} not found in phone book.", "PS_Main");
            return false;
        }

        SBGDebug.LogInfo($"Dialing {number} for dialogue: {dialogueId}", "PS_Main");
        DialogueLoader.Instance.LoadDialogue(dialogueId);
        return true;
    }

    private IEnumerator ConnectingCall(string number)
    {
        bool callSuccess = false;

        try
        {
            RuntimeManager.PlayOneShot(phoneRingbackTone, transform.position);
            SBGDebug.LogInfo($"Calling {number}...", "PS_Main");
            callSuccess = true;
        }
        catch (System.Exception e)
        {
            SBGDebug.LogException(e, "PS_Main");
            StartCoroutine(InvalidCall(number));
            yield break;
        }

        if (callSuccess)
        {
            yield return new WaitForSeconds(rickBackLength);

            try
            {
                SBGDebug.LogInfo($"Connected to {number}. Loading dialogue...", "PS_Main");
                DialogueBox.Instance.LoadDialogue(phoneNumbers[number]);
                DialogueBox.Instance.OpenDialogueBox();
            }
            catch (System.Exception e)
            {
                SBGDebug.LogException(e, "PS_Main");
                StartCoroutine(InvalidCall(number));
            }
        }
    }

    private IEnumerator InvalidCall(string number)
    {
        SBGDebug.LogWarning($"Dialing failed for number: {number}", "PS_Main");
        
        bool soundPlayed = false;
        
        try
        {
            RuntimeManager.PlayOneShot(phoneInvalidCall, transform.position);
            soundPlayed = true;
        }
        catch (System.Exception e)
        {
            SBGDebug.LogException(e, "PS_Main");
        }

        if (soundPlayed)
        {
            yield return new WaitForSeconds(invalidCallLength);
        }

        DialogueBox.Instance.CloseDialogueBox(); //fail safe, probably not needed
        DeactivePhone();
    }
    #endregion

    #region Player Management
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

        SBGDebug.LogInfo("Player teleported to: " + playerTeleportPoint.position, "PS_Main");
    }
    #endregion
    
    private void OnDestroy()
    {
        RuntimeManager.StudioSystem.getBus("bus:/Pay Phone", out FMOD.Studio.Bus phoneBank);
        phoneBank.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        
        GameMaster.Instance.gm_PlayerSpawned.RemoveListener(_GetPlayer);
        GameMaster.Instance.gm_ReticleSystemSpawned.RemoveListener(_GetReticle);
        GameMaster.Instance.gm_WeaponHudSpawned.RemoveListener(_GetWeaponHUD);
    }
}
