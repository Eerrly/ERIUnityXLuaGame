using System.IO;
using System.Collections.Generic;

public class BattleController : IBattleController
{
    public Dictionary<int, FrameBuffer.Input> frameInputs;
    public BattleEntity battleEntity { get; set; }

    private long _enterMilliseconds = 0;
    private long _lastMilliseconds = 0;

    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        battleEntity.Init();
        for (int i = 0; i < data.players.Length; i++)
        {
            BaseEntity entity = new PlayerEntity();
            entity.Init(data.players[i]);
            battleEntity.entities.Add(entity);
        }
    }

    public override void Initialize()
    {
        frameInputs = new Dictionary<int, FrameBuffer.Input>();
        battleEntity.deltaTime = FrameEngine.frameInterval * battleEntity.timeScale;
    }

    public override void LogicUpdate()
    {
        if(_enterMilliseconds == 0)
        {
            _enterMilliseconds = BattleManager.Instance.Time;
        }
        long startMillSecondes = BattleManager.Instance.Time - _enterMilliseconds;

        while(startMillSecondes - _lastMilliseconds >= BattleConstant.FrameInterval)
        {
            _lastMilliseconds += BattleConstant.FrameInterval;

            try
            {
                if (!Paused)
                {
                    battleEntity.deltaTime = FrameEngine.frameInterval * battleEntity.timeScale;
                    battleEntity.time += battleEntity.deltaTime;

                    var entities = battleEntity.entities;

                    BattleStateMachine.Instance.Update(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var playerEntity = (PlayerEntity)entities[i];
                        PlayerStateMachine.Instance.Update(playerEntity, battleEntity);
                    }

                    BattleStateMachine.Instance.LateUpdate(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var playerEntity = (PlayerEntity)entities[i];
                        PlayerStateMachine.Instance.LateUpdate(playerEntity, battleEntity);
                    }

                    PhysicsSystem.Update(battleEntity);

                    BattleStateMachine.Instance.DoChangeState(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var playerEntity = (PlayerEntity)entities[i];
                        PlayerStateMachine.Instance.DoChangeState(playerEntity, battleEntity);
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Log(LogLevel.Exception, e.Message);
            }
        }
        
    }

    public void UpdateInput(FrameBuffer.Input input) {
        var playerEntity = battleEntity.FindEntity(input.pos);
        playerEntity.input.yaw = input.yaw - MathManager.YawOffset;
        playerEntity.input.key = input.key;
    }

    public override void RenderUpdate()
    {
        try
        {
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
