using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using ObjectField = UnityEditor.Search.ObjectField;

namespace Azzmurr.Utils {
    internal class MaterialChecklist : EditorWindow {
        public MaterialMeta Material {
            get => _material;
            set {
                _material = value;
                MaterialSelector.value = _material?.Material;
            }
        }

        private ObjectField MaterialSelector => rootVisualElement.Q<ObjectField>("MaterialObject");
        private ObjectField BehaviourSelector => rootVisualElement.Q<ObjectField>("MaterialChecklist");
        private MultiColumnListView ChecklistView => rootVisualElement.Q<MultiColumnListView>("Checklist");
        private MaterialMeta _material;

        private MultiColumnListView CreateChecklistGUI() {
            var checklist = new MultiColumnListView {
                name = "Checklist",
                focusable = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                reorderMode = ListViewReorderMode.Animated,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = {
                    marginTop = 8,
                },
            };

            checklist.columns.Add(new Column {
                title = "Property Name",
                minWidth = 300,
                makeCell = () => new Label(),
                bindCell = (element, index) => {
                    var label = (Label)element;
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);
                    label.text = item.propertyName;
                }
            });

            checklist.columns.Add(new Column {
                title = "Status",
                maxWidth = 65,
                minWidth = 65,
                makeCell = () => new Label(),
                bindCell = (element, index) => {
                    var label = (Label)element;
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);
                    label.text = Material.Material.HasProperty(item.propertyName) ? "Found" : "Missing";
                    label.style.color = Material.Material.HasProperty(item.propertyName) ? Color.green : Color.red;
                }
            });

