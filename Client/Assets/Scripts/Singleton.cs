using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

    private static T _instance;

    private static object _lock = new object();

    private static bool _applicationIsQuiting = false;

    public static T Instance {
        get {
            if (_applicationIsQuiting)
            {
                return null;
            }
            if (_instance == null) {
                lock (_lock) {
                    _instance = FindObjectOfType<T>();
                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        return _instance;
                    }
                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }

    public virtual void Awake()
    {
        OnInitialize();
    }

    public virtual void OnInitialize() { return; }

    public virtual void OnRelease() { }

    private void OnDestroy()
    {
        _applicationIsQuiting = true;
    }

}
