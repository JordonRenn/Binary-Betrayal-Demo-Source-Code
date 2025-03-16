using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using System.Collections;

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
    
    private enum MenuState { Main, Play, Settings, Credits, StartingGame }
    private MenuState currentState = MenuState.Main;
    private bool isTransitioning = false;

    void Start()
    {
        videoPlayer.clip = clip_Main;
        videoPlayer.Play();

        FPS_InputHandler.Instance.menu_MovePerformed.AddListener(OnMenuMove);
    }

    void OnDestroy()
    {
        FPS_InputHandler.Instance.menu_MovePerformed.RemoveListener(OnMenuMove);
    }

    private void OnMenuMove()
    {
        if (isTransitioning) return;
        
        if (currentState == MenuState.Main)
        {
            StartCoroutine(TransitionToState(MenuState.Play));


            ////////menu_ClickTriggered
            FPS_InputHandler.Instance.menu_ClickTriggered.AddListener(OnMenuSelction);
            FPS_InputHandler.Instance.menu_CancelTriggered.AddListener(OnMenuCancellation);

            return;
        }

        Vector2 input = FPS_InputHandler.Instance.Menu_MoveInput;

        // Simplified directional check
        if (Mathf.Abs(input.x) > 0.5f)
        {
            if (input.x > 0)
            {
                if (currentState == MenuState.Play) StartCoroutine(TransitionToState(MenuState.Settings));
                else if (currentState == MenuState.Credits) StartCoroutine(TransitionToState(MenuState.Play));
            }
            else
            {
                if (currentState == MenuState.Play) StartCoroutine(TransitionToState(MenuState.Credits));
                else if (currentState == MenuState.Settings) StartCoroutine(TransitionToState(MenuState.Play));
            }
        }
    }

    private void OnMenuSelction()
    {
        Debug.Log($"Menu Selction Made; Current state = {currentState}");

        switch (currentState)
        {
            case MenuState.Play:
                StartCoroutine(TransitionToState(MenuState.StartingGame));
                break;
            case MenuState.Settings:
                //
                break;
            case MenuState.Credits:
                //
                break;
            case MenuState.StartingGame:
                //too late bozo
                break;
        }
    }

    private void OnMenuCancellation()
    {
        switch (currentState)
        {
            case MenuState.Play:
                StartCoroutine(TransitionToState(MenuState.Main));
                break;
            case MenuState.Settings:
                StartCoroutine(TransitionToState(MenuState.Play));
                break;
            case MenuState.Credits:
                StartCoroutine(TransitionToState(MenuState.Play));
                break;
            case MenuState.StartingGame:
                //too late bozo
                break;
            default: 
                break;
        }
    }

    private IEnumerator TransitionToState(MenuState newState)
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
            case MenuState.Main:
                videoPlayer.clip = clip_Main;
                break;
            case MenuState.Play:
                videoPlayer.clip = clip_Play;
                break;
            case MenuState.Settings:
                videoPlayer.clip = clip_Settings;
                break;
            case MenuState.Credits:
                videoPlayer.clip = clip_Credits;
                break;
            case MenuState.StartingGame:
                videoPlayer.clip = clip_StaticLong;
                yield return new WaitForSeconds((float)clip_StaticLong.length * 0.5f);
                CustomSceneManager.Instance.LoadScene(SceneName.Dev_1);
                break;
        }
        videoPlayer.Play();
        
        currentState = newState;
        isTransitioning = false;
    }
}
