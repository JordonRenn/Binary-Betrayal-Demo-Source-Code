using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneManager))]

public class ContentLoader : MonoBehaviour
{
    public static ContentLoader Instance { get; private set; }
    
    [SerializeField] private GameObject inputHandler;

    [Header("Singleton Managers")]

    [SerializeField] private GameObject EventSystem;
    [SerializeField] private GameObject GlobalVolume;
    [SerializeField] private GameObject UI_Master;
    [SerializeField] private GameObject NotificationSystem;
    [SerializeField] private GameObject PauseMenu;

    private bool freshGame = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Instantiate(EventSystem);
        Instantiate(inputHandler);
        Instantiate(GlobalVolume);
    }

    public void LoadScene(SceneName sceneName)
    {
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

        Instantiate(PauseMenu);
        Debug.Log($"CONTENT LOADER | PauseMenu Instantiated");
    }

    private IEnumerator LOAD_SAMPLESCENE()
    {
        yield return StartCoroutine(LOAD_FRESHGAME());
    }
}
