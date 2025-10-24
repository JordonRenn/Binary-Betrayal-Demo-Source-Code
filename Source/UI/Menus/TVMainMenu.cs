using UnityEngine;
using UnityEngine.Video;
using BinaryBetrayal.InputManagement;
using SBG;

public class TVMainMenu : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoClip clip_Main;
    private bool switching = false; 

    void Start()
    {
        videoPlayer.clip = clip_Main;
        videoPlayer.Play();

        // Subscribe to input events
        InputSystem.OnInteractDown_fp += OnInteractPressed;
        InputSystem.OnInteractDown_focus += OnInteractPressed;
        InputSystem.OnInteractDown_ui += OnInteractPressed;
    }

    void OnDestroy()
    {
        InputSystem.OnInteractDown_fp -= OnInteractPressed;
        InputSystem.OnInteractDown_focus -= OnInteractPressed;
        InputSystem.OnInteractDown_ui -= OnInteractPressed;
    }

    private void OnInteractPressed()
    {
        if (!switching)
        {
            switching = true;
            StartGame();
        }
    }

    private void StartGame()
    {
        CustomSceneManager.Instance.LoadScene(SceneName.Dev_1);
    }
}
