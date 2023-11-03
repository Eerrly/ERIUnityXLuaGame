using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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

    public static int mainThreadId;
    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

    private BattleController _battle;
    public BattleController battle => _battle;

    private BattleNetController _battleNetController;
    public BattleNetController battleNetController => _battleNetController;

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

    private PlayerInput _playerInput;
    public PlayerInput playerInput
    {
        get { return _playerInput; }
        private set { _playerInput = value; }
    }

    private CameraControl _cameraControl;
    public CameraControl cameraControl
    {
        get { return _cameraControl; }
        private set { _cameraControl = value; }
    }

    private long _time;
    public long Time => _time;

    private FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;

    private bool _battleStarted = false;

    public int selfPlayerId { get; set; }

    private byte[] recvBuffer;
    private Queue<Packet> _recvQueue;
    private MemoryStream _receiveStream = new MemoryStream(256);
    private BinaryReader _binaryReader;

    private byte _raw;
    private int _frame;
    private const byte posMask = 0x01;
    private const byte yawMask = 0xF0;
    private const byte keyMask = 0x0E;

    [System.NonSerialized] public volatile int renderFrame = -1;
    [System.NonSerialized] public volatile int logicFrame = -1;
    private FrameBuffer.Input _input;
    private FrameBuffer.Input _lastSendPlayerInput;
    private FrameBuffer.Input _lastRecvPlayerInput = new FrameBuffer.Input(0);
    private int _realSentFrame = 0;
    private int _heartBeatFrame = 0;

    private void Awake()
    {
        Instance = this;
        _cameraControl = Util.GetOrAddComponent<CameraControl>(Camera.main.transform.parent.gameObject);
        _battleView = Util.GetOrAddComponent<BattleView>(gameObject);
        _playerInput = Util.GetOrAddComponent<PlayerInput>(gameObject);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        for (int i = 0; i < BattleConstant.buttonNames.Length; i++)
        {
            _playerInput.AddKey(new InputKeyCode() { _name = BattleConstant.buttonNames[i] });
        }
        recvBuffer = new byte[6];
        _binaryReader = new BinaryReader(_receiveStream);
    }

    /// <summary>
    /// 获取输入
    /// </summary>
    /// <returns></returns>
    public FrameBuffer.Input GetInput()
    {
        FrameBuffer.Input input = _playerInput.GetPlayerInput(selfPlayerId);
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
    /// <param name="selfPlayerId"></param>
    public void StartBattle(int selfPlayerId)
    {
        this._battleStarted = false;
        this.selfPlayerId = selfPlayerId;
        BufferPool.InitPool(32, 1024, 5, 5);
        _battle = new BattleController(_battleClientData);
        _battleNetController = new BattleNetController();
        _battleView.InitView(_battleClientData);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
        _frameEngine.RegisterFrameUpdateListener(EngineUpdate);
        _frameEngine.RegisterNetUpdateListener(NetUpdate);
        _frameEngine.StartEngine(1 / (float)BattleConstant.FrameInterval);
        _battle.Initialize();
        _battleNetController.Initialize();
        _battleNetController.Connect(NetConstant.IP, NetConstant.Port);
    }

    /// <summary>
    /// 逻辑线程轮询
    /// </summary>
    private void EngineUpdate()
    {
        try
        {
            if (_battleStarted)
            {
                _battle.LogicUpdate();
                _battle.SwitchProceedingStatus(_paused);
            }
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
            if (_battleNetController.IsConnected)
            {
                if (!_battleStarted)
                {
                    // 加入
                    _battleNetController.SendReadyMsg();
                }
                else
                {
                    _input = GetInput();
                    // 输入
                    if (_realSentFrame != _battle.battleEntity.frame && !_input.Compare(_lastSendPlayerInput))
                    {
                        _lastSendPlayerInput = _input;
                        _realSentFrame = _battle.battleEntity.frame;
                        _battleNetController.SendInputMsg(_battle.battleEntity.frame, _input);
                    }
                    // 心跳
                    if (_battle.battleEntity.frame - _heartBeatFrame >= BattleConstant.HeartBeatFrame)
                    {
                        _heartBeatFrame = _battle.battleEntity.frame;
                        _battleNetController.SendHeartBeatMsg();
                    }
                }
                _battleNetController.Update();

                // 接受
                _recvQueue = _battleNetController.RecvData();
                while(_recvQueue.Count > 0)
                {
                    lock (_recvQueue)
                    {
                        HandleRecvData(_recvQueue.Dequeue());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
            _battleStarted = false;
        }
        System.Threading.Thread.Sleep(1);
    }

    /// <summary>
    /// 接受服务器数据回调
    /// </summary>
    /// <param name="packet">包体</param>
    private void HandleRecvData(Packet packet)
    {
        if(packet.head.act == (byte)ACT.JOIN)
        {
            _battleStarted = true;
#if UNITY_DEBUG
            Logger.Log(LogLevel.Info, $"【服务器返回数据】 行为: {Enum.GetName(typeof(ACT), packet.head.act)} 序号: {packet.head.index} 玩家ID:{_lastRecvPlayerInput.pos}");
#endif
            BufferPool.ReleaseBuff(packet.data);
            return;
        }

        _receiveStream.Reset();
        _receiveStream.Write(packet.data, 0, packet.head.size);
        _receiveStream.Seek(0, SeekOrigin.Begin);

        BufferPool.ReleaseBuff(packet.data);
        while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length)
        {
            _raw = _binaryReader.ReadByte();
            _frame = _binaryReader.ReadInt32();

            _lastRecvPlayerInput.pos = (byte)(posMask & _raw);
            _lastRecvPlayerInput.yaw = (byte)((yawMask & _raw) >> 4);
            _lastRecvPlayerInput.key = (byte)((keyMask & _raw) >> 1);
#if UNITY_DEBUG
            Logger.Log(LogLevel.Info, $"【服务器返回数据】 行为: {Enum.GetName(typeof(ACT), packet.head.act)} 序号: {packet.head.index} 玩家ID:{_lastRecvPlayerInput.pos} 帧:{_frame} 操作:{_raw}");
#endif
            _battle.UpdateInput(_lastRecvPlayerInput);
        }
    }

    /// <summary>
    /// 主线程轮询
    /// </summary>
    private void Update()
    {
        if (_battleStarted)
        {
            _time = (int)(UnityEngine.Time.time * 1000);
            RenderUpdate();
        }
    }

    /// <summary>
    /// 渲染主线程轮询
    /// </summary>
    private void RenderUpdate()
    {
        try
        {
            renderFrame = renderFrame + 1;
            _battle.RenderUpdate();
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    private void OnDestroy()
    {
        _battleNetController.DisConnect();
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.UnRegisterNetUpdateListener();
        _frameEngine.StopEngine();
    }

}
