using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private float pauseCooldown = 0.5f;
    [SerializeField] private GameObject pauseMenuCanvas;
    
    private InputState previousState;
    private bool isPaused = false;

    void Start()
    {
        FPS_InputHandler.Instance.pauseMenuButtonTriggered.AddListener(Pause);
        previousState = FPS_InputHandler.Instance.currentState;
    }

    private void Pause()
    {
        PauseGame(true);
    }
    
    public void PauseGame(bool showMenu = true) //arguement allows for pausing without showing the menu
    {
        if (!isPaused)
        {
            FPS_InputHandler.Instance.pauseMenuButtonTriggered.RemoveListener(Pause);
            
            previousState = FPS_InputHandler.Instance.currentState;

            FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
            
            VolumeManager.Instance.SetVolume(VolumeType.PauseMenu);
            UI_Master.Instance.HideAllHUD();

            FPS_InputHandler.Instance.pauseMenuButtonTriggered.AddListener(ResumeGame);

            StartCoroutine(TimeStopWaitTime(0.15f, showMenu));
        }
    }

    private IEnumerator TimeStopWaitTime(float time, bool showMenu)
    {
        yield return new WaitForSecondsRealtime(time);

        Time.timeScale = 0;

        isPaused = true;

        if (showMenu)
        {
            ShowPauseMenu();
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            FPS_InputHandler.Instance.pauseMenuButtonTriggered.RemoveListener(ResumeGame);
            
            
            Time.timeScale = 1;

            ShowPauseMenu(false);
            
            FPS_InputHandler.Instance.SetInputState(previousState);

            VolumeManager.Instance.SetVolume(VolumeType.Default);
            UI_Master.Instance.ShowAllHUD();

            FPS_InputHandler.Instance.pauseMenuButtonTriggered.AddListener(Pause);

            StartCoroutine(PauseCooldown());
        }
    }

    private IEnumerator PauseCooldown()
    {
        yield return new WaitForSecondsRealtime(pauseCooldown);
        isPaused = false;
    }

    //MENU LOGIC

    private void ShowPauseMenu(bool showMenu = true)
    {
        //Show pause menu
        if (showMenu)
        {
            pauseMenuCanvas.SetActive(true);
        }
        else
        {
            pauseMenuCanvas.SetActive(false);
        }
    }
}
