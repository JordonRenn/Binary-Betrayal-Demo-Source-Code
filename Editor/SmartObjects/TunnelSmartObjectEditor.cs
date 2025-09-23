using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace SBG.SmartObjects.Editor
{
    [CustomEditor(typeof(TunnelSmartObject))]
    public class TunnelSmartObjectEditor : UnityEditor.Editor
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
            base.OnInspectorGUI();

            if (headerImage != null)
            {
                GUILayout.Label(headerImage);
            }

            /* if (GUILayout.Button("Refresh Tunnel"))
            {
                ((TunnelSmartObject)target).RefreshTunnel();
            } */
        }

        #endregion

        #region --- Helpers ---

        #endregion
    }
}