using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 战斗管理器
/// </summary>
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

    /// <summary>
    /// 战斗渲染类
    /// </summary>
    private BattleView _battleView;
    public BattleView battleView
    {
        get { return _battleView; }
        private set { _battleView = value; }
    }

    /// <summary>
    /// 玩家输入类
    /// </summary>
    private PlayerInput _playerInput;
    public PlayerInput playerInput
    {
        get { return _playerInput; }
        private set { _playerInput = value; }
    }

    private FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;

    private void Awake()
    {
        Instance = this;
        _battleView = Util.GetOrAddComponent<BattleView>(gameObject);
        _playerInput = Util.GetOrAddComponent<PlayerInput>(gameObject);
    }

    public void Initialize()
    {
        for (int i = 0; i < BattleConstant.buttonNames.Length; i++)
        {
            _playerInput.AddKey(new KeyCode() { _name = BattleConstant.buttonNames[i] });
        }
    }

    /// <summary>
    /// 获取玩家输入操作
    /// </summary>
    /// <returns></returns>
    public FrameBuffer.Input GetInput()
    {
        FrameBuffer.Input input = _playerInput.GetPlayerInput();
        return input;
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

    private void Update()
    {
        RenderUpdate();
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
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

    private void OnDestroy()
    {
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.StopEngine();
    }

}
