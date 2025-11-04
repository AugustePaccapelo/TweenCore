#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Author : Auguste Paccapelo

[CustomPropertyDrawer(typeof(TweenPropertyBase), true)]
public class TweenPropertyBaseEditor : PropertyDrawer
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private UnityEngine.Object _currentObject;

    // ----- Others ----- \\

    private Dictionary<long, string[]> _propertiesNamesMap = new Dictionary<long, string[]>();

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        _currentObject = (UnityEngine.Object)property.FindPropertyRelative("_obj").boxedValue;

        SerializedProperty propLastObj = property.FindPropertyRelative("_lastKnownObject");
        UnityEngine.Object lastObj = (UnityEngine.Object)propLastObj.boxedValue;
        
        SerializedProperty propIndex = property.FindPropertyRelative("_propertyIndex");
        int currentIndex = propIndex.intValue;

        long currentId = property.managedReferenceId;

        if (_currentObject != null)
        {
            SerializedProperty propName = property.FindPropertyRelative("_propertyName");

            Rect popupRect = new Rect(position.x, position.y + EditorGUI.GetPropertyHeight(property, label, true) + 2,
                                  position.width, EditorGUIUtility.singleLineHeight);
            position.height += EditorGUIUtility.singleLineHeight;

            if (_currentObject != lastObj || !_propertiesNamesMap.ContainsKey(currentId))
            {
                propLastObj.boxedValue = _currentObject;

                BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;

                PropertyInfo[] allProperties = _currentObject.GetType().GetProperties(flag);
                Type genericType = property.managedReferenceValue.GetType().GetGenericArguments()[0];
                _propertiesNamesMap[currentId] = allProperties.Where(p => p.PropertyType == genericType).Select(p => p.Name).ToArray();
            }

            if (_propertiesNamesMap[currentId].Length > 0)
            {
                int newIndex = EditorGUI.Popup(popupRect, currentIndex, _propertiesNamesMap[currentId]);
                if (newIndex != currentIndex)
                {
                    propName.stringValue = _propertiesNamesMap[currentId][newIndex];
                    propIndex.intValue = newIndex;
                }
            }
        }
        else
        {
            propLastObj.boxedValue = null;
            propIndex.intValue = 0;
        }
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float heigth = EditorGUI.GetPropertyHeight(property);
        SerializedProperty prop = property.FindPropertyRelative("_propertyName");
        if (!string.IsNullOrEmpty(prop.stringValue)) heigth += EditorGUIUtility.singleLineHeight * 1.25f;
        return heigth;
    }
}
#endif