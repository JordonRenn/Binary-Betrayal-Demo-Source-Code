// new UI manager that leverages UI builder / toolkit
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using FMODUnity;
using System.Collections.Generic;

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
    /* [SerializeField] private UIDocument MapDocument; */

    [SerializeField] private UIDocument HUDDocument;
    private PauseMenu pauseMenu;
    private InventoryMenu inventoryMenu;
    private JournalMenu journalMenu;
    private PlayerMenu playerMenu;
    /* private MapMenu mapMenu; */

    private HUDController hudController;

    private List<UIDocument> allMenuDocuments = new List<UIDocument>();

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
        if (PauseMenuDocument != null)
        {
            pauseMenu = PauseMenuDocument.gameObject.GetComponent<PauseMenu>();
            allMenuDocuments.Add(PauseMenuDocument);
        }

        if (InventoryDocument != null)
        {
            inventoryMenu = InventoryDocument.gameObject.GetComponent<InventoryMenu>();
            allMenuDocuments.Add(InventoryDocument);
        }

        if (JournalDocument != null)
        {
            journalMenu = JournalDocument.gameObject.GetComponent<JournalMenu>();
            allMenuDocuments.Add(JournalDocument);
        }

        if (PlayerStatsDocument != null)
        {
            playerMenu = PlayerStatsDocument.gameObject.GetComponent<PlayerMenu>();
            allMenuDocuments.Add(PlayerStatsDocument);
        }

        /* if (MapDocument != null)
        {
            mapMenu = MapDocument.gameObject.GetComponent<MapMenu>();
            allMenuDocuments.Add(MapDocument);
        } */

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
                      element.ClassListContains("unity-tab__header"))) ||
                    evt.target is TabButton)
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

        InputHandler.Instance?.OnPauseMenuInput?.AddListener(async () => await ShowPauseMenu());
        InputHandler.Instance?.OnInventoryMenuInput?.AddListener(async () => await ShowInventoryMenu());
        InputHandler.Instance?.OnPlayerMenuInput?.AddListener(async () => await ShowPlayerMenu());
        InputHandler.Instance?.OnJournalMenuInput?.AddListener(async () => await ShowJournalMenu());
    }

    #region Menu Switching
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

        await MenuExitCheck();
    }

    /* async so we can animate in future */
    private void ClearAllMenuListeners()
    {
        var input = InputHandler.Instance;
        if (input == null) return;

        input.OnPauseMenuInput?.RemoveAllListeners();
        input.OnUI_CancelInput?.RemoveAllListeners();
        input.OnUI_InventoryInput?.RemoveAllListeners();
        input.OnUI_JournalInput?.RemoveAllListeners();
        input.OnUI_PlayerInput?.RemoveAllListeners();
    }

    private async Task MenuEntranceCheck()
    {
        var input = InputHandler.Instance;
        if (input == null) return;

        // SBGDebug.LogInfo($"Current Input State: {input.currentState}", "UIManager | MenuEntranceCheck");

        if (input.currentState == InputState.FirstPerson)
        {
            ClearAllMenuListeners();
            input.SetInputState(InputState.UI);
            HideAllHUD(true);

            // Add UI state listeners - using local async lambdas
            input.OnUI_CancelInput.AddListener(() => { _ = HideAllMenus(); });
            input.OnUI_InventoryInput.AddListener(() => { _ = ShowInventoryMenu(); });
            input.OnUI_JournalInput.AddListener(() => { _ = ShowJournalMenu(); });
            input.OnUI_PlayerInput.AddListener(() => { _ = ShowPlayerMenu(); });

            await Task.Delay(10); // simulate animation time
        }
    }

    private async Task MenuExitCheck()
    {
        var input = InputHandler.Instance;
        if (input == null) return;

        // SBGDebug.LogInfo($"Current Input State: {input.currentState}", "UIManager | MenuExitCheck");

        if (input.currentState == InputState.UI)
        {
            ClearAllMenuListeners();
            input.SetInputState(InputState.FirstPerson);
            HideAllHUD(false);

            // Add FirstPerson state listener
            input.OnPauseMenuInput?.AddListener(() => { _ = ShowPauseMenu(); });

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