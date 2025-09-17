using UnityEngine;
using System.Collections;
using GlobalEvents;

/* 
    First Person Controller Hierarchy:
    
    **Game Object Name (Script Name)**

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)           <--- THIS SCRIPT
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs)           
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                  
                    - POS_GUN_AUDIO
                    - 0_0_Ak-47 (Gun_AK47.cs)
                        - AK_47
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        // Need to make "Gun_SniperRifle.cs"
                    - 1_0_HandGun (Gun_HandGun.cs)
                        - HandGun
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 1_1_ShotGun (FPSS_WeaponSlotObject.cs)            // Need to make "Gun_ShotGun.cs"
                    - 2_0_Knife (FPSS_WeaponSlotObject.cs)              // Need to make "Melee_Knife.cs"
                    - 3_0_Grenade (FPSS_WeaponSlotObject.cs)            // Need to make "Grenade.cs"
                    - 3_1_FlashGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "FlashGrenade.cs"
                    - 3_2_SmokeGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "SmokeGrenade.cs"
                    - 4_0_Unarmed (FPSS_WeaponSlotObject.cs)            // Need to make "Unarmed.cs"
 */

public class FirstPersonCamController : MonoBehaviour
{
    public static FirstPersonCamController Instance { get; private set; }

    [SerializeField] public GameObject playerObject;

    [Header("Look Settings")]
    private float baseSensitivity = 50f;  // Base sensitivity value
    private float sensitivityX;           // Will be calculated from settings
    private float sensitivityY;           // Will be calculated from settings
    private bool invertY;                 // Will be set from settings

    private Vector2 lookInput;
    float xRotation;
    float yRotation;

    private float initDelay = 0.25f;    //used to pause execution between steps of initialization when needed
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    private bool isOverridden = false;

    //private bool debugMode = false;            //Enable/Disable debug mode

    void Awake()
    {
        Debug.Log("FIRST PERSON CAM CONTROLLER | Instantiated");

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(initDelay);

        // Apply initial settings
        UpdateFromSettings();

        // Subscribe to settings change event
        // GameMaster.Instance.gm_SettingsChanged.AddListener(UpdateFromSettings);
        ConfigEvents.SettingsChanged += UpdateFromSettings;

        initialized = true;
    }

    private void UpdateFromSettings()
    {
        if (GameMaster.Instance != null)
        {
            var settings = GameMaster.Instance.GetSettings();
            sensitivityX = baseSensitivity * settings.GetHorizontalSensitivityMultiplier();
            sensitivityY = baseSensitivity * settings.GetVerticalSensitivityMultiplier();
            invertY = settings.invertYAxis;
        }
    }

    void Update()
    {
        if (!initialized || isOverridden) { return; }

        lookInput = InputHandler.Instance.LookInput;

        // Direct mouse-to-view conversion, scaled by sensitivity and deltaTime
        yRotation += lookInput.x * sensitivityX * Time.deltaTime;
        float verticalInput = invertY ? lookInput.y : -lookInput.y;
        xRotation += verticalInput * sensitivityY * Time.deltaTime;

        //clamp how far up and down you can look
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate using input values
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerObject.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ApplySpread(Vector2 spreadOffset)
    {
        xRotation -= spreadOffset.x;
        yRotation += spreadOffset.y;
    }

    public void SetRotation(Vector3 rotation)
    {
        xRotation = rotation.x;
        yRotation = rotation.y;
    }

    public void AllowOverride(bool allow)
    {
        isOverridden = allow;
    }

    void OnDestroy()
    {
        if (GameMaster.Instance != null)
        {
            // GameMaster.Instance.gm_SettingsChanged.RemoveListener(UpdateFromSettings);
            ConfigEvents.SettingsChanged -= UpdateFromSettings;
        }
    }
}
