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

        public string Name
        {
            get { return material.name; }
        }

        public Material Material
        {
            get { return material; }
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