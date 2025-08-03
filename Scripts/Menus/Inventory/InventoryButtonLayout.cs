using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper component to ensure inventory buttons have proper layout settings
/// Attach this to button prefabs used in the inventory system
/// </summary>
public class InventoryButtonLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float preferredHeight = 50f;
    [SerializeField] private bool flexibleWidth = true;
    [SerializeField] private bool flexibleHeight = false;

    void Awake()
    {
        SetupLayoutElement();
    }

    private void SetupLayoutElement()
    {
        // Add LayoutElement if it doesn't exist
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        // Configure the layout element
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.flexibleWidth = flexibleWidth ? 1f : 0f;
        layoutElement.flexibleHeight = flexibleHeight ? 1f : 0f;

        Debug.Log($"Setup layout element for {gameObject.name}");
    }

    /// <summary>
    /// Call this method to update layout settings at runtime
    /// </summary>
    public void UpdateLayout(float height = -1)
    {
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null && height > 0)
        {
            layoutElement.preferredHeight = height;
            preferredHeight = height;
        }
    }
}
