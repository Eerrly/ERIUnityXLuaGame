using System;
using UnityEngine;
using System.Collections.Generic;

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

    private FrameEngine _frameEngine = new FrameEngine();

    private BattleCommonData _battleClientData;

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

    private void OnDestroy()
    {
        _frameEngine.UnRegisterFrameUpdateListener();
        _frameEngine.StopEngine();
    }

#if UNITY_DEBUG
    private int height = 20;
    private void OnGUI()
    {
        int index = 1;
        PlayerEntity playerEntity = (_battle as BattleController).battleEntity.selfPlayerEntity;
        GUI.Label(new Rect(20, index++ * height, 800, height),
            string.Format("[player]\tid:{0}", playerEntity.ID));
        GUI.Label(new Rect(20, index++ * height, 800, height), 
            string.Format("[input]\tyaw:{0}, key:{1}", playerEntity.input.yaw, playerEntity.input.key));
        GUI.Label(new Rect(20, index++ * height, 800, height), 
            string.Format("[cell]\t{0}", playerEntity.cell.ToString()));
        GUI.Label(new Rect(20, index++ * height, 800, height), 
            string.Format("[state]\t{0}", Enum.GetName(typeof(EPlayerState), playerEntity.curStateId)));
        GUI.Label(new Rect(20, index++ * height, 800, height),
            string.Format("[move]\tposition:{0}, rotation:{1}", MathManager.ToVector3(playerEntity.movement.position).ToString(), MathManager.ToQuaternion(playerEntity.movement.rotation).ToString()));
    }

    private void OnDrawGizmos()
    {
        List<Cell> cellList = SpacePartition.GetCellList();
        if (cellList == null)
        {
            return;
        }
        for (int i = 0; i < cellList.Count; i++)
        {
            Cell cell = cellList[i];
            if (cell.entities.Count > 0)
            {
                Gizmos.color = new Color(0, 1f, 0, 0.2f);
                Gizmos.DrawCube(cell.bounds.center, cell.bounds.size);
                for (int j = 0; j < cell.entities.Count; j++)
                {
                    if(cell.entities[j].ID == selfPlayerId)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.2f);
                        Gizmos.DrawCube(cell.bounds.center, cell.bounds.size * 3);
                        break;
                    }
                }
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(cell.bounds.center, cell.bounds.size);
            }
        }
    }
#endif

}
