using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

/* 
Visibility controlled by HUD_Controller class
 */

public class NavCompass : MonoBehaviour
{
    public static NavCompass _instance;
    public static NavCompass Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning("Attempting to access NavCompass before initialization.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [SerializeField] private RawImage compassImg;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float updateFrequency = 0.1f; // Update every 0.1 seconds instead of every frame

    private GameObject playerObject;
    //private Dictionary<Trackable, CompassIcon> activeIcons = new Dictionary<Trackable, CompassIcon>();
    private Dictionary<SauceObject, CompassIcon> activeIcons = new Dictionary<SauceObject, CompassIcon>();
    private float lastUpdateTime;
    private float compassUnit;

    void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            DontDestroyOnLoad(gameObject);
            GameMaster.Instance.gm_PlayerSpawned.AddListener(GetPlayer);
        }
    }

    void Start()
    {
        compassUnit = compassImg.rectTransform.rect.width / 360f;
    }

    void GetPlayer()
    {
        playerObject = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (playerObject == null) return;

        // Update compass rotation
        compassImg.uvRect = new Rect(playerObject.transform.localEulerAngles.y / 360f, 0f, 1f, 1f);

        // Throttle updates for performance
        if (Time.time - lastUpdateTime < updateFrequency) return;
        lastUpdateTime = Time.time;

        UpdateCompass();
    }

    void UpdateCompass()
    {
        // Get all trackables that should be displayed
        var trackablesToShow = GetTrackablesInRange();

        // Remove icons that are no longer needed
        var iconsToRemove = activeIcons.Keys.Where(t => t == null || t.gameObject == null || !trackablesToShow.Contains(t)).ToList();
        foreach (var trackable in iconsToRemove)
        {
            RemoveIcon(trackable);
        }

        // Add icons for new trackables
        foreach (var trackable in trackablesToShow)
        {
            if (!activeIcons.ContainsKey(trackable))
            {
                AddIcon(trackable);
            }
            else
            {
                // Update existing icon position
                activeIcons[trackable].UpdatePosition(GetCompassPosition(trackable));
            }
        }
    }

    List<SauceObject> GetTrackablesInRange()
    {
        if (playerObject == null || GameMaster.Instance?.allTrackableSauceObjects == null)
            return new List<SauceObject>();

        Vector2 playerPos = new Vector2(playerObject.transform.position.x, playerObject.transform.position.z);

        return GameMaster.Instance.allTrackableSauceObjects
            .Where(t => t != null && t.gameObject != null)
            .Where(t => Vector2.Distance(playerPos, t.position) <= t.nav_CompassDrawDistance)
            .ToList();
    }

    void AddIcon(SauceObject trackable)
    {
        if (trackable == null || iconPrefab == null) return;

        GameObject iconObj = Instantiate(iconPrefab, compassImg.transform);
        CompassIcon icon = iconObj.GetComponent<CompassIcon>();

        icon.Initialize(trackable);
        activeIcons[trackable] = icon;

        // Fade in animation
        Image iconImage = iconObj.GetComponent<Image>();
        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = 0f;
            iconImage.color = color;
            iconImage.DOFade(1f, 0.3f);
        }

        // Set initial position
        icon.UpdatePosition(GetCompassPosition(trackable));
    }

    void RemoveIcon(SauceObject trackable)
    {
        if (!activeIcons.ContainsKey(trackable)) return;

        CompassIcon icon = activeIcons[trackable];
        activeIcons.Remove(trackable);

        if (icon != null && icon.gameObject != null)
        {
            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.DOFade(0f, 0.3f).OnComplete(() =>
                {
                    if (icon != null && icon.gameObject != null)
                    {
                        icon.Cleanup();
                        Destroy(icon.gameObject);
                    }
                });
            }
            else
            {
                icon.Cleanup();
                Destroy(icon.gameObject);
            }
        }
    }

    Vector2 GetCompassPosition(SauceObject trackable)
    {
        if (playerObject == null || trackable == null) return Vector2.zero;

        Vector2 playerPos = new Vector2(playerObject.transform.position.x, playerObject.transform.position.z);
        Vector2 playerForward = new Vector2(playerObject.transform.forward.x, playerObject.transform.forward.z);

        float angle = Vector2.SignedAngle(trackable.position - playerPos, playerForward);
        return new Vector2(compassUnit * angle, 0f);
    }

    // Public methods for external calls (maintain compatibility)
    public void AddCompassMarker(SauceObject trackable)
    {
        // This is now handled automatically by UpdateCompass
        // Keep for compatibility but no longer needed
    }

    public void RemoveCompassMarker(SauceObject trackable)
    {
        RemoveIcon(trackable);
    }

    // Legacy property for backward compatibility
    public List<SauceObject> displayedTrackables => activeIcons.Keys.Where(t => t != null).ToList();
}
