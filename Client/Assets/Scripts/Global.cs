using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Global : Singleton<Global>
{

    private ResManager _resManager;
    public ResManager ResManager => _resManager;

    private LuaManager _luaManager;
    public LuaManager LuaManager => _luaManager;

    private UIManager _uiManager;
    public UIManager UIManager => _uiManager;

    private SceneManager _sceneManager;
    public SceneManager SceneManager => _sceneManager;

    public List<IManager> managers;

    public UnityEvent OnGameStart;
    public UnityEvent OnSceneChanged;

    public override void OnInitialize()
    {
        managers = new List<IManager>();
        OnGameStart = new UnityEvent();

        Util.GetOrAddComponent<EventSystem>(gameObject);
        Util.GetOrAddComponent<StandaloneInputModule>(gameObject);

        _resManager = Util.GetOrAddComponent<ResManager>(gameObject);
        _luaManager = Util.GetOrAddComponent<LuaManager>(gameObject);
        _uiManager = Util.GetOrAddComponent<UIManager>(gameObject);
        _sceneManager = Util.GetOrAddComponent<SceneManager>(gameObject);
        managers.Add(_resManager);
        managers.Add(_luaManager);
        managers.Add(_uiManager);
        managers.Add(_sceneManager);
    }

    public void Run()
    {
        StartCoroutine(nameof(CoStart));
    }

    private IEnumerator CoStart()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].OnInitialize();
        }
        while (true)
        {
            var _IsAllInitialized = true;
            for (int i = 0; i < managers.Count; i++)
            {
                if (!managers[i].IsInitialized)
                {
                    _IsAllInitialized = false;
                }
            }
            if (_IsAllInitialized)
            {
                break;
            }
            yield return null;
        }

        OnGameStart.Invoke();
    }

    public override void OnRelease()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].OnRelease();
        }
    }

}
