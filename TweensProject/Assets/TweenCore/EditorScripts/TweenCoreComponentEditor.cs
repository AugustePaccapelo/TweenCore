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

    private SerializedProperty _playOnStart;
    private SerializedProperty _isParallel;
    private SerializedProperty _isLoop;
    private SerializedProperty _isInfinite;
    private SerializedProperty _numIteration;
    private SerializedProperty _destroyWhenFinish;
    private SerializedProperty _surviveOnUnload;
    private SerializedProperty _unityEvents;

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
        SetPropertiesList();
        GetProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_playOnStart);
        EditorGUILayout.PropertyField(_isParallel);

        EditorGUILayout.PropertyField(_isLoop);

        if (_isLoop.boolValue)
        {
            EditorGUILayout.PropertyField(_isInfinite);

            if (!_isInfinite.boolValue)
            {
                EditorGUILayout.PropertyField(_numIteration);
            }
        }

        EditorGUILayout.PropertyField(_destroyWhenFinish);
        EditorGUILayout.PropertyField(_surviveOnUnload);

        _propertiesEditorList.DoLayoutList();

        EditorGUILayout.PropertyField(_unityEvents);

        serializedObject.ApplyModifiedProperties();
    }

    // ----- My Functions ----- \\

    private void GetProperties()
    {
        _playOnStart = serializedObject.FindProperty("_playOnStart");
        _isParallel = serializedObject.FindProperty("_isParallel");
        _isLoop = serializedObject.FindProperty("_isLoop");
        _isInfinite = serializedObject.FindProperty("_isInfinite");
        _numIteration = serializedObject.FindProperty("_numIteration");
        _destroyWhenFinish = serializedObject.FindProperty("_DestroyWhenFinished");
        _surviveOnUnload = serializedObject.FindProperty("_surviveOnUnload");
        _unityEvents = serializedObject.FindProperty("_unityEvents");
    }

    private void SetPropertiesList()
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