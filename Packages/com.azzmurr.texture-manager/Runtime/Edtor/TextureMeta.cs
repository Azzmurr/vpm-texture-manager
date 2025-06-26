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
        public string Path => AssetDatabase.GetAssetPath(texture);
        public bool Poiyomi => Path.Contains("_PoiyomiShaders");
        public TextureImporter Importer => AssetImporter.GetAtPath(Path) as TextureImporter;
        public long Size => getSize();
        public string SizeString => ToMebiByteString(Size);
        public TextureFormat? Format => GetTextureFormat();
        public RenderTextureFormat? RT_format => GetRenderTextureFormat();
        public string FormatString => Format != null ? Format.ToString() : RT_format != null ? RT_format.ToString() : "";
        public float BPP => getBPP();
        public int MinBPP => getMinBPP();
        public bool HasAlpha => getHasAlpha();
        public int PcResolution => GetMaxResolution("PC");
        public int AndroidResolution => GetMaxResolution("Android");
        public bool TextureWithChangableResolution => Importer != null && !Poiyomi;
        public bool TextureWithChangableFormat => getTextureHasChangableFormat();
        public TextureImporterFormat? BetterTextureFormat => getBetterFormat();
        public string SavedSizeWithBetterTextureFormat => getSavedSizeString();
        public bool TextureTooBig => Importer != null && PcResolution > 2048;
        public string SaveSizeWithSmallerTexture => ToShortMebiByteString(Size - TextureToBytesUsingBPP(texture, BPP, 2048f / PcResolution));
        public TextureImporterFormat? BestTextureFormat => getTheBestFormat();

        public TextureMeta(Texture t)
        {
            texture = t;
        }

        public void ChangeImportSize(int size)
        {
            if (TextureWithChangableResolution)
            {
                TextureImporterPlatformSettings settings = Importer.GetPlatformTextureSettings("PC");
                Importer.maxTextureSize = size;
                settings.maxTextureSize = size;
                Importer.SetPlatformTextureSettings(settings);
                Importer.SaveAndReimport();
            }
        }

        public void ChangeImportSizeAndroid(int size)
        {
            if (TextureWithChangableResolution)
            {
                TextureImporterPlatformSettings settings = Importer.GetPlatformTextureSettings("Android");
                settings.maxTextureSize = size;
                settings.overridden = true;
                Importer.SetPlatformTextureSettings(settings);
                Importer.SaveAndReimport();
            }
        }

        public void ChangeImporterFormat(TextureImporterFormat format)
        {
            if (TextureWithChangableFormat)
            {
                TextureImporterPlatformSettings settings = Importer.GetPlatformTextureSettings("PC");
                TextureImporterPlatformSettings settingsA = Importer.GetPlatformTextureSettings("Android");

                settings.overridden = (int)format != -1;
                settings.format = format;
                settings.compressionQuality = 100;

                settingsA.overridden = (int)format != -1;
                settingsA.format = format;
                settingsA.compressionQuality = 100;

                Importer.SetPlatformTextureSettings(settings);
                Importer.SetPlatformTextureSettings(settingsA);

                Importer.SaveAndReimport();
            }
        }

        private TextureImporterFormat? getBetterFormat()
        {
            if (TextureWithChangableFormat && BPP > MinBPP)
            {
                return getTheBestFormat();
            }

            return null;
        }

        private TextureImporterFormat? getTheBestFormat()
        {
            if (TextureWithChangableFormat)
            {
                return HasAlpha || Importer.textureType == TextureImporterType.NormalMap ? TextureImporterFormat.DXT5Crunched : TextureImporterFormat.DXT1Crunched;
            }

            return null;
        }

        private string getSavedSizeString()
        {
            if (BetterTextureFormat != null)
            {
                return ToShortMebiByteString(Size - TextureToBytesUsingBPP(texture, BPPConfig.BPP[(TextureFormat)BetterTextureFormat]));
            }

            return "";
        }

        private int GetMaxResolution(string platform)
        {
            if (Importer)
            {
                return Importer.GetPlatformTextureSettings(platform).overridden ? Importer.GetPlatformTextureSettings(platform).maxTextureSize : Importer.GetDefaultPlatformTextureSettings().maxTextureSize;
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
            Texture2D => BPPConfig.BPP.GetValueOrDefault((TextureFormat)Format, 16),
            Texture2DArray => BPPConfig.BPP.GetValueOrDefault((TextureFormat)Format, 16),
            Cubemap => BPPConfig.BPP.GetValueOrDefault((TextureFormat)Format, 16),
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
            Texture2D => Importer != null ? Importer.DoesSourceTextureHaveAlpha() : false,
            RenderTexture => RT_format == RenderTextureFormat.ARGB32 || RT_format == RenderTextureFormat.ARGBHalf || RT_format == RenderTextureFormat.ARGBFloat,
            _ => false,
        };

        private int getMinBPP() => texture switch
        {
            Texture2D => (HasAlpha || Importer != null && Importer.textureType == TextureImporterType.NormalMap) ? 8 : 4,
            _ => 8,
        };

        private bool getTextureHasChangableFormat() => texture switch
        {
            Texture2D => !Poiyomi && Importer != null && Importer.textureType != TextureImporterType.SingleChannel && FormatString.Length > 0,
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