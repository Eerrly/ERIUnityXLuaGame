using System.IO;
using System.Collections.Generic;
using System.Linq;
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
        battleEntity.DeltaTime = FrameEngine.FrameInterval * battleEntity.TimeScale;
    }

    public override void LogicUpdate()
    {
        try
        {
            if (!Paused)
            {
                Interlocked.Increment(ref BattleManager.Instance.LogicFrame);
                while (BattleManager.Instance.AsyncServerFrame.Count > 0)
                {
                    var frame = BattleManager.Instance.AsyncServerFrame.First();
                    UpdateInput(frame);
                    UpdatePlayerState(battleEntity);
                    battleEntity.Frame = frame;
                }
                RefreshBattleEntity(battleEntity);
            }
        }
        catch (System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
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
        entity.DeltaTime = FrameEngine.FrameInterval * entity.TimeScale;
        entity.Time += entity.DeltaTime;
    }

    /// <summary>
    /// 更新玩家操作
    /// </summary>
    private void UpdateInput(int frame) {
        var playerEntity = battleEntity.FindEntity(BattleManager.Instance.selfPlayerId);
        var inputFrame = FrameBuffer.Frame.defFrame;
        if (frameBuffer.TryGetFrame(frame, ref inputFrame))
        {
            BattleManager.Instance.AsyncServerFrame.Remove(frame);
            
            playerEntity.Input.yaw = inputFrame[playerEntity.ID].yaw - FixedMath.YawOffset;
            playerEntity.Input.key = inputFrame[playerEntity.ID].key;
            Logger.Log(LogLevel.Info, $"从缓存帧数据里取到对应帧的帧数据 [frame]->{frame} [yaw]->{playerEntity.Input.yaw} key:{playerEntity.Input.key}");
        }
        else
        {
            Logger.Log(LogLevel.Error, $"从缓存帧数据里无法取到对应帧的帧数据 [frame]->{frame}");
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
