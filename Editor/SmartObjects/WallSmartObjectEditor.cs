#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[CustomEditor(typeof(WallSmartObject))]
public class WallSmartObjectEditor : Editor
{
    private static readonly string WallFolder = "Assets/Prefabs/SmartObjects/Wall";
    private static readonly string WindowFolder = "Assets/Prefabs/SmartObjects/Window";

    // Cached prefabs
    private static Dictionary<WallSectionType, List<GameObject>> wallPrefabsCache = new();
    private static List<GameObject> framePrefabsCache = new();
    private static List<GameObject> panePrefabsCache = new();
    private static List<GameObject> decorPrefabsCache = new();

    private static Dictionary<string, List<GameObject>> wallSizeToPrefabs = new();

    // Header image
    private Texture2D headerImage;

    private void OnEnable()
    {
        // Load header image once
        headerImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/SmartObjects/img/wallsmartobject-header.png");
    }

    public override void OnInspectorGUI()
    {
        var smart = (WallSmartObject)target;

        // --- Draw header image ---
        if (headerImage != null)
        {
            float aspect = (float)headerImage.width / headerImage.height;
            float width = EditorGUIUtility.currentViewWidth - 20; // padding
            float height = width / aspect;
            GUILayout.Label(headerImage, GUILayout.Width(width), GUILayout.Height(height));
        }

        EditorGUILayout.Space();

        // --- Refresh Assets Button ---
        if (GUILayout.Button("Refresh Assets"))
        {
            RefreshAllAssets();
        }

        EditorGUI.BeginChangeCheck();

        // --- Wall Type ---
        WallSectionType newType = (WallSectionType)EditorGUILayout.EnumPopup("Wall Type", smart.sectionType);
        if (newType != smart.sectionType)
        {
            smart.sectionType = newType;
            ResetIndices(smart);
        }

        // --- Ensure wall prefabs cached ---
        if (!wallPrefabsCache.ContainsKey(smart.sectionType))
            wallPrefabsCache[smart.sectionType] = new List<GameObject>();

        var wallPrefabs = wallPrefabsCache[smart.sectionType];
        if (wallPrefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("No prefabs found. Click 'Refresh Assets'.", MessageType.Warning);
            return;
        }

        // --- Build wall size dictionary ---
        BuildWallSizeDictionary(wallPrefabs);

        // --- Wall Size Dropdown ---
        string[] wallSizes = wallSizeToPrefabs.Keys.OrderBy(s => s).ToArray();
        if (wallSizes.Length == 0) return;

        smart.selectedIndex = Mathf.Clamp(smart.selectedIndex, 0, wallSizes.Length - 1);
        int newWallSizeIndex = EditorGUILayout.Popup("Wall Size", smart.selectedIndex, wallSizes);
        if (newWallSizeIndex != smart.selectedIndex)
        {
            smart.selectedIndex = newWallSizeIndex;
            smart.selectedCutoutIndex = 0;
            ResetWindowIndices(smart);
        }

        // --- Cutout Dropdown ---
        string selectedWallSize = wallSizes[smart.selectedIndex];
        var cutouts = wallSizeToPrefabs[selectedWallSize];
        string[] cutoutNames = cutouts.Select(p => p.name).ToArray();

        smart.selectedCutoutIndex = Mathf.Clamp(smart.selectedCutoutIndex, 0, cutouts.Count - 1);
        int newCutoutIndex = EditorGUILayout.Popup("Cutout Size", smart.selectedCutoutIndex, cutoutNames);
        if (newCutoutIndex != smart.selectedCutoutIndex)
        {
            smart.selectedCutoutIndex = newCutoutIndex;
            ResetWindowIndices(smart);
        }

        // --- Window Children ---
        GameObject framePrefab = null, panePrefab = null;
        List<GameObject> decorPrefabs = null;

        if (smart.sectionType == WallSectionType.Window)
        {
            framePrefab = DrawWindowDropdown("Frame", ref smart.selectedFrameIndex, framePrefabsCache);
            panePrefab = DrawWindowDropdown("Pane", ref smart.selectedPaneIndex, panePrefabsCache);

            // --- Multiple Decor ---
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Decor Options", EditorStyles.boldLabel);

            if (smart.selectedDecorIndices == null)
                smart.selectedDecorIndices = new List<int>();

            if (smart.selectedDecorIndices.Count == 0)
                smart.selectedDecorIndices.Add(0); // always at least one

            for (int i = 0; i < smart.selectedDecorIndices.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                int index = smart.selectedDecorIndices[i];
                GameObject selectedDecor = DrawWindowDropdown($"Decor {i + 1}", ref index, decorPrefabsCache);
                smart.selectedDecorIndices[i] = index;

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    smart.selectedDecorIndices.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Decor"))
            {
                smart.selectedDecorIndices.Add(0);
            }

            // Collect selected decor prefabs
            decorPrefabs = smart.selectedDecorIndices
                .Where(idx => idx >= 0 && idx < decorPrefabsCache.Count + 1)
                .Select(idx => idx == 0 ? null : decorPrefabsCache[idx - 1])
                .Where(p => p != null)
                .ToList();
        }

        // --- Apply selected prefabs ---
        if (cutouts.Count > 0)
        {
            int cutoutIndex = Mathf.Clamp(smart.selectedCutoutIndex, 0, cutouts.Count - 1);
            smart.RefreshWall(cutouts[cutoutIndex], framePrefab, panePrefab, decorPrefabs);
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(smart);
    }

    #region --- Helpers ---

    private void RefreshAllAssets()
    {
        // Walls
        wallPrefabsCache.Clear();
        foreach (WallSectionType type in System.Enum.GetValues(typeof(WallSectionType)))
            wallPrefabsCache[type] = LoadWallPrefabs(type);

        // Window children
        framePrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_frame_");
        panePrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_pane_");
        decorPrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_decor_");

        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private GameObject DrawWindowDropdown(string label, ref int selectedIndex, List<GameObject> cachedPrefabs)
    {
        // Create a temporary array with "None" at index 0
        GameObject[] options = new GameObject[cachedPrefabs.Count + 1];
        options[0] = null; // None
        cachedPrefabs.CopyTo(options, 1);

        // Create display names
        string[] names = options.Select(p => p != null ? p.name : "None").ToArray();

        // Clamp selectedIndex
        selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);

        // Draw popup
        selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names);

        // Return the prefab for the current selection
        return options[selectedIndex];
    }

    private void ResetIndices(WallSmartObject smart)
    {
        smart.selectedIndex = 0;
        smart.selectedCutoutIndex = 0;
        ResetWindowIndices(smart);
    }

    private void ResetWindowIndices(WallSmartObject smart)
    {
        smart.selectedFrameIndex = 0;
        smart.selectedPaneIndex = 0;
        smart.selectedDecorIndices = new List<int>(); // reset to empty list
    }

    private List<GameObject> LoadWallPrefabs(WallSectionType type)
    {
        string prefix = type switch
        {
            WallSectionType.Wall => "wall_base_",
            WallSectionType.Door => "wall_door_",
            WallSectionType.Window => "wall_window_",
            _ => ""
        };

        AssetDatabase.Refresh();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { WallFolder });
        var list = new List<GameObject>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.ToLower().StartsWith(prefix))
                list.Add(prefab);
        }
        return list.OrderBy(p => p.name).ToList();
    }

    private void BuildWallSizeDictionary(List<GameObject> prefabs)
    {
        wallSizeToPrefabs.Clear();
        Regex regex = new Regex(@"\d+x\d+", RegexOptions.IgnoreCase);
        foreach (var prefab in prefabs)
        {
            var match = regex.Match(prefab.name.ToLower());
            if (!match.Success) continue;

            string wallSize = match.Value;
            if (!wallSizeToPrefabs.ContainsKey(wallSize))
                wallSizeToPrefabs[wallSize] = new List<GameObject>();
            wallSizeToPrefabs[wallSize].Add(prefab);
        }
    }

    private List<GameObject> LoadPrefabsWithPrefix(string folder, string prefix)
    {
        AssetDatabase.Refresh();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        var list = new List<GameObject>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.ToLower().StartsWith(prefix))
                list.Add(prefab);
        }
        return list.OrderBy(p => p.name).ToList();
    }

    #endregion
}
#endif
