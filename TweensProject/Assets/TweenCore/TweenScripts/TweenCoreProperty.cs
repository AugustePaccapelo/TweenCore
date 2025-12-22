using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author : Auguste Paccapelo

[Serializable]
public class TweenCoreProperty<TweenValueType> : TweenCorePropertyBase
{
    // ---------- VARIABLES ---------- \\

    // ----- Others ----- \\

    [SerializeField] private bool _fromCurrentValue = false;
    public bool FromCurrentValue => _fromCurrentValue;

    [SerializeField] private TweenValueType _startValue;
    public TweenValueType StartValue => _startValue;

    [SerializeField] private TweenValueType _finalValue;
    public TweenValueType FinalValue => _finalValue;

    private TweenValueType _currentValue;
    public TweenValueType CurrentValue => _currentValue;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<TweenValueType> _function;

    private List<TweenCorePropertyBase> _nextProperties = new List<TweenCorePropertyBase>();

    [Serializable]
    private class TweenUnityEvents
    {
        public UnityEvent unityOnStart;
        public UnityEvent unityOnUpdate;
        public UnityEvent<TweenValueType> unityOnUpdateValue;
        public UnityEvent unityOnFinish;
        public UnityEvent unityOnLoopFinish;
    }

    [SerializeField] private TweenUnityEvents _unityEvents = new TweenUnityEvents();

    public event Action<TweenValueType> OnUpdateValue;

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// Constructor of TweenProperty for when not modifying a property or field.
    /// </summary>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="time">The duration.</param>
    /// <param name="tween">The attached tween.</param>
    public TweenCoreProperty(TweenValueType startVal, TweenValueType finalVal, float time, TweenCore tween)
    {
        _currentMethod = MethodUse.ReturnValue;

        SetCommonValues(finalVal, time, tween);

        _startValue = startVal;

        _fromCurrentValue = false;
    }

