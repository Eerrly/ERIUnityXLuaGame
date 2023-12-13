using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using KCPNet;

/// <summary>
/// MemoryStream的扩展类
/// 注意：类与方法都必须是静态，第一个参数必须传递"this System.IO.MemoryStream stream"
/// </summary>
public static class MemoryStreamEx
{
    public static void Reset(this System.IO.MemoryStream stream)
    {
        stream.Position = 0;
        stream.SetLength(0);
    }
}

public class BattleManager : MonoBehaviour
{
    private volatile bool _paused = false;

    public BattleController battle { get; private set; }

    private BattleNetController battleNetController { get; set; }

    public static BattleManager Instance { get; private set; } = null;

    public BattleView battleView { get; private set; }

    private PlayerInput playerInput { get; set; }

    /// <summary>
    /// 相机控制类
    /// </summary>
    public CameraControl cameraControl { get; private set; }

    public long Time { get; private set; }

    private readonly FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;
    public BattleCommonData BattleClientData => _battleClientData;

    public bool battleStarted = false;

    /// <summary>
    /// 自己的ID
    /// </summary>
    public int selfPlayerId { get; private set; }

    private byte[] _recvBuffer;
    private Queue<Packet> _recvQueue;
    private readonly MemoryStream _receiveStream = new MemoryStream(256);
    private BinaryReader _binaryReader;

    private byte _raw;
    private int _frame;
    private const byte PosMask = 0x01;
    private const byte YawMask = 0xF0;
    private const byte KeyMask = 0x0E;

    [System.NonSerialized] public volatile int RenderFrame = -1;
    [System.NonSerialized] public volatile int LogicFrame = -1;
    private FrameBuffer.Input _input;
    private FrameBuffer.Input _lastSendPlayerInput;
    private FrameBuffer.Input _lastRecvPlayerInput = new FrameBuffer.Input(0);
    private int _realSentFrame = 0;
    private int _heartBeatFrame = 0;
    
    /// <summary>
    /// 从第0帧接到服务器数据开始，客户端流逝时间的一个计时器
    /// </summary>
    private Stopwatch _stopWatch = new Stopwatch();
    /// <summary>
    /// 客户端流逝时间与最后一次接受服务器的帧数据的时间差
    /// </summary>
    private long _clientServerOffsetTime = 0;
    /// <summary>
    /// 上一次进行逻辑更新的客户端流逝时间
    /// </summary>
    private long _lastMilliseconds = 0;

    public List<int> AsyncServerFrame = new List<int>();

    private void Awake()
    {
        Instance = this;
        if (Camera.main != null) cameraControl = Util.GetOrAddComponent<CameraControl>(Camera.main.transform.parent.gameObject);
        battleView = Util.GetOrAddComponent<BattleView>(gameObject);
        playerInput = Util.GetOrAddComponent<PlayerInput>(gameObject);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        foreach (var t in BattleConstant.ButtonNames)
        {
            playerInput.AddKey(new InputKeyCode() { _name = t });
        }
        _recvBuffer = new byte[6];
        _binaryReader = new BinaryReader(_receiveStream);
    }

    /// <summary>
    /// 同步更新客户端与服务器的时间差
    /// </summary>
    /// <param name="frame">接收到的服务器帧数据的帧号</param>
    public void SyncClientServerOffsetTime(int frame)
    {
        if (frame == 0)
        {
            _stopWatch.Start();
            _clientServerOffsetTime = 0;
        }

        var tmpClientServerOffsetTime = _stopWatch.ElapsedMilliseconds - frame * BattleConstant.FrameInterval;
        // if(tmpClientServerOffsetTime < _clientServerOffsetTime)
        {
            _clientServerOffsetTime = tmpClientServerOffsetTime;
        }
    }

