public class BattleController : IBattleController
{
    public int nextFrame;

    public BattleEntity battleEntity { get; set; }

    private long _enterMilliseconds = 0;
    private long _lastMilliseconds = 0;

    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        battleEntity.Init();
        for (int i = 0; i < data.players.Length; i++)
        {
            BaseEntity entity;
            if (data.players[i].camp == 1)
                entity = new PlayerEntity();
            else
                entity = new EnemyEntity();

            entity.Init(data.players[i]);
            battleEntity.entities.Add(entity);
        }
    }

    public override void Initialize()
    {
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

                    UpdateInput();

                    var entities = battleEntity.entities;

                    BattleStateMachine.Instance.Update(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (entities[i].property.camp == ECamp.Alliance)
                        {
                            var playerEntity = (PlayerEntity)entities[i];
                            PlayerStateMachine.Instance.Update(playerEntity, battleEntity);
                        }
                        else
                        {
                            var enemyEntity = (EnemyEntity)entities[i];
                            EnemyStateMachine.Instance.Update(enemyEntity, battleEntity);
                        }
                    }

                    BattleStateMachine.Instance.LateUpdate(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (entities[i].property.camp == ECamp.Alliance)
                        {
                            var playerEntity = (PlayerEntity)entities[i];
                            PlayerStateMachine.Instance.LateUpdate(playerEntity, battleEntity);
                        }
                        else
                        {
                            var enemyEntity = (EnemyEntity)entities[i];
                            EnemyStateMachine.Instance.LateUpdate(enemyEntity, battleEntity);
                        }
                    }

                    PhysicsSystem.Update(battleEntity);

                    BattleStateMachine.Instance.DoChangeState(battleEntity, null);
                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (entities[i].property.camp == ECamp.Alliance)
                        {
                            var playerEntity = (PlayerEntity)entities[i];
                            PlayerStateMachine.Instance.DoChangeState(playerEntity, battleEntity);
                        }
                        else
                        {
                            var enemyEntity = (EnemyEntity)entities[i];
                            EnemyStateMachine.Instance.DoChangeState(enemyEntity, battleEntity);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        
    }

    public void UpdateInput() {
        var input = BattleManager.Instance.GetInput();
        battleEntity.selfPlayerEntity.input.yaw = input.yaw - MathManager.YawOffset;
        battleEntity.selfPlayerEntity.input.key = input.key;
    }

    public override void RenderUpdate()
    {
        try
        {
            BattleManager.Instance.battleView.RenderUpdate(battleEntity);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public override void Release() { }

    public override void GameOver() { }

}
