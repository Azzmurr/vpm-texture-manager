using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Azzmurr.Utils
{
    public class TextureManager : EditorWindow
    {
        [MenuItem("Azzmurr/Avatar/Texture Manager")]
        public static void Init()
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window.Show();
        }

        [MenuItem("GameObject/Azzmurr/Avatar/Texture Manager", true, 0)]
        static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Azzmurr/Avatar/Texture Manager", false, 0)]
        public static void ShowFromSelection()
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window.Avatar = new AvatarMeta(Selection.activeGameObject);
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window.Avatar = new AvatarMeta(avatar);
            window.Show();
        }

        AvatarMeta Avatar;
        Vector2 MainScrollPosition;
        GUIContent RefreshIcon;

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
            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {

                Button(GUILayout.Button(RefreshIcon, GUILayout.Width(30), GUILayout.Height(30)), () => Avatar?.Recalculate());

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
                    if (Avatar.TextureCount != 0)
                    {
                        VariableGridScope Grid = new(new float[] { 200, 200 });
                        using (Grid)
                        {
                            Grid.Cell((index) =>
                            {
                                if (GUILayout.Button("Textures max size -> 2k"))
                                {
                                    Avatar.ForeachTexture((texture) =>
                                    {
                                        if (texture.PcResolution > 2048) texture.ChangeImportSize(2048);
                                    });
                                }
                            });
                            Grid.Cell((index) => GUILayout.Label("Makes max texture size 2k"));



                            Grid.Cell((index) =>
                            {
                                if (GUILayout.Button("Prepare textures for Android"))
                                {
                                    Avatar.ForeachTexture((texture) =>
                                    {
                                        if (texture.AndroidResolution > texture.PcResolution / 2 && texture.PcResolution > 512)
                                        {
                                            texture.ChangeImportSizeAndroid(texture.PcResolution / 2);
                                        }
                                    });
                                }
                            });
                            Grid.Cell((index) => GUILayout.Label("Makes max texture for android half size of PC size"));

                            Grid.Cell((index) =>
                            {
                                if (GUILayout.Button("Crunch textures"))
                                {
                                    Avatar.ForeachTexture((texture) =>
                                    {
                                        if (texture.BestTextureFormat != null && (TextureImporterFormat)texture.Format != texture.BestTextureFormat)
                                        {
                                            texture.ChangeImporterFormat((TextureImporterFormat)texture.BestTextureFormat);
                                        }
                                    });
                                }
                            });
                            Grid.Cell((index) => GUILayout.Label("Sets texture format to DTX1Crunched or DTX5Crunched"));

                            Boolean createQuestMaterialPresets = false;
                            Grid.Cell((index) => createQuestMaterialPresets = GUILayout.Button("Create Quest Material Presets"));
                            Grid.Cell((index) => GUILayout.Label("This will create quest materilas with VRChat/Mobile/Standard Lite shader"));

                            if (createQuestMaterialPresets)
                            {
                                Scene scene = SceneManager.GetActiveScene();

                                if (EditorUtility.DisplayDialog("Create Quest Materials", $"You are going to create Quest materials with changed shader to VRChat/Mobile/Standard in Assets/Quest Materials/{scene.name}/{Avatar.Name}.", "Yes let's do this!", "Naaah, I just hanging around"))
                                {
                                    if (!Directory.Exists("Assets/Quest Materials"))
                                    {
                                        Directory.CreateDirectory("Assets/Quest Materials");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}/{Avatar.Name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}/{Avatar.Name.Trim()}");
                                    }

                                    Avatar.ForeachMaterial((material) =>
                                    {
                                        if (material != null)
                                        {
                                            Material newQuestMaterial = material.getQuestMaterial();
                                            AssetDatabase.CreateAsset(newQuestMaterial, $"Assets/Quest Materials/{scene.name.Trim()}/{Avatar.Name.Trim()}/{material.Name}.mat");

                                        }
                                    });
                                }
                            }
                        }


                        GUILine();
                        EditorGUILayout.Space();

                        VariableGridScope GridResults = new VariableGridScope(new float[] {
                            // Config.MATERIAL_BUTTON_WTDTH,
                            Config.MATERIAL_NAME_WIDTH,
                            Config.TEXTURE_WIDTH,
                            Config.SIZE_WIDTH,
                            Config.PC_WIDTH,
                            Config.ANDROID_WIDTH,
                            Config.FORMAT_WIDTH,
                            Config.ACTIONS_WIDTH,
                        });
                        using (GridResults)
                        {
                            GridResults.Cell((Material) => GUILayout.Label("Material"));
                            GridResults.Cell((Texture) => GUILayout.Label("Texture"));
                            GridResults.Cell((Size) => GUILayout.Label("Size"));
                            GridResults.Cell((PC) => GUILayout.Label("PC"));
                            GridResults.Cell((ANDROID) => GUILayout.Label("ANDROID"));
                            GridResults.Cell((Format) => GUILayout.Label("Format"));
                            GridResults.Cell((Actions) => GUILayout.Label("Actions"));

                            Avatar.ForeachTexture((texture) =>
                            {
                                GridResults.Cell((Material) =>
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        Avatar.ForeachTextureMaterial(texture, (material) => EditorGUILayout.ObjectField(material, typeof(Material), false));
                                    }
                                });
                                GridResults.Cell((Texture) => EditorGUILayout.ObjectField(texture.texture, typeof(object), false));
                                GridResults.Cell((Size) => GUILayout.Label(texture.SizeString));
                                GridResults.Cell((PC) =>
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

                                GridResults.Cell((ANDROID) =>
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

                                GridResults.Cell((Format) =>
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

                                GridResults.Cell((Actions) =>
                                {
                                    if (texture.Poiyomi) GUILayout.Label("Poiyomi textures are ignored and can't be changed");

                                    if (texture.BetterTextureFormat != null)
                                    {
                                        bool changeFormat = GUILayout.Button($"{texture.BetterTextureFormat} → -{texture.SavedSizeWithBetterTextureFormat}");
                                        if (changeFormat)
                                        {
                                            bool changeFormatPopup = EditorUtility.DisplayDialog(
                                                $"Confirm Compression Format Change!",
                                                $"You are about to change the compression format of texture '{texture.texture.name}' from {texture.Format} => {texture.BetterTextureFormat}\n\n" +
                                                $"If you wish to return this texture's compression to {texture.FormatString}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?",
                                                "Yes",
                                                "No"
                                            );

                                            if (changeFormatPopup)
                                            {
                                                texture.ChangeImporterFormat((TextureImporterFormat)texture.BetterTextureFormat);
                                                Avatar.Recalculate();
                                            }
                                        }
                                    }

                                    if (texture.TextureTooBig)
                                    {
                                        bool chageImportSize = GUILayout.Button($"2k → -{texture.SaveSizeWithSmallerTexture}");

                                        if (chageImportSize)
                                        {
                                            texture.ChangeImportSize(2048);
                                            Avatar.Recalculate();
                                        }
                                    }
                                });
                            });
                        }
                    }

                }
            }
        }

        static void Button(bool button, Action action)
        {
            if (button) action.Invoke();
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
