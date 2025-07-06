using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using System.Collections;
#nullable enable

namespace Azzmurr.Utils
{
    class MaterialMeta
    {
        readonly private Material material;
        private IEnumerable<TextureMeta> Textures => GetTextures();

        public string Name => material.name;

        public Material Material => material;

        public Shader Shader => material.shader;

        public bool Poiyomi => Shader.name.Contains(".poiyomi");

        public bool Standard => Shader.name == "Standard";

        public bool StandardLite => Shader.name == "VRChat/Mobile/Standard Lite";

        public bool ToonStandard => Shader.name == "VRChat/Mobile/Toon Standard";

        public bool ToonLit => Shader.name == "VRChat/Mobile/Toon Lit";

        public bool FastFur => Shader.name.Contains("Fast Fur");

        public bool Goo => Shader.name.Contains(".ValueFactory");

        public bool NotLockable => ShaderVersion == "Unknown" || Standard || StandardLite || ToonStandard || ToonLit;

        public bool ShaderLocked => Shader.name.Contains("Hidden");

        public string ShaderLockedString
        {
            get
            {
                if (NotLockable) return "---";
                return Shader.name.Contains("Hidden").ToString();
            }
        }

        public string ShaderName
        {
            get
            {
                if (ShaderLocked) return Shader.name.Split("/")[^2];
                return Shader.name.Split("/")[^1];
            }
        }

        public bool? ShaderLockedError => !NotLockable ? !ShaderLocked : null;

