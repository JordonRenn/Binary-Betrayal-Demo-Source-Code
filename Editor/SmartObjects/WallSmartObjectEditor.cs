#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SBG.SmartObjects.Editor
{
    [CustomEditor(typeof(WallSmartObject))]
    public class WallSmartObjectEditor : UnityEditor.Editor
    {
        private static readonly string WallFolder = "Assets/Prefabs/SmartObjects/Wall";
        private static readonly string WindowFolder = "Assets/Prefabs/SmartObjects/Wall/Window";
        private static readonly string DoorFolder = "Assets/Prefabs/SmartObjects/Wall/Door";

        // Cached prefabs
        // walls
        private static Dictionary<WallSectionType, List<GameObject>> wallPrefabsCache = new();

        // doors
        private static List<GameObject> doorFramePrefabsCache = new();
        private static List<GameObject> doorPrefabsCache = new();
        private static List<GameObject> doorDecorPrefabsCache = new();

        // windows
        private static List<GameObject> windowFramePrefabsCache = new();
        private static List<GameObject> windowPanePrefabsCache = new();
        private static List<GameObject> windowDecorPrefabsCache = new();

        private static Dictionary<string, List<GameObject>> wallSizeToPrefabs = new();

        // Header image
        private Texture2D headerImage;

        private void OnEnable()
        {
            headerImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/SmartObjects/img/wallsmartobject-header.png");
        }

        #region Inspector GUI
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

            // --- Window/Door Children ---
            GameObject framePrefab = null, panePrefab = null;
            List<GameObject> decorPrefabs = null;

            // Get selected cutout prefab and extract cutout size (after third underscore)
            int commonCutoutIndex = Mathf.Clamp(smart.selectedCutoutIndex, 0, cutouts.Count - 1);
            GameObject commonSelectedCutout = cutouts.Count > 0 ? cutouts[commonCutoutIndex] : null;
            string doorCutoutSizeForFilter = null;
            if (commonSelectedCutout != null)
            {
                string[] parts = commonSelectedCutout.name.Split('_');
                if (parts.Length > 3)
                {
                    string candidate = parts[3];
                    var match = System.Text.RegularExpressions.Regex.Match(candidate, @"^\d+x\d+");
                    if (match.Success)
                        doorCutoutSizeForFilter = match.Value.ToLower();
                }
            }

            if (smart.sectionType == WallSectionType.Door)
            {
                // Filter prefabs by cutout size in name
                List<GameObject> filteredFrames = (doorCutoutSizeForFilter == null)
                    ? doorFramePrefabsCache
                    : doorFramePrefabsCache.Where(p => p.name.ToLower().Contains(doorCutoutSizeForFilter)).ToList();
                List<GameObject> filteredPanels = (doorCutoutSizeForFilter == null)
                    ? doorPrefabsCache
                    : doorPrefabsCache.Where(p => p.name.ToLower().Contains(doorCutoutSizeForFilter)).ToList();
                List<GameObject> filteredDecors = (doorCutoutSizeForFilter == null)
                    ? doorDecorPrefabsCache
                    : doorDecorPrefabsCache.Where(p => p.name.ToLower().Contains(doorCutoutSizeForFilter)).ToList();

                framePrefab = DrawWindowDropdown("Frame", ref smart.selectedDoorFrameIndex, filteredFrames);
                panePrefab = DrawWindowDropdown("Door Panel", ref smart.selectedDoorPanelIndex, filteredPanels);

                // --- Multiple Decor ---
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Door Decor Options", EditorStyles.boldLabel);

                if (smart.selectedDoorDecorIndices == null)
                    smart.selectedDoorDecorIndices = new List<int>();

                if (smart.selectedDoorDecorIndices.Count == 0)
                    smart.selectedDoorDecorIndices.Add(0); // always at least one

                for (int i = 0; i < smart.selectedDoorDecorIndices.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    int index = smart.selectedDoorDecorIndices[i];
                    GameObject selectedDecor = DrawWindowDropdown($"Decor {i + 1}", ref index, filteredDecors);
                    smart.selectedDoorDecorIndices[i] = index;

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        smart.selectedDoorDecorIndices.RemoveAt(i);
                        i--;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+ Add Door Decor"))
                {
                    smart.selectedDoorDecorIndices.Add(0);
                }

                // Collect selected decor prefabs
                decorPrefabs = smart.selectedDoorDecorIndices
                    .Where(idx => idx >= 0 && idx < filteredDecors.Count + 1)
                    .Select(idx => idx == 0 ? null : filteredDecors[idx - 1])
                    .Where(p => p != null)
                    .ToList();
            }
            else if (smart.sectionType == WallSectionType.Window)
            {
                // Get selected cutout prefab and extract cutout size (after last underscore)
                int cutoutIndex = Mathf.Clamp(smart.selectedCutoutIndex, 0, cutouts.Count - 1);
                GameObject selectedCutout = cutouts.Count > 0 ? cutouts[cutoutIndex] : null;
                string windowCutoutSizeForFilter = null;
                if (selectedCutout != null)
                {
                    string[] parts = selectedCutout.name.Split('_');
                    if (parts.Length > 3)
                    {
                        // Get the part after the third underscore
                        string candidate = parts[3];
                        // Remove any trailing suffix (e.g., _a, _b)
                        var match = System.Text.RegularExpressions.Regex.Match(candidate, @"^\d+x\d+");
                        if (match.Success)
                            windowCutoutSizeForFilter = match.Value.ToLower();
                    }
                }

                // Filter prefabs by cutout size in name
                List<GameObject> filteredFrames = (windowCutoutSizeForFilter == null)
                    ? windowFramePrefabsCache
                    : windowFramePrefabsCache.Where(p => p.name.ToLower().Contains(windowCutoutSizeForFilter)).ToList();
                List<GameObject> filteredPanes = (windowCutoutSizeForFilter == null)
                    ? windowPanePrefabsCache
                    : windowPanePrefabsCache.Where(p => p.name.ToLower().Contains(windowCutoutSizeForFilter)).ToList();
                List<GameObject> filteredDecors = (windowCutoutSizeForFilter == null)
                    ? windowDecorPrefabsCache
                    : windowDecorPrefabsCache.Where(p => p.name.ToLower().Contains(windowCutoutSizeForFilter)).ToList();

                framePrefab = DrawWindowDropdown("Frame", ref smart.selectedFrameIndex, filteredFrames);
                panePrefab = DrawWindowDropdown("Pane", ref smart.selectedPaneIndex, filteredPanes);

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
                    GameObject selectedDecor = DrawWindowDropdown($"Decor {i + 1}", ref index, filteredDecors);
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
                    .Where(idx => idx >= 0 && idx < filteredDecors.Count + 1)
                    .Select(idx => idx == 0 ? null : filteredDecors[idx - 1])
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
        #endregion

        #region --- Helpers ---

        private void RefreshAllAssets()
        {
            // Walls
            wallPrefabsCache.Clear();
            foreach (WallSectionType type in System.Enum.GetValues(typeof(WallSectionType)))
                wallPrefabsCache[type] = LoadWallPrefabs(type);

            // Window children
            windowFramePrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_frame_");
            windowPanePrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_pane_");
            windowDecorPrefabsCache = LoadPrefabsWithPrefix(WindowFolder, "window_decor_");

            // Door children
            doorFramePrefabsCache = LoadPrefabsWithPrefix(DoorFolder, "door_frame_");
            doorPrefabsCache = LoadPrefabsWithPrefix(DoorFolder, "door_panel_");
            doorDecorPrefabsCache = LoadPrefabsWithPrefix(DoorFolder, "door_decor_");

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
            // Reset window indices
            smart.selectedFrameIndex = 0;
            smart.selectedPaneIndex = 0;
            smart.selectedDecorIndices = new List<int>();

            // Reset door indices
            smart.selectedDoorFrameIndex = 0;
            smart.selectedDoorPanelIndex = 0;
            smart.selectedDoorDecorIndices = new List<int>();
        }

        private List<GameObject> LoadWallPrefabs(WallSectionType type)
        {
            string prefix = type switch
            {
                WallSectionType.Blank => "wall_base_",
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
}
#endif