            checklist.columns.Add(new Column {
                title = "Current Value",
                minWidth = 300,
                makeCell = () => new VisualElement(),
                bindCell = (element, index) => {
                    element.Clear();
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);

                    if (!Material.Material.HasProperty(item.propertyName)) return;

                    var helper = new CheckListHelper(Material);
                    var (needsUpdate,actualValue, desiredValue, itemElement) = helper.CheckProperty(item);
                    helper.SetFieldValue(itemElement, actualValue);
                    element.Add(itemElement);
                }
            });

            checklist.columns.Add(new Column {
                title = "Desired Value",
                minWidth = 300,
                makeCell = () => new VisualElement(),
                bindCell = (element, index) => {
                    element.Clear();
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);

                    if (!Material.Material.HasProperty(item.propertyName)) return;

                    if (item.propertyCheckType == PropertyCheckType.Exists) {
                        element.Add(new Label("Exists"));
                        return;
                    }

                    var helper = new CheckListHelper(Material);
                    var (needsUpdate,actualValue, desiredValue, itemElement) = helper.CheckProperty(item);
                    helper.SetFieldValue(itemElement, desiredValue);

                    element.Add(itemElement);
                }
            });

            checklist.columns.Add(new Column {
                title = "Status",
                maxWidth = 65,
                minWidth = 65,
                makeCell = () => new Label { style = { justifyContent = Justify.Center, alignItems = Align.Center }},
                bindCell = (element, index) => {
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);

                    if (!Material.Material.HasProperty(item.propertyName))
                        return;

                    var helper = new CheckListHelper(Material);
                    var (needsUpdate, actualValue, desiredValue, itemElement) = helper.CheckProperty(item);

                    ((Label)element).text = needsUpdate ? "✖" : "✔";
                    ((Label)element).style.color = needsUpdate ? Color.red : Color.green;
                    ((Label)element).style.color = needsUpdate ? Color.red : Color.green;

                }
            });

            checklist.columns.Add(new Column {
                title = "Actions",
                minWidth = 100,
                makeCell = () => new VisualElement(),
                bindCell = (element, index) => {
                    element.Clear();
                    var item = (MaterialCheckListItem)checklist.viewController.GetItemForIndex(index);

                    if (!Material.Material.HasProperty(item.propertyName))
                        return;

                    if (item.propertyCheckType == PropertyCheckType.Exists) return;

                    var helper = new CheckListHelper(Material);
                    var (needsUpdate, actualValue, desiredValue, itemElement) = helper.CheckProperty(item);

                    if (needsUpdate) {
                        element.Add(new Button(() =>
                            DoAndRedraw(() => {
                                if (item.copyPropertyOnFix == CopyPropertyOnFix.Copy) {
                                    Material.SetPropertyValue(item.copyPropertyValueToPropertyName, actualValue);
                                }

                                Material.SetPropertyValue(item.propertyName, desiredValue);
                            }))
                        {
                            text = "FIX"
                        });
                    }

                }
            });

            return checklist;
        }

        private void CreateGUI() {
            var root = rootVisualElement;
            root.style.paddingRight = 8;
            root.style.paddingLeft = 8;

            var materialSelector = new VisualElement { style = { flexShrink = 0 } };
            var materialGameObjectField = new ObjectField {
                objectType = typeof(Material),
                value = Material?.Material,
                name = "MaterialObject",
                label = "Material: ",
                style = {
                    flexShrink = 0,
                    flexGrow = 1,
                }
            };

            var checklistSelector = new VisualElement { style = { flexShrink = 0 } };
            var checklistSelectorField = new ObjectField {
                objectType = typeof(MaterialCheckListBehaviourList),
                value = AssetDatabase.LoadAllAssetsAtPath("Packages/com.azzmurr.texture-manager/Edtor/Scripts/MaterialCheckList/Checklist.asset")[0],
                name = "MaterialChecklist",
                label = "Checklist: ",
                style = {
                    flexShrink = 0,
                    flexGrow = 1,
                }
            };

            var checklistView = CreateChecklistGUI();

            materialGameObjectField.RegisterValueChangedCallback((e) => {
                var material = (Material)e.newValue;
                if (material == null) {
                    _material = null;
                    return;
                }

                _material = new MaterialMeta(material);
                Refresh();
            });

            checklistSelectorField.RegisterValueChangedCallback((e) => { Refresh(); });

            checklistSelector.Add(checklistSelectorField);
            materialSelector.Add(materialGameObjectField);
            root.Add(materialSelector);
            root.Add(checklistSelector);

            root.Add(new Button(Refresh) { text = "Refresh" });

            root.Add(checklistView);
        }

        private void Refresh() {
            var material = (Material)MaterialSelector.value;
            var behavioursContainer = (MaterialCheckListBehaviourList)BehaviourSelector.value;

            if (material == null || behavioursContainer == null) {
                ChecklistView.itemsSource = null;
                return;
            }

            var behaviour = behavioursContainer.behaviours
                .ToList()
                .Find((behaviour) => Material.Shader.name.Contains(behaviour.shaderName));

            ChecklistView.itemsSource = behaviour?.items.Where((item) => {
                if (item.showPropertyIf == ShowPropertyIf.ShowAlways) return true;

                switch (item.showPropertyIfPropertyType) {
                    case SupportedMaterialPropertyType.Bool: {
                        var actualBoolPropertyValue = Material.GetPropertyValue<float>(item.showPropertyIfPropertyName);
                        var propertyBoolValue = item.showPropertyIfPropertyBoolValue ? 1f : 0f;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? Mathf.Approximately(actualBoolPropertyValue, propertyBoolValue)
                            : !Mathf.Approximately(actualBoolPropertyValue, propertyBoolValue);
                    }

                    case SupportedMaterialPropertyType.Int:
                        var actualIntPropertyValue = Material.GetPropertyValue<int>(item.showPropertyIfPropertyName);
                        var propertyIntValue = item.showPropertyIfPropertyIntValue;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? actualIntPropertyValue == propertyIntValue
                            : actualIntPropertyValue != propertyIntValue;

                    case SupportedMaterialPropertyType.Float:
                        var actualFloatPropertyValue =
                            Material.GetPropertyValue<float>(item.showPropertyIfPropertyName);
                        var propertyFloatValue = item.showPropertyIfPropertyFloatValue;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? Mathf.Approximately(actualFloatPropertyValue, propertyFloatValue)
                            : !Mathf.Approximately(actualFloatPropertyValue, propertyFloatValue);

                    case SupportedMaterialPropertyType.Color:
                    case SupportedMaterialPropertyType.ColorHDR:
                        var actualColorPropertyValue =
                            Material.GetPropertyValue<Color>(item.showPropertyIfPropertyName);
                        var propertyColorValue = item.showPropertyIfPropertyColorValue;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? actualColorPropertyValue == propertyColorValue
                            : actualColorPropertyValue != propertyColorValue;

                    case SupportedMaterialPropertyType.Vector4:
                        var actualVector4PropertyValue =
                            Material.GetPropertyValue<Vector4>(item.showPropertyIfPropertyName);
                        var propertyVector4Value = item.showPropertyIfPropertyVector4Value;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? actualVector4PropertyValue == propertyVector4Value
                            : actualVector4PropertyValue != propertyVector4Value;

                    case SupportedMaterialPropertyType.Texture:
                        var actualTexturePropertyValue =
                            Material.GetPropertyValue<Texture>(item.showPropertyIfPropertyName);
                        var propertyTextureValue = item.showPropertyIfPropertyTextureValue;

                        return item.showPropertyIf == ShowPropertyIf.ShowIfEquals
                            ? actualTexturePropertyValue == propertyTextureValue
                            : actualTexturePropertyValue != propertyTextureValue;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).ToList();
        }

        private void DoAndRedraw(Action action) {
            var list = rootVisualElement.Q<MultiColumnListView>("Checklist");
            action.Invoke();
            list.RefreshItems();
        }

        private void DoAndRedraw(MultiColumnListView view, Action action) {
            action.Invoke();
            view.RefreshItems();
        }

        private void DoAndRedraw(MultiColumnListView view, int index, Action action) {
            action.Invoke();
            view.RefreshItem(index);
        }

        [MenuItem("Tools/Azzmurr/Material Checklist")]
        public static void Init() {
            var window = (MaterialChecklist)GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Show();
        }

        [MenuItem("Assets/Azzmurr/Material Checklist", true, 0)]
        public static bool CanShowFromSelection() {
            return Selection.activeObject is Material;
        }

        [MenuItem("Assets/Azzmurr/Material Checklist", false, 0)]
        public static void ShowFromSelection() {
            var window = (MaterialChecklist)GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Material = new MaterialMeta((Material)Selection.activeObject);
            window.Show();
        }

        public static void Init(Material activeGameObject) {
            var window = (MaterialChecklist)GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Material = new MaterialMeta(activeGameObject);
            window.Show();
        }
    }
}
