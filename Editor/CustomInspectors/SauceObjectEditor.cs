using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SauceObject), true)]
public class SauceObjectEditor : Editor
{
    private SerializedProperty objectID;
    private SerializedProperty objectDisplayName;
    private SerializedProperty isInteractable;
    private SerializedProperty isTrackable;
    
    // Tracking properties
    private SerializedProperty nav_CompassIcon;
    private SerializedProperty compassImage;
    private SerializedProperty nav_CompassDrawDistance;
    private SerializedProperty iconColorState;
    private SerializedProperty defaultColor;
    private SerializedProperty lockedColor;
    private SerializedProperty highlightedColor;

    private bool trackingFoldout = true;

    void OnEnable()
    {
        // Cache all serialized properties
        objectID = serializedObject.FindProperty("objectID");
        objectDisplayName = serializedObject.FindProperty("objectDisplayName");
        isInteractable = serializedObject.FindProperty("isInteractable");
        isTrackable = serializedObject.FindProperty("isTrackable");
        
        // Tracking properties
        nav_CompassIcon = serializedObject.FindProperty("nav_CompassIcon");
        compassImage = serializedObject.FindProperty("compassImage");
        nav_CompassDrawDistance = serializedObject.FindProperty("nav_CompassDrawDistance");
        iconColorState = serializedObject.FindProperty("iconColorState");
        defaultColor = serializedObject.FindProperty("defaultColor");
        lockedColor = serializedObject.FindProperty("lockedColor");
        highlightedColor = serializedObject.FindProperty("highlightedColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Object Properties section
        EditorGUILayout.LabelField("Sauce Object Properties", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.PropertyField(objectID);
        EditorGUILayout.PropertyField(objectDisplayName);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.PropertyField(isInteractable);
        EditorGUILayout.PropertyField(isTrackable);

        // Only show tracking section if isTrackable is true
        if (isTrackable.boolValue)
        {
            EditorGUILayout.Space(10);
            
            // Create a foldout for the tracking section
            trackingFoldout = EditorGUILayout.Foldout(trackingFoldout, "Trackable Properties", true, EditorStyles.foldoutHeader);
            
            if (trackingFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(nav_CompassIcon);
                EditorGUILayout.PropertyField(compassImage);
                EditorGUILayout.PropertyField(nav_CompassDrawDistance);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(iconColorState);
                EditorGUILayout.PropertyField(defaultColor);
                EditorGUILayout.PropertyField(lockedColor);
                EditorGUILayout.PropertyField(highlightedColor);
                
                EditorGUI.indentLevel--;
            }
        }

        // Display any additional properties from child classes
        DrawPropertiesExcluding(serializedObject, new string[]
        {
            "m_Script",
            "objectID",
            "objectDisplayName", 
            "isInteractable",
            "isTrackable",
            "nav_CompassIcon",
            "compassImage",
            "nav_CompassDrawDistance",
            "iconColorState",
            "defaultColor",
            "lockedColor",
            "highlightedColor"
        });

        serializedObject.ApplyModifiedProperties();
    }
}
