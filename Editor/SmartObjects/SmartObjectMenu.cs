#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SmartObjectMenu
{
    [MenuItem("GameObject/Smart Objects/Wall", false, 10)]
    private static void CreateWallSmartObject(MenuCommand command)
    {
        // Create empty root
        var go = new GameObject("WallSmartObject");
        var smart = go.AddComponent<WallSmartObject>();

        // Register undo
        Undo.RegisterCreatedObjectUndo(go, "Create Wall Smart Object");

        // Parenting (if right-click was on a GameObject in hierarchy)
        GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);

        Selection.activeObject = go;
    }
}
#endif
