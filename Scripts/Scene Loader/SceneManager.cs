using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CustomSceneManager : MonoBehaviour
{
    private static CustomSceneManager _instance;
    public static CustomSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(CustomSceneManager)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Editor-time validation
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning($"{nameof(CustomSceneManager)}: CanvasGroup reference is required for scene transitions!");
        }
        if (fadeImgObj == null)
        {
            Debug.LogWarning($"{nameof(CustomSceneManager)}: Fade image GameObject reference is required!");
        }
        
        // Validate scene name constants are set in build settings
        ValidateSceneNames();
    }

    private void ValidateSceneNames()
    {
        ValidateSceneName(scene_MainMenu, "Main Menu");
        ValidateSceneName(scene_C01_01, "Chapter 1-1");
        ValidateSceneName(scene_C01_02, "Chapter 1-2");
        ValidateSceneName(scene_C01_03, "Chapter 1-3");
        ValidateSceneName(scene_C01_04, "Chapter 1-4");
        ValidateSceneName(scene_EndCredits, "End Credits");
        ValidateSceneName(scene_Dev_1, "Development Scene");
    }

    private void ValidateSceneName(string sceneName, string description)
    {
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains(sceneName))
            {
                sceneExists = true;
                break;
            }
        }
        if (!sceneExists)
        {
            Debug.LogWarning($"{nameof(CustomSceneManager)}: Scene '{sceneName}' ({description}) is not added to build settings!");
        }
    }
#endif

    private void ValidateRequiredComponents()
    {
        loader = GetComponent<ContentLoader>();
        if (loader == null)
            Debug.LogError($"{nameof(CustomSceneManager)}: Required ContentLoader component is missing!");
        if (fadeCanvasGroup == null)
            Debug.LogError($"{nameof(CustomSceneManager)}: Required CanvasGroup for fading is missing!");
        if (fadeImgObj == null)
            Debug.LogError($"{nameof(CustomSceneManager)}: Required fade image GameObject is missing!");

        ValidateSceneNames();
    }

    private void Awake()
    {
        // Initialize as singleton and persist across scenes since scene management is global
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            ValidateRequiredComponents();
        }
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
}
