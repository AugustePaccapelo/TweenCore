using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

public class TweenCore
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private List<TweenCorePropertyBase> _tweenProperties = new List<TweenCorePropertyBase>();

    // ----- Others ----- \\

    private bool _isPlaying = false;
    public bool IsPlaying => _isPlaying;

    private bool _isPaused = true;
    public bool IsPaused => _isPaused;

    private bool _hasStarted = false;
    public bool HasStarted => _hasStarted;

    private bool _isFinished = false;
    public bool IsFinished => _isFinished;

    private bool _isParallel = true;
    public bool IsParallel => _isParallel;

    private bool _isLoop = false;
    public bool IsLoop => _isLoop;

    private bool _destroyOnFinish = true;
    public bool DestroyOnFinish => _destroyOnFinish;

    private bool _surviveOnSceneUnload = false;
    public bool SurviveOnSceneUnload => _surviveOnSceneUnload;

    private int _numPropertiesFinished = 0;
    public int NumPropertiesFinished => _numPropertiesFinished;

    private int _exeptedNumProperties;

    public int NumProperties => _exeptedNumProperties;

    private float _elapseTime = 0f;
    public float ElapseTime => _elapseTime;

    private int _numIteration = -1;
    public int NumIteration => _numIteration;

    private int _currentIteration = 0;
    public int CurrentIteration => _currentIteration;

    public event Action<TweenCore> OnStart;
    public event Action<TweenCore> OnUpdate;
    public event Action<TweenCore> OnFinish;
    public event Action<TweenCore> OnLoopFinish;

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// You don't need to call this function, TweenManager is handling it.
    /// Update the tween and all properties, if all properties are finished, stop the tween.
    /// </summary>
    /// <param name="deltaTime">Time since last call.</param>
    /// <returns>This tween.</returns>
    public TweenCore Update(float deltaTime)
    {
        if (!_isPlaying || _isPaused) return this;
        
        OnUpdate?.Invoke(this);
        _elapseTime += deltaTime;

        for (int i = _tweenProperties.Count - 1; i >= 0; i--)
        {
            _tweenProperties[i].Update(deltaTime);
        }

        if (_numPropertiesFinished == _exeptedNumProperties)
        {
            _currentIteration++;
            
            if (_isLoop && (_numIteration < 0 || _currentIteration < _numIteration))
            {
                RestartTween();
            }
            else
            {
                Stop();
            }
        }
        
        return this;
    }

    private void RestartTween()
    {
        OnLoopFinish?.Invoke(this);
        _numPropertiesFinished = 0;

        if (!_isParallel) _tweenProperties[0].Start();
        else
        {
            foreach (TweenCorePropertyBase property in _tweenProperties) property.Start();
        }
    }

    /// <summary>
    /// Pause the tween and all properties attached.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Pause().Resume();).</returns>
    public TweenCore Pause()
    {
        _isPaused = true;

        return this;
    }

    /// <summary>
    /// Resume the tween and all properties attached at the state it was paused.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Resume().Pause();).</returns>
    public TweenCore Resume()
    {
        _isPaused = false;

        return this;
    }

    /// <summary>
    /// Start the tween if this is called for the first time.
    /// In parrele mode, all properties start at the same time, in chain mode only one is executed at the time.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Play().Pause();).</returns>
    public TweenCore Play()
    {
        // Can't start 2 times
        if (_hasStarted) return this;
        
        // Set values
        _numPropertiesFinished = 0;
        _currentIteration = 0;
        _hasStarted = true;
        _isPaused = false;
        _isPlaying = true;
        _isFinished = false;

        _elapseTime = 0;

        OnStart?.Invoke(this);

        TweenCorePropertyBase property;

        int numProperties = _tweenProperties.Count;

        // Iterrate on each property
        for (int i = 0; i < numProperties; i++)
        {
            property = _tweenProperties[i];
            
            // If parallel, start all properties
            if (_isParallel) property.Start();
            // If chain, build the chain and skip last
            else if (i < numProperties - 1)
            {
                property.AddNextProperty(_tweenProperties[i + 1]);
            }
        }

        // If in chain, start first property after the chain is built
        if (!_isParallel && numProperties > 0)
        {
            _tweenProperties[0].Start();
        }

        // Tracking of finished properties
        _exeptedNumProperties = _tweenProperties.Count;

        if (_isLoop && _numIteration == 0)
        {
            Stop();
        }

        return this;
    }

    private void DestroyTweenProperty(TweenCorePropertyBase property)
    {
        if (!_tweenProperties.Contains(property)) throw new ArgumentException("The tween does not contain the given property to destroy");
        _tweenProperties.Remove(property);
    }

    private void NewPropertyFinished(TweenCorePropertyBase property)
    {
        _numPropertiesFinished++;
        
        if (!_isLoop && _destroyOnFinish)
        {
            DestroyTweenProperty(property);
        }
    }

    /// <summary>
    /// Stop and then destroy all Tween Properties and then it self.
    /// OnFinish event is called here after all properties are stopped.
    /// </summary>
    public void Stop(bool setToFinalValue = true)
    {
        if (!_hasStarted) return;

        _hasStarted = false;
        _isPaused = false;
        _isPlaying = false;
        _elapseTime = 0;
        _currentIteration = 0;
        _numPropertiesFinished = 0;

        int length = _tweenProperties.Count - 1;
        for (int i = length; i >= 0; i --)
        {
            //if (_tweenProperties[i].HasStarted) _tweenProperties[i].Stop(setToFinalValue);
            if (_tweenProperties[0].HasStarted) _tweenProperties[0].Stop(setToFinalValue);
        }
        _isFinished = true;
        OnFinish?.Invoke(this);
        if (_destroyOnFinish) DestroyTween();
    }

    /// <summary>
    /// Function used to create a new Tween.
    /// A Tween handle one or multiples TweenProperty.
    /// </summary>
    /// <returns>The tween created.</returns>
    public static TweenCore CreateTween()
    {
        TweenCore tween = new TweenCore();
        TweenCoreManager.Instance?.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Create a new TweenProperty that don't modify any exterior property or field.
    /// Use OnUpdate or CurrentValue to get the value of the property.
    /// </summary>
    /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
    /// <param name="startVal">The start value of the property.</param>
    /// <param name="finalVal">The end value of the property.</param>
    /// <param name="time">The duration of the property.</param>
    /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(startVal, finalVal, time);
        AddProperty(property);
        return property;
    }

    /// <summary>
    /// Create a new TweenProperty that use a function to modify a property or field.
    /// This use less ressources but is a bit harder to use.
    /// </summary>
    /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
    /// <param name="function">The function to run each frame when udpating the value
    ///  (e.g. v => transform.position = v)</param>
    /// <param name="startVal">The start value of the property.</param>
    /// <param name="finalVal">The end value of the property.</param>
    /// <param name="time">The duration of the property.</param>
    /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(function, startVal, finalVal, time);
        AddProperty(property);
        return property;
    }

    /// <summary>
    /// Create a new TweenProperty that modify the given method of the given object.
    /// This use reflexion, it use more ressources but is a lot easier to use.
    /// By default startValue is the value when Play() is call.
    /// </summary>
    /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
    /// <param name="obj">The target object of the tween (e.g. transform)</param>
    /// <param name="method">The method to modify (e.g. "position")</param>
    /// <param name="finalVal">The end value of the property.</param>
    /// <param name="time">The duration of the property.</param>
    /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType finalVal, float time)
    {
        TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(obj, method, finalVal, time);
        AddProperty(property);
        return property;
    }

    /// <summary>
    /// Create a new TweenProperty that modify the given method of the given object.
    /// This use reflexion, it use more ressources but is a lot easier to use.
    /// </summary>
    /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
    /// <param name="obj">The target object of the tween (e.g. transform)</param>
    /// <param name="method">The method to modify (e.g. "position")</param>
    /// <param name="startVal">The start value of the property.</param>
    /// <param name="finalVal">The end value of the property.</param>
    /// <param name="time">The duration of the property.</param>
    /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
    public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(obj, method, startVal, finalVal, time);
        AddProperty(property);
        return property;
    }

    public TweenCore AddProperty(TweenCorePropertyBase property)
    {
        _tweenProperties.Add(property);
        property.OnFinish += NewPropertyFinished;
        return this;
    }
    
    /// <summary>
    /// Set the Parallel or Chain mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <param name="isParallel">If is in parallel.</param>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetParallel(true).Play();).</returns>
    public TweenCore SetParallel(bool isParallel)
    {
        _isParallel = isParallel;
        return this;
    }

    /// <summary>
    /// Set the Parallel or Chain mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <param name="isChain">If is in chain.</param>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetChain(true).Play();).</returns>
    public TweenCore SetChain(bool isChain)
    {
        _isParallel = !isChain;
        return this;
    }

    /// <summary>
    /// Set the Parallel mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetChain(true).Play();).</returns>
    public TweenCore Parallel()
    {
        _isParallel = true;
        return this;
    }

    /// <summary>
    /// Set the loop mode, wait for all properties to be finished, and then replay the tween.
    /// </summary>
    /// <param name="isLoop">If loop mode.</param>
    /// <param name="numIteration"> Number of iteration, negative for infinte. </param>
    /// <returns>This tween.</returns>
    public TweenCore SetLoop(bool isLoop, int numIteration = -1)
    {
        _isLoop = isLoop;
        _numIteration = numIteration;
        return this;
    }

    /// <summary>
    /// Set the Chain mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetChain(true).Play();).</returns>
    public TweenCore Chain()
    {
        _isParallel = false;
        return this;
    }

    /// <summary>
    /// The tween will then survive when the scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore SurviveOnSceneLoad()
    {
        _surviveOnSceneUnload = true;
        return this;
    }

    /// <summary>
    /// The tween will not survive when the scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore KillOnSceneUnLoad()
    {
        _surviveOnSceneUnload = false;
        return this;
    }

    /// <summary>
    /// Set if the tween should survive or not on scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore SetSurviveOnUnload(bool survive)
    {
        _surviveOnSceneUnload = survive;
        return this;
    }

    /// <summary>
    /// This tween and properties attached will be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore DestroyWhenFinish()
    {
        _destroyOnFinish = true;
        return this;
    }

    /// <summary>
    /// This tween and propreties attached will not be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore DontDestroyWhenFinish()
    {
        _destroyOnFinish = false;
        return this;
    }

    /// <summary>
    /// Set if this tween and properties attached should be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public TweenCore SetDestroyWhenFinish(bool destroy)
    {
        _destroyOnFinish = destroy;
        return this;
    }

    /// <summary>
    /// Destroy this TweenCore without modifying the value.
    /// </summary>
    public void DestroyTween()
    {
        TweenCoreManager.Instance?.RemoveTween(this);
    }
}