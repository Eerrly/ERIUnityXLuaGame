using System.IO;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 战斗管理器
/// </summary>
public class BattleController : IBattleController
{
    public Dictionary<int, FrameBuffer.Input> FrameInputs;
    public BattleEntity battleEntity { get; set; }
    public FrameBuffer frameBuffer { get; private set; }

    private long _enterMilliseconds = 0;
    private long _lastMilliseconds = 0;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">玩家数据</param>
    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        battleEntity.Init();
        foreach (var t in data.players)
        {
            BaseEntity entity = new PlayerEntity();
            entity.Init(t);
            battleEntity.Entities.Add(entity);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        FrameInputs = new Dictionary<int, FrameBuffer.Input>();
        frameBuffer = new FrameBuffer(2);
        battleEntity.DeltaTime = FrameEngine.frameInterval * battleEntity.TimeScale;
    }

    public override void LogicUpdate()
    {
        if(_enterMilliseconds == 0) _enterMilliseconds = BattleManager.Instance.Time;

        var startMillSeconds = BattleManager.Instance.Time - _enterMilliseconds;
        while(startMillSeconds - _lastMilliseconds >= BattleConstant.FrameInterval)
        {
            _lastMilliseconds += BattleConstant.FrameInterval;
            try
            {
                if (!Paused)
                {
                    Interlocked.Increment(ref BattleManager.Instance.LogicFrame);
                    UpdateInput();
                    RefreshBattleEntity(battleEntity);
                    UpdatePlayerState(battleEntity);
                }
            }
            catch (System.Exception e)
            {
                Logger.Log(LogLevel.Exception, e.Message);
            }
        }
        
    }

    private void UpdatePlayerState(BattleEntity entity)
    {
        var entities = entity.Entities;

        BattleStateMachine.Instance.Update(entity, null);
        foreach (var t in entities)
        {
            var playerEntity = (PlayerEntity)t;
            PlayerStateMachine.Instance.Update(playerEntity, entity);
        }

        BattleStateMachine.Instance.LateUpdate(entity, null);
        foreach (var t in entities)
        {
            var playerEntity = (PlayerEntity)t;
            PlayerStateMachine.Instance.LateUpdate(playerEntity, entity);
        }

        PhysicsSystem.Update(entity);

        BattleStateMachine.Instance.DoChangeState(entity, null);
        foreach (var t in entities)
        {
            var playerEntity = (PlayerEntity)t;
            PlayerStateMachine.Instance.DoChangeState(playerEntity, entity);
        }
    }

    private void RefreshBattleEntity(BattleEntity entity)
    {
        entity.Frame += 1;
        entity.DeltaTime = FrameEngine.frameInterval * entity.TimeScale;
        entity.Time += entity.DeltaTime;
    }

    /// <summary>
    /// 更新玩家操作
    /// </summary>
    private void UpdateInput() {
        var playerEntity = battleEntity.FindEntity(BattleManager.Instance.selfPlayerId);
        var inputFrame = FrameBuffer.Frame.defFrame;
        if (frameBuffer.TryGetFrame(battleEntity.Frame, ref inputFrame))
        {
            playerEntity.Input.yaw = inputFrame[playerEntity.ID].yaw - MathManager.YawOffset;
            playerEntity.Input.key = inputFrame[playerEntity.ID].key;
        }
        else
        {
            Logger.Log(LogLevel.Error, $"【从缓存帧数据里无法取到对应帧的帧数据】frame:{battleEntity.Frame}");
        }
    }

    public override void RenderUpdate()
    {
        try
        {
            Interlocked.Increment(ref BattleManager.Instance.RenderFrame);
            BattleManager.Instance.battleView.RenderUpdate(battleEntity);
        }
        catch (System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public override void Release() { }

    public override void GameOver() { }

}
