using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public enum WallSectionType { Wall, Door, Window }

public class WallSmartObject : MonoBehaviour
{
    public WallSectionType sectionType;
    public int selectedIndex;
    public int selectedCutoutIndex;
    public int selectedFrameIndex;
    public int selectedPaneIndex;

    // Multiple decor selections
    public List<int> selectedDecorIndices = new List<int>();

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
#endif
}
