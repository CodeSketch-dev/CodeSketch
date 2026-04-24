using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace CodeSketch.Editor
{
    public class Window_BatchMaterialInstancing : OdinEditorWindow
    {
        readonly List<Material> _materials = new List<Material>();

        [Title("Input")]
        [LabelText("Material")]
        Material _singleMaterial;

        [LabelText("Folder")]
        DefaultAsset _folderAsset;

        Vector2 _scroll;

        static MethodInfo _hasInstancingMethod;

        [MenuItem("CodeSketch/Tools/Window/Batch Material Instancing")]
        static void Open()
        {
            var window = GetWindow<Window_BatchMaterialInstancing>("Batch Material Instancing");
            window.minSize = new Vector2(700f, 400f);
            window.Show();
        }

        protected override void OnGUI()
        {
            DrawAddSection();
            EditorGUILayout.Space(8f);
            DrawDropArea();
            EditorGUILayout.Space(8f);
            DrawActionSection();
            EditorGUILayout.Space(8f);
            DrawListSection();
        }

        void DrawAddSection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _singleMaterial = (Material)EditorGUILayout.ObjectField("Material", _singleMaterial, typeof(Material), false);
                    using (new EditorGUI.DisabledScope(_singleMaterial == null))
                    {
                        if (GUILayout.Button("Add Material", GUILayout.Width(120f)))
                        {
                            AddMaterial(_singleMaterial);
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder", _folderAsset, typeof(DefaultAsset), false);
                    using (new EditorGUI.DisabledScope(_folderAsset == null))
                    {
                        if (GUILayout.Button("Add Folder", GUILayout.Width(120f)))
                        {
                            AddMaterialsFromFolder(_folderAsset);
                        }
                    }
                }
            }
        }

        void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0f, 70f, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "Drag & Drop Materials / Folders Here", EditorStyles.helpBox);

            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        var obj = DragAndDrop.objectReferences[i];
                        if (obj is Material mat)
                        {
                            AddMaterial(mat);
                            continue;
                        }

                        if (obj is DefaultAsset folder)
                        {
                            AddMaterialsFromFolder(folder);
                        }
                    }
                }

                evt.Use();
            }
        }

        void DrawActionSection()
        {
            int compatible = 0;
            int enabled = 0;

            for (int i = 0; i < _materials.Count; i++)
            {
                var mat = _materials[i];
                if (mat == null)
                    continue;

                if (SupportsInstancing(mat.shader))
                {
                    compatible++;
                    if (mat.enableInstancing)
                        enabled++;
                }
            }

            EditorGUILayout.LabelField($"Loaded: {_materials.Count} materials | Compatible: {compatible} | Instancing ON: {enabled}", EditorStyles.helpBox);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_materials.Count == 0))
                {
                    if (GUILayout.Button("Tick All GPU Instancing", GUILayout.Height(28f)))
                    {
                        SetInstancingForAll(true);
                    }
                }

                using (new EditorGUI.DisabledScope(_materials.Count == 0))
                {
                    if (GUILayout.Button("Untick All", GUILayout.Height(28f)))
                    {
                        SetInstancingForAll(false);
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Height(28f)))
                {
                    ResetWindow();
                }
            }
        }

        void DrawListSection()
        {
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _materials.Count; i++)
            {
                Material mat = _materials[i];
                if (mat == null)
                    continue;

                DrawMaterialRow(i, mat);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawMaterialRow(int index, Material mat)
        {
            bool supported = SupportsInstancing(mat.shader);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label((index + 1).ToString(), GUILayout.Width(30f));

                if (GUILayout.Button(mat.name, EditorStyles.linkLabel, GUILayout.Width(220f)))
                {
                    Selection.activeObject = mat;
                    EditorGUIUtility.PingObject(mat);
                    EditorUtility.FocusProjectWindow();
                }

                EditorGUILayout.ObjectField(mat, typeof(Material), false);

                using (new EditorGUI.DisabledScope(!supported))
                {
                    bool next = EditorGUILayout.ToggleLeft("GPU Instancing", mat.enableInstancing, GUILayout.Width(120f));
                    if (next != mat.enableInstancing)
                    {
                        Undo.RecordObject(mat, "Change Material Instancing");
                        mat.enableInstancing = next;
                        EditorUtility.SetDirty(mat);
                    }
                }

                GUILayout.Label(supported ? "Supported" : "Not Supported", GUILayout.Width(100f));

                if (GUILayout.Button("X", GUILayout.Width(24f)))
                {
                    _materials.RemoveAt(index);
                    GUIUtility.ExitGUI();
                }
            }
        }

        [Button("Add Material")]
        void AddMaterial(Material mat)
        {
            if (mat == null)
                return;

            if (!_materials.Contains(mat))
            {
                _materials.Add(mat);
            }
        }

        [Button("Add Folder")]
        void AddMaterialsFromFolder(DefaultAsset folder)
        {
            if (folder == null)
                return;

            string folderPath = AssetDatabase.GetAssetPath(folder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                AddMaterial(mat);
            }
        }

        void SetInstancingForAll(bool enabled)
        {
            int changed = 0;

            for (int i = 0; i < _materials.Count; i++)
            {
                var mat = _materials[i];
                if (mat == null)
                    continue;

                if (!SupportsInstancing(mat.shader))
                    continue;

                if (mat.enableInstancing == enabled)
                    continue;

                Undo.RecordObject(mat, "Batch Change Material Instancing");
                mat.enableInstancing = enabled;
                EditorUtility.SetDirty(mat);
                changed++;
            }

            if (changed > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"Batch Material Instancing: Updated {changed} material(s). Set enableInstancing = {enabled}.");
        }

        [Button(ButtonSizes.Medium)]
        void ResetWindow()
        {
            _singleMaterial = null;
            _folderAsset = null;
            _materials.Clear();
            _scroll = Vector2.zero;
        }

        static bool SupportsInstancing(Shader shader)
        {
            if (shader == null)
                return false;

            // Use ShaderUtil.HasInstancing via reflection to avoid API differences across Unity versions.
            if (_hasInstancingMethod == null)
            {
                Type shaderUtil = Type.GetType("UnityEditor.ShaderUtil, UnityEditor");
                _hasInstancingMethod = shaderUtil?.GetMethod(
                    "HasInstancing",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Shader) },
                    null);
            }

            if (_hasInstancingMethod != null)
            {
                try
                {
                    object value = _hasInstancingMethod.Invoke(null, new object[] { shader });
                    if (value is bool result)
                    {
                        return result;
                    }
                }
                catch
                {
                    // Ignore reflection errors and fallback below.
                }
            }

            return true;
        }
    }
}
