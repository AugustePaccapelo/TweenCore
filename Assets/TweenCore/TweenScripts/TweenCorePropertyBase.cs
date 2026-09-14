using System;
using System.Collections.Generic;
using UnityEngine;

// Author : Auguste Paccapelo

[Serializable]
public abstract class TweenCorePropertyBase
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    [SerializeField] private GameObject _tweenTargetObj;
    [SerializeField, HideInInspector] protected UnityEngine.Object obj;
    public UnityEngine.Object TargetObject => obj;

    [SerializeField, HideInInspector] private UnityEngine.Object _lastKnownObject;
    [SerializeField, HideInInspector] private GameObject _lastKnownTweenTargetGO;

    // ----- Others ----- \\

    private const float BACK_C1 = 1.70158f;
    private const float BACK_C3 = BACK_C1 + 1;
    private const float ELASTIC_C4 = (2 * Mathf.PI) / 3;
    private const float BOUNCE_N1 = 7.5625f;
    private const float BOUNCE_D1 = 2.75f;

    protected enum MethodUse
    {
        Reflexion, Strategy, ReturnValue
    }

    [SerializeField] protected TweenCoreType type = TweenCoreType.Linear;
    public TweenCoreType Type => type;

    [SerializeField] protected TweenCoreEase ease = TweenCoreEase.In;
    public TweenCoreEase Ease => ease;

    [SerializeField] protected float duration = 1f;
    public float Duration => duration;

    [SerializeField] protected float delay = 0f;
    public float Delay => delay;

    [SerializeField] protected bool fromCurrentValue = false;
    public bool FromCurrentValue => fromCurrentValue;

    [SerializeField] protected bool isIncreasingValue = false;
    public bool IsIncreasingValue => isIncreasingValue;

    protected Func<float, Func<float, float>, float> EaseFunc;
    protected Func<float, float> TypeFunc;

    [SerializeField] protected AnimationCurve easeAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] protected AnimationCurve typeAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField, HideInInspector] protected string propertyName;
    public string PropertyName => propertyName;

    [SerializeField, HideInInspector] protected int propertyIndex;

    [SerializeField] protected bool isEmpty = false;

    protected bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    protected bool isPaused = true;
    public bool IsPaused => isPaused;

    protected bool hasStarted = false;
    public bool HasStarted => hasStarted;

    protected bool isFinish = false;
    public bool IsFinish => isFinish;

    protected float elapseTime = 0f;
    public float ElapseTime => elapseTime;

    public event Action<TweenCorePropertyBase> OnStart;
    public event Action<TweenCorePropertyBase> OnUpdate;
    public event Action<TweenCorePropertyBase> OnFinish;

    protected static readonly Dictionary<Type, Func<object, object, float, object>> lerpsFunc = new Dictionary<Type, Func<object, object, float, object>>()
    {
        // C# types
        {typeof(float), (a, b, t) => (float)a + ((float)b - (float)a) * t},
        {typeof(double), (a, b, t) => (double)a + ((double)b - (double)a) * t},
        {typeof(int), (a, b, t) => (int)a + ((int)b - (int)a) * t },
        {typeof(uint), (a, b, t) => (uint)a + ((uint)b - (uint)a) * t },
        {typeof(long), (a, b, t) => (long)a + ((long)b - (long)a) * t },
        {typeof(ulong), (a, b, t) => (ulong)a + ((ulong)b - (ulong)a) * t },
        {typeof(decimal), (a, b, t) => (decimal)a + ((decimal)b - (decimal)a) * (decimal)t },
        // Unity types
        {typeof(Vector2), (a, b, t) => (Vector2)a + ((Vector2)b - (Vector2)a) * t },
        {typeof(Vector3), (a, b, t) => (Vector3)a + ((Vector3)b - (Vector3)a) * t },
        {typeof(Vector4), (a, b, t) => (Vector4)a + ((Vector4)b - (Vector4)a) * t },
        {typeof(Quaternion), (a, b, t) => Quaternion.Lerp((Quaternion)a, (Quaternion)b, t)},
        {typeof(Color), (a, b, t) => (Color)a + ((Color)b - (Color)a) * t },
        {typeof(Color32), (a, b, t) => Color32.Lerp((Color32)a, (Color32)b, t)},
    };

    protected static readonly Dictionary<Type, Func<object, object, object>> addFuncs = new()
    {
        // C# types
        {typeof(float), (a, b) => (float)a + (float)b },
        {typeof(double), (a, b) => (double)a + (double)b },
        {typeof(int), (a, b) => (int)a + (int)b },
        {typeof(uint), (a, b) => (uint)a + (uint)b },
        {typeof(long), (a, b) => (long)a + (long)b },
        {typeof(ulong), (a, b) => (ulong)a + (ulong)b },
        {typeof(decimal), (a, b) => (decimal)a + (decimal)b },
        // Unity types
        {typeof(Vector2), (a, b) => (Vector2)a + (Vector2)b },
        {typeof(Vector3), (a, b) => (Vector3)a + (Vector3)b },
        {typeof(Vector4), (a, b) => (Vector4)a + (Vector4)b },
        {typeof(Color), (a, b) => (Color)a + (Color)b }
    };

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// Update the TweenProperty, you don't need to call this function, the Tween attched si handling it.
    /// OnUpdate is called here.
    /// </summary>
    /// <param name="deltaTime">Time since last call.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Start the TweenProperty, OnStart is called here.
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Stop and destroy the TweenProperty, OnFinish is called here.
    /// </summary>
    public abstract void Stop(bool setToFinalValue = true);

    /// <summary>
    /// Set the property to the final value. 
    /// You should call Stop() or Pause() if tween is playing.
    /// </summary>
    /// <returns>This TweenPropertyBase.</returns>
    public abstract TweenCorePropertyBase SetToFinalVals();

    /// <summary>
    /// Add a TweenProperty to start when this TweenProperty is finished.
    /// </summary>
    /// <param name="property">The TweenProperty to start.</param>
    /// <returns>This TweenPropertyBase.</returns>
    public abstract TweenCorePropertyBase AddNextProperty(TweenCorePropertyBase property);

    /// <summary>
    /// Set the base values when using the empty constructor.
    /// Using this in a different context may have unexpted results.
    /// </summary>
    /// <returns>This TweenPropertyBase.</returns>
    public abstract TweenCorePropertyBase SetBaseValues();

    protected virtual void TriggerOnStart() => OnStart?.Invoke(this);
    protected virtual void TriggerOnUpdate() => OnUpdate?.Invoke(this);
    protected virtual void TriggerOnFinish() => OnFinish?.Invoke(this);

    protected void SetTypeFunc(TweenCoreType newType)
    {
        switch (newType)
        {
            case TweenCoreType.Linear:
                TypeFunc = Linear;
                break;
            case TweenCoreType.Quad:
                TypeFunc = Quad;
                break;
            case TweenCoreType.Cubic:
                TypeFunc = Cubic;
                break;
            case TweenCoreType.Quart:
                TypeFunc = Quart;
                break;
            case TweenCoreType.Quint:
                TypeFunc = Quint;
                break;
            case TweenCoreType.Back:
                TypeFunc = Back;
                break;
            case TweenCoreType.Elastic:
                TypeFunc = Elastic;
                break;
            case TweenCoreType.Bounce:
                TypeFunc = Bounce;
                break;
            case TweenCoreType.Circ:
                TypeFunc = Circ;
                break;
            case TweenCoreType.Sine:
                TypeFunc = Sine;
                break;
            case TweenCoreType.Expo:
                TypeFunc = Expo;
                break;
        }
    }

    protected void SetEaseFunc(TweenCoreEase newEase)
    {
        switch (newEase)
        {
            case TweenCoreEase.In:
                EaseFunc = In;
                break;
            case TweenCoreEase.Out:
                EaseFunc = Out;
                break;
            case TweenCoreEase.InOut:
                EaseFunc = InOut;
                break;
            case TweenCoreEase.OutIn:
                EaseFunc = OutIn;
                break;
        }
    }

    // Types \\

    private float Linear(float t)
    {
        return t;
    }

    private float Quad(float t)
    {
        return t * t;
    }

    private float Cubic(float t)
    {
        return t * t * t;
    }

    private float Quart(float t)
    {
        return t * t * t * t;
    }

    private float Quint(float t)
    {
        return t * t * t * t * t;
    }

    private float Back(float t)
    {
        return BACK_C3 * t * t * t - BACK_C1 * t * t;
    }

    private float Elastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((float)(t * 10 - 10.75) * ELASTIC_C4);
    }

    private float Bounce(float t)
    {
        if (t < 1f / BOUNCE_D1) return BOUNCE_N1 * t * t;
        else if (t < 2f / BOUNCE_D1)
        {
            t -= 1.5f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.75f;
        }
        else if (t < 2.5f / BOUNCE_D1)
        {
            t -= 2.25f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.984375f;
        }
    }

    private float Circ(float t)
    {
        return 1 - Mathf.Sqrt(1 - t * t);
    }

    private float Sine(float t)
    {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    private float Expo(float t)
    {
        if (t == 0) return 0f;
        return Mathf.Pow(2, 10 * t - 10);
    }

    // Eases \\

    private float In(float t, Func<float, float> TypeFunc)
    {
        return TypeFunc(t);
    }

    private float Out(float t, Func<float, float> TypeFunc)
    {
        return 1 - TypeFunc(1 - t);
    }

    private float InOut(float t, Func<float, float> TypeFunc)
    {
        return t < 0.5f ?
            0.5f * TypeFunc(t * 2) :
            1 - 0.5f * TypeFunc(2 * (1 - t));
    }

    private float OutIn(float t, Func<float, float> TypeFunc)
    {
        return t < 0.5f ?
            0.5f * (1 - TypeFunc(1 - t * 2)) :
            0.5f + 0.5f * TypeFunc((t - 0.5f) * 2f);
            
    }
}