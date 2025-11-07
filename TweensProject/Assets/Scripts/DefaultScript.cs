using UnityEngine;

// Author : Auguste Paccapelo

public class DefaultScript : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private Tween _myTween;

    // ----- Others ----- \\

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _myTween = Tween.CreateTween().SurviveOnSceneLoad();
        _myTween.NewProperty(TweenFunc, Vector3.zero, new Vector3(5, 5, 0), 2f)
            .SetEase(TweenEase.Out).SetType(TweenType.Elastic);
    }

    private void Update() { }

    // ----- My Functions ----- \\

    private void TweenFunc(Vector3 pos)
    {
        transform.position = pos;
    }

    public void PlayTween()
    {
        //_myTween.Play();
    }

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}