using UnityEngine;
using UnityEngine.UIElements;

/* 
PLACEHOLDER FOR FUTURE REFERENCE:
 */

public enum ReticleState
{   
    Disabled,
    Weapon,
    Interact,
    Reload,
    Stealth
}

#region Reticle System
public class ReticleSystem : MonoBehaviour
{
    private static ReticleSystem _instance;
    public static ReticleSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning($"Attempting to access ReticleSystem before initialization.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [SerializeField] private UIDocument reticleUIDocument;

    public Label HoverInfoText => _hoverInfoText;
    private Label _hoverInfoText;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this); // just destroy the class instance, not the whole game object --  safety first
            return;
        }
        _instance = this;

        reticleUIDocument = GetComponent<UIDocument>();

        _hoverInfoText = reticleUIDocument.rootVisualElement.Q<Label>("HoverInfoText");
    }

    #region Public API
    public void Impulse(float duration, float intensity)
    {
        // Implement reticle impulse effect here
        Debug.Log($"Reticle Impulse: Duration={duration}, Intensity={intensity}");
    }
    #endregion

    #region Private API
    private void SetReticleState(ReticleState state)
    {
        // Implement reticle state change logic here
        Debug.Log($"Reticle State Changed to: {state}");
    }
    #endregion
}
#endregion
