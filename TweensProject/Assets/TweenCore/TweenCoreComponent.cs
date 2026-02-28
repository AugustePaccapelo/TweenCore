using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author : Auguste Paccapelo

public class TweenCoreComponent : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private TweenCore _tween;
    public TweenCore Tween => _tween;

    // ----- Others ----- \\

    [SerializeField] private string _name = "";
    public string TweenName
    {
        get => _name;
        set => _name = value;
    }

    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _isParallel = true;
    [SerializeField] private bool _isLoop = false;
    [SerializeField] private bool _isInfinite = false;
    [SerializeField] private int _numIteration = 1;
    [SerializeField] private bool _DestroyWhenFinished = true;
    [SerializeField] private bool _surviveOnUnload = false;

    [SerializeReference] private List<TweenCorePropertyBase> _properties = new List<TweenCorePropertyBase>();

    [Serializable]
    private class TweenUnityEvents
    {
        public UnityEvent<TweenCore> OnStart;
        public UnityEvent<TweenCore> OnUpdate;
        public UnityEvent<TweenCore> OnFinish;
        public UnityEvent<TweenCore> OnLoopFinish;
    }

    [SerializeField] private TweenUnityEvents _unityEvents = new TweenUnityEvents();

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void Awake()
    {
        _tween = TweenCore.CreateTween();
    }

    private void Start()
    {
        if (_isLoop && _isInfinite) _numIteration = -1;

        _tween.SetLoop(_isLoop, _numIteration)
            .SetParallel(_isParallel)
            .SetSurviveOnUnload(_surviveOnUnload)
            .SetDestroyWhenFinish(_DestroyWhenFinished);

        if (_surviveOnUnload)
        {
            DontDestroyOnLoad(gameObject);
        }

        foreach (TweenCorePropertyBase property in _properties)
        {
            _tween.AddProperty(property);
            property.SetBaseValues();
        }

        TweenCoreManager.Instance?.AddTween(_tween);

        _tween.OnStart += OnTweenStart;
        _tween.OnUpdate += OnTweenUpdate;
        _tween.OnFinish += OnTweenFinish;
        _tween.OnLoopFinish += OnTweenLoopFinish;

        if (_playOnStart) Play();
    }

    // ----- My Functions ----- \\

    public void AddProperty(TweenCorePropertyBase property)
    {
        _properties.Add(property);
    }

    public void Play()
    {
        _tween.Play();
    }

    public void StopAndSetToFinalValue()
    {
        _tween.Stop(true);
    }

    public void StopAndDontChangeValue()
    {
        _tween.Stop(false);
    }

    private void OnTweenStart(TweenCore tween)
    {
        _unityEvents.OnStart?.Invoke(tween);
    }

    private void OnTweenUpdate(TweenCore tween)
    {
        _unityEvents.OnUpdate?.Invoke(tween);
    }

    private void OnTweenFinish(TweenCore tween)
    {
        _unityEvents.OnFinish?.Invoke(tween);
    }

    private void OnTweenLoopFinish(TweenCore tween)
    {
        _unityEvents.OnLoopFinish?.Invoke(tween);
    }

    // ----- Destructor ----- \\

    private void OnDestroy()
    {
        _tween?.Stop(false);
        _tween.DestroyTween();
    }
}