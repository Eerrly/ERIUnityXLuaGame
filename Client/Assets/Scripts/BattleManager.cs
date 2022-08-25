using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{

    private volatile bool _paused = false;

    public static int mainThreadId;
    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

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
            _playerInput.AddKey(new KeyCode() { _name = BattleConstant.buttonNames[i] });
        }
    }

    public FrameBuffer.Input GetInput()
    {
        FrameBuffer.Input input = _playerInput.GetPlayerInput();
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
        _battleView.InitView(_battleClientData);
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Initialize), false);
        _frameEngine.RegisterFrameUpdateListener(EngineUpdate);
        _frameEngine.StartEngine(1 / BattleConstant.FrameInterval);
        _battle.Initialize();
    }

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
