using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton class to handle all the input actions for the FPS system.
/// </summary>
public class FPS_InputHandler : MonoBehaviour
{
    public static FPS_InputHandler Instance {get ; private set;} 

    [Header("Input Action Class")]
    [Space(10)]
    
    [SerializeField] private InputActionAsset playerControls;
    [Space(10)]

    [Header("Action Map Name Ref")]
    [Space(5)]

    [SerializeField] private const string actionMapName = "fpsControls";

    //[Header("Locomotion Action Name Refs")]
    
    private const string move = "Move";
    private const string look = "Look";
    private const string slowWalk = "Slow Walk";
    private const string crouch = "Crouch";
    private const string jump = "Jump";

    //[Header("Interaction Action Name Refs")]
    
    private const string interact = "Interact";
    private const string cancel = "Cancel";

    //[Header("Menu Action Name Refs")]

    private const string menuEquip = "Equipment Menu";
    private const string menuPause = "Pause Menu";

    //[Header("FPS Action Name Refs")]

    private const string aim = "Aim";
    private const string fire = "Fire";
    private const string reload = "Reload";
    private const string swapSlot = "Switch Weapon Slot";
    private const string weaponPrimary = "Equip Weapon Slot 1";
    private const string weaponSecondary = "Equip Weapon Slot 2";
    private const string utilLeft = "use Util Slot 1";
    private const string utilRight = "use Util Slot 2";
    private const string unarmed = "Equip Unarmed";

    private InputAction[] inputActions;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction slowWalkAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction cancelAction;
    private InputAction menuEquipAction;
    private InputAction menuPauseAction;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction swapSlotAction;
    private InputAction weaponPrimaryAction;
    private InputAction weaponSecondaryAction;
    private InputAction utilLeftAction;
    private InputAction utilRightAction;
    private InputAction unarmedAction;

    public Vector2 MoveInput {get ; private set;}
    public Vector2 LookInput {get ; private set;}
    public bool SlowWalkInput {get ; private set;}
    public bool CrouchInput {get ; private set;}
    public bool JumpInput {get ; private set;}
    public bool InteractInput {get ; private set;}
    public bool CancelInput {get ; private set;}
    public bool PauseMenuButtonInput {get ; private set;}
    public bool EquipmentMenuButtonInput {get ; private set;}
    public bool AimInput {get ; private set;}
    public bool FireInput {get ; private set;}
    public bool ReloadInput { get; private set; }
    public bool SwapSlotInput {get ; private set;}
    public bool ActivatePrimaryWeaponSlotInput {get ; private set;}
    public bool ActivateSecondaryWeaponSlotInput {get ; private set;}
    public bool UseUtilLeftInput {get ; private set;}
    public bool UseUtilRightInput {get ; private set;}
    public bool ActivateUnarmedInput {get ; private set;}

    [Header("FPS Unity Events")]
    [Space(10)]
    
    public UnityEvent slowWalkTriggered;
    public UnityEvent crouchTriggered;
    public UnityEvent jumpTriggered;
    public UnityEvent interactTriggered;
    public UnityEvent cancelTriggered;
    public UnityEvent pauseMenuButtonTriggered;
    public UnityEvent equipMenuButtonTriggered;
    public UnityEvent aimTriggered;
    public UnityEvent fireTriggered;
    public UnityEvent reloadTriggered;
    public UnityEvent swapTriggered;
    public UnityEvent activatePrimaryTriggered;
    public UnityEvent activateSecondaryTriggered;
    public UnityEvent activateUtilLeftTriggered;
    public UnityEvent activateUtilRightTriggered;
    public UnityEvent activateUnarmedTriggered;

    /// <summary>
    /// Initializes the input handler.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputActionMap actionMap = playerControls.FindActionMap(actionMapName);

        moveAction = actionMap.FindAction(move);
        lookAction = actionMap.FindAction(look);
        slowWalkAction = actionMap.FindAction(slowWalk);
        crouchAction = actionMap.FindAction(crouch);
        jumpAction = actionMap.FindAction(jump);
        interactAction = actionMap.FindAction(interact);
        cancelAction = actionMap.FindAction(cancel);
        menuPauseAction = actionMap.FindAction(menuPause);
        menuEquipAction = actionMap.FindAction(menuEquip);
        aimAction = actionMap.FindAction(aim);
        fireAction = actionMap.FindAction(fire);
        reloadAction = actionMap.FindAction(reload);
        swapSlotAction = actionMap.FindAction(swapSlot);
        weaponPrimaryAction = actionMap.FindAction(weaponPrimary);
        weaponSecondaryAction = actionMap.FindAction(weaponSecondary);
        utilLeftAction = actionMap.FindAction(utilLeft);
        utilRightAction = actionMap.FindAction(utilRight);
        unarmedAction = actionMap.FindAction(unarmed);

        inputActions = new InputAction[]
        {
            moveAction, 
            lookAction, 
            slowWalkAction, 
            crouchAction, 
            jumpAction,
            interactAction, 
            cancelAction, 
            menuPauseAction, 
            menuEquipAction,
            aimAction, 
            fireAction, 
            swapSlotAction, 
            weaponPrimaryAction,
            weaponSecondaryAction, 
            utilLeftAction, 
            utilRightAction, 
            unarmedAction,
            reloadAction
        };

