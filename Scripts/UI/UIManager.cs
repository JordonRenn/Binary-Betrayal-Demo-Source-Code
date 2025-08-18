using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Threading.Tasks;

/* 
UNITY OBJECT HIERARCHY
UI_Master
├── HUD_Controller
│   ├── FPSS_WeaponHUD
│   ├── NavCompass
│   └── FPSS_ReticleSystem
├── DialogueBox
├── PauseMenu
├── InventoryMenu
├── JournalMenu (**needs to be developed**)
├── PlayerStats (**needs to be developed**)

INFO:
- DialogueBox is not handled by state machine as it can display over other menus
- Can pause game while in first person
    - While paused, you can enter the Inventory, Journal or Player Stats Menu
    - Pressing cancel while in any of these menus will return to First Person
- You can enter the Inventory, Journal or Player Stats Menu from First Person as well
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

    private HUD_Controller hudController;
    private DialogueBox dialogueBox;
    private InventoryMenu inventoryMenu;
    private PauseMenu pauseMenu;
    private JournalMenu journalMenu;

    public UIMasterState defaultState { get; private set; } = UIMasterState.FirstPerson;
    public UIMasterState currentState { get; private set; }
    public UIMasterState previousState { get; private set; }
    private UIMasterState queuedState = UIMasterState.None;
    private bool alwaysHideHUD;

    /// <summary>
    /// Event triggered when the UI state has completely changed. Provides (fromState, toState).
    /// </summary>
    [HideInInspector] public UnityEvent<UIMasterState, UIMasterState> onUIStateChanged;

    // FOR FUTURE UPDATE FOR MULTI-LANGUAGE SUPPORT
    /* private const string SETTINGS_KEY = "PlayerSettings";
    private const string PATH_LABEL_DATA = "StreamingAssets/MenuLabelData/";
    private const string FILE_LABEL_DATA_HUD = "labelData_HUD.json";
    private const string FILE_LABEL_DATA_INVENTORY = "labelData_InventoryMenu.json";
    private const string FILE_LABEL_DATA_PAUSE = "labelData_PauseMenu.json";
    private const string FILE_LABEL_DATA_JOURNAL = "labelData_JournalMenu.json";
    private const string FILE_LABEL_DATA_PLAYER_STATS = "labelData_PlayerStatsMenu.json"; */

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
            SBGDebug.LogInfo("UIManager initialized as singleton", "UIManager | Awake");

            InitializeComponents();
            SubscribeToEvents();
            currentState = defaultState;
        }
        else
        {
            SBGDebug.LogError("UIManager instance already exists, destroying duplicate.", "UIManager | Awake");
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        // MENUS
        pauseMenu = GetComponentInChildren<PauseMenu>();
        inventoryMenu = GetComponentInChildren<InventoryMenu>(true);
        //journalMenu = GetComponentInChildren<JournalMenu>(true);
        // HUD
        hudController = GetComponentInChildren<HUD_Controller>(true);
        // DIALOGUE
        dialogueBox = GetComponentInChildren<DialogueBox>(true);

        if (pauseMenu == null) SBGDebug.LogError("PauseMenu not found as child component", "UI_Master | InitializeComponents");
        if (inventoryMenu == null) SBGDebug.LogError("InventoryMenu not found as child component", "UI_Master | InitializeComponents");
        if (hudController == null) SBGDebug.LogError("HUD_Controller not found as child component", "UI_Master | InitializeComponents");
        if (dialogueBox == null) SBGDebug.LogError("DialogueBox not found as child component", "UI_Master | InitializeComponents");
    }

    private void SubscribeToEvents()
    {
        if (InputHandler.Instance == null)
        {
            SBGDebug.LogError("InputHandler instance is null, unable to subscribe to events", "UI_Master | SubscribeToEvents");
            return;
        }
        else
        {
            SBGDebug.LogInfo("InputHandler instance found, subscribing to events", "UI_Master | SubscribeToEvents");
            InputHandler.Instance?.OnInventoryMenuInput?.AddListener(() => SetState(UIMasterState.Inventory));
            InputHandler.Instance?.OnPauseMenuInput?.AddListener(() => SetState(UIMasterState.Pause));
            InputHandler.Instance?.OnPlayerMenuInput?.AddListener(() => SetState(UIMasterState.PlayerStats));
        }
    }
    #endregion

    #region State Management

    // Flag to track if a state change is in progress
    private bool isChangingState = false;

    public async void SetState(UIMasterState newState)
    {
        SBGDebug.LogInfo($"Setting UI state to: {newState}", "UIManager | SetState");

        // Ignore repeated requests for the same state
        if (newState == currentState)
        {
            SBGDebug.LogWarning($"Requested State is same as Current State: {currentState}", "UIManager | SetState");
            return;
        }

        // Validate state transition
        if (newState == UIMasterState.ExitingState || newState == UIMasterState.EnteringState)
        {
            SBGDebug.LogError($"Cannot manually set state to '{newState}'. This is used internally.", "UIManager | SetState");
            return;
        }

        // If we're already changing state, queue the new state
        if (isChangingState)
        {
            SBGDebug.LogWarning($"State change already in progress. Queuing new state: {newState}", "UIManager | SetState");
            queuedState = newState;
            return;
        }

        // Lock state changes
        isChangingState = true;

        try
        {
            SBGDebug.LogInfo($"Changing UI state from {currentState} to {newState}", "UIManager | SetState");
            await PerformStateChangeAsync(newState);
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogError($"Error during state change: {ex.Message}", "UIManager | SetState");
        }
        finally
        {
            // Always unlock state changes when done
            isChangingState = false;
        }
    }

    private async Task PerformStateChangeAsync(UIMasterState newState)
    {
        SBGDebug.LogInfo($"Starting State Change | {currentState} to {newState}", "UIManager | PerformStateChangeAsync");

        var fromState = currentState;
        previousState = currentState;
        queuedState = newState;

        try {
            // Exit current state
            SBGDebug.LogInfo("About to exit current state", "UIManager | PerformStateChangeAsync");
            await ExitStateAsync();
            SBGDebug.LogInfo("Finished exiting current state", "UIManager | PerformStateChangeAsync");

            // Small delay between exiting and entering states
            await Task.Delay(50);

            // Enter new state
            SBGDebug.LogInfo("About to enter new state", "UIManager | PerformStateChangeAsync");
            await EnterStateAsync(queuedState);
            SBGDebug.LogInfo("Finished entering new state", "UIManager | PerformStateChangeAsync");

            // Update state and notify
            currentState = queuedState;
            queuedState = UIMasterState.None;

            // Notify external systems that state change is complete
            onUIStateChanged?.Invoke(fromState, currentState);
            
            SBGDebug.LogInfo($"State change complete: {fromState} -> {currentState}", "UIManager | PerformStateChangeAsync");
        }
        catch (System.Exception ex) {
            // If anything goes wrong, try to recover to a stable state
            SBGDebug.LogError($"Error during state change: {ex.Message}\nStack trace: {ex.StackTrace}", "UIManager | PerformStateChangeAsync");
            
            // Reset to FirstPerson as a fallback if we encounter an error
            currentState = UIMasterState.FirstPerson;
            await EnterStateAsync(UIMasterState.FirstPerson);
            queuedState = UIMasterState.None;
        }
    }

    private async Task EnterStateAsync(UIMasterState state)
    {
        SBGDebug.LogInfo($"Entering state: {state}", "UIManager | EnterStateAsync");

        switch (state)
        {
            case UIMasterState.FirstPerson:
                await ShowFirstPersonAsync();
                break;
            case UIMasterState.Inventory:
                await ShowInventoryAsync();
                break;
            case UIMasterState.Pause:
                await ShowPauseMenuAsync();
                break;
            default:
                SBGDebug.LogError($"EnterStateAsync: Unhandled state '{state}'", "UIManager");
                // Fallback to FirstPerson state
                queuedState = UIMasterState.FirstPerson;
                await ShowFirstPersonAsync();
                break;
        }
    }

    private async Task ExitStateAsync()
    {
        // Validate previousState before using it
        if (previousState == UIMasterState.None || 
            previousState == UIMasterState.ExitingState || 
            previousState == UIMasterState.EnteringState)
        {
            SBGDebug.LogWarning($"Invalid previousState '{previousState}' in ExitStateAsync. Skipping exit.", "UIManager | ExitStateAsync");
            return;
        }

        SBGDebug.LogInfo($"Exiting state: {previousState}", "UIManager | ExitStateAsync");

        switch (previousState)
        {
            case UIMasterState.FirstPerson:
                SBGDebug.LogInfo("Starting to hide FirstPerson state", "UIManager | ExitStateAsync");
                await HideFirstPersonAsync();
                SBGDebug.LogInfo("Finished hiding FirstPerson state", "UIManager | ExitStateAsync");
                break;
            case UIMasterState.Inventory:
                await HideInventoryAsync();
                break;
            case UIMasterState.Pause:
                await HidePauseMenuAsync();
                break;
            default:
                SBGDebug.LogError($"ExitStateAsync: Unhandled previous state '{previousState}'", "UIManager | ExitStateAsync");
                break;
        }
    }
    #endregion

    // ported methods

    #region First Person
    private async Task ShowFirstPersonAsync()
    {
        hudController.HideAllHUD(false);

        // Wait until HUD is shown
        while (hudController.hud_Hidden)
        {
            await Task.Yield(); // Yield control back to Unity's main thread
        }

        InputHandler.Instance.SetInputState(InputState.FirstPerson);
        StopGamePlay(false);
    }

    private async Task HideFirstPersonAsync()
    {
        SBGDebug.LogInfo("HideFirstPersonAsync started", "UIManager | HideFirstPersonAsync");
        hudController.HideAllHUD(true);
        
        SBGDebug.LogInfo($"Initial hud_Hidden state: {hudController.hud_Hidden}", "UIManager | HideFirstPersonAsync");
        
        // Wait until HUD is hidden
        while (!hudController.hud_Hidden)
        {
            await Task.Yield(); // Yield control back to Unity's main thread
        }
        
        SBGDebug.LogInfo("HUD is now hidden, HideFirstPersonAsync complete", "UIManager | HideFirstPersonAsync");
    }
    #endregion

    #region Dialogue
    // does not use state machine as it can overlay during any state. 
    public void DisplayDialogue(string dialogueID)
    {
        if (dialogueBox == null) return;
        if (DialogueLoader.Instance == null) return;
        DialogueLoader.Instance?.LoadDialogue(dialogueID);

        StartCoroutine(DisplayDialogueRoutine());
    }

    private IEnumerator DisplayDialogueRoutine()
    {
        var previousInputState = InputHandler.Instance.currentState;
        bool dialogueFinished = false;
        dialogueBox.dialogueBoxClosed.AddListener(() => dialogueFinished = true);
        InputHandler.Instance.SetInputState(InputState.Focus);
        dialogueBox.OpenDialogueBox();

        yield return new WaitUntil(() => dialogueFinished);

        InputHandler.Instance.SetInputState(previousInputState);
    }
    #endregion

    #region Inventory Menu
    /// <summary>
    /// You must Initialize the inventory menu before showing it.
    /// </summary>
    private async Task ShowInventoryAsync()
    {
        if (inventoryMenu == null) return;

        // Set input state first
        InputHandler.Instance.SetInputState(InputState.UI);
        
        // handle subscriptions
        InputHandler.Instance.OnInventoryMenuInput.RemoveListener(() => SetState(UIMasterState.Inventory));
        InputHandler.Instance.OnUI_CancelInput.AddListener(() => SetState(UIMasterState.FirstPerson));

        StopGamePlay(true);
        hudController.HideAllHUD(true);
        inventoryMenu.ShowInventory();

        // Wait until inventory is open
        while (!inventoryMenu.isOpen)
        {
            await Task.Yield();
        }

        GameMaster.Instance?.gm_InventoryOpened?.Invoke();
    }

    private async Task HideInventoryAsync()
    {
        if (inventoryMenu == null) return;

        // Remove cancel listener
        InputHandler.Instance?.OnUI_CancelInput?.RemoveListener(() => SetState(UIMasterState.FirstPerson));

        inventoryMenu.HideInventory();
        hudController.HideAllHUD(false);

        // Wait until inventory is closed
        while (inventoryMenu.isOpen)
        {
            await Task.Yield();
        }

        StopGamePlay(false);

        // Restore inventory button listener
        InputHandler.Instance?.OnInventoryMenuInput?.AddListener(() => SetState(UIMasterState.Inventory));

        GameMaster.Instance?.gm_InventoryClosed?.Invoke();
    }
    #endregion

    #region Pause Menu
    private async Task ShowPauseMenuAsync()
    {
        if (pauseMenu == null) return;
        
        // First ensure we're not still processing any previous input events
        await Task.Yield();
        
        // Remove pause menu listener to prevent double-triggering
        InputHandler.Instance?.OnPauseMenuInput?.RemoveListener(() => SetState(UIMasterState.Pause));

        // Set input state
        InputHandler.Instance?.SetInputState(InputState.UI);
        
        // Show the pause menu UI
        pauseMenu.ShowPauseMenu();
        GameMaster.Instance?.gm_GamePaused?.Invoke();

        // Add cancel listener after a short delay to avoid accidental immediate cancellation
        await Task.Delay(50);
        InputHandler.Instance?.OnUI_CancelInput?.AddListener(() => SetState(UIMasterState.FirstPerson));

        StopGamePlay(true);

        // Give time for the UI to fully appear
        await Task.Delay(20);
    }

    private async Task HidePauseMenuAsync()
    {
        if (pauseMenu == null) return;

        // First ensure we're not still processing any previous input events
        await Task.Yield();

        // Remove cancel listener
        InputHandler.Instance?.OnUI_CancelInput?.RemoveListener(() => SetState(UIMasterState.FirstPerson));

        // Hide the pause menu UI
        pauseMenu.HidePauseMenu();

        // Save settings when exiting pause
        GameMaster.Instance?.SaveAndApplySettings();
        GameMaster.Instance?.gm_GameUnpaused?.Invoke();

        // Restore normal gameplay
        StopGamePlay(false);
        
        // Give time for the UI to fully disappear
        await Task.Delay(20);
        
        // Only restore the pause button listener after a delay to prevent accidental re-triggering
        await Task.Delay(50);
        InputHandler.Instance?.OnPauseMenuInput?.AddListener(() => SetState(UIMasterState.Pause));
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