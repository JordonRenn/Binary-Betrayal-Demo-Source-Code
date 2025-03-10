using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("")] // Hide from Add Component menu
public class HierarchyColorID : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string id = "";

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
        }
    }

    public string ID => id;
}
