using System.Collections.Generic;
using UnityEngine;

// Author : Auguste Paccapelo

public class TweenComponent : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private Tween _tween;
    public Tween Tween => _tween;

    // ----- Others ----- \\

    [SerializeReference] private List<TweenPropertyBase> _properties = new List<TweenPropertyBase>();

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake() { }

    private void Start()
    {
        _tween = Tween.CreateTween();
        foreach (TweenPropertyBase property in _properties)
        {
            property.SetBaseValues();
            _tween.AddProperty(property);
        }
        TweenManager.Instance.AddTween(_tween);
        _tween.Play();
    }

    private void Update() { }

    // ----- My Functions ----- \\

    public void AddProperty(TweenPropertyBase property)
    {
        _properties.Add(property);
    }

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}