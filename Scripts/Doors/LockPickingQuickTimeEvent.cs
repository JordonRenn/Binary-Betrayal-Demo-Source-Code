using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using DG.Tweening;

// previous declarations:
// public class DoorLock1 : Interactable
//public class LockPickingQuickTimeEvent : SauceObject
public class LockPickingQuickTimeEvent : MonoBehaviour // : SauceObject
{
    [SerializeField] GameObject doorParentObject;
    private Door door;
    //[SerializeField] bool keyRequired = false;
    [SerializeField] int lockKeyId;
    [SerializeField] bool isPickable;
    [SerializeField] CanvasGroup canvasGroup;

    //[Header("Tracking")]
    //[Space(10)]
    //[SerializeField] Trackable tracker;

    [Header("Lock Picking")]
    [Space(10)]

    [SerializeField] Image img_Indicator;
    [SerializeField] Image img_SuccessArea;
    [SerializeField] Image img_Cylinder; //don't use yet
    [SerializeField] Image img_Pick;
    [SerializeField] float pickWiggleSpeed = 2f;
    private Tweener pickWiggleTweener;

    private float successAreaAngle = 40f; // Angle in degrees for the success area
    private float successAreaStartAngle; // Starting angle for the success area
    private float successAreaMinAngle; // Minimum angle for the success area
    private float successAreaMaxAngle; // Maximum angle for the success area

    [SerializeField] LockDifficulty difficulty = LockDifficulty.Easy;
    [SerializeField] int pinCount = 3;

    //QTE speed 
    [SerializeField] float LP_Speed_Base = 250f;
    [SerializeField] float QTE2_SpeedMultiplier = 1.5f;
    private float QTE_Speed;
    private Tweener clockwiseTweener;
    private Tweener counterClockwiseTweener;
    //private Tweener indicatorTweener;

    [Header("FMOD")]
    [Space(10)]

    [SerializeField] private Transform audioPosition;
    [SerializeField] private EventReference sfx_QTE_1;
    [SerializeField] private EventReference sfx_QTE_2;
    [SerializeField] private EventReference sfx_QTE_Fail;

    //dev options
    bool initialized = false;
    int unlockedPins;
    private bool isInQTE1Phase = false;
    private bool isInQTE2Phase = false;
    private bool isPicking = false;
    InputState prevInputState;
    private float totalRotation = 0f; // Add this field near other private fields
    //private float currentAngle = 0f;  // Add this near other private fields
    private float continuousAngle = 0f;  // Add this field - never gets reset

    #region Init
    void Awake()
    {
        door = doorParentObject.GetComponent<Door>(); //dont delete
    }

    void Start()
    {
        SetQTESpeedValues(); // Initialize QTE_Speed
        SetSuccessArea(difficulty);
        initialized = true;
    }

    void SetQTESpeedValues()
    {
        QTE_Speed = LP_Speed_Base;
    }

