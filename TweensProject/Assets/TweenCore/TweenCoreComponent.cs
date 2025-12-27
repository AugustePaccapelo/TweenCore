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

    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _isParallel = true;
    [SerializeField] private bool _isLoop = false;
    [SerializeField] private bool _DestroyedWhenFinished = true;
    [SerializeField] private bool _surviveOnUnload = false;

    [SerializeReference] private List<TweenCorePropertyBase> _properties = new List<TweenCorePropertyBase>();

    [SerializeField] private UnityEvent<TweenCore> OnStart;
    [SerializeField] private UnityEvent<TweenCore> OnFinish;

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake() { }

    private void Start()
    {
        _tween = TweenCore.CreateTween()
            .SetParallel(_isParallel).SetLoop(_isLoop)
            .SetSurviveOnUnload(_surviveOnUnload)
            .SetDestroyWhenFinish(_DestroyedWhenFinished);
        
        foreach (TweenCorePropertyBase property in _properties)
        {
            _tween.AddProperty(property);
            property.SetBaseValues();
        }
        TweenCoreManager.Instance.AddTween(_tween);

        _tween.OnStart += OnTweenStart;
        _tween.OnFinish += OnTweenFinish;

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

    public void Test(bool test = true)
    {

    }

    private void OnTweenStart(TweenCore tween)
    {
        OnStart?.Invoke(tween);
    }

    private void OnTweenFinish(TweenCore tween)
    {
        OnFinish?.Invoke(tween);
    }

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}