#if UNITY_EDITOR
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
            window._avatar = new AvatarMeta(Selection.activeGameObject);
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window._avatar = new AvatarMeta(avatar);
            window.Show();
        }

        AvatarMeta _avatar;

        Vector2 _scrollPosMajor;

        GUIContent matActiveIcon;
        GUIContent matInactiveIcon;
        GUIContent refreshIcon;

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
            matActiveIcon = EditorGUIUtility.IconContent("d_Material Icon");
            matInactiveIcon = EditorGUIUtility.IconContent("d_Material On Icon");
            refreshIcon = EditorGUIUtility.IconContent("RotateTool On", "Recalculate");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = _avatar != null;

                bool refresh = GUILayout.Button(refreshIcon, GUILayout.Width(30), GUILayout.Height(30));
                if (refresh) _avatar.recalculate();

                GUI.enabled = true;

                GameObject gameObject = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _avatar?.GameObject, typeof(GameObject), true, GUILayout.Height(30));
                _avatar = gameObject ? new AvatarMeta(gameObject) : null;

                if (EditorGUI.EndChangeCheck() && _avatar != null) _avatar.recalculate();
            }



            if (_avatar != null)
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosMajor))
                {

                    _scrollPosMajor = scroll.scrollPosition;
                    GUILine();
                    EditorGUILayout.Space();
                    if (_avatar.TextureCount != 0)
                    {
                        VariableGridScope Grid = new VariableGridScope(new float[] { 200, 200 });
                        using (Grid)
                        {
                            Boolean makeAllTexturesLessThan2k = false;
                            Grid.Cell((index) => makeAllTexturesLessThan2k = GUILayout.Button("Textures max size -> 2k"));
                            Grid.Cell((index) => GUILayout.Label("Makes max texture size 2k"));

                            Boolean makeAllTexturesForAndroid = false;
                            Grid.Cell((index) => makeAllTexturesForAndroid = GUILayout.Button("Prepare textures for Android"));
                            Grid.Cell((index) => GUILayout.Label("Makes max texture for android half size of PC size"));

                            Boolean makeAlltexturesCrunched = false;
                            Grid.Cell((index) => makeAlltexturesCrunched = GUILayout.Button("Crunch textures"));
                            Grid.Cell((index) => GUILayout.Label("Sets texture format to DTX1Crunched or DTX5Crunched"));

                            if (makeAllTexturesLessThan2k || makeAllTexturesForAndroid || makeAlltexturesCrunched)
                            {
                                _avatar.ForeachTexture((texture) =>
                                {
                                    if (makeAllTexturesLessThan2k && texture.pcResolution > 2048)
                                    {
                                        texture.ChangeImportSize(2048);
                                    }

                                    if (makeAllTexturesForAndroid && texture.androidResolution > texture.pcResolution / 2 && texture.pcResolution > 512)
                                    {
                                        texture.ChangeImportSizeAndroid(texture.pcResolution / 2);
                                    }

                                    if (makeAlltexturesCrunched && texture.bestTextureFormat != null && (TextureImporterFormat)texture.format != texture.bestTextureFormat)
                                    {
                                        texture.ChangeImporterFormat((TextureImporterFormat)texture.bestTextureFormat);
                                    }
                                });


                                _avatar.recalculate();
                            }

                            Boolean createQuestMaterialPresets = false;
                            Grid.Cell((index) => createQuestMaterialPresets = GUILayout.Button("Create Quest Material Presets"));
                            Grid.Cell((index) => GUILayout.Label("This will create quest materilas with VRChat/Mobile/Standard Lite shader"));

                            if (createQuestMaterialPresets)
                            {
                                Scene scene = SceneManager.GetActiveScene();

                                if (EditorUtility.DisplayDialog("Create Quest Materials", $"You are going to create Quest materials with changed shader to VRChat/Mobile/Standard in Assets/Quest Materials/{scene.name}/{_avatar.Name}.", "Yes let's do this!", "Naaah, I just hanging around"))
                                {
                                    if (!Directory.Exists("Assets/Quest Materials"))
                                    {
                                        Directory.CreateDirectory("Assets/Quest Materials");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.Name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.Name.Trim()}");
                                    }

                                    _avatar.ForeachMaterial((material) =>
                                    {
                                        if (material != null)
                                        {
                                            Material newQuestMaterial = new Material(material);
                                            newQuestMaterial.shader = Shader.Find("VRChat/Mobile/Standard Lite");
                                            AssetDatabase.CreateAsset(newQuestMaterial, $"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.Name.Trim()}/{material.name}.mat");

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

                            _avatar.ForeachTexture((texture) =>
                            {
                                GridResults.Cell((Material) =>
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        texture.ForeachMaterial((material) => EditorGUILayout.ObjectField(material, typeof(Material), false));
                                    }
                                });
                                GridResults.Cell((Texture) => EditorGUILayout.ObjectField(texture.texture, typeof(object), false));
                                GridResults.Cell((Size) => GUILayout.Label(texture.sizeString));
                                GridResults.Cell((PC) =>
                                {
                                    if (texture.textureWithChangableResolution)
                                    {
                                        _textureSizeOptions[0] = texture.pcResolution;
                                        int newResolution = EditorGUILayout.IntPopup(texture.pcResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                        if (newResolution != texture.pcResolution)
                                        {
                                            texture.ChangeImportSize(newResolution);
                                            _avatar.recalculate();
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Label(texture.pcResolution.ToString());
                                    }
                                });

                                GridResults.Cell((ANDROID) =>
                                {
                                    if (texture.textureWithChangableResolution)
                                    {
                                        _textureSizeOptions[0] = texture.androidResolution;
                                        int newResolutionAndroid = EditorGUILayout.IntPopup(texture.androidResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                        if (newResolutionAndroid != texture.androidResolution)
                                        {
                                            texture.ChangeImportSizeAndroid(newResolutionAndroid);
                                            _avatar.recalculate();
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Label(texture.androidResolution.ToString());
                                    }
                                });

                                GridResults.Cell((Format) =>
                                {
                                    if (texture.formatString.Length > 0)
                                    {
                                        if (texture.textureWithChangableFormat)
                                        {
                                            _compressionFormatOptions[0] = ((TextureImporterFormat)texture.format);
                                            int newFormat = EditorGUILayout.Popup(0, _compressionFormatOptions.Select(x => x.ToString()).ToArray());
                                            if (newFormat != 0)
                                            {
                                                texture.ChangeImporterFormat(_compressionFormatOptions[newFormat]);
                                                _avatar.recalculate();
                                            }
                                        }

                                        else
                                        {
                                            GUILayout.Label(texture.format.ToString());
                                        }
                                    }
                                });

                                GridResults.Cell((Actions) =>
                                {
                                    if (texture.isPoiyomi) GUILayout.Label("Poiyomi textures are ignored and can't be changed");

                                    if (texture.betterTextureFormat != null)
                                    {
                                        bool changeFormat = GUILayout.Button($"{texture.betterTextureFormat} → -{texture.savedSizeWithBetterTextureFormat}");
                                        if (changeFormat)
                                        {
                                            bool changeFormatPopup = EditorUtility.DisplayDialog(
                                                $"Confirm Compression Format Change!",
                                                $"You are about to change the compression format of texture '{texture.texture.name}' from {texture.format} => {texture.betterTextureFormat}\n\n" +
                                                $"If you wish to return this texture's compression to {texture.formatString}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?",
                                                "Yes",
                                                "No"
                                            );

                                            if (changeFormatPopup)
                                            {
                                                texture.ChangeImporterFormat((TextureImporterFormat)texture.betterTextureFormat);
                                                _avatar.recalculate();
                                            }
                                        }
                                    }

                                    if (texture.textureTooBig)
                                    {
                                        bool chageImportSize = GUILayout.Button($"2k → -{texture.saveSizeWithSmallerTexture}");

                                        if (chageImportSize)
                                        {
                                            texture.ChangeImportSize(2048);
                                            _avatar.recalculate();
                                        }
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
