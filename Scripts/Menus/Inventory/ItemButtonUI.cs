using UnityEngine;
using UnityEngine.UI;
using TMPro;

//NO ICONS ON ITEMS

public class ItemButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    private TMP_Text label;
    private Image background;

    [Header("Text Color Settings")]
    [SerializeField] private Color highlightedTextColor = Color.yellow;
    private Color defaultTextColor;

    void Awake()
    {
        InitializeComponents();
        CaptureDefaultColors();
    }

    private void InitializeComponents()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>();
        if (background == null)
            background = GetComponent<Image>();
    }

    private void CaptureDefaultColors()
    {
        if (label != null)
        {
            defaultTextColor = label.color;
        }
    }

    public void SetItem(IItem item)
    {
        // Ensure components are initialized
        InitializeComponents();

        if (label != null)
        {
            label.text = item.Name;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (label != null)
        {
            label.color = highlighted ? highlightedTextColor : defaultTextColor;
        }
    }
}
