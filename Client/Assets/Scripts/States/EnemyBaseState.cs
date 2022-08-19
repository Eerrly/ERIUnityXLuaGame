[EnemyState(EEnemyState.None)]
public class EnemyBaseState : BaseState<EnemyEntity>
{

    public EEnemyState StateId
    {
        get { return (EEnemyState)_stateId; }
        set { _stateId = (int)value; }
    }

    public override void Reset(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.state.curStateId = (int)StateId;
        enemyEntity.state.nextStateId = 0;
        enemyEntity.state.enterTime = battleEntity.time;
    }

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity) { }

    public override void OnUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity) { }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity) { }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity) { }

    public virtual void OnCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity) { }

}
