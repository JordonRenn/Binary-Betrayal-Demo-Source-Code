using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(SceneManager))]

public class ContentLoader : MonoBehaviour
{
    private static ContentLoader _instance;
    public static ContentLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                SBGDebug.LogError($"Attempting to access {nameof(ContentLoader)} before it is initialized.", "ContentLoader");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    [SerializeField] private GameObject inputHandler;

    [Header("Singleton Managers")]

    [SerializeField] private GameObject GlobalVolume;
    [SerializeField] private GameObject UI_Master;
    [SerializeField] private GameObject NotificationSystem;
    //[SerializeField] private GameObject PauseMenu;

    private bool freshGame = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateRequiredComponents();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    private void ValidateRequiredComponents()
    {
        if (!inputHandler)
            SBGDebug.LogError($"{nameof(ContentLoader)}: Input Handler prefab is missing!", "ContentLoader | ValidateRequiredComponents");
        if (!GlobalVolume)
            SBGDebug.LogError($"{nameof(ContentLoader)}: Global Volume prefab is missing!", "ContentLoader | ValidateRequiredComponents");
        if (!UI_Master)
            SBGDebug.LogError($"{nameof(ContentLoader)}: UI Master prefab is missing!", "ContentLoader | ValidateRequiredComponents");
        if (!NotificationSystem)
            SBGDebug.LogError($"{nameof(ContentLoader)}: Notification System prefab is missing!", "ContentLoader | ValidateRequiredComponents");
        /* if (!PauseMenu)
            Debug.LogError($"{nameof(ContentLoader)}: Pause Menu prefab is missing!"); */
    }

    private void Awake()
    {
        // Initialize as singleton and persist across scenes since content loading is global
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            ValidateRequiredComponents();
        }
    }

    void Start()
    {
        /* if (inputHandler && InputHandler.Instance == null)
        {
            Instantiate(inputHandler);
            // Debug.Log($"CONTENT LOADER | InputHandler Instantiated");
        } */
        
        if (GlobalVolume)
        {
            Instantiate(GlobalVolume);
        }
    }

    public void LoadScene(SceneName sceneName)
    {
        Debug.Log("CONTENT LOADER | Beginning scene content loading");
        
        switch (sceneName)
        {
            case SceneName.MainMenu:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_01:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_02:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_03:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_04:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.EndCredits:
                // Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.Dev_1:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                StartCoroutine(LOAD_SAMPLESCENE());
                break;
        }
    }

    private IEnumerator LOAD_FRESHGAME()
    {
        if (!freshGame)
        {
            yield break;
        }

        freshGame = false;

        // Load other singleton classes
        Instantiate(UI_Master);
        Debug.Log($"CONTENT LOADER | UI_Master Instantiated");

        Instantiate(NotificationSystem);
        Debug.Log($"CONTENT LOADER | NotificationSystem Instantiated");

        SceneManager.LoadScene("ItemViewer", new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D));
    }

    private IEnumerator LOAD_SAMPLESCENE()
    {
        yield return StartCoroutine(LOAD_FRESHGAME());
    }
}
