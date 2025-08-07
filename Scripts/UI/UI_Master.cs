using Unity.VisualScripting;
using UnityEngine;

public class UI_Master : MonoBehaviour
{
    private static UI_Master _instance;
    public static UI_Master Instance
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
    [SerializeField] private GameObject DialogueBox;

    [Header("Menu References")]
    [SerializeField] private InventoryMenu inventoryMenu;
    private PauseMenu pauseMenu;
    private bool gamePaused = false;

    private bool isInventoryOpen = false;

    void Awake()
    {
        // persist accross scenes
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            InitializeUI();
            SubscribeToEvents();

            try
            {
                pauseMenu = FindFirstObjectByType<PauseMenu>();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error finding PauseMenu: {ex.Message}");
            }
        }
    }

    private void InitializeUI()
    {
        if (inventoryMenu != null)
        {
            inventoryMenu.gameObject.SetActive(false);
        }
    }

    private void SubscribeToEvents()
    {
        if (FPS_InputHandler.Instance != null)
        {
            FPS_InputHandler.Instance.inventoryMenuButtonTriggered.AddListener(ToggleInventory);
        }

        GameMaster.Instance.gm_GamePaused.AddListener(() =>
        {
            gamePaused = true;
        });

        GameMaster.Instance.gm_GameUnpaused.AddListener(() =>
        {
            gamePaused = false;
        });
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
        {
            ShowInventory();
        }
        else if (gamePaused && pauseMenu != null)
        {
            pauseMenu.HidePauseMenu();
            ShowInventory();
        }
        else if (gamePaused && pauseMenu == null)
        {
            PauseMenu menu = FindFirstObjectByType<PauseMenu>();

            if (menu != null)
            {
                menu.HidePauseMenu();
                pauseMenu = menu;
                Debug.Log("PauseMenu found, hiding it before showing inventory.");
                ShowInventory();
            }
        }
        else
        {
            HideInventory();
        }
    }

    #region Inventory
    private void ShowInventory()
    {
        if (inventoryMenu == null) return;
        
        inventoryMenu.RefreshInventory();

        GameMaster.Instance.gm_InventoryMenuOpened.Invoke();

        HideAllHUD();

        VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);

        Time.timeScale = 0f;

        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);

        inventoryMenu.gameObject.SetActive(true);

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

        inventoryMenu.Initialize(playerInventory);

        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.RemoveListener(ToggleInventory);
        FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(ToggleInventory);
    }

    private void HideInventory()
    {
        if (inventoryMenu == null) return;

        GameMaster.Instance.gm_InventoryMenuClosed.Invoke();

        inventoryMenu.gameObject.SetActive(false);

        ShowAllHUD();

        VolumeManager.Instance.SetVolume(VolumeType.Default);

        Time.timeScale = 1f;

        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.AddListener(ToggleInventory);
        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(ToggleInventory);
    }
    #endregion

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
}