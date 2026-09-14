#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using TweenCore.Runtime;

// Author : Auguste Paccapelo

namespace TweenCore.Editor
{
    [CustomEditor(typeof(TweenCoreComponent))]
    public class TweenCoreComponentEditor : UnityEditor.Editor
    {
        // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

        private SerializedProperty _name;
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
        {"double", typeof(double)},
        {"int", typeof(int)},
        {"uint", typeof(uint)},
        {"long", typeof(long)},
        {"ulong", typeof(ulong)},
        {"decimal", typeof(decimal)},
        {"Vector2", typeof(Vector2)},
        {"Vector3", typeof(Vector3)},
        {"Vector4", typeof(Vector4)},
        {"Quaternion", typeof(Quaternion)},
        {"Color", typeof(Color)},
        {"Color32", typeof(Color32)},
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

        EditorGUILayout.PropertyField(_name);
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
        _name = FindProperty(TweenCoreComponent.NAME_PROPERTY);
        _playOnStart = FindProperty(TweenCoreComponent.PLAY_ON_START_PROPERTY);
        _isParallel = FindProperty(TweenCoreComponent.IS_PARALLEL_PROPERTY);
        _isLoop = FindProperty(TweenCoreComponent.IS_LOOP_PROPERTY);
        _isInfinite = FindProperty(TweenCoreComponent.IS_INFINITE_PROPERTY);
        _numIteration = FindProperty(TweenCoreComponent.NUM_ITERATION_PROPERTY);
        _destroyWhenFinish = FindProperty(TweenCoreComponent.DESTROY_WHEN_FINISHED_PROPERTY);
        _surviveOnUnload = FindProperty(TweenCoreComponent.SURVIVE_ON_UNLOAD_PROPERTY);
        _unityEvents = FindProperty(TweenCoreComponent.UNITY_EVENTS_PROPERTY);
    }

    private SerializedProperty FindProperty(string propertyName)
    {
        return serializedObject.FindProperty(propertyName);
    }

    private void SetPropertiesList()
    {
        SerializedProperty property = FindProperty(TweenCoreComponent.PROPERTIES_PROPERTY);

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
                Type genericType = typeof(TweenProperty<>).MakeGenericType(supportedType);
                TweenPropertyBase propertyBase = (TweenPropertyBase)Activator.CreateInstance(genericType);

                Undo.RecordObject(comp, "Add Tween Property");

                SerializedProperty properties = FindProperty(TweenCoreComponent.PROPERTIES_PROPERTY);
                int index = properties.arraySize;
                properties.InsertArrayElementAtIndex(index);
                properties.GetArrayElementAtIndex(index).managedReferenceValue = propertyBase;
                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(comp);
            });
        }

        menu.ShowAsContext();
    }
    }
}
#endif
