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

        FPS_InputHandler.Instance.menu_MovePerformed.AddListener(OnMenuMove);
        InputSystem.onAnyButtonPress.CallOnce(ctrl => EnterMenu());
    }

    void OnDestroy()
    {
        FPS_InputHandler.Instance.menu_MovePerformed.RemoveListener(OnMenuMove);
    }

    private void OnMenuMove()
    {
        if (isTransitioning) return;

        Vector2 input = FPS_InputHandler.Instance.Menu_MoveInput;

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
        Debug.Log($"Menu Selction Made; Current state = {currentState}");

        switch (currentState)
        {
            case MainMenuState.Play:
                StartCoroutine(TransitionToState(MainMenuState.StartingGame));
                break;
            case MainMenuState.Settings:
                //
                break;
            case MainMenuState.Credits:
                //
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
        Debug.Log($"Transitioning menu to {newState}");
        
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
                videoPlayer.clip = clip_Main;
                InputSystem.onEvent += (eventPtr, device) => EnterMenu();
                break;
            case MainMenuState.Play:
                videoPlayer.clip = clip_Play;
                break;
            case MainMenuState.Settings:
                videoPlayer.clip = clip_Settings;
                break;
            case MainMenuState.Credits:
                videoPlayer.clip = clip_Credits;
                break;
            case MainMenuState.StartingGame:
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
        if (currentState == MainMenuState.Main)
        {
            StartCoroutine(TransitionToState(MainMenuState.Play));

            ////////menu_ClickTriggered
            FPS_InputHandler.Instance.menu_ClickTriggered.AddListener(OnMenuSelction);
            FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(OnMenuCancellation);
        }
    }

    void OnDestroyMenu()
    {
        FPS_InputHandler.Instance.menu_ClickTriggered.RemoveListener(OnMenuSelction);
        FPS_InputHandler.Instance.menu_CancelTriggered.RemoveListener(OnMenuCancellation);
    }
}
