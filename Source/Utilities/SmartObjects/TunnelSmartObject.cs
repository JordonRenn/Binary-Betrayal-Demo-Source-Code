using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace SBG.SmartObjects
{
    /// <summary>
    /// Smart object for tunnels. Editor-only: refreshes tunnel segments, lights, and decorations.
    /// </summary>
    public class TunnelSmartObject : MonoBehaviour, ISmartObject
    {

#if UNITY_EDITOR
        public void RefreshTunnel(GameObject tunnelPrefab, GameObject lightPrefab = null, List<GameObject> decorPrefabs = null)
        {
            //
        }
#endif

        public void Refresh()
        {
            // Implement refresh logic if needed
        }

        public void Clear()
        {
            // Implement clear logic if needed
        }
        public void Flatten()
        {
#if UNITY_EDITOR
            // Check if this is a prefab instance and unpack it first
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                // Get the root prefab instance
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                if (prefabRoot != null)
                {
                    PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }

            // Now safe to rename and remove component
            gameObject.name = "TunnelSmartObject_Flattened";
            DestroyImmediate(this);
#endif
        }
    }
}