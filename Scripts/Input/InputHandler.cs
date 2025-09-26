using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public enum InputState
{
    FirstPerson,
    Focus,
    Cutscene,
    UI
}

/// <summary>
/// Singleton class to handle all the input actions for the FPS system.
/// </summary>
#region Input Handler
public class InputHandler : MonoBehaviour
{
    private static InputHandler _instance;
    public static InputHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(InputHandler)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    [Header("Settings")]
    [Space(10)]

    private InputState defaultState = InputState.FirstPerson;

    private float horizontalLookSensitivity = 1.0f;
    private float verticalLookSensitivity = 1.0f;
    private float horizontalSensitivityMultiplier = 1.0f;
    private float verticalSensitivityMultiplier = 1.0f;
    private bool invertYAxis = false;

    [Header("Input Action Asset")]
    [Space(10)]

    [SerializeField] private InputActionAsset inputActionAsset;

    #region Action Map Names
    private const string actionMapName_FirstPerson = "FirstPerson";
    private const string actionMapName_Focus = "Focus";
    private const string actionMapName_Cutscene = "Cutscene";
    private const string actionMapName_UI = "UI";
    #endregion

    #region Action Name Refs
    // FIRST PERSON ACTION NAME REFS
    private const string fp_an_move = "Move";
    private const string fp_an_look = "Look";
    private const string fp_an_slowWalk = "Slow Walk";
    private const string fp_an_crouch = "Crouch";
    private const string fp_an_jump = "Jump";
    private const string fp_an_interact = "Interact";
    private const string fp_an_slot1 = "Equip Weapon Slot 1";
    private const string fp_an_slot2 = "Equip Weapon Slot 2";
    private const string fp_an_melee = "Equip Melee";
    private const string fp_an_utility = "Equip Utility";
    private const string fp_an_unarmed = "Equip Unarmed";
    private const string fp_an_swap = "Weapon Swap";
    private const string fp_an_aim = "Aim";
    private const string fp_an_fire = "Fire";
    private const string fp_an_reload = "Reload";

    private const string fp_an_equipMenu = "Equipment Menu";
    private const string fp_an_pauseMenu = "Pause Menu";
    private const string fp_an_inventoryMenu = "Inventory Menu";
    private const string fp_an_journalMenu = "Journal Menu";
    private const string fp_an_playerMenu = "Player Menu";
    private const string fp_an_mapMenu = "Map Menu";

    // FOCUS ACTION NAME REFS
    private const string f_an_point = "Point";
    private const string f_an_click = "Click";
    private const string f_an_interact = "Interact";
    private const string f_an_cancel = "Cancel";

    private const string f_an_num1 = "Num_1";
    private const string f_an_num2 = "Num_2";
    private const string f_an_num3 = "Num_3";
    private const string f_an_num4 = "Num_4";
    private const string f_an_num5 = "Num_5";
    private const string f_an_num6 = "Num_6";
    private const string f_an_num7 = "Num_7";
    private const string f_an_num8 = "Num_8";
    private const string f_an_num9 = "Num_9";
    private const string f_an_num0 = "Num_0";

    // CUT SCENE ACTION NAME REFS
    private const string cs_an_skip = "Skip";
    private const string cs_an_next = "Next";

    // UI ACTION NAME REFS
    private const string ui_an_navigate = "Navigate";
    private const string ui_an_submit = "Submit";
    private const string ui_an_cancel = "Cancel";
    private const string ui_an_point = "Point";
    private const string ui_an_click = "Click";
    private const string ui_an_rightClick = "RightClick";
    private const string ui_an_middleClick = "MiddleClick";
    private const string ui_an_scroll = "ScrollWheel";
    private const string ui_an_interact = "Interact";
    private const string ui_an_inventory = "Inventory Menu";
    private const string ui_an_journal = "Journal Menu";
    private const string ui_an_player = "Player Menu";
    private const string ui_an_map = "Map Menu";
    #endregion

    #region Input Actions
    // FIRST PERSON INPUT ACTIONS
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction slowWalkAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction slot1Action;
    private InputAction slot2Action;
    private InputAction meleeAction;
    private InputAction utilityAction;
    private InputAction unarmedAction;
    private InputAction swapAction;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction reloadAction;

