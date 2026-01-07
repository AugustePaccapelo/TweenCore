#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

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

        public SerializedProperty propTweenType;
        public SerializedProperty propTweenEase;
        public SerializedProperty propDuration;
        public SerializedProperty propDelay;
        public SerializedProperty propTypeAnimCurve;
        public SerializedProperty propEaseAnimCurve;
        public SerializedProperty propFromCurrentValue;
        public SerializedProperty propStartValue;
        public SerializedProperty propEndValue;
        public SerializedProperty propUnityEvents;

        public int currentPropertyChoosedIndex;
        public long referenceId;

        // Unity Editor related \\

        public Rect Position
        {
            get => _position;

            set
            {
                _position = value;
                _propertyPos = _position;
                _propertyPos.height = EditorGUIUtility.singleLineHeight;
            }
        }

        private Rect _position;

        public Rect PropertyPos => _propertyPos;

        private Rect _propertyPos;
        public GUIContent label;

        // ----- FUNCTIONS ----- \\

        public void DrawProperty(SerializedProperty property)
        {
            NewLine();
            EditorGUI.PropertyField(_propertyPos, property, true);
        }

        public void NewLine()
        {
            _propertyPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

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

            propTweenType = property.FindPropertyRelative("type");
            propTweenEase = property.FindPropertyRelative("ease");
            propDuration = property.FindPropertyRelative("duration");
            propDelay = property.FindPropertyRelative("delay");
            propTypeAnimCurve = property.FindPropertyRelative("typeAnimationCurve");
            propEaseAnimCurve = property.FindPropertyRelative("easeAnimationCurve");
            propFromCurrentValue = property.FindPropertyRelative("fromCurrentValue");
            propStartValue = property.FindPropertyRelative("_startValue");
            propEndValue = property.FindPropertyRelative("_finalValue");
            propUnityEvents = property.FindPropertyRelative("_unityEvents");
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

    private float Line => EditorGUIUtility.singleLineHeight;

    private Dictionary<long, string[]> _propertiesNamesMap = new Dictionary<long, string[]>();

    private static readonly Dictionary<TweenCoreType, string> _possibleTypes = new Dictionary<TweenCoreType, string>
    {
        { TweenCoreType.Linear, "Linear" },
        { TweenCoreType.Back, "Back"},
        { TweenCoreType.Bounce, "Bounce"},
        { TweenCoreType.Circ, "Circ"},
        { TweenCoreType.Cubic, "Cubic"},
        { TweenCoreType.Elastic, "Elastic"},
        { TweenCoreType.Expo, "Expo"},
        { TweenCoreType.Quad, "Quad"},
        { TweenCoreType.Quart, "Quart"},
        { TweenCoreType.Quint, "Quint"},
        { TweenCoreType.Sine, "Sine"},
        { TweenCoreType.CustomCurve, "CustomCurve"},
    };

    private static readonly Dictionary<TweenCoreEase, string> _possibleEases = new Dictionary<TweenCoreEase, string>
    {
        { TweenCoreEase.In, "In"},
        { TweenCoreEase.Out, "Out"},
        { TweenCoreEase.InOut, "InOut"},
        { TweenCoreEase.OutIn, "OutIn"},
        { TweenCoreEase.CustomCurve, "CustomCurve" }
    };

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Call each editor frame
        EditorGUI.BeginProperty(position, label, property);

        // Show name of TweenProperty & ability to collapse it
        Rect foldoutRect = position;
        foldoutRect.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        // If property is collapse, the user can't interact with it so no needs to compute
        if (property.isExpanded)
        {
            TweenPropertyEditorContext propContext = new TweenPropertyEditorContext(property);
            propContext.Position = position;
            propContext.label = label;

            EditorGUI.indentLevel++;
            HandlePropertyIsExpand(propContext);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        TweenPropertyEditorContext propContext = new TweenPropertyEditorContext();
        propContext.property = property;
        propContext.InitSerializedProperties();

        float height = Line;
        
        if (!property.isExpanded) return height;

        //Obj
        height += Line;

        // If has an obj, field for methods to tween
        if (propContext.propCurrentObject.boxedValue != null)
        {
            height += Line;
        }

        //Type
        height += Line;

        // If type is CustomCurve, show curve
        if ((TweenCoreType)propContext.propTweenType.boxedValue == TweenCoreType.CustomCurve)
        {
            height += Line;
        }

        //Ease
        height += Line;

        // If ease is CustomCurve, show curve
        if ((TweenCoreEase)propContext.propTweenEase.boxedValue == TweenCoreEase.CustomCurve)
        {
            height += Line;
        }

        // From current value
        height += Line;

        // If not from current, show startValue
        if (!propContext.propFromCurrentValue.boolValue)
        {
            height += Line;
        }

        // EndValue
        height += Line;

        // Duration
        height += Line;

        // Delay
        height += Line;

        // Unity events
        height += EditorGUI.GetPropertyHeight(propContext.propUnityEvents, true);

        return height;
    }

    // ----- My functions ----- \\

    private void DrawEasePopup(TweenPropertyEditorContext propContext)
    {
        TweenCoreEase currentEase = (TweenCoreEase)propContext.propTweenEase.enumValueIndex;

        int currentIndex = Array.IndexOf(_possibleEases.Keys.ToArray(), currentEase);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        propContext.NewLine();
        int newIndex = EditorGUI.Popup(propContext.PropertyPos, "Ease", currentIndex, _possibleEases.Values.ToArray());
        propContext.propTweenEase.enumValueIndex = (int)_possibleEases.Keys.ToArray()[newIndex];
    }

    private void DrawTypePopup(TweenPropertyEditorContext propContext)
    {
        TweenCoreType currentType = (TweenCoreType)propContext.propTweenType.enumValueIndex;

        int currentIndex = Array.IndexOf(_possibleTypes.Keys.ToArray(), currentType);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        propContext.NewLine();
        int newIndex = EditorGUI.Popup(propContext.PropertyPos, "Ease", currentIndex, _possibleTypes.Values.ToArray());
        propContext.propTweenType.enumValueIndex = (int)_possibleTypes.Keys.ToArray()[newIndex];
    }

    private void HandlePropertyIsExpand(TweenPropertyEditorContext propContext)
    {
        propContext.DrawProperty(propContext.propCurrentObject);

        // If an object to tween where given
        if (propContext.currentObject != null)
        {
            propContext.NewLine();
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

        DrawTypePopup(propContext);

        if ((TweenCoreType)propContext.propTweenType.boxedValue == TweenCoreType.CustomCurve)
        {
            propContext.DrawProperty(propContext.propTypeAnimCurve);
        }

        DrawEasePopup(propContext);

        if ((TweenCoreEase)propContext.propTweenEase.boxedValue == TweenCoreEase.CustomCurve)
        {
            propContext.DrawProperty(propContext.propEaseAnimCurve);
        }

        propContext.DrawProperty(propContext.propFromCurrentValue);
        if (!propContext.propFromCurrentValue.boolValue)
        {
            propContext.DrawProperty(propContext.propStartValue);
        }
        propContext.DrawProperty(propContext.propEndValue);

        propContext.DrawProperty(propContext.propDuration);

        propContext.DrawProperty(propContext.propDelay);

        propContext.DrawProperty(propContext.propUnityEvents);
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

        // Draw button and get index of the chosen property by user
        int choosedIndex = EditorGUI.Popup(propContext.PropertyPos, propContext.currentPropertyChoosedIndex, _propertiesNamesMap[propContext.referenceId]);
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