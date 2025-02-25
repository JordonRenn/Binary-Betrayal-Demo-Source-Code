using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using FMODUnity;

public class PS_Main : Interactable
{
    private FPSS_WeaponHUD c_WeaponHud;
    private FPSS_ReticleSystem c_ReticleSystem;    
    private FPS_InputHandler c_InputHandler;
    private GameObject playerObj;
    private NotificationSystem c_NotificationSystem;
    [SerializeField] private GameObject fauxArms;
    private int phoneID = 0;
    [SerializeField] private CinemachineCamera c_PhoneCam;
    [SerializeField] private PS_Keypad c_Keypad;
    [SerializeField] private Collider interactCollider;
    [SerializeField] private Transform playerTeleportPoint;

    [SerializeField] private Animator payphoneAnimator;
    private bool usingPhone = false;

    [Header("FMOD")]
    [SerializeField] public EventReference phonePickupSound;
    [SerializeField] public EventReference phoneHangupSound;
    

    void Start()
    {
        phoneID = Random.Range(1000, 9999); //debugging?
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(1f);
        Init();
    }

    private void Init()
    {
        try
        {
            float timeout = 10f; // 5 seconds timeout
            float startTime = Time.time;

            while (c_WeaponHud == null && Time.time - startTime < timeout) // WEAPON HUD
            {
                c_WeaponHud = FindFirstObjectByType<FPSS_WeaponHUD>();
                Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for WeaponHUD");
            }

            if (c_WeaponHud == null)
            {
                Debug.LogError($"PHONE SYSTEM {phoneID}: WeaponHUD not found within timeout");
                return;
            }

            Debug.Log($"PHONE SYSTEM {phoneID}: {c_WeaponHud} found");

            startTime = Time.time;
            while (c_ReticleSystem == null && Time.time - startTime < timeout) // RETICLE SYSTEM
            {
                c_ReticleSystem = FindFirstObjectByType<FPSS_ReticleSystem>();
                Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for ReticleSystem");
            }

            if (c_ReticleSystem == null)
            {
                Debug.LogError($"PHONE SYSTEM {phoneID}: ReticleSystem not found within timeout");
                return;
            }

            Debug.Log($"PHONE SYSTEM {phoneID}: {c_ReticleSystem} found");

            startTime = Time.time;
            while (playerObj == null && Time.time - startTime < timeout) // PLAYER OBJECT
            {
                playerObj = GameObject.FindWithTag("Player");
                Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for Player Object");
            }

            if (playerObj == null)
            {
                Debug.LogError($"PHONE SYSTEM {phoneID}: Player Object not found within timeout");
                return;
            }

            Debug.Log($"PHONE SYSTEM {phoneID}: {playerObj} found");

            startTime = Time.time;
            while (c_InputHandler == null && Time.time - startTime < timeout) // INPUT HANDLER
            {
                c_InputHandler = FPS_InputHandler.Instance;
                Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for InputHandler");
            }

            if (c_InputHandler == null)
            {
                Debug.LogError($"PHONE SYSTEM {phoneID}: InputHandler not found within timeout");
                return;
            }

            Debug.Log($"PHONE SYSTEM {phoneID}: {c_InputHandler} found");

            while (c_NotificationSystem == null && Time.time - startTime < timeout) // NOTIFICATION SYSTEM
            {
                c_NotificationSystem = FindFirstObjectByType<NotificationSystem>();
                Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for NotificationSystem");
            }

            if (c_NotificationSystem == null)
            {
                Debug.LogError($"PHONE SYSTEM {phoneID}: NotificationSystem not found within timeout");
                return;
            }

            Debug.Log($"PHONE SYSTEM {phoneID}: {c_NotificationSystem} found");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PHONE SYSTEM {phoneID}: Exception occurred during initialization: {ex.Message}");
        }
    }

    public override void Interact()
    {
        if (!usingPhone)
        {
           StartCoroutine(ActivePhone());
        }
    }

    private IEnumerator ActivePhone()
    {
        usingPhone = true;

        FPSS_WeaponPool.Instance.currentWeaponSlotObject.ToggleWeaponActive();

        c_InputHandler.SetInputState(InputState.LockedInteraction);
        c_InputHandler.lint_CancelTriggered.AddListener(DeactivePhone);
        interactCollider.enabled = false;

        c_WeaponHud.Hide();
        c_ReticleSystem.Hide();

        yield return new WaitForSeconds(0.2f);
        
        c_PhoneCam.Priority = 300;

        yield return new WaitForSeconds(0.5f); //allow time for the camera to fully transistion

        c_NotificationSystem.DisplayNotification(new Notification("Dial 616-6174 to save your progress", NotificationType.Normal));
        
        fauxArms.SetActive(true);
        payphoneAnimator.SetTrigger("payphone_pickup");
        StartCoroutine(TeleportPlayer());

        yield return new WaitForSeconds(0.45f);

        RuntimeManager.PlayOneShot(phonePickupSound, transform.position);

        yield return new WaitForSeconds(0.85f); 

        c_Keypad.enabled = true;
        FPSS_PlayerCamController.Instance.ToggleCursorLock();
    }

    private void DeactivePhone()
    {
        StartCoroutine(DeactivePhoneRoutine());
    }

    private IEnumerator DeactivePhoneRoutine()
    {
        c_Keypad.enabled = false;
        c_InputHandler.lint_CancelTriggered.RemoveListener(DeactivePhone);

        payphoneAnimator.SetTrigger("payphone_hangup");

        yield return new WaitForSeconds(0.85f);
        
        RuntimeManager.PlayOneShot(phoneHangupSound, transform.position);

        yield return new WaitForSeconds(0.45f);
        
        fauxArms.SetActive(false);
        
        c_PhoneCam.Priority = 0;
        
        FPSS_PlayerCamController.Instance.AllowOverride(false);
        FPSS_PlayerCamController.Instance.ToggleCursorLock();
        
        interactCollider.enabled = true;
        usingPhone = false;

        FPSS_WeaponPool.Instance.currentWeaponSlotObject.ToggleWeaponActive();

        yield return new WaitForSeconds(0.2f); //allow time for weapon to be re-enabled
        
        c_InputHandler.SetInputState(InputState.FirstPerson);

        c_WeaponHud.Hide();
        c_ReticleSystem.Hide();
    }

    public void DialNumber(string number)
    {
        Debug.Log("Dialed number: " + number);
        // Dummy method for now
    }

    private IEnumerator TeleportPlayer()
    {
        
        FPSS_PlayerCamController.Instance.AllowOverride(true);
        
        CharacterController characterController = playerObj.GetComponent<CharacterController>();
        Rigidbody playerRigidbody = playerObj.GetComponent<Rigidbody>();

        FPSS_PlayerCamController.Instance.SetRotation(new Vector2(playerTeleportPoint.transform.eulerAngles.x, playerTeleportPoint.transform.eulerAngles.y + 90f));

        if (characterController != null)
        {
            characterController.enabled = false; // Disable the CharacterController to change the position
            yield return new WaitForSeconds(0.05f);
            playerObj.transform.position = playerTeleportPoint.position;
            yield return new WaitForSeconds(0.05f);
            characterController.enabled = true; // Re-enable the CharacterController
        }
        else
        {
            playerObj.transform.position = playerTeleportPoint.position;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero; // Reset the Rigidbody velocity
        }

        FPSS_PlayerCamController.Instance.AllowOverride(false);

        Debug.Log("Player teleported to: " + playerTeleportPoint.position);
    }   
}
