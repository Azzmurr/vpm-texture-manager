#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Azzmurr.AvatarHelpers {
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

        //https://docs.unity3d.com/Manual/class-TextureImporterOverride.html

        static string DEFAULT_MAT_NAME = "Used in mat swap";
        static int ROW_HEIGHT = 20;
        static int MATERIAL_BUTTON_WTDTH = 25;
        static int MATERIAL_NAME_WIDTH = 150;
        static int TEXTURE_WIDTH = 450;
        static int SIZE_WIDTH = 70;
        static int PC_WIDTH = 55;
        static int ANDROID_WIDTH = 65;
        static int FORMAT_WIDTH = 150;
        static int ACTIONS_WIDTH = 175;

        static Dictionary<TextureFormat, float> BPP = new Dictionary<TextureFormat, float>()
        {
        //
        // Summary:
        //     Alpha-only texture format, 8 bit integer.
            { TextureFormat.Alpha8 , 9 },
        //
        // Summary:
        //     A 16 bits/pixel texture format. Texture stores color with an alpha channel.
            { TextureFormat.ARGB4444 , 16 },
        //
        // Summary:
        //     Color texture format, 8-bits per channel.
            { TextureFormat.RGB24 , 24 },
        //
        // Summary:
        //     Color with alpha texture format, 8-bits per channel.
            { TextureFormat.RGBA32 , 32 },
        //
        // Summary:
        //     Color with alpha texture format, 8-bits per channel.
            { TextureFormat.ARGB32 , 32 },
        //
        // Summary:
        //     A 16 bit color texture format.
            { TextureFormat.RGB565 , 16 },
        //
        // Summary:
        //     Single channel (R) texture format, 16 bit integer.
            { TextureFormat.R16 , 16 },
        //
        // Summary:
        //     Compressed color texture format.
            { TextureFormat.DXT1 , 4 },
        //
        // Summary:
        //     Compressed color with alpha channel texture format.
            { TextureFormat.DXT5 , 8 },
        //
        // Summary:
        //     Color and alpha texture format, 4 bit per channel.
            { TextureFormat.RGBA4444 , 16 },
        //
        // Summary:
        //     Color with alpha texture format, 8-bits per channel.
            { TextureFormat.BGRA32 , 32 },
        //
        // Summary:
        //     Scalar (R) texture format, 16 bit floating point.
            { TextureFormat.RHalf , 16 },
        //
        // Summary:
        //     Two color (RG) texture format, 16 bit floating point per channel.
            { TextureFormat.RGHalf , 32 },
        //
        // Summary:
        //     RGB color and alpha texture format, 16 bit floating point per channel.
            { TextureFormat.RGBAHalf , 64 },
        //
        // Summary:
        //     Scalar (R) texture format, 32 bit floating point.
            { TextureFormat.RFloat , 32 },
        //
        // Summary:
        //     Two color (RG) texture format, 32 bit floating point per channel.
            { TextureFormat.RGFloat , 64 },
        //
        // Summary:
        //     RGB color and alpha texture format, 32-bit floats per channel.
            { TextureFormat.RGBAFloat , 128 },
        //
        // Summary:
        //     A format that uses the YUV color space and is often used for video encoding or
        //     playback.
            { TextureFormat.YUY2 , 16 },
        //
        // Summary:
        //     RGB HDR format, with 9 bit mantissa per channel and a 5 bit shared exponent.
            { TextureFormat.RGB9e5Float , 32 },
        //
        // Summary:
        //     HDR compressed color texture format.
            { TextureFormat.BC6H , 8 },
        //
        // Summary:
        //     High quality compressed color texture format.
            { TextureFormat.BC7 , 8 },
        //
        // Summary:
        //     Compressed one channel (R) texture format.
            { TextureFormat.BC4 , 4 },
        //
        // Summary:
        //     Compressed two-channel (RG) texture format.
            { TextureFormat.BC5 , 8 },
        //
        // Summary:
        //     Compressed color texture format with Crunch compression for smaller storage sizes.
            { TextureFormat.DXT1Crunched , 4 },
        //
        // Summary:
        //     Compressed color with alpha channel texture format with Crunch compression for
        //     smaller storage sizes.
            { TextureFormat.DXT5Crunched , 8 },
        //
        // Summary:
        //     PowerVR (iOS) 2 bits/pixel compressed color texture format.
            { TextureFormat.PVRTC_RGB2 , 6 },
        //
        // Summary:
        //     PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format.
            { TextureFormat.PVRTC_RGBA2 , 8 },
        //
        // Summary:
        //     PowerVR (iOS) 4 bits/pixel compressed color texture format.
            { TextureFormat.PVRTC_RGB4 , 12 },
        //
        // Summary:
        //     PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format.
            { TextureFormat.PVRTC_RGBA4 , 16 },
        //
        // Summary:
        //     ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
            { TextureFormat.ETC_RGB4 , 12 },
        //
        // Summary:
        //     ETC2 EAC (GL ES 3.0) 4 bitspixel compressed unsigned single-channel texture format.
            { TextureFormat.EAC_R , 4 },
        //
        // Summary:
        //     ETC2 EAC (GL ES 3.0) 4 bitspixel compressed signed single-channel texture format.
            { TextureFormat.EAC_R_SIGNED , 4 },
        //
        // Summary:
        //     ETC2 EAC (GL ES 3.0) 8 bitspixel compressed unsigned dual-channel (RG) texture
        //     format.
            { TextureFormat.EAC_RG , 8 },
        //
        // Summary:
        //     ETC2 EAC (GL ES 3.0) 8 bitspixel compressed signed dual-channel (RG) texture
        //     format.
            { TextureFormat.EAC_RG_SIGNED , 8 },
        //
        // Summary:
        //     ETC2 (GL ES 3.0) 4 bits/pixel compressed RGB texture format.
            { TextureFormat.ETC2_RGB , 12 },
        //
        // Summary:
        //     ETC2 (GL ES 3.0) 4 bits/pixel RGB+1-bit alpha texture format.
            { TextureFormat.ETC2_RGBA1 , 12 },
        //
        // Summary:
        //     ETC2 (GL ES 3.0) 8 bits/pixel compressed RGBA texture format.
            { TextureFormat.ETC2_RGBA8 , 32 },
        //
        // Summary:
        //     ASTC (4x4 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_4x4 , 8 },
        //
        // Summary:
        //     ASTC (5x5 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_5x5 , 5.12f },
        //
        // Summary:
        //     ASTC (6x6 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_6x6 , 3.55f },
        //
        // Summary:
        //     ASTC (8x8 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_8x8 , 2 },
        //
        // Summary:
        //     ASTC (10x10 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_10x10 , 1.28f },
        //
        // Summary:
        //     ASTC (12x12 pixel block in 128 bits) compressed RGB texture format.
            { TextureFormat.ASTC_12x12 , 1 },
        //
        // Summary:
        //     Two color (RG) texture format, 8-bits per channel.
            { TextureFormat.RG16 , 16 },
        //
        // Summary:
        //     Single channel (R) texture format, 8 bit integer.
            { TextureFormat.R8 , 8 },
        //
        // Summary:
        //     Compressed color texture format with Crunch compression for smaller storage sizes.
            { TextureFormat.ETC_RGB4Crunched , 12 },
        //
        // Summary:
        //     Compressed color with alpha channel texture format using Crunch compression for
        //     smaller storage sizes.
            { TextureFormat.ETC2_RGBA8Crunched , 32 },
        //
        // Summary:
        //     ASTC (4x4 pixel block in 128 bits) compressed RGB(A) HDR texture format.
            { TextureFormat.ASTC_HDR_4x4 , 8 },
        //
        // Summary:
        //     ASTC (5x5 pixel block in 128 bits) compressed RGB(A) HDR texture format.
            { TextureFormat.ASTC_HDR_5x5 , 5.12f },
        //
        // Summary:
        //     ASTC (6x6 pixel block in 128 bits) compressed RGB(A) HDR texture format.
            { TextureFormat.ASTC_HDR_6x6 , 3.55f },
        //
        // Summary:
        //     ASTC (8x8 pixel block in 128 bits) compressed RGB(A) texture format.
            { TextureFormat.ASTC_HDR_8x8 , 2 },
        //
        // Summary:
        //     ASTC (10x10 pixel block in 128 bits) compressed RGB(A) HDR texture format.
            { TextureFormat.ASTC_HDR_10x10 , 1.28f },
        //
        // Summary:
        //     ASTC (12x12 pixel block in 128 bits) compressed RGB(A) HDR texture format.
            { TextureFormat.ASTC_HDR_12x12 , 1 },
        //
        // Summary:
        //     Two channel (RG) texture format, 16 bit integer per channel.
            { TextureFormat.RG32 , 32 },
        //
        // Summary:
        //     Three channel (RGB) texture format, 16 bit integer per channel.
            { TextureFormat.RGB48 , 48 },
        //
        // Summary:
        //     Four channel (RGBA) texture format, 16 bit integer per channel.
            { TextureFormat.RGBA64 , 64 }
        };

        static Dictionary<RenderTextureFormat, float> RT_BPP = new Dictionary<RenderTextureFormat, float>()
        {
            //
            // Summary:
            //     Color render texture format, 8 bits per channel.
            { RenderTextureFormat.ARGB32 , 32 },
            //
            // Summary:
            //     A depth render texture format.
            { RenderTextureFormat.Depth , 0 },
            //
            // Summary:
            //     Color render texture format, 16 bit floating point per channel.
            { RenderTextureFormat.ARGBHalf , 64 },
            //
            // Summary:
            //     A native shadowmap render texture format.
            { RenderTextureFormat.Shadowmap , 8 }, //guessed bpp
            //
            // Summary:
            //     Color render texture format.
            { RenderTextureFormat.RGB565 , 32 }, //guessed bpp
            //
            // Summary:
            //     Color render texture format, 4 bit per channel.
            { RenderTextureFormat.ARGB4444 , 16 }, 
            //
            // Summary:
            //     Color render texture format, 1 bit for Alpha channel, 5 bits for Red, Green and
            //     Blue channels.
            { RenderTextureFormat.ARGB1555 , 16 },
            //
            // Summary:
            //     Default color render texture format: will be chosen accordingly to Frame Buffer
            //     format and Platform.
            { RenderTextureFormat.Default , 32 }, //fuck
            //
            // Summary:
            //     Color render texture format. 10 bits for colors, 2 bits for alpha.
            { RenderTextureFormat.ARGB2101010 , 32 },
            //
            // Summary:
            //     Default HDR color render texture format: will be chosen accordingly to Frame
            //     Buffer format and Platform.
            { RenderTextureFormat.DefaultHDR , 128 }, //fuck
            //
            // Summary:
            //     Four color render texture format, 16 bits per channel, fixed point, unsigned
            //     normalized.
            { RenderTextureFormat.ARGB64 , 64 },
            //
            // Summary:
            //     Color render texture format, 32 bit floating point per channel.
            { RenderTextureFormat.ARGBFloat , 128 },
            //
            // Summary:
            //     Two color (RG) render texture format, 32 bit floating point per channel.
            { RenderTextureFormat.RGFloat , 64 },
            //
            // Summary:
            //     Two color (RG) render texture format, 16 bit floating point per channel.
            { RenderTextureFormat.RGHalf , 32 },
            //
            // Summary:
            //     Scalar (R) render texture format, 32 bit floating point.
            { RenderTextureFormat.RFloat , 32 },
            //
            // Summary:
            //     Scalar (R) render texture format, 16 bit floating point.
            { RenderTextureFormat.RHalf , 16 },
            //
            // Summary:
            //     Single channel (R) render texture format, 8 bit integer.
            { RenderTextureFormat.R8 , 8 },
            //
            // Summary:
            //     Four channel (ARGB) render texture format, 32 bit signed integer per channel.
            { RenderTextureFormat.ARGBInt , 128 },
            //
            // Summary:
            //     Two channel (RG) render texture format, 32 bit signed integer per channel.
            { RenderTextureFormat.RGInt , 64 },
            //
            // Summary:
            //     Scalar (R) render texture format, 32 bit signed integer.
            { RenderTextureFormat.RInt , 32 },
            //
            // Summary:
            //     Color render texture format, 8 bits per channel.
            { RenderTextureFormat.BGRA32 , 32 },
            //
            // Summary:
            //     Color render texture format. R and G channels are 11 bit floating point, B channel is 10 bit floating point.
            { RenderTextureFormat.RGB111110Float , 32 },
            //
            // Summary:
            //     Two color (RG) render texture format, 16 bits per channel, fixed point, unsigned normalized
            { RenderTextureFormat.RG32 , 32 },
            //
            // Summary:
            //     Four channel (RGBA) render texture format, 16 bit unsigned integer per channel.
            { RenderTextureFormat.RGBAUShort , 64 },
            //
            // Summary:
            //     Two channel (RG) render texture format, 8 bits per channel.
            { RenderTextureFormat.RG16 , 16 },
            //
            // Summary:
            //     Color render texture format, 10 bit per channel, extended range.
            { RenderTextureFormat.BGRA10101010_XR , 40 },
            //
            // Summary:
            //     Color render texture format, 10 bit per channel, extended range.
            { RenderTextureFormat.BGR101010_XR , 30 },
            //
            // Summary:
            //     Single channel (R) render texture format, 16 bit integer.
            { RenderTextureFormat.R16 , 16 }
        };


        struct TextureMeta : IComparable
        {
            public Texture texture;
            public string path;
            public bool isPoiyomi;
            public TextureImporter importer;
            public long size;
            public string sizeString;
            public TextureFormat? format;
            public RenderTextureFormat? RT_format;
            public string formatString;
            public float BPP;
            public int minBPP;
            public bool hasAlpha;
            public List<Material> materials;
            public string firstMaterialName;
            public bool materialDropDown;
            public int pcResolution;
            public int androidResolution;
            public bool textureWithChangableResolution;
            public bool textureWithChangableFormat;
            public TextureImporterFormat? betterTextureFormat;
            public string savedSizeWithBetterTextureFormat;
            public bool textureTooBig;
            public string saveSizeWithSmallerTexture;
            public TextureImporterFormat? bestTextureFormat;

            public void calculate(Texture t, List<Material> materials) {
                try {
                texture = t;
                path = AssetDatabase.GetAssetPath(texture);
                isPoiyomi = path.Contains("_PoiyomiShaders");
                importer = AssetImporter.GetAtPath(path) as TextureImporter;
                format = GetTextureFormat();
                RT_format = GetRenderTextureFormat();
                formatString = format != null ? format.ToString() : RT_format != null ? RT_format.ToString() : "";
                hasAlpha = getHasAlpha();
                BPP = getBPP();
                minBPP = getMinBPP();
                size = getSize();
                sizeString = AvatarEvaluator.ToMebiByteString(size);
                this.materials = materials;
                firstMaterialName = GetFirstMaterialName();
                pcResolution = GetMaxResolution("PC");
                androidResolution = GetMaxResolution("Android");
                textureWithChangableResolution = importer != null && !isPoiyomi;
                textureWithChangableFormat = getTextureHasChangableFormat();
                betterTextureFormat = getBetterFormat();
                savedSizeWithBetterTextureFormat = getSavedSizeString();
                textureTooBig = importer != null && pcResolution > 2048;
                saveSizeWithSmallerTexture = AvatarEvaluator.ToShortMebiByteString(size - TextureToBytesUsingBPP(texture, BPP, 2048f / pcResolution));
                bestTextureFormat = getTheBestFormat();
                
                } catch (Exception e) {
                    Debug.LogError($"Error for texture {texture.name}");
                    throw e;
                }
                
            }

            public void ChangeImportSize(int size)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("PC");
                importer.maxTextureSize = size;
                settings.maxTextureSize = size;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }

            public void ChangeImportSizeAndroid(int size)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
                settings.maxTextureSize = size;
                settings.overridden = true;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }

            public void ChangeCompression(TextureImporterFormat format)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("PC");
                TextureImporterPlatformSettings settingsA = importer.GetPlatformTextureSettings("Android");

                settings.overridden = (int)format != -1;
                settings.format = format;
                settings.compressionQuality = 100;

                settingsA.overridden = (int)format != -1;
                settingsA.format = format;
                settingsA.compressionQuality = 100;

                importer.SetPlatformTextureSettings(settings);
                importer.SetPlatformTextureSettings(settingsA);

                importer.SaveAndReimport();
            }
            public int CompareTo(object obj)
            {
                if (obj is TextureMeta) {
                  return firstMaterialName.CompareTo(((TextureMeta) obj).firstMaterialName);
                }

                return 0;
            }

            private TextureImporterFormat? getBetterFormat() {
                if (textureWithChangableFormat && BPP > minBPP) {
                    return getTheBestFormat();
                }

                return null;
            }

            private TextureImporterFormat? getTheBestFormat() {
                if (textureWithChangableFormat) {
                    return hasAlpha || importer.textureType == TextureImporterType.NormalMap ? TextureImporterFormat.DXT5Crunched : TextureImporterFormat.DXT1Crunched;
                }

                return null;
            }

            private string getSavedSizeString() {
                if (betterTextureFormat != null)
                {
                    return AvatarEvaluator.ToShortMebiByteString(size - TextureToBytesUsingBPP(texture, TextureManager.BPP[(TextureFormat) betterTextureFormat]));
                }

                return "";
            }

            private int GetMaxResolution(string platform) {
                if (importer) {
                    return importer.GetPlatformTextureSettings(platform).overridden ? importer.GetPlatformTextureSettings(platform).maxTextureSize : importer.GetDefaultPlatformTextureSettings().maxTextureSize;
                }

                return 0;
            }

            private string GetFirstMaterialName() {
                if (materials.Count == 0) {
                    return DEFAULT_MAT_NAME;
                }

                if (materials.Count == 1) {
                    return materials[0].name;
                }

                return $"{materials[0].name} + {materials.Count - 1} more";
            }

            private TextureFormat? GetTextureFormat() => texture switch
            {
                Texture2D       => ((Texture2D) texture).format,
                Texture2DArray  => ((Texture2DArray) texture).format,
                Cubemap         => ((Cubemap) texture).format,
                _               => null,
            };

            private RenderTextureFormat? GetRenderTextureFormat() => texture switch
            {
                RenderTexture   => ((RenderTexture) texture).format,
                _               => null,
            };

            private float getBPP() => texture switch
            {
                Texture2D       => TextureManager.BPP.GetValueOrDefault((TextureFormat) format, 16),
                Texture2DArray  => TextureManager.BPP.GetValueOrDefault((TextureFormat) format, 16),
                Cubemap         => TextureManager.BPP.GetValueOrDefault((TextureFormat) format, 16),
                RenderTexture   => TextureManager.RT_BPP.GetValueOrDefault((RenderTextureFormat) RT_format, 16) + ((RenderTexture) texture).depth,
                _               => 16,
            };

            private long getSize() => texture switch
            {
                Texture2D       => TextureToBytesUsingBPP(texture, BPP),
                Texture2DArray  => TextureToBytesUsingBPP(texture, BPP) * ((Texture2DArray) texture).depth,
                Cubemap         => TextureToBytesUsingBPP(texture, BPP) * ((((Cubemap) texture).dimension == TextureDimension.Tex3D) ? 6 : 1),
                RenderTexture   => TextureToBytesUsingBPP(texture, BPP),
                _               => Profiler.GetRuntimeMemorySizeLong(texture),
            };

            private Boolean getHasAlpha() => texture switch
            {
                Texture2D       => importer != null ? importer.DoesSourceTextureHaveAlpha() : false,
                RenderTexture   => RT_format == RenderTextureFormat.ARGB32 || RT_format == RenderTextureFormat.ARGBHalf || RT_format == RenderTextureFormat.ARGBFloat,
                _               => false,
            };

            private int getMinBPP() =>  texture switch
            {
                Texture2D       => (hasAlpha || importer != null && importer.textureType == TextureImporterType.NormalMap) ? 8 : 4,
                _               => 8,
            };

            private bool getTextureHasChangableFormat() =>  texture switch
            {
                Texture2D       => !isPoiyomi && importer != null && importer.textureType != TextureImporterType.SingleChannel && formatString.Length > 0,
                _               => false,
            };
        }

        GameObject _avatar;
        List<TextureMeta> _texturesList;

        Vector2 _scrollPosMajor;

        GUIStyle styleButtonTextFloatLeft;
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


        private void OnEnable() {
            matActiveIcon = EditorGUIUtility.IconContent("d_Material Icon");
            matInactiveIcon = EditorGUIUtility.IconContent("d_Material On Icon");
            refreshIcon = EditorGUIUtility.IconContent("RotateTool On", "Recalculate");
        }

        private void InitilizeStyles()
        {
            styleButtonTextFloatLeft = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
        }

        private void OnGUI()
        {
            if(styleButtonTextFloatLeft == null)
            {
                InitilizeStyles();
            }

            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                //GUILayout.Label("Avatar", GUILayout.Width(40));
                GUI.enabled = _avatar != null;
                if(GUILayout.Button(refreshIcon, GUILayout.Width(30), GUILayout.Height(30))) {
                    Calc(_avatar);
                }
                GUI.enabled = true;

                _avatar = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _avatar, typeof(GameObject), true, GUILayout.Height(30));
                if (EditorGUI.EndChangeCheck() && _avatar != null) {
                    Calc(_avatar);
                }

            }

            if (_avatar != null)
            {
                _scrollPosMajor = EditorGUILayout.BeginScrollView(_scrollPosMajor);

                GUILine();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

                if (_texturesList == null) Calc(_avatar);
                if (_texturesList != null)
                {

                    EditorGUILayout.BeginHorizontal();
                    bool makeAllTexturesLessThan2k = GUILayout.Button(new GUIContent("Textures max size -> 2k"), EditorStyles.miniButtonLeft, GUILayout.Width(200), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Makes max texture size 2k", GUILayout.MinWidth(200), GUILayout.Height(ROW_HEIGHT));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    bool makeAllTexturesForAndroid = GUILayout.Button(new GUIContent("Prepare textures for Android"), EditorStyles.miniButtonLeft, GUILayout.Width(200), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Makes max texture for android half size of PC size", GUILayout.MinWidth(200), GUILayout.Height(ROW_HEIGHT));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    bool makeAlltexturesCrunched = GUILayout.Button(new GUIContent("Crunch textures"), EditorStyles.miniButtonLeft, GUILayout.Width(200), GUILayout.Height(ROW_HEIGHT));
                     GUILayout.Label("Sets texture format to DTX1Crunched or DTX5Crunched", GUILayout.MinWidth(200), GUILayout.Height(ROW_HEIGHT));
                    EditorGUILayout.EndHorizontal();

                    if (makeAllTexturesLessThan2k || makeAllTexturesForAndroid || makeAlltexturesCrunched) 
                    {
                        for (int texIdx = 0; texIdx < _texturesList.Count; texIdx++)
                        {
                            TextureMeta textureMeta = _texturesList[texIdx];

                            if (makeAllTexturesLessThan2k && textureMeta.textureWithChangableResolution && textureMeta.pcResolution > 2048) textureMeta.ChangeImportSize(2048);
                            if (makeAllTexturesForAndroid && textureMeta.textureWithChangableResolution && textureMeta.androidResolution > textureMeta.pcResolution / 2 && textureMeta.pcResolution > 512) textureMeta.ChangeImportSizeAndroid(textureMeta.pcResolution / 2);
                            if (makeAlltexturesCrunched && textureMeta.textureWithChangableFormat && textureMeta.bestTextureFormat != null) textureMeta.ChangeCompression((TextureImporterFormat) textureMeta.bestTextureFormat);
                        }

                        Calc(_avatar);
                    }

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label("Material", GUILayout.Width(MATERIAL_BUTTON_WTDTH + MATERIAL_NAME_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Texture", GUILayout.Width(TEXTURE_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Size", GUILayout.Width(SIZE_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("PC", GUILayout.Width(PC_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("ANDROID", GUILayout.Width(ANDROID_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Format", GUILayout.Width(FORMAT_WIDTH), GUILayout.Height(ROW_HEIGHT));
                    GUILayout.Label("Actions", GUILayout.Width(ACTIONS_WIDTH), GUILayout.Height(ROW_HEIGHT));

                    EditorGUILayout.EndHorizontal();

                    for (int texIdx = 0; texIdx < _texturesList.Count; texIdx++)
                    {
                        TextureMeta textureMeta = _texturesList[texIdx];

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical("box");

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(textureMeta.materials.Count < 1))
                            {
                                GUIContent content = textureMeta.materialDropDown ? matActiveIcon : matInactiveIcon;
                                if (GUILayout.Button(content, GUILayout.Width(MATERIAL_BUTTON_WTDTH), GUILayout.Height(ROW_HEIGHT)))
                                {
                                    textureMeta.materialDropDown = !textureMeta.materialDropDown;
                                    _texturesList[texIdx] = textureMeta;
                                }
                            }

                            GUILayout.Label(textureMeta.firstMaterialName, GUILayout.Width(MATERIAL_NAME_WIDTH), GUILayout.Height(ROW_HEIGHT));
                            EditorGUILayout.ObjectField(textureMeta.texture, typeof(object), false, GUILayout.MinWidth(TEXTURE_WIDTH), GUILayout.Height(ROW_HEIGHT));
                            GUILayout.Label(textureMeta.sizeString, GUILayout.Width(SIZE_WIDTH), GUILayout.Height(ROW_HEIGHT));
                            
                            if (textureMeta.textureWithChangableResolution)
                            {
                                _textureSizeOptions[0] = textureMeta.pcResolution;
                                int newResolution = EditorGUILayout.IntPopup(textureMeta.pcResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions, GUILayout.Width(PC_WIDTH), GUILayout.Height(ROW_HEIGHT));
                                if(newResolution != textureMeta.pcResolution) 
                                {
                                    textureMeta.ChangeImportSize(newResolution);
                                    Calc(_avatar);
                                }

                                _textureSizeOptions[0] = textureMeta.androidResolution;
                                int newResolutionAndroid = EditorGUILayout.IntPopup(textureMeta.androidResolution, _textureSizeOptions.Select(x => x.ToString()).ToArray(), _textureSizeOptions, GUILayout.Width(ANDROID_WIDTH), GUILayout.Height(ROW_HEIGHT));
                                if(newResolutionAndroid != textureMeta.androidResolution) 
                                {
                                    textureMeta.ChangeImportSizeAndroid(newResolutionAndroid);
                                    Calc(_avatar);
                                }
                                
                            } else 
                            {
                                GUILayout.Space(PC_WIDTH + ANDROID_WIDTH);
                            }
                           
                           
                            if(textureMeta.formatString.Length > 0)
                            {
                                if(textureMeta.textureWithChangableFormat)
                                {
                                    _compressionFormatOptions[0] = ((TextureImporterFormat)textureMeta.format);
                                    int newFormat = EditorGUILayout.Popup(0, _compressionFormatOptions.Select(x => x.ToString()).ToArray(), GUILayout.Width(FORMAT_WIDTH), GUILayout.Height(ROW_HEIGHT));
                                    if(newFormat != 0) {
                                        textureMeta.ChangeCompression(_compressionFormatOptions[newFormat]);
                                        Calc(_avatar);
                                    }

                                } else
                                {
                                    if(GUILayout.Button(new GUIContent(textureMeta.formatString), EditorStyles.label, GUILayout.Width(FORMAT_WIDTH), GUILayout.Height(ROW_HEIGHT)))
                                        Application.OpenURL("https://docs.unity.cn/2019.4/Documentation/Manual/class-TextureImporterOverride.html");
                                }
                            }else
                            {
                                GUILayout.Space(FORMAT_WIDTH);
                            }
                            
                            if (textureMeta.betterTextureFormat != null)
                            {
                                if (GUILayout.Button($"{textureMeta.betterTextureFormat} → -{textureMeta.savedSizeWithBetterTextureFormat}", styleButtonTextFloatLeft, GUILayout.Width(ACTIONS_WIDTH), GUILayout.Height(ROW_HEIGHT))
                                    && EditorUtility.DisplayDialog("Confirm Compression Format Change!", $"You are about to change the compression format of texture '{textureMeta.texture.name}' from {textureMeta.format} => {textureMeta.betterTextureFormat}\n\n" +
                                    $"If you wish to return this texture's compression to {textureMeta.formatString}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?", "Yes", "No"))
                                {
                                   textureMeta.ChangeCompression((TextureImporterFormat) textureMeta.betterTextureFormat);
                                   Calc(_avatar);
                                }
                            }

                            if(textureMeta.textureTooBig)
                            {
                                if (GUILayout.Button($"2k → -{textureMeta.saveSizeWithSmallerTexture}", styleButtonTextFloatLeft, GUILayout.Width(ACTIONS_WIDTH), GUILayout.Height(ROW_HEIGHT)))
                                {
                                    textureMeta.ChangeImportSize(2048);
                                    Calc(_avatar);
                                }
                            }
                            else
                            {
                                
                            }
                            GUILayout.FlexibleSpace();
                        }
                        if (textureMeta.materialDropDown)
                        {
                            GUILayout.Label($"Used in {textureMeta.materials.Count()} material(s) on '{_avatar.name}'", EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;
                            foreach (Material mat in textureMeta.materials){
                                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(395), GUILayout.Height(ROW_HEIGHT));
                            }
                            EditorGUI.indentLevel--;
                            GUILayout.Space(5);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        static void GUILine(int i_height = 1)
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            if (rect != null){
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

        List<Material> GetMaterialsUsingTexture(Texture texture, List<Material> materialsToSearch) {
            List<Material> materials = new List<Material>();

            foreach(Material mat in materialsToSearch) {
                foreach (string propName in mat.GetTexturePropertyNames()) {
                    Texture matTex = mat.GetTexture(propName);
                    if (matTex != null && matTex == texture) {
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
                
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        

        static long TextureToBytesUsingBPP(Texture t, float bpp, float resolutionScale = 1)
        {
            int width = (int)(t.width * resolutionScale);
            int height = (int)(t.height * resolutionScale);
            long bytes = 0;
            if (t is Texture2D || t is Texture2DArray || t is Cubemap)
            {
                for (int index = 0; index < t.mipmapCount; ++index)
                    bytes += (long) Mathf.RoundToInt((float) ((width * height) >> 2 * index) * bpp / 8);
            }
            else if (t is RenderTexture)
            {
                RenderTexture rt = t as RenderTexture;
                double mipmaps = 1;
                for (int i = 0; i < rt.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                bytes = (long)((RT_BPP[rt.format] + rt.depth) * width * height * (rt.useMipMap ? mipmaps : 1) / 8);
            }
            else
            {
                bytes = Profiler.GetRuntimeMemorySizeLong(t);
            }
            return bytes;
        }
    }
}
#endif
