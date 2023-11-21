using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// 全局管理器总类
/// </summary>
public class Global : Singleton<Global>
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public ResManager ResManager { get; private set; }

    /// <summary>
    /// Lua管理器
    /// </summary>
    public LuaManager LuaManager { get; private set; }

    /// <summary>
    /// UI管理器
    /// </summary>
    public UIManager UIManager { get; private set; }

    /// <summary>
    /// 场景管理器
    /// </summary>
    public SceneManager SceneManager { get; private set; }

    /// <summary>
    /// Http管理器
    /// </summary>
    public HttpManager HttpManager { get; private set; }

    /// <summary>
    /// 热更管理器
    /// </summary>
    public PatchingManager PatchingManager { get; private set; }

    /// <summary>
    /// 管理器列表
    /// </summary>
    private List<IManager> _managers;

    [FormerlySerializedAs("OnPatchingDone")] public UnityEvent onPatchingDone;
    [FormerlySerializedAs("OnGameStart")] public UnityEvent onGameStart;
    [FormerlySerializedAs("OnSceneChanged")] public UnityEvent onSceneChanged;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void OnInitialize()
    {
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        _managers = new List<IManager>(10);
        onGameStart = new UnityEvent();
        onPatchingDone = new UnityEvent();
        onSceneChanged = new UnityEvent();

        Util.GetOrAddComponent<EventSystem>(gameObject);
        Util.GetOrAddComponent<StandaloneInputModule>(gameObject);

        ResManager = Util.GetOrAddComponent<ResManager>(gameObject);
        SceneManager = Util.GetOrAddComponent<SceneManager>(gameObject);
        UIManager = Util.GetOrAddComponent<UIManager>(gameObject);
        LuaManager = Util.GetOrAddComponent<LuaManager>(gameObject);
        HttpManager = Util.GetOrAddComponent<HttpManager>(gameObject);
        PatchingManager = Util.GetOrAddComponent<PatchingManager>(gameObject);
        _managers.Add(ResManager);
        _managers.Add(SceneManager);
        _managers.Add(UIManager);
        _managers.Add(LuaManager);
        _managers.Add(HttpManager);
        _managers.Add(PatchingManager);

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
    /// 关闭
    /// </summary>
    public void Shutdown()
    {
        OnRelease();
    }

    /// <summary>
    /// 初始化管理器列表
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoStart()
    {
        foreach (var t in _managers)
        {
            t.IsInitialized = false;
        }
        foreach (var t in _managers)
        {
            t.OnInitialize();
        }
        while (true)
        {
            var isAllInitialized = true;
            foreach (var t in _managers.Where(t => !t.IsInitialized))
            {
                isAllInitialized = false;
            }
            if (isAllInitialized)
            {
                break;
            }
            yield return null;
        }

        onGameStart.Invoke();
    }

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    private void InitializeDebugLogSystem()
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
#if UNITY_DEBUG
            Logger.Log(LogLevel.Info, "创建日志 - 时间：" + System.DateTime.Now.ToString(CultureInfo.InvariantCulture));
#endif
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
        for (int i = _managers.Count - 1; i >= 0; i--)
        {
            try
            {
                _managers[i].OnRelease();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    public void OnDestroy()
    {
        Shutdown();
    }

}
