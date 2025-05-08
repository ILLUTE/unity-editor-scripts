using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public class SceneViewerEditor : EditorWindow
{
    private Vector2 scrollPos;
    private string[] scenePaths;

    [MenuItem("Tools/Scene Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneViewerEditor>("Scene Viewer");
        window.LoadScenes();
    }

    private void LoadScenes()
    {
        string projectPath = Path.GetFullPath(Application.dataPath);
        scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => Path.GetFullPath(path).StartsWith(projectPath))
            .ToArray();
    }


    private void OnGUI()
    {
        EditorGUILayout.LabelField("All Project Scenes", EditorStyles.boldLabel);

        if (scenePaths == null || scenePaths.Length == 0)
        {
            EditorGUILayout.HelpBox("No scenes found!", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (string scenePath in scenePaths)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            bool isOpen = IsSceneOpen(scenePath);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(isOpen ?$"[{sceneName}]" : sceneName);

            Color originalColor = GUI.backgroundColor;
            if (isOpen)
            {
                GUI.backgroundColor = Color.green;
                GUI.enabled = false;
                GUILayout.Button("Opened", GUILayout.Width(60));
                GUI.enabled = true;
            }
            else
            {
                GUI.backgroundColor = Color.gray;
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    }
                }
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }


    private bool IsSceneOpen(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene openScene = SceneManager.GetSceneAt(i);
            if (openScene.path == scenePath)
                return true;
        }
        return false;
    }
}
