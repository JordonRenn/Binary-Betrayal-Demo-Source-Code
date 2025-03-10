using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NavCompass : MonoBehaviour
{
    private GameObject playerObject;
    
    [SerializeField] private RawImage compassImg;
    [SerializeField] GameObject iconPrefab;
    

    private Dictionary<Trackable, GameObject> displayedIcons = new Dictionary<Trackable, GameObject>(); //icon game obj ref
    private List<Trackable> displayedTrackables = new List<Trackable>(); //class ref

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

    public void AddCompassMarker(Trackable trackable)
    {
        GameObject newtrackable = Instantiate(iconPrefab, compassImg.transform);
        
        if (!displayedIcons.ContainsKey(trackable)) //if not in dict
        {
            displayedIcons.Add(trackable, newtrackable); //add to dict
        }

        trackable.compassImage = newtrackable.GetComponent<Image>();
        trackable.compassImage.sprite = trackable.compassIcon;

        Color color = trackable.compassImage.color;
        color.a = 0f;
        trackable.compassImage.color = color;

        trackable.compassImage.DOFade(1f, 0.5f);

        displayedTrackables.Add(trackable);
    }

    public void RemoveCompassMarker(Trackable trackable)
    {
        if (displayedIcons.ContainsKey(trackable))
        {
            displayedIcons[trackable].GetComponent<Image>().DOFade(0f, 0.5f).OnComplete(() =>
            {
                GameObject.Destroy(displayedIcons[trackable].gameObject);
                displayedIcons.Remove(trackable);
            });
        }

        displayedTrackables.Remove(trackable);
    }

    Vector2 GetCompassPosition(Trackable trackable)
    {
        Vector2 playerPos = new Vector2(playerObject.transform.position.x, playerObject.transform.position.z);
        Vector2 playerForward = new Vector2(playerObject.transform.forward.x, playerObject.transform.forward.z);

        float angle = Vector2.SignedAngle(trackable.position - playerPos, playerForward);

        return new Vector2(compassUnit * angle, 0f);
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
            Debug.Log($"NAV COMPASS | {trackable} marker removed");
        }

        foreach (Trackable trackable in GameMaster.Instance.allTrackables)
        {
            if (!displayedTrackables.Contains(trackable))
            {
                float distance = Vector2.Distance(new Vector2(playerObject.transform.position.x, playerObject.transform.position.z), trackable.position);
                if (distance <= trackable.compassDrawDistance)
                {
                    AddCompassMarker(trackable);
                    Debug.Log($"NAV COMPASS | {trackable} marker added");
                }
            }
        }
    }
}
