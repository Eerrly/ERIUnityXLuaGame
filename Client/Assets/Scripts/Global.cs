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

    private PatchingManager _patchingManager;
    public PatchingManager PatchingManager => _patchingManager;

    public List<IManager> managers;

    public UnityEvent OnGameStart;
    public UnityEvent OnSceneChanged;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void OnInitialize()
    {
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        managers = new List<IManager>();
        OnGameStart = new UnityEvent();

        Util.GetOrAddComponent<EventSystem>(gameObject);
        Util.GetOrAddComponent<StandaloneInputModule>(gameObject);

        _resManager = Util.GetOrAddComponent<ResManager>(gameObject);
        _luaManager = Util.GetOrAddComponent<LuaManager>(gameObject);
        _uiManager = Util.GetOrAddComponent<UIManager>(gameObject);
        _sceneManager = Util.GetOrAddComponent<SceneManager>(gameObject);
        _patchingManager = Util.GetOrAddComponent<PatchingManager>(gameObject);
        managers.Add(_resManager);
        managers.Add(_luaManager);
        managers.Add(_uiManager);
        managers.Add(_sceneManager);
        managers.Add(_patchingManager);

        InitializeDebugLogSystem();
    }

    /// <summary>
    /// 运行
    /// </summary>
    public void Run()
    {
        StartCoroutine(nameof(CoStart));
    }

    /// <summary>
    /// 初始化管理器列表
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    public void InitializeDebugLogSystem()
    {
        try
        {
            var currentLoggerPath = FileUtil.CombinePaths(Application.persistentDataPath, "game.log");

            Logger.Initialize(currentLoggerPath, new Logger());
            Logger.SetLoggerLevel((int)LogLevel.Exception | (int)LogLevel.LuaException | (int)LogLevel.LuaError | (int)LogLevel.Info | (int)LogLevel.LuaInfo);
            Debug.unityLogger.logEnabled = true;

            Logger.log = Debug.Log;
            Logger.logError = Debug.LogError;
            Logger.logWarning = Debug.LogWarning;

            var console = transform.Find("__Console__");

            if (console == null)
            {
                var prefab = Resources.Load<GameObject>("Console");

                console = GameObject.Instantiate(prefab).transform;
                console.name = "__Console__";
                console.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                console.SetParent(transform);
            }
            console.gameObject.SetActive(true);

            Logger.Log(LogLevel.Info, "create log at " + System.DateTime.Now.ToString());
        }
        catch(System.Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    /// <summary>
    /// 释放
    /// </summary>
    public override void OnRelease()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].OnRelease();
        }
    }

}
