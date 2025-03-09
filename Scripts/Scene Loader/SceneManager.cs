using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager Instance { get; private set; }
    private ContentLoader loader;

    [Header("Settings")]
    [Space(10)]

    [SerializeField] float playerLoadDelay = 0.25f;
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] GameObject fadeImgObj;
    [SerializeField] CanvasGroup fadeCanvasGroup;

    [Header("Scene Names")]
    [Space(10)]
    
    [SerializeField] private const string scene_MainMenu = "MainMenu";
    [SerializeField] private const string scene_C01_01 = "C01_01";
    [SerializeField] private const string scene_C01_02 = "C01_02";
    [SerializeField] private const string scene_C01_03 = "C01_03";
    [SerializeField] private const string scene_C01_04 = "C01_04";
    [SerializeField] private const string scene_EndCredits = "_EndCredits";
    [SerializeField] private const string scene_Dev_1 = "SampleScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        loader = GetComponent<ContentLoader>();
    }

    public void LoadScene(SceneName sceneName)
    {
        switch (sceneName)
        {
            case SceneName.MainMenu:
                SceneManager.LoadScene(scene_MainMenu);
                break;
            case SceneName.C01_01:
                SceneManager.LoadScene(scene_C01_01);
                break;
            case SceneName.C01_02:
                SceneManager.LoadScene(scene_C01_02);
                break;
            case SceneName.C01_03:
                Debug.Log("CUSTOM SCENE MANAGER | Attempting Scene activation: {scene_C01_03}");
                
                fadeCanvasGroup.DOFade(1f, fadeDuration)
                    .OnComplete(() => {
                        Debug.Log("CUSTOM SCENE MANAGER | Scene fade to black complete");
                        SceneManager.LoadScene(scene_C01_03);
                        Debug.Log("CUSTOM SCENE MANAGER | Scene loaded");
                        StartCoroutine(ACTIVATESCENE_DEV1(scene_C01_03));
                        Debug.Log("CUSTOM SCENE MANAGER | Scene Activation Coroutine Started");
                        DOVirtual.DelayedCall(2f, () => {fadeCanvasGroup.DOFade(0f, fadeDuration)
                            .OnComplete(() => {
                                Debug.Log("CUSTOM SCENE MANAGER | Scene fade from black complete");
                                fadeImgObj.SetActive(false);
                                });
                        });
                    });
                break;
            case SceneName.C01_04:
                SceneManager.LoadScene(scene_C01_04);
                break;
            case SceneName.EndCredits:
                SceneManager.LoadScene(scene_EndCredits);
                break;
            case SceneName.Dev_1:
                //FadeScreen();
                Debug.Log("CUSTOM SCENE MANAGER | Attempting Scene activation: {scene_Dev_1}");

                fadeCanvasGroup.DOFade(1f, fadeDuration)
                    .OnComplete(() => {
                        Debug.Log("CUSTOM SCENE MANAGER | Scene fade to black complete");
                        SceneManager.LoadScene(scene_Dev_1);
                        Debug.Log("CUSTOM SCENE MANAGER | Scene loaded");
                        StartCoroutine(ACTIVATESCENE_DEV1(scene_Dev_1));
                        Debug.Log("CUSTOM SCENE MANAGER | Scene Activation Coroutine Started");
                        DOVirtual.DelayedCall(2f, () => {fadeCanvasGroup.DOFade(0f, fadeDuration)
                            .OnComplete(() => {
                                Debug.Log("CUSTOM SCENE MANAGER | Scene fade from black complete");
                                fadeImgObj.SetActive(false);
                                });
                        });
                    });
                break;
            default:
                Debug.LogError("Scene not found");
                SceneManager.LoadScene(scene_MainMenu);
                break;
        }
    }

    private IEnumerator ACTIVATESCENE_MAINMENU(string sceneName)
    {
        Debug.Log("CUSTOM SCENE MANAGER | Scene Activation started");
        yield return new WaitForSeconds(0.05f);
        
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene found be name: {sceneName}");

        SceneManager.SetActiveScene(scene);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene activated: {sceneName}");

        ContentLoader.Instance.LoadScene(SceneName.MainMenu);

        FPS_InputHandler.Instance.SetInputState(InputState.MenuNavigation);
    }

    private IEnumerator ACTIVATESCENE_DEV1(string sceneName)
    {
        Debug.Log("CUSTOM SCENE MANAGER | Scene Activation started");
        yield return new WaitForSeconds(0.25f);
        
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene found be name: {sceneName}");

        SceneManager.SetActiveScene(scene);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene activated: {sceneName}");


        ContentLoader.Instance.LoadScene(SceneName.Dev_1);
        Debug.Log($"CUSTOM SCENE MANAGER | Calling content loader to load content for scene: {sceneName}");

        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        Debug.Log("CUSTOM SCENE MANAGER | Calling ''PlayerSpawner'' to spawn the Player");
        PlayerSpawner.Instance.SpawnPlayer();
    }

    private IEnumerator ACTIVATESCENE_C01_03(string sceneName)
    {
        Debug.Log("CUSTOM SCENE MANAGER | Scene Activation started");
        yield return new WaitForSeconds(0.25f);
        
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene found be name: {sceneName}");

        SceneManager.SetActiveScene(scene);
        Debug.Log($"CUSTOM SCENE MANAGER | Scene activated: {sceneName}");

        ContentLoader.Instance.LoadScene(SceneName.C01_03);
        Debug.Log($"CUSTOM SCENE MANAGER | Calling content loader to load content for scene: {sceneName}");

        FPS_InputHandler.Instance.SetInputState(InputState.FirstPerson);
        Debug.Log("CUSTOM SCENE MANAGER | Calling ''PlayerSpawner'' to spawn the Player");
        PlayerSpawner.Instance.SpawnPlayer();
    }

    private void FadeScreen()
    {
        //
        
        // Fade in
        fadeCanvasGroup.DOFade(1f, fadeDuration)
            .OnComplete(() => {
                // Wait for 2 seconds then fade out
                DOVirtual.DelayedCall(2f, () => {
                    fadeCanvasGroup.DOFade(0f, fadeDuration)
                        .OnComplete(() => {
                            fadeImgObj.SetActive(false);
                        });
                });
            });
    }
}
