// new UI manager that leverages UI builder / toolkit

using BinaryBetrayal.InputManagement;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using FMODUnity;
using System.Collections.Generic;
using InputSys = BinaryBetrayal.InputManagement.InputSystem;

// rename once existing UIManager can be replaced by this
#region UIManager
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

    // only commented out to avoid compile errors for now
    /* public enum UIMasterState
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
    } */


    // [SerializeField] private EventReference openMenuSound;
    // [SerializeField] private EventReference closeMenuSound;
    [SerializeField] private EventReference mouseClickEnterSound;
    [SerializeField] private EventReference mouseClickExitSound;
    [SerializeField] private EventReference mouseHoverSound;

    [SerializeField] private UIDocument PauseMenuDocument;
    [SerializeField] private UIDocument InventoryDocument;
    [SerializeField] private UIDocument JournalDocument;
    [SerializeField] private UIDocument PlayerStatsDocument;
    // [SerializeField] private UIDocument MapDocument;
    [SerializeField] private UIDocument reticleDocument;
    [SerializeField] private UIDocument HUDDocument;

    private PauseMenu pauseMenu;
    private InventoryMenu inventoryMenu;
    private JournalMenu journalMenu;
    private PlayerMenu playerMenu;
    // private MapMenu mapMenu; 
    // private ReticleSystem reticle;

    private HUDController hudController;

    private List<UIDocument> allMenuDocuments = new List<UIDocument>();
    
    // Track which menu is currently open
    private UIDocument currentlyOpenMenu = null;

    #region Initialization
    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            GetComponents();
            SubscribeToEvents();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void GetComponents()
    {
        // Load PanelSettings if not assigned
        PanelSettings panelSettings = Resources.Load<PanelSettings>("UI Toolkit/PanelSettings");
        if (panelSettings == null)
        {
            Debug.LogError("[UIManager] PanelSettings not found in Resources/UI Toolkit/PanelSettings - UI Toolkit may not work properly");
        }
        // IMPORTANT: In the Unity Inspector, set PanelSettings "Update Mode" to "Unscaled Time" for UI to work with Time.timeScale = 0

        if (PauseMenuDocument != null)
        {
            pauseMenu = PauseMenuDocument.gameObject.GetComponent<PauseMenu>();
            allMenuDocuments.Add(PauseMenuDocument);
            if (PauseMenuDocument.panelSettings == null && panelSettings != null)
            {
                PauseMenuDocument.panelSettings = panelSettings;
                Debug.Log("[UIManager] Assigned PanelSettings to PauseMenuDocument");
            }
        }

        if (InventoryDocument != null)
        {
            inventoryMenu = InventoryDocument.gameObject.GetComponent<InventoryMenu>();
            allMenuDocuments.Add(InventoryDocument);
            if (InventoryDocument.panelSettings == null && panelSettings != null)
            {
                InventoryDocument.panelSettings = panelSettings;
                Debug.Log("[UIManager] Assigned PanelSettings to InventoryDocument");
            }
        }

        if (JournalDocument != null)
        {
            journalMenu = JournalDocument.gameObject.GetComponent<JournalMenu>();
            allMenuDocuments.Add(JournalDocument);
            if (JournalDocument.panelSettings == null && panelSettings != null)
            {
                JournalDocument.panelSettings = panelSettings;
                Debug.Log("[UIManager] Assigned PanelSettings to JournalDocument");
            }
        }

        if (PlayerStatsDocument != null)
        {
            playerMenu = PlayerStatsDocument.gameObject.GetComponent<PlayerMenu>();
            allMenuDocuments.Add(PlayerStatsDocument);
            if (PlayerStatsDocument.panelSettings == null && panelSettings != null)
            {
                PlayerStatsDocument.panelSettings = panelSettings;
                Debug.Log("[UIManager] Assigned PanelSettings to PlayerStatsDocument");
            }
        }

        hudController = HUDDocument?.gameObject.GetComponent<HUDController>();

        RegisterGlobalUICallbacks();
    }
    #endregion

    private void RegisterGlobalUICallbacks()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            // Register hover sound
            document.rootVisualElement.RegisterCallback<PointerEnterEvent>((evt) =>
            {
                if (evt.target is Button ||
                    (evt.target is VisualElement element &&
                     (element.ClassListContains("hoverable") ||
                      element.ClassListContains("unity-tab__header")))
/* #if UNITY_EDITOR
                    || evt.target is TabButton
#endif */
                    )
                {
                    RuntimeManager.PlayOneShot(mouseHoverSound);
                }
            }, TrickleDown.TrickleDown);

            // Register click sounds
            document.rootVisualElement.RegisterCallback<ClickEvent>((evt) =>
            {
                if (evt.target is VisualElement element)
                {
                    if (element.ClassListContains("click-sfx__enter"))
                    {
                        RuntimeManager.PlayOneShot(mouseClickEnterSound);
                    }
                    else if (element.ClassListContains("click-sfx__exit"))
                    {
                        RuntimeManager.PlayOneShot(mouseClickExitSound);
                    }
                }
            }, TrickleDown.TrickleDown);
        }

        SetInitialState();
    }

    private void SetInitialState()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private async void SubscribeToEvents()
    {
        await Task.Run(() => new WaitUntil(() => InventoryManager.Instance != null && GameMaster.Instance != null));

        // Global input map is always active - subscribe once and handle menu toggling
        InputSys.OnPauseMenuDown_global += HandlePauseMenuToggle;
        InputSys.OnInventoryDown_global += HandleInventoryMenuToggle;
        InputSys.OnPlayerMenuDown_global += HandlePlayerMenuToggle;
        InputSys.OnJournalDown_global += HandleJournalMenuToggle;
    }

    #region Menu Switching
    // Toggle handlers for global input
    private async void HandlePauseMenuToggle()
    {
        // Only respond to menu toggles from FirstPerson or if toggling the currently open menu
        if (InputSys.currentState == InputState.FirstPerson)
        {
            await ShowPauseMenu();
        }
        else if (InputSys.currentState == InputState.UI && currentlyOpenMenu == PauseMenuDocument)
        {
            await HideAllMenus();
        }
        // Ignore if in UI state but different menu is open
    }

    private async void HandleInventoryMenuToggle()
    {
        if (InputSys.currentState == InputState.FirstPerson)
        {
            await ShowInventoryMenu();
        }
        else if (InputSys.currentState == InputState.UI)
        {
            if (currentlyOpenMenu == InventoryDocument)
            {
                await HideAllMenus();
            }
            else
            {
                // Switch to inventory from another menu
                await ShowInventoryMenu();
            }
        }
    }

    private async void HandleJournalMenuToggle()
    {
        if (InputSys.currentState == InputState.FirstPerson)
        {
            await ShowJournalMenu();
        }
        else if (InputSys.currentState == InputState.UI)
        {
            if (currentlyOpenMenu == JournalDocument)
            {
                await HideAllMenus();
            }
            else
            {
                // Switch to journal from another menu
                await ShowJournalMenu();
            }
        }
    }

    private async void HandlePlayerMenuToggle()
    {
        if (InputSys.currentState == InputState.FirstPerson)
        {
            await ShowPlayerMenu();
        }
        else if (InputSys.currentState == InputState.UI)
        {
            if (currentlyOpenMenu == PlayerStatsDocument)
            {
                await HideAllMenus();
            }
            else
            {
                // Switch to player menu from another menu
                await ShowPlayerMenu();
            }
        }
    }

    private async Task ShowPauseMenu()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        StopGamePlay(true);

        await MenuEntranceCheck();

        PauseMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        PauseMenuDocument.rootVisualElement.pickingMode = PickingMode.Position;
        Debug.Log($"[UIManager] Pause Menu Shown - PickingMode: {PauseMenuDocument.rootVisualElement.pickingMode}, Enabled: {PauseMenuDocument.rootVisualElement.enabledSelf}");
        currentlyOpenMenu = PauseMenuDocument;
    }

    private async Task ShowInventoryMenu()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        StopGamePlay(true);

        await MenuEntranceCheck();

        InventoryDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        InventoryDocument.rootVisualElement.pickingMode = PickingMode.Position;
        currentlyOpenMenu = InventoryDocument;
    }

    private async Task ShowJournalMenu()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        StopGamePlay(true);

        await MenuEntranceCheck();

        JournalDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        currentlyOpenMenu = JournalDocument;
    }

    private async Task ShowPlayerMenu()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        StopGamePlay(true);

        await MenuEntranceCheck();

        PlayerStatsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        currentlyOpenMenu = PlayerStatsDocument;
    }

    /* private async Task ShowMapMenu()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        StopGamePlay(true);

        await MenuEntranceCheck();

        MapDocument.rootVisualElement.style.display = DisplayStyle.Flex;
    } */

    public async Task HideAllMenus()
    {
        foreach (var document in allMenuDocuments)
        {
            if (document?.rootVisualElement == null) continue;

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        currentlyOpenMenu = null;
        await MenuExitCheck();
    }

    /* async so we can animate in future */
    private void ClearAllMenuListeners()
    {
        // No longer needed - global input map handles everything
        // Events are subscribed once in SubscribeToEvents() and never unsubscribed
    }

    private async Task MenuEntranceCheck()
    {
        if (InputSys.currentState == InputState.FirstPerson)
        {
            // Switch to UI input state
            InputSys.SetInputState(InputState.UI);
            HideAllHUD(true);
            reticleDocument.rootVisualElement.style.display = DisplayStyle.None;

            await Task.Delay(10); // simulate animation time
        }
    }

    private async Task MenuExitCheck()
    {
        // SBGDebug.LogInfo($"Current Input State: {InputSystem.currentState}", "UIManager | MenuExitCheck");

        if (InputSys.currentState == InputState.UI)
        {
            // Switch back to FirstPerson input state
            InputSys.SetInputState(InputState.FirstPerson);
            HideAllHUD(false);
            reticleDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            await Task.Delay(10); // simulate animation time

            StopGamePlay(false);
        }
    }
    #endregion

    #region Sound Effects
    private void PlaySoundEffect(EventReference soundEvent)
    {
        RuntimeManager.PlayOneShot(soundEvent);
    }

    public void PlayMouseClickEnterSound()
    {
        PlaySoundEffect(mouseClickEnterSound);
    }

    public void PlayMouseClickExitSound()
    {
        PlaySoundEffect(mouseClickExitSound);
    }

    public void PlayMouseHoverSound()
    {
        PlaySoundEffect(mouseHoverSound);
    }
    #endregion

    #region Helper Methods
    private void StopGamePlay(bool stop = true)
    {
        if (stop)
        {
            VolumeManager.Instance?.SetVolume(VolumeType.PauseMenu);
            Time.timeScale = 0f;
        }
        else
        {
            VolumeManager.Instance?.SetVolume(VolumeType.Default);
            Time.timeScale = 1f;
        }
    }
    #endregion

    #region Public Methods
    public void HideAllHUD(bool hide = true)
    {
        // will act according if HUD is globally enabled or disabled in settings

        hudController.HideAllHUD(hide);
    }
    #endregion
}
#endregion