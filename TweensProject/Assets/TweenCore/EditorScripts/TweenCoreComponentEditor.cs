#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// Author : Auguste Paccapelo

[CustomEditor(typeof(TweenCoreComponent))]
public class TweenCoreComponentEditor : Editor
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private ReorderableList _propertiesEditorList;

    // ----- Others ----- \\

    private static readonly Dictionary<string, Type> _typesMap = new Dictionary<string, Type>()
    {
        {"float", typeof(float)},
        {"Vector2", typeof(Vector2)},
        {"Vector3", typeof(Vector3)},
        {"Vector4", typeof(Vector4)},
        {"Color", typeof(Color)},
    };

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable()
    {
        SerializedProperty property = serializedObject.FindProperty("_properties");

        _propertiesEditorList = new ReorderableList(serializedObject, property, true, true, true, true);

        _propertiesEditorList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Tween Properties");
        };

        _propertiesEditorList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            float height = EditorGUI.GetPropertyHeight(element, true);

            if (element.isExpanded) height += EditorGUIUtility.singleLineHeight * 1.25f;

            return height;
        };

        _propertiesEditorList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            string elementName = "Property " + index;
            rect.x += 10f;
            rect.width -= 10 + 10;
            EditorGUI.PropertyField(rect, element, new GUIContent(elementName), true);
        };

        _propertiesEditorList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
        {
            ButtonNewPropertyPressed();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();

        while (property.NextVisible(true))
        {
            if (property.depth != 0) continue;
            if (property.name == "_properties") _propertiesEditorList.DoLayoutList();
            else EditorGUILayout.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ----- My Functions ----- \\

    private void ButtonNewPropertyPressed()
    {
        GenericMenu menu = new GenericMenu();

        TweenCoreComponent comp = (TweenCoreComponent)target;

        foreach (string typeName in _typesMap.Keys)
        {
            Type supportedType = _typesMap[typeName];

            menu.AddItem(new GUIContent(typeName), false, () =>
            {
                Type genericType = typeof(TweenCoreProperty<>).MakeGenericType(supportedType);
                TweenCorePropertyBase propertyBase = (TweenCorePropertyBase)Activator.CreateInstance(genericType);

                comp.AddProperty(propertyBase);
                EditorUtility.SetDirty(comp);
            });
        }

        menu.ShowAsContext();
    }
}
#endif