    private InputAction equipMenuAction;
    private InputAction pauseMenuAction;
    private InputAction inventoryMenuAction;
    private InputAction journalMenuAction;
    private InputAction playerMenuAction;
    private InputAction mapMenuAction;

    // FOCUS INPUT ACTIONS
    private InputAction f_pointAction;
    private InputAction f_clickAction;
    private InputAction f_interactAction;
    private InputAction f_cancelAction;

    private InputAction f_num1Action;
    private InputAction f_num2Action;
    private InputAction f_num3Action;
    private InputAction f_num4Action;
    private InputAction f_num5Action;
    private InputAction f_num6Action;
    private InputAction f_num7Action;
    private InputAction f_num8Action;
    private InputAction f_num9Action;
    private InputAction f_num0Action;

    // CUT SCENE INPUT ACTIONS
    private InputAction cs_skipAction;
    private InputAction cs_nextAction;

    // UI INPUT ACTIONS
    private InputAction ui_navigateAction;
    private InputAction ui_submitAction;
    private InputAction ui_cancelAction;
    private InputAction ui_pointAction;
    private InputAction ui_clickAction;
    private InputAction ui_rightClickAction;
    private InputAction ui_middleClickAction;
    private InputAction ui_scrollAction;
    private InputAction ui_interactAction;
    private InputAction ui_inventoryAction;
    private InputAction ui_journalAction;
    private InputAction ui_playerAction;
    private InputAction ui_mapAction;
    #endregion

    #region Public Values
    // FIRST PERSON PUBLIC VALUES
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SlowWalkInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool Slot1Input { get; private set; }
    public bool Slot2Input { get; private set; }
    public bool MeleeInput { get; private set; }
    public bool UtilityInput { get; private set; }
    public bool UnarmedInput { get; private set; }
    public bool SwapInput { get; private set; }
    public bool AimInput { get; private set; }
    public bool FireInput { get; private set; }
    public bool ReloadInput { get; private set; }

    public bool EquipMenuInput { get; private set; }
    public bool PauseMenuInput { get; private set; }
    public bool InventoryMenuInput { get; private set; }
    public bool JournalMenuInput { get; private set; }
    public bool PlayerMenuInput { get; private set; }
    public bool MapMenuInput { get; private set; }

    //FOCUS PUBLIC VALUES
    public Vector2 F_PointInput { get; private set; }
    public bool F_ClickInput { get; private set; }
    public bool F_InteractInput { get; private set; }
    public bool F_CancelInput { get; private set; }

    public bool F_Num1Input { get; private set; }
    public bool F_Num2Input { get; private set; }
    public bool F_Num3Input { get; private set; }
    public bool F_Num4Input { get; private set; }
    public bool F_Num5Input { get; private set; }
    public bool F_Num6Input { get; private set; }
    public bool F_Num7Input { get; private set; }
    public bool F_Num8Input { get; private set; }
    public bool F_Num9Input { get; private set; }
    public bool F_Num0Input { get; private set; }

    // CUT SCENE PUBLIC VALUES
    public bool CS_SkipInput { get; private set; }
    public bool CS_NextInput { get; private set; }

    // UI PUBLIC VALUES
    public Vector2 UI_NavigateInput { get; private set; }
    public bool UI_SubmitInput { get; private set; }
    public bool UI_CancelInput { get; private set; }
    public Vector2 UI_PointInput { get; private set; }
    public bool UI_ClickInput { get; private set; }
    public bool UI_RightClickInput { get; private set; }
    public bool UI_MiddleClickInput { get; private set; }
    public Vector2 UI_ScrollInput { get; private set; }
    public bool UI_InteractInput { get; private set; }
    public bool UI_InventoryInput { get; private set; }
    public bool UI_JournalInput { get; private set; }
    public bool UI_PlayerInput { get; private set; }
    public bool UI_MapInput { get; private set; }
    #endregion

    #region Unity Events
    // First Person Input Events
    [HideInInspector] public UnityEvent<Vector2> OnMoveInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent<Vector2> OnLookInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent OnSlowWalkInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffSlowWalkInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnCrouchInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffCrouchInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnJumpInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnInteractInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnSlot1Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnSlot2Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnMeleeInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUtilityInput = new UnityEvent(); //TODO ---> Check this
    [HideInInspector] public UnityEvent OnUnarmedInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnSwapInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnAimInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffAimInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnFireInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffFireInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnReloadInput = new UnityEvent();

