[EnemyState(EEnemyState.AttackReady)]
public class EnemyAttackReadyState : EnemyBaseState
{

    public override bool TryEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        return AttackSystem.CheckAttackCdTime(enemyEntity, battleEntity);
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Attack);
    }

}
