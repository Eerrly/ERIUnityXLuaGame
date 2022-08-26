[EnemyState(EEnemyState.Attack)]
public class EnemyAttackState : EnemyBaseState
{

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = false;
        AnimationSystem.ChangePlayerAnimation(enemyEntity, EAnimationID.Attack);
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        var entities = SectorSystem.GetWithinRangeOfTheAttack(enemyEntity);
        if (AnimationSystem.CheckAnimationNormalizedTime(enemyEntity, 0.65f))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if(entities[i].property.hp > 0)
                {
                    AttackSystem.Attack(enemyEntity, entities[i]);
                }
            }
            EntityStateSystem.ChangeEntityState(enemyEntity, EPlayerState.Idle);
        }
        if (AnimationSystem.CheckAnimationNormalizedTime(enemyEntity))
        {
            var hasTarget = false;
            for (int i = 0; i < entities.Count; i++)
            {
                if(entities[i].property.hp > 0)
                {
                    hasTarget = true;
                    break;
                }
            }
            if (!hasTarget)
            {
                EntityStateSystem.ChangeEntityState(enemyEntity, EPlayerState.Idle);
            }
        }
    }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.lastAttackTime = battleEntity.time;
    }

}
