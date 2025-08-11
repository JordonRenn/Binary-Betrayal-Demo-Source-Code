using UnityEngine;
using TMPro;

/* 
    First Person Controller Hierarchy: 
    
    **Game Object Name (Script Name)**

    - Character Controller (CharacterMovement.cs)
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs)           <--- THIS SCRIPT
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

/// <summary>
/// Handles player interaction with objects in the game world through raycasting.
/// </summary>
public class FirstPersonInteraction : MonoBehaviour
{
    public static FirstPersonInteraction Instance { get; private set; }

    [SerializeField] float interactionCooldown = 0.25f;
    [SerializeField] float reachDistance = 3f;
    private FPS_InputHandler input;
    private TMP_Text objctInfoText;
    private float lastInteractionTime = 0f;

    private Ray ray;
    private RaycastHit hitInfo;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        input = FPS_InputHandler.Instance;
        objctInfoText = FPSS_ReticleSystem.Instance.objectInfoText;
    }

    /// <summary>
    /// Updates the interaction system each frame, handling input and hover states.
    /// </summary>
    void Update()
    {
        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (input.InteractInput && Time.time >= lastInteractionTime + interactionCooldown)
        {
            AttemptInteraction();
            lastInteractionTime = Time.time;
        }
        else if (objctInfoText == null)
        {
            objctInfoText = FPSS_ReticleSystem.Instance.objectInfoText;
        }
        else
        {
            InteractHover();
        }
    }

    private SauceObject GetSauceObjectFromRaycast()
    {
        if (Physics.Raycast(ray, out hitInfo, reachDistance))
        {
            SauceObject sauceObject = hitInfo.collider.GetComponent<SauceObject>();
            return sauceObject ?? hitInfo.collider.GetComponentInParent<SauceObject>();
        }
        return null;
    }

    /// <summary>
    /// Attempts to interact with an object in front of the player.
    /// Called when the interaction input is triggered.
    /// </summary>
    public void AttemptInteraction()
    {
        //Interactable interactable = GetInteractableFromRaycast();
        SauceObject sauceObject = GetSauceObjectFromRaycast();

        if (sauceObject != null)
        {
            GameMaster.Instance?.oe_InteractionEvent?.Invoke(sauceObject.objectID);
            sauceObject.Interact();
        }
        else
        {
            SBGDebug.LogInfo("No interactable object found in front of the player", "FPSS_Interaction");
        }

    }

    /// <summary>
    /// Handles the hover state when looking at interactable objects.
    /// Updates the UI with object information.
    /// </summary>
    private void InteractHover()
    {
        //Interactable interactable = GetInteractableFromRaycast();
        SauceObject sauceObject = GetSauceObjectFromRaycast();
        if (sauceObject != null)
        {
            ShowObjectInfo(sauceObject);
        }
        else
        {
            objctInfoText.text = "";
            objctInfoText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the UI with information about the currently hovered interactable object.
    /// </summary>
    /// <param name="interactable">The interactable object to show information for.</param>
    private void ShowObjectInfo(SauceObject obj)
    {
        if (objctInfoText != null)
        {
            objctInfoText.text = obj.GetObjectDisplayName();
            objctInfoText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Object Info Text is not assigned in FPSS_Interaction.");
            objctInfoText.text = "Interact";
        }
    }
}
