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

    public static int MainThreadId;
    /// <summary>
    /// 是否为主线程
    /// </summary>
    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadId;

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

    private bool _battleStarted = false;

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
        foreach (var t in BattleConstant.buttonNames)
        {
            playerInput.AddKey(new InputKeyCode() { _name = t });
        }
        _recvBuffer = new byte[6];
        _binaryReader = new BinaryReader(_receiveStream);
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
        this._battleStarted = false;
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
        battleNetController.Connect(NetConstant.IP, NetConstant.Port);
    }

    /// <summary>
    /// 逻辑线程轮询
    /// </summary>
    private void EngineUpdate()
    {
        try
        {
            if (!_battleStarted) return;
            
            battle.LogicUpdate();
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
                if (!_battleStarted)
                {
                    // 加入
                    battleNetController.SendReadyMsg();
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
                        battleNetController.SendHeartBeatMsg();
                    }
                }
                battleNetController.Update();

                // 接受
                _recvQueue = battleNetController.RecvData();
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
            battle.battleEntity.Frame = 0;
        }

        _receiveStream.Reset();
        _receiveStream.Write(packet.data, 0, packet.head.size);
        _receiveStream.Seek(0, SeekOrigin.Begin);

        BufferPool.ReleaseBuff(packet.data);
        while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length)
        {
            var inputFrame = FrameBuffer.Frame.defFrame;
            inputFrame.frame = _binaryReader.ReadInt32();
            inputFrame.playerCount = _binaryReader.ReadInt32();
            for (var i = 0; i < inputFrame.playerCount; i++)
            {
                var input = new FrameBuffer.Input(_binaryReader.ReadByte());
                inputFrame[input.pos] = input;
            }

            var diff = 0;
            while (!battle.frameBuffer.SyncFrame(inputFrame.frame, ref inputFrame, ref diff))
            {
                battleNetController.DisConnect();
                break;
            }
            Logger.Log(LogLevel.Info, $"【客户端同步帧数据】frame:{inputFrame.frame}");
        }
    }

    /// <summary>
    /// 主线程轮询
    /// </summary>
    private void Update()
    {
        if (!_battleStarted) return;
        
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
        battleNetController.DisConnect();
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.UnRegisterNetUpdateListener();
        _frameEngine.StopEngine();
    }

}
