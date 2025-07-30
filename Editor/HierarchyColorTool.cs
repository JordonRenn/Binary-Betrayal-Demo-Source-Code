using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HierarchyTool
{
    [InitializeOnLoad]
    public class HierarchyColorTool
    {
        private static Dictionary<int, Color> objectColors = new Dictionary<int, Color>();
        private static Dictionary<int, Color> textColors = new Dictionary<int, Color>();
        private static Dictionary<int, string> objectIcons = new Dictionary<int, string>();
        private const string PREF_PREFIX = "HierarchyColor_";
        private const string PREF_TEXT_PREFIX = "HierarchyTextColor_";
        private const string PREF_ICON_PREFIX = "HierarchyIcon_";
        private const string PREF_ID_LIST = "HierarchyColorIDs_";

        static HierarchyColorTool()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            // Delay the initial load to ensure Unity is fully initialized
            EditorApplication.delayCall += LoadColorData;
        }

        private static Color GetInverseColor(Color color)
        {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b, 1f);
        }

        private static string GetPersistentID(GameObject obj)
        {
            // Check if the object already has the component
            var persistentID = obj.GetComponent<HierarchyColorID>();
            
            if (persistentID == null)
            {
                try
                {
                    // Try to add the component using Undo system
                    persistentID = Undo.AddComponent<HierarchyColorID>(obj);
                    
                    // If we still couldn't add it, provide clear error message
                    if (persistentID == null)
                    {
                        Debug.LogError($"Failed to add HierarchyColorID to {obj.name}. The script may not be in a runtime-accessible folder. Make sure it's not in the Editor folder.");
                        return System.Guid.NewGuid().ToString(); // Temporary fallback ID
                    }
                    
                    EditorUtility.SetDirty(obj);
                }
                catch (System.Exception e)
                {
                    // If there's an exception, log it with helpful information
                    Debug.LogError($"Error adding HierarchyColorID to {obj.name}: {e.Message}");
                    return System.Guid.NewGuid().ToString(); // Temporary fallback ID
                }
            }
            
            // Ensure the ID isn't null
            if (string.IsNullOrEmpty(persistentID.ID))
            {
                Debug.LogWarning($"Empty ID found on {obj.name}, generating new ID");
                // Force a new ID generation by setting the field directly for an empty ID
                var serializedObject = new SerializedObject(persistentID);
                var idProperty = serializedObject.FindProperty("id");
                idProperty.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(persistentID);
            }
            
            return persistentID.ID;
        }

        [MenuItem("GameObject/Hierarchy Colors/Set Background Color", false, 0)]
        static void SetSelectedObjectsColor()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;

            ColorPicker.Show((color) =>
            {
                foreach (var obj in selectedObjects)
                {
                    string persistentID = GetPersistentID(obj);
                    objectColors[persistentID.GetHashCode()] = color;
                    
                    if (!textColors.ContainsKey(persistentID.GetHashCode()))
                    {
                        textColors[persistentID.GetHashCode()] = GetInverseColor(color);
                    }
                }
                SaveColorData();
                EditorApplication.RepaintHierarchyWindow();
            }, Color.white);
        }

        [MenuItem("GameObject/Hierarchy Colors/Set Text Color", false, 1)]
        static void SetSelectedObjectsTextColor()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;

            ColorPicker.Show((color) =>
            {
                foreach (var obj in selectedObjects)
                {
                    string persistentID = GetPersistentID(obj);
                    textColors[persistentID.GetHashCode()] = color;
                }
                SaveColorData();
                EditorApplication.RepaintHierarchyWindow();
            }, Color.white);
        }

        [MenuItem("GameObject/Hierarchy Colors/Set Icon", false, 2)]
        static void SetSelectedObjectsIcon()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;

            IconPicker.Show((iconName) =>
            {
                foreach (var obj in selectedObjects)
                {
                    string persistentID = GetPersistentID(obj);
                    if (string.IsNullOrEmpty(iconName))
                        objectIcons.Remove(persistentID.GetHashCode());
                    else
                        objectIcons[persistentID.GetHashCode()] = iconName;
                }
                SaveColorData();
                EditorApplication.RepaintHierarchyWindow();
            });
        }

        [MenuItem("GameObject/Hierarchy Colors/Clear Colors", false, 2)]
        static void ClearSelectedObjectsColors()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;

            foreach (var obj in selectedObjects)
            {
                string persistentID = GetPersistentID(obj);
                objectColors.Remove(persistentID.GetHashCode());
                textColors.Remove(persistentID.GetHashCode());
            }
            SaveColorData();
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Hierarchy Colors/Reload Saved Colors", false, 3)]
        static void ReloadSavedColors()
        {
            LoadColorData();
            //Debug.Log("Manual reload of hierarchy colors triggered");
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            var persistentID = obj.GetComponent<HierarchyColorID>();
            if (persistentID == null) return;

            int colorKey = persistentID.ID.GetHashCode();
            
            // Draw background
            if (objectColors.TryGetValue(colorKey, out Color bgColor))
            {
                EditorGUI.DrawRect(selectionRect, bgColor);
            }

            // Draw icon if exists
            if (objectIcons.TryGetValue(colorKey, out string iconName))
            {
                Rect iconRect = new Rect(selectionRect.x + 2, selectionRect.y, 16, 16);
                var iconContent = EditorGUIUtility.IconContent(iconName);
                if (iconContent != null && iconContent.image != null)
                {
                    GUI.DrawTexture(iconRect, iconContent.image);
                }
            }

            // Draw custom text color
            if (textColors.TryGetValue(colorKey, out Color textColor))
            {
                EditorGUI.LabelField(selectionRect, obj.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = textColor },
                    padding = new RectOffset(EditorStyles.label.padding.left + 18, 0, 0, 0)
                });
            }
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            Debug.Log($"Scene opened: {scene.name}");
            LoadColorData(); // Load the new scene's color data
        }

        private static void OnSceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            Debug.Log($"Scene closing: {scene.name}");
            SaveColorData(); // Save current scene's color data before closing the scene
        }

        private static void SaveColorData()
        {
            string sceneName = EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) return;

            // Save IDs ||| STOP EDITING THIS "Join" NEEDS TO BE CAPITALIZED
            string objectIDs = objectColors.Count > 0 ? string.Join(",", objectColors.Keys) : "";
            string textIDs = textColors.Count > 0 ? string.Join(",", textColors.Keys) : "";
            string iconIDs = objectIcons.Count > 0 ? string.Join(",", objectIcons.Keys) : "";

            EditorPrefs.SetString(PREF_ID_LIST + sceneName + "_bg", objectIDs);
            EditorPrefs.SetString(PREF_ID_LIST + sceneName + "_text", textIDs);
            EditorPrefs.SetString(PREF_ID_LIST + sceneName + "_icons", iconIDs);

            // Save colors with # prefix to ensure proper parsing later
            foreach (var entry in objectColors)
            {
                string colorStr = "#" + ColorUtility.ToHtmlStringRGBA(entry.Value);
                EditorPrefs.SetString(PREF_PREFIX + sceneName + "_" + entry.Key, colorStr);
            }

            foreach (var entry in textColors)
            {
                string colorStr = "#" + ColorUtility.ToHtmlStringRGBA(entry.Value);
                EditorPrefs.SetString(PREF_TEXT_PREFIX + sceneName + "_" + entry.Key, colorStr);
            }

            // Save icons
            foreach (var entry in objectIcons)
            {
                EditorPrefs.SetString(PREF_ICON_PREFIX + sceneName + "_" + entry.Key, entry.Value);
            }
        }

        private static void LoadColorData()
        {
            objectColors.Clear();
            textColors.Clear();
            objectIcons.Clear();

            string sceneName = EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) return;

            // Load background colors
            string objectIDsList = EditorPrefs.GetString(PREF_ID_LIST + sceneName + "_bg", "");
            if (!string.IsNullOrEmpty(objectIDsList))
            {
                foreach (string idStr in objectIDsList.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        string colorStr = EditorPrefs.GetString(PREF_PREFIX + sceneName + "_" + id, "");
                        // Check if the color string already has a # prefix
                        if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
                        if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
                        {
                            objectColors[id] = color;
                        }
                    }
                }
            }

            // Load text colors
            string textIDsList = EditorPrefs.GetString(PREF_ID_LIST + sceneName + "_text", "");
            if (!string.IsNullOrEmpty(textIDsList))
            {
                foreach (string idStr in textIDsList.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        string colorStr = EditorPrefs.GetString(PREF_TEXT_PREFIX + sceneName + "_" + id, "");
                        // Check if the color string already has a # prefix
                        if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
                        if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
                        {
                            textColors[id] = color;
                        }
                    }
                }
            }

            // Load icons
            string iconIDsList = EditorPrefs.GetString(PREF_ID_LIST + sceneName + "_icons", "");
            if (!string.IsNullOrEmpty(iconIDsList))
            {
                foreach (string idStr in iconIDsList.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        string iconName = EditorPrefs.GetString(PREF_ICON_PREFIX + sceneName + "_" + id, "");
                        if (!string.IsNullOrEmpty(iconName))
                        {
                            objectIcons[id] = iconName;
                        }
                    }
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }
    }

    // Color Picker Window
    public static class ColorPicker
    {
        public static void Show(System.Action<Color> onColorSelected, Color defaultColor)
        {
            ColorPickerWindow.CloseAll(); // Close any existing windows first
            EditorWindow colorWindow = EditorWindow.GetWindow<ColorPickerWindow>(true, "Color Picker");
            if (colorWindow is ColorPickerWindow colorPickerWindow)
            {
                colorPickerWindow.OnColorSelected = onColorSelected;
                colorPickerWindow.selectedColor = defaultColor;
            }
        }
    }

    public class ColorPickerWindow : EditorWindow
    {
        public System.Action<Color> OnColorSelected;
        public Color selectedColor = Color.white;
        private List<Color> recentColors = new List<Color>();
        private const string RECENT_COLORS_KEY = "HierarchyTool_RecentColors";
        private const int MAX_RECENT_COLORS = 12;
        private Color? colorToApply = null;

        private void OnEnable()
        {
            titleContent = new GUIContent("Color Picker");
            LoadRecentColors();
        }

        private void OnDisable()
        {
            colorToApply = null;
            OnColorSelected = null;
        }

        public static void CloseAll()
        {
            var windows = Resources.FindObjectsOfTypeAll<ColorPickerWindow>();
            foreach (var window in windows)
            {
                window.Close();
            }
        }

        private void OnGUI()
        {
            selectedColor = EditorGUILayout.ColorField("Pick a color", selectedColor);

            if (recentColors.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Recent Colors:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                int colorsPerRow = 6;
                for (int i = 0; i < recentColors.Count; i++)
                {
                    if (i > 0 && i % colorsPerRow == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    if (GUILayout.Button(string.Empty, GUILayout.Width(32), GUILayout.Height(32)))
                    {
                        colorToApply = recentColors[i];
                    }

                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawRect(rect, recentColors[i]);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply"))
            {
                colorToApply = selectedColor;
            }
        }

        private void Update()
        {
            if (colorToApply.HasValue)
            {
                AddToRecentColors(colorToApply.Value);
                OnColorSelected?.Invoke(colorToApply.Value);
                Close();
            }
        }

        private void LoadRecentColors()
        {
            recentColors.Clear();
            string savedColors = EditorPrefs.GetString(RECENT_COLORS_KEY, "");
            if (!string.IsNullOrEmpty(savedColors))
            {
                var colorStrings = savedColors.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var colorStr in colorStrings)
                {
                    if (ColorUtility.TryParseHtmlString("#" + colorStr, out Color color))
                    {
                        recentColors.Add(color);
                    }
                }
            }
        }

        private void AddToRecentColors(Color color)
        {
            recentColors.RemoveAll(c => Mathf.Approximately(c.r, color.r) && 
                                      Mathf.Approximately(c.g, color.g) && 
                                      Mathf.Approximately(c.b, color.b) && 
                                      Mathf.Approximately(c.a, color.a));
            recentColors.Insert(0, color);
            
            if (recentColors.Count > MAX_RECENT_COLORS)
            {
                recentColors.RemoveRange(MAX_RECENT_COLORS, recentColors.Count - MAX_RECENT_COLORS);
            }
            
            var colorStrings = recentColors.Select(c => ColorUtility.ToHtmlStringRGBA(c));
            EditorPrefs.SetString(RECENT_COLORS_KEY, string.Join(",", colorStrings));
        }
    }

    public class IconPicker : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchString = "";
        private System.Action<string> onIconSelected;
        private List<string> customIcons = new List<string>();
        private List<string> recentIcons = new List<string>();
        private const string RECENT_ICONS_KEY = "HierarchyTool_RecentIcons";
        private const int MAX_RECENT_ICONS = 12;
        private string selectedFolderPath = "Assets/Editor/HierarchyIcons";

        private void OnEnable()
        {
            // Ensure the folder exists
            string fullPath = Path.Combine(Application.dataPath, "Editor/HierarchyIcons");
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
            LoadRecentIcons();
        }

        public static void Show(System.Action<string> callback)
        {
            var window = GetWindow<IconPicker>("Icon Picker");
            window.onIconSelected = callback;
            window.LoadRecentIcons();
        }

        private void OnGUI()
        {
            searchString = EditorGUILayout.TextField("Search", searchString);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Icons Folder:", GUILayout.Width(120));
            EditorGUILayout.LabelField(selectedFolderPath, EditorStyles.textField);
            
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Icons Folder", selectedFolderPath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    if (newPath.StartsWith(Application.dataPath))
                    {
                        selectedFolderPath = "Assets" + newPath.Substring(Application.dataPath.Length);
                        SearchForCustomIcons();
                    }
                }
            }

            if (GUILayout.Button("Search All Assets", GUILayout.Width(100)))
            {
                selectedFolderPath = "Assets";
                SearchForCustomIcons();
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (GUILayout.Button("None"))
            {
                onIconSelected?.Invoke("");
                Close();
            }

            EditorGUILayout.Space();
            
            if (recentIcons.Count > 0)
            {
                EditorGUILayout.LabelField("Recent Icons:", EditorStyles.boldLabel);
                DrawIconGroup(recentIcons);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Custom Icons:", EditorStyles.boldLabel);
            DrawCustomIcons();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawIconGroup(List<string> icons)
        {
            if (icons == null || icons.Count == 0) return;

            var iconsToDisplay = new List<string>(icons); // Create a copy for safe iteration
            EditorGUILayout.BeginHorizontal();
            int iconsPerRow = 6;
            int currentInRow = 0;

            foreach (string iconPath in iconsToDisplay)
            {
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                if (icon != null)
                {
                    if (currentInRow >= iconsPerRow)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        currentInRow = 0;
                    }

                    if (GUILayout.Button(new GUIContent(icon, iconPath), GUILayout.Width(32), GUILayout.Height(32)))
                    {
                        string selectedPath = iconPath; // Store path before closing
                        AddToRecentIcons(selectedPath);
                        EditorGUILayout.EndHorizontal(); // Ensure layout is closed
                        onIconSelected?.Invoke(selectedPath);
                        Close();
                        return;
                    }
                    currentInRow++;
                }
            }

            // Only end horizontal if we haven't closed the window
            if (currentInRow > 0)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private void SearchForCustomIcons()
        {
            customIcons.Clear();
            string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { selectedFolderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                customIcons.Add(path);
            }
            Repaint();
        }

        private void DrawCustomIcons()
        {
            if (string.IsNullOrEmpty(selectedFolderPath)) return;

            string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { selectedFolderPath });
            EditorGUILayout.BeginHorizontal();
            int iconsPerRow = 6;
            int currentInRow = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(searchString) || 
                    Path.GetFileNameWithoutExtension(path).ToLower().Contains(searchString.ToLower()))
                {
                    Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (icon != null)
                    {
                        if (currentInRow >= iconsPerRow)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            currentInRow = 0;
                        }

                        if (GUILayout.Button(new GUIContent(icon, path), GUILayout.Width(32), GUILayout.Height(32)))
                        {
                            AddToRecentIcons(path);
                            onIconSelected?.Invoke(path);
                            Close();
                        }
                        currentInRow++;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void LoadRecentIcons()
        {
            recentIcons.Clear();
            string savedIcons = EditorPrefs.GetString(RECENT_ICONS_KEY, "");
            if (!string.IsNullOrEmpty(savedIcons))
            {
                recentIcons.AddRange(savedIcons.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void AddToRecentIcons(string iconName)
        {
            recentIcons.Remove(iconName); // Remove if already exists
            recentIcons.Insert(0, iconName); // Add to start
            
            if (recentIcons.Count > MAX_RECENT_ICONS)
            {
                recentIcons.RemoveRange(MAX_RECENT_ICONS, recentIcons.Count - MAX_RECENT_ICONS);
            }
            
            EditorPrefs.SetString(RECENT_ICONS_KEY, string.Join(",", recentIcons));
        }
    }

    [CustomEditor(typeof(HierarchyColorID))]
    public class HierarchyColorIDEditor : Editor
    {
        // Override the OnInspectorGUI method to draw nothing
        public override void OnInspectorGUI()
        {
            // Draw nothing in the inspector
        }
    }           
}

