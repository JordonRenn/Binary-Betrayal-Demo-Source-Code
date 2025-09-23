using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System;

namespace SBG.SmartObjects
{
public enum WallSectionType { Blank, Door, Window }

    public class WallSmartObject : MonoBehaviour, ISmartObject
    {
        public WallSectionType sectionType;

        // wall indices
        public int selectedIndex;
        public int selectedCutoutIndex;

        // Window indices
        public int selectedFrameIndex;
        public int selectedPaneIndex;
        public List<int> selectedDecorIndices = new List<int>();

        // Door indices
        public int selectedDoorFrameIndex;
        public int selectedDoorPanelIndex;
        public List<int> selectedDoorDecorIndices = new List<int>();

        [HideInInspector] public GameObject currentInstance;
        [HideInInspector] public GameObject currentFrame;
        [HideInInspector] public GameObject currentPane;
        [HideInInspector] public List<GameObject> currentDecors = new List<GameObject>();

#if UNITY_EDITOR
        public void RefreshWall(GameObject wallPrefab, GameObject framePrefab = null, GameObject panePrefab = null, List<GameObject> decorPrefabs = null)
        {
            // --- Clear main wall ---
            if (currentInstance != null)
            {
                DestroyImmediate(currentInstance);
                currentInstance = null;
            }

            // --- Clear children ---
            if (currentFrame != null) DestroyImmediate(currentFrame);
            if (currentPane != null) DestroyImmediate(currentPane);
            foreach (var decor in currentDecors)
                if (decor != null) DestroyImmediate(decor);

            currentFrame = null;
            currentPane = null;
            currentDecors.Clear();

            // --- Recreate wall ---
            if (wallPrefab != null)
            {
                currentInstance = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, transform);
                currentInstance.transform.localPosition = Vector3.zero;
                currentInstance.transform.localRotation = Quaternion.identity;
            }

            // --- Frame ---
            if (framePrefab != null)
            {
                currentFrame = (GameObject)PrefabUtility.InstantiatePrefab(framePrefab, transform);
                currentFrame.transform.localPosition = Vector3.zero;
                currentFrame.transform.localRotation = Quaternion.identity;
            }

            // --- Pane ---
            if (panePrefab != null)
            {
                currentPane = (GameObject)PrefabUtility.InstantiatePrefab(panePrefab, transform);
                currentPane.transform.localPosition = Vector3.zero;
                currentPane.transform.localRotation = Quaternion.identity;
            }

            // --- Multiple Decors ---
            if (decorPrefabs != null)
            {
                foreach (var prefab in decorPrefabs)
                {
                    if (prefab == null) continue;
                    var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    currentDecors.Add(obj);
                }
            }
        }

        public void Refresh()
        {
            // Called when the RectTransform dimensions change
            // You can add code here to handle the change if needed
        }

        public void Clear()
        {
            // Destroys all instantiated objects and clears references
            if (currentInstance != null)
            {
                DestroyImmediate(currentInstance);
                currentInstance = null;
            }

            if (currentFrame != null)
            {
                DestroyImmediate(currentFrame);
                currentFrame = null;
            }

            if (currentPane != null)
            {
                DestroyImmediate(currentPane);
                currentPane = null;
            }

            foreach (var decor in currentDecors)
                if (decor != null) DestroyImmediate(decor);

            currentDecors.Clear();
        }

        public void Flatten()
        {
            // removes this component from game object
            // leaves instantiated objects intact
            // rename object in hierarchy to match the wall prefab name
            if (currentInstance != null)
                gameObject.name = currentInstance.name;
            else
                gameObject.name = "WallSmartObject_Flattened";

            DestroyImmediate(this);
        }
#endif
    }
}
