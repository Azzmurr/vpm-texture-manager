using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Azzmurr.Utils
{
    internal class MaterialMeneger : EditorWindow
    {
        [MenuItem("Tools/Azzmurr/Material Manager")]
        public static void Init()
        {
            MaterialMeneger window = (MaterialMeneger)EditorWindow.GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window.Show();
        }

        [MenuItem("GameObject/Azzmurr/Material Manager", true, 0)]
        public static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Azzmurr/Material Manager", false, 0)]
        public static void ShowFromSelection()
        {
            var window = (MaterialMeneger)GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window._avatar = new AvatarMeta(Selection.activeGameObject);
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            var window = (MaterialMeneger)GetWindow(typeof(MaterialMeneger));
            window.titleContent = new GUIContent("Material Manager");
            window._avatar = new AvatarMeta(avatar);
            window.Show();
        }

        private AvatarMeta _avatar;
        private Vector2 _mainScrollPosition;
        private GUIContent _refreshIcon;

        private bool _moreTextureInfo;

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
                var refresh = GUILayout.Button(_refreshIcon, GUILayout.Width(30), GUILayout.Height(30));
                if (refresh && _avatar != null) _avatar.Recalculate();

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

                if (_avatar.MaterialsCount <= 0) return;
                using (var actionGrid = new VariableGridScope(new float[] { 75, 75 }))
                {
                    actionGrid.Cell(_ => GUILayout.Label("Poiyomi", Config.Label));

                    actionGrid.Cell(_ =>
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Unlock")) _avatar.UnlockMaterials();
                            if (GUILayout.Button("Update")) _avatar.UpdateMaterials();
                            if (GUILayout.Button("Lock")) _avatar.LockMaterials();
                        }
                    });

                    actionGrid.Cell(_ =>
                    {
                        GUILayout.Label("Textures", Config.Label);
                    });

                    actionGrid.Cell(_ =>
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("-> 2k")) _avatar.MakeAllTextures2K();
                            if (GUILayout.Button("Prepare for Android")) _avatar.MakeTexturesReadyForAndroid();
                            if (GUILayout.Button("Crunch")) _avatar.CrunchTextures();
                        }
                    });

                    actionGrid.Cell(_ => GUILayout.Label("Materials", Config.Label));

                    actionGrid.Cell(_ =>
                    {
                        if (GUILayout.Button("Create Quest Presets")) _avatar.CreateQuestMaterialPresets();
                    });
                }

                GUILine();
                EditorGUILayout.Space();

                using (var resultsGrid = new VariableGridScope(new float[] { 88, 200, 1 }, 8))
                {
                    resultsGrid.Cell(_ => GUILayout.Label("Preview", Config.Label));
                    resultsGrid.Cell(_ => GUILayout.Label("Info", Config.Label, GUILayout.Width(50)));
                    resultsGrid.Cell(_ =>
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.Space();
                            GUILayout.Label("Textures", Config.Label);
                            if (GUILayout.Button(_moreTextureInfo ? "Less Info" : "More Info"))
                            {
                                _moreTextureInfo = !_moreTextureInfo;
                            }
                        }
                    });

                    _avatar.ForeachMaterial(material =>
                    {
                        resultsGrid.Cell(_ =>
                        {
                            var preview = AssetPreview.GetAssetPreview(material.Material);
                            EditorGUILayout.ObjectField(GUIContent.none, preview, typeof(Texture2D), false, GUILayout.Width(88), GUILayout.Height(88));
                        });

                        resultsGrid.Cell(_ =>
                        {
                            EditorGUILayout.ObjectField(material.Material, typeof(Material), false);
                            EditorGUILayout.Space();
                            using var materialInfoGrid = new VariableGridScope(new float[] { 75, 125 });
                            materialInfoGrid.Cell(_ => GUILayout.Label("Shader:", Config.Label));
                            materialInfoGrid.Cell(_ => GUILayout.Label(material.ShaderName, Config.Label));
                            materialInfoGrid.Cell(_ => GUILayout.Label("Locked:", Config.Label));
                            materialInfoGrid.Cell(_ => GUILayout.Label(material.ShaderLockedString, material.ShaderLockedError switch
                            {
                                null => Config.Label,
                                true => Config.InvalidLabel,
                                _ => Config.ValidLabel
                            }));
                            
                            materialInfoGrid.Cell(_ => GUILayout.Label("Version:", Config.Label));
                            materialInfoGrid.Cell(_ => GUILayout.Label(material.ShaderVersion, material.ShaderVersionError switch
                            {
                                null => Config.Label,
                                true => Config.InvalidLabel,
                                _ => Config.ValidLabel
                            }));
                             
                            materialInfoGrid.Cell(_ => GUILayout.Label("Checklist:", Config.Label));
                            materialInfoGrid.Cell(_ => Button(GUILayout.Button("Check"), () =>
                            {
                                var window = (MaterialChecklist) GetWindow(typeof(MaterialChecklist));;
                                window.titleContent = new GUIContent("Material Checklist");
                                window.Material = material;
                            }));
                        });

                        resultsGrid.Cell(_ =>
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                material.ForeachTexture(texture =>
                                {
                                    EditorGUILayout.Space();
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.ObjectField(GUIContent.none, texture.Texture, typeof(Texture), false, GUILayout.Width(88), GUILayout.Height(88));

                                        if (!_moreTextureInfo) return;
                                        using (new EditorGUILayout.VerticalScope())
                                        {
                                            using (var textureSActions = new VariableGridScope(new float[] { 50, 100 }))
                                            {
                                                textureSActions.Cell(_ => GUILayout.Label("PC:", Config.Label));
                                                textureSActions.Cell(_ =>
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

                                                textureSActions.Cell(_ => GUILayout.Label("Android:", Config.Label));
                                                textureSActions.Cell(_ =>
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

                                                textureSActions.Cell(_ => GUILayout.Label("Format:", Config.Label));
                                                textureSActions.Cell(_ =>
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