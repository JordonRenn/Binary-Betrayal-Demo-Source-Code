using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

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
        // Delay the initial load to ensure Unity is fully initialized
        EditorApplication.delayCall += LoadColorData;
    }

    private static Color GetInverseColor(Color color)
    {
        return new Color(1f - color.r, 1f - color.g, 1f - color.b, 1f);
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
                int instanceID = obj.GetInstanceID();
                objectColors[instanceID] = color;
                
                // If no custom text color exists, set it to the inverse of background
                if (!textColors.ContainsKey(instanceID))
                {
                    textColors[instanceID] = GetInverseColor(color);
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
                int instanceID = obj.GetInstanceID();
                textColors[instanceID] = color;
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
            int instanceID = obj.GetInstanceID();
            objectColors.Remove(instanceID);
            textColors.Remove(instanceID);
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
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj == null) return;

        if (objectColors.TryGetValue(instanceID, out Color bgColor))
        {
            EditorGUI.DrawRect(selectionRect, bgColor);
        }

        if (textColors.TryGetValue(instanceID, out Color textColor))
        {
            string name = obj.name;
            EditorGUI.LabelField(selectionRect, name, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = textColor },
                padding = new RectOffset(EditorStyles.label.padding.left + 18, 0, 0, 0)
            });
        }
    }

    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        LoadColorData();
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

        //Debug.Log($"Saving colors for scene: {sceneName}");
        //Debug.Log($"Background color IDs: {objectIDs}");
        //Debug.Log($"Text color IDs: {textIDs}");

        // Save colors
        foreach (var entry in objectColors)
        {
            string colorStr = ColorUtility.ToHtmlStringRGBA(entry.Value);
            EditorPrefs.SetString(PREF_PREFIX + sceneName + "_" + entry.Key, colorStr);
            //Debug.Log($"Saved bg color for ID {entry.Key}: {colorStr}");
        }

        foreach (var entry in textColors)
        {
            string colorStr = ColorUtility.ToHtmlStringRGBA(entry.Value);
            EditorPrefs.SetString(PREF_TEXT_PREFIX + sceneName + "_" + entry.Key, colorStr);
            //Debug.Log($"Saved text color for ID {entry.Key}: {colorStr}");
        }
    }

    private static void LoadColorData()
    {
        objectColors.Clear();
        textColors.Clear();

        string sceneName = EditorSceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName)) return;

        //Debug.Log($"Loading colors for scene: {sceneName}");

        // Load background colors
        string objectIDsList = EditorPrefs.GetString(PREF_ID_LIST + sceneName + "_bg", "");
        //Debug.Log($"Found background color IDs: {objectIDsList}");

        if (!string.IsNullOrEmpty(objectIDsList))
        {
            foreach (string idStr in objectIDsList.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(idStr, out int id))
                {
                    string colorStr = EditorPrefs.GetString(PREF_PREFIX + sceneName + "_" + id, "");
                    if (ColorUtility.TryParseHtmlString("#" + colorStr, out Color color))
                    {
                        objectColors[id] = color;
                        //Debug.Log($"Loaded bg color for ID {id}: #{colorStr}");
                    }
                }
            }
        }

        // Load text colors
        string textIDsList = EditorPrefs.GetString(PREF_ID_LIST + sceneName + "_text", "");
        //Debug.Log($"Found text color IDs: {textIDsList}");

        if (!string.IsNullOrEmpty(textIDsList))
        {
            foreach (string idStr in textIDsList.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(idStr, out int id))
                {
                    string colorStr = EditorPrefs.GetString(PREF_TEXT_PREFIX + sceneName + "_" + id, "");
                    if (ColorUtility.TryParseHtmlString("#" + colorStr, out Color color))
                    {
                        textColors[id] = color;
                        //Debug.Log($"Loaded text color for ID {id}: #{colorStr}");
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
