using UnityEngine;

public class UI_Master : MonoBehaviour
{
    public static UI_Master Instance { get; private set; }
    
    [Header("HUD Elements")]
    [Space(10)]

    [SerializeField] private GameObject[] HUD_Status_Elements;
    [SerializeField] private GameObject[] HUD_Weapon_Elements;
    [SerializeField] private GameObject[] HUD_Utility_Elements;
    [SerializeField] private GameObject crosshair_Element;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
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