    void SetSuccessArea(LockDifficulty difficulty)
    {
        // Set the angle for the success area based on difficulty
        switch (difficulty)
        {
            case LockDifficulty.Easy:
                successAreaAngle = Random.Range(60f, 80f);
                break;
            case LockDifficulty.Moderate:
                successAreaAngle = Random.Range(40f, 60f);
                break;
            case LockDifficulty.Hard:
                successAreaAngle = Random.Range(25f, 35f);
                break;
            default:
                successAreaAngle = 30f;
                break;
        }

        // Set a random starting angle for the success area
        successAreaStartAngle = Random.Range(0f, 360f);
        successAreaMinAngle = successAreaStartAngle;
        successAreaMaxAngle = successAreaStartAngle + successAreaAngle;

        Debug.Log($"New Success Area - Start: {successAreaStartAngle}, Angle: {successAreaAngle}, End: {successAreaMaxAngle}");

        // Ensure the success area image has a material with the custom radial mask shader
        Material material = img_SuccessArea.material;
        if (material == null || material.shader.name != "UI/RadialMask")
        {
            material = new Material(Shader.Find("UI/RadialMask"));
            img_SuccessArea.material = material;
        }

        // Set the angle and starting point for the success area
        material.SetFloat("_Angle", successAreaAngle);
        img_SuccessArea.rectTransform.localRotation = Quaternion.Euler(0, 0, successAreaStartAngle);

        // Only rotate success area, leave indicator at 0
        img_SuccessArea.rectTransform.localRotation = Quaternion.Euler(0, 0, successAreaStartAngle);
        //img_Indicator.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    #endregion

    //PUBLIC METHODS

    public void Interact()
    {
        if (door.doorLockState != DoorLockState.Unlocked && isPickable)
        {
            prevInputState = FPS_InputHandler.Instance.currentState;
            FPS_InputHandler.Instance.SetInputState(InputState.LockedInteraction);

            StartCoroutine(OpenLockGUI());
        }
        else if (door.doorLockState == DoorLockState.Unlocked)
        {
            door.Interact();
        }
    }

    void UnlockDoor()
    {
        Debug.Log("Door Unlocked");
        door.LockDoor(false);
        door.OpenDoor();
    }

    /* private void UpdateIconColor()
    {
        if (tracker != null)
        {
            tracker = NavCompass.Instance.Find(tracker, this);
        }
    } */

    // DO NOT EDIT THIS REGION UNLESS YOU KNOW WHAT YOU ARE DOING and IT'S NECESSARY
    // IT'S HELD TOGETHR BY HOPES AND DREAMS
    #region Lock Pick GUI
    private IEnumerator OpenLockGUI()
    {
        VolumeManager.Instance.SetVolume(VolumeType.LockPick);
        if (UI_Master.Instance != null) { UI_Master.Instance.HideAllHUD(); }
        if (FPSS_Pool.Instance != null) { FPSS_Pool.Instance.currentActiveWPO.SetCurrentWeaponActive(false); }

        isPicking = true;

        SetSuccessArea(difficulty);
        ResetIndicators();
        StartPickWiggle();

        canvasGroup.gameObject.SetActive(true);

        float elapsedTime = 0;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        FPS_InputHandler.Instance.lint_InteractTriggered.AddListener(StartInitialQTE1);
        FPS_InputHandler.Instance.lint_CancelTriggered.AddListener(ResetLockPicking);
    }

    private void StartPickWiggle()
    {
        if (pickWiggleTweener != null)
        {
            pickWiggleTweener.Kill();
        }

        pickWiggleTweener = img_Pick.rectTransform
            .DORotate(new Vector3(0, 0, 10f), 1f / pickWiggleSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartInitialQTE1()
    {
        // Remove initial listener
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(StartInitialQTE1);
        QTE_1();
    }

    void ResetIndicators()
    {
        img_Indicator.rectTransform.anchoredPosition = Vector2.zero;
        img_Indicator.rectTransform.localRotation = Quaternion.identity;
        totalRotation = 0f;
    }

    void CleanupEventListeners()
    {
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(StartInitialQTE1);
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_2_Check);
        FPS_InputHandler.Instance.lint_InteractReleased.RemoveListener(QTE_1_Check);
    }

    void ResetLockPicking()
    {
        isInQTE1Phase = false;
        isInQTE2Phase = false;
        unlockedPins = 0;

        CleanupEventListeners();
        StartCoroutine(CloseLockGUI());

        if (pickWiggleTweener != null)
        {
            pickWiggleTweener.Kill();
            pickWiggleTweener = null;
        }

        if (clockwiseTweener != null)
        {
            clockwiseTweener.Kill();
            clockwiseTweener = null;
        }

        if (counterClockwiseTweener != null)
        {
            counterClockwiseTweener.Kill();
            counterClockwiseTweener = null;
        }
    }

    private IEnumerator CloseLockGUI() //do not call directly
    {
        isPicking = false;

        float elapsedTime = 0;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;

        canvasGroup.gameObject.SetActive(false);

        VolumeManager.Instance.SetVolume(VolumeType.Default);

        if (FPSS_Pool.Instance != null) { FPSS_Pool.Instance.currentActiveWPO.SetCurrentWeaponActive(true); }
        if (UI_Master.Instance != null) { UI_Master.Instance.ShowAllHUD(); }
        FPS_InputHandler.Instance.SetInputState(prevInputState);
    }

    #region QTEs
    private void Update()
    {
        if (!isPicking) { return; }

        if (pickWiggleTweener != null)
        {
            pickWiggleTweener.timeScale = isInQTE2Phase ? QTE2_SpeedMultiplier : 1f;
        }

        if (isInQTE1Phase && (clockwiseTweener == null || !clockwiseTweener.IsPlaying()))
        {
            if (counterClockwiseTweener != null)
            {
                counterClockwiseTweener.Kill();
                counterClockwiseTweener = null;
            }

            float startAngle = continuousAngle;
            float rotationDuration = 1f / QTE_Speed;
            clockwiseTweener = DOTween.To(() => continuousAngle, x =>
            {
                continuousAngle = x;
                img_Indicator.rectTransform.localRotation = Quaternion.Euler(0, 0, x % 360f);
            }, startAngle + 360f, rotationDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
        else if (isInQTE2Phase && (counterClockwiseTweener == null || !counterClockwiseTweener.IsPlaying()))
        {
            if (clockwiseTweener != null)
            {
                clockwiseTweener.Kill();
                clockwiseTweener = null;
            }

            float startAngle = continuousAngle;
            float rotationDuration = (1f / QTE_Speed) / QTE2_SpeedMultiplier;
            counterClockwiseTweener = DOTween.To(() => continuousAngle, x =>
            {
                continuousAngle = x;
                img_Indicator.rectTransform.localRotation = Quaternion.Euler(0, 0, x % 360f);
            }, startAngle - 360f, rotationDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        //Debug.Log($"FINAL continuousAngle: Frame {Time.frameCount}, {continuousAngle}");
    }

    void QTE_1()
    {
        isInQTE1Phase = true;
        isInQTE2Phase = false;

        FPS_InputHandler.Instance.lint_InteractReleased.AddListener(QTE_1_Check);
    }

    void QTE_1_Check()
    {
        isInQTE1Phase = false;
        FPS_InputHandler.Instance.lint_InteractReleased.RemoveListener(QTE_1_Check);

        float currentRotation = continuousAngle % 360f;
        if (currentRotation < 0) currentRotation += 360f;
        bool isSuccess = IsWithinSuccessArea(currentRotation);

        if (!isSuccess)
        {
            PlaySFX(sfx_QTE_Fail);
            ResetLockPicking();
            return;
        }

        PlaySFX(sfx_QTE_1);
        QTE_2();
    }

    void QTE_2()
    {
        isInQTE2Phase = true;
        FPS_InputHandler.Instance.lint_InteractTriggered.AddListener(QTE_2_Check);
    }

    void QTE_2_Check()
    {
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_2_Check);

        float currentRotation;
        currentRotation = continuousAngle % 360f;
        if (currentRotation < 0) currentRotation += 360f;
        bool isSuccess = IsWithinSuccessArea(currentRotation);

        if (!isSuccess)
        {
            PlaySFX(sfx_QTE_Fail);
            ResetLockPicking();
            return;
        }

        unlockedPins++;
        PlaySFX(sfx_QTE_2);

        if (unlockedPins >= pinCount)
        {
            UnlockDoor();
            ResetLockPicking();
            return;
        }

        isInQTE2Phase = false;
        ResetAndReinitialize();  // Add this line

        QTE_1();
    }

    private void ResetAndReinitialize()
    {
        if (clockwiseTweener != null)
        {
            clockwiseTweener.Kill();
            clockwiseTweener = null;
        }
        if (counterClockwiseTweener != null)
        {
            counterClockwiseTweener.Kill();
            counterClockwiseTweener = null;
        }

        img_Indicator.rectTransform.localRotation = Quaternion.identity;
        img_SuccessArea.rectTransform.localRotation = Quaternion.identity;
        totalRotation = 0f;

        SetSuccessArea(difficulty);
        ResetIndicators();
    }
    #endregion

    #region Success Check
    bool IsWithinSuccessArea(float indicatorAngle)
    {
        float normalizedIndicator = NormalizeAngle(indicatorAngle);
        float normalizedStart = NormalizeAngle(successAreaStartAngle);
        float normalizedEnd = NormalizeAngle(successAreaMaxAngle);

        if (normalizedStart > normalizedEnd)
        {
            bool success = normalizedIndicator >= normalizedStart || normalizedIndicator <= normalizedEnd;
            return success;
        }
        else
        {
            bool success = normalizedIndicator >= normalizedStart && normalizedIndicator <= normalizedEnd;
            return success;
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0)
            angle += 360f;
        return angle;
    }
    #endregion

    void PlaySFX(EventReference eventRef)
    {
        RuntimeManager.PlayOneShot(eventRef, audioPosition.position);
    }
    #endregion

    public bool IsPickable()
    {
        return isPickable;
    }
}
