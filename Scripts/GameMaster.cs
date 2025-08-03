using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance 
    {
        get
        {
            if (_instance == null)
            {
                SBGDebug.LogWarning($"Attempting to access before initialization.", "GameMaster");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    public List<Trackable> allTrackables = new List<Trackable>(); 

    //OBJECT INSTANTIATION

    //Player Objects
    [HideInInspector] public UnityEvent gm_PlayerSpawned;
    [HideInInspector] public UnityEvent gm_WeaponHudSpawned;
    [HideInInspector] public UnityEvent gm_ReticleSystemSpawned;
    [HideInInspector] public UnityEvent gm_WeaponPoolSpawned;
    [HideInInspector] public UnityEvent gm_FPSMainSpawned;

    //Level Objects
    //
    //
    //

    //Game Play Events
    [HideInInspector] public UnityEvent gm_GamePaused;
    [HideInInspector] public UnityEvent gm_GameUnpaused;
    [HideInInspector] public UnityEvent gm_ReturnToMainMenu;

    //tick event
    [HideInInspector] public UnityEvent globalTick;

    //Settings Events
    [HideInInspector] public UnityEvent gm_SettingsChanged;

    //Inventory Events
    [HideInInspector] public UnityEvent gm_InventoryMenuOpened;
    [HideInInspector] public UnityEvent gm_InventoryMenuOpened_Container;
    [HideInInspector] public UnityEvent gm_InventoryMenuOpened_NPC;
    [HideInInspector] public UnityEvent gm_InventoryMenuClosed;
    [HideInInspector] public UnityEvent gm_InventoryMenuClosed_Container;
    [HideInInspector] public UnityEvent gm_InventoryMenuClosed_NPC;
    [HideInInspector] public UnityEvent gm_InventoryItemAdded;
    [HideInInspector] public UnityEvent gm_InventoryItemAdded_Container;
    [HideInInspector] public UnityEvent gm_InventoryItemAdded_NPC;
    [HideInInspector] public UnityEvent gm_InventoryItemRemoved;
    [HideInInspector] public UnityEvent gm_InventoryItemRemoved_Container;
    [HideInInspector] public UnityEvent gm_InventoryItemRemoved_NPC;

    //Settings Management
    [SerializeField] private PlayerSettings playerSettings = new PlayerSettings();
    private const string SETTINGS_KEY = "PlayerSettings";

    void Awake() 
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            LoadSettings();
        }
    }

    #region Settings Management
    /// <summary>
    /// Loads player settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                playerSettings = JsonUtility.FromJson<PlayerSettings>(json);
                SBGDebug.LogInfo("Settings loaded successfully", "GameMaster");
            }
            catch (System.Exception e)
            {
                SBGDebug.LogError($"Failed to load settings: {e.Message}", "GameMaster");
                playerSettings = new PlayerSettings();
            }
        }
        else
        {
            playerSettings = new PlayerSettings();
            SBGDebug.LogInfo("Using default settings", "GameMaster");
        }
    }

    /// <summary>
    /// Gets the current player settings
    /// </summary>
    public PlayerSettings GetSettings()
    {
        return playerSettings;
    }

    /// <summary>
    /// Saves and applies player settings
    /// </summary>
    public void SaveAndApplySettings()
    {
        SaveSettings();
        ApplySettings();
    }
    
    /// <summary>
    /// Saves player settings to PlayerPrefs
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(playerSettings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
            SBGDebug.LogInfo("Settings saved successfully", "GameMaster");
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Failed to save settings: {e.Message}", "GameMaster");
        }
    }
    
    /// <summary>
    /// Applies settings changes to relevant systems
    /// </summary>
    private void ApplySettings()
    {
        // Notify any systems that need to update when settings change
        if (FPS_InputHandler.Instance != null)
        {
            FPS_InputHandler.Instance.UpdateSensitivitySettings();
        }

        // Notify all listeners that settings have changed
        gm_SettingsChanged.Invoke();
    }

    /// <summary>
    /// Resets player settings to default values
    /// </summary>
    public void ResetSettingsToDefault()
    {
        playerSettings = new PlayerSettings();
        SaveSettings();
        ApplySettings();
    }
    #endregion
}

/// <summary>
/// Class to store player-specific settings
/// </summary>
[System.Serializable]
public class PlayerSettings
{
    // Default language is English
    public Language language = Language.English;

    // Mouse sensitivity (0.5 is default/neutral)
    public float mouseSensitivityHorizontal = 0.5f;
    public float mouseSensitivityVertical = 0.5f;
    public bool invertYAxis = false;
    
    // Default multipliers - what 0.5 (neutral) sensitivity translates to
    public const float DEFAULT_HORIZONTAL_MULTIPLIER = 1.0f;
    public const float DEFAULT_VERTICAL_MULTIPLIER = 1.0f;
    
    // Calculate actual sensitivity multipliers
    public float GetHorizontalSensitivityMultiplier()
    {
        // 0.0 = 0.5x speed, 0.5 = 1.0x speed, 1.0 = 2.0x speed
        return DEFAULT_HORIZONTAL_MULTIPLIER * (0.5f + mouseSensitivityHorizontal);
    }
    
    public float GetVerticalSensitivityMultiplier()
    {
        // 0.0 = 0.5x speed, 0.5 = 1.0x speed, 1.0 = 2.0x speed
        return DEFAULT_VERTICAL_MULTIPLIER * (0.5f + mouseSensitivityVertical);
    }
}
