using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author : Auguste Paccapelo

public class TweenComponent : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private Tween _tween;
    public Tween Tween => _tween;

    // ----- Others ----- \\

    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _isParallel = true;
    [SerializeField] private bool _isLoop = false;
    [SerializeField] private bool _DestroyedWhenFinished = true;
    [SerializeField] private bool _surviveOnUnload = false;

    [SerializeReference] private List<TweenPropertyBase> _properties = new List<TweenPropertyBase>();

    [SerializeField] private UnityEvent OnStart;
    [SerializeField] private UnityEvent OnFinish;

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake() { }

    private void Start()
    {
        _tween = Tween.CreateTween()
            .SetParallel(_isParallel).SetLoop(_isLoop)
            .SetSurviveOnUnload(_surviveOnUnload)
            .SetDestroyWhenFinish(_DestroyedWhenFinished);
        
        foreach (TweenPropertyBase property in _properties)
        {
            _tween.AddProperty(property);
            property.SetBaseValues();
        }
        TweenManager.Instance.AddTween(_tween);

        _tween.OnStart += OnTweenStart;
        _tween.OnFinish += OnTweenFinish;

        if (_playOnStart) Play();
    }

    // ----- My Functions ----- \\

    public void AddProperty(TweenPropertyBase property)
    {
        _properties.Add(property);
    }

    public void Play()
    {
        _tween.Play();
    }

    public void Stop()
    {
        _tween.Stop();
    }

    private void OnTweenStart()
    {
        OnStart?.Invoke();
    }

    private void OnTweenFinish()
    {
        OnFinish?.Invoke();
    }

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}