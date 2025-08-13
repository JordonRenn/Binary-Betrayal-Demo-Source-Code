using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using FMODUnity;

public class NotificationSystem : MonoBehaviour
{
    private static NotificationSystem _instance;
    public static NotificationSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(NotificationSystem)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    [SerializeField] private bool isDebugMode;
    
    [Header("Rect & Positions")]
    [Space(10)]
    
    [SerializeField] private RectTransform rect_NotifSystem; //transfomr of the whole UI element

    [SerializeField] private Vector2 pos_Hidden; //rect position when hidden
    [SerializeField] private Vector2 pos_Active; //rect position when active

    [Header("Text Options")]
    [Space(10)]

    [SerializeField] private TMP_Text notif_TextBody; //TPM_Text to display message
    [SerializeField] private Color color_Normal;
    [SerializeField] private Color color_Warning;
    [SerializeField] private Color color_Error;
    [SerializeField] private Color color_Reward;

    private Dictionary<NotificationType, Color> color_NotificationType;

    private Queue<Notification> notificationQueue = new Queue<Notification>();
    
    [Header("Display Options")]
    [Space(10)]

    private float display_HoldTime = 3f; //amount of time display stays open
    private float display_TimeToOpen = 0.6f; //how long it takes to open
    private float display_TimeToClose = 1.25f; //how long it takes to close

    [Header("SFX")]
    [Space(10)]

    [SerializeField] private EventReference sfx_Normal; //FMOD event reference
    [SerializeField] private EventReference sfx_Reward; //FMOD event reference
    [SerializeField] private EventReference sfx_Warning; //FMOD event reference
    [SerializeField] private EventReference sfx_Error; //FMOD event reference
    [SerializeField] [Range(0, 100)]float sfx_Volume = 80; //sfx volume

    /* [Header("Console Options")]
    [Space(10)]

    [SerializeField] private TMP_InputField consoleInput; //console input field
    private bool isConsoleInputOpen = false; //is the input showing? */

    enum NotificationDisplayState
    {
        Active,
        Hidden
    }

    private NotificationDisplayState currentNotificationDisplayState;

    private bool paused = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rect_NotifSystem == null)
        {
            Debug.LogWarning($"{nameof(NotificationSystem)}: RectTransform reference is required!");
        }
        if (notif_TextBody == null)
        {
            Debug.LogWarning($"{nameof(NotificationSystem)}: TextMeshPro Text reference is required!");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    void Awake()
    {
        // Initialize as singleton and persist across scenes
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            // Validate required components
            if (rect_NotifSystem == null)
            {
                Debug.LogError($"{nameof(NotificationSystem)}: Required RectTransform component is missing!");
            }
            if (notif_TextBody == null)
            {
                Debug.LogError($"{nameof(NotificationSystem)}: Required TextMeshPro Text component is missing!");
            }
        }
        
        rect_NotifSystem.anchoredPosition = new Vector2(pos_Hidden.x, pos_Hidden.y);
        currentNotificationDisplayState = NotificationDisplayState.Hidden;

        color_NotificationType = new Dictionary<NotificationType, Color>
        {
            { NotificationType.Normal, color_Normal },
            { NotificationType.Warning, color_Warning },
            { NotificationType.Error, color_Error },
            { NotificationType.Reward, color_Reward }
        };
    }

    void Start()
    {
        if (isDebugMode)
        {
            DEBUG_STARTUP();
        }
    }

    void DEBUG_STARTUP()
    {
       Notification debug_m_01 = new Notification("Testing Notification System...", NotificationType.Reward);
       DisplayNotification(debug_m_01);

       Notification debug_m_02 = new Notification("Testing normal text format and overflow functionality", NotificationType.Normal);
       DisplayNotification(debug_m_02);

       Notification debug_m_03 = new Notification("Testing WARNING format", NotificationType.Warning);
       DisplayNotification(debug_m_03);

       Notification debug_m_04 = new Notification("Testing ERROR format", NotificationType.Error);
       DisplayNotification(debug_m_04);
    }

    public void DisplayNotification(Notification notification)
    {
        Debug.Log($"ENQUEUING NOTIFICATION | type: {notification.type} | message: {notification.message}");

        notificationQueue.Enqueue(notification);
        
        if (currentNotificationDisplayState != NotificationDisplayState.Active && !paused)
        {
            //StartCoroutine(DisplayNotificationSequence(message, type));
            ProcessNextNotification();
        }
        
    }

    private void ProcessNextNotification()
    {
        if (currentNotificationDisplayState != NotificationDisplayState.Active && notificationQueue.Count > 0)
        {
            var nextNotificiation = notificationQueue.TryPeek(out Notification results);

            StartCoroutine(DisplayNotificationSequence(results.message, results.type));
        }
    }

    IEnumerator DisplayNotificationSequence(string message, NotificationType type)
    {
        notif_TextBody.SetText(message);
        notif_TextBody.color = color_NotificationType[type];

        // Ensure the color is applied
        notif_TextBody.ForceMeshUpdate();

        PlayNotificationSFX(type);
        
         //open notification display
        yield return OpenNotificationDisplay(display_TimeToOpen);

        yield return new WaitForSeconds(display_HoldTime);

        //close notification display
        yield return CloseNotificationDisplay(display_TimeToClose);

        notificationQueue.Dequeue();
        ProcessNextNotification();
    }

    IEnumerator OpenNotificationDisplay(float time)
    {
        //Debug.Log("Opening notification display....");
        
        UpdateNotificationDisplayState(NotificationDisplayState.Active);
        yield return UpdateNotificationDisplayPosition(pos_Active, time);
    }

    IEnumerator CloseNotificationDisplay(float time)
    {
        //Debug.Log("closing notification display....");
        
        yield return UpdateNotificationDisplayPosition(pos_Hidden, time);
        UpdateNotificationDisplayState(NotificationDisplayState.Hidden);
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator UpdateNotificationDisplayPosition(Vector2 targetPos, float duration)
    {
        yield return rect_NotifSystem.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad).WaitForCompletion();
    }

    void UpdateNotificationDisplayState(NotificationDisplayState state)
    {
        switch (state)
        {
            case NotificationDisplayState.Active:
                currentNotificationDisplayState = NotificationDisplayState.Active;
                break;
            case NotificationDisplayState.Hidden:
                currentNotificationDisplayState = NotificationDisplayState.Hidden;
                break;
        }
    }

    void PlayNotificationSFX(NotificationType type)
    {
        EventReference eventRef = type switch
        {
            NotificationType.Normal => sfx_Normal,
            NotificationType.Warning => sfx_Warning,
            NotificationType.Error => sfx_Error,
            NotificationType.Reward => sfx_Reward,
            _ => sfx_Normal
        };

        var instance = RuntimeManager.CreateInstance(eventRef);
        instance.setVolume(sfx_Volume / 100f);
        instance.start();
        instance.release();
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
        ProcessNextNotification();
    }
}
