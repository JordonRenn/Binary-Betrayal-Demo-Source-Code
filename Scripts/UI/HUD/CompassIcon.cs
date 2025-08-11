using UnityEngine;
using UnityEngine.UI;

public class CompassIcon : MonoBehaviour
{
    [HideInInspector] public SauceObject trackable;

    private Image iconImage;

    void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    public void Initialize(SauceObject target)
    {
        trackable = target;
        if (iconImage != null && trackable != null)
        {
            iconImage.sprite = trackable.nav_CompassIcon;
            trackable.compassImage = iconImage;
        }
    }

    public void UpdatePosition(Vector2 position)
    {
        if (iconImage != null)
        {
            iconImage.rectTransform.anchoredPosition = position;
        }
    }

    // Called by NavCompass when cleaning up
    public void Cleanup()
    {
        if (trackable != null)
        {
            trackable.compassImage = null;
        }
        trackable = null;
    }
}
