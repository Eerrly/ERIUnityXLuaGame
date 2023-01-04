using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private volatile bool _paused = false;

    public static int mainThreadId;
    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

    private IBattleController _battle;
    public IBattleController battle => _battle;

    private BattleNetworkController _battleNetController;
    public BattleNetworkController battleNetController => _battleNetController;

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

    private long _time;
    public long Time => _time;

    private FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;

    private bool _battleStarted = false;

    public int selfPlayerId { get; set; }

    private byte[] recvBuffer;
    private MemoryStream _receiveStream = new MemoryStream(256);
    private BinaryReader _binaryReader;

    public volatile int renderFrame = -1;
    public volatile int logicFrame = -1;
    private FrameBuffer.Input _lastSendPlayerInput;
    private FrameBuffer.Input _lastRecvPlayerInput = new FrameBuffer.Input(0);

    private void Awake()
    {
        Instance = this;
        _battleView = Util.GetOrAddComponent<BattleView>(gameObject);
        _playerInput = Util.GetOrAddComponent<PlayerInput>(gameObject);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
    }

    public void Initialize()
    {
        for (int i = 0; i < BattleConstant.buttonNames.Length; i++)
        {
            _playerInput.AddKey(new InputKeyCode() { _name = BattleConstant.buttonNames[i] });
        }
        recvBuffer = new byte[9];
        _binaryReader = new BinaryReader(_receiveStream);
    }

    public FrameBuffer.Input GetInput()
    {
        FrameBuffer.Input input = _playerInput.GetPlayerInput(selfPlayerId);
        return input;
    }

    public void SetBattleData(BattleCommonData data)
    {
        _battleClientData = data;
    }

    public void StartBattle(int selfPlayerId)
    {
        this._battleStarted = true;
        this.selfPlayerId = selfPlayerId;
        _battle = new BattleController(_battleClientData);
        _battleNetController = new BattleNetworkController();
        _battleView.InitView(_battleClientData);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
        _frameEngine.RegisterFrameUpdateListener(EngineUpdate);
        _frameEngine.RegisterNetUpdateListener(NetUpdate);
        _frameEngine.StartEngine(1 / BattleConstant.FrameInterval);
        _battle.Initialize();
        _battleNetController.Initialize();
        _battleNetController.Connect("127.0.0.1", 10086);
    }

    private void EngineUpdate()
    {
        try
        {
            logicFrame = logicFrame + 1;
            _battle.LogicUpdate();
            _battle.SwitchProceedingStatus(_paused);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    private void NetUpdate()
    {
        try
        {
            if (_battleNetController.IsConnected)
            {
                FrameBuffer.Input input = BattleManager.Instance.GetInput();
                if (!input.Compare(_lastSendPlayerInput))
                {
                    _lastSendPlayerInput = input;
                    _battleNetController.SendInputToServer(input.pos, logicFrame, input);
                }
                _battleNetController.Update();
                if(_battleNetController.RecvData(ref recvBuffer, 0, recvBuffer.Length) > 0)
                {
                    HandleRecvData(recvBuffer, 0, recvBuffer.Length);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        System.Threading.Thread.Sleep(1);
    }

    private void HandleRecvData(byte[] data, int offset, int length)
    {
        _receiveStream.Reset();
        if (null != data)
        {
            _receiveStream.Write(data, offset, length);
        }
        _receiveStream.Seek(0, SeekOrigin.Begin);
        while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length)
        {
            int playerId = _binaryReader.ReadInt32();
            int frame = _binaryReader.ReadInt32();
            byte raw = _binaryReader.ReadByte();
#if UNITY_DEBUG
            Logger.Log(LogLevel.Info, "RecvData playerId:" + playerId + ", frame:" + frame + ", raw:" + raw);
#endif
            _lastRecvPlayerInput.pos = (byte)playerId;
            _lastRecvPlayerInput.yaw = (byte)((0xF0 & raw) >> 4);
            _lastRecvPlayerInput.key = (byte)((0x0E & raw) >> 1);
            ((BattleController)_battle).UpdateInput(_lastRecvPlayerInput);
        }
    }

    private void Update()
    {
        if (_battleStarted)
        {
            _time = (int)(UnityEngine.Time.time * 1000);
            RenderUpdate();
        }
    }

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
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.UnRegisterNetUpdateListener();
        _frameEngine.StopEngine();
    }

}
