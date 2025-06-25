using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Azzmurr.Utils
{
    class TextureMeta : IEqualityComparer<TextureMeta>
    {
        readonly public Texture texture;
        readonly public string path;
        readonly public bool isPoiyomi;
        readonly public TextureImporter importer;
        readonly public long size;
        readonly public string sizeString;
        readonly public TextureFormat? format;
        readonly public RenderTextureFormat? RT_format;
        readonly public string formatString;
        readonly public float BPP;
        readonly public int minBPP;
        readonly public bool hasAlpha;
        readonly public List<Material> materials;
        readonly public string firstMaterialName;
        readonly public bool materialDropDown;
        readonly public int pcResolution;
        readonly public int androidResolution;
        readonly public bool textureWithChangableResolution;
        readonly public bool textureWithChangableFormat;
        readonly public TextureImporterFormat? betterTextureFormat;
        readonly public string savedSizeWithBetterTextureFormat;
        readonly public bool textureTooBig;
        readonly public string saveSizeWithSmallerTexture;
        readonly public TextureImporterFormat? bestTextureFormat;

        public TextureMeta(Texture t)
        {
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
            sizeString = ToMebiByteString(size);
            pcResolution = GetMaxResolution("PC");
            androidResolution = GetMaxResolution("Android");
            textureWithChangableResolution = importer != null && !isPoiyomi;
            textureWithChangableFormat = getTextureHasChangableFormat();
            betterTextureFormat = getBetterFormat();
            savedSizeWithBetterTextureFormat = getSavedSizeString();
            textureTooBig = importer != null && pcResolution > 2048;
            saveSizeWithSmallerTexture = ToShortMebiByteString(size - TextureToBytesUsingBPP(texture, BPP, 2048f / pcResolution));
            bestTextureFormat = getTheBestFormat();
        }

        public void ChangeImportSize(int size)
        {
            if (textureWithChangableResolution)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("PC");
                importer.maxTextureSize = size;
                settings.maxTextureSize = size;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        public void ChangeImportSizeAndroid(int size)
        {
            if (textureWithChangableResolution)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
                settings.maxTextureSize = size;
                settings.overridden = true;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }


        }

        public void ChangeImporterFormat(TextureImporterFormat format)
        {
            if (textureWithChangableFormat)
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
        }

        private TextureImporterFormat? getBetterFormat()
        {
            if (textureWithChangableFormat && BPP > minBPP)
            {
                return getTheBestFormat();
            }

            return null;
        }

        private TextureImporterFormat? getTheBestFormat()
        {
            if (textureWithChangableFormat)
            {
                return hasAlpha || importer.textureType == TextureImporterType.NormalMap ? TextureImporterFormat.DXT5Crunched : TextureImporterFormat.DXT1Crunched;
            }

            return null;
        }

        private string getSavedSizeString()
        {
            if (betterTextureFormat != null)
            {
                return ToShortMebiByteString(size - TextureToBytesUsingBPP(texture, BPPConfig.BPP[(TextureFormat)betterTextureFormat]));
            }

            return "";
        }

        private int GetMaxResolution(string platform)
        {
            if (importer)
            {
                return importer.GetPlatformTextureSettings(platform).overridden ? importer.GetPlatformTextureSettings(platform).maxTextureSize : importer.GetDefaultPlatformTextureSettings().maxTextureSize;
            }

            return 0;
        }

        private TextureFormat? GetTextureFormat() => texture switch
        {
            Texture2D => ((Texture2D)texture).format,
            Texture2DArray => ((Texture2DArray)texture).format,
            Cubemap => ((Cubemap)texture).format,
            _ => null,
        };

        private RenderTextureFormat? GetRenderTextureFormat() => texture switch
        {
            RenderTexture => ((RenderTexture)texture).format,
            _ => null,
        };

        private float getBPP() => texture switch
        {
            Texture2D => BPPConfig.BPP.GetValueOrDefault((TextureFormat)format, 16),
            Texture2DArray => BPPConfig.BPP.GetValueOrDefault((TextureFormat)format, 16),
            Cubemap => BPPConfig.BPP.GetValueOrDefault((TextureFormat)format, 16),
            RenderTexture => BPPConfig.RT_BPP.GetValueOrDefault((RenderTextureFormat)RT_format, 16) + ((RenderTexture)texture).depth,
            _ => 16,
        };

        private long getSize() => texture switch
        {
            Texture2D => TextureToBytesUsingBPP(texture, BPP),
            Texture2DArray => TextureToBytesUsingBPP(texture, BPP) * ((Texture2DArray)texture).depth,
            Cubemap => TextureToBytesUsingBPP(texture, BPP) * ((((Cubemap)texture).dimension == TextureDimension.Tex3D) ? 6 : 1),
            RenderTexture => TextureToBytesUsingBPP(texture, BPP),
            _ => Profiler.GetRuntimeMemorySizeLong(texture),
        };

        private Boolean getHasAlpha() => texture switch
        {
            Texture2D => importer != null ? importer.DoesSourceTextureHaveAlpha() : false,
            RenderTexture => RT_format == RenderTextureFormat.ARGB32 || RT_format == RenderTextureFormat.ARGBHalf || RT_format == RenderTextureFormat.ARGBFloat,
            _ => false,
        };

        private int getMinBPP() => texture switch
        {
            Texture2D => (hasAlpha || importer != null && importer.textureType == TextureImporterType.NormalMap) ? 8 : 4,
            _ => 8,
        };

        private bool getTextureHasChangableFormat() => texture switch
        {
            Texture2D => !isPoiyomi && importer != null && importer.textureType != TextureImporterType.SingleChannel && formatString.Length > 0,
            _ => false,
        };

        private string ToMebiByteString(long l)
        {
            if (l < Math.Pow(2, 10)) return l + " B";
            if (l < Math.Pow(2, 20)) return (l / Math.Pow(2, 10)).ToString("n2") + " KiB";
            if (l < Math.Pow(2, 30)) return (l / Math.Pow(2, 20)).ToString("n2") + " MiB";
            else return (l / Math.Pow(2, 30)).ToString("n2") + " GiB";
        }

        private string ToShortMebiByteString(long l)
        {
            if (l < Math.Pow(2, 10)) return l + " B";
            if (l < Math.Pow(2, 20)) return (l / Math.Pow(2, 10)).ToString("n0") + " KiB";
            if (l < Math.Pow(2, 30)) return (l / Math.Pow(2, 20)).ToString("n1") + " MiB";
            else return (l / Math.Pow(2, 30)).ToString("n1") + " GiB";
        }

        private long TextureToBytesUsingBPP(Texture t, float bpp, float resolutionScale = 1)
        {
            int width = (int)(t.width * resolutionScale);
            int height = (int)(t.height * resolutionScale);
            long bytes = 0;
            if (t is Texture2D || t is Texture2DArray || t is Cubemap)
            {
                for (int index = 0; index < t.mipmapCount; ++index)
                    bytes += (long)Mathf.RoundToInt((float)((width * height) >> 2 * index) * bpp / 8);
            }
            else if (t is RenderTexture)
            {
                RenderTexture rt = t as RenderTexture;
                double mipmaps = 1;
                for (int i = 0; i < rt.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                bytes = (long)((BPPConfig.RT_BPP[rt.format] + rt.depth) * width * height * (rt.useMipMap ? mipmaps : 1) / 8);
            }
            else
            {
                bytes = Profiler.GetRuntimeMemorySizeLong(t);
            }
            return bytes;
        }

        public bool Equals(TextureMeta x, TextureMeta y)
        {
            return x.texture.Equals(y.texture);
        }

        public int GetHashCode(TextureMeta obj)
        {
            return obj.texture.GetHashCode();
        }
    }
}