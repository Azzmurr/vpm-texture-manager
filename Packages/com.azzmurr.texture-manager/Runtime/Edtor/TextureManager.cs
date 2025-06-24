#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Thry.AvatarHelpers;
using UnityEngine.UIElements;

namespace Azzmurr.AvatarHelpers
{
    public class TextureManager : EditorWindow
    {
        [MenuItem("Thry/Avatar/Texture Manager")]
        public static void Init()
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window.Show();
        }

        [MenuItem("GameObject/Thry/Avatar/Texture Manager", true, 0)]
        static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Thry/Avatar/Texture Manager", false, 0)]
        public static void ShowFromSelection()
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window._avatar = Selection.activeGameObject;
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            TextureManager window = (TextureManager)EditorWindow.GetWindow(typeof(TextureManager));
            window.titleContent = new GUIContent("Texture Manager");
            window._avatar = avatar;
            window.Calc(avatar);
            window.Show();
        }

        GameObject _avatar;
        List<TextureMeta> _texturesList;

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
                if (GUILayout.Button(refreshIcon, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    Calc(_avatar);
                }
                GUI.enabled = true;

                _avatar = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _avatar, typeof(GameObject), true, GUILayout.Height(30));
                if (EditorGUI.EndChangeCheck() && _avatar != null)
                {
                    Calc(_avatar);
                }

            }



            if (_avatar != null)
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosMajor))
                {

                    _scrollPosMajor = scroll.scrollPosition;
                    GUILine();
                    EditorGUILayout.Space();

                    if (_texturesList == null) Calc(_avatar);

                    if (_texturesList != null)
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
                                for (int texIdx = 0; texIdx < _texturesList.Count; texIdx++)
                                {
                                    TextureMeta textureMeta = _texturesList[texIdx];

                                    if (makeAllTexturesLessThan2k && textureMeta.pcResolution > 2048)
                                    {
                                        textureMeta.ChangeImportSize(2048);
                                    }

                                    if (makeAllTexturesForAndroid && textureMeta.androidResolution > textureMeta.pcResolution / 2 && textureMeta.pcResolution > 512)
                                    {
                                        textureMeta.ChangeImportSizeAndroid(textureMeta.pcResolution / 2);
                                    }

                                    if (makeAlltexturesCrunched && textureMeta.bestTextureFormat != null && (TextureImporterFormat)textureMeta.format != textureMeta.bestTextureFormat)
                                    {
                                        textureMeta.ChangeImporterFormat((TextureImporterFormat)textureMeta.bestTextureFormat);
                                    }
                                }

                                Calc(_avatar);
                            }

                            Boolean createQuestMaterialPresets = false;
                            Grid.Cell((index) => makeAlltexturesCrunched = GUILayout.Button("Create Quest Material Presets"));
                            Grid.Cell((index) => GUILayout.Label("This will create quest materilas with VRChat/Mobile/Standard Lite shader"));

                            if (createQuestMaterialPresets)
                            {
                                Scene scene = SceneManager.GetActiveScene();

                                if (EditorUtility.DisplayDialog("Create Quest Materials", $"You are going to create Quest materials with changed shader to VRChat/Mobile/Standard in Assets/Quest Materials/{scene.name}/{_avatar.name}.", "Yes let's do this!", "Naaah, I just hanging around"))
                                {
                                    IEnumerable<Material>[] materials = AvatarEvaluator.GetMaterials(_avatar);


                                    if (!Directory.Exists("Assets/Quest Materials"))
                                    {
                                        Directory.CreateDirectory("Assets/Quest Materials");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}");
                                    }

                                    if (!Directory.Exists($"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.name.Trim()}"))
                                    {
                                        Directory.CreateDirectory($"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.name.Trim()}");
                                    }

                                    foreach (Material m in materials[1])
                                    {
                                        if (m != null)
                                        {
                                            Material newQuestMaterial = new Material(m);
                                            newQuestMaterial.shader = Shader.Find("VRChat/Mobile/Standard Lite");
                                            UnityEditor.AssetDatabase.CreateAsset(newQuestMaterial, $"Assets/Quest Materials/{scene.name.Trim()}/{_avatar.name.Trim()}/{m.name}.mat");

                                        }
                                    }
                                }
                            }
                        }


                        GUILine();
                        EditorGUILayout.Space();

                        VariableGridScope GridResults = new VariableGridScope(new float[] {
                            Config.MATERIAL_BUTTON_WTDTH,
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
                            GridResults.Cell((Icon) => GUILayout.Label(""));
                            GridResults.Cell((Material) => GUILayout.Label("Material"));
                            GridResults.Cell((Texture) => GUILayout.Label("Texture"));
                            GridResults.Cell((Size) => GUILayout.Label("Size"));
                            GridResults.Cell((PC) => GUILayout.Label("PC"));
                            GridResults.Cell((ANDROID) => GUILayout.Label("ANDROID"));
                            GridResults.Cell((Format) => GUILayout.Label("Format"));
                            GridResults.Cell((Actions) => GUILayout.Label("Actions"));


                            for (int texIdx = 0; texIdx < _texturesList.Count; texIdx++)
                            {
                                TextureMeta textureMeta = _texturesList[texIdx];

                                GridResults.Cell((Icon) =>
                                {
                                    using (new EditorGUI.DisabledGroupScope(textureMeta.materials.Count < 1))
                                    {
                                        GUIContent content = textureMeta.materialDropDown ? matActiveIcon : matInactiveIcon;
                                        bool toggleMaterialList = GUILayout.Button(content, GUILayout.Width(Config.MATERIAL_BUTTON_WTDTH), GUILayout.Height(Config.ROW_HEIGHT));
                                        if (toggleMaterialList)
                                        {
                                            textureMeta.materialDropDown = !textureMeta.materialDropDown;
                                            _texturesList[texIdx] = textureMeta;
                                        }
                                    }
                                });

                                GridResults.Cell((Material) => EditorGUILayout.ObjectField(textureMeta.materials[0], typeof(Material), false));
                                GridResults.Cell((Texture) => EditorGUILayout.ObjectField(textureMeta.texture, typeof(object), false));
                                GridResults.Cell((Size) => GUILayout.Label(textureMeta.sizeString));
                                GridResults.Cell((PC) =>
                                {
                                    if (textureMeta.textureWithChangableResolution)
                                    {
                                        _textureSizeOptions[0] = textureMeta.pcResolution;
                                        int newResolution = EditorGUILayout.IntPopup(textureMeta.pcResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                        if (newResolution != textureMeta.pcResolution)
                                        {
                                            textureMeta.ChangeImportSize(newResolution);
                                            Calc(_avatar);
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Label(textureMeta.pcResolution.ToString());
                                    }
                                });

                                GridResults.Cell((ANDROID) =>
                                {
                                    if (textureMeta.textureWithChangableResolution)
                                    {
                                        _textureSizeOptions[0] = textureMeta.androidResolution;
                                        int newResolutionAndroid = EditorGUILayout.IntPopup(textureMeta.androidResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions);
                                        if (newResolutionAndroid != textureMeta.androidResolution)
                                        {
                                            textureMeta.ChangeImportSizeAndroid(newResolutionAndroid);
                                            Calc(_avatar);
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Label(textureMeta.androidResolution.ToString());
                                    }
                                });

                                GridResults.Cell((Format) =>
                                {
                                    if (textureMeta.formatString.Length > 0)
                                    {
                                        if (textureMeta.textureWithChangableFormat)
                                        {
                                            _compressionFormatOptions[0] = ((TextureImporterFormat)textureMeta.format);
                                            int newFormat = EditorGUILayout.Popup(0, _compressionFormatOptions.Select(x => x.ToString()).ToArray());
                                            if (newFormat != 0)
                                            {
                                                textureMeta.ChangeImporterFormat(_compressionFormatOptions[newFormat]);
                                                Calc(_avatar);
                                            }
                                        }

                                        else
                                        {
                                            GUILayout.Label(textureMeta.format.ToString());
                                        }
                                    }
                                });

                                GridResults.Cell((Actions) =>
                                {
                                    if (textureMeta.betterTextureFormat != null)
                                    {
                                        bool changeFormat = GUILayout.Button($"{textureMeta.betterTextureFormat} → -{textureMeta.savedSizeWithBetterTextureFormat}");
                                        if (changeFormat)
                                        {
                                            bool changeFormatPopup = EditorUtility.DisplayDialog(
                                                $"Confirm Compression Format Change!",
                                                $"You are about to change the compression format of texture '{textureMeta.texture.name}' from {textureMeta.format} => {textureMeta.betterTextureFormat}\n\n" +
                                                $"If you wish to return this texture's compression to {textureMeta.formatString}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?",
                                                "Yes",
                                                "No"
                                            );

                                            if (changeFormatPopup)
                                            {
                                                textureMeta.ChangeImporterFormat((TextureImporterFormat)textureMeta.betterTextureFormat);
                                                Calc(_avatar);
                                            }
                                        }
                                    }

                                    if (textureMeta.textureTooBig)
                                    {
                                        bool chageImportSize = GUILayout.Button($"2k → -{textureMeta.saveSizeWithSmallerTexture}");

                                        if (chageImportSize)
                                        {
                                            textureMeta.ChangeImportSize(2048);
                                            Calc(_avatar);
                                        }
                                    }
                                });


                                if (textureMeta.materialDropDown)
                                {
                                    GUILayout.Label($"Used in {textureMeta.materials.Count()} material(s) on '{_avatar.name}'", EditorStyles.boldLabel);
                                    EditorGUI.indentLevel++;
                                    foreach (Material mat in textureMeta.materials)
                                    {
                                        EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(395), GUILayout.Height(Config.ROW_HEIGHT));
                                    }
                                    EditorGUI.indentLevel--;
                                    GUILayout.Space(5);
                                }
                            }

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

        static Dictionary<Texture, bool> GetTextures(GameObject avatar)
        {
            IEnumerable<Material>[] materials = AvatarEvaluator.GetMaterials(avatar);

            Dictionary<Texture, bool> textures = new Dictionary<Texture, bool>();
            foreach (Material m in materials[1])
            {
                if (m == null) continue;
                int[] textureIds = m.GetTexturePropertyNameIDs();
                bool isActive = materials[0].Contains(m);
                foreach (int id in textureIds)
                {
                    if (!m.HasProperty(id)) continue;
                    Texture t = m.GetTexture(id);
                    if (t == null) continue;
                    if (textures.ContainsKey(t))
                    {
                        if (textures[t] == false && isActive) textures[t] = true;
                    }
                    else
                    {
                        textures.Add(t, isActive);
                    }
                }
            }
            return textures;
        }

        List<Material> GetMaterialsUsingTexture(Texture texture, List<Material> materialsToSearch)
        {
            List<Material> materials = new List<Material>();

            foreach (Material mat in materialsToSearch)
            {
                foreach (string propName in mat.GetTexturePropertyNames())
                {
                    Texture matTex = mat.GetTexture(propName);
                    if (matTex != null && matTex == texture)
                    {
                        materials.Add(mat);
                        break;
                    }
                }
            }

            return materials;
        }

        public void Calc(GameObject avatar)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Getting Materials", 0.5f);
                // get all materials in avatar
                List<Material> tempMaterials = avatar.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(r => r.sharedMaterials)
                    .Where(mat => mat != null)
                    .Distinct()
                    .ToList();

                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Getting Textures", 0.5f);
                Dictionary<Texture, bool> textures = GetTextures(avatar);
                _texturesList = new List<TextureMeta>();

                int numTextures = textures.Keys.Count;
                int texIdx = 1;
                foreach (KeyValuePair<Texture, bool> t in textures)
                {
                    EditorUtility.DisplayProgressBar("Getting VRAM Data", $"Calculating texture size for {t.Key.name}", texIdx / (float)numTextures);
                    TextureMeta textureMeta = new TextureMeta();
                    textureMeta.calculate(t.Key, GetMaterialsUsingTexture(t.Key, tempMaterials));

                    _texturesList.Add(textureMeta);

                    texIdx++;
                }


                _texturesList.Sort((t1, t2) => t1.CompareTo(t2));

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif
