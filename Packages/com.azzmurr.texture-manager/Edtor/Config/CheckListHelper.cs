using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Azzmurr.Utils {
    internal class CheckListHelper {
        private readonly MaterialMeta _material;

        private readonly Dictionary<SupportedMaterialPropertyType,
                Func<MaterialCheckListItem, (bool needsUpdate, object actualValue, object desiredValue, VisualElement element)>> _propertyCheckers;

        public CheckListHelper(MaterialMeta material) {
            _material = material;
            _propertyCheckers = CreatePropertyCheckers();
        }

        public (bool needsUpdate, object actualValue, object desiredValue, VisualElement element) CheckProperty(MaterialCheckListItem item) {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (!_propertyCheckers.TryGetValue(item.type, out var checker))
                throw new ArgumentOutOfRangeException(nameof(item.type));

            return checker(item);
        }

        public void SetFieldValue(VisualElement field, object value) {
            switch (field) {
                case Toggle toggle when value is bool boolValue:
                    toggle.SetValueWithoutNotify(boolValue);
                    break;
                case Toggle toggle when value is float floatValue:
                    toggle.SetValueWithoutNotify(Mathf.Approximately(floatValue, 1f));
                    break;
                case IntegerField intField when value is int intValue:
                    intField.SetValueWithoutNotify(intValue);
                    break;
                case FloatField floatField when value is float floatValue:
                    floatField.SetValueWithoutNotify(floatValue);
                    break;
                case ColorField colorField when value is Color colorValue:
                    colorField.SetValueWithoutNotify(colorValue);
                    break;
                case Vector4Field vector4Field when value is Vector4 vectorValue:
                    vector4Field.SetValueWithoutNotify(vectorValue);
                    break;
                case ObjectField objectField when value is UnityEngine.Object objectValue:
                    objectField.SetValueWithoutNotify(objectValue);
                    break;
            }
        }


        private Dictionary<SupportedMaterialPropertyType, Func<MaterialCheckListItem, (bool, object, object, VisualElement)>>
            CreatePropertyCheckers() {
            return new Dictionary<SupportedMaterialPropertyType, Func<MaterialCheckListItem, (bool, object, object, VisualElement)>> {
                { SupportedMaterialPropertyType.Bool, CheckBoolProperty },
                { SupportedMaterialPropertyType.Int, CheckIntProperty },
                { SupportedMaterialPropertyType.Float, CheckFloatProperty },
                { SupportedMaterialPropertyType.Color, CheckColorProperty },
                { SupportedMaterialPropertyType.ColorHDR, CheckColorHDRProperty },
                { SupportedMaterialPropertyType.Vector4, CheckVector4Property },
                { SupportedMaterialPropertyType.Texture, CheckTextureProperty }
            };
        }

        private (bool, object, object, VisualElement) CheckBoolProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<float>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredBoolValue ? 1f : 0f,
                () => _material.GetPropertyValue<float>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && !Mathf.Approximately(actual, desired);

            return (needUpdate,  actual, desired, new Toggle());
        }

        private  (bool, object, object, VisualElement) CheckIntProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<int>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredIntValue,
                () => _material.GetPropertyValue<int>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && actual != desired;

            return (needUpdate, actual, desired, new IntegerField());
        }

        private  (bool, object, object, VisualElement) CheckFloatProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<float>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredFloatValue,
                () => _material.GetPropertyValue<float>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && !Mathf.Approximately(actual, desired);

            return (needUpdate,  actual, desired, new FloatField());
        }

        private  (bool, object, object, VisualElement) CheckColorProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<Color>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredColorValue,
                () => _material.GetPropertyValue<Color>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && actual != desired;

            return (needUpdate, actual, desired, new ColorField { hdr = false });
        }

        private  (bool, object, object, VisualElement) CheckColorHDRProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<Color>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredColorHDRValue,
                () => _material.GetPropertyValue<Color>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && actual != desired;

            return (needUpdate, actual, desired, new ColorField { hdr = true });
        }

        private  (bool, object, object, VisualElement) CheckVector4Property(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<Vector4>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredVector4Value,
                () => _material.GetPropertyValue<Vector4>(item.desiredPropertyNameToCopyValue));
            var needUpdate = item.propertyCheckType != PropertyCheckType.Exists && actual != desired;

            return (needUpdate, actual, desired, new Vector4Field());
        }

        private  (bool, object, object, VisualElement) CheckTextureProperty(MaterialCheckListItem item) {
            var actual = _material.GetPropertyValue<Texture>(item.propertyName);
            var desired = GetDesiredValue(item,
                () => item.desiredTextureValue,
                () => _material.GetPropertyValue<Texture>(item.desiredPropertyNameToCopyValue));

            var needUpdate = item.propertyCheckType == PropertyCheckType.Exists
                ? actual == null
                : actual != desired;

            return (needUpdate, actual, desired, new ObjectField { objectType = typeof(Texture) });
        }

        private T GetDesiredValue<T>(MaterialCheckListItem item, Func<T> equalsValue, Func<T> sameAsValue) {
            return item.propertyCheckType switch {
                PropertyCheckType.Equals => equalsValue(),
                PropertyCheckType.SameAs => sameAsValue(),
                PropertyCheckType.Exists => equalsValue(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
