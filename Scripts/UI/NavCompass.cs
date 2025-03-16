using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NavCompass : MonoBehaviour
{
    private GameObject playerObject;
    
    [SerializeField] private RawImage compassImg;
    [SerializeField] GameObject iconPrefab;
    

    private Dictionary<Trackable, CompassIcon> displayedIcons = new Dictionary<Trackable, CompassIcon>(); //icon game obj ref
    private List<Trackable> displayedTrackables = new List<Trackable>(); //class ref

    private Queue<Trackable> markerAddQueue = new Queue<Trackable>();
    private Queue<Trackable> markerRemoveQueue = new Queue<Trackable>();

    float compassUnit;

    void Awake()
    {
        GameMaster.Instance.gm_PlayerSpawned.AddListener(GetPlayer);
        GameMaster.Instance.globalTick.AddListener(CheckDrawDistance);
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
        if (playerObject == null)
        {
            return;
        }

        compassImg.uvRect = new Rect(playerObject.transform.localEulerAngles.y / 360f, 0f, 1f, 1f);

        if (displayedTrackables.Count > 0)
        {
            foreach (Trackable trackable in displayedTrackables)
            {
                if (trackable.compassImage != null)
                {
                    trackable.compassImage.rectTransform.anchoredPosition = GetCompassPosition(trackable);
                }
            }
        }
    }

    Vector2 GetCompassPosition(Trackable trackable)
    {
        Vector2 playerPos = new Vector2(playerObject.transform.position.x, playerObject.transform.position.z);
        Vector2 playerForward = new Vector2(playerObject.transform.forward.x, playerObject.transform.forward.z);

        float angle = Vector2.SignedAngle(trackable.position - playerPos, playerForward);

        return new Vector2(compassUnit * angle, 0f);
    }

    public void AddCompassMarker(Trackable trackable)
    {
        if (!displayedTrackables.Contains(trackable) && !markerAddQueue.Contains(trackable))
        {
            markerAddQueue.Enqueue(trackable);
        }
    }

    public void RemoveCompassMarker(Trackable trackable)
    {
        if (displayedTrackables.Contains(trackable) && !markerRemoveQueue.Contains(trackable))
        {
            markerRemoveQueue.Enqueue(trackable);
        }
    }

    void ProcessMarkerQueues()
    {
        while (markerRemoveQueue.Count > 0)
        {
            Trackable trackable = markerRemoveQueue.Dequeue();
            displayedIcons[trackable].GetComponent<Image>().DOFade(0f, 0.5f).OnComplete(() =>
            {
                displayedIcons[trackable].DestroyIcon();
                displayedIcons.Remove(trackable);
                displayedTrackables.Remove(trackable);
            });
        }

        while (markerAddQueue.Count > 0)
        {
            Trackable trackable = markerAddQueue.Dequeue();
            GameObject newtrackable = Instantiate(iconPrefab, compassImg.transform);
            newtrackable.GetComponent<CompassIcon>().compass = this;
            
            if (!displayedIcons.ContainsKey(trackable))
            {
                displayedIcons.Add(trackable, newtrackable.GetComponent<CompassIcon>());
            }

            trackable.compassImage = newtrackable.GetComponent<Image>();
            trackable.compassImage.sprite = trackable.compassIcon;

            Color color = trackable.compassImage.color;
            color.a = 0f;
            trackable.compassImage.color = color;

            trackable.compassImage.DOFade(1f, 0.5f);
            
            displayedTrackables.Add(trackable);
        }
    }

    void CheckDrawDistance()
    {
        List<Trackable> trackablesToRemove = new List<Trackable>();

        foreach (Trackable trackable in displayedTrackables)
        {
            float distance = Vector2.Distance(new Vector2(playerObject.transform.position.x, playerObject.transform.position.z), trackable.position);
            if (distance > trackable.compassDrawDistance)
            {
                trackablesToRemove.Add(trackable);
                Debug.Log($"NAV COMPASS | Queueing {trackable} marker for display removal");
            }
        }

        foreach (Trackable trackable in trackablesToRemove)
        {
            RemoveCompassMarker(trackable);
            Debug.Log($"NAV COMPASS | {trackable} marker queued for removal");
        }

        foreach (Trackable trackable in GameMaster.Instance.allTrackables)
        {
            if (!displayedTrackables.Contains(trackable))
            {
                float distance = Vector2.Distance(new Vector2(playerObject.transform.position.x, playerObject.transform.position.z), trackable.position);
                if (distance <= trackable.compassDrawDistance)
                {
                    AddCompassMarker(trackable);
                    Debug.Log($"NAV COMPASS | {trackable} marker queued for adding");
                }
            }
        }

        ProcessMarkerQueues();
    }
}
