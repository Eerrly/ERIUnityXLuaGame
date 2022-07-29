using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    /// <summary>
    /// 是否暂停 (volatile 关键字指示一个字段可以由多个同时执行的线程修改)
    /// </summary>
    private volatile bool _paused = false;

    /// <summary>
    /// 主线程Id
    /// </summary>
    public static int mainThreadId;
    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

    /// <summary>
    /// 当前战斗控制器
    /// </summary>
    private IBattleController _battle;
    public IBattleController battle => _battle;

    private static BattleManager _instance = null;
    public static BattleManager Instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    private BattleView _battleView;
    public BattleView battleView
    {
        get { return _battleView; }
        private set { _battleView = value; }
    }

    private FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;

    private void Awake()
    {
        Instance = this;
        _battleView = Util.GetOrAddComponent<BattleView>(gameObject);
    }

    /// <summary>
    /// 设置战斗数据
    /// </summary>
    /// <param name="data"></param>
    public void SetBattleData(BattleCommonData data)
    {
        _battleClientData = data;
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle()
    {
        _battle = new BattleController(_battleClientData);
        _battleView.InitView(_battleClientData);
        _frameEngine.RegisterFrameUpdateListener(EngineUpdate);
        _frameEngine.StartEngine();
        _battle.Initialize();
    }

    /// <summary>
    /// 逻辑轮询
    /// </summary>
    private void EngineUpdate()
    {
        try
        {
            _battle.LogicUpdate();
            _battle.SwitchProceedingStatus(_paused);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
    private void Update()
    {
        RenderUpdate();
    }

    private void RenderUpdate()
    {
        try
        {
            _battle.RenderUpdate();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
