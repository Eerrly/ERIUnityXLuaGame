using System;

[EnemyState(EEnemyState.Patrol)]
public class EnemyPatrolState : EnemyBaseState
{

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = true;
        enemyEntity.animation.fixedTransitionDuration = 0.1f;
        enemyEntity.movement.moveSpeed = EnemyPropertyConstant.MoveSpeed;
        enemyEntity.movement.turnSpeed = EnemyPropertyConstant.TurnSpeed;
        AnimationSystem.ChangePlayerAnimation(enemyEntity, EAnimationID.Run);

        enemyEntity.movement.target = RandomSystem.RandomUintCircle(-EnemyPropertyConstant.patrolMaxDistance, EnemyPropertyConstant.patrolMaxDistance);
    }

    public override void OnUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.targets = SectorSystem.GetAroundEntities(enemyEntity, EnemyPropertyConstant.atkMaxCount, true);

        var target = MathManager.ToVector3(enemyEntity.movement.target);
        var pos = MathManager.ToVector3(enemyEntity.transform.pos);
        var direction = target - pos;

        if (direction.sqrMagnitude > Math.Pow(EnemyPropertyConstant.CollisionRadius + EnemyPropertyConstant.CollisionRadius, 2))
        {
            enemyEntity.input.yaw = MathManager.Format8DirInput(direction.normalized);
            MoveSystem.UpdatePosition(enemyEntity);
            MoveSystem.UpdateRotaion(enemyEntity);
        }
        else
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Idle);
        }
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        if (enemyEntity.attack.targets.Length > 0)
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Move);
        }
    }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.input.yaw = MathManager.YawStop;
    }

}
