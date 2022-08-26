using System;

[EnemyState(EEnemyState.Move)]
public class EnemyMoveState : EnemyBaseState
{

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = true;
        enemyEntity.animation.fixedTransitionDuration = 0.1f;
        enemyEntity.movement.moveSpeed = EnemyPropertyConstant.MoveSpeed;
        enemyEntity.movement.turnSpeed = EnemyPropertyConstant.TurnSpeed;
        AnimationSystem.ChangePlayerAnimation(enemyEntity, EAnimationID.Run);
    }

    public override void OnUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.attack.targets = SectorSystem.GetAroundEntities(enemyEntity, EnemyPropertyConstant.atkMaxCount, true);
        if(enemyEntity.attack.targets.Length > 0)
        {
            var entity = battleEntity.FindEntity(enemyEntity.attack.targets[0]);
            var target = MathManager.ToVector3(entity.transform.pos);
            var pos = MathManager.ToVector3(enemyEntity.transform.pos);
            var direction = target - pos;
            if (direction.sqrMagnitude > Math.Pow(EnemyPropertyConstant.CollisionRadius + PlayerPropertyConstant.CollisionRadius, 2))
            {
                enemyEntity.input.yaw = MathManager.Format8DirInput(direction.normalized);
                MoveSystem.UpdatePosition(enemyEntity);
                MoveSystem.UpdateRotaion(enemyEntity);
            }
            else
            {
                EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.AttackReady);
                enemyEntity.input.yaw = MathManager.YawStop;
                enemyEntity.movement.position = MathManager.Vector3Zero;
            }
        }
    }

    public override void OnLateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        if (enemyEntity.attack.targets.Length <= 0)
        {
            EntityStateSystem.ChangeEntityState(enemyEntity, EEnemyState.Idle);
        }
    }

    public override void OnExit(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.input.yaw = MathManager.YawStop;
    }

}