    [HideInInspector] public UnityEvent OnEquipmentMenuInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnPauseMenuInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnInventoryMenuInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnJournalMenuInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnPlayerMenuInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnMapMenuInput = new UnityEvent();

    // Focus Unity Events
    [HideInInspector] public UnityEvent<Vector2> OnFocus_PointInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent OnFocus_ClickInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffFocus_ClickInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_InteractInput = new UnityEvent();
    [HideInInspector] public UnityEvent OffFocus_InteractInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_CancelInput = new UnityEvent();

    [HideInInspector] public UnityEvent OnFocus_Num1Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num2Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num3Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num4Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num5Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num6Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num7Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num8Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num9Input = new UnityEvent();
    [HideInInspector] public UnityEvent OnFocus_Num0Input = new UnityEvent();

    // CUT SCENE UNITY EVENTS

    [HideInInspector] public UnityEvent OnCutScene_NextInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnCutScene_SkipInput = new UnityEvent();

    // UI UNITY EVENTS
    [HideInInspector] public UnityEvent<Vector2> OnUI_NavigateInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent OnUI_SubmitInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_CancelInput = new UnityEvent();
    [HideInInspector] public UnityEvent<Vector2> OnUI_PointInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent OnUI_ClickInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_RightClickInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_MiddleClickInput = new UnityEvent();
    [HideInInspector] public UnityEvent<Vector2> OnUI_ScrollInput = new UnityEvent<Vector2>();
    [HideInInspector] public UnityEvent OnUI_InteractInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_InventoryInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_JournalInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_PlayerInput = new UnityEvent();
    [HideInInspector] public UnityEvent OnUI_MapInput = new UnityEvent();
    #endregion

    public InputState currentState { get; private set; }

