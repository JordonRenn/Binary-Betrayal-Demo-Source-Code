using UnityEngine;

namespace Saus.HierarchyColorTool
{
    [ExecuteInEditMode]
    [AddComponentMenu("")] // Hide from Add Component menu
    [DisallowMultipleComponent]
    public class HierarchyColorID : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private string id = "";

        private void OnEnable()
        {
            // Ensure we have a valid ID when the component is enabled
            EnsureID();
        }

        private void Awake()
        {
            // Also ensure ID on Awake
            EnsureID();
        }

        private void EnsureID()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public string ID 
        { 
            get 
            {
                EnsureID(); // Make sure we have an ID when getting it
                return id;
            }
        }
    }
}
