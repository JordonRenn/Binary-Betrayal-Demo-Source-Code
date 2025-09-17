using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;
using GlobalEvents;

public class PS_Main : SauceObject
{
    /* private FPSS_WeaponHUD c_WeaponHud; */
    // private FPSS_ReticleSystem c_ReticleSystem;
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

    //[SerializeField] private float initDelay = 0.1f;
    [SerializeField] private float initTimeout = 10f;

    //substates
    private bool usingPhone = false;
    private bool initialized = false;

    #region Initialization
    void Awake()
    {
        SBGDebug.LogInfo($"PAY PHONE | {this.gameObject.transform.position} | Instantiated", "PS_Main");

        LevelEvents.PlayerControllerInstantiated += _GetPlayer;
        DialogueEvents.DialogueEnded += OnDialogueEnded;
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

        /* while (c_ReticleSystem == null && Time.time - initTime < initTimeout) // RETICLE SYSTEM
        {
            SBGDebug.LogDebug($"PAY PHONE | {this.gameObject.transform.position} | Searching for RETICLE SYSTEM", "PS_Main");
            yield return null;
        } */

        initialized = true;
        SBGDebug.LogInfo($"PAY PHONE | {this.gameObject.transform.position} | Initialization COMPLETE", "PS_Main");
    }

    private void _GetPlayer()
    {
        playerObj = GameObject.FindWithTag("Player");
        characterMovement = playerObj.GetComponent<CharacterMovement>();
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

        WeaponPool.Instance.activeWSO.SetCurrentWeaponActive(false);
        characterMovement.moveDisabled = true; //needed to stop Update loop from running so controller can be disabled so player can teleport
        InputHandler.Instance.SetInputState(InputState.Focus);
        InputHandler.Instance.OnFocus_CancelInput.AddListener(DeactivePhone);
        interactCollider.enabled = false;

        UIManager.Instance.HideAllHUD(true);

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
        InputHandler.Instance.OnFocus_CancelInput.RemoveListener(DeactivePhone);

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
        if (DialogueDisplayController.Instance != null && DialogueDisplayController.Instance.IsDialogueActive)
        {
            DialogueDisplayController.Instance.EndDialogue();
        }

        yield return new WaitForSeconds(0.45f);

        // Reset visual and camera states
        fauxArms.SetActive(false);
        c_PhoneCam.Priority = 0;
        FirstPersonCamController.Instance.AllowOverride(false);

        // Re-enable interaction and reset states
        interactCollider.enabled = true;
        usingPhone = false;

        // Re-enable weapon
        WeaponPool.Instance.activeWSO.SetCurrentWeaponActive(true);

        yield return new WaitForSeconds(0.2f); //allow time for weapon to be re-enabled

        // Re-enable movement and input
        characterMovement.moveDisabled = false;
        InputHandler.Instance.SetInputState(InputState.FirstPerson);

        UIManager.Instance.HideAllHUD(false);
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
        
        // Use the new dialogue system
        if (DialogueDisplayController.Instance != null)
        {
            var dialogueData = DialogueLoader.LoadDialogue(dialogueId);
            return dialogueData != null;
        }
        else
        {
            SBGDebug.LogError("DialogueDisplayController.Instance is null", "PS_Main");
            return false;
        }
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

            // GameMaster.Instance?.oe_PhoneCallEvent?.Invoke(phoneID, number, PhoneCallEvent.Outgoing);
            PhoneEvents.RaisePhoneCallMade(phoneID, number, PhoneCallEvent.Outgoing);

            try
            {
                SBGDebug.LogInfo($"Connected to {number}. Loading dialogue...", "PS_Main");
                if (DialogueDisplayController.Instance != null)
                {
                    DialogueDisplayController.Instance.StartDialogue(phoneNumbers[number]);
                }
                else
                {
                    SBGDebug.LogError("DialogueDisplayController.Instance is null", "PS_Main");
                    StartCoroutine(InvalidCall(number));
                }
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

        // Fail safe - ensure dialogue is closed
        if (DialogueDisplayController.Instance != null && DialogueDisplayController.Instance.IsDialogueActive)
        {
            DialogueDisplayController.Instance.EndDialogue();
        }
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
    
    #region Event Handlers
    
    /// <summary>
    /// Called when any dialogue ends - used to clean up phone state if needed
    /// </summary>
    private void OnDialogueEnded()
    {
        // If we're using the phone and dialogue ends, we should deactivate the phone
        if (usingPhone)
        {
            SBGDebug.LogInfo("Dialogue ended while using phone - deactivating phone", "PS_Main");
            DeactivePhone();
        }
    }

    #endregion

    private void OnDestroy()
    {
        RuntimeManager.StudioSystem.getBus("bus:/Pay Phone", out FMOD.Studio.Bus phoneBank);
        phoneBank.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);

        LevelEvents.PlayerControllerInstantiated -= _GetPlayer;
        DialogueEvents.DialogueEnded -= OnDialogueEnded;
    }
}
