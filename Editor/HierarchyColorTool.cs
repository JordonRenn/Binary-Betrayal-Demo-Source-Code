using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;



namespace HierarchyTool
{
    [InitializeOnLoad]
    public class HierarchyColorTool
    {
        private static Dictionary<int, Color> objectColors = new Dictionary<int, Color>();
        private static Dictionary<int, Color> textColors = new Dictionary<int, Color>();
        private const string PREF_PREFIX = "HierarchyColor_";
        private const string PREF_TEXT_PREFIX = "HierarchyTextColor_";
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
            var persistentID = obj.GetComponent<HierarchyColorID>();
            if (persistentID == null)
            {
                Undo.AddComponent<HierarchyColorID>(obj); // Make component addition undoable
                persistentID = obj.GetComponent<HierarchyColorID>();
                EditorUtility.SetDirty(obj); // Mark object as dirty to ensure save
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
            if (objectColors.TryGetValue(colorKey, out Color bgColor))
            {
                EditorGUI.DrawRect(selectionRect, bgColor);
            }

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

            // Save IDs
            string objectIDs = objectColors.Count > 0 ? string.Join(",", objectColors.Keys) : "";
            string textIDs = textColors.Count > 0 ? string.Join(",", textColors.Keys) : "";

            EditorPrefs.SetString(PREF_ID_LIST + sceneName + "_bg", objectIDs);
            EditorPrefs.SetString(PREF_ID_LIST + sceneName + "_text", textIDs);

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
        }

        private static void LoadColorData()
        {
            objectColors.Clear();
            textColors.Clear();

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

            EditorApplication.RepaintHierarchyWindow();
        }
    }

    // Color Picker Window
    public static class ColorPicker
    {
        public static void Show(System.Action<Color> onColorSelected, Color defaultColor)
        {
            EditorWindow colorWindow = EditorWindow.GetWindow(typeof(ColorPickerWindow));
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

        private void OnGUI()
        {
            selectedColor = EditorGUILayout.ColorField("Pick a color", selectedColor);

            if (GUILayout.Button("Apply"))
            {
                OnColorSelected?.Invoke(selectedColor);
                Close();
            }
        }
    }

    public class HierarchyColorID : MonoBehaviour
    {
        public string ID = System.Guid.NewGuid().ToString();
    }
}
