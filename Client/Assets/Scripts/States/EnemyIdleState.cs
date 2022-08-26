using System;

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
        enemyEntity.attack.targets = SectorSystem.GetAroundEntities(enemyEntity, EnemyPropertyConstant.atkMaxCount, true);
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        if(enemyEntity.attack.targets.Length > 0)
        {
            var entity = battleEntity.FindEntity(enemyEntity.attack.targets[0]);
            var target = MathManager.ToVector3(entity.transform.pos);
            var pos = MathManager.ToVector3(enemyEntity.transform.pos);
            var direction = target - pos;
            if (direction.sqrMagnitude > Math.Pow(EnemyPropertyConstant.CollisionRadius + PlayerPropertyConstant.CollisionRadius, 2))
            {
                EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Move);
            }
            else
            {
                EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.AttackReady);
            }
        }
        else
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Patrol);
        }
    }

}
