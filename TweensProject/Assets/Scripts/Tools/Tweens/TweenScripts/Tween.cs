using System;
using System.Collections.Generic;
using System.Diagnostics;

// Author : Auguste Paccapelo

public class Tween
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private List<TweenPropertyBase> _tweenProperties = new List<TweenPropertyBase>();

    // ----- Others ----- \\

    private bool _isPaused = true;
    private bool _hasStarted = false;

    private bool _isParallel = true;

    private bool _destroyWhenFinish = true;
    private bool _surviveOnSceneUnload = false;
    public bool SurviveOnSceneUnload => _surviveOnSceneUnload;

    private bool _isLoop = false;
    private int _numTweenFinished = 0;
    private int _exeptedNumProperties;

    public event Action OnStart;
    public event Action OnFinish;

    // ---------- FUNCTIONS ---------- \\

    /// <summary>
    /// You don't need to call this function, TweenManager is handling it.
    /// Update the tween and all properties, if all properties are finished, stop the tween.
    /// </summary>
    /// <param name="deltaTime">Time since last call.</param>
    /// <returns>This tween.</returns>
    public Tween Update(float deltaTime)
    {
        if (_isPaused) return this;
        
        for (int i = _tweenProperties.Count - 1; i >= 0; i--)
        {
            _tweenProperties[i].Update(deltaTime);
        }

        if (_numTweenFinished == _exeptedNumProperties)
        {
            if (!_isLoop) Stop();
            else
            {
                if (!_isParallel) _tweenProperties[0].NewIteration();
                else
                {
                    foreach (TweenPropertyBase property in _tweenProperties) property.NewIteration();
                }
                _numTweenFinished = 0;
            }
        }

        return this;
    }

    /// <summary>
    /// Pause the tween and all properties attached.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Pause().Resume();).</returns>
    public Tween Pause()
    {
        _isPaused = true;

        return this;
    }

    /// <summary>
    /// Resume the tween and all properties attached at the state it was paused.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Resume().Pause();).</returns>
    public Tween Resume()
    {
        _isPaused = false;

        return this;
    }

    /// <summary>
    /// Start the tween if this is called for the first time.
    /// In parrele mode, all properties start at the same time, in chain mode only one is executed at the time.
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.Play().Pause();).</returns>
    public Tween Play()
    {
        if (_hasStarted) return this;

        _numTweenFinished = 0;
        _hasStarted = true;
        _isPaused = false;

        TweenPropertyBase property;
        int length = _tweenProperties.Count-1;

        for (int i = length; i >= 1; i--)
        {
            property = _tweenProperties[i];
            property.SetLoop(_isLoop);
            if (_isParallel) property.Start();
            else
            {
                _tweenProperties[i-1].AddNextProperty(property);
            }
        }
        if (length > 0)
        {
            _tweenProperties[0].Start();
            _tweenProperties[0].SetLoop(_isLoop);
        }

        _exeptedNumProperties = _tweenProperties.Count;

        OnStart?.Invoke();

        return this;
    }

    /// <summary>
    /// Destroy the given property.
    /// !WARNING! This does not stop the property, 
    /// it will no longer be updated, but the TweenPropertyBase.OnFinish event will not be called.
    /// To Stop and Destroy a property use TweenPropertyBase.Stop();.
    /// </summary>
    /// <param name="property">The property to destroy.</param>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.DestroyTweenProperty(...).Play();).</returns>
    public Tween DestroyTweenProperty(TweenPropertyBase property)
    {
        if (!_tweenProperties.Contains(property)) throw new ArgumentException("The tween does not contain the given property to destroy");
        if (_destroyWhenFinish) _tweenProperties.Remove(property);
        return this;
    }

    /// <summary>
    /// Stop and then destroy all Tween Properties and then it self.
    /// OnFinish event is called here after all properties are stopped.
    /// </summary>
    public void Stop()
    {
        _hasStarted = false;
        _isPaused = false;
        int length = _tweenProperties.Count - 1;
        for (int i = length; i >= 0; i --)
        {
            if (_tweenProperties[i].HasStarted) _tweenProperties[i].Stop();
        }
        OnFinish?.Invoke();
        if (_destroyWhenFinish) TweenManager.Instance.RemoveTween(this);
    }

    /// <summary>
    /// Function used to create a new Tween.
    /// A Tween handle one or multiples TweenProperty.
    /// </summary>
    /// <returns>The tween created.</returns>
    public static Tween CreateTween()
    {
        Tween tween = new Tween();
        TweenManager.Instance.AddTween(tween);
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
    public TweenProperty<TweenValueType> NewProperty<TweenValueType>(TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenProperty<TweenValueType> property = new TweenProperty<TweenValueType>(startVal, finalVal, time, this);
        _tweenProperties.Add(property);
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
    public TweenProperty<TweenValueType> NewProperty<TweenValueType>(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenProperty<TweenValueType> property = new TweenProperty<TweenValueType>(function, startVal, finalVal, time, this);
        _tweenProperties.Add(property);
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
    public TweenProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType finalVal, float time)
    {
        TweenProperty<TweenValueType> property = new TweenProperty<TweenValueType>(obj, method, finalVal, time, this);
        _tweenProperties.Add(property);
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
    public TweenProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float time)
    {
        TweenProperty<TweenValueType> property = new TweenProperty<TweenValueType>(obj, method, startVal, finalVal, time, this);
        _tweenProperties.Add(property);
        return property;
    }

    /// <summary>
    /// Add an existing property to the list of properties.
    /// </summary>
    /// <param name="property">The proeprty to add.</param>
    /// <returns>The property added.</returns>
    public TweenPropertyBase AddProperty(TweenPropertyBase property)
    {
        _tweenProperties.Add(property);
        property.AttachedTween = this;
        return property;
    }

    /// <summary>
    /// Set the Parallel or Chain mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <param name="isParallel">If is in parallel.</param>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetParallel(true).Play();).</returns>
    public Tween SetParallel(bool isParallel)
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
    public Tween SetChain(bool isChain)
    {
        _isParallel = !isChain;
        return this;
    }

    /// <summary>
    /// Set the Parallel mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetChain(true).Play();).</returns>
    public Tween Parallel()
    {
        _isParallel = true;
        return this;
    }

    /// <summary>
    /// Set the loop mode, wait for all properties to be finished, and then replay the tween.
    /// </summary>
    /// <param name="isLoop">If loop mode.</param>
    /// <returns>This tween.</returns>
    public Tween SetLoop(bool isLoop)
    {
        _isLoop = isLoop;
        return this;
    }

    /// <summary>
    /// Use to track the number of tweens properties finished for loop mode.
    /// Do not call this function or it may cause unexpeted results.
    /// </summary>
    public void NewPropertyFinished()
    {
        _numTweenFinished++;
    }

    /// <summary>
    /// Set the Chain mode, if Parallel all tweensProperties Play at the same time, in Chain only one can play at the time.
    /// Parallel is true by default;
    /// </summary>
    /// <returns>This tween, so you can chained the methods calls (e.g. tween.SetChain(true).Play();).</returns>
    public Tween Chain()
    {
        _isParallel = false;
        return this;
    }

    /// <summary>
    /// The tween will then survive when the scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween SurviveOnSceneLoad()
    {
        _surviveOnSceneUnload = true;
        return this;
    }

    /// <summary>
    /// The tween will not survive when the scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween KillOnSceneUnLoad()
    {
        _surviveOnSceneUnload = false;
        return this;
    }

    /// <summary>
    /// Set if the tween should survive or not on scene unloads.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween SetSurviveOnUnload(bool survive)
    {
        _surviveOnSceneUnload = survive;
        return this;
    }

    /// <summary>
    /// This tween and properties attached will be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween DestroyWhenFinish()
    {
        _destroyWhenFinish = true;
        return this;
    }

    /// <summary>
    /// This tween and propreties attached will not be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween DontDestroyWhenFinish()
    {
        _destroyWhenFinish = false;
        return this;
    }

    /// <summary>
    /// Set if this tween and properties attached should be destroyed when finished.
    /// </summary>
    /// <returns>This Tween.</returns>
    public Tween SetDestroyWhenFinish(bool destroy)
    {
        _destroyWhenFinish = destroy;
        return this;
    }
}