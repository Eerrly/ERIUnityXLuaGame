using System;
using UnityEngine;

#if UNITY_DEBUG
using System.Collections.Generic;
#endif

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
        GUI.Label(new Rect(20, index++ * height, 1024, height),
            string.Format("[player]\tid:{0}", playerEntity.ID));
        GUI.Label(new Rect(20, index++ * height, 1024, height), 
            string.Format("[input]\tyaw:{0}, key:{1}", playerEntity.input.yaw, playerEntity.input.key));
        GUI.Label(new Rect(20, index++ * height, 1024, height), 
            string.Format("[state]\t{0}", Enum.GetName(typeof(EPlayerState), playerEntity.curStateId)));
        GUI.Label(new Rect(20, index++ * height, 1024, height),
            string.Format("[anim]\tanimId:{0}, normalizedTime:{1}", Enum.GetName(typeof(EAnimationID), playerEntity.animation.animId), playerEntity.animation.normalizedTime));
        GUI.Label(new Rect(20, index++ * height, 1024, height),
            string.Format("[move]\tposition:{0}, rotation:{1}", MathManager.ToVector3(playerEntity.movement.position).ToString(), MathManager.ToQuaternion(playerEntity.movement.rotation).ToString()));
        
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
        string strCellInfo = "[cell]\t";
        for (int i = 0; i < aroundCellList.Count; i++)
        {
            if (aroundCellList[i].entities.Count == 1 && aroundCellList[i].entities[0].ID == selfPlayerId)
                continue;
            if (aroundCellList[i].entities.Count > 0)
            {
                strCellInfo += (aroundCellList[i].ToString() + " ");
            }
        }
        GUI.Label(new Rect(20, index++ * height, 1024, height), strCellInfo);
    }

    private void OnDrawGizmos()
    {
        if (_battle != null)
        {
            PlayerEntity playerEntity = (_battle as BattleController).battleEntity.selfPlayerEntity;
            List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
            for (int i = 0; i < aroundCellList.Count; i++)
            {
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawCube(aroundCellList[i].bounds.center, aroundCellList[i].bounds.size);
            }
        }

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