    /// <summary>
    /// 获取输入
    /// </summary>
    /// <returns></returns>
    private FrameBuffer.Input GetInput()
    {
        var input = playerInput.GetPlayerInput(selfPlayerId);
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
    /// <param name="playerId"></param>
    public void StartBattle(int playerId)
    {
        this.battleStarted = false;
        this.selfPlayerId = playerId;
        BufferPool.InitPool(32, 1024, 5, 5);
        battle = new BattleController(_battleClientData);
        battleNetController = new BattleNetController();
        battleView.InitView(_battleClientData);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
        _frameEngine.RegisterFrameUpdateListener(EngineUpdate);
        _frameEngine.RegisterNetUpdateListener(NetUpdate);
        _frameEngine.StartEngine(1 / (float)BattleConstant.FrameInterval);
        battle.Initialize();
        battleNetController.Initialize();
        battleNetController.DoConnect(NetConstant.IP, NetConstant.Port);
    }

    /// <summary>
    /// 逻辑线程轮询
    /// </summary>
    private void EngineUpdate()
    {
        try
        {
            battleNetController.TryRecivePackages();
            if (!battleStarted) return;

            var nextFrame = battle.battleEntity.Frame + 1;
            var serverTime = _stopWatch.ElapsedMilliseconds - _clientServerOffsetTime + battleNetController.MinPing * 0.5f;
            // 如果服务器时间 + 半个ping时间 + 魔法数字 >= 如果大于等于下一帧的时间，就说明理论上应该接受到服务器的新帧了，更新一帧
            var hasNewFrame = serverTime + battleNetController.Ping * 0.5f + 4 >= nextFrame * BattleConstant.FrameInterval;
            // 客户端流逝时间 - 上一帧执行时候的流逝时间 >= 两帧时间 也更新。这一般是比较卡的时候
            var slowdown = _stopWatch.ElapsedMilliseconds - _lastMilliseconds >= BattleConstant.FrameInterval * 2f;

            if (hasNewFrame || slowdown)
            {
                Logger.Log(LogLevel.Info, $"逻辑轮询 " +
                                          $"nextFrame:{nextFrame} " +
                                          $"clientTime:{_stopWatch.ElapsedMilliseconds} " +
                                          $"serverTime:{serverTime}" +
                                          $"MinPing:{battleNetController.MinPing}" +
                                          $"Ping:{battleNetController.Ping}" +
                                          $"_clientServerOffsetTime:{_clientServerOffsetTime} " +
                                          $"hasNewFrame:{hasNewFrame} slowdown:{slowdown}");
                _lastMilliseconds = _stopWatch.ElapsedMilliseconds;
                battle.LogicUpdate();
            }
            battle.SwitchProceedingStatus(_paused);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    /// <summary>
    /// 网络线程轮询
    /// </summary>
    private void NetUpdate()
    {
        try
        {
            if (battleNetController.IsConnected)
            {
                if (!battleStarted)
                {
                    // 加入
                    battleNetController.SendReadyMsg(battle.battleEntity.Frame, (byte)selfPlayerId);
                }
                else
                {
                    _input = GetInput();
                    // 输入
                    if (_realSentFrame != battle.battleEntity.Frame && !_input.Compare(_lastSendPlayerInput))
                    {
                        _lastSendPlayerInput = _input;
                        _realSentFrame = battle.battleEntity.Frame + 1;
                        battleNetController.SendInputMsg(battle.battleEntity.Frame, _input);
                    }
                    // 心跳
                    if (battle.battleEntity.Frame - _heartBeatFrame >= BattleConstant.HeartBeatFrame)
                    {
                        _heartBeatFrame = battle.battleEntity.Frame;
                        battleNetController.SendPingMsg();
                    }
                }
                battleNetController.TryUpdate();
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
            battleStarted = false;
        }
        System.Threading.Thread.Sleep(1);
    }

    /// <summary>
    /// 主线程轮询
    /// </summary>
    private void Update()
    {
        if (!battleStarted) return;
        
        Time = (int)(UnityEngine.Time.time * 1000);
        RenderUpdate();
    }

    /// <summary>
    /// 渲染主线程轮询
    /// </summary>
    private void RenderUpdate()
    {
        try
        {
            RenderFrame = RenderFrame + 1;
            battle.RenderUpdate();
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    private void OnDestroy()
    {
        battleNetController.DoDisconnect();
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.UnRegisterNetUpdateListener();
        _frameEngine.StopEngine();
    }

}
