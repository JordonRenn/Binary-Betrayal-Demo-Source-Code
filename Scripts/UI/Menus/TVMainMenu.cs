using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TVMainMenu : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoClip clip_Main;
    private bool switching = false; 

    void Start()
    {
        videoPlayer.clip = clip_Main;
        videoPlayer.Play();

        // Wait for InputHandler to be available
        /* StartCoroutine(WaitForInputHandler()); */
    }

    void Update()
    {
        if (InputHandler.Instance.UI_InteractInput == true && !switching)
        {
            switching = true;
            StartGame();
        }

        if (InputHandler.Instance.InteractInput == true && !switching)
        {
            switching = true;
            StartGame();
        }

        if (InputHandler.Instance.F_InteractInput == true && !switching)
        {
            switching = true;
            StartGame();
        }
    }

    /* private IEnumerator WaitForInputHandler()
    {
        while (InputHandler.Instance == null)
        {
            yield return null;
        }

        // Subscribe to interact input to load game
        InputHandler.Instance.OnInteractInput.AddListener(StartGame);
        InputHandler.Instance.OnFocus_InteractInput.AddListener(StartGame);
        InputHandler.Instance.OnUI_InteractInput.AddListener(StartGame);

    } */

    void OnDestroy()
    {
        /* if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInteractInput.RemoveListener(StartGame);
            InputHandler.Instance.OnFocus_InteractInput.RemoveListener(StartGame);
            InputHandler.Instance.OnUI_InteractInput.RemoveListener(StartGame);
        } */
    }

    private void StartGame()
    {
        CustomSceneManager.Instance.LoadScene(SceneName.Dev_1);
    }
}
