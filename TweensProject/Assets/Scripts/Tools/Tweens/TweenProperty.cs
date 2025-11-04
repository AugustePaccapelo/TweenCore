using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author : Auguste Paccapelo

[Serializable]
public class TweenProperty<TweenValueType> : TweenPropertyBase
{
    // ---------- VARIABLES ---------- \\

    // ----- Others ----- \\

    [SerializeField] private bool _fromCurrentValue = true;
    [SerializeField] private TweenValueType _startValue;
    [SerializeField] private TweenValueType _finalValue;
    private TweenValueType _currentValue;
    public TweenValueType CurrentValue => _currentValue;

    private bool _isPlaying = false;
    private bool _hasStarted = false;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<TweenValueType> _function;

    private List<TweenPropertyBase> _nextProperties = new List<TweenPropertyBase>();

    [Serializable]
    private class TweenUnityEvents
    {
        public UnityEvent unityOnStart;
        public UnityEvent unityOnFinish;
    }

    [SerializeField] private TweenUnityEvents _unityEvents;

    public event Action<TweenValueType> OnUpdate;

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// Constructor of TweenProperty for when not modifying a property or field.
    /// </summary>
    /// <param name="startVal">The start value.</param>
    /// <param name="finalVal">The end value.</param>
    /// <param name="time">The duration.</param>
    /// <param name="tween">The attached tween.</param>
    public TweenProperty(TweenValueType startVal, TweenValueType finalVal, float time, Tween tween)
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
    public TweenProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration, Tween tween)
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
    public TweenProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        
        SetCommonValues(finalVal, duration, tween, method);

        _obj = obj;
        SetReflexionFiels(_propertyName);

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
    public TweenProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;

        SetCommonValues(finalVal, duration, tween, method);

        _obj = obj;
        SetReflexionFiels(_propertyName);
        _startValue = startVal;

        _fromCurrentValue = false;
    }

    /// <summary>
    /// An empty constructor just to create an object.
    /// Properties and fields are assign in the editor.
    /// </summary>
    public TweenProperty()
    {
        _currentMethod = MethodUse.Reflexion;
    }

    /// <summary>
    /// Set the base values when using the empty constructor.
    /// Using this in a different context may have unexpted results.
    /// </summary>
    /// <returns>This TweenPropertyBase.</returns>
    public override TweenPropertyBase SetBaseValues()
    {
        SetType(type);
        SetEase(ease);
        SetReflexionFiels(_propertyName);
        return this;
    }

    private void SetCommonValues(TweenValueType finalVal, float duration, Tween tween, string propertyName = "")
    {
        _finalValue = finalVal;
        time = duration;
        myTween = tween;
        _propertyName = propertyName;
        SetType(type);
        SetEase(ease);
    }

    private void SetReflexionFiels(string method)
    {
        _property = _obj.GetType().GetProperty(method);
        if (_property == null)
            _field = _obj.GetType().GetField(method);
        if (_property == null && _field == null)
        {
            Stop();
            Debug.LogError("No property or field found");
        }
    }

    public override void Start()
    {
        if (_hasStarted) return;

        _hasStarted = true;
        _isPlaying = true;
        if (_currentMethod == MethodUse.Reflexion && _fromCurrentValue) _startValue = GetObjValue();
        TriggerOnStart();
    }

    public override void NewIteration()
    {
        _isPlaying = true;
    }

    public override void Update(float deltaTime)
    {
        if (!_isPlaying) return;

        _elapseTime += deltaTime;
        if (_elapseTime <= delay) return;

        float elapse = Mathf.Clamp(_elapseTime - delay, 0, time);
        float w = Mathf.Clamp01(elapse / time);
        w = RealWeight(w);
        if (lerpsFunc.ContainsKey(typeof(TweenValueType)))
        {
            _currentValue = (TweenValueType)lerpsFunc[typeof(TweenValueType)](_startValue, _finalValue, w);
            OnUpdate?.Invoke(_currentValue);
        }
        else
        { 
            throw new ArgumentException("The ValueType given is not supported (" + typeof(TweenValueType) + ").");
        }

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

        if (elapse >= time) Stop();
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
        object value = _property != null ? _property.GetValue(_obj) : _field.GetValue(_obj);
        return (TweenValueType)value;
    }

    /// <summary>
    /// Add a delay before the animation start.
    /// </summary>
    /// <param name="tweenDelay">Time to wait in seconds.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetDelay(...).SetEase(...);).</returns>
    public TweenProperty<TweenValueType> SetDelay(float tweenDelay)
    {
        delay = tweenDelay;
        return this;
    }

    /// <summary>
    /// Set the tween type.
    /// </summary>
    /// <param name="newType">The wanted tween type.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
    public TweenProperty<TweenValueType> SetType(TweenType newType)
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
    public TweenProperty<TweenValueType> SetCustomType(Func<float, float> customType)
    {
        TypeFunc = customType;
        type = TweenType.Custom;
        return this;
    }

    /// <summary>
    /// Set the tween Ease.
    /// </summary>
    /// <param name="newEase">The wanted Tween Ease.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
    public TweenProperty<TweenValueType> SetEase(TweenEase newEase)
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
    public TweenProperty<TweenValueType> SetCustomEase(Func<float, Func<float, float>, float> customEase)
    {
        ease = TweenEase.Custom;
        EaseFunc = customEase;
        return this;
    }

    public TweenValueType GetCurrentValue() => CurrentValue;

    /// <summary>
    /// Set the start value of the property.
    /// </summary>
    /// <param name="value">The start value.</param>
    /// <returns>This TweenProperty to chain the methods calls (e.g. property.From(...).SetEase(...);).</returns>
    public TweenProperty<TweenValueType> From(TweenValueType value)
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
    public TweenProperty<TweenValueType> FromCurrent()
    {
        _fromCurrentValue = true;
        return this;
    }

    public override TweenPropertyBase AddNextProperty(TweenPropertyBase property)
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

    protected override void TriggerOnFinish()
    {
        base.TriggerOnFinish();
        _unityEvents.unityOnFinish?.Invoke();
    }

    /// <summary>
    /// Pause the TweenProperty.
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// Resume the TweenProperty at the last state.
    /// </summary>
    public void Resume()
    {
        _isPlaying = true;
    }

    /// <summary>
    /// Stop and Destroy the TweenProperty.
    /// </summary>
    public override void Stop()
    {
        _isPlaying = false;
        TriggerOnFinish();

        StartNextProperties();

        if (!_isLoop) DestroyProperty();
        else
        {
            myTween.NewPropertyFinishedLoop();
            _elapseTime = 0;
        }
    }

    private void StartNextProperties()
    {
        int length = _nextProperties.Count - 1;
        for (int i = length; i >= 0; i--)
        {
            if (!_isLoop) _nextProperties[i].Start();
            else _nextProperties[i].NewIteration();
            //_nextProperties.RemoveAt(i);
        }
    }

    private void DestroyProperty()
    {
        myTween.DestroyTweenProperty(this);
    }
}