using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypeButtonUI : MonoBehaviour
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
    
    public void SetType(string typeName)
    {
        // Ensure components are initialized
        InitializeComponents();
        
        if (label != null)
        {
            label.text = typeName;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        //apply highlight color to text
        if (label != null)
        {
            label.color = highlighted ? highlightedTextColor : defaultTextColor;
        }
    }
}
