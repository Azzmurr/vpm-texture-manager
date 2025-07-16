using System;
using System.Numerics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector4 = UnityEngine.Vector4;

namespace Azzmurr.Utils {
    [CreateAssetMenu(
        fileName = "MaterialCheckListBehaviour",
        menuName = "Azzmurr/MaterialCheckListBehaviour",
        order = 1
    )]
    public class MaterialCheckListBehaviourList : ScriptableObject {
        public MaterialCheckListBehaviour[] behaviours;
    }

    [Serializable]
    public class MaterialCheckListBehaviour {
        public string shaderName;
        public MaterialCheckListItem[] items;
    }

    [Serializable]
    public class MaterialCheckListItem {
        public string propertyName;

        public SupportedMaterialPropertyType type;
        public PropertyCheckType propertyCheckType;

        public bool desiredBoolValue;
        public int desiredIntValue;
        public float desiredFloatValue;
        public Color desiredColorValue;
        public Color desiredColorHDRValue;
        public Vector4 desiredVector4Value;
        public Texture desiredTextureValue;

        public string desiredPropertyNameToCopyValue;

        public ShowPropertyIf showPropertyIf;
        public SupportedMaterialPropertyType showPropertyIfPropertyType;
        public string showPropertyIfPropertyName;
        public bool showPropertyIfPropertyBoolValue;
        public int showPropertyIfPropertyIntValue;
        public float showPropertyIfPropertyFloatValue;
        public Color showPropertyIfPropertyColorValue;
        public Color showPropertyIfPropertyColorHDRValue;
        public Vector4 showPropertyIfPropertyVector4Value;
        public Texture showPropertyIfPropertyTextureValue;

        public CopyPropertyOnFix copyPropertyOnFix;
        public string copyPropertyValueToPropertyName;
    }

    public enum SupportedMaterialPropertyType {
        Bool,
        Int,
        Float,
        Color,
        ColorHDR,
        Vector4,
        Texture,
    }

    public enum PropertyCheckType {
        Equals,
        SameAs,
        Exists,
    }

    public enum ShowPropertyIf {
        ShowAlways,
        ShowIfEquals,
        ShowIfNotEquals,
    }

    public enum CopyPropertyOnFix {
        DoNotCopy,
        Copy,
    }

    [CustomEditor(typeof(MaterialCheckListBehaviourList))]
    public class Editor : UnityEditor.Editor {
        private MaterialCheckListBehaviourList _script;
        private SerializedProperty _behaviours;

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            var listView = new ListView {
                focusable = true,
                name = "Behaviours List",
                showAddRemoveFooter = true,
                reorderable = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                headerTitle = "Check Lists",
                showFoldoutHeader = true,
                reorderMode = ListViewReorderMode.Animated,
                showBoundCollectionSize = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = {
                    marginTop = 8,
                },
            };

            listView.makeItem += () => new MaterialCheckListField();
            listView.bindItem += BindParameterListItem;

            listView.itemsAdded += _ => { serializedObject.Update(); };
            listView.itemsRemoved += _ => { serializedObject.Update(); };

            listView.BindProperty(_behaviours);

            root.Add(listView);

            return root;
        }

        private void BindParameterListItem(VisualElement element, int i) {
            var parameterField = (MaterialCheckListField)element;
            var param = _behaviours.GetArrayElementAtIndex(i);
            parameterField.BindProperty(param);
        }

        private void OnEnable() {
            _script = target as MaterialCheckListBehaviourList;
            if (_script == null) return;

            _behaviours = serializedObject.FindProperty(nameof(MaterialCheckListBehaviourList.behaviours));
        }
    }
}
