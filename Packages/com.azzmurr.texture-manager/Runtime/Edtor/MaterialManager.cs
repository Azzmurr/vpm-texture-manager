using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Azzmurr.Utils
{
    class MaterialMeneger : EditorWindow
    {
        [MenuItem("Azzmurr/Avatar/Material Manager")]
        public static void Init()
        {
            MaterialMeneger window = (MaterialMeneger)EditorWindow.GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window.Show();
        }

        [MenuItem("GameObject/Azzmurr/Avatar/Material Manager", true, 0)]
        static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Azzmurr/Avatar/Material Manager", false, 0)]
        public static void ShowFromSelection()
        {
            MaterialMeneger window = (MaterialMeneger)EditorWindow.GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window.Avatar = new AvatarMeta(Selection.activeGameObject);
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            MaterialMeneger window = (MaterialMeneger)EditorWindow.GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window.Avatar = new AvatarMeta(avatar);
            window.Show();
        }

        AvatarMeta Avatar;
        Vector2 MainScrollPosition;
        GUIContent RefreshIcon;

        GUIStyle label;
        GUIStyle validLabel;
        GUIStyle invalidLabel;

        private void OnEnable()
        {
            RefreshIcon = EditorGUIUtility.IconContent("RotateTool On", "Recalculate");
        }

        private void OnGUI()
        {
            label = new GUIStyle(EditorStyles.label);

            validLabel = new GUIStyle(EditorStyles.label);
            validLabel.normal.textColor = Color.green;

            invalidLabel = new GUIStyle(EditorStyles.label);
            invalidLabel.normal.textColor = Color.red;

            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                bool refresh = GUILayout.Button(RefreshIcon, GUILayout.Width(30), GUILayout.Height(30));
                if (refresh && Avatar != null) Avatar.Recalculate();

                GameObject gameObject = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, Avatar?.GameObject, typeof(GameObject), true, GUILayout.Height(30));

                if (Avatar == null || Avatar.GameObject != gameObject)
                {
                    Avatar = gameObject ? new AvatarMeta(gameObject) : null;
                }
            }

            if (Avatar != null)
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(MainScrollPosition))
                {
                    MainScrollPosition = scroll.scrollPosition;
                    GUILine();
                    EditorGUILayout.Space();

                    if (Avatar.MaterialsCount > 0)
                    {
                        VariableGridScope ActionGrid = new(new float[] { 200, 200 });

                        using (ActionGrid)
                        {
                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Update poiyomi to latest version"))
                                {

                                }
                            });

                            ActionGrid.Cell((index) => GUILayout.Label("I do not work for now :(", label));
                        }

                        GUILine();
                        EditorGUILayout.Space();

                        VariableGridScope ResultsGrid = new(new float[] { 72, 200, 200 });

                        using (ResultsGrid)
                        {
                            ResultsGrid.Cell((index) => GUILayout.Label("Preview", label));
                            ResultsGrid.Cell((index) => GUILayout.Label("Info", label));
                            ResultsGrid.Cell((index) => GUILayout.Label("Textures", label));


                            Avatar.ForeachMaterial((material) =>
                            {
                                ResultsGrid.Cell((index) =>
                                {
                                    Texture2D preview = AssetPreview.GetAssetPreview(material.Material);
                                    EditorGUILayout.ObjectField(GUIContent.none, preview, typeof(Texture2D), false, GUILayout.Width(72), GUILayout.Height(72));

                                });

                                ResultsGrid.Cell((index) =>
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.ObjectField(material.Material, typeof(Material), false);
                                        GUILayout.Label($"Shader - {material.ShaderName}", label);
                                        GUILayout.Label($"Locked - {material.ShaderLocked}", material.ShaderLockedError ? invalidLabel : validLabel);
                                        GUILayout.Label($"Version - {material.ShaderVersion}", material.ShaderVersionError ? invalidLabel : validLabel);
                                    }
                                });

                                ResultsGrid.Cell((index) =>
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        material.ForeachTexture((texture) =>
                                        {
                                            EditorGUILayout.ObjectField(GUIContent.none, texture.texture, typeof(Texture), false, GUILayout.Width(72), GUILayout.Height(72));
                                        });
                                    }

                                });
                            });
                        }
                    }
                }
            }
        }

        static void GUILine(int i_height = 1)
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            if (rect != null)
            {
                rect.width = EditorGUIUtility.currentViewWidth;
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            }
            GUILayout.Space(10);
        }
    }
}