    #region Initialization
    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) != this)
        {
            // This is a duplicate instance, it will be destroyed
            return;
        }

        if (inputActionAsset == null)
        {
            Debug.LogError($"{nameof(InputHandler)}: Required Input Action Asset is missing!");
            return;
        }

        InputActionMap actionMap_FirstPerson = inputActionAsset.FindActionMap("FirstPerson");
        InputActionMap actionMap_Focus = inputActionAsset.FindActionMap("Focus");
        InputActionMap actionMap_CutScene = inputActionAsset.FindActionMap("CutScene");
        InputActionMap actionMap_UI = inputActionAsset.FindActionMap("UI");

        // ASSIGN FIRST PERSON ACTIONS
        moveAction = actionMap_FirstPerson.FindAction(fp_an_move);
        lookAction = actionMap_FirstPerson.FindAction(fp_an_look);
        slowWalkAction = actionMap_FirstPerson.FindAction(fp_an_slowWalk);
        crouchAction = actionMap_FirstPerson.FindAction(fp_an_crouch);
        jumpAction = actionMap_FirstPerson.FindAction(fp_an_jump);
        interactAction = actionMap_FirstPerson.FindAction(fp_an_interact);
        slot1Action = actionMap_FirstPerson.FindAction(fp_an_slot1);
        slot2Action = actionMap_FirstPerson.FindAction(fp_an_slot2);
        meleeAction = actionMap_FirstPerson.FindAction(fp_an_melee);
        utilityAction = actionMap_FirstPerson.FindAction(fp_an_utility);
        unarmedAction = actionMap_FirstPerson.FindAction(fp_an_unarmed);
        swapAction = actionMap_FirstPerson.FindAction(fp_an_swap);
        aimAction = actionMap_FirstPerson.FindAction(fp_an_aim);
        fireAction = actionMap_FirstPerson.FindAction(fp_an_fire);
        reloadAction = actionMap_FirstPerson.FindAction(fp_an_reload);

        equipMenuAction = actionMap_FirstPerson.FindAction(fp_an_equipMenu);
        pauseMenuAction = actionMap_FirstPerson.FindAction(fp_an_pauseMenu);
        inventoryMenuAction = actionMap_FirstPerson.FindAction(fp_an_inventoryMenu);
        journalMenuAction = actionMap_FirstPerson.FindAction(fp_an_journalMenu);
        playerMenuAction = actionMap_FirstPerson.FindAction(fp_an_playerMenu);
        mapMenuAction = actionMap_FirstPerson.FindAction(fp_an_mapMenu);

        // ASSIGN FOCUS ACTIONS
        f_pointAction = actionMap_Focus.FindAction(f_an_point);
        f_clickAction = actionMap_Focus.FindAction(f_an_click);
        f_interactAction = actionMap_Focus.FindAction(f_an_interact);
        f_cancelAction = actionMap_Focus.FindAction(f_an_cancel);

        f_num0Action = actionMap_Focus.FindAction(f_an_num0);
        f_num1Action = actionMap_Focus.FindAction(f_an_num1);
        f_num2Action = actionMap_Focus.FindAction(f_an_num2);
        f_num3Action = actionMap_Focus.FindAction(f_an_num3);
        f_num4Action = actionMap_Focus.FindAction(f_an_num4);
        f_num5Action = actionMap_Focus.FindAction(f_an_num5);
        f_num6Action = actionMap_Focus.FindAction(f_an_num6);
        f_num7Action = actionMap_Focus.FindAction(f_an_num7);
        f_num8Action = actionMap_Focus.FindAction(f_an_num8);
        f_num9Action = actionMap_Focus.FindAction(f_an_num9);

        // ASSIGN CUTSCENE ACTIONS
        cs_skipAction = actionMap_CutScene.FindAction(cs_an_skip);
        cs_nextAction = actionMap_CutScene.FindAction(cs_an_next);

        // ASSIGN UI ACTIONS
        ui_navigateAction = actionMap_UI.FindAction(ui_an_navigate);
        ui_submitAction = actionMap_UI.FindAction(ui_an_submit);
        ui_cancelAction = actionMap_UI.FindAction(ui_an_cancel);
        ui_pointAction = actionMap_UI.FindAction(ui_an_point);
        ui_clickAction = actionMap_UI.FindAction(ui_an_click);
        ui_rightClickAction = actionMap_UI.FindAction(ui_an_rightClick);
        ui_middleClickAction = actionMap_UI.FindAction(ui_an_middleClick);
        ui_scrollAction = actionMap_UI.FindAction(ui_an_scroll);
        ui_interactAction = actionMap_UI.FindAction(ui_an_interact);
        ui_inventoryAction = actionMap_UI.FindAction(ui_an_inventory);
        ui_journalAction = actionMap_UI.FindAction(ui_an_journal);
        ui_playerAction = actionMap_UI.FindAction(ui_an_player);
        ui_mapAction = actionMap_UI.FindAction(ui_an_map);

        //UpdateSensitivitySettings();
        RegisterInputActions();
        SetInputState(defaultState);
    }

    private void RegisterInputActions()
    {
        // FIRST PERSON INPUT ACTIONS
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context =>
        {
            Vector2 input = context.ReadValue<Vector2>();
            // Apply sensitivity multipliers
            input.x *= horizontalSensitivityMultiplier;
            input.y *= verticalSensitivityMultiplier * (invertYAxis ? -1 : 1);
            LookInput = input;
        };
        lookAction.canceled += context => LookInput = Vector2.zero;

        slowWalkAction.started += context => OnSlowWalkInput.Invoke();
        slowWalkAction.performed += context => SlowWalkInput = true;
        slowWalkAction.canceled += context =>
        {
            OffSlowWalkInput.Invoke();
            SlowWalkInput = false;
        };

        crouchAction.started += context => OnCrouchInput.Invoke();
        crouchAction.performed += context => CrouchInput = true;
        crouchAction.canceled += context =>
        {
            OffCrouchInput.Invoke();
            CrouchInput = false;
        };

        jumpAction.started += context => OnJumpInput.Invoke();
        jumpAction.performed += context => JumpInput = true;
        jumpAction.canceled += context => JumpInput = false;

        interactAction.started += context => OnInteractInput.Invoke();
        interactAction.performed += context => InteractInput = true;
        interactAction.canceled += context => InteractInput = false;

        slot1Action.started += context => OnSlot1Input.Invoke();
        slot1Action.performed += context => Slot1Input = true;
        slot1Action.canceled += context => Slot1Input = false;

        slot2Action.started += context => OnSlot2Input.Invoke();
        slot2Action.performed += context => Slot2Input = true;
        slot2Action.canceled += context => Slot2Input = false;

        meleeAction.started += context => OnMeleeInput.Invoke();
        meleeAction.performed += context => MeleeInput = true;
        meleeAction.canceled += context => MeleeInput = false;

        utilityAction.started += context => OnUtilityInput.Invoke();
        utilityAction.performed += context => UtilityInput = true;
        utilityAction.canceled += context => UtilityInput = false;

        unarmedAction.started += context => OnUnarmedInput.Invoke();
        unarmedAction.performed += context => UnarmedInput = true;
        unarmedAction.canceled += context => UnarmedInput = false;

        swapAction.started += context => OnSwapInput.Invoke();
        swapAction.performed += context => SwapInput = true;
        swapAction.canceled += context => SwapInput = false;

        aimAction.started += context => OnAimInput.Invoke();
        aimAction.performed += context => AimInput = true;
        aimAction.canceled += context =>
        {
            OffAimInput.Invoke();
            AimInput = false;
        };

        fireAction.started += context => OnFireInput.Invoke();
        fireAction.performed += context => FireInput = true;
        fireAction.canceled += context =>
        {
            OffFireInput.Invoke();
            FireInput = false;
        };

        reloadAction.started += context => OnReloadInput.Invoke();
        reloadAction.performed += context => ReloadInput = true;
        reloadAction.canceled += context => ReloadInput = false;

        equipMenuAction.started += context => OnEquipmentMenuInput.Invoke();
        equipMenuAction.performed += context => EquipMenuInput = true;
        equipMenuAction.canceled += context => EquipMenuInput = false;

        pauseMenuAction.started += context => OnPauseMenuInput.Invoke();
        pauseMenuAction.performed += context => PauseMenuInput = true;
        pauseMenuAction.canceled += context => PauseMenuInput = false;

        inventoryMenuAction.started += context => OnInventoryMenuInput.Invoke();
        inventoryMenuAction.performed += context => InventoryMenuInput = true;
        inventoryMenuAction.canceled += context => InventoryMenuInput = false;

        journalMenuAction.started += context => OnJournalMenuInput.Invoke();
        journalMenuAction.performed += context => JournalMenuInput = true;
        journalMenuAction.canceled += context => JournalMenuInput = false;

        playerMenuAction.started += context => OnPlayerMenuInput.Invoke();
        playerMenuAction.performed += context => PlayerMenuInput = true;
        playerMenuAction.canceled += context => PlayerMenuInput = false;

        mapMenuAction.started += context => OnMapMenuInput.Invoke();
        mapMenuAction.performed += context => MapMenuInput = true;
        mapMenuAction.canceled += context => MapMenuInput = false;

        // FOCUS INPUT ACTIONS
        f_pointAction.performed += context =>
        {
            F_PointInput = context.ReadValue<Vector2>();
            OnFocus_PointInput.Invoke(F_PointInput);
        };
        f_pointAction.canceled += context => F_PointInput = Vector2.zero;

        f_clickAction.started += context => OnFocus_ClickInput.Invoke();
        f_clickAction.performed += context => F_ClickInput = true;
        f_clickAction.canceled += context => F_ClickInput = false;

        f_interactAction.started += context => OnFocus_InteractInput.Invoke();
        f_interactAction.performed += context => F_InteractInput = true;
        f_interactAction.canceled += context =>
        {
            F_InteractInput = false;
            OffFocus_InteractInput.Invoke();
        };

        f_cancelAction.started += context => OnFocus_CancelInput.Invoke();
        f_cancelAction.performed += context => F_CancelInput = true;
        f_cancelAction.canceled += context => F_CancelInput = false;

        f_num0Action.started += context => OnFocus_Num0Input.Invoke();
        f_num0Action.performed += context => F_Num0Input = true;
        f_num0Action.canceled += context => F_Num0Input = false;

        f_num1Action.started += context => OnFocus_Num1Input.Invoke();
        f_num1Action.performed += context => F_Num1Input = true;
        f_num1Action.canceled += context => F_Num1Input = false;

        f_num2Action.started += context => OnFocus_Num2Input.Invoke();
        f_num2Action.performed += context => F_Num2Input = true;
        f_num2Action.canceled += context => F_Num2Input = false;

        f_num3Action.started += context => OnFocus_Num3Input.Invoke();
        f_num3Action.performed += context => F_Num3Input = true;
        f_num3Action.canceled += context => F_Num3Input = false;

        f_num4Action.started += context => OnFocus_Num4Input.Invoke();
        f_num4Action.performed += context => F_Num4Input = true;
        f_num4Action.canceled += context => F_Num4Input = false;

        f_num5Action.started += context => OnFocus_Num5Input.Invoke();
        f_num5Action.performed += context => F_Num5Input = true;
        f_num5Action.canceled += context => F_Num5Input = false;

        f_num6Action.started += context => OnFocus_Num6Input.Invoke();
        f_num6Action.performed += context => F_Num6Input = true;
        f_num6Action.canceled += context => F_Num6Input = false;

        f_num7Action.started += context => OnFocus_Num7Input.Invoke();
        f_num7Action.performed += context => F_Num7Input = true;
        f_num7Action.canceled += context => F_Num7Input = false;

        f_num8Action.started += context => OnFocus_Num8Input.Invoke();
        f_num8Action.performed += context => F_Num8Input = true;
        f_num8Action.canceled += context => F_Num8Input = false;

        f_num9Action.started += context => OnFocus_Num9Input.Invoke();
        f_num9Action.performed += context => F_Num9Input = true;
        f_num9Action.canceled += context => F_Num9Input = false;

        // CUT SCENE INPUT ACTIONS
        cs_nextAction.started += context => OnCutScene_NextInput.Invoke();
        cs_nextAction.performed += context => CS_NextInput = true;
        cs_nextAction.canceled += context => CS_NextInput = false;

        cs_skipAction.started += context => OnCutScene_SkipInput.Invoke();
        cs_skipAction.performed += context => CS_SkipInput = true;
        cs_skipAction.canceled += context => CS_SkipInput = false;

        // UI INPUT ACTIONS
        ui_navigateAction.performed += context =>
        {
            UI_NavigateInput = context.ReadValue<Vector2>();
            OnUI_NavigateInput.Invoke(UI_NavigateInput);
        };
        ui_navigateAction.canceled += context => UI_NavigateInput = Vector2.zero;

        ui_pointAction.performed += context =>
        {
            UI_PointInput = context.ReadValue<Vector2>();
            OnUI_PointInput.Invoke(UI_PointInput);
        };
        ui_pointAction.canceled += context => UI_PointInput = Vector2.zero;

        ui_submitAction.started += context => OnUI_SubmitInput.Invoke();
        ui_submitAction.performed += context => UI_SubmitInput = true;
        ui_submitAction.canceled += context => UI_SubmitInput = false;

        ui_cancelAction.started += context => OnUI_CancelInput.Invoke();
        ui_cancelAction.performed += context => UI_CancelInput = true;
        ui_cancelAction.canceled += context => UI_CancelInput = false;

        ui_clickAction.started += context => OnUI_ClickInput.Invoke();
        ui_clickAction.performed += context => UI_ClickInput = true;
        ui_clickAction.canceled += context => UI_ClickInput = false;

        ui_rightClickAction.started += context => OnUI_RightClickInput.Invoke();
        ui_rightClickAction.performed += context => UI_RightClickInput = true;
        ui_rightClickAction.canceled += context => UI_RightClickInput = false;

        ui_middleClickAction.started += context => OnUI_MiddleClickInput.Invoke();
        ui_middleClickAction.performed += context => UI_MiddleClickInput = true;
        ui_middleClickAction.canceled += context => UI_MiddleClickInput = false;

        ui_scrollAction.performed += context =>
        {
            UI_ScrollInput = context.ReadValue<Vector2>();
            OnUI_ScrollInput.Invoke(UI_ScrollInput);
        };
        ui_scrollAction.canceled += context => UI_ScrollInput = Vector2.zero;

        ui_interactAction.started += context => OnUI_InteractInput.Invoke();
        ui_interactAction.performed += context => UI_InteractInput = true;
        ui_interactAction.canceled += context => UI_InteractInput = false;

        ui_inventoryAction.started += context => OnUI_InventoryInput.Invoke();
        ui_inventoryAction.performed += context => UI_InventoryInput = true;
        ui_inventoryAction.canceled += context => UI_InventoryInput = false;

        ui_journalAction.started += context => OnUI_JournalInput.Invoke();
        ui_journalAction.performed += context => UI_JournalInput = true;
        ui_journalAction.canceled += context => UI_JournalInput = false;

        ui_playerAction.started += context => OnUI_PlayerInput.Invoke();
        ui_playerAction.performed += context => UI_PlayerInput = true;
        ui_playerAction.canceled += context => UI_PlayerInput = false;

        ui_mapAction.started += context => OnUI_MapInput.Invoke();
        ui_mapAction.performed += context => UI_MapInput = true;
        ui_mapAction.canceled += context => UI_MapInput = false;

    }
    #endregion

    /// <summary>
    /// Set the input state on enable.
    /// </summary>
    void OnEnable()
    {
        //SetInputState(defaultState); //maybe need to use default state here????
    }

    /// <summary>
    /// Disables all input actions on disable.
    /// </summary>
    void OnDisable()
    {
        inputActionAsset.FindActionMap(actionMapName_FirstPerson).Disable();
        inputActionAsset.FindActionMap(actionMapName_Focus).Disable();
        inputActionAsset.FindActionMap(actionMapName_Cutscene).Disable();
        inputActionAsset.FindActionMap(actionMapName_UI).Disable();
    }

    #region Input State
    /// <summary>
    /// Pubic method to set the InputState "currentState" and disable/enable the appropriate input actions.
    /// </summary>
    public void SetInputState(InputState state)
    {
        switch (state)
        {
            case InputState.FirstPerson:
                CursorLock(true);
                currentState = InputState.FirstPerson;
                inputActionAsset.FindActionMap(actionMapName_FirstPerson).Enable();
                inputActionAsset.FindActionMap(actionMapName_Focus).Disable();
                inputActionAsset.FindActionMap(actionMapName_Cutscene).Disable();
                inputActionAsset.FindActionMap(actionMapName_UI).Disable();
                break;

            case InputState.Focus:
                CursorLock(false);
                currentState = InputState.Focus;
                inputActionAsset.FindActionMap(actionMapName_FirstPerson).Disable();
                inputActionAsset.FindActionMap(actionMapName_Focus).Enable();
                inputActionAsset.FindActionMap(actionMapName_Cutscene).Disable();
                inputActionAsset.FindActionMap(actionMapName_UI).Disable();
                break;

            case InputState.Cutscene:
                CursorLock(false);
                currentState = InputState.Cutscene;
                inputActionAsset.FindActionMap(actionMapName_FirstPerson).Disable();
                inputActionAsset.FindActionMap(actionMapName_Focus).Disable();
                inputActionAsset.FindActionMap(actionMapName_Cutscene).Enable();
                inputActionAsset.FindActionMap(actionMapName_UI).Disable();
                break;

            case InputState.UI:
                CursorLock(false);
                currentState = InputState.UI;
                inputActionAsset.FindActionMap(actionMapName_FirstPerson).Disable();
                inputActionAsset.FindActionMap(actionMapName_Focus).Disable();
                inputActionAsset.FindActionMap(actionMapName_Cutscene).Disable();
                inputActionAsset.FindActionMap(actionMapName_UI).Enable();
                break;
            default:
                Debug.LogError($"Invalid InputState: {state}");
                return;
        }
    }
    #endregion

    #region Sensitivity
    /// <summary>
    /// Updates sensitivity settings from the GameMaster
    /// </summary>
    public void UpdateSensitivitySettings()
    {
        if (GameMaster.Instance != null)
        {
            PlayerSettings settings = GameSettings.GetSettings();
            horizontalSensitivityMultiplier = settings.GetHorizontalSensitivityMultiplier();
            verticalSensitivityMultiplier = settings.GetVerticalSensitivityMultiplier();
            invertYAxis = settings.invertYAxis;

            // SBGDebug.LogInfo($"Mouse sensitivity updated - H: {horizontalSensitivityMultiplier}, V: {verticalSensitivityMultiplier}, InvertY: {invertYAxis}", "FPS_InputHandler");
        }
    }
    #endregion

    #region Cursor Lock
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
    #endregion
}
#endregion