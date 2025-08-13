using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                Debug.LogError($"Attempting to access {nameof(ContentLoader)} before it is initialized.");
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
            Debug.LogError($"{nameof(ContentLoader)}: Input Handler prefab is missing!");
        if (!GlobalVolume)
            Debug.LogError($"{nameof(ContentLoader)}: Global Volume prefab is missing!");
        if (!UI_Master)
            Debug.LogError($"{nameof(ContentLoader)}: UI Master prefab is missing!");
        if (!NotificationSystem)
            Debug.LogError($"{nameof(ContentLoader)}: Notification System prefab is missing!");
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
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_01:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_02:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_03:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.C01_04:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
                //StartCoroutine(LOAD_SAMPLESCENE());
                break;
            case SceneName.EndCredits:
                Debug.Log($"CONTENT LOADER | Begin Loading Content for Scene: {sceneName.ToString()}");
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

        //load other singleton classes
        Instantiate(UI_Master);
        Debug.Log($"CONTENT LOADER | UI_Master Instantiated");

        Instantiate(NotificationSystem);
        Debug.Log($"CONTENT LOADER | NotificationSystem Instantiated");

        /* Instantiate(PauseMenu);
        Debug.Log($"CONTENT LOADER | PauseMenu Instantiated"); */
    }

    private IEnumerator LOAD_SAMPLESCENE()
    {
        yield return StartCoroutine(LOAD_FRESHGAME());
    }
}
