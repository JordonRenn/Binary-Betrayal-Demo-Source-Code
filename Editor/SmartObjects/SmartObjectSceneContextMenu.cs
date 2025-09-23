// Assets/Editor/SmartObjectEditorMenus.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SmartObjectEditorMenus
{
    static SmartObjectEditorMenus()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("GameObject/Smart Objects/Wall", false, 10)]
    private static void CreateWallFromHierarchy(MenuCommand menuCommand)
    {
        var go = new GameObject("WallSmartObject");
        Undo.RegisterCreatedObjectUndo(go, "Create Wall Smart Object");
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        go.AddComponent<WallSmartObject>();
        Selection.activeObject = go;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Detect Space key (no modifiers)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space && !e.shift && !e.control && !e.alt)
        {
            // Convert mouse position to ray
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 spawnPos;
            Vector3 spawnNormal;

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                spawnPos = hit.point;
                spawnNormal = hit.normal;
            }
            else
            {
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                if (ground.Raycast(ray, out float enter))
                {
                    spawnPos = ray.GetPoint(enter);
                    spawnNormal = Vector3.up;
                }
                else
                {
                    spawnPos = sceneView.pivot;
                    spawnNormal = Vector3.up;
                }
            }

            // Build context menu
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Smart Objects/Wall"), false, () =>
            {
                CreateWallAt(spawnPos, spawnNormal, sceneView);
            });

            menu.ShowAsContext();
            e.Use(); // swallow space event so it doesn’t recenter pivot
        }
    }

    private static void CreateWallAt(Vector3 position, Vector3 normal, SceneView sceneView)
    {
        var go = new GameObject("WallSmartObject");
        go.transform.position = position;

        Vector3 camForward = sceneView.camera ? sceneView.camera.transform.forward : Vector3.forward;
        Vector3 forwardProj = Vector3.ProjectOnPlane(camForward, normal);
        if (forwardProj.sqrMagnitude < 0.001f) forwardProj = Vector3.Cross(normal, Vector3.right);
        go.transform.rotation = Quaternion.LookRotation(forwardProj.normalized, normal);

        go.AddComponent<WallSmartObject>();

        Undo.RegisterCreatedObjectUndo(go, "Create Wall Smart Object");
        Selection.activeObject = go;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
#endif
