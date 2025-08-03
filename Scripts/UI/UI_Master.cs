using System.ComponentModel;
using System.Collections;
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

    private bool isInventoryOpen = false;

    [Header("Developer Options")]
    [SerializeField] private bool GenerateDummyInventories = false;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance) == this)
        {
            InitializeUI();
            SubscribeToEvents();
        }
    }

    private void InitializeUI()
    {
        if (inventoryMenu != null)
        {
            inventoryMenu.gameObject.SetActive(false);
        }

        if (GenerateDummyInventories)
        {
            // Generate dummy inventories for testing purposes
            // Wait a frame to ensure InventoryManager is initialized
            StartCoroutine(GenerateDummyInventoriesCoroutine());
        }
    }

    private void SubscribeToEvents()
    {
        if (FPS_InputHandler.Instance != null)
        {
            FPS_InputHandler.Instance.inventoryMenuButtonTriggered.AddListener(ToggleInventory);
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
            ShowInventory();
        else
            HideInventory();
    }

    private void ShowInventory()
    {
        if (inventoryMenu == null) return;

        GameMaster.Instance.gm_InventoryMenuOpened.Invoke();
        
        // Hide HUD elements
        HideAllHUD();
        
        // Setup post-processing
        VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Switch input mode
        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
        
        // Show inventory
        inventoryMenu.gameObject.SetActive(true);
        
        // Get the player inventory from InventoryManager or generate dummy data
        IInventory playerInventory = null;
        
        if (GenerateDummyInventories)
        {
            // Force regenerate dummy inventory from JSON
            DummyInventoryGenerator.GenerateDummyInventories();
        }
        
        // Get the inventory from InventoryManager
        if (InventoryManager.Instance != null)
        {
            playerInventory = InventoryManager.Instance.GetPlayerInventory();
        }
        
        // Fallback to empty inventory if nothing is available
        if (playerInventory == null)
        {
            playerInventory = new Inv_Player("temp_player", "Player Inventory", 100);
            Debug.LogWarning("No inventory found, using empty temporary inventory");
        }
        
        inventoryMenu.Initialize(playerInventory);
        
        // Update input listeners
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.RemoveListener(ToggleInventory);
        FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(ToggleInventory);
    }

    private void HideInventory()
    {
        if (inventoryMenu == null) return;

        GameMaster.Instance.gm_InventoryMenuClosed.Invoke();

        // Hide inventory
        inventoryMenu.gameObject.SetActive(false);
        
        // Show HUD elements
        ShowAllHUD();
        
        // Reset post-processing
        VolumeManager.Instance.SetVolume(VolumeType.Default);
        
        // Unpause the game
        Time.timeScale = 1f;
        
        // Switch input mode back
        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        
        // Update input listeners
        FPS_InputHandler.Instance.inventoryMenuButtonTriggered.AddListener(ToggleInventory);
        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(ToggleInventory);
    }

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

    private IEnumerator GenerateDummyInventoriesCoroutine()
    {
        // Wait a frame to ensure all managers are initialized
        yield return null;
        
        // Generate inventory from JSON when needed
        Debug.Log("Dummy inventory generation will happen when inventory is opened.");
    }
}