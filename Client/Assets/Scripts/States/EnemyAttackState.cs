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
        if (AnimationSystem.CheckAnimationNormalizedTime(enemyEntity))
        {
            BaseEntity targetEntity = null;
            for (int i = 0; i < entities.Count; i++)
            {
                if(entities[i].property.hp > 0)
                {
                    targetEntity = entities[i];
                    break;
                }
            }
            if(targetEntity != null)
            {
                AttackSystem.Attack(enemyEntity, targetEntity);
            }
            EntityStateSystem.ChangeEntityState(enemyEntity, EPlayerState.Idle);
        }
    }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.lastAttackTime = battleEntity.time;
    }

}
