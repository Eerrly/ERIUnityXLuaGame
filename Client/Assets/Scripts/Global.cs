using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Global : Singleton<Global>
{

    private ResManager _resManager;
    public ResManager ResManager => _resManager;

    private LuaManager _luaManager;
    public LuaManager LuaManager => _luaManager;

    public List<IManager> managers;
    public UnityEvent OnGameStart;

    public override void OnInitialize()
    {
        managers = new List<IManager>();
        OnGameStart = new UnityEvent();
    }

    private void Start()
    {
        _resManager = Util.GetOrAddComponent<ResManager>(gameObject);
        _luaManager = Util.GetOrAddComponent<LuaManager>(gameObject);
        managers.Add(_resManager);
        managers.Add(_luaManager);
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
