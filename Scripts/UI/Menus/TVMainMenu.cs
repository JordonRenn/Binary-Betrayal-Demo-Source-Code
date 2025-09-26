using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.InputSystem.Utilities;

public class TVMainMenu : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    [Header("Video Clips")]
    [Space(10)]

    [SerializeField] VideoClip clip_Main;
    [SerializeField] VideoClip clip_Play;
    [SerializeField] VideoClip clip_Settings;
    [SerializeField] VideoClip clip_Credits;
    [SerializeField] VideoClip clip_StaticLong;
    [SerializeField] VideoClip[] clip_StaticShort;
    
    

    private MainMenuState currentState = MainMenuState.Main;
    private bool isTransitioning = false;

    //private UnityEvent anyInputEvent = new UnityEvent();

    void Start()
    {
        videoPlayer.clip = clip_Main;
        videoPlayer.Play();

        // Wait for InputHandler to be available before subscribing to events
        StartCoroutine(WaitForInputHandler());
        InputSystem.onAnyButtonPress.CallOnce(ctrl => EnterMenu());
    }

    private IEnumerator WaitForInputHandler()
    {
        // Wait until InputHandler is available
        while (InputHandler.Instance == null)
        {
            // SBGDebug.LogInfo("Waiting for InputHandler to be available...", "TVMainMenu | WaitForInputHandler");
            yield return null;
        }
        
        // SBGDebug.LogInfo("InputHandler is now available, proceeding with menu setup...", "TVMainMenu | WaitForInputHandler");
        // Now safely subscribe to events
        InputHandler.Instance.OnUI_NavigateInput.AddListener(OnMenuMove);
    }

    void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnUI_NavigateInput.RemoveListener(OnMenuMove);
        }
    }

    private void OnMenuMove(Vector2 input)
    {
        if (isTransitioning) return;

        // Simplified directional check
        if (Mathf.Abs(input.x) > 0.5f)
        {
            if (input.x > 0)
            {
                if (currentState == MainMenuState.Play) StartCoroutine(TransitionToState(MainMenuState.Settings));
                else if (currentState == MainMenuState.Credits) StartCoroutine(TransitionToState(MainMenuState.Play));
            }
            else
            {
                if (currentState == MainMenuState.Play) StartCoroutine(TransitionToState(MainMenuState.Credits));
                else if (currentState == MainMenuState.Settings) StartCoroutine(TransitionToState(MainMenuState.Play));
            }
        }
    }

    private void OnMenuSelction()
    {
        // Debug.Log($"Menu Selction Made; Current state = {currentState}");

        switch (currentState)
        {
            case MainMenuState.Play:
                // SBGDebug.LogInfo("Starting game...", "TVMainMenu | OnMenuSelection");
                StartCoroutine(TransitionToState(MainMenuState.StartingGame));
                break;
            case MainMenuState.Settings:
                // SBGDebug.LogInfo("Opening settings...", "TVMainMenu | OnMenuSelection");
                StartCoroutine(TransitionToState(MainMenuState.Settings));
                break;
            case MainMenuState.Credits:
                // SBGDebug.LogInfo("Opening credits...", "TVMainMenu | OnMenuSelection");
                StartCoroutine(TransitionToState(MainMenuState.Credits));
                break;
            case MainMenuState.StartingGame:
                //too late bozo
                break;
        }
    }

    private void OnMenuCancellation()
    {
        switch (currentState)
        {
            case MainMenuState.Play:
                StartCoroutine(TransitionToState(MainMenuState.Main));
                break;
            case MainMenuState.Settings:
                StartCoroutine(TransitionToState(MainMenuState.Play));
                break;
            case MainMenuState.Credits:
                StartCoroutine(TransitionToState(MainMenuState.Play));
                break;
            case MainMenuState.StartingGame:
                //too late bozo
                break;
            default: 
                break;
        }
    }

    private IEnumerator TransitionToState(MainMenuState newState)
    {
        // Debug.Log($"Transitioning menu to {newState}");
        
        isTransitioning = true;
        
        // Play random static
        VideoClip staticClip = clip_StaticShort[Random.Range(0, clip_StaticShort.Length)];
        videoPlayer.clip = staticClip;
        videoPlayer.Play();
        
        yield return new WaitForSeconds((float)staticClip.length);

        // Switch to new state video
        switch (newState)
        {
            case MainMenuState.Main:
                // SBGDebug.LogInfo("Transitioning to Main Menu...", "TVMainMenu | TransitionToState");
                videoPlayer.clip = clip_Main;
                InputSystem.onAnyButtonPress.CallOnce(ctrl => EnterMenu());
                break;
            case MainMenuState.Play:
                // SBGDebug.LogInfo("Transitioning to Play Menu...", "TVMainMenu | TransitionToState");
                videoPlayer.clip = clip_Play;
                break;
            case MainMenuState.Settings:
                // SBGDebug.LogInfo("Transitioning to Settings Menu...", "TVMainMenu | TransitionToState");
                videoPlayer.clip = clip_Settings;
                break;
            case MainMenuState.Credits:
                // SBGDebug.LogInfo("Transitioning to Credits Menu...", "TVMainMenu | TransitionToState");
                videoPlayer.clip = clip_Credits;
                break;
            case MainMenuState.StartingGame:
                // SBGDebug.LogInfo("Transitioning to Starting Game...", "TVMainMenu | TransitionToState");
                videoPlayer.clip = clip_StaticLong;
                yield return new WaitForSeconds((float)clip_StaticLong.length * 0.5f);
                CustomSceneManager.Instance.LoadScene(SceneName.Dev_1);
                break;
        }
        videoPlayer.Play();
        
        currentState = newState;
        isTransitioning = false;
    }

    void EnterMenu()
    {
        // SBGDebug.LogInfo("Entering Main Menu...", "TVMainMenu | EnterMenu");

        if (currentState == MainMenuState.Main)
        {

            StartCoroutine(TransitionToState(MainMenuState.Play));

            InputHandler.Instance?.OnUI_ClickInput?.AddListener(OnMenuSelction);
            InputHandler.Instance?.OnUI_InteractInput?.AddListener(OnMenuSelction);
            InputHandler.Instance?.OnUI_CancelInput?.AddListener(OnMenuCancellation);

            if (InputHandler.Instance == null)
            {
                SBGDebug.LogWarning("InputHandler instance is null", "TVMainMenu | EnterMenu");
            }

            if (InputHandler.Instance?.OnUI_CancelInput == null || InputHandler.Instance?.OnUI_InteractInput == null)
            {
                SBGDebug.LogWarning("One or more InputHandler actions are null... Cannot subscribe to events", "TVMainMenu | EnterMenu");
            }
        }
    }

    void OnDestroyMenu()
    {
        InputHandler.Instance?.OnUI_ClickInput?.RemoveListener(OnMenuSelction);
        InputHandler.Instance?.OnUI_CancelInput?.RemoveListener(OnMenuCancellation);

        if (InputHandler.Instance == null)
        {
            SBGDebug.LogWarning("InputHandler instance is null", "TVMainMenu | OnDestroyMenu");
        }

        if (InputHandler.Instance?.OnUI_CancelInput == null || InputHandler.Instance?.OnUI_InteractInput == null)
        {
            SBGDebug.LogWarning("One or more InputHandler actions are null... Cannot unsubscribe from events", "TVMainMenu | OnDestroyMenu");
        }
    }
}
