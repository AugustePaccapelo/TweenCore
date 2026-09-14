using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Author : Auguste Paccapelo

[Serializable]
public class TweenCoreProperty<TweenValueType> : TweenCorePropertyBase
{
    // ---------- VARIABLES ---------- \\

    // ----- Others ----- \\

    [SerializeField] private TweenValueType _startValue;
    public TweenValueType StartValue => _startValue;

    [SerializeField] private TweenValueType _finalValue;
    public TweenValueType FinalValue => _finalValue;

    [SerializeField] private TweenValueType _increaseValue;
    public TweenValueType IncreasingValue => _increaseValue;

    private TweenValueType _currentValue;
    public TweenValueType CurrentValue => _currentValue;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<TweenValueType> _function;
    
    private List<TweenCorePropertyBase> _nextProperties = new List<TweenCorePropertyBase>();

    [Serializable]
    private class TweenPropertyUnityEvents
    {
        [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnStart;
        [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnUpdate;
        [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>, TweenValueType> unityOnUpdateValue;
        [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnFinish;
    }

    [SerializeField] private TweenPropertyUnityEvents _unityEvents = new TweenPropertyUnityEvents();

    public event Action<TweenCoreProperty<TweenValueType>, TweenValueType> OnUpdateValue;

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// Constructor of TweenProperty for when not modifying a property or field.
    /// </summary>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="time">The duration.</param>
    public TweenCoreProperty(TweenValueType startVal, TweenValueType finalVal, float time)
    {
        _currentMethod = MethodUse.ReturnValue;

        SetCommonValues(finalVal, time);

        _startValue = startVal;

        fromCurrentValue = false;
    }

    /// <summary>
    /// Constructor of TweenProperty when using a function to update a property or field.
    /// </summary>
    /// <param name="function">The function to call each Updates.</param>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    public TweenCoreProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration)
    {
        _currentMethod = MethodUse.Strategy;

        SetCommonValues(finalVal, duration);

        _startValue = startVal;
        _function = function;

        fromCurrentValue = false;
    }

    /// <summary>
    /// Constructor of TweenProperty using reflexion, with the current value as startValue.
    /// </summary>
    /// <param name="obj">The targeted object.</param>
    /// <param name="method">The targeted property or field.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration)
    {
        _currentMethod = MethodUse.Reflexion;
        
        SetCommonValues(finalVal, duration, method);

        base.obj = obj;
        SetReflexionFiels(propertyName);

        fromCurrentValue = true;
    }

    /// <summary>
    /// Constructor of TweenProperty using reflexion.
    /// </summary>
    /// <param name="obj">The targeted object.</param>
    /// <param name="method">The targeted property or field.</param>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration)
    {
        _currentMethod = MethodUse.Reflexion;

        SetCommonValues(finalVal, duration, method);

        base.obj = obj;
        SetReflexionFiels(propertyName);
        _startValue = startVal;

        fromCurrentValue = false;
    }

    /// <summary>
    /// An empty constructor just to create an object.
    /// Properties and fields are assign in the editor.
    /// </summary>
    public TweenCoreProperty()
    {
        _currentMethod = MethodUse.Reflexion;
    }

    public override TweenCorePropertyBase SetBaseValues()
    {
        if (type == TweenCoreType.CustomCurve)
        {
            SetType(typeAnimationCurve);
        }
        else
        {
            SetType(type);
        }
        
        if (ease == TweenCoreEase.CustomCurve)
        {
            SetEase(easeAnimationCurve);
        }
        else
        {
            SetEase(ease);
        }

        if (isEmpty)
        {
            _currentMethod = MethodUse.ReturnValue;
        }
        else
        {
            SetReflexionFiels(propertyName);
        }

        return this;
    }

    private void SetCommonValues(TweenValueType finalVal, float duration, string propertyName = "")
    {
        _finalValue = finalVal;
        base.duration = duration;
        base.propertyName = propertyName;
        SetType(type);
        SetEase(ease);
    }

    private void SetReflexionFiels(string method)
    {
        if (obj == null)
        {
            Debug.LogError("Given object to tween is null");
            return;
        }

        _property = obj.GetType().GetProperty(method);
        if (_property == null) _field = obj.GetType().GetField(method);

        if (_property == null && _field == null)
        {
            Debug.LogError("No property or field found : " + method);
            return;
        }
    }

    public override void Start()
    {
        if (hasStarted) return;

        hasStarted = true;
        isPaused = false;
        isPlaying = true;
        isFinish = false;

        if (_currentMethod == MethodUse.Reflexion)
        {
            if (obj == null)
            {
                TriggerOnStart();
                Stop(false);
                return;
            }

            if (fromCurrentValue)
            {
                _startValue = GetObjValue();
            }
        }

        TriggerOnStart();

        if (duration == 0f)
        {
            Stop(true);
        }
    }

    public override void Update(float deltaTime)
    {
        if (!isPlaying || isPaused) return;

        elapseTime += deltaTime;
        if (elapseTime <= delay) return;

        float elapse = Mathf.Clamp(elapseTime - delay, 0, duration);
        float w;

        if (duration != 0)
        {
             w = Mathf.Clamp01(elapse / duration);
        }
        else
        {
            w = 1;
        }

        w = RealWeight(w);

        if (lerpsFunc.ContainsKey(typeof(TweenValueType)))
        {

            TweenValueType endVal = fromCurrentValue && isIncreasingValue ?
                (TweenValueType)addFuncs[typeof(TweenValueType)](_startValue, _finalValue) :
                _finalValue;

            TweenValueType value = (TweenValueType)lerpsFunc[typeof(TweenValueType)](_startValue, endVal, w);
            SetValue(value);
        }
        else
        { 
            throw new ArgumentException("The ValueType given is not supported (" + typeof(TweenValueType) + ").");
        }

        TriggerOnUpdate();

        if (elapse >= duration) Stop();
    }

    private void StrategyMethod()
    {
        _function.Invoke(_currentValue);
    }

    private void ReflexionMethod()
    {
        if (_property != null) _property.SetValue(obj, _currentValue);
        else _field.SetValue(obj, _currentValue);
    }

    private TweenValueType GetObjValue()
    {
        object value = default;

        if (_property != null) value = _property.GetValue(obj);
        else if (_field != null) value = _field.GetValue(obj);

        return (TweenValueType)value;
    }

    /// <summary>
    /// Add a delay before the animation start.
    /// </summary>
    /// <param name="tweenDelay">Time to wait in seconds.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetDelay(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetDelay(float tweenDelay)
    {
        delay = tweenDelay;
        return this;
    }

    /// <summary>
    /// Set the tween type.
    /// </summary>
    /// <param name="newType">The wanted tween type.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType)
    {
        if (newType == TweenCoreType.Custom || newType == TweenCoreType.CustomCurve)
        {
            throw new Exception("Need to give the a function or an AnimCurve for a custom type.");
        }

        type = newType;
        SetTypeFunc(type);
        return this;
    }

    /// <summary>
    /// Set a custom tween Type, must be a function that return a float and have et float in parameters.
    /// </summary>
    /// <param name="newType">The wanted tween type.</param>
    /// <param name="customType">The function of the custome type, must return a float and take a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomType(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType, Func<float, float> customType)
    {
        if (newType != TweenCoreType.Custom)
        {
            SetType(newType);
            return this;
        }

        TypeFunc = customType;
        type = TweenCoreType.Custom;
        return this;
    }

    /// <summary>
    /// Set a custom tween Type, must be a function that return a float and have et float in parameters.
    /// </summary>
    /// <param name="customType">The function of the custome type, must return a float and take a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomType(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetType(Func<float, float> customType)
    {
        TypeFunc = customType;
        type = TweenCoreType.Custom;
        return this;
    }

    /// <summary>
    /// Set a custom type, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="newType">The wanted Tween Type.</param>
    /// <param name="animationCurve">The animation curve to use as type.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType, AnimationCurve animationCurve)
    {
        if (newType != TweenCoreType.CustomCurve)
        {
            SetType(newType);
            return this;
        }

        type = TweenCoreType.CustomCurve;
        typeAnimationCurve = animationCurve;
        return this;
    }

    /// <summary>
    /// Set a custom type, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="animationCurve">The animation curve to use as type.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetType(AnimationCurve animationCurve)
    {
        type = TweenCoreType.CustomCurve;
        typeAnimationCurve = animationCurve;
        return this;
    }

    /// <summary>
    /// Set the tween Ease.
    /// </summary>
    /// <param name="newEase">The wanted Tween Ease.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase)
    {
        if (newEase == TweenCoreEase.Custom || newEase == TweenCoreEase.CustomCurve)
        {
            throw new Exception("Need to give the a function or an AnimCurve for a custom ease.");
        }

        ease = newEase;
        SetEaseFunc(ease);
        return this;
    }

    /// <summary>
    /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="newEase">The wanted Tween Ease.</param>
    /// <param name="customEase">The function of the custom ease, 
    /// must return a float and take a Type function and a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase, Func<float, Func<float, float>, float> customEase)
    {
        if (newEase != TweenCoreEase.Custom)
        {
            SetEase(newEase);
            return this;
        }

        ease = TweenCoreEase.Custom;
        EaseFunc = customEase;
        return this;
    }

    /// <summary>
    /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="customEase">The function of the custom ease, 
    /// must return a float and take a Type function and a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(Func<float, Func<float, float>, float> customEase)
    {
        ease = TweenCoreEase.Custom;
        EaseFunc = customEase;
        return this;
    }

    /// <summary>
    /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="newEase">The wanted Tween Ease.</param>
    /// <param name="animationCurve">The animation curve to use as ease.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase, AnimationCurve animationCurve)
    {
        if (newEase != TweenCoreEase.CustomCurve)
        {
            SetEase(newEase);
            return this;
        }

        ease = TweenCoreEase.CustomCurve;
        easeAnimationCurve = animationCurve;
        return this;
    }

    /// <summary>
    /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="animationCurve">The animation curve to use as ease.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(AnimationCurve animationCurve)
    {
        ease = TweenCoreEase.CustomCurve;
        easeAnimationCurve = animationCurve;
        return this;
    }

    public TweenValueType GetCurrentValue() => CurrentValue;

    /// <summary>
    /// Set the start value of the property.
    /// </summary>
    /// <param name="value">The start value.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.From(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> From(TweenValueType value)
    {
        _startValue = value;
        fromCurrentValue = false;
        return this;
    }

    /// <summary>
    /// Use the start value of the property.
    /// !WARNING! Works only using a Reflexion method.
    /// </summary>
    /// <returns>This TweenProperty.</returns>
    public TweenCoreProperty<TweenValueType> FromCurrent()
    {
        fromCurrentValue = true;
        return this;
    }

    public TweenCoreProperty<TweenValueType> SetIsAdditive(bool isAdd)
    {
        isIncreasingValue = isAdd;
        if (isAdd)
        {
            fromCurrentValue = isAdd;
        }
        
        return this;
    }

    public override TweenCorePropertyBase AddNextProperty(TweenCorePropertyBase property)
    {
        _nextProperties.Add(property);
        return this;
    }

    private float RealWeight(float w)
    {
        // The ease is a curve
        if (ease == TweenCoreEase.CustomCurve)
        {
            if (type == TweenCoreType.CustomCurve)
            {
                //Type is a curve
                return easeAnimationCurve.Evaluate(typeAnimationCurve.Evaluate(w));
            }
            
            // Type is a func
            return easeAnimationCurve.Evaluate(TypeFunc(w));
        }
        
        // The ease is a func
        if (type == TweenCoreType.CustomCurve)
        {
            // Type is a curve
            return EaseFunc(w, typeAnimationCurve.Evaluate);
        }

        // Type is a func
        return EaseFunc(w, TypeFunc);
    }

    protected override void TriggerOnStart()
    {
        base.TriggerOnStart();
        _unityEvents.unityOnStart?.Invoke(this);
    }

    protected override void TriggerOnUpdate()
    {
        base.TriggerOnUpdate();
        OnUpdateValue?.Invoke(this, _currentValue);
        _unityEvents.unityOnUpdate?.Invoke(this);
        _unityEvents.unityOnUpdateValue?.Invoke(this, _currentValue);
    }

    protected override void TriggerOnFinish()
    {
        base.TriggerOnFinish();
        _unityEvents.unityOnFinish?.Invoke(this);
    }

    /// <summary>
    /// Pause the TweenProperty.
    /// </summary>
    public void Pause()
    {
        isPaused = true;
    }

    /// <summary>
    /// Resume the TweenProperty at the last state.
    /// </summary>
    public void Resume()
    {
        isPaused = false;
    }

    /// <summary>
    /// Stop and Destroy the TweenProperty.
    /// </summary>
    /// <param name="setToFinalValue"></param>
    public override void Stop(bool setToFinalValue = true)
    {
        if (setToFinalValue) SetToFinalVals();

        isPlaying = false;
        isPaused = true;
        isFinish = true;

        StartNextProperties();

        elapseTime = 0;
        hasStarted = false;

        TriggerOnFinish();
    }

    private void StartNextProperties()
    {
        int length = _nextProperties.Count;
        for (int i = 0; i < length; i++)
        {
            _nextProperties[i].Start();
        }        
    }

    private void SetValue(TweenValueType value)
    {
        _currentValue = value;

        switch (_currentMethod)
        {
            case MethodUse.Reflexion:
                ReflexionMethod();
                break;
            case MethodUse.Strategy:
                StrategyMethod();
                break;
            case MethodUse.ReturnValue:
                break;
            default:
                throw new NotImplementedException();
        }

        OnUpdateValue?.Invoke(this, _currentValue);
    }

    public override TweenCorePropertyBase SetToFinalVals()
    {
        TweenValueType endVal = fromCurrentValue && isIncreasingValue ?
                (TweenValueType)addFuncs[typeof(TweenValueType)](_startValue, _finalValue) :
                _finalValue;

        SetValue((TweenValueType)lerpsFunc[typeof(TweenValueType)](_startValue, endVal, RealWeight(1)));

        return this;
    }
}