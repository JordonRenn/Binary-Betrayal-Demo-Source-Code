using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button b_Start;
    [SerializeField] Button b_C01_03;
    [SerializeField] Button b_Settings;
    [SerializeField] Button b_Exit;

    void Awake()
    {
        b_Start.onClick.AddListener(StartClicked);
        b_Settings.onClick.AddListener(SettingsClicked);
        b_Exit.onClick.AddListener(ExitClicked);
        b_C01_03.onClick.AddListener(C01_03Clicked);
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

    public void C01_03Clicked()
    {
        CustomSceneManager.Instance.LoadScene(SceneName.C01_03);
    }
}
