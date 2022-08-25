[EnemyState(EEnemyState.Idle)]
public class EnemyIdleState : EnemyBaseState
{
    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = true;
        enemyEntity.animation.fixedTransitionDuration = 0.1f;
        AnimationSystem.ChangePlayerAnimation(enemyEntity, EAnimationID.Idle);
    }

    public override void OnUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.targets = SectorSystem.GetAroundEntities(enemyEntity, EnemyPropertyConstant.atkMaxCount);
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        if(enemyEntity.attack.targets.Length > 0)
        {
            //EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Move);
        }
        else
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Patrol);
        }
    }

}
