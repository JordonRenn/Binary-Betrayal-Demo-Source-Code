using UnityEngine;

public class UI_Master : MonoBehaviour
{
    private static UI_Master _instance;
    public static UI_Master Instance 
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(UI_Master)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    [Header("HUD Elements")]
    [Space(10)]

    [SerializeField] private GameObject[] HUD_Status_Elements;
    [SerializeField] private GameObject[] HUD_Weapon_Elements;
    [SerializeField] private GameObject[] HUD_Utility_Elements;
    [SerializeField] private GameObject crosshair_Element;
    [SerializeField] private GameObject DialogueBox;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance) == this)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        // UI initialization code
    }

    public void HideAllHUD()
    {
        FPSS_WeaponHUD.Instance.Hide(true);
        FPSS_ReticleSystem.Instance.Hide(true);

        foreach (GameObject element in HUD_Status_Elements)
        {
            element.SetActive(false);
        }
    }

    public void ShowAllHUD()
    {
        FPSS_WeaponHUD.Instance.Hide(false);
        FPSS_ReticleSystem.Instance.Hide(false);

        foreach (GameObject element in HUD_Status_Elements)
        {
            element.SetActive(true);
        }
    }
}
