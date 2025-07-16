using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Azzmurr.Utils {
    public class MaterialCheckListItemField : VisualElement {
        private SerializedProperty _checklistItem; // MaterialCheckListItem

        private SerializedProperty _propertyName;
        private SerializedProperty _type;
        private SerializedProperty _propertyCheckType;

        private SerializedProperty _desiredBoolValue;
        private SerializedProperty _desiredIntValue;
        private SerializedProperty _desiredFloatValue;
        private SerializedProperty _desiredColorValue;
        private SerializedProperty _desiredColorHDRValue;
        private SerializedProperty _desiredVector4Value;
        private SerializedProperty _desiredTextureValue;
        private SerializedProperty _desiredPropertyNameToCopyValue;

        private SerializedProperty _showPropertyIf;
        private SerializedProperty _showPropertyIfPropertyType;
        private SerializedProperty _showPropertyIfPropertyName;
        private SerializedProperty _showPropertyIfPropertyBoolValue;
        private SerializedProperty _showPropertyIfPropertyIntValue;
        private SerializedProperty _showPropertyIfPropertyFloatValue;
        private SerializedProperty _showPropertyIfPropertyColorValue;
        private SerializedProperty _showPropertyIfPropertyColorHDRValue;
        private SerializedProperty _showPropertyIfPropertyVector4Value;
        private SerializedProperty _showPropertyIfPropertyTextureValue;

        private SerializedProperty _copyPropertyOnFix;
        private SerializedProperty _copyPropertyValueToPropertyName;

        private readonly TextField _propertyNameField = new() { style = { flexGrow = 1, flexShrink = 0, minWidth = 150 } };
        private readonly EnumField _typeField = new() { style = { width = 100 } };
        private readonly EnumField _propertyCheckTypeField = new() { style = { width = 100 } };
        private readonly Toggle _desiredBoolValueField = new() { style = {  width = 200 } };
        private readonly IntegerField _desiredIntValueField = new() { style = {  width = 200 } };
        private readonly FloatField _desiredFloatValueField = new() { style = {  width = 200 } };
        private readonly ColorField _desiredColorValueField = new() { style = {  width = 200 } };
        private readonly ColorField _desiredColorHDRValueField = new() { hdr = true, style = {  width = 200 } };
        private readonly Vector4Field _desiredVector4ValueField = new() { style = {  width = 200 } };
        private readonly ObjectField _desiredTextureValueField = new() { style = {  width = 200 } };
        private readonly TextField _desiredPropertyNameToCopyValueField = new() { style = {  width = 200 } };

        private readonly EnumField _showPropertyIfField = new() { style = { width = 100 } };
        private readonly TextField _showPropertyIfPropertyNameField = new() { label = "Show if", style = { flexGrow = 1, flexShrink = 0, minWidth = 150 } };
        private readonly EnumField _showPropertyIfPropertyTypeField = new() { style = { width = 200 } };
        private readonly Toggle _showPropertyIfPropertyBoolValueField = new() { style = {  width = 403 } };
        private readonly IntegerField _showPropertyIfPropertyIntValueField = new() { style = {  width = 403 } };
        private readonly FloatField _showPropertyIfPropertyFloatValueField = new() { style = {  width = 403 } };
        private readonly ColorField _showPropertyIfPropertyColorValueField = new() { style = {  width = 403 } };
        private readonly ColorField _showPropertyIfPropertyColorHDRValueField = new() { hdr = true, style = {  width = 403 } };
        private readonly Vector4Field _showPropertyIfPropertyVector4ValueField = new() { style = {  width = 403 } };
        private readonly ObjectField _showPropertyIfPropertyTextureValueField = new() { style = {  width = 403 } };

        private readonly EnumField _copyPropertyOnFixField = new() { style = { width = 100 } };
        private readonly TextField _copyPropertyValueToPropertyNameField = new() { label = "Copy to", style = { flexGrow = 1, flexShrink = 0, minWidth = 150 } };

        private readonly VisualElement _spacer = new() { style = { width = 201 } };
        private readonly VisualElement _spacer2 = new() { style = { width = 605 } };

        private readonly List<VisualElement> _elements;
        private readonly List<VisualElement> _elements2;

        public MaterialCheckListItemField() {
            style.paddingTop = 8;
            style.paddingBottom = 8;

            var main = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            main.Add(_propertyNameField);
            main.Add(_typeField);
            main.Add(_propertyCheckTypeField);
            main.Add(_desiredBoolValueField);
            main.Add(_desiredIntValueField);
            main.Add(_desiredFloatValueField);
            main.Add(_desiredColorValueField);
            main.Add(_desiredColorHDRValueField);
            main.Add(_desiredVector4ValueField);
            main.Add(_desiredTextureValueField);
            main.Add(_desiredPropertyNameToCopyValueField);
            main.Add(_spacer);
            main.Add(_showPropertyIfField);
            main.Add(_copyPropertyOnFixField);

            Add(main);

            var showIf = new VisualElement { style = { flexDirection = FlexDirection.Row } };


            showIf.Add(_showPropertyIfPropertyNameField);
            showIf.Add(_showPropertyIfPropertyTypeField);
            showIf.Add(_showPropertyIfPropertyBoolValueField);
            showIf.Add(_showPropertyIfPropertyIntValueField);
            showIf.Add(_showPropertyIfPropertyFloatValueField);
            showIf.Add(_showPropertyIfPropertyColorValueField);
            showIf.Add(_showPropertyIfPropertyColorHDRValueField);
            showIf.Add(_showPropertyIfPropertyVector4ValueField);
            showIf.Add(_showPropertyIfPropertyTextureValueField);


            Add(showIf);

            var copyTo = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            copyTo.Add(_copyPropertyValueToPropertyNameField);
            copyTo.Add(_spacer2);

            Add(copyTo);

            _elements = new List<VisualElement> {
                _desiredBoolValueField,
                _desiredIntValueField,
                _desiredFloatValueField,
                _desiredColorValueField,
                _desiredColorHDRValueField,
                _desiredVector4ValueField,
                _desiredTextureValueField,
            };

            _elements2 = new List<VisualElement> {
                _showPropertyIfPropertyBoolValueField,
                _showPropertyIfPropertyIntValueField,
                _showPropertyIfPropertyFloatValueField,
                _showPropertyIfPropertyColorValueField,
                _showPropertyIfPropertyColorHDRValueField,
                _showPropertyIfPropertyVector4ValueField,
                _showPropertyIfPropertyTextureValueField,
            };

            _typeField.RegisterValueChangedCallback((e) => {
                if (e.newValue == null) return;
                ChangeVisibility();
            });

            _propertyCheckTypeField.RegisterValueChangedCallback(e => {
                if (e.newValue == null) return;
                ChangeVisibility();
            });

            _showPropertyIfField.RegisterValueChangedCallback(e => {
                if (e.newValue == null) return;
                ChangeVisibility();
            });

            _showPropertyIfPropertyTypeField.RegisterValueChangedCallback(e => {
                if (e.newValue == null) return;
                ChangeVisibility();
            });

            _copyPropertyOnFixField.RegisterValueChangedCallback(e => {
                if (e.newValue == null) return;
                ChangeVisibility();
            });
        }

        public void BindProperty(SerializedProperty prop) {
            _checklistItem = prop;

            _propertyName = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.propertyName));

            _type = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.type));

            _propertyCheckType = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.propertyCheckType));

            _desiredBoolValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredBoolValue));

            _desiredIntValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredIntValue));

            _desiredFloatValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredFloatValue));

            _desiredColorValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredColorValue));

            _desiredColorHDRValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredColorHDRValue));

            _desiredVector4Value = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredVector4Value));

            _desiredTextureValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredTextureValue));

            _desiredPropertyNameToCopyValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.desiredPropertyNameToCopyValue));

            _showPropertyIf = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIf));

            _showPropertyIfPropertyType = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyType));

            _showPropertyIfPropertyName = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyName));

            _showPropertyIfPropertyBoolValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyBoolValue));

            _showPropertyIfPropertyIntValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyIntValue));

            _showPropertyIfPropertyFloatValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyFloatValue));

            _showPropertyIfPropertyColorValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyColorValue));

            _showPropertyIfPropertyColorHDRValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyColorHDRValue));

            _showPropertyIfPropertyVector4Value = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyVector4Value));

            _showPropertyIfPropertyTextureValue = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.showPropertyIfPropertyTextureValue));

            _copyPropertyOnFix = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.copyPropertyOnFix));

            _copyPropertyValueToPropertyName = _checklistItem
                .FindPropertyRelative(nameof(MaterialCheckListItem.copyPropertyValueToPropertyName));

            _propertyNameField.BindProperty(_propertyName);
            _typeField.BindProperty(_type);
            _propertyCheckTypeField.BindProperty(_propertyCheckType);

            _desiredBoolValueField.BindProperty(_desiredBoolValue);
            _desiredIntValueField.BindProperty(_desiredIntValue);
            _desiredFloatValueField.BindProperty(_desiredFloatValue);
            _desiredColorValueField.BindProperty(_desiredColorValue);
            _desiredColorHDRValueField.BindProperty(_desiredColorHDRValue);
            _desiredVector4ValueField.BindProperty(_desiredVector4Value);
            _desiredTextureValueField.BindProperty(_desiredTextureValue);
            _desiredPropertyNameToCopyValueField.BindProperty(_desiredPropertyNameToCopyValue);

            _showPropertyIfField.BindProperty(_showPropertyIf);
            _showPropertyIfPropertyTypeField.BindProperty(_showPropertyIfPropertyType);
            _showPropertyIfPropertyNameField.BindProperty(_showPropertyIfPropertyName);
            _showPropertyIfPropertyBoolValueField.BindProperty(_showPropertyIfPropertyBoolValue);
            _showPropertyIfPropertyIntValueField.BindProperty(_showPropertyIfPropertyIntValue);
            _showPropertyIfPropertyFloatValueField.BindProperty(_showPropertyIfPropertyFloatValue);
            _showPropertyIfPropertyColorValueField.BindProperty(_showPropertyIfPropertyColorValue);
            _showPropertyIfPropertyColorHDRValueField.BindProperty(_showPropertyIfPropertyColorHDRValue);
            _showPropertyIfPropertyVector4ValueField.BindProperty(_showPropertyIfPropertyVector4Value);
            _showPropertyIfPropertyTextureValueField.BindProperty(_showPropertyIfPropertyTextureValue);

            _copyPropertyOnFixField.BindProperty(_copyPropertyOnFix);
            _copyPropertyValueToPropertyNameField.BindProperty(_copyPropertyValueToPropertyName);

            ChangeVisibility();
        }

        private void ChangeVisibility() {
            var setMode = (PropertyCheckType)_propertyCheckTypeField.value;
            _desiredPropertyNameToCopyValueField.style.display = setMode == PropertyCheckType.SameAs ? DisplayStyle.Flex : DisplayStyle.None;
            _spacer.style.display = setMode == PropertyCheckType.Exists ? DisplayStyle.Flex : DisplayStyle.None;

            _elements.ForEach(element => {
                var index = _elements.IndexOf(element);
                var displayMode = index == _type.enumValueIndex && setMode == PropertyCheckType.Equals ? DisplayStyle.Flex : DisplayStyle.None;
                element.style.display = displayMode;

                if (displayMode == DisplayStyle.None) return;
                // SET OTHER VALUES TO NULL SOMEHOW
            });

            var showMode = (ShowPropertyIf)_showPropertyIfField.value;
            var display = showMode == ShowPropertyIf.ShowAlways ? DisplayStyle.None : DisplayStyle.Flex;
            _showPropertyIfPropertyNameField.style.display = display;
            _showPropertyIfPropertyTypeField.style.display = display;

            _elements2.ForEach(element => {
                var index = _elements2.IndexOf(element);
                var displayMode = index == _showPropertyIfPropertyType.enumValueIndex && showMode != ShowPropertyIf.ShowAlways ? DisplayStyle.Flex : DisplayStyle.None;
                element.style.display = displayMode;
            });

            var copyMode = (CopyPropertyOnFix)_copyPropertyOnFixField.value;
            _copyPropertyValueToPropertyNameField.style.display = copyMode == CopyPropertyOnFix.Copy ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
