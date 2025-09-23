#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;  

namespace SBG.SmartObjects.Editor
{
    [CustomEditor(typeof(SmartObjectGroup))]
    public class SmartObjectGroupEditor : UnityEditor.Editor
    {
        private Texture2D headerImage;

        private void OnEnable()
        {
            // Load header image once
            headerImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/SmartObjects/img/wallsmartobject-header.png");
        }

        #region --- Inspector GUI ---
        public override void OnInspectorGUI()
        {
            if (headerImage != null)
            {
                GUILayout.Label(headerImage);
            }

            SmartObjectGroup group = (SmartObjectGroup)target;
            if (GUILayout.Button("Collect Smart Objects"))
            {
                group.CollectSmartObjects();
            }
            if (GUILayout.Button("Flatten Smart Objects"))
            {
                group.Flatten();
            }
            if (GUILayout.Button("Extract Excluded Objects"))
            {
                group.ExtractExcludedObjects();
            }
            if (GUILayout.Button("Combine Meshes"))
            {
                group.CombineMeshes();
            }

            DrawDefaultInspector();
        }
        #endregion

        #region --- Helpers ---

        #endregion
    }
}
#endif