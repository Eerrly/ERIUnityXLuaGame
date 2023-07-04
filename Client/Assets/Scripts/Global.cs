using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Global : Singleton<Global>
{

    private ResManager _resManager;
    /// <summary>
    /// 资源管理器
    /// </summary>
    public ResManager ResManager => _resManager;

    private LuaManager _luaManager;
    /// <summary>
    /// Lua管理器
    /// </summary>
    public LuaManager LuaManager => _luaManager;

    private UIManager _uiManager;
    /// <summary>
    /// UI管理器
    /// </summary>
    public UIManager UIManager => _uiManager;

    private SceneManager _sceneManager;
    /// <summary>
    /// 场景管理器
    /// </summary>
    public SceneManager SceneManager => _sceneManager;

    private HttpManager _httpManager;
    /// <summary>
    /// Http管理器
    /// </summary>
    public HttpManager HttpManager => _httpManager;

    private PatchingManager _patchingManager;
    /// <summary>
    /// 热更管理器
    /// </summary>
    public PatchingManager PatchingManager => _patchingManager;

    /// <summary>
    /// 管理器列表
    /// </summary>
    public List<IManager> Managers;

    public UnityEvent OnGameStart;
    public UnityEvent OnSceneChanged;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void OnInitialize()
    {
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Managers = new List<IManager>(10);
        OnGameStart = new UnityEvent();

        Util.GetOrAddComponent<EventSystem>(gameObject);
        Util.GetOrAddComponent<StandaloneInputModule>(gameObject);

        _resManager = Util.GetOrAddComponent<ResManager>(gameObject);
        _luaManager = Util.GetOrAddComponent<LuaManager>(gameObject);
        _uiManager = Util.GetOrAddComponent<UIManager>(gameObject);
        _sceneManager = Util.GetOrAddComponent<SceneManager>(gameObject);
        _httpManager = Util.GetOrAddComponent<HttpManager>(gameObject);
        _patchingManager = Util.GetOrAddComponent<PatchingManager>(gameObject);
        Managers.Add(_resManager);
        Managers.Add(_luaManager);
        Managers.Add(_uiManager);
        Managers.Add(_sceneManager);
        Managers.Add(_httpManager);
        Managers.Add(_patchingManager);

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
        for (int i = 0; i < Managers.Count; i++)
        {
            Managers[i].OnInitialize();
        }
        while (true)
        {
            var _IsAllInitialized = true;
            for (int i = 0; i < Managers.Count; i++)
            {
                if (!Managers[i].IsInitialized)
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
            Logger.SetLoggerLevel((int)LogLevel.Exception | (int)LogLevel.LuaException | (int)LogLevel.Error | (int)LogLevel.LuaError | (int)LogLevel.Info | (int)LogLevel.LuaInfo);
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
        for (int i = 0; i < Managers.Count; i++)
        {
            Managers[i].OnRelease();
        }
    }

}
