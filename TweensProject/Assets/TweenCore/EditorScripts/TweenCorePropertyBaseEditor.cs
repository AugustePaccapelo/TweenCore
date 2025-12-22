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

[CustomPropertyDrawer(typeof(TweenCorePropertyBase), true)]
public class TweenCorePropertyBaseEditor : PropertyDrawer
{
    // ----- CLASS ----- \\
    private class TweenPropertyEditorContext
    {
        // ----- VARIABLES ----- \\

        // Tween related \\

        public SerializedProperty property;
        public SerializedProperty propCurrentObject;
        public UnityEngine.Object currentObject;
        public SerializedProperty propLastKnownObject;
        public UnityEngine.Object lastKnownObject;
        public SerializedProperty propCurrentPropertyChoosedIndex;
        public SerializedProperty propPropertyChoosedName;

        public int currentPropertyChoosedIndex;
        public long referenceId;

        // Unity Editor related \\

        public Rect position;
        public GUIContent label;

        // ----- FUNCTIONS ----- \\

        public TweenPropertyEditorContext(SerializedProperty property = null)
        {
            if (property == null) return;
            
            InitSerializedProperties(property);
            InitVariables();
        }

        public void InitSerializedProperties()
        {
            InitSerializedProperties(property);
        }

        public void InitSerializedProperties(SerializedProperty property)
        {
            if (property == null) return;

            this.property = property;
            propCurrentObject = property.FindPropertyRelative("obj");
            propLastKnownObject = property.FindPropertyRelative("_lastKnownObject");
            propCurrentPropertyChoosedIndex = property.FindPropertyRelative("propertyIndex");
            propPropertyChoosedName = property.FindPropertyRelative("propertyName");
        }

        public void InitVariables()
        {
            if (propCurrentObject != null) currentObject = propCurrentObject.objectReferenceValue;
            if (propLastKnownObject != null) lastKnownObject = propLastKnownObject.objectReferenceValue;
            if (propCurrentPropertyChoosedIndex != null) currentPropertyChoosedIndex = propCurrentPropertyChoosedIndex.intValue;
            if (property != null) referenceId = property.managedReferenceId;
        }

        public bool DidObjectChanged()
        {
            return currentObject != lastKnownObject;
        }
    }

    // ---------- VARIABLES ---------- \\

    private Dictionary<long, string[]> _propertiesNamesMap = new Dictionary<long, string[]>();

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Call each editor frame
        EditorGUI.BeginProperty(position, label, property);
        
        TweenPropertyEditorContext propContext = new TweenPropertyEditorContext(property);
        propContext.position = position;
        propContext.label = label;

        // If property is collapse, the user can't interact with it so no needs to compute
        if (property.isExpanded)
        {
            HandlePropertyIsExpand(propContext);
        }

        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float heigth = EditorGUI.GetPropertyHeight(property, label, true);

        if (!property.isExpanded) return heigth;

        SerializedProperty propCurrentObj = property.FindPropertyRelative("obj");

        long propId = property.managedReferenceId;

        if (propCurrentObj.objectReferenceValue == null || !_propertiesNamesMap.ContainsKey(propId) || _propertiesNamesMap[propId].Length <= 0)
        {
            heigth -= EditorGUIUtility.singleLineHeight;
        }

        return heigth;
    }

    // ----- My functions ----- \\

    private void HandlePropertyIsExpand(TweenPropertyEditorContext propContext)
    {
        // If an object to tween where given
        if (propContext.currentObject != null)
        {
            HandlePropertyHasAnObject(propContext);
        }
        else
        {
            if (propContext.lastKnownObject != null)
            {
                propContext.propLastKnownObject.boxedValue = null;
                propContext.lastKnownObject = null;
            }
            if (propContext.currentPropertyChoosedIndex != 0)
            {
                propContext.propCurrentPropertyChoosedIndex.intValue = 0;
                propContext.currentPropertyChoosedIndex = 0;
            }
            propContext.property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void HandlePropertyHasAnObject(TweenPropertyEditorContext propContext)
    {
        if (propContext.DidObjectChanged())
        {
            HandleNewObject(propContext);
        }

        if (!_propertiesNamesMap.ContainsKey(propContext.referenceId))
        {
            HandleMissingPropertiesMap(propContext);
        }

        if (_propertiesNamesMap[propContext.referenceId].Length > 0)
        {
            DrawPopupChooseProperty(propContext);
        }
    }

    private void HandleNewObject(TweenPropertyEditorContext propContext)
    {
        // Update last known object
        propContext.propLastKnownObject.objectReferenceValue = propContext.currentObject;

        NewPropertiesNames(propContext);

        // If at least one property found, by default set to the first
        if (_propertiesNamesMap[propContext.referenceId].Length > 0)
        {
            propContext.propPropertyChoosedName.stringValue = _propertiesNamesMap[propContext.referenceId][0];
            propContext.propCurrentPropertyChoosedIndex.intValue = 0;
            propContext.currentPropertyChoosedIndex = 0;
            propContext.property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void HandleMissingPropertiesMap(TweenPropertyEditorContext propContext)
    {
        NewPropertiesNames(propContext);
    }

    private void NewPropertiesNames(TweenPropertyEditorContext propContext)
    {
        // Search all properties of Instance and that are Public
        BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;

        // Get all properties availbles that correspond in the object given to tween
        PropertyInfo[] allProperties = propContext.currentObject.GetType().GetProperties(flag);
        // Get the exact value of the TweenProperty (float, Vector, ...)
        Type genericType = propContext.property.managedReferenceValue.GetType().GetGenericArguments()[0];
        // Get all properties name avaible
        _propertiesNamesMap[propContext.referenceId] = allProperties.Where(p => p.PropertyType == genericType).Select(p => p.Name).ToArray();
    }

    private void DrawPopupChooseProperty(TweenPropertyEditorContext propContext)
    {
        // Set the position of the button to open the list of properties
        float propertyHeight = EditorGUI.GetPropertyHeight(propContext.property, propContext.label, true);

        Rect popupRectPropertyChoosed = new Rect(propContext.position.x, propContext.position.y + propertyHeight + EditorGUIUtility.standardVerticalSpacing,
                              propContext.position.width, EditorGUIUtility.singleLineHeight);
        
        // Draw button and get index of the chosen property by user
        int choosedIndex = EditorGUI.Popup(popupRectPropertyChoosed, propContext.currentPropertyChoosedIndex, _propertiesNamesMap[propContext.referenceId]);
        // If new property choosed
        if (choosedIndex != propContext.currentPropertyChoosedIndex)
        {
            propContext.propPropertyChoosedName.stringValue = _propertiesNamesMap[propContext.referenceId][choosedIndex];
            propContext.propCurrentPropertyChoosedIndex.intValue = choosedIndex;
            propContext.currentPropertyChoosedIndex = choosedIndex;

            propContext.property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(propContext.property.serializedObject.targetObject);
        }
    }
}
#endif