        RegisterInputActions();
    }

    /// <summary>
    /// Registers the input actions.
    /// </summary>
    private void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        slowWalkAction.started += context => slowWalkTriggered.Invoke();
        slowWalkAction.performed += context => SlowWalkInput = true;
        slowWalkAction.canceled += context => SlowWalkInput = false;

        crouchAction.started += context => FPSS_CharacterController.Instance.StartCrouch();
        crouchAction.started += context => crouchTriggered.Invoke();
        crouchAction.performed += context => CrouchInput = true;
        crouchAction.canceled += context => FPSS_CharacterController.Instance.StopCrouch();
        crouchAction.canceled += context => CrouchInput = false;

        jumpAction.started += context => FPSS_CharacterController.Instance.Jump();
        jumpAction.started += context => jumpTriggered.Invoke();
        jumpAction.performed += context => JumpInput = true;
        jumpAction.canceled += context => JumpInput = false;

        interactAction.started += context => interactTriggered.Invoke();
        interactAction.performed += context => InteractInput = true;
        interactAction.canceled += context => InteractInput = false;

        cancelAction.started += context => cancelTriggered.Invoke();
        cancelAction.performed += context => CancelInput = true;
        cancelAction.canceled += context => CancelInput = false;

        menuPauseAction.started += context => pauseMenuButtonTriggered.Invoke();
        menuPauseAction.performed += context => PauseMenuButtonInput = true;
        menuPauseAction.canceled += context => PauseMenuButtonInput = false;

        menuEquipAction.started += context => equipMenuButtonTriggered.Invoke();
        menuEquipAction.performed += context => EquipmentMenuButtonInput = true;
        menuEquipAction.canceled += context => EquipmentMenuButtonInput = false;

        //aimAction.started += context => FPSS_WeaponPool.Instance.urrentWeapon.Aim();
        aimAction.performed += context => AimInput = true;
        aimAction.canceled += context => AimInput = false;

        fireAction.started += context => FPSS_WeaponPool.Instance.Fire();
        fireAction.started += context => fireTriggered.Invoke();
        fireAction.performed += context => FireInput = true;
        fireAction.canceled += context => FireInput = false;

        reloadAction.started += context => FPSS_WeaponPool.Instance.Reload();
        reloadAction.started += context => reloadTriggered.Invoke();
        reloadAction.performed += context => ReloadInput = true;
        reloadAction.canceled += context => ReloadInput = false;

        swapSlotAction.started += context => FPSS_WeaponPool.Instance.SwapPrimarySecondary();
        swapSlotAction.started += context => swapTriggered.Invoke();
        swapSlotAction.performed += context => SwapSlotInput = true;
        swapSlotAction.canceled += context => SwapSlotInput = false;

        weaponPrimaryAction.started += context => FPSS_WeaponPool.Instance.SelectPrimary();
        weaponPrimaryAction.performed += context => ActivatePrimaryWeaponSlotInput = true;
        weaponPrimaryAction.canceled += context => ActivatePrimaryWeaponSlotInput = false;
        
        weaponSecondaryAction.started += context => FPSS_WeaponPool.Instance.SelectSecondary();
        weaponSecondaryAction.performed += context => ActivateSecondaryWeaponSlotInput = true;
        weaponSecondaryAction.canceled += context => ActivateSecondaryWeaponSlotInput = false;

        utilLeftAction.performed += context => UseUtilLeftInput = true;
        utilLeftAction.canceled += context => UseUtilLeftInput = false;

        utilRightAction.performed += context => UseUtilRightInput = true;
        utilRightAction.canceled += context => UseUtilRightInput = false;

        unarmedAction.started += context => FPSS_WeaponPool.Instance.SelectUnarmed();
        unarmedAction.performed += context => ActivateUnarmedInput = true;
        unarmedAction.canceled += context => ActivateUnarmedInput = false;
    }

    /// <summary>
    /// Enables the input actions.
    /// </summary>
    void OnEnable()
    {   
        foreach (var action in inputActions)
        {
            action.Enable();
        }
    }

    /// <summary>
    /// Disables the input actions.
    /// </summary>
    void OnDisable()
    {
        foreach (var action in inputActions)
        {
            action.Disable();
        }
    }

    public void ToggleMovement(bool state)
    {
        if (!state)
        {
            moveAction.Disable();
            lookAction.Disable();
            slowWalkAction.Disable();
            crouchAction.Disable();
            jumpAction.Disable();
        }
        else
        {
            moveAction.Enable();
            lookAction.Enable();
            slowWalkAction.Enable();
            crouchAction.Enable();
            jumpAction.Enable();
        }
    }

    public void ToggleFPSActions(bool state)
    {
        if (!state)
        {
            aimAction.Disable();
            fireAction.Disable();
            reloadAction.Disable();
            swapSlotAction.Disable();
            weaponPrimaryAction.Disable();
            weaponSecondaryAction.Disable();
            utilLeftAction.Disable();
            utilRightAction.Disable();
            unarmedAction.Disable();
        }
        else
        {
            aimAction.Enable();
            fireAction.Enable();
            reloadAction.Enable();
            swapSlotAction.Enable();
            weaponPrimaryAction.Enable();
            weaponSecondaryAction.Enable();
            utilLeftAction.Enable();
            utilRightAction.Enable();
            unarmedAction.Enable();
        }
    }
}
