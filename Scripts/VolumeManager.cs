using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }

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
        Instance = this;
    }

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

    public void LerpWeight(float targetWeight, float duration)
    {
        DOTween.To(() => volume.weight, x => volume.weight = x, targetWeight, duration);
    }
}