        public string ShaderVersion
        {
            get
            {
                if (Standard || StandardLite || ToonStandard || ToonLit)
                {
                    return "---";
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

        public bool? ShaderVersionError => Poiyomi ? ShaderVersion != "Latest" : null;

        public MaterialMeta(Material material)
        {
            this.material = material;
        }

        public void ForeachTexture(Action<TextureMeta> action)
        {
            foreach (TextureMeta texture in Textures)
            {
                action.Invoke(texture);
            }
        }

        public Material GetQuestMaterial()
        {
            Material newQuestMaterial = new(material)
            {
                shader = Shader.Find("VRChat/Mobile/Toon Standard")
            };
            return newQuestMaterial;
        }

        private IEnumerable<TextureMeta> GetTextures()
        {
            HashSet<Texture> textures = new();
            int[] textureIds = material.GetTexturePropertyNameIDs();
            string[] textureNames = material.GetTexturePropertyNames();

            foreach (int id in textureIds)
            {
                if (!material.HasProperty(id)) continue;
                Texture texture = material.GetTexture(id);
                if (texture == null) continue;

                textures.Add(texture);
            }

            return textures.ToList().ConvertAll((texture) => new TextureMeta(texture));
        }

        public void RenderMaterialChecks()
        {
            if (Poiyomi)
            {
                using var ChecksGrid = new VariableGridScope(new float[] { 120, 100, 100, 50, 50 });
                ChecksGrid.Cell((_) => GUILayout.Label("Name"));
                ChecksGrid.Cell((_) => GUILayout.Label("Current"));
                ChecksGrid.Cell((_) => GUILayout.Label("Expected"));
                ChecksGrid.Cell((_) => GUILayout.Label("Status"));
                ChecksGrid.Cell((_) => GUILayout.Label("Action"));

                Dictionary<float, string> lmDictionary = new()
                {
                    { 0f, "TextureRamp" },
                    { 1f, "Multilayer Math" },
                    { 2f, "Wrapped" },
                    { 3f, "Skin" },
                    { 4f, "ShadeMap" },
                    { 5f, "Flat" },
                    { 6f, "Realistic" },
                    { 7f, "Cloth" },
                    { 8f, "Add Pass" },
                };

                Dictionary<float, string> booleanDictionary = new()
                {
                    { 0f, "Disabled" },
                    { 1f, "Enabled" },
                };

                Dictionary<float, string> bandDictionary = new()
                {
                    { 0f, "Bass" },
                    { 1f, "Low Mid" },
                    { 2f, "High Mid" },
                    { 3f, "Treble" },
                    { 4f, "Volume" },
                };

                RenderMaterialCheck(ChecksGrid, "_LightingMode", "Lighting mode", 6f, (value) => lmDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);

                RenderMaterialCheck(ChecksGrid, "_EnableAudioLink", "AudioLink", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);

                bool audioLinkEnabled = GetPropertyValue<float>("_EnableAudioLink") == 1f;

                RenderMaterialCheck(ChecksGrid, "_EnableEmission", "Emission Enabled 0", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                RenderMaterialCheck(ChecksGrid, "_EnableEmission1", "Emission Enabled 1", audioLinkEnabled ? 1f : 0f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                RenderMaterialCheck(ChecksGrid, "_EnableEmission2", "Emission Enabled 2", audioLinkEnabled ? 1f : 0f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                RenderMaterialCheck(ChecksGrid, "_EnableEmission3", "Emission Enabled 3", audioLinkEnabled ? 1f : 0f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);


                Texture mask = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_PoiyomiShaders/Textures/Noise/T_Noise_No (3).jpg");
                RenderMaterialCheck(ChecksGrid, "_EmissionMask", "Emission Mask 0", mask, null, (propertyName, value) =>
                {
                    Texture? old = GetPropertyValue<Texture>("_EmissionMask");
                    if (old != null && old != mask) SetPropertyValue("_EmissionMap", old);

                    SetPropertyValue(propertyName, value);
                }, null);

                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionMask1", "Emission Mask 1", mask, null, (propertyName, value) =>
                    {
                        Texture? old = GetPropertyValue<Texture>("_EmissionMask1");
                        if (old != null && old != mask) SetPropertyValue("_EmissionMap1", old);

                        SetPropertyValue(propertyName, value);
                    }, null);

                    RenderMaterialCheck(ChecksGrid, "_EmissionMask2", "Emission Mask 2", mask, null, (propertyName, value) =>
                    {
                        Texture? old = GetPropertyValue<Texture>("_EmissionMask2");
                        if (old != null && old != mask) SetPropertyValue("_EmissionMap2", old);

                        SetPropertyValue(propertyName, value);
                    }, null);

                    RenderMaterialCheck(ChecksGrid, "_EmissionMask3", "Emission Mask 3", mask, null, (propertyName, value) =>
                    {
                        Texture? old = GetPropertyValue<Texture>("_EmissionMask3");
                        if (old != null && old != mask) SetPropertyValue("_EmissionMap3", old);

                        SetPropertyValue(propertyName, value);
                    }, null);
                }


                RenderMaterialCheck(ChecksGrid, "_EmissionMap", "Emission Map 0", new Texture2D(0, 0), (value) => value == null ? "Nothing" : "Specified", (propertyName, value) => { }, (x, y) => x != null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionMap1", "Emission Map 1", new Texture2D(0, 0), (value) => value == null ? "Nothing" : "Specified", (propertyName, value) => { }, (x, y) => x != null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionMap2", "Emission Map 2", new Texture2D(0, 0), (value) => value == null ? "Nothing" : "Specified", (propertyName, value) => { }, (x, y) => x != null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionMap3", "Emission Map 3", new Texture2D(0, 0), (value) => value == null ? "Nothing" : "Specified", (propertyName, value) => { }, (x, y) => x != null);
                }

                RenderMaterialCheck(ChecksGrid, "_EmissionMaskPan", "Emission Vector 0", new Vector4(0f, -0.36f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionMask1Pan", "Emission Vector 1", new Vector4(0f, -0.36f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionMask2Pan", "Emission Vector 2", new Vector4(0f, -0.36f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionMask3Pan", "Emission Vector 3", new Vector4(0f, -0.36f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_EmissionColor", "Emission Color 0", new Color(0.14798677f, 1.05927384f, 0.06231023f, 1f), null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionColor1", "Emission Color 1", new Color(1.05927372f, 0.489466071f, 0f, 1f), null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionColor2", "Emission Color 2", new Color(0f, 0.338780373f, 1.05927384f, 1f), null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionColor3", "Emission Color 3", new Color(1.05927372f, 0.499134213f, 1.05927372f, 1f), null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_EmissionStrength", "Emission Strength 0", 1f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionStrength1", "Emission Strength 1", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionStrength2", "Emission Strength 2", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionStrength3", "Emission Strength 3", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_EmissionReplace0", "Emission Replace 0", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionReplace1", "Emission Replace 1", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionReplace2", "Emission Replace 2", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionReplace3", "Emission Replace 3", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_EmissionAL0Enabled", "AL enabled 0", audioLinkEnabled ? 1f : 0f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_EmissionAL1Enabled", "AL enabled 1", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionAL2Enabled", "AL enabled 2", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_EmissionAL3Enabled", "AL enabled 3", 1f, (value) => booleanDictionary[value], (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission0CenterOutBand", "AL Band 0", 3f, (value) => bandDictionary[value], (propertyName, value) =>
                {
                    SetPropertyValue(propertyName, value);
                    SetPropertyValue("_EmissionAL0MultipliersBand", value);
                    SetPropertyValue("_EmissionAL0StrengthBand", value);
                }, (x, y) =>
                {
                    return GetPropertyValue<float>("_EmissionAL0MultipliersBand") == y
                    && GetPropertyValue<float>("_EmissionAL0StrengthBand") == y
                    && GetPropertyValue<float>("_AudioLinkEmission0CenterOutBand") == y;
                });

                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission1CenterOutBand", "AL Band 1", 0f, (value) => bandDictionary[value], (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL1MultipliersBand", value);
                        SetPropertyValue("_EmissionAL1StrengthBand", value);
                    }, (x, y) =>
                    {
                        return GetPropertyValue<float>("_EmissionAL1MultipliersBand") == y
                        && GetPropertyValue<float>("_EmissionAL1StrengthBand") == y
                        && GetPropertyValue<float>("_AudioLinkEmission1CenterOutBand") == y;
                    });

                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission2CenterOutBand", "AL Band 2", 2f, (value) => bandDictionary[value], (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL2MultipliersBand", value);
                        SetPropertyValue("_EmissionAL2StrengthBand", value);
                    }, (x, y) =>
                    {
                        return GetPropertyValue<float>("_EmissionAL2MultipliersBand") == y
                        && GetPropertyValue<float>("_EmissionAL2StrengthBand") == y
                        && GetPropertyValue<float>("_AudioLinkEmission2CenterOutBand") == y;
                    });

                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission3CenterOutBand", "AL Band 3", 1f, (value) => bandDictionary[value], (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL3MultipliersBand", value);
                        SetPropertyValue("_EmissionAL3StrengthBand", value);
                    }, (x, y) =>
                    {
                        return GetPropertyValue<float>("_EmissionAL3MultipliersBand") == y
                        && GetPropertyValue<float>("_EmissionAL3StrengthBand") == y
                        && GetPropertyValue<float>("_AudioLinkEmission3CenterOutBand") == y;
                    });
                }

                RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission0CenterOut", "AL strength 0", new Vector4(0f, 1f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) =>
                {
                    SetPropertyValue(propertyName, value);
                    SetPropertyValue("_EmissionAL0Multipliers", new Vector4(0f, 0f, 0f, 0f));
                    SetPropertyValue("_EmissionAL0StrengthMod", new Vector4(0f, 0f, 0f, 0f));
                }, (x, y) =>
                {
                    return GetPropertyValue<Vector4>("_EmissionAL0Multipliers") == new Vector4(0f, 0f, 0f, 0f)
                    && GetPropertyValue<Vector4>("_EmissionAL0StrengthMod") == new Vector4(0f, 0f, 0f, 0f)
                    && GetPropertyValue<Vector4>("_AudioLinkEmission0CenterOut") == y;
                });

                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission1CenterOut", "AL strength 1", new Vector4(0f, 1f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL1Multipliers", new Vector4(0f, 0f, 0f, 0f));
                        SetPropertyValue("_EmissionAL1StrengthMod", new Vector4(0f, 0f, 0f, 0f));
                    }, (x, y) =>
                    {
                        return GetPropertyValue<Vector4>("_EmissionAL1Multipliers") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_EmissionAL1StrengthMod") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_AudioLinkEmission1CenterOut") == y;
                    });

                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission2CenterOut", "AL strength 2", new Vector4(0f, 1f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL2Multipliers", new Vector4(0f, 0f, 0f, 0f));
                        SetPropertyValue("_EmissionAL2StrengthMod", new Vector4(0f, 0f, 0f, 0f));
                    }, (x, y) =>
                    {
                        return GetPropertyValue<Vector4>("_EmissionAL2Multipliers") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_EmissionAL2StrengthMod") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_AudioLinkEmission2CenterOut") == y;
                    });

                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission3CenterOut", "AL strength 3", new Vector4(0f, 1f, 0f, 0f), (value) => $"{value.x}, {value.y}", (propertyName, value) =>
                    {
                        SetPropertyValue(propertyName, value);
                        SetPropertyValue("_EmissionAL3Multipliers", new Vector4(0f, 0f, 0f, 0f));
                        SetPropertyValue("_EmissionAL3StrengthMod", new Vector4(0f, 0f, 0f, 0f));
                    }, (x, y) =>
                    {
                        return GetPropertyValue<Vector4>("_EmissionAL3Multipliers") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_EmissionAL3StrengthMod") == new Vector4(0f, 0f, 0f, 0f)
                        && GetPropertyValue<Vector4>("_AudioLinkEmission3CenterOut") == y;
                    });
                }

                RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission0CenterOutSize", "AL Threshold 0", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission1CenterOutSize", "AL Threshold 1", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission2CenterOutSize", "AL Threshold 2", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission3CenterOutSize", "AL Threshold 3", 0f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

                RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission0CenterOutDuration", "AL Duration 0", 0.4f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                if (audioLinkEnabled)
                {
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission1CenterOutDuration", "AL Duration 1", 0.4f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission2CenterOutDuration", "AL Duration 2", 0.4f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                    RenderMaterialCheck(ChecksGrid, "_AudioLinkEmission3CenterOutDuration", "AL Duration 3", 0.4f, null, (propertyName, value) => SetPropertyValue(propertyName, value), null);
                }

            }
            else
            {
                EditorGUILayout.Space();
            }
        }

        public void RenderMaterialCheck<T>(VariableGridScope grid, string propertyName, string prefferedLabel, T prefferedValue, Func<T?, string>? valueMaper, Action<string, T> fix, Func<T?, T?, bool>? comparator)
        {
            T? value = GetPropertyValue<T>(propertyName);
            bool equal = comparator != null ? comparator(value, prefferedValue) : EqualityComparer<T?>.Default.Equals(value, prefferedValue);
            grid.Cell((_) => GUILayout.Label(prefferedLabel));
            grid.Cell((_) =>
            {
                string? mappedValue = valueMaper != null ? valueMaper(value) : value?.ToString();

                if (typeof(T) == typeof(Texture) && value != null) EditorGUILayout.ObjectField((UnityEngine.Object)(object)value, typeof(T), false, GUILayout.Width(48), GUILayout.Height(48));
                else if (typeof(T) == typeof(Color) && value != null) EditorGUILayout.ColorField(GUIContent.none, (Color)(object)value, false, true, true, GUILayout.Height(16), GUILayout.Width(100));
                else GUILayout.Label(mappedValue ?? "---");
            });
            grid.Cell((_) =>
            {
                string? mappedValue = valueMaper != null ? valueMaper(prefferedValue) : prefferedValue?.ToString();

                if (typeof(T) == typeof(Texture) && prefferedValue != null) EditorGUILayout.ObjectField(GUIContent.none, (UnityEngine.Object)(object)prefferedValue, typeof(T), false, GUILayout.Width(48), GUILayout.Height(48));
                else if (typeof(T) == typeof(Color) && prefferedValue != null) EditorGUILayout.ColorField(GUIContent.none, (Color)(object)prefferedValue, false, true, true, GUILayout.Height(16), GUILayout.Width(100));
                else GUILayout.Label(mappedValue ?? "---");
            });
            grid.Cell((_) => GUILayout.Label(equal ? "✔️" : "✖️", equal ? Config.ValidCenteredLabel : Config.InvalidCenteredLabel));
            grid.Cell((_) =>
            {
                if (equal) EditorGUILayout.Space();
                else if (GUILayout.Button("Fix")) fix.Invoke(propertyName, prefferedValue);

            });
        }

        public bool DefaultComparator<T>(T x, T y)
        {
            return EqualityComparer<T?>.Default.Equals(x, y);
        }

        public T? GetPropertyValue<T>(string propertyName)
        {
            if (!material.HasProperty(propertyName))
                return default;

            Type type = typeof(T);

            return type switch
            {
                Type t when t == typeof(float) => (T)(object)material.GetFloat(propertyName),
                Type t when t == typeof(int) => (T)(object)material.GetInt(propertyName),
                Type t when t == typeof(Color) => (T)(object)material.GetColor(propertyName),
                Type t when t == typeof(Vector4) => (T)(object)material.GetVector(propertyName),
                Type t when t == typeof(Matrix4x4) => (T)(object)material.GetMatrix(propertyName),
                Type t when t == typeof(Texture) => (T)(object)material.GetTexture(propertyName)!,
                Type t when t == typeof(Texture2D) => (T)(object)material.GetTexture(propertyName)!,
                _ => throw new ArgumentException($"Unsupported property type: {type}"),
            };
        }

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            if (!material.HasProperty(propertyName))
                return;

            Type type = typeof(T);

            switch (type)
            {
                case Type t when t == typeof(float):
                    material.SetFloat(propertyName, (float)(object)value!);
                    break;

                case Type t when t == typeof(int):
                    material.SetInt(propertyName, (int)(object)value!);
                    break;

                case Type t when t == typeof(Color):
                    material.SetColor(propertyName, (Color)(object)value!);
                    break;

                case Type t when t == typeof(Vector4):
                    material.SetVector(propertyName, (Vector4)(object)value!);
                    break;

                case Type t when t == typeof(Texture):
                    material.SetTexture(propertyName, (Texture)(object)value!);
                    break;

                case Type t when t == typeof(Matrix4x4):
                    material.SetMatrix(propertyName, (Matrix4x4)(object)value!);
                    break;

                default:
                    throw new ArgumentException($"Unsupported property type: {type}");
            }
        }
    }
}