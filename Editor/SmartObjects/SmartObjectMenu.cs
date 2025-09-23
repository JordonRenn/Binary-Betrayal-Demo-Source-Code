#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SBG.SmartObjects.Editor
{

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

        [MenuItem("GameObject/Smart Objects/Tunnel", false, 10)]
        private static void CreateTunnelSmartObject(MenuCommand command)
        {
            // Create empty root
            var go = new GameObject("TunnelSmartObject");
            var smart = go.AddComponent<TunnelSmartObject>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Tunnel Smart Object");

            // Parenting (if right-click was on a GameObject in hierarchy)
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);

            Selection.activeObject = go;
        }

        [MenuItem("GameObject/Smart Objects/Smart Object Group", false, 10)]
        private static void CreateSmartObjectGroup(MenuCommand command)
        {
            // Create empty root
            var go = new GameObject("SmartObjectGroup");
            var smart = go.AddComponent<SmartObjectGroup>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Smart Object Group");

            // Parenting (if right-click was on a GameObject in hierarchy)
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);

            Selection.activeObject = go;
        }
    }
}
#endif
