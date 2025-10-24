using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Saus.StartSceneTool
{
    public class StartSceneSelector : EditorWindow
    {
        private SceneAsset sceneAsset;
        private const string SCENE_PREF_KEY = "StartSceneGUID";

        void OnEnable()
        {
            string sceneGUID = EditorPrefs.GetString(SCENE_PREF_KEY, "");
            if (!string.IsNullOrEmpty(sceneGUID))
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                    EditorSceneManager.playModeStartScene = sceneAsset;
                }
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Set Play Mode Start Scene", EditorStyles.boldLabel);

            SceneAsset previousScene = sceneAsset;
            sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(
                "Start Scene",
                sceneAsset,
                typeof(SceneAsset),
                false);

            if (sceneAsset != previousScene)
            {
                SaveSelectedScene();
            }

            if (GUILayout.Button("Set as Start Scene"))
            {
                SaveSelectedScene();
            }
        }

        private void SaveSelectedScene()
        {
            if (sceneAsset != null)
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));
                EditorPrefs.SetString(SCENE_PREF_KEY, guid);
                EditorSceneManager.playModeStartScene = sceneAsset;
                Debug.Log($"Start scene set to: {sceneAsset.name}");
            }
            else
            {
                EditorPrefs.DeleteKey(SCENE_PREF_KEY);
                EditorSceneManager.playModeStartScene = null;
                Debug.LogWarning("Please select a scene first!");
            }
        }

        [MenuItem("Tools/Saus/Set Start Scene")]
        static void Open()
        {
            GetWindow<StartSceneSelector>("Set Start Scene");
        }
    }
}