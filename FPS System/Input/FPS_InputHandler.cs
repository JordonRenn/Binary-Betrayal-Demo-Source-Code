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

    [Header("Locomotion Action Name Refs")]
    [Space(5)]
    [SerializeField] private const string move = "Move";
    [SerializeField] private const string look = "Look";
    [SerializeField] private const string slowWalk = "Slow Walk";
    [SerializeField] private const string crouch = "Crouch";
    [SerializeField] private const string jump = "Jump";

    [Header("Interaction Action Name Refs")]
    [Space(5)]
    [SerializeField] private const string interact = "Interact";
    [SerializeField] private const string cancel = "Cancel";

    [Header("Menu Action Name Refs")]
    [Space(5)]
    [SerializeField] private const string menuEquip = "Equipment Menu";
    [SerializeField] private const string menuPause = "Pause Menu";

    [Header("FPS Action Name Refs")]
    [Space(5)]
    [SerializeField] private const string aim = "Aim";
    [SerializeField] private const string fire = "Fire";
    [SerializeField] private const string reload = "Reload";
    [SerializeField] private const string swapSlot = "Switch Weapon Slot";
    [SerializeField] private const string weaponPrimary = "Equip Weapon Slot 1";
    [SerializeField] private const string weaponSecondary = "Equip Weapon Slot 2";
    [SerializeField] private const string utilLeft = "use Util Slot 1";
    [SerializeField] private const string utilRight = "use Util Slot 2";
    [SerializeField] private const string unarmed = "Equip Unarmed";

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

    private void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        slowWalkAction.started += context => slowWalkTriggered.Invoke();
        slowWalkAction.performed += context => SlowWalkInput = true;
        slowWalkAction.canceled += context => SlowWalkInput = false;

        crouchAction.started += context => FPSS_CharacterController.Instance.Crouch();
        crouchAction.started += context => crouchTriggered.Invoke();
        crouchAction.performed += context => CrouchInput = true;
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

        aimAction.started += context => aimTriggered.Invoke();
        aimAction.performed += context => AimInput = true;
        aimAction.canceled += context => AimInput = false;

        fireAction.started += context => FPSS_WeaponPool.Instance.FireWeapon();
        fireAction.started += context => fireTriggered.Invoke();
        fireAction.performed += context => FireInput = true;
        fireAction.canceled += context => FireInput = false;

        reloadAction.started += context => FPSS_WeaponPool.Instance.ReloadWeapon();
        reloadAction.started += context => reloadTriggered.Invoke();
        reloadAction.performed += context => ReloadInput = true;
        reloadAction.canceled += context => ReloadInput = false;

        swapSlotAction.started += context => FPSS_WeaponPool.Instance.SwapPrimarySecondary();
        swapSlotAction.started += context => swapTriggered.Invoke();
        swapSlotAction.performed += context => SwapSlotInput = true;
        swapSlotAction.canceled += context => SwapSlotInput = false;

        weaponPrimaryAction.started += context => FPSS_WeaponPool.Instance.SelectPrimary();
        activatePrimaryTriggered.Invoke();
        weaponPrimaryAction.performed += context => ActivatePrimaryWeaponSlotInput = true;
        weaponPrimaryAction.canceled += context => ActivatePrimaryWeaponSlotInput = false;

        weaponSecondaryAction.started += context => FPSS_WeaponPool.Instance.SelectSecondary();
        weaponSecondaryAction.started += context => activateSecondaryTriggered.Invoke();
        weaponSecondaryAction.performed += context => ActivateSecondaryWeaponSlotInput = true;
        weaponSecondaryAction.canceled += context => ActivateSecondaryWeaponSlotInput = false;

        utilLeftAction.started += context => activateUtilLeftTriggered.Invoke();
        utilLeftAction.performed += context => UseUtilLeftInput = true;
        utilLeftAction.canceled += context => UseUtilLeftInput = false;

        utilRightAction.started += context => activateUtilRightTriggered.Invoke();
        utilRightAction.performed += context => UseUtilRightInput = true;
        utilRightAction.canceled += context => UseUtilRightInput = false;

        unarmedAction.started += context => activateUnarmedTriggered.Invoke();
        unarmedAction.performed += context => ActivateUnarmedInput = true;
        unarmedAction.canceled += context => ActivateUnarmedInput = false;
    }

    void OnEnable()
    {   
        foreach (var action in inputActions)
        {
            action.Enable();
        }
    }

    void OnDisable()
    {
        foreach (var action in inputActions)
        {
            action.Disable();
        }
    }
}
