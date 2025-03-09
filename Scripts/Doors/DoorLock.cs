using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorLock : Interactable
{
    [SerializeField] GameObject doorParentObject;
    private Door door;
    [SerializeField] bool keyRequired;
    [SerializeField] int lockKeyId;
    [SerializeField] bool isPickable;
    [SerializeField] DoorLockState state;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Lock Picking")]
    [Space(10)]

    [SerializeField] LockDifficulty difficulty = LockDifficulty.Easy;
    [SerializeField] int pinCount = 3;

    [SerializeField] Image QTE_1_Indicator;
    [SerializeField] Image QTE_2_Indicator;
    float QTE_1_IndicatorValue;
    float QTE_2_IndicatorValue;

    //QTE Background
    [SerializeField] Image QTE_1_Background;
    [SerializeField] Image QTE_2_Background;
    float QTE_1_FullSize; //base input value for algorithm
    float QTE_2_FullSize; //base input value for algorithm

    //QTE target zone Image
    [SerializeField] Image QTE_1_TargetImage;
    [SerializeField] Image QTE_2_TargetImage;
    [SerializeField] float LP_TargetSize_BaseMultiplier = 0.175f; //base input value for algorithm
    float LP_TargetSize_ModerateMultiplier = 0.85f;
    float LP_TargetSize_HardMultiplier = 0.6f;
    private float QTE_1_TargetSize; //final value used
    private float QTE_2_TargetSize; //final value used

    //QTE Perfect target zone image
    [SerializeField] Image QTE_1_PerfectTargetImage;
    [SerializeField] float LP_PerfectPickTargetSize_Multiplier = 0.25f; //base input value for algorithm
    private float QTE_1_PerfectTargetSize; //final value used

    //QTE speed 
    [SerializeField] float LP_Speed_Base = 250f; //base input value for algorithm
    float LP_Speed_ModerateMultiplier = 1.6f;
    float LP_Speed_HardMultiplier = 2.2f;
    private float QTE_Speed; //final value used

    
    TargetFloatRange QTE_1_TargetRange;
    TargetFloatRange QTE_2_TargetRange;
    TargetFloatRange perfectTargetRange;

    //dev options
    bool initialized = false;
    int unlockedPins;
    private bool isInQTE1Phase = false;
    private bool isInQTE2Phase = false;
    InputState prevInputState;
    
    //QTE_1_

    #region Init
    void Awake()
    {
        door = doorParentObject.GetComponent<Door>(); //dont delete
    }

    void Start()
    {
        StartCoroutine(Init());
        state = door.doorLockState;  //dont delete
        
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(0.125f);
        GetValues();
        yield return new WaitForSeconds(0.125f);
        SetValues();
        yield return new WaitForSeconds(0.125f);
        FPS_InputHandler.Instance.menu_DevTriggered.AddListener(Interact); //replace in future
        yield return new WaitForSeconds(0.125f);
        initialized = true;
    }

    /* void Update() 
    {
        if (!initialized) { return; }
    } */

    void GetValues()
    {
        //get size of QTE background elements
        QTE_1_FullSize = QTE_1_Background.rectTransform.rect.width;
        QTE_2_FullSize = QTE_2_Background.rectTransform.rect.width;
    }

    void SetValues()
    {
        switch (difficulty)
        {
            case LockDifficulty.Easy:
                QTE_Speed = LP_Speed_Base;

                QTE_1_TargetSize = QTE_1_FullSize * LP_TargetSize_BaseMultiplier;
                QTE_2_TargetSize = QTE_2_FullSize * LP_TargetSize_BaseMultiplier;

                QTE_1_PerfectTargetSize = QTE_1_TargetSize * LP_PerfectPickTargetSize_Multiplier;
                break;
            case LockDifficulty.Moderate:
                QTE_Speed = LP_Speed_Base * LP_Speed_ModerateMultiplier;

                QTE_1_TargetSize = QTE_1_FullSize * LP_TargetSize_BaseMultiplier * LP_TargetSize_ModerateMultiplier;
                QTE_2_TargetSize = QTE_2_FullSize * LP_TargetSize_BaseMultiplier * LP_TargetSize_ModerateMultiplier;

                QTE_1_PerfectTargetSize = QTE_1_TargetSize * LP_PerfectPickTargetSize_Multiplier;
                break;
            case LockDifficulty.Hard:
                QTE_Speed = LP_Speed_Base * LP_Speed_HardMultiplier;
                
                QTE_1_TargetSize = QTE_1_FullSize * LP_TargetSize_BaseMultiplier * LP_TargetSize_HardMultiplier;
                QTE_2_TargetSize = QTE_2_FullSize * LP_TargetSize_BaseMultiplier * LP_TargetSize_HardMultiplier;

                QTE_1_PerfectTargetSize = QTE_1_TargetSize * LP_PerfectPickTargetSize_Multiplier;
                break;
            default:
                break;
        }

        
    }
    #endregion

    //PUBLIC METHODS

    public override void Interact()
    {
        if (isPickable && state != DoorLockState.Unlocked)
        {
            prevInputState = FPS_InputHandler.Instance.currentState;
            FPS_InputHandler.Instance.SetInputState(InputState.LockedInteraction);
            
            StartCoroutine(OpenLockGUI());
        }
        else
        {
            door.Interact();
        }
    }

    void UnlockDoor()
    {
        Debug.Log("Door Unlocked");
        door.LockDoor(false);
    }

    
    #region LOCK PICKING

    private IEnumerator OpenLockGUI()
    {
        VolumeManager.Instance.SetVolume(VolumeType.LockPick);
        
        ResetIndicators(); // Add this line
        SetQTEImages();

        canvasGroup.gameObject.SetActive(true);
        
        // Fade in UI
        float elapsedTime = 0;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        FPS_InputHandler.Instance.lint_InteractTriggered.AddListener(QTE_1);
    }

    void ResetIndicators()
    {
        QTE_1_IndicatorValue = 0;
        QTE_2_IndicatorValue = 0;
        QTE_1_Indicator.rectTransform.anchoredPosition = Vector2.zero;
        QTE_2_Indicator.rectTransform.anchoredPosition = Vector2.zero;
    }

    void CleanupEventListeners()
    {
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_1);
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_2_Check);
        FPS_InputHandler.Instance.lint_InteractReleased.RemoveListener(QTE_1_Check);
    }

    void SetQTEImages()
    {
        // Set the width of the first QTE target zone
        RectTransform QTE_1_Target_RT = QTE_1_TargetImage.rectTransform;
        QTE_1_Target_RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, QTE_1_TargetSize);

        // Set the width of the second QTE target zone
        RectTransform QTE_2_Target_RT = QTE_2_TargetImage.rectTransform;
        QTE_2_Target_RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, QTE_2_TargetSize);

        // Set the width of the perfect target zone
        RectTransform QTE_1_PerfectTarget_RT = QTE_1_PerfectTargetImage.rectTransform;
        QTE_1_PerfectTarget_RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, QTE_1_PerfectTargetSize);

        // Position indicators at the start (left side) of their backgrounds
        QTE_1_Indicator.rectTransform.anchoredPosition = new Vector2(0, 0);
        QTE_2_Indicator.rectTransform.anchoredPosition = new Vector2(0, 0);
    }

    void ResetLockPicking()
    {
        isInQTE1Phase = false;
        isInQTE2Phase = false;
        unlockedPins = 0;
        
        CleanupEventListeners(); // Add this line
        StartCoroutine(CloseLockGUI());
    }

    private IEnumerator CloseLockGUI() //do not call directly
    {
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
        
        // Reset indicators
        SetQTEImages();
        SetValues();

        VolumeManager.Instance.SetVolume(VolumeType.Default);
        
        FPS_InputHandler.Instance.SetInputState(prevInputState);
    }

    #region QTEs
    void QTE_1()
    {
        isInQTE2Phase = false;
        
        Debug.Log("Starting QTE 1");
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_1);
        FPS_InputHandler.Instance.lint_InteractReleased.AddListener(QTE_1_Check);

        isInQTE1Phase = true;
    }

    void QTE_1_Check()
    {
        isInQTE1Phase = false;

        // Check QTE_1 success
        Debug.Log("Checking QTE 1 success");
        
        float targetCenter = QTE_1_FullSize / 2;
        bool isPerfect = Mathf.Abs(QTE_1_IndicatorValue - targetCenter) <= QTE_1_PerfectTargetSize / 2;
        bool isSuccess = Mathf.Abs(QTE_1_IndicatorValue - targetCenter) <= QTE_1_TargetSize / 2;

        if (!isSuccess)
        {
            Debug.Log("Lock picking unsucessful");
            
            ResetLockPicking();
            return;
        }
        else if (isPerfect)
        {
            Debug.Log("PERFECT PICK");
            Debug.Log($"Pin successfully set, {unlockedPins}/{pinCount} pins set");
            unlockedPins++;

            if (unlockedPins >= pinCount)
            {
                Debug.Log("Lock Sucessfully Picked, door is unlocking now");
                UnlockDoor();
                ResetLockPicking();
                //StartCoroutine(CloseLockGUI());
                return;
            }

            Debug.Log($"Pin sucessfully set, {unlockedPins}/{pinCount} pins set");
            FPS_InputHandler.Instance.lint_InteractTriggered.AddListener(QTE_1);
            FPS_InputHandler.Instance.lint_InteractReleased.RemoveListener(QTE_1_Check);
            return;
        }

        FPS_InputHandler.Instance.lint_InteractReleased.RemoveListener(QTE_1_Check);
        QTE_2();
    }

    void QTE_2()
    {
        Debug.Log("Starting QTE 2");
        
        // Reset QTE_2 indicator position before starting
        QTE_2_IndicatorValue = 0;
        QTE_2_Indicator.rectTransform.anchoredPosition = Vector2.zero;
        
        isInQTE2Phase = true;
        FPS_InputHandler.Instance.lint_InteractTriggered.AddListener(QTE_2_Check);
    }

    void QTE_2_Check()
    {
        FPS_InputHandler.Instance.lint_InteractTriggered.RemoveListener(QTE_2_Check);
        
        // Don't disable QTE2 phase until after we check success
        float targetCenter = QTE_2_FullSize / 2;
        bool isSuccess = Mathf.Abs(QTE_2_IndicatorValue - targetCenter) <= QTE_2_TargetSize / 2;

        if (isSuccess)
        {
            unlockedPins++;
            Debug.Log($"Pin successfully set, {unlockedPins}/{pinCount} pins set");
            
            if (unlockedPins >= pinCount)
            {
                UnlockDoor();
                ResetLockPicking();
                return;
            }

            // Important: Set QTE2 phase to false AFTER success check
            isInQTE2Phase = false;

            isInQTE1Phase = true;
            QTE_1_IndicatorValue = 0; // Reset QTE1 position
            QTE_1_Indicator.rectTransform.anchoredPosition = Vector2.zero;
            FPS_InputHandler.Instance.lint_InteractReleased.AddListener(QTE_1_Check);
            return;
        }

        isInQTE2Phase = false;
        ResetLockPicking();
    }

    private void Update()
    {
        if (!initialized) { return; }
        
        // Handle QTE_1 oscillation
        if (/* FPS_InputHandler.Instance.Lint_InteractInput &&  */!isInQTE2Phase && isInQTE1Phase)
        {
            QTE_1_IndicatorValue = Mathf.PingPong(Time.time * QTE_Speed, QTE_1_FullSize);
            QTE_1_Indicator.rectTransform.anchoredPosition = new Vector2(QTE_1_IndicatorValue - (QTE_1_FullSize/2), QTE_1_Indicator.rectTransform.anchoredPosition.y);
        }

        // Handle QTE_2 oscillation during second phase
        if (isInQTE2Phase)
        {
            QTE_2_IndicatorValue = Mathf.PingPong(Time.time * QTE_Speed, QTE_2_FullSize);
            QTE_2_Indicator.rectTransform.anchoredPosition = new Vector2(QTE_2_IndicatorValue - (QTE_2_FullSize/2), QTE_2_Indicator.rectTransform.anchoredPosition.y);
        }

        // Add escape key handling
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetLockPicking();
        }
    }
    #endregion
    #endregion
}
