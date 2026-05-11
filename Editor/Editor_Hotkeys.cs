#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class Editor_Hotkeys
{
    static Editor_Hotkeys()
    {
        // Khởi tạo khi Unity mở, không cần code trong đây nếu không dùng
    }

    // Ctrl + Shift + Number
    [MenuItem("Tools/⚙QuickScene/Open Scene 1 #1", false, int.MaxValue)]
    static void OpenScene1() => OpenSceneByBuildIndex(0);

    [MenuItem("Tools/⚙QuickScene/Open Scene 2 #2", false, int.MaxValue)]
    static void OpenScene2() => OpenSceneByBuildIndex(1);

    [MenuItem("Tools/⚙QuickScene/Open Scene 3 #3", false, int.MaxValue)]
    static void OpenScene3() => OpenSceneByBuildIndex(2);

    [MenuItem("Tools/⚙QuickScene/Open Scene 4 #4", false, int.MaxValue)]
    static void OpenScene4() => OpenSceneByBuildIndex(3);

    [MenuItem("Tools/⚙QuickScene/Open Scene 5 #5", false, int.MaxValue)]
    static void OpenScene5() => OpenSceneByBuildIndex(4);

    [MenuItem("Tools/⚙QuickScene/Open Scene 6 #6", false, int.MaxValue)]
    static void OpenScene6() => OpenSceneByBuildIndex(5);

    [MenuItem("Tools/⚙QuickScene/Open Scene 7 #7", false, int.MaxValue)]
    static void OpenScene7() => OpenSceneByBuildIndex(6);

    [MenuItem("Tools/⚙QuickScene/Open Scene 8 #8", false, int.MaxValue)]
    static void OpenScene8() => OpenSceneByBuildIndex(7);

    [MenuItem("Tools/⚙QuickScene/Open Scene 9 #9", false, int.MaxValue)]
    static void OpenScene9() => OpenSceneByBuildIndex(8);

    static void OpenSceneByBuildIndex(int index)
    {
        var scenes = EditorBuildSettings.scenes;
        if (index >= scenes.Length)
        {
            Debug.LogWarning($"❌ Không có scene ở index {index} trong Build Settings.");
            return;
        }

        string path = scenes[index].path;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
            Debug.Log($"✅ Đã mở scene: {path}");
        }
    }

    // Shift + S: Open scene picker window with scrollable list
    [MenuItem("Tools/⚙QuickScene/Open Scene List #s", false, int.MaxValue)]
    static void OpenSceneListWindow() => QuickSceneWindow.ShowWindow();

    class QuickSceneWindow : EditorWindow
    {
        Vector2 scroll;

        public static void ShowWindow()
        {
            var w = GetWindow<QuickSceneWindow>(true, "Quick Scenes", true);
            w.minSize = new Vector2(300, 200);
            w.Focus();
        }

        void OnGUI()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                EditorGUILayout.HelpBox("Build Settings hiện không có scene nào. Vào File > Build Settings... để thêm.", MessageType.Info);
                if (GUILayout.Button("Open Build Settings")) EditorApplication.ExecuteMenuItem("File/Build Settings...");
                return;
            }

            EditorGUILayout.LabelField($"Scenes in Build Settings ({scenes.Length})", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < scenes.Length; i++)
            {
                var s = scenes[i];
                string name = Path.GetFileNameWithoutExtension(s.path);
                EditorGUILayout.BeginHorizontal();
                if (!s.enabled)
                {
                    GUI.color = Color.gray;
                }
                if (GUILayout.Button($"[{i}] {name}", GUILayout.Height(22)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(s.path);
                        Close();
                    }
                }
                GUI.color = Color.white;

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(s.enabled ? "Enabled" : "Disabled", GUILayout.Width(70));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh")) Repaint();
            if (GUILayout.Button("Open Build Settings")) EditorApplication.ExecuteMenuItem("File/Build Settings...");
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif