using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using System;
using HarmonyLib;

namespace Azzmurr.Utils
{
    class AvatarMeta
    {
        private GameObject avatarObject;
        private IEnumerable<MaterialMeta> materials;
        private IEnumerable<TextureMeta> textures;

        private Dictionary<Texture, HashSet<Material>> materialsRelatedToTextures = new();

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
            Recalculate();
        }

        public void Recalculate()
        {
            EditorUtility.DisplayProgressBar("Getting Avatar Data", "Getting Materials", 0.3f);
            materials = GetMaterials();

            EditorUtility.DisplayProgressBar("Getting Avatar Data", "Getting Textures", 0.6f);
            textures = GetTextures();

            EditorUtility.ClearProgressBar();
        }

        public void ForeachMaterial(Action<MaterialMeta> action)
        {
            foreach (MaterialMeta material in materials)
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

        public void ForeachTextureMaterial(TextureMeta texture, Action<Material> action)
        {
            if (materialsRelatedToTextures[texture.texture] != null)
            {
                foreach (Material material in materialsRelatedToTextures[texture.texture])
                {
                    action.Invoke(material);
                }
            }
        }

        private IEnumerable<MaterialMeta> GetMaterials()
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

            List<MaterialMeta> materialMetas = materialsAll
                .ToList()
                .ConvertAll((material) => new MaterialMeta(material));

            return materialMetas.Distinct();
        }

        private IEnumerable<TextureMeta> GetTextures()
        {
            HashSet<TextureMeta> textures = new HashSet<TextureMeta>();
            materialsRelatedToTextures = new();

            ForeachMaterial((material) =>
            {
                if (material == null) return;

                material.ForeachTexture((texture) =>
                {
                    if (materialsRelatedToTextures.ContainsKey(texture.texture))
                    {
                        HashSet<Material> materials = materialsRelatedToTextures.GetValueSafe(texture.texture);
                        materials.Add(material.Material);
                    }
                    else
                    {
                        materialsRelatedToTextures.Add(texture.texture, new HashSet<Material> { material.Material });
                        textures.Add(texture);
                    }
                });
            });

            List<TextureMeta> textureMetas = textures.ToList();
            textureMetas.Sort((t1, t2) =>
            {
                string material1 = materialsRelatedToTextures[t1.texture].ToList()[0].name;
                string material2 = materialsRelatedToTextures[t2.texture].ToList()[0].name;

                return material1.CompareTo(material2);
            });

            return textureMetas;
        }
    }
}