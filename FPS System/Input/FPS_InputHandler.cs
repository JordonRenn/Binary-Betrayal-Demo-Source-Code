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
    
    //[Header("UI Input Module")]
    //[Space(10)]

    //[SerializeField] private InputSystemUIInputModule UIInputModule;

    [Header("Input Action Asset")]
    [Space(10)]
    
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Ref")]
    [Space(10)]

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
    private const string lint_Interact = "lint_Interact";  // Add this line
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

    private const string menu_CursorMove = "menu_CursorMove";
    private const string menu_Move = "menu_Move";
    private const string menu_Click = "menu_Click";
    private const string menu_Cancel = "menu_Cancel";
    private const string menu_Dev = "menu_Dev";
    private const string menu_Confirm = "menu_Confirm";

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
    private InputAction lint_InteractAction;  // Add this line
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

    private InputAction menu_CursorMoveAction;
    private InputAction menu_MoveAction;
    private InputAction menu_ClickAction;
    private InputAction menu_CancelAction;
    private InputAction menu_DevAction;
    private InputAction menu_ConfirmAction;

    //Cutscene input actions

    [SerializeField] private float horizontalLookSensitivity = 1.0f;
    [SerializeField] private float verticalLookSensitivity = 1.0f;
    private float horizontalSensitivityMultiplier = 1.0f; 
    private float verticalSensitivityMultiplier = 1.0f;
    private bool invertYAxis = false;

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
    public bool Lint_InteractInput {get ; private set;}  // Add this line
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

    //Menu Navigation

    public Vector2 Menu_CursorMoveInput {get ; private set;}
    public Vector2 Menu_MoveInput {get ; private set;}
    public bool Menu_ClickInput {get ; private set;}
    public bool Menu_CancelInput {get ; private set;}
    public bool Menu_DevInput {get ; private set;}
    public bool Menu_ConfirmInput {get ; private set;}

    //Cutscene

    [Header("FPS Unity Events")]
    [Space(10)]
    
   [HideInInspector]  public UnityEvent slowWalkTriggered;
    [HideInInspector] public UnityEvent crouchTriggered;
    [HideInInspector] public UnityEvent crouchReleased;
    [HideInInspector] public UnityEvent jumpTriggered;
    [HideInInspector] public UnityEvent interactTriggered;
    [HideInInspector] public UnityEvent cancelTriggered;
    [HideInInspector] public UnityEvent pauseMenuButtonTriggered;
    [HideInInspector] public UnityEvent equipMenuButtonTriggered;
    [HideInInspector] public UnityEvent aimTriggered;
    [HideInInspector] public UnityEvent fireTriggered;
    [HideInInspector] public UnityEvent reloadTriggered;
    [HideInInspector] public UnityEvent swapTriggered;
    [HideInInspector] public UnityEvent activatePrimaryTriggered;
    [HideInInspector] public UnityEvent activateSecondaryTriggered;
    [HideInInspector] public UnityEvent activateUtilLeftTriggered;
    [HideInInspector] public UnityEvent activateUtilRightTriggered;
    [HideInInspector] public UnityEvent activateUnarmedTriggered;

    [Header("Locked Interaction Unity Events")]
    [Space(10)]

    [HideInInspector] public UnityEvent lint_CursorMoveTriggered;
    [HideInInspector] public UnityEvent lint_ClickTriggered;
    [HideInInspector] public UnityEvent lint_CancelTriggered;
    [HideInInspector] public UnityEvent lint_InteractTriggered; 
    [HideInInspector] public UnityEvent lint_InteractReleased;
    [HideInInspector] public UnityEvent lint_Num_1Triggered;
    [HideInInspector] public UnityEvent lint_Num_2Triggered;
    [HideInInspector] public UnityEvent lint_Num_3Triggered;
    [HideInInspector] public UnityEvent lint_Num_4Triggered;
    [HideInInspector] public UnityEvent lint_Num_5Triggered;
    [HideInInspector] public UnityEvent lint_Num_6Triggered;
    [HideInInspector] public UnityEvent lint_Num_7Triggered;
    [HideInInspector] public UnityEvent lint_Num_8Triggered;
    [HideInInspector] public UnityEvent lint_Num_9Triggered;
    [HideInInspector] public UnityEvent lint_Num_0Triggered;

    [Header("Menu Navigation Unity Events")]
    [Space(10)]

    [HideInInspector] public UnityEvent menu_ClickTriggered;
    [HideInInspector] public UnityEvent menu_CancelTriggered;
    [HideInInspector] public UnityEvent menu_DevTriggered;
    [HideInInspector] public UnityEvent menu_MovePerformed;
    [HideInInspector] public UnityEvent menu_ConfirmTriggered;

    public InputState currentState {get ; private set;}
    [SerializeField] private InputState defaultState = InputState.FirstPerson;

    /// <summary>
    /// Initializes the input handler.
    /// </summary>
    void Awake()
    {
        currentState = defaultState;
        
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(Instance);
        
        /* // Find the UI input module if it's not already assigned
        if (UIInputModule == null)
        {
            UIInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
            if (UIInputModule == null)
            {
                Debug.LogWarning("UIInputModule not assigned and couldn't be found automatically. UI input functionality may be limited.");
            }
            else
            {
                Debug.Log("UIInputModule was auto-assigned at runtime.");
            }
        } */

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
        lint_InteractAction = actionMap_LockedInteraction.FindAction(lint_Interact);  // Add this line
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

        menu_CursorMoveAction = actionMap_MenuNav.FindAction(menu_CursorMove);
        menu_MoveAction = actionMap_MenuNav.FindAction(menu_Move);
        menu_ClickAction = actionMap_MenuNav.FindAction(menu_Click);
        menu_CancelAction = actionMap_MenuNav.FindAction(menu_Cancel);
        menu_DevAction = actionMap_MenuNav.FindAction(menu_Dev);
        menu_ConfirmAction = actionMap_MenuNav.FindAction(menu_Confirm);

        RegisterInputActions();

        // Apply settings at initialization
        UpdateSensitivitySettings();
    }

    /// <summary>
    /// Registers the input actions.
    /// </summary>
    private void RegisterInputActions()
    {
        //FPS Actions
        
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => {
            Vector2 input = context.ReadValue<Vector2>();
            // Apply sensitivity multipliers
            input.x *= horizontalSensitivityMultiplier;
            input.y *= verticalSensitivityMultiplier * (invertYAxis ? -1 : 1);
            LookInput = input;
        };
        lookAction.canceled += context => LookInput = Vector2.zero;

        slowWalkAction.started += context => slowWalkTriggered.Invoke();
        slowWalkAction.performed += context => SlowWalkInput = true;
        slowWalkAction.canceled += context => SlowWalkInput = false;

        //crouchAction.started += context => FPSS_CharacterController.Instance.StartCrouch();
        crouchAction.started += context => crouchTriggered.Invoke();
        crouchAction.performed += context => CrouchInput = true;
        //crouchAction.canceled += context => FPSS_CharacterController.Instance.StopCrouch();
        crouchAction.canceled += context => crouchReleased.Invoke();
        crouchAction.canceled += context => CrouchInput = false;

        //jumpAction.started += context => FPSS_CharacterController.Instance.Jump();
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

        //fireAction.started += context => FPSS_WeaponPool.Instance.Fire();
        fireAction.started += context => fireTriggered.Invoke();
        fireAction.performed += context => FireInput = true;
        fireAction.canceled += context => FireInput = false;

        //reloadAction.started += context => FPSS_WeaponPool.Instance.Reload();
        reloadAction.started += context => reloadTriggered.Invoke();
        reloadAction.performed += context => ReloadInput = true;
        reloadAction.canceled += context => ReloadInput = false;

        //swapSlotAction.started += context => FPSS_WeaponPool.Instance.SwapPrimarySecondary();
        swapSlotAction.started += context => swapTriggered.Invoke();
        swapSlotAction.performed += context => SwapSlotInput = true;
        swapSlotAction.canceled += context => SwapSlotInput = false;

        //weaponPrimaryAction.started += context => FPSS_WeaponPool.Instance.SelectPrimary();
        weaponPrimaryAction.started += context => activatePrimaryTriggered.Invoke();
        weaponPrimaryAction.performed += context => ActivatePrimaryWeaponSlotInput = true;
        weaponPrimaryAction.canceled += context => ActivatePrimaryWeaponSlotInput = false;
        
        //weaponSecondaryAction.started += context => FPSS_WeaponPool.Instance.SelectSecondary();
        weaponSecondaryAction.started += context => activateSecondaryTriggered.Invoke();
        weaponSecondaryAction.performed += context => ActivateSecondaryWeaponSlotInput = true;
        weaponSecondaryAction.canceled += context => ActivateSecondaryWeaponSlotInput = false;

        utilLeftAction.performed += context => UseUtilLeftInput = true;
        utilLeftAction.canceled += context => UseUtilLeftInput = false;

        utilRightAction.performed += context => UseUtilRightInput = true;
        utilRightAction.canceled += context => UseUtilRightInput = false;

        //unarmedAction.started += context => FPSS_WeaponPool.Instance.SelectUnarmed();
        unarmedAction.performed += context => ActivateUnarmedInput = true;
        unarmedAction.canceled += context => ActivateUnarmedInput = false;

        //LOCKED INTERACTION ACTIONS

        lint_CursorMoveAction.performed += context => Lint_CursorMoveInput = context.ReadValue<Vector2>();
        lint_CursorMoveAction.canceled += context => Lint_CursorMoveInput = Vector2.zero;

        lint_ClickAction.started += context => lint_ClickTriggered.Invoke();
        lint_ClickAction.performed += context => Lint_ClickInput = true;
        lint_ClickAction.canceled += context => Lint_ClickInput = false;

        lint_InteractAction.started += context => lint_InteractTriggered.Invoke();
        lint_InteractAction.performed += context => Lint_InteractInput = true;
        lint_InteractAction.canceled += context => lint_InteractReleased.Invoke();
        lint_InteractAction.canceled += context => Lint_InteractInput = false;

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

        menu_CursorMoveAction.performed += context => Menu_CursorMoveInput = context.ReadValue<Vector2>();
        menu_CursorMoveAction.canceled += context => Menu_CursorMoveInput = Vector2.zero;

        //menu_MoveAction.performed += context => Menu_MoveInput = context.ReadValue<Vector2>();
        menu_MoveAction.performed += context => 
        {
            Menu_MoveInput = context.ReadValue<Vector2>();
            menu_MovePerformed.Invoke();
        };
        menu_MoveAction.canceled += context => Menu_MoveInput = Vector2.zero;

        menu_ClickAction.started += context => menu_ClickTriggered.Invoke();
        menu_ClickAction.performed += context => Menu_ClickInput = true;
        menu_ClickAction.canceled += context => Menu_ClickInput = false;

        menu_CancelAction.started += context => menu_CancelTriggered.Invoke();
        menu_CancelAction.performed += context => Menu_CancelInput = true;
        menu_CancelAction.canceled += context => Menu_CancelInput = false;

        menu_DevAction.started += context => menu_DevTriggered.Invoke();
        menu_DevAction.performed += context => Menu_DevInput = true;
        menu_DevAction.canceled += context => Menu_DevInput = false;

        menu_ConfirmAction.started += context => menu_ConfirmTriggered.Invoke();
        menu_ConfirmAction.performed += context => Menu_ConfirmInput = true;
        menu_ConfirmAction.canceled += context => Menu_ConfirmInput = false;

        //CUTSCENE ACTIONS

        //
    }

    /// <summary>
    /// Updates sensitivity settings from the GameMaster
    /// </summary>
    public void UpdateSensitivitySettings()
    {
        if (GameMaster.Instance != null)
        {
            PlayerSettings settings = GameMaster.Instance.GetSettings();
            horizontalSensitivityMultiplier = settings.GetHorizontalSensitivityMultiplier();
            verticalSensitivityMultiplier = settings.GetVerticalSensitivityMultiplier();
            invertYAxis = settings.invertYAxis;
            
            SBGDebug.LogInfo($"Mouse sensitivity updated - H: {horizontalSensitivityMultiplier}, V: {verticalSensitivityMultiplier}, InvertY: {invertYAxis}", "FPS_InputHandler");
        }
    }

    /// <summary>
    /// Set the input state on enable.
    /// </summary>
    void OnEnable()
    {   
        SetInputState(defaultState); //maybe need to use default state here????
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
                CursorLock(true);
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_FPS).Enable();
                
                currentState = state;
                break;
            case InputState.MenuNavigation:
                CursorLock(false);
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Enable();

                currentState = state;
                break;
            case InputState.LockedInteraction:
                CursorLock(false);
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Enable();

                currentState = state;
                break;
            case InputState.Cutscene:
                CursorLock(false);
                playerControls.FindActionMap(actionMapName_FPS).Disable();
                playerControls.FindActionMap(actionMapName_MenuNav).Disable();
                playerControls.FindActionMap(actionMapName_LockedInteraction).Disable();
                playerControls.FindActionMap(actionMapName_Cutscene).Enable();

                currentState = state;
                break;
        }

        //SetUIModuleValues(state);
    }

    public void CursorLock(bool state)
    {
        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    //UI Module Values
    /* void SetUIModuleValues(InputState state)
    {
        if (UIInputModule == null)
        {
            Debug.LogError("UIInputModule is null in SetUIModuleValues. UI input won't work correctly.");
            return;
        }

        switch (state)
        {
            case InputState.FirstPerson:
                //UIInputModule.move.Set(playerControls, actionMapName_FPS, move);
                //UIInputModule.point.Set(playerControls, actionMapName_FPS, look);
                //UIInputModule.leftClick.Set(playerControls, actionMapName_FPS, fire);
                //UIInputModule.rightClick.Set(playerControls, actionMapName_FPS, aim);
                //UIInputModule.cancel.Set(playerControls, actionMapName_FPS, cancel);
                break;
            case InputState.MenuNavigation:
                UIInputModule.move.Set(menu_MoveAction);
                UIInputModule.point.Set(menu_CursorMoveAction);
                UIInputModule.leftClick.Set(menu_ClickAction);
                UIInputModule.cancel.Set(menu_CancelAction);
                UIInputModule.submit.Set(menu_ConfirmAction);
                break;
            case InputState.LockedInteraction:
                //UIInputModule.move.Set(lint_CursorMoveAction);
                UIInputModule.point.Set(lint_CursorMoveAction);
                UIInputModule.leftClick.Set(lint_ClickAction);
                UIInputModule.cancel.Set(lint_CancelAction);
                break;
            case InputState.Cutscene:
                break;
        }
    } */
}