    /// <summary>
    /// Constructor of TweenProperty when using a function to update a property or field.
    /// </summary>
    /// <param name="function">The function to call each Updates.</param>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="tween">The attached tween.</param>
    public TweenCoreProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration, TweenCore tween)
    {
        _currentMethod = MethodUse.Strategy;

        SetCommonValues(finalVal, duration, tween);

        _startValue = startVal;
        _function = function;

        _fromCurrentValue = false;
    }

    /// <summary>
    /// Constructor of TweenProperty using reflexion, with the current value as startValue.
    /// </summary>
    /// <param name="obj">The targeted object.</param>
    /// <param name="method">The targeted property or field.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="tween">The attached tween.</param>
    public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration, TweenCore tween)
    {
        _currentMethod = MethodUse.Reflexion;
        
        SetCommonValues(finalVal, duration, tween, method);

        _obj = obj;
        SetReflexionFiels(propertyName);

        _fromCurrentValue = false;
    }

    /// <summary>
    /// Constructor of TweenProperty using reflexion.
    /// </summary>
    /// <param name="obj">The targeted object.</param>
    /// <param name="method">The targeted property or field.</param>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="tween">The attached tween.</param>
    public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration, TweenCore tween)
    {
        _currentMethod = MethodUse.Reflexion;

        SetCommonValues(finalVal, duration, tween, method);

        _obj = obj;
        SetReflexionFiels(propertyName);
        _startValue = startVal;

        _fromCurrentValue = false;
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
        SetType(type);
        SetEase(ease);
        SetReflexionFiels(propertyName);
        return this;
    }

    private void SetCommonValues(TweenValueType finalVal, float duration, TweenCore tween, string propertyName = "")
    {
        _finalValue = finalVal;
        base.duration = duration;
        myTween = tween;
        base.propertyName = propertyName;
        SetType(type);
        SetEase(ease);
    }

    private void SetReflexionFiels(string method)
    {
        if (_obj == null)
        {
            Debug.LogError("Given object to tween is null");
            return;
        }

        _property = _obj.GetType().GetProperty(method);
        if (_property == null) _field = _obj.GetType().GetField(method);

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

        if (_currentMethod == MethodUse.Reflexion && _fromCurrentValue) _startValue = GetObjValue();
        TriggerOnStart();

        if (_currentMethod == MethodUse.Reflexion && _obj == null)
        {
            Stop();
        }
    }

    public override void NewIteration()
    {
        isPlaying = true;
    }

    public override void Update(float deltaTime)
    {
        if (!isPlaying || isPaused) return;

        elapseTime += deltaTime;
        if (elapseTime <= delay) return;

        float elapse = Mathf.Clamp(elapseTime - delay, 0, duration);
        float w = Mathf.Clamp01(elapse / duration);
        w = RealWeight(w);

        if (lerpsFunc.ContainsKey(typeof(TweenValueType)))
        {
            TweenValueType value = (TweenValueType)lerpsFunc[typeof(TweenValueType)](_startValue, _finalValue, w);
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
        if (_property != null) _property.SetValue(_obj, _currentValue);
        else _field.SetValue(_obj, _currentValue);
    }

    private TweenValueType GetObjValue()
    {
        object value = default;

        if (_property != null) value = _property.GetValue(_obj);
        else if (_field != null) value = _field.GetValue(_obj);

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
        type = newType;
        SetTypeFunc(type);
        return this;
    }

    /// <summary>
    /// Set a custom tween Type, must be a function that return a float and have et float in parameters.
    /// </summary>
    /// <param name="customType">The function of the custome type, must return a float and take a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomType(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetCustomType(Func<float, float> customType)
    {
        TypeFunc = customType;
        type = TweenCoreType.Custom;
        return this;
    }

    /// <summary>
    /// Set the tween Ease.
    /// </summary>
    /// <param name="newEase">The wanted Tween Ease.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase)
    {
        ease = newEase;
        SetEaseFunc(ease);
        return this;
    }

    /// <summary>
    /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
    /// </summary>
    /// <param name="customEase">The function of the custom ease, 
    /// must return a float and take a Type function and a float in parameters.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetCustomEase(...).SetType(...);).</returns>
    public TweenCoreProperty<TweenValueType> SetCustomEase(Func<float, Func<float, float>, float> customEase)
    {
        ease = TweenCoreEase.Custom;
        EaseFunc = customEase;
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
        _fromCurrentValue = false;
        return this;
    }

    /// <summary>
    /// Use the start value of the property.
    /// !WARNING! Works only using a Reflexion method.
    /// </summary>
    /// <returns>This TweenProperty.</returns>
    public TweenCoreProperty<TweenValueType> FromCurrent()
    {
        _fromCurrentValue = true;
        return this;
    }

    public override TweenCorePropertyBase AddNextProperty(TweenCorePropertyBase property)
    {
        _nextProperties.Add(property);
        return this;
    }

    private float RealWeight(float w)
    {
        return EaseFunc(w, TypeFunc);
    }

    protected override void TriggerOnStart()
    {
        base.TriggerOnStart();
        _unityEvents.unityOnStart?.Invoke();
    }

    protected override void TriggerOnUpdate()
    {
        base.TriggerOnUpdate();
        OnUpdateValue?.Invoke(_currentValue);
        _unityEvents.unityOnUpdate?.Invoke();
        _unityEvents.unityOnUpdateValue?.Invoke(_currentValue);
    }

    protected override void TriggerOnFinish()
    {
        base.TriggerOnFinish();
        _unityEvents.unityOnFinish?.Invoke();
    }

    protected override void TriggerOnLoopFinish()
    {
        base.TriggerOnLoopFinish();
        _unityEvents.unityOnLoopFinish?.Invoke();
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
    public override void Stop()
    {
        isPlaying = false;
        isPaused = true;
        
        SetToFinalVals();

        StartNextProperties();

        myTween.NewPropertyFinished();
        elapseTime = 0;
        hasStarted = false;

        if (!isLoop)
        {
            TriggerOnFinish();
            DestroyProperty();
        }
        else
        {
            TriggerOnLoopFinish();
        }
    }

    private void StartNextProperties()
    {
        int length = _nextProperties.Count - 1;
        for (int i = length; i >= 0; i--)
        {
            if (!isLoop) _nextProperties[i].Start();
            else _nextProperties[i].NewIteration();
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

        OnUpdateValue?.Invoke(_currentValue);
    }

    public override TweenCorePropertyBase SetToFinalVals()
    {
        SetValue(_finalValue);

        return this;
    }

    private void DestroyProperty()
    {
        myTween.DestroyTweenProperty(this);
    }
}