using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace CodeSketch.Editor.Scriptable
{
    internal class EndNameEdit : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(pathName));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// ScriptableObject browser window.
    /// Automatically finds all ScriptableObjects inside CodeSketch assemblies.
    /// Works in Assets folder AND Packages (Git URL / UPM).
    /// </summary>
    public class ScriptableObjectWindow : EditorWindow
    {
        string _search = "";
        Vector2 _scroll;

        Type[] _types;
        string[] _names;

        bool _focused;

        public Type[] Types
        {
            get => _types;
            set => _types = value;
        }

        // =========================================================
        // MENU
        // =========================================================

        [MenuItem("Tools/CodeSketch/ScriptableObject Browser")]
        public static void Open()
        {
            var window = GetWindow<ScriptableObjectWindow>();
            window.titleContent = new GUIContent("ScriptableObjects");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        // =========================================================
        // INIT
        // =========================================================

        void OnEnable()
        {
            LoadTypes();
        }

        void LoadTypes()
        {
            // Unity internal cached type system (works with Packages)
            _types = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith("CodeSketch") // filter namespace
                )
                .OrderBy(t => t.Name)
                .ToArray();

            _names = _types.Select(t => t.FullName).ToArray();

            Debug.Log($"[CodeSketch] Found {_types.Length} ScriptableObject types.");
        }

        // =========================================================
        // GUI
        // =========================================================

        void OnGUI()
        {
            DrawSearchBar();

            if (_types == null || _types.Length == 0)
            {
                EditorGUILayout.HelpBox("No ScriptableObject types found.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _types.Length; i++)
            {
                if (!string.IsNullOrEmpty(_search) &&
                    _types[i].Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                DrawTypeButton(i);
            }

            EditorGUILayout.EndScrollView();

            if (!_focused)
            {
                GUI.FocusControl("SearchField");
                _focused = true;
            }
        }

        void DrawSearchBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.SetNextControlName("SearchField");

            _search = GUILayout.TextField(
                _search,
                GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.textField
            );

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                LoadTypes();
            }

            GUILayout.EndHorizontal();
        }

        void DrawTypeButton(int index)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_types[index].Name, GUILayout.Height(22)))
            {
                CreateAssetOfType(_types[index], _names[index]);
                Close();
            }

            GUILayout.EndHorizontal();
        }

        // =========================================================
        // CREATE
        // =========================================================

        void CreateAssetOfType(Type type, string fullName)
        {
            var asset = CreateInstance(type);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                CreateInstance<EndNameEdit>(),
                $"{type.Name}.asset",
                AssetPreview.GetMiniThumbnail(asset),
                null
            );
        }
    }
}