using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Azzmurr.Utils
{
    class MaterialMeta
    {
        readonly private Material material;
        readonly private IEnumerable<TextureMeta> textures;

        public string Name => material.name;

        public Material Material => material;

        public Shader Shader => material.shader;

        public bool Poiyomi => Shader.name.Contains(".poiyomi");

        public bool Standard => Shader.name == "Standard";

        public bool FastFur => Shader.name.Contains("Fast Fur");

        public bool Goo => Shader.name.Contains(".ValueFactory");

        public bool Locked => Shader.name.Contains("Hidden");

        public string ShaderName
        {
            get
            {
                if (Locked) return Shader.name.Split("/")[^2];
                return Shader.name.Split("/")[^1];
            }
        }

        public string ShaderVersion
        {
            get
            {
                if (Standard)
                {
                    return "Latest";
                }

                if (Poiyomi)
                {
                    if (Shader.name.Contains("Hidden") && Shader.name.Contains("Old Versions")) return $"{Shader.name.Split("/")[4]}";
                    if (!Shader.name.Contains("Hidden") && Shader.name.Contains("Old Versions")) return Shader.name.Split("/")[2];
                    return "Latest";
                }

                if (FastFur)
                {
                    if (Shader.name.Contains("Hidden")) return $"{Shader.name.Split("/")[2]}";
                    return Shader.name.Split("/")[0];
                }

                if (Goo)
                {
                    if (Shader.name.Contains("Hidden")) return $"{Shader.name.Split("/")[2]}";
                    return Shader.name.Split("/")[1];
                }

                return "Unknown";
            }
        }

        public MaterialMeta(Material material)
        {
            this.material = material;
            textures = GetTextures();
        }

        public void ForeachTexture(Action<TextureMeta> action)
        {
            foreach (TextureMeta texture in textures)
            {
                action.Invoke(texture);
            }
        }

        public Material getQuestMaterial()
        {
            Material newQuestMaterial = new Material(material);
            newQuestMaterial.shader = Shader.Find("VRChat/Mobile/Standard Lite");
            return newQuestMaterial;
        }

        private IEnumerable<TextureMeta> GetTextures()
        {
            HashSet<Texture> textures = new HashSet<Texture>();
            int[] textureIds = material.GetTexturePropertyNameIDs();
            string[] textureNames = material.GetTexturePropertyNames();
            Debug.Log($"Shader Version - {ShaderName}");

            foreach (int id in textureIds)
            {
                if (!material.HasProperty(id)) continue;
                Texture texture = material.GetTexture(id);
                if (texture == null) continue;

                textures.Add(texture);
            }

            return textures.ToList().ConvertAll((texture) => new TextureMeta(texture));
        }
    }
}