using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PauseMenu : MonoBehaviour
{
    #region Variables

    [Header("General")]
    [Space(10)]

    [SerializeField] private float pauseCooldown = 0.5f;
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Canvas settingsMenuCanvas;

    [Header("Main Pause Menu")]
    [Space(10)]
    
    [SerializeField] private Button b_Resume;
    [SerializeField] private Button b_Settings;
    [SerializeField] private Button b_Quit;
    
    [Header("Settings Menu")]
    [Space(10)]
    
    [SerializeField] private Button b_BackToMenu; // Return button
    
    [Header("Settings Tabs")]
    [Space(10)]
    
    [SerializeField] private Button b_Gameplay;
    [SerializeField] private Button b_Video;
    [SerializeField] private Button b_Audio;
    [SerializeField] private Button b_Controls;
    [SerializeField] private Button b_Credits;
    [Space(10)]
    [SerializeField] private Image i_Gameplay;
    [SerializeField] private Image i_Video;
    [SerializeField] private Image i_Audio;
    [SerializeField] private Image i_Controls;
    [SerializeField] private Image i_Credits;
    [Space(10)]
    [SerializeField] private Sprite tab_normal;
    [SerializeField] private Sprite tab_selected;
    [SerializeField] private Sprite tab_active;

    private enum SettingsMenuTab
    {
        Gameplay,
        Video,
        Audio,
        Controls,
        Credits
    }

    
    
    private MenuTab mt_Gameplay;
    private MenuTab mt_Video;
    private MenuTab mt_Audio;
    private MenuTab mt_Controls;
    private MenuTab mt_Credits;

    private MenuTab activeTab;
    private MenuTab defaultTab;

    private MenuTab[] tabs;

    [Header("Settings Panels")]
    [Space(10)]
    
    [SerializeField] private Canvas gameplayPanel; // default panel
    [SerializeField] private Canvas videoPanel;
    [SerializeField] private Canvas audioPanel;
    [SerializeField] private Canvas controlsPanel;
    [SerializeField] private Canvas creditsPanel;
    
    [Header("Gameplay Settings Elements")]
    [Space(10)]
    
    [SerializeField] private Toggle t_InvertY;
    [SerializeField] private Slider s_VertSensitivity;
    [SerializeField] private Slider s_HorizSensitivity;

    private float defaultVerticalSensitivity = 0.5f;
    private float defaultHorizontalSensitivity = 0.5f;
    
    [Header("Visual Feedback")]
    [Space(10)]
    
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color selectedColor = new Color(1f, .8f, .2f, 1f);
    [SerializeField] private Color activeTabColor = new Color(.8f, .8f, 1f, 1f);

    #endregion

    void Start()
    {
        mt_Gameplay = new MenuTab(b_Gameplay, i_Gameplay, gameplayPanel);
        mt_Video = new MenuTab(b_Video, i_Video, videoPanel);
        mt_Audio = new MenuTab(b_Audio, i_Audio, audioPanel);
        mt_Controls = new MenuTab(b_Controls, i_Controls, controlsPanel);
        mt_Credits = new MenuTab(b_Credits, i_Credits, creditsPanel);

        tabs = new MenuTab[] { mt_Gameplay, mt_Video, mt_Audio, mt_Controls, mt_Credits };

        SetTabColor(mt_Gameplay, tab_normal);
        SetTabColor(mt_Video, tab_normal);
        SetTabColor(mt_Audio, tab_normal);
        SetTabColor(mt_Controls, tab_normal);
        SetTabColor(mt_Credits, tab_normal);

        defaultTab = mt_Gameplay;
        
        SwitchTab(defaultTab);

        t_InvertY.onValueChanged.AddListener(OnInvertYChanged);
        s_VertSensitivity.onValueChanged.AddListener(OnVerticalSensitivityChanged);
        s_HorizSensitivity.onValueChanged.AddListener(OnHorizontalSensitivityChanged);
    }

    void SetTabColor(MenuTab tab, Sprite sprite)
    {
        tab.image.sprite = sprite;
    }

    void OnTabHover(MenuTab tab)
    {
        if (tab == activeTab)
        {
            SetTabColor(tab, tab_active);
        }
        else
        {
            SetTabColor(tab, tab_selected);
        }
    }

    void OnTabExit(MenuTab tab) // OnTabUnhover
    {
        if (tab == activeTab)
        {
            SetTabColor(tab, tab_active);
        }
        else
        {
            SetTabColor(tab, tab_normal);
        }
    }

    void OnTabClick(MenuTab tab)
    {
        if (tab == activeTab)
        {
            return;
        }
        else
        {
            SwitchTab(tab);
        }
    }

    void SwitchTab(MenuTab tab)
    {
        if (tab == activeTab)
        {
            return;
        }

        foreach (MenuTab t in tabs)
        {
            if (t == tab)
            {
                SetTabColor(t, tab_active);
            }
            else
            {
                SetTabColor(t, tab_normal);
            }
        }

        if (tab == mt_Gameplay)
        {
            SwitchSettingsPanel(SettingsMenuTab.Gameplay);
        }
        else if (tab == mt_Video)
        {
            SwitchSettingsPanel(SettingsMenuTab.Video);
        }
        else if (tab == mt_Audio)
        {
            SwitchSettingsPanel(SettingsMenuTab.Audio);
        }
        else if (tab == mt_Controls)
        {
            SwitchSettingsPanel(SettingsMenuTab.Controls);
        }
        else if (tab == mt_Credits)
        {
            SwitchSettingsPanel(SettingsMenuTab.Credits);
        }

        activeTab = tab;
    }

    private void SwitchSettingsPanel(SettingsMenuTab tab)
    {
        gameplayPanel.enabled = (tab == SettingsMenuTab.Gameplay);
        videoPanel.enabled = (tab == SettingsMenuTab.Video);
        audioPanel.enabled = (tab == SettingsMenuTab.Audio);
        controlsPanel.enabled = (tab == SettingsMenuTab.Controls);
        creditsPanel.enabled = (tab == SettingsMenuTab.Credits);
    }

    // Settings Handlers
    #region Settings Handlers
    
    /// <summary>
    /// Handle invert Y axis toggle change
    /// </summary>
    private void OnInvertYChanged(bool isInverted)
    {
        GameMaster.Instance.GetSettings().invertYAxis = isInverted;
        GameMaster.Instance.SaveSettings();
        GameMaster.Instance.ApplySettings();
    }
    
    /// <summary>
    /// Handle vertical sensitivity slider change
    /// </summary>
    private void OnVerticalSensitivityChanged(float value)
    {
        GameMaster.Instance.GetSettings().mouseSensitivityVertical = value;
        GameMaster.Instance.SaveSettings();
        GameMaster.Instance.ApplySettings();
    }
    
    /// <summary>
    /// Handle horizontal sensitivity slider change
    /// </summary>
    private void OnHorizontalSensitivityChanged(float value)
    {
        GameMaster.Instance.GetSettings().mouseSensitivityHorizontal = value;
        GameMaster.Instance.SaveSettings();
        GameMaster.Instance.ApplySettings();
    }
    
    /// <summary>
    /// Reset vertical sensitivity to default value
    /// </summary>
    public void ResetVerticalSensitivity()
    {
        if (s_VertSensitivity != null)
        {
            s_VertSensitivity.value = defaultVerticalSensitivity;
        }
    }
    
    /// <summary>
    /// Reset horizontal sensitivity to default value
    /// </summary>
    public void ResetHorizontalSensitivity()
    {
        if (s_HorizSensitivity != null)
        {
            s_HorizSensitivity.value = defaultHorizontalSensitivity;
        }
    }
    
    #endregion
}
