using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Azzmurr.Utils
{
    public class TextureManager : EditorWindow
    {
        [MenuItem("Tools/Azzmurr/Texture Manager")]
        public static void Init()
        {
            var window = (TextureManager)GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window.Show();
        }

        [MenuItem("GameObject/Azzmurr/Texture Manager", true, 0)]
        public static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Azzmurr/Texture Manager", false, 0)]
        public static void ShowFromSelection()
        {
            var window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window._avatar = new AvatarMeta(Selection.activeGameObject);
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            var window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window._avatar = new AvatarMeta(avatar);
            window.Show();
        }

        private AvatarMeta _avatar;
        private Vector2 _mainScrollPosition;
        private GUIContent _refreshIcon;

        private readonly int[] _textureSizeOptions = { 0, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private readonly TextureImporterFormat[] _compressionFormatOptions = {
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
            _refreshIcon = EditorGUIUtility.IconContent("RotateTool On", "Recalculate");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {

                Button(GUILayout.Button(_refreshIcon, GUILayout.Width(30), GUILayout.Height(30)), () => _avatar?.Recalculate());

                var gameObject = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _avatar?.GameObject, typeof(GameObject), true, GUILayout.Height(30));

                if (_avatar == null || _avatar.GameObject != gameObject)
                {
                    _avatar = gameObject ? new AvatarMeta(gameObject) : null;
                }
            }

            if (_avatar == null) return;
            using (var scroll = new EditorGUILayout.ScrollViewScope(_mainScrollPosition))
            {

                _mainScrollPosition = scroll.scrollPosition;
                GUILine();
                EditorGUILayout.Space();
                if (_avatar.TextureCount == 0) return;
                VariableGridScope actionGrid = new(new float[] { 75, 75 });
                using (actionGrid)
                {
                    actionGrid.Cell(_ => GUILayout.Label("Textures"));

                    actionGrid.Cell(_ =>
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("-> 2k")) _avatar.MakeAllTextures2K();
                            if (GUILayout.Button("Prepare textures for Android")) _avatar.MakeTexturesReadyForAndroid();
                            if (GUILayout.Button("Crunch")) _avatar.CrunchTextures();
                        }
                    });

                    actionGrid.Cell(_ => GUILayout.Label("Materials"));

                    actionGrid.Cell(_ =>
                    {
                        if (GUILayout.Button("Create Quest Presets")) _avatar.CreateQuestMaterialPresets();
                    });
                }


                GUILine();
                EditorGUILayout.Space();

                var gridResults = new VariableGridScope(new float[] {
                    Config.MaterialNameWidth,
                    Config.TextureWidth,
                    Config.SizeWidth,
                    Config.PCWidth,
                    Config.AndroidWidth,
                    Config.FormatWidth,
                    Config.ActionsWidth,
                });
                using (gridResults)
                {
                    gridResults.Cell(_ => GUILayout.Label("Material"));
                    gridResults.Cell(_ => GUILayout.Label("Texture"));
                    gridResults.Cell(_ => GUILayout.Label("Size"));
                    gridResults.Cell(_ => GUILayout.Label("PC"));
                    gridResults.Cell(_ => GUILayout.Label("ANDROID"));
                    gridResults.Cell(_ => GUILayout.Label("Format"));
                    gridResults.Cell(_ => GUILayout.Label("Actions"));

                    _avatar.ForeachTexture(texture =>
                    {
                        gridResults.Cell(_ =>
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                _avatar.ForeachTextureMaterial(texture, material => EditorGUILayout.ObjectField(material, typeof(Material), false));
                            }
                        });
                        gridResults.Cell(_ => EditorGUILayout.ObjectField(texture.Texture, typeof(object), false));
                        gridResults.Cell(_ => GUILayout.Label(texture.SizeString));
                        gridResults.Cell(_ =>
                        {
                            if (texture.TextureWithChangeableResolution)
                            {
                                _textureSizeOptions[0] = texture.PcResolution;
                                var newResolution = EditorGUILayout.IntPopup(texture.PcResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                if (newResolution == texture.PcResolution) return;
                                
                                texture.ChangeImportSize(newResolution);
                                _avatar.Recalculate();
                            }
                            else
                            {
                                GUILayout.Label(texture.PcResolution.ToString());
                            }
                        });

                        gridResults.Cell(_ =>
                        {
                            if (texture.TextureWithChangeableResolution)
                            {
                                _textureSizeOptions[0] = texture.AndroidResolution;
                                var newResolutionAndroid = EditorGUILayout.IntPopup(texture.AndroidResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                if (newResolutionAndroid == texture.AndroidResolution) return;
                                
                                texture.ChangeImportSizeAndroid(newResolutionAndroid);
                                _avatar.Recalculate();
                            }
                            else
                            {
                                GUILayout.Label(texture.AndroidResolution.ToString());
                            }
                        });

                        gridResults.Cell(_ =>
                        {
                            if (texture.FormatString.Length <= 0) return;
                            
                            if (texture.TextureWithChangeableFormat && texture.Format != null)
                            {
                                _compressionFormatOptions[0] = (TextureImporterFormat)texture.Format;
                                var newFormat = EditorGUILayout.Popup(0, _compressionFormatOptions.Select(x => x.ToString()).ToArray());
                                if (newFormat == 0) return;
                                
                                texture.ChangeImporterFormat(_compressionFormatOptions[newFormat]);
                                _avatar.Recalculate();
                            }

                            else
                            {
                                GUILayout.Label(texture.Format.ToString());
                            }
                        });

                        gridResults.Cell(_ =>
                        {
                            if (texture.Poiyomi) GUILayout.Label("Poiyomi textures are ignored and can't be changed");

                            if (texture.BetterTextureFormat != null)
                            {
                                var changeFormat = GUILayout.Button($"{texture.BetterTextureFormat} → -{texture.SavedSizeWithBetterTextureFormat}");
                                if (changeFormat)
                                {
                                    var changeFormatPopup = EditorUtility.DisplayDialog(
                                        "Confirm Compression Format Change!",
                                        $"You are about to change the compression format of texture '{texture.Texture.name}' from {texture.Format} => {texture.BetterTextureFormat}\n\n" +
                                        $"If you wish to return this texture's compression to {texture.FormatString}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?",
                                        "Yes",
                                        "No"
                                    );

                                    if (changeFormatPopup)
                                    {
                                        texture.ChangeImporterFormat((TextureImporterFormat)texture.BetterTextureFormat);
                                        _avatar.Recalculate();
                                    }
                                }
                            }

                            if (texture.TextureTooBig)
                            {
                                var chageImportSize = GUILayout.Button($"2k → -{texture.SaveSizeWithSmallerTexture}");

                                if (!chageImportSize) return;
                                texture.ChangeImportSize(2048);
                                _avatar.Recalculate();
                            }
                        });
                    });
                }

            }
        }

        private static void Button(bool button, Action action)
        {
            if (button) action.Invoke();
        }

        private static void GUILine(int iHeight = 1)
        {
            GUILayout.Space(10);
            var rect = EditorGUILayout.GetControlRect(false, iHeight);
            rect.width = EditorGUIUtility.currentViewWidth;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUILayout.Space(10);
        }
    }
}