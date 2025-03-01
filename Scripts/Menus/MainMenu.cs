using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button b_Start;
    [SerializeField] Button b_Settings;
    [SerializeField] Button b_Exit;

    void Awake()
    {
        b_Start.onClick.AddListener(StartClicked);
        b_Settings.onClick.AddListener(SettingsClicked);
        b_Exit.onClick.AddListener(ExitClicked);
    }

    public void StartClicked()
    {
        Debug.Log("MAIN MENU | Start Clicked");

        CustomSceneManager.Instance.LoadScene(SceneName.Dev_1);
    }

    public void SettingsClicked()
    {
        Debug.Log("MAIN MENU | Settings Clicked");
    }

    public void ExitClicked()
    {
        Debug.Log("MAIN MENU | Exit Clicked");
    }
}
