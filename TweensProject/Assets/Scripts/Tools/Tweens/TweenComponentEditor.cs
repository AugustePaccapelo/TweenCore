#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Author : Auguste Paccapelo

[CustomEditor(typeof(TweenComponent))]
public class TweenComponentEditor : Editor
{
    // ---------- VARIABLES ---------- \\

    // ----- Others ----- \\

    private const string BUTTON_NAME = "Add a new Tween Property";

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

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button(BUTTON_NAME))
        {
            ButtonNewPropertyPressed();
        }
    }

    // ----- My Functions ----- \\

    private void ButtonNewPropertyPressed()
    {
        GenericMenu menu = new GenericMenu();

        TweenComponent comp = (TweenComponent)target;

        foreach (string typeName in _typesMap.Keys)
        {
            Type supportedType = _typesMap[typeName];

            menu.AddItem(new GUIContent(typeName), false, () =>
            {
                Type genericType = typeof(TweenProperty<>).MakeGenericType(supportedType);
                TweenPropertyBase propertyBase = (TweenPropertyBase)Activator.CreateInstance(genericType);

                comp.AddProperty(propertyBase);
                EditorUtility.SetDirty(comp);
            });
        }

        menu.ShowAsContext();
    }
}
#endif