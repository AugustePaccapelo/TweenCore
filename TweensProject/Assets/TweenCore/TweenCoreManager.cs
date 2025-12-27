using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Author : Auguste Paccapelo

public class TweenCoreManager : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Singleton ----- \\

    private static TweenCoreManager _instance;
    public static TweenCoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject(nameof(TweenCoreManager));
                _instance = obj.AddComponent<TweenCoreManager>();
            }
            return _instance;
        }
    }

    // ----- Objects ----- \\

    private List<TweenCore> _tweens = new List<TweenCore>();

    // ----- Others ----- \\

    private bool _isPlaying = true;
    public bool IsPlaying => _isPlaying;

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void Awake()
    {
        // Singleton
        if (_instance != null && _instance != this)
        {
            Debug.Log(nameof(TweenCoreManager) + " Instance already exist, destorying last added.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!_isPlaying) return;
        
        for (int i = _tweens.Count - 1; i >= 0; i--)
        {
            _tweens[i].Update(Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // ----- My Functions ----- \\

    private void OnSceneUnloaded(Scene scene)
    {
        int length = _tweens.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (!_tweens[i].SurviveOnSceneUnload) _tweens[i].Stop();
        }
    }

    public void PauseAll()
    {
        _isPlaying = false;
    }

    public void ResumeAll()
    {
        _isPlaying = true;
    }

    public void StopAll()
    {
        int length = _tweens.Count - 1;
        for (int i = length;  i >= 0; i--)
        {
            _tweens[i].Stop();
        }
    }

    public void AddTween(TweenCore tween)
    {
        if (!_tweens.Contains(tween)) _tweens.Add(tween);
    }

    public void RemoveTween(TweenCore tween)
    {
        if (_tweens.Contains(tween)) _tweens.Remove(tween);
    }

    // ----- Destructor ----- \\

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}