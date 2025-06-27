#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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
                                if (GUILayout.Button("Update poiyomi to latest version")) Debug.Log("FFFFFFFFFFFFFFF");
                            });

                            ActionGrid.Cell((index) => GUILayout.Label("I do not work for now :(", label));

                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Textures max size -> 2k")) Avatar.MakeAllTextures2k();
                            });
                            ActionGrid.Cell((index) => GUILayout.Label("Makes max texture size 2k"));

                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Prepare textures for Android")) Avatar.MakeTexturesReadyForAndroid();
                            });
                            ActionGrid.Cell((index) => GUILayout.Label("Makes max texture for android half size of PC size"));

                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Crunch textures")) Avatar.CrunchTextures();
                            });
                            ActionGrid.Cell((index) => GUILayout.Label("Sets texture format to DTX1Crunched or DTX5Crunched"));

                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Create Quest Material Presets")) Avatar.CreateQuestMaterialPresets();
                            });
                            ActionGrid.Cell((index) => GUILayout.Label("This will create quest materilas with VRChat/Mobile/Standard Lite shader"));
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
                                        GUILayout.Label($"Locked - {material.ShaderLockedString}", material.ShaderLockedError == null ? label : material.ShaderLockedError == true ? invalidLabel : validLabel);
                                        GUILayout.Label($"Version - {material.ShaderVersion}", material.ShaderVersionError == null ? label : material.ShaderVersionError == true ? invalidLabel : validLabel);
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
#endif