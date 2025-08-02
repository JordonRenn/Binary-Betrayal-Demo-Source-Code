using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class VolumeManager : MonoBehaviour
{
    private static VolumeManager _instance;
    public static VolumeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(VolumeManager)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [SerializeField] private Volume volume;

    [Header("Pause Menu Volume")]
    [Space(10)]
    
    [SerializeField] private VolumeProfile profilePauseMenu;
    [SerializeField] private float pauseMenuWeight;
    [SerializeField] private float pauseMenuTransitionTime;

    [Header("Cutscene Volume")]
    [Space(10)]

    [SerializeField] private VolumeProfile profileCutscene;

    [SerializeField] private float cutsceneWeight;
    [SerializeField] private float cutsceneTransitionTime;

    [Header("LockPick Volume")]
    [Space(10)]

    [SerializeField] private VolumeProfile profileLockPick;
    [SerializeField] private float lockPickWeight;
    [SerializeField] private float lockPickTransitionTime;

    [Header("Default Volume")]
    [Space(10)]

    [SerializeField] private float defaultTransitionTime;

    void Awake()
    {
        // Initialize as singleton, don't persist across scenes since post-processing is scene-specific
        if (this.InitializeSingleton(ref _instance) == this)
        {
            // Validate required components
            if (volume == null)
            {
                Debug.LogError($"{nameof(VolumeManager)}: Required Volume component is missing!");
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Editor-time validation
        if (volume == null)
        {
            Debug.LogWarning($"{nameof(VolumeManager)}: Volume reference is required!");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    public void SetVolume(VolumeType volumeType)
    {
        switch (volumeType)
        {
            case VolumeType.PauseMenu:
                volume.profile = profilePauseMenu;
                LerpWeight(pauseMenuWeight, pauseMenuTransitionTime);
                break;
            case VolumeType.Cutscene:
                volume.profile = profileCutscene;
                LerpWeight(cutsceneWeight, cutsceneTransitionTime);
                break;
            case VolumeType.LockPick:
                volume.profile = profileLockPick;
                LerpWeight(lockPickWeight, lockPickTransitionTime);
                break;
            case VolumeType.Default:
                LerpWeight(0, defaultTransitionTime);
                break;
        }
    }

    private Tweener currentTween;

    public void LerpWeight(float targetWeight, float duration)
    {
        if (volume == null) return;

        // Kill any existing transition
        currentTween?.Kill();
        
        // Create new transition
        currentTween = DOTween.To(() => volume.weight, x => volume.weight = x, targetWeight, duration)
            .SetUpdate(true) // Important: run even when timeScale = 0
            .OnComplete(() => currentTween = null);
    }

    private void OnDestroy()
    {
        // Clean up any running tweens
        currentTween?.Kill();
    }
}
