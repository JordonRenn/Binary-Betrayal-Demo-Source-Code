using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    private FPS_InputHandler input;

    [Header("Main Menus")]
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Canvas settingsMenuCanvas;

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button b_Resume;
    [SerializeField] private Button b_Settings;
    [SerializeField] private Button b_Quit;

    [Header("Settings Menu")]
    [SerializeField] private Button b_BackToMenu;

    [Header("Settings Tabs")]
    [SerializeField] private Button b_Gameplay;
    [SerializeField] private Button b_Video;
    [SerializeField] private Button b_Audio;
    [SerializeField] private Button b_Controls;
    [SerializeField] private Button b_Credits;
    [Space(10)]
    [SerializeField] private Color activeTabColor;
    [SerializeField] private Color inactiveTabColor;

    private Image btnImg_Gameplay;
    private Image btnImg_Video;
    private Image btnImg_Audio;
    private Image btnImg_Controls;
    private Image btnImg_Credits;

    [Header("Settings Panels")]
    [SerializeField] private Canvas gameplayPanel;
    [SerializeField] private Canvas videoPanel;
    [SerializeField] private Canvas audioPanel;
    [SerializeField] private Canvas controlsPanel;
    [SerializeField] private Canvas creditsPanel;

    [Header("Gameplay Elements")]
    [SerializeField] private Slider s_VertSensitivity;
    [SerializeField] private Slider s_HorizSensitivity;
    [SerializeField] private Toggle t_InvertY;
    [SerializeField] private TMP_Dropdown d_Language;


    // Internal states
    private bool isPaused = false;
    private PauseMenuState pMenuState = PauseMenuState.NotDisplayed;
    private SettingsMenuState sMenuState = SettingsMenuState.NotDisplayed;

    private void Start()
    {
        input = FPS_InputHandler.Instance;

        input.pauseMenuButtonTriggered.AddListener(TogglePauseMenu);
        SetupButtonListeners();

        pauseMenuCanvas.gameObject.SetActive(false);
        settingsMenuCanvas.gameObject.SetActive(false);
        HideAllSettingsPanels();

        // Setup settings UI listeners
        SetupSettingsUIBindings();

        btnImg_Gameplay = b_Gameplay.GetComponent<Image>();
        btnImg_Video = b_Video.GetComponent<Image>();
        btnImg_Audio = b_Audio.GetComponent<Image>();
        btnImg_Controls = b_Controls.GetComponent<Image>();
        btnImg_Credits = b_Credits.GetComponent<Image>();
    }

    private void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (isPaused)
            ShowPauseMenu();
        else
            HidePauseMenu();
    }

    private void ShowPauseMenu()
    {
        GameMaster.Instance.gm_GamePaused.Invoke();
        pMenuState = PauseMenuState.Main;
        sMenuState = SettingsMenuState.NotDisplayed;

        UI_Master.Instance.HideAllHUD();
        VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);
        Time.timeScale = 0f;

        input.pauseMenuButtonTriggered.RemoveListener(TogglePauseMenu);
        input.menu_CancelTriggered.AddListener(TogglePauseMenu);
        input.SetInputState(InputState.MenuNavigation);

        pauseMenuCanvas.gameObject.SetActive(true);
        settingsMenuCanvas.gameObject.SetActive(false);
    }

    private void HidePauseMenu()
    {
        pMenuState = PauseMenuState.NotDisplayed;
        sMenuState = SettingsMenuState.NotDisplayed;

        input.pauseMenuButtonTriggered.AddListener(TogglePauseMenu);
        input.menu_CancelTriggered.RemoveListener(TogglePauseMenu);
        input.SetInputState(InputState.FirstPerson);

        pauseMenuCanvas.gameObject.SetActive(false);
        settingsMenuCanvas.gameObject.SetActive(false);

        UI_Master.Instance.ShowAllHUD();
        VolumeManager.Instance.SetVolume(VolumeType.Default);
        Time.timeScale = 1f;

        // Save settings when exiting pause
        GameMaster.Instance.SaveAndApplySettings();
        GameMaster.Instance.gm_GameUnpaused.Invoke();
    }

    // ---- Settings Menu Logic ----
    private void ToggleSettingsMenu()
    {
        if (pMenuState == PauseMenuState.Main)
        {
            pMenuState = PauseMenuState.Settings;
            SwitchSettingsTab(SettingsMenuState.Gameplay);

            // Load settings into UI when entering settings menu
            LoadSettingsToUI();

            pauseMenuCanvas.gameObject.SetActive(false);
            settingsMenuCanvas.gameObject.SetActive(true);
        }
        else if (pMenuState == PauseMenuState.Settings)
        {
            pMenuState = PauseMenuState.Main;
            sMenuState = SettingsMenuState.NotDisplayed;

            settingsMenuCanvas.gameObject.SetActive(false);
            pauseMenuCanvas.gameObject.SetActive(true);
        }
    }

    private void SwitchSettingsTab(SettingsMenuState tab)
    {
        sMenuState = tab;
        HideAllSettingsPanels();
        UnhighlightAllButtons();

        switch (tab)
        {
            case SettingsMenuState.Gameplay:
                gameplayPanel.gameObject.SetActive(true);
                btnImg_Gameplay.color = activeTabColor;
                break;
            case SettingsMenuState.Video:
                videoPanel.gameObject.SetActive(true);
                btnImg_Video.color = activeTabColor;
                break;
            case SettingsMenuState.Audio:
                audioPanel.gameObject.SetActive(true);
                btnImg_Audio.color = activeTabColor;
                break;
            case SettingsMenuState.Controls:
                controlsPanel.gameObject.SetActive(true);
                btnImg_Controls.color = activeTabColor;
                break;
            case SettingsMenuState.Credits:
                creditsPanel.gameObject.SetActive(true);
                btnImg_Credits.color = activeTabColor;
                break;
        }
    }

    private void HideAllSettingsPanels()
    {
        gameplayPanel.gameObject.SetActive(false);
        videoPanel.gameObject.SetActive(false);
        audioPanel.gameObject.SetActive(false);
        controlsPanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(false);
    }

    private void UnhighlightAllButtons()
    {
        btnImg_Gameplay.color = inactiveTabColor;
        btnImg_Video.color = inactiveTabColor;
        btnImg_Audio.color = inactiveTabColor;
        btnImg_Controls.color = inactiveTabColor;
        btnImg_Credits.color = inactiveTabColor;
    }

    // ---- Button Listeners Setup ----
    private void SetupButtonListeners()
    {
        b_Resume.onClick.AddListener(TogglePauseMenu);
        b_Settings.onClick.AddListener(ToggleSettingsMenu);
        b_Quit.onClick.AddListener(QuitGame);

        b_BackToMenu.onClick.AddListener(ToggleSettingsMenu);

        b_Gameplay.onClick.AddListener(() => SwitchSettingsTab(SettingsMenuState.Gameplay));
        b_Video.onClick.AddListener(() => SwitchSettingsTab(SettingsMenuState.Video));
        b_Audio.onClick.AddListener(() => SwitchSettingsTab(SettingsMenuState.Audio));
        b_Controls.onClick.AddListener(() => SwitchSettingsTab(SettingsMenuState.Controls));
        b_Credits.onClick.AddListener(() => SwitchSettingsTab(SettingsMenuState.Credits));
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    // ---- Settings UI Binding ----
    private void SetupSettingsUIBindings()
    {
        s_VertSensitivity.onValueChanged.AddListener(value =>
        {
            var settings = GameMaster.Instance.GetSettings();
            settings.mouseSensitivityVertical = value;
            GameMaster.Instance.SaveAndApplySettings();
        });

        s_HorizSensitivity.onValueChanged.AddListener(value =>
        {
            var settings = GameMaster.Instance.GetSettings();
            settings.mouseSensitivityHorizontal = value;
            GameMaster.Instance.SaveAndApplySettings();
        });

        t_InvertY.onValueChanged.AddListener(isOn =>
        {
            var settings = GameMaster.Instance.GetSettings();
            settings.invertYAxis = isOn;
            GameMaster.Instance.SaveAndApplySettings();
        });

        d_Language.onValueChanged.AddListener(index =>
        {
            var settings = GameMaster.Instance.GetSettings();
            settings.language = (Language)index;
            GameMaster.Instance.SaveAndApplySettings();
        });
    }

    private void LoadSettingsToUI()
    {
        var settings = GameMaster.Instance.GetSettings();

        s_VertSensitivity.SetValueWithoutNotify(settings.mouseSensitivityVertical);
        s_HorizSensitivity.SetValueWithoutNotify(settings.mouseSensitivityHorizontal);
        t_InvertY.SetIsOnWithoutNotify(settings.invertYAxis);
        d_Language.SetValueWithoutNotify((int)settings.language);
    }
}
