using UnityEngine;
using UnityEngine.Events;
using System.Collections;


// intended replacement for UI_Master
// uses state machine to manage UI states
// brings in Pause Menu as a child object to simplify management
// allow for more seemless transitions between states

// POSSIBLE ABSTRACTION / EXTRACTION:
// move Post Process volume management here? would consolidate a lot of logic
// possible to bring Time Scale here to bring more consistency to time-related effects


#region UI_Master
public class NewUI_Master : MonoBehaviour
{
    private static NewUI_Master _instance;
    public static NewUI_Master Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(UI_Master)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("HUD Elements")]
    [Space(10)]

    [SerializeField] private GameObject[] HUD_Status_Elements;
    [SerializeField] private GameObject[] HUD_Weapon_Elements;
    [SerializeField] private GameObject[] HUD_Utility_Elements;
    [SerializeField] private GameObject crosshair_Element;

    private DialogueBox dialogueBox;
    private InventoryMenu inventoryMenu;
    private PauseMenu pauseMenu;

    public UIMasterState defaultState { get; private set; } = UIMasterState.FirstPerson;
    public UIMasterState currentState { get; private set; }
    public UIMasterState previousState { get; private set; }
    private UIMasterState queuedState;

    [HideInInspector] public UnityEvent<UIMasterState> onUIStateChanged; // newState
    [HideInInspector] public UnityEvent<UIMasterState, UIMasterState> onUIStateTransition; // oldState, newState

    public enum UIMasterState
    {
        ExitingState,
        EnteringState,
        None, //used for queuing
        FirstPerson,
        FreeCam,
        MainMenu,
        Inventory,
        PlayerStats,
        Journal,
        Dialogue,
        Pause
    }

    #region Initialization
    void Awake()
    {
        // persist accross scenes
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            InitializeComponents();
            SubscribeToEvents();
            SetState(defaultState);
        }
    }

    private void InitializeComponents()
    {
        pauseMenu = GetComponentInChildren<PauseMenu>();
        inventoryMenu = GetComponentInChildren<InventoryMenu>(true);
        dialogueBox = GetComponentInChildren<DialogueBox>(true);

        if (pauseMenu == null) Debug.LogError("PauseMenu not found as child component");
        if (inventoryMenu == null) Debug.LogError("InventoryMenu not found as child component");
        if (dialogueBox == null) Debug.LogError("DialogueBox not found as child component");
    }

    private void SubscribeToEvents()
    {
        FPS_InputHandler.Instance?.inventoryMenuButtonTriggered?.AddListener(() => SetState(UIMasterState.Inventory));
        FPS_InputHandler.Instance?.pauseMenuButtonTriggered?.AddListener(() => SetState(UIMasterState.Pause));

        if (FPS_InputHandler.Instance == null) { SBGDebug.LogError("FPS_InputHandler instance is null, unable to subscribe to events", "UI_Master"); }
    }
    #endregion

    #region State Management

    public void SetState(UIMasterState newState)
    {
        if (newState == currentState) return;

        if (currentState == UIMasterState.ExitingState)
        {
            // simply replace the queued state with the new state and continue exiting
            queuedState = newState;
            return;
        }

        if (currentState == UIMasterState.EnteringState)
        {
            // do nothing
            return;
        }
        
        StartCoroutine(SetStateRoutine(newState));
    }

    private IEnumerator SetStateRoutine(UIMasterState newState)
    {
        queuedState = newState;
        previousState = currentState;

        ExitState();
        bool stateExited = false;
        onUIStateChanged.AddListener(OnOldStateExited); // once fully exited
        void OnOldStateExited(UIMasterState exitedState)
        {
            onUIStateChanged.RemoveListener(OnOldStateExited);
            stateExited = true;
            EnterState(queuedState);
            queuedState = UIMasterState.None;
        }

        yield return new WaitUntil(() => stateExited);

        onUIStateChanged.AddListener(OnNewStateEntered); // once full entered
        void OnNewStateEntered(UIMasterState enteredState)
        {
            currentState = enteredState;
            onUIStateChanged.RemoveListener(OnNewStateEntered);
        }

        yield return null;
    }

    private void ExitState()
    {
        currentState = UIMasterState.ExitingState;
        
        switch (currentState)
        {
            case UIMasterState.FirstPerson:
                //ExitFirstPersonState();
                break;
            case UIMasterState.Inventory:
                //ExitInventoryState();
                break;
            case UIMasterState.Pause:
                //ExitPauseState();
                break;
            case UIMasterState.Dialogue:
                //ExitDialogueState();
                break;
        }
    }

    private void EnterState(UIMasterState state)
    {
        currentState = UIMasterState.EnteringState;

        switch (state)
        {
            case UIMasterState.FirstPerson:
                //EnterFirstPersonState();
                break;
            case UIMasterState.Inventory:
                //EnterInventoryState();
                break;
            case UIMasterState.Pause:
                //EnterPauseState();
                break;
            case UIMasterState.Dialogue:
                //EnterDialogueState();
                break;
                // Add other states as they're implemented
        }
    }
    #endregion

    // ported methods

    #region Inventory Menu
    private void ShowInventory(InventoryType inventoryType = InventoryType.Player, IInventory inventory = null)
    {
        if (inventoryMenu == null) return;

        GameMaster.Instance.gm_InventoryOpened.Invoke(inventoryType);
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.RemoveListener(() => ShowInventory());

        HideAllHUD();
        VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);
        Time.timeScale = 0f;
        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);

        switch (inventoryType)
        {
            case InventoryType.Player:
                IInventory playerInventory = null;

                if (InventoryManager.Instance != null)
                {
                    playerInventory = InventoryManager.Instance.GetPlayerInventory();
                }

                if (playerInventory == null)
                {
                    playerInventory = new Inv_Player("player", "Player Inventory", 100);
                    Debug.LogWarning("No inventory found, generating empty inventory");
                }

                inventoryMenu?.Initialize(playerInventory); //also calls RefreshInventory
                if (inventoryMenu == null) { Debug.LogWarning("Inventory menu is null."); }
                break;
            case InventoryType.Container:
                inventoryMenu?.Initialize(inventory); //also calls RefreshInventory
                if (inventoryMenu == null) { Debug.LogWarning("Inventory menu is null."); }
                break;
            case InventoryType.NPC:
                inventoryMenu?.Initialize(inventory); //also calls RefreshInventory
                if (inventoryMenu == null) { Debug.LogWarning("Inventory menu is null."); }
                break;
            default:
                Debug.LogWarning("Unhandled inventory type.");
                break;
        }

        FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(() => HideInventory());

        inventoryMenu.gameObject.SetActive(true);
    }

    private void HideInventory(InventoryType inventoryType = InventoryType.Player)
    {
        if (inventoryMenu == null) return;

        GameMaster.Instance.gm_InventoryClosed.Invoke(inventoryType);

        inventoryMenu.gameObject.SetActive(false);

        ShowAllHUD();

        VolumeManager.Instance.SetVolume(VolumeType.Default);

        Time.timeScale = 1f;

        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        // needs updating to work with state switch
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.AddListener(() => ShowInventory(InventoryType.Player));
        // needs updating to work with state switch
        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(() => HideInventory());
    }
    #endregion

    #region Public Methods
    public void HideAllHUD()
    {
        if (FPSS_WeaponHUD.Instance != null)
        {
            FPSS_WeaponHUD.Instance.Hide(true);
        }

        if (FPSS_ReticleSystem.Instance != null)
        {
            FPSS_ReticleSystem.Instance.Hide(true);
        }

        foreach (GameObject element in HUD_Status_Elements)
        {
            element.SetActive(false);
        }
    }

    public void ShowAllHUD()
    {
        FPSS_WeaponHUD.Instance.Hide(false);
        FPSS_ReticleSystem.Instance.Hide(false);

        foreach (GameObject element in HUD_Status_Elements)
        {
            element.SetActive(true);
        }
    }
    #endregion
}
#endregion