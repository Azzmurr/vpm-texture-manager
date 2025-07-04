using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Azzmurr.Utils
{
    class MaterialMeneger : EditorWindow
    {
        [MenuItem("Tools/Azzmurr/Material Manager")]
        public static void Init()
        {
            MaterialMeneger window = (MaterialMeneger)EditorWindow.GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window.Show();
        }

        [MenuItem("GameObject/Azzmurr/Material Manager", true, 0)]
        static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Azzmurr/Material Manager", false, 0)]
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

        bool MoreTextureInfo = false;

        GUIStyle label;
        GUIStyle validLabel;
        GUIStyle invalidLabel;

        int[] _textureSizeOptions = new int[] { 0, 128, 256, 512, 1024, 2048, 4096, 8192 };
        TextureImporterFormat[] _compressionFormatOptions = new TextureImporterFormat[]{
            TextureImporterFormat.Automatic,
            TextureImporterFormat.Automatic,
            TextureImporterFormat.BC7,
            TextureImporterFormat.DXT1,
            TextureImporterFormat.DXT5,
            TextureImporterFormat.DXT1Crunched,
            TextureImporterFormat.DXT5Crunched,
        };

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
                        using (var ActionGrid = new VariableGridScope(new float[] { 75, 75 }))
                        {
                            ActionGrid.Cell((index) => GUILayout.Label("Poiyomi", label));

                            ActionGrid.Cell((index) =>
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button("Unlock")) Avatar.UnlockMaterials();
                                    if (GUILayout.Button("Update")) Avatar.UpdateMaterials();
                                    if (GUILayout.Button("Lock")) Avatar.LockMaterials();
                                }
                            });

                            ActionGrid.Cell((index) =>
                            {
                                GUILayout.Label("Textures", label);
                            });

                            ActionGrid.Cell((index) =>
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button("-> 2k")) Avatar.MakeAllTextures2k();
                                    if (GUILayout.Button("Prepare textures for Android")) Avatar.MakeTexturesReadyForAndroid();
                                    if (GUILayout.Button("Crunch")) Avatar.CrunchTextures();
                                }
                            });

                            ActionGrid.Cell((index) =>
                            {
                                GUILayout.Label("Materials", label);
                            });

                            ActionGrid.Cell((index) =>
                            {
                                if (GUILayout.Button("Create Quest Presets")) Avatar.CreateQuestMaterialPresets();
                            });
                        }

                        GUILine();
                        EditorGUILayout.Space();

                        using (var ResultsGrid = new VariableGridScope(new float[] { 88, 200, 1 }, 8))
                        {
                            ResultsGrid.Cell((index) => GUILayout.Label("Preview", label));
                            ResultsGrid.Cell((index) => GUILayout.Label("Info", label));
                            ResultsGrid.Cell((index) =>
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.Space();
                                    GUILayout.Label("Textures", label);
                                    if (GUILayout.Button(MoreTextureInfo ? "Less Info" : "More Info"))
                                    {
                                        MoreTextureInfo = !MoreTextureInfo;
                                    }
                                }
                            });


                            Avatar.ForeachMaterial((material) =>
                            {
                                ResultsGrid.Cell((index) =>
                                {
                                    Texture2D preview = AssetPreview.GetAssetPreview(material.Material);
                                    EditorGUILayout.ObjectField(GUIContent.none, preview, typeof(Texture2D), false, GUILayout.Width(88), GUILayout.Height(88));
                                });

                                ResultsGrid.Cell((index) =>
                                {
                                    EditorGUILayout.ObjectField(material.Material, typeof(Material), false);
                                    EditorGUILayout.Space();
                                    using (var MaterialInfoGrid = new VariableGridScope(new float[] { 20, 50 }))
                                    {
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label("Shader:", label));
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label(material.ShaderName, label));
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label("Locked:", label));
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label(material.ShaderLockedString, material.ShaderLockedError == null ? label : material.ShaderLockedError == true ? invalidLabel : validLabel));
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label("Version:", label));
                                        MaterialInfoGrid.Cell((index) => GUILayout.Label(material.ShaderVersion, material.ShaderVersionError == null ? label : material.ShaderVersionError == true ? invalidLabel : validLabel));
                                    }
                                });

                                ResultsGrid.Cell((index) =>
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        material.ForeachTexture((texture) =>
                                        {
                                            EditorGUILayout.Space();
                                            using (new EditorGUILayout.HorizontalScope())
                                            {
                                                EditorGUILayout.ObjectField(GUIContent.none, texture.texture, typeof(Texture), false, GUILayout.Width(88), GUILayout.Height(88));

                                                if (MoreTextureInfo)
                                                {
                                                    using (new EditorGUILayout.VerticalScope())
                                                    {
                                                        using (var TextureSActions = new VariableGridScope(new float[] { 50, 100 }))
                                                        {
                                                            TextureSActions.Cell((index) => GUILayout.Label("PC:", label));
                                                            TextureSActions.Cell((index) =>
                                                            {
                                                                if (texture.TextureWithChangableResolution)
                                                                {
                                                                    _textureSizeOptions[0] = texture.PcResolution;
                                                                    int newResolution = EditorGUILayout.IntPopup(texture.PcResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                                                    if (newResolution != texture.PcResolution)
                                                                    {
                                                                        texture.ChangeImportSize(newResolution);
                                                                        Avatar.Recalculate();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    GUILayout.Label(texture.PcResolution.ToString());
                                                                }
                                                            });

                                                            TextureSActions.Cell((index) => GUILayout.Label("Android:", label));
                                                            TextureSActions.Cell((ANDROID) =>
                                                            {
                                                                if (texture.TextureWithChangableResolution)
                                                                {
                                                                    _textureSizeOptions[0] = texture.AndroidResolution;
                                                                    int newResolutionAndroid = EditorGUILayout.IntPopup(texture.AndroidResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                                                    if (newResolutionAndroid != texture.AndroidResolution)
                                                                    {
                                                                        texture.ChangeImportSizeAndroid(newResolutionAndroid);
                                                                        Avatar.Recalculate();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    GUILayout.Label(texture.AndroidResolution.ToString());
                                                                }
                                                            });

                                                            TextureSActions.Cell((index) => GUILayout.Label("Format:", label));
                                                            TextureSActions.Cell((Format) =>
                                                            {
                                                                if (texture.FormatString.Length > 0)
                                                                {
                                                                    if (texture.TextureWithChangableFormat)
                                                                    {
                                                                        _compressionFormatOptions[0] = ((TextureImporterFormat)texture.Format);
                                                                        int newFormat = EditorGUILayout.Popup(0, _compressionFormatOptions.Select(x => x.ToString()).ToArray());
                                                                        if (newFormat != 0)
                                                                        {
                                                                            texture.ChangeImporterFormat(_compressionFormatOptions[newFormat]);
                                                                            Avatar.Recalculate();
                                                                        }
                                                                    }

                                                                    else
                                                                    {
                                                                        GUILayout.Label(texture.Format.ToString());
                                                                    }
                                                                }
                                                            });
                                                        }
                                                    }
                                                }
                                            }
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