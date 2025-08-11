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

/* 

UNITY OBJECT HIERARCHY
UI_Master
├── HUD_Controller
│   ├── FPSS_WeaponHUD
│   ├── NavCompass
│   └── FPSS_ReticleSystem
├── PauseMenu
│   └── SettingsMenu (**possibility to abstract settings menu into own class/object**)
├── InventoryMenu
├── DialogueBox
├── JournalMenu (**needs to be developed**)
├── PlayerStats (**needs to be developed**)
└── LockPickingQuickTimeEvent (**possibility to be moved into this system**)

INFO:
- DialogueBox is not handled by state machine as it can display over other menus

 */

#region UI_Master
public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(UIManager)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    // moved to HUD_Controller
    /* [Header("HUD Elements")]
    [Space(10)]

    [SerializeField] private GameObject[] HUD_Status_Elements;
    [SerializeField] private GameObject[] HUD_Weapon_Elements;
    [SerializeField] private GameObject[] HUD_Utility_Elements; */

    private HUD_Controller hudController;
    private DialogueBox dialogueBox;
    private InventoryMenu inventoryMenu;
    private PauseMenu pauseMenu;
    private JournalMenu journalMenu;

    public UIMasterState defaultState { get; private set; } = UIMasterState.FirstPerson;
    public UIMasterState currentState { get; private set; }
    public UIMasterState previousState { get; private set; }
    private UIMasterState queuedState;
    private bool alwaysHideHUD;

    /// <summary>
    /// Event triggered when the UI state finishes changing.
    /// </summary>
    [HideInInspector] public UnityEvent<UIMasterState> onUIStateChanged;

    private const string SETTINGS_KEY = "PlayerSettings";
    private const string PATH_LABEL_DATA = "StreamingAssets/MenuLabelData/";

    private const string FILE_LABEL_DATA_HUD = "labelData_HUD.json";
    private const string FILE_LABEL_DATA_INVENTORY = "labelData_InventoryMenu.json";
    private const string FILE_LABEL_DATA_PAUSE = "labelData_PauseMenu.json";
    private const string FILE_LABEL_DATA_JOURNAL = "labelData_JournalMenu.json";
    private const string FILE_LABEL_DATA_PLAYER_STATS = "labelData_PlayerStatsMenu.json";

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
        // MENUS
        pauseMenu = GetComponentInChildren<PauseMenu>();
        inventoryMenu = GetComponentInChildren<InventoryMenu>(true);
        journalMenu = GetComponentInChildren<JournalMenu>(true);
        // HUD
        hudController = GetComponentInChildren<HUD_Controller>(true);
        // DIALOGUE
        dialogueBox = GetComponentInChildren<DialogueBox>(true);

        if (pauseMenu == null) SBGDebug.LogError("PauseMenu not found as child component", "UI_Master");
        if (inventoryMenu == null) SBGDebug.LogError("InventoryMenu not found as child component", "UI_Master");
        if (journalMenu == null) SBGDebug.LogError("JournalMenu not found as child component", "UI_Master");
        if (hudController == null) SBGDebug.LogError("HUD_Controller not found as child component", "UI_Master");
        if (dialogueBox == null) SBGDebug.LogError("DialogueBox not found as child component", "UI_Master");
    }

    private void SubscribeToEvents()
    {
        FPS_InputHandler.Instance?.inventoryMenuButtonTriggered?.AddListener(() => SetState(UIMasterState.Inventory));
        FPS_InputHandler.Instance?.pauseMenuButtonTriggered?.AddListener(() => SetState(UIMasterState.Pause));
        //FPS_InputHandler.Instance?.journalButtonTriggered?.AddListener(() => SetState(UIMasterState.Journal));

        if (FPS_InputHandler.Instance == null) SBGDebug.LogError("FPS_InputHandler instance is null, unable to subscribe to events", "UI_Master");
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

        StartCoroutine(ChangeStateRoutine(newState));
    }

    private IEnumerator ChangeStateRoutine(UIMasterState newState)
    {
        queuedState = newState;
        previousState = currentState;

        // exit current state
        bool stateExited = false;
        onUIStateChanged.AddListener(OnOldStateExited);
        ExitState();
        void OnOldStateExited(UIMasterState exitedState)
        {
            onUIStateChanged.RemoveListener(OnOldStateExited);
            stateExited = true;
        }

        //wait until after "onUIStateChanged" has been invoked
        yield return new WaitUntil(() => stateExited);

        // enter new state
        bool stateEntered = false;
        onUIStateChanged.AddListener(OnNewStateEntered);
        EnterState(queuedState);
        void OnNewStateEntered(UIMasterState enteredState)
        {
            onUIStateChanged.RemoveListener(OnNewStateEntered);
            queuedState = UIMasterState.None;
            currentState = enteredState;
            stateEntered = true;

        }

        //wait until after "onUIStateChanged" has been invoked, again
        yield return new WaitUntil(() => stateEntered);
    }

    

    private void EnterState(UIMasterState state)
    {
        currentState = UIMasterState.EnteringState;

        switch (state)
        {
            case UIMasterState.FirstPerson:
                ShowFirstPerson();
                //FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
                break;
            case UIMasterState.Inventory:
                ShowInventory();
                //FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
                break;
            case UIMasterState.Pause:
                ShowPauseMenu();
                //FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
                break;
        }
    }

    private void ExitState()
    {
        currentState = UIMasterState.ExitingState;

        switch (previousState)
        {
            case UIMasterState.FirstPerson: //Coming from FirstPerson View
                HideFirstPerson();
                break;
            case UIMasterState.Inventory: // coming from Inventory Menu
                HideInventory();
                break;
            case UIMasterState.Pause: // coming from Pause Menu
                HidePauseMenu();
                break;
        }
    }
    #endregion

    // ported methods

    #region First Person
    private void ShowFirstPerson()
    {
        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        StartCoroutine(ShowFirstPersonRoutine());
    }

    private void HideFirstPerson()
    {
        StartCoroutine(HideFirstPersonRoutine());
    }

    private IEnumerator ShowFirstPersonRoutine()
    {
        hudController.HideAllHUD(false);
        yield return new WaitUntil(() => !hudController.hud_Hidden);

        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        StopGamePlay(false);

        onUIStateChanged?.Invoke(UIMasterState.FirstPerson);
    }

    private IEnumerator HideFirstPersonRoutine()
    {
        hudController.HideAllHUD(true);
        yield return new WaitUntil(() => hudController.hud_Hidden);

        onUIStateChanged?.Invoke(previousState);
    }
    #endregion

    #region Inventory Menu
    /// <summary>
    /// you must Initialize the inventory menu before showing it.
    /// </summary>
    private void ShowInventory()
    {
        if (inventoryMenu == null) return;
        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
        StartCoroutine(ShowInventoryRoutine());
    }

    private void HideInventory()
    {
        if (inventoryMenu == null) return;
        StartCoroutine(HideInventoryRoutine());
    }


    private IEnumerator ShowInventoryRoutine()
    {
        // handle subscriptions
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.RemoveListener(() => ShowInventory());
        FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(() => SetState(UIMasterState.FirstPerson));

        StopGamePlay(true);
        hudController.HideAllHUD(true);
        yield return new WaitUntil(() => hudController.hud_Hidden);

        inventoryMenu.gameObject.SetActive(true);

        inventoryMenu.OnMenuOpen();
        onUIStateChanged?.Invoke(UIMasterState.Inventory);
    }

    private IEnumerator HideInventoryRoutine()
    {
        if (inventoryMenu == null) yield break;

        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(() => SetState(UIMasterState.FirstPerson));

        inventoryMenu.gameObject.SetActive(false);

        StopGamePlay(false);
        hudController.HideAllHUD(false);
        yield return new WaitUntil(() => !hudController.hud_Hidden);

        inventoryMenu.OnMenuClose();
        onUIStateChanged?.Invoke(previousState);
    }
    #endregion

    #region Pause Menu
    private void ShowPauseMenu()
    {
        if (pauseMenu == null) return;

        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);

        StartCoroutine(ShowPauseMenuRoutine());
    }

    private void HidePauseMenu()
    {
        if (pauseMenu == null) return;

        StartCoroutine(HidePauseMenuRoutine());
    }

    private IEnumerator ShowPauseMenuRoutine()
    {
        FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(() => SetState(UIMasterState.FirstPerson));

        StopGamePlay(true);

        return null;
    }

    private IEnumerator HidePauseMenuRoutine()
    {
        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(() => SetState(UIMasterState.FirstPerson));

        return null;
    }

    #endregion

    #region Helper Methods
    private void StopGamePlay(bool stop = true)
    {
        if (stop)
        {
            VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);
            Time.timeScale = 0f;
        }
        else
        {
            VolumeManager.Instance.SetVolume(VolumeType.Default);
            Time.timeScale = 1f;
        }
    }
    #endregion

    #region Public Methods
    public void HideAllHUD(bool hide = true)
    {
        hudController.HideAllHUD(hide);
    }
    #endregion
}
#endregion