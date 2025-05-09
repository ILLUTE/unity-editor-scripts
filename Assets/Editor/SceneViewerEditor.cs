using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class SceneViewerEditor : EditorWindow
{
    private Vector2 scrollPos;
    private Dictionary<string, List<string>> folderToScenes;
    private Dictionary<string, bool> folderFoldouts = new Dictionary<string, bool>();

    [MenuItem("Tools/Scene Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneViewerEditor>("Scene Viewer");
        window.LoadScenes();
    }

    private void LoadScenes()
    {
        folderToScenes = new Dictionary<string, List<string>>();

        string projectPath = Path.GetFullPath(Application.dataPath);
        var allScenePaths = AssetDatabase.FindAssets("t:Scene")
      .Select(AssetDatabase.GUIDToAssetPath)
      .Where(path => path.StartsWith("Assets/"))
      .ToArray();


        foreach (var scenePath in allScenePaths)
        {
            string folder = Path.GetDirectoryName(scenePath).Replace("\\", "/");
            if (!folderToScenes.ContainsKey(folder))
            {
                folderToScenes[folder] = new List<string>();
            }
            folderToScenes[folder].Add(scenePath);
        }
    }

    private void OnGUI()
    {
        float totalWidth = position.width;
        float labelWidth = 130f;
        float buttonWidth = 80f;
        float spacing = 10f;
        float totalContentWidth = labelWidth + spacing + buttonWidth;
        float leftPadding = (totalWidth - totalContentWidth) / 2f;

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(leftPadding);

        if (GUILayout.Button("Refresh", GUILayout.Width(buttonWidth)))
        {
            LoadScenes();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Scenes by Folder", EditorStyles.boldLabel);

        if (folderToScenes == null || folderToScenes.Count == 0)
        {
            EditorGUILayout.HelpBox("No scenes found!", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var kvp in folderToScenes.OrderBy(f => f.Key))
        {
            string folder = kvp.Key;
            List<string> scenes = kvp.Value;

            if (!folderFoldouts.ContainsKey(folder))
                folderFoldouts[folder] = true;

            folderFoldouts[folder] = EditorGUILayout.Foldout(folderFoldouts[folder], folder, true);

            if (folderFoldouts[folder])
            {
                EditorGUI.indentLevel++;
                foreach (var scenePath in scenes.OrderBy(s => s))
                {
                    DrawSceneRow(scenePath);
                }
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSceneRow(string scenePath)
    {
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        bool isOpen = IsSceneOpen(scenePath);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(sceneName);

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
