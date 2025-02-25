using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum InputState
{
    FirstPerson,
    MenuNavigation,
    LockedInteraction,
    Cutscene,
}

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

    [SerializeField] private const string actionMapName_FPS = "fpsControls";
    [SerializeField] private const string actionMapName_LockedInteraction = "lockedInteraction";
    [SerializeField] private const string actionMapName_MenuNav = "menuNav";
    [SerializeField] private const string actionMapName_Cutscene = "cutscene";

    //FPS Action Name Refs
    
    private const string move = "Move";
    private const string look = "Look";
    private const string slowWalk = "Slow Walk";
    private const string crouch = "Crouch";
    private const string jump = "Jump";
    private const string interact = "Interact";
    private const string cancel = "Cancel";
    private const string menuEquip = "Equipment Menu";
    private const string menuPause = "Pause Menu";
    private const string aim = "Aim";
    private const string fire = "Fire";
    private const string reload = "Reload";
    private const string swapSlot = "Switch Weapon Slot";
    private const string weaponPrimary = "Equip Weapon Slot 1";
    private const string weaponSecondary = "Equip Weapon Slot 2";
    private const string utilLeft = "use Util Slot 1";
    private const string utilRight = "use Util Slot 2";
    private const string unarmed = "Equip Unarmed";

    //Locked Interaction Action Name Refs

    private const string lint_CursorMove = "lint_CursorMove";
    private const string lint_Click = "lint_Click";
    private const string lint_Cancel = "lint_Cancel";
    private const string lint_Num_1 = "lint_Num_1";
    private const string lint_Num_2 = "lint_Num_2";
    private const string lint_Num_3 = "lint_Num_3";
    private const string lint_Num_4 = "lint_Num_4";
    private const string lint_Num_5 = "lint_Num_5";
    private const string lint_Num_6 = "lint_Num_6";
    private const string lint_Num_7 = "lint_Num_7";
    private const string lint_Num_8 = "lint_Num_8";
    private const string lint_Num_9 = "lint_Num_9";
    private const string lint_Num_0 = "lint_Num_0";

    //Menu Navigation Action Name Refs

    private InputAction[] inputActions_FPS;
    private InputAction[] inputActions_LockedInteraction;
    private InputAction[] inputActions_MenuNav;
    private InputAction[] inputActions_Cutscene;

    //FPS input actions
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

    //Locked Interaction input actions
    private InputAction lint_CursorMoveAction;
    private InputAction lint_ClickAction;
    private InputAction lint_CancelAction;
    private InputAction lint_Num_1Action;
    private InputAction lint_Num_2Action;
    private InputAction lint_Num_3Action;
    private InputAction lint_Num_4Action;
    private InputAction lint_Num_5Action;
    private InputAction lint_Num_6Action;
    private InputAction lint_Num_7Action;
    private InputAction lint_Num_8Action;
    private InputAction lint_Num_9Action;
    private InputAction lint_Num_0Action;

    //Menu Navigation input actions

    //Cutscene input actions


    //FPS
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

    //locked interaction
    public Vector2 Lint_CursorMoveInput {get ; private set;}
    public bool Lint_ClickInput {get ; private set;}
    public bool Lint_CancelInput {get ; private set;}
    public bool Lint_Num_1Input {get ; private set;}
    public bool Lint_Num_2Input {get ; private set;}
    public bool Lint_Num_3Input {get ; private set;}
    public bool Lint_Num_4Input {get ; private set;}
    public bool Lint_Num_5Input {get ; private set;}
    public bool Lint_Num_6Input {get ; private set;}
    public bool Lint_Num_7Input {get ; private set;}
    public bool Lint_Num_8Input {get ; private set;}
    public bool Lint_Num_9Input {get ; private set;}
    public bool Lint_Num_0Input {get ; private set;}

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

    [Header("Locked Interaction Unity Events")]
    [Space(10)]

    public UnityEvent lint_CursorMoveTriggered;
    public UnityEvent lint_ClickTriggered;
    public UnityEvent lint_CancelTriggered;
    public UnityEvent lint_Num_1Triggered;
    public UnityEvent lint_Num_2Triggered;
    public UnityEvent lint_Num_3Triggered;
    public UnityEvent lint_Num_4Triggered;
    public UnityEvent lint_Num_5Triggered;
    public UnityEvent lint_Num_6Triggered;
    public UnityEvent lint_Num_7Triggered;
    public UnityEvent lint_Num_8Triggered;
    public UnityEvent lint_Num_9Triggered;
    public UnityEvent lint_Num_0Triggered;

    private InputState currentState;
    [SerializeField] private InputState defaultState = InputState.FirstPerson;

    /// <summary>
    /// Initializes the input handler.
    /// </summary>
    void Awake()
    {
        currentState = defaultState;
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputActionMap actionMap_FPS = playerControls.FindActionMap(actionMapName_FPS);
        InputActionMap actionMap_LockedInteraction = playerControls.FindActionMap(actionMapName_LockedInteraction);
        InputActionMap actionMap_MenuNav = playerControls.FindActionMap(actionMapName_MenuNav);
        InputActionMap actionMap_Cutscene = playerControls.FindActionMap(actionMapName_Cutscene);

        //FPS actions
        moveAction = actionMap_FPS.FindAction(move);
        lookAction = actionMap_FPS.FindAction(look);
        slowWalkAction = actionMap_FPS.FindAction(slowWalk);
        crouchAction = actionMap_FPS.FindAction(crouch);
        jumpAction = actionMap_FPS.FindAction(jump);
        interactAction = actionMap_FPS.FindAction(interact);
        cancelAction = actionMap_FPS.FindAction(cancel);
        menuPauseAction = actionMap_FPS.FindAction(menuPause);
        menuEquipAction = actionMap_FPS.FindAction(menuEquip);
        aimAction = actionMap_FPS.FindAction(aim);
        fireAction = actionMap_FPS.FindAction(fire);
        reloadAction = actionMap_FPS.FindAction(reload);
        swapSlotAction = actionMap_FPS.FindAction(swapSlot);
        weaponPrimaryAction = actionMap_FPS.FindAction(weaponPrimary);
        weaponSecondaryAction = actionMap_FPS.FindAction(weaponSecondary);
        utilLeftAction = actionMap_FPS.FindAction(utilLeft);
        utilRightAction = actionMap_FPS.FindAction(utilRight);
        unarmedAction = actionMap_FPS.FindAction(unarmed);

        //Locked Interaction actions

        lint_CursorMoveAction = actionMap_LockedInteraction.FindAction(lint_CursorMove);
        lint_ClickAction = actionMap_LockedInteraction.FindAction(lint_Click);
        lint_CancelAction = actionMap_LockedInteraction.FindAction(lint_Cancel);
        lint_Num_1Action = actionMap_LockedInteraction.FindAction(lint_Num_1);
        lint_Num_2Action = actionMap_LockedInteraction.FindAction(lint_Num_2);
        lint_Num_3Action = actionMap_LockedInteraction.FindAction(lint_Num_3);
        lint_Num_4Action = actionMap_LockedInteraction.FindAction(lint_Num_4);
        lint_Num_5Action = actionMap_LockedInteraction.FindAction(lint_Num_5);
        lint_Num_6Action = actionMap_LockedInteraction.FindAction(lint_Num_6);
        lint_Num_7Action = actionMap_LockedInteraction.FindAction(lint_Num_7);
        lint_Num_8Action = actionMap_LockedInteraction.FindAction(lint_Num_8);
        lint_Num_9Action = actionMap_LockedInteraction.FindAction(lint_Num_9);
        lint_Num_0Action = actionMap_LockedInteraction.FindAction(lint_Num_0);

        //Menu Navigation actions

        //Cutscene actions
        

        inputActions_FPS = new InputAction[]
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

        inputActions_LockedInteraction = new InputAction[]
        {
            lint_CursorMoveAction,
            lint_ClickAction,
            lint_CancelAction,
            lint_Num_1Action,
            lint_Num_2Action,
            lint_Num_3Action,
            lint_Num_4Action,
            lint_Num_5Action,
            lint_Num_6Action,
            lint_Num_7Action,
            lint_Num_8Action,
            lint_Num_9Action,
            lint_Num_0Action
        };

        inputActions_MenuNav = new InputAction[]
        {
           //
        };

        inputActions_Cutscene = new InputAction[]
        {
            //
        };

        RegisterInputActions();
    }

    /// <summary>
    /// Registers the input actions.
    /// </summary>
    private void RegisterInputActions()
    {
        //FPS Actions
        
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

        //LOCKED INTERACTION ACTIONS

        lint_CursorMoveAction.performed += context => Lint_CursorMoveInput = context.ReadValue<Vector2>();
        lint_CursorMoveAction.canceled += context => Lint_CursorMoveInput = Vector2.zero;

        lint_ClickAction.started += context => lint_ClickTriggered.Invoke();
        lint_ClickAction.performed += context => Lint_ClickInput = true;
        lint_ClickAction.canceled += context => Lint_ClickInput = false;

        lint_CancelAction.started += context => lint_CancelTriggered.Invoke();
        lint_CancelAction.performed += context => Lint_CancelInput = true;
        lint_CancelAction.canceled += context => Lint_CancelInput = false;

        lint_Num_1Action.started += context => lint_Num_1Triggered.Invoke();
        lint_Num_1Action.performed += context => Lint_Num_1Input = true;
        lint_Num_1Action.canceled += context => Lint_Num_1Input = false;

        lint_Num_2Action.started += context => lint_Num_2Triggered.Invoke();
        lint_Num_2Action.performed += context => Lint_Num_2Input = true;
        lint_Num_2Action.canceled += context => Lint_Num_2Input = false;

        lint_Num_3Action.started += context => lint_Num_3Triggered.Invoke();
        lint_Num_3Action.performed += context => Lint_Num_3Input = true;
        lint_Num_3Action.canceled += context => Lint_Num_3Input = false;

        lint_Num_4Action.started += context => lint_Num_4Triggered.Invoke();
        lint_Num_4Action.performed += context => Lint_Num_4Input = true;
        lint_Num_4Action.canceled += context => Lint_Num_4Input = false;

        lint_Num_5Action.started += context => lint_Num_5Triggered.Invoke();
        lint_Num_5Action.performed += context => Lint_Num_5Input = true;
        lint_Num_5Action.canceled += context => Lint_Num_5Input = false;

        lint_Num_6Action.started += context => lint_Num_6Triggered.Invoke();
        lint_Num_6Action.performed += context => Lint_Num_6Input = true;
        lint_Num_6Action.canceled += context => Lint_Num_6Input = false;

        lint_Num_7Action.started += context => lint_Num_7Triggered.Invoke();
        lint_Num_7Action.performed += context => Lint_Num_7Input = true;
        lint_Num_7Action.canceled += context => Lint_Num_7Input = false;

        lint_Num_8Action.started += context => lint_Num_8Triggered.Invoke();
        lint_Num_8Action.performed += context => Lint_Num_8Input = true;
        lint_Num_8Action.canceled += context => Lint_Num_8Input = false;

        lint_Num_9Action.started += context => lint_Num_9Triggered.Invoke();
        lint_Num_9Action.performed += context => Lint_Num_9Input = true;
        lint_Num_9Action.canceled += context => Lint_Num_9Input = false;

        lint_Num_0Action.started += context => lint_Num_0Triggered.Invoke();
        lint_Num_0Action.performed += context => Lint_Num_0Input = true;
        lint_Num_0Action.canceled += context => Lint_Num_0Input = false;

        //MENU NAVIGATION ACTIONS

        //CUTSCENE ACTIONS

        //
    }

    /// <summary>
    /// Set the input state on enable.
    /// </summary>
    void OnEnable()
    {   
        SetInputState(currentState); //maybe need to use default state here????
    }

    /// <summary>
    /// Disables all input actions on disable.
    /// </summary>
    void OnDisable()
    {
        playerControls.FindActionMap(actionMapName_FPS).Disable();
        playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
        playerControls.FindActionMap(actionMapName_MenuNav).Disable();
        playerControls.FindActionMap(actionMapName_Cutscene).Disable();
    }

    /// <summary>
    /// Pubic method to set the InputState "currentState" and disable/enable the appropriate input actions.
    /// </summary>
    public void SetInputState(InputState state)
    {
        switch (state)
        {
            case InputState.FirstPerson:
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_FPS).Enable();
                currentState = state;
                break;
            case InputState.MenuNavigation:
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Enable();
                currentState = state;
                break;
            case InputState.LockedInteraction:
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Enable();
                currentState = state;
                break;
            case InputState.Cutscene:
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Enable();
                currentState = state;
                break;
        }
    }

    //OBSOLETED LEGACY CODE, DO NOT EDIT BELOW THIS LINE
    //OBSOLETED LEGACY CODE, DO NOT EDIT BELOW THIS LINE
    //OBSOLETED LEGACY CODE, DO NOT EDIT BELOW THIS LINE
    //OBSOLETED LEGACY CODE, DO NOT EDIT BELOW THIS LINE  

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
