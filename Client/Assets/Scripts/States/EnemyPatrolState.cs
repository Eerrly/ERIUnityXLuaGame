using UnityEngine;

[EnemyState(EEnemyState.Patrol)]
public class EnemyPatrolState : EnemyBaseState
{
    private uint xSeed, ySeed;
    private Vector3 direction;

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = true;
        enemyEntity.animation.fixedTransitionDuration = 0.1f;
        enemyEntity.movement.moveSpeed = EnemyPropertyConstant.MoveSpeed;
        enemyEntity.movement.turnSpeed = EnemyPropertyConstant.TurnSpeed;
        AnimationSystem.ChangePlayerAnimation(enemyEntity, EAnimationID.Run);

        xSeed = enemyEntity.runtimeProperty.seed + (uint)enemyEntity.ID;
        ySeed = enemyEntity.runtimeProperty.seed - (uint)enemyEntity.ID;
        enemyEntity.movement.target = RandomSystem.RandomUintCircle(-EnemyPropertyConstant.patrolMaxDistance, EnemyPropertyConstant.patrolMaxDistance);
    }

    public override void OnUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.targets = SectorSystem.GetAroundEntities(enemyEntity, EnemyPropertyConstant.atkMaxCount);

        Vector3 target = MathManager.ToVector3(enemyEntity.movement.target);
        Vector3 pos = MathManager.ToVector3(enemyEntity.transform.pos);
        direction = target - pos;

        if (direction.sqrMagnitude > EnemyPropertyConstant.CollisionRadius * EnemyPropertyConstant.CollisionRadius)
        {
            enemyEntity.input.yaw = MathManager.Format8DirInput(direction.normalized);
            MoveSystem.UpdatePosition(enemyEntity);
            MoveSystem.UpdateRotaion(enemyEntity);
        }
        if(enemyEntity.attack.targets.Length > 0)
        {
            enemyEntity.input.yaw = MathManager.YawStop;
            enemyEntity.movement.position = MathManager.Vector3Zero;
            enemyEntity.movement.rotation = MathManager.QuaternionIdentity;
        }
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        if (direction.sqrMagnitude <= EnemyPropertyConstant.CollisionRadius * EnemyPropertyConstant.CollisionRadius)
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Idle);
        }
        if (enemyEntity.attack.targets.Length > 0)
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Idle);
        }
    }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.input.yaw = MathManager.YawStop;
    }

}
