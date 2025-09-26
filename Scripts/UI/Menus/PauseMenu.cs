using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Threading.Tasks;

public class PauseMenu : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement settingsPanel;
    private UIManager uiManager;
    // private TabView settingsTabView;
    // private int currentTabIndex = 0;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        uiManager = GetComponentInParent<UIManager>();

        mainPanel = root.Q<VisualElement>("Pause-Container");
        settingsPanel = root.Q<VisualElement>("Settings-Container");
        // settingsTabView = root.Q<TabView>("Settings-TabView");

        SubscribeMainPanelButtons();
    }

    private void OnEnable()
    {
        // Show the main panel and hide the settings panel
        mainPanel.style.display = DisplayStyle.Flex;
        settingsPanel.style.display = DisplayStyle.None;
        
        // Setup controls panel
        SetupControlsPanel();
    }

    private void OnDisable()
    {
        // Hide the main panel and settings panel
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;
    }

    #region Button Subscriptions
    private void SubscribeMainPanelButtons()
    {
        var resumeButton = mainPanel.Q<Button>("Resume-Button");
        var settingsButton = mainPanel.Q<Button>("Settings-Button");
        var exitButton = mainPanel.Q<Button>("Exit-Button");

        resumeButton.clicked += OnResumeClicked;
        settingsButton.clicked += OnSettingsClicked;
        exitButton.clicked += OnExitClicked;
    }

    private void UnsubscribeMainPanelButtons()
    {
        var resumeButton = mainPanel.Q<Button>("Resume-Button");
        var settingsButton = mainPanel.Q<Button>("Settings-Button");
        var exitButton = mainPanel.Q<Button>("Exit-Button");

        resumeButton.clicked -= OnResumeClicked;
        settingsButton.clicked -= OnSettingsClicked;
        exitButton.clicked -= OnExitClicked;
    }

    private void SubscribeSettingsPanelButtons()
    {
        var backButton = settingsPanel.Q<Button>("Back-Button");
        var ApplyButton = settingsPanel.Q<Button>("Apply-Button");

        backButton.clicked += OnBackClicked;
        ApplyButton.clicked += OnApplyClicked;
    }

    private void UnsubscribeSettingsPanelButtons()
    {
        var backButton = settingsPanel.Q<Button>("Back-Button");
        var ApplyButton = settingsPanel.Q<Button>("Apply-Button");

        backButton.clicked -= OnBackClicked;
        ApplyButton.clicked -= OnApplyClicked;
    }
    #endregion

    #region Button Actions
    private void OnResumeClicked()
    {
        _ = uiManager.HideAllMenus();
    }

    private void OnSettingsClicked()
    {
        // Logic to switch to settings panel
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.Flex;
        UnsubscribeMainPanelButtons();
        SubscribeSettingsPanelButtons();
    }

    private void OnExitClicked()
    {
        // Logic to exit to main menu or quit game
        // Debug.Log("Exit button clicked");
        QuitGame();
    }

    private void OnBackClicked()
    {
        // Logic to switch back to main panel
        settingsPanel.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.Flex;
        UnsubscribeSettingsPanelButtons();
        SubscribeMainPanelButtons();
    }

    private void OnApplyClicked()
    {
        // You might want to add specific apply logic here
        // For example, if you want to batch-apply changes or show a confirmation
        GameSettings.SaveAndApplySettings();
    }
    #endregion

    private void QuitGame()
    {
        Application.Quit();
    }

    #region Settings
    private void SetupControlsPanel()
    {
        var controlsPanel = root.Q<VisualElement>("Controls-Panel");
        var gameplayPanel = root.Q<VisualElement>("Gameplay-Panel");
        
        if (controlsPanel != null)
        {
            // Setup sensitivity sliders
            var vertSlider = controlsPanel.Q<Slider>("VertSensitivity-Slider");
            var vertLabel = controlsPanel.Q<Label>("VertSensitivity-Value");
            var horizSlider = controlsPanel.Q<Slider>("HorizSensitivity-Slider");
            var horizLabel = controlsPanel.Q<Label>("HorizSensitivity-Value");
            
            SetupSensitivitySlider(vertSlider, vertLabel, true);
            SetupSensitivitySlider(horizSlider, horizLabel, false);
            
            // Setup invert Y switch
            var invertYSwitch = controlsPanel.Q<Switch>();
            if (invertYSwitch != null)
            {
                invertYSwitch.onValueChanged = (value) => {
                    var settings = GameSettings.GetSettings();
                    settings.invertYAxis = value;
                    GameSettings.SaveAndApplySettings();
                };
            }
        }

        if (gameplayPanel != null)
        {
            // Setup language dropdown in Gameplay panel
            var languageDropdown = gameplayPanel.Q<DropdownField>("Language-Dropdown");
            if (languageDropdown != null)
            {
                languageDropdown.choices = System.Enum.GetNames(typeof(Language)).ToList();
                languageDropdown.RegisterValueChangedCallback(evt => {
                    var settings = GameSettings.GetSettings();
                    settings.language = (Language)languageDropdown.index;
                    GameSettings.SaveAndApplySettings();
                });
            }
        }

        LoadControlSettings();
    }

    private void SetupSensitivitySlider(Slider slider, Label valueLabel, bool isVertical)
    {
        slider.lowValue = 0f;
        slider.highValue = 100f;
        
        slider.RegisterValueChangedCallback(evt => {
            valueLabel.text = $"{evt.newValue:F0}%";
            var settings = GameSettings.GetSettings();
            if (isVertical)
                settings.mouseSensitivityVertical = evt.newValue;
            else
                settings.mouseSensitivityHorizontal = evt.newValue;
            GameSettings.SaveAndApplySettings();
        });
    }

    private async void LoadControlSettings()
    {
        await Task.Run(() => new WaitUntil(() => GameMaster.Instance != null));

        var settings = GameSettings.GetSettings();
        var controlsPanel = root.Q<VisualElement>("Controls-Panel");
        var gameplayPanel = root.Q<VisualElement>("Gameplay-Panel");
        
        if (controlsPanel != null)
        {
            var vertSlider = controlsPanel.Q<Slider>("VertSensitivity-Slider");
            var horizSlider = controlsPanel.Q<Slider>("HorizSensitivity-Slider");
            var invertYSwitch = controlsPanel.Q<Switch>();
            
            if (vertSlider != null)
            {
                vertSlider.value = settings.mouseSensitivityVertical;
                controlsPanel.Q<Label>("VertSensitivity-Value").text = $"{settings.mouseSensitivityVertical:F0}%";
            }
            
            if (horizSlider != null)
            {
                horizSlider.value = settings.mouseSensitivityHorizontal;
                controlsPanel.Q<Label>("HorizSensitivity-Value").text = $"{settings.mouseSensitivityHorizontal:F0}%";
            }
            
            if (invertYSwitch != null)
            {
                invertYSwitch.value = settings.invertYAxis;
            }
        }

        if (gameplayPanel != null)
        {
            var languageDropdown = gameplayPanel.Q<DropdownField>("Language-Dropdown");
            if (languageDropdown != null)
            {
                languageDropdown.index = (int)settings.language;
            }
        }
    }

    
    #endregion
}