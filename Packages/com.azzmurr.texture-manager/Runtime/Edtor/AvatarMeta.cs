using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using System;

namespace Azzmurr.Utils
{
    class AvatarMeta
    {
        private GameObject avatarObject;
        private IEnumerable<Material> materials;
        private IEnumerable<TextureMeta> textures;

        public string Name
        {
            get { return avatarObject.name; }
        }

        public GameObject GameObject
        {
            get { return avatarObject; }
        }

        public int MaterialsCount
        {
            get { return materials != null ? materials.Count() : 0; }
        }

        public int TextureCount
        {
            get { return textures != null ? textures.Count() : 0; }
        }

        public AvatarMeta(GameObject gameObject)
        {
            avatarObject = gameObject;
            recalculate();
        }

        public void recalculate()
        {
            EditorUtility.DisplayProgressBar("Getting Avatar Data", "Getting Materials", 0.3f);
            materials = GetMaterials();

            EditorUtility.DisplayProgressBar("Getting Avatar Data", "Getting Textures", 0.6f);
            textures = GetTextures();

            EditorUtility.ClearProgressBar();
        }

        public void ForeachMaterial(Action<Material> action)
        {
            foreach (Material material in materials)
            {
                action.Invoke(material);
            }
        }

        public void ForeachTexture(Action<TextureMeta> action)
        {
            foreach (TextureMeta texture in textures)
            {
                action.Invoke(texture);
            }
        }

        private IEnumerable<Material> GetMaterials()
        {
            IEnumerable<Renderer> allBuiltRenderers = avatarObject
                .GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.gameObject.GetComponentsInParent<Transform>(true)
                .All(transform => transform.tag != "EditorOnly"));

            List<Material> materialsAll = allBuiltRenderers.SelectMany(r => r.sharedMaterials).ToList();
            VRCAvatarDescriptor descriptor = avatarObject.GetComponent<VRCAvatarDescriptor>();

            if (descriptor != null)
            {
                IEnumerable<AnimationClip> clips = descriptor
                    .baseAnimationLayers
                    .Select(layer => layer.animatorController)
                    .Where(controller => controller != null)
                    .SelectMany(controller => controller.animationClips)
                    .Distinct();

                foreach (AnimationClip clip in clips)
                {
                    IEnumerable<Material> clipMaterials = AnimationUtility
                        .GetObjectReferenceCurveBindings(clip)
                        .Where(binding => binding.isPPtrCurve && binding.type.IsSubclassOf(typeof(Renderer)) && binding.propertyName.StartsWith("m_Materials"))
                        .SelectMany(binding => AnimationUtility.GetObjectReferenceCurve(clip, binding))
                        .Select(r => r.value as Material);

                    materialsAll.AddRange(clipMaterials);
                }
            }

            return materialsAll.Distinct();
        }

        private IEnumerable<TextureMeta> GetTextures()
        {
            HashSet<Texture> textures = new HashSet<Texture>();

            foreach (Material material in materials)
            {
                if (material == null) continue;

                int[] textureIds = material.GetTexturePropertyNameIDs();

                foreach (int id in textureIds)
                {
                    if (!material.HasProperty(id)) continue;

                    Texture texture = material.GetTexture(id);
                    if (texture == null) continue;

                    textures.Add(texture);

                }
            }

            List<TextureMeta> textureMetas = textures
                .ToList()
                .ConvertAll((texture) => new TextureMeta(texture, GetMaterialsUsingTexture(texture, materials)));

            textureMetas.Sort((t1, t2) => t1.CompareTo(t2));

            return textureMetas;
                
        }

        private List<Material> GetMaterialsUsingTexture(Texture texture, IEnumerable<Material> materialsToSearch)
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
    }
}