
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PS_Main : Interactable
{
    private FPSS_WeaponHUD c_WeaponHud;
    private FPSS_ReticleSystem c_ReticleSystem;    
    private FPS_InputHandler c_InputHandler;
    private GameObject playerObj;
    private int phoneID = 0;
    [SerializeField] private CinemachineCamera c_PhoneCam;
    [SerializeField] private PS_Keypad c_Keypad;
    [SerializeField] private Collider interactCollider;
    [SerializeField] private Transform playerTeleportPoint;
    private bool usingPhone = false;

    void Start()
    {
        phoneID = Random.Range(1000, 9999); //debugging?

        Init();
    }

    private void Init()
    {
        while (c_WeaponHud == null)                 // WEAPON HUD
        {
            c_WeaponHud = FindFirstObjectByType<FPSS_WeaponHUD>();
            Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for WeaponHUD");
        }

        Debug.Log($"PHONE SYSTEM {phoneID}: {c_WeaponHud} found");

        while (c_ReticleSystem == null)             // RETICLE SYSTEM
        {
            c_ReticleSystem = FindFirstObjectByType<FPSS_ReticleSystem>();
            Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for ReticleSystem");
        }

        Debug.Log($"PHONE SYSTEM {phoneID}: {c_ReticleSystem} found");
        
        while (playerObj == null)                   // PLAYER OBJECT
        {
            playerObj = GameObject.FindWithTag("Player");
            Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for Player Object");
        }

        Debug.Log($"PHONE SYSTEM {phoneID}: {playerObj} found");
        
        while (c_InputHandler == null)              // INPUT HANDLER
        {
            c_InputHandler = FPS_InputHandler.Instance;
            Debug.Log($"PHONE SYSTEM {phoneID}: Waiting for InputHandler");
        }

        Debug.Log($"PHONE SYSTEM {phoneID}: {c_InputHandler} found");
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

        c_InputHandler.ToggleMovement(false);
        c_InputHandler.ToggleFPSActions(false);
        c_InputHandler.cancelTriggered.AddListener(DeactivePhone);
        interactCollider.enabled = false;

        c_WeaponHud.Hide();
        c_ReticleSystem.Hide();

        yield return new WaitForSeconds(0.2f);
        
        c_PhoneCam.Priority = 300;

        c_Keypad.enabled = true;
        FPSS_PlayerCamController.Instance.ToggleCursorLock();
        
        yield return new WaitForSeconds(0.5f); //allow time for the camera to fully transistion

        StartCoroutine(TeleportPlayer());
    }

    private void DeactivePhone()
    {
        StartCoroutine(DeactivePhoneRoutine());
    }

    private IEnumerator DeactivePhoneRoutine()
    {
        c_PhoneCam.Priority = 0;
        c_InputHandler.cancelTriggered.RemoveListener(DeactivePhone);
        c_Keypad.enabled = false;
        FPSS_PlayerCamController.Instance.AllowOverride(false);
        FPSS_PlayerCamController.Instance.ToggleCursorLock();
        
        usingPhone = false;
        interactCollider.enabled = true;

        FPSS_WeaponPool.Instance.currentWeaponSlotObject.ToggleWeaponActive();

        yield return new WaitForSeconds(0.2f); //allow time for weapon to be re-enabled
        
        c_InputHandler.ToggleMovement(true);
        c_InputHandler.ToggleFPSActions(true);

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
        FPSS_PlayerCamController.Instance.SetRotation(new Vector2(playerTeleportPoint.transform.localRotation.x, playerTeleportPoint.transform.localRotation.y + 90f));
        
        CharacterController characterController = playerObj.GetComponent<CharacterController>();
        Rigidbody playerRigidbody = playerObj.GetComponent<Rigidbody>();

        if (characterController != null)
        {
            characterController.enabled = false; // Disable the CharacterController to change the position
            yield return new WaitForSeconds(0.05f);
            playerObj.transform.position = playerTeleportPoint.position;
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

        Debug.Log("Player teleported to: " + playerTeleportPoint.position);
    }   
}
