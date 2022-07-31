[PlayerState(EPlayerState.Move)]
public class PlayerMoveState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Move);
        playerEntity.animation.fixedTransitionDuration = 0.1f;
        playerEntity.movement.moveSpeed = BattleConstant.moveSpeed;
        playerEntity.movement.turnSpeed = BattleConstant.turnSpeed;
    }

    public override void OnUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        MoveSystem.UpdatePosition(playerEntity);
        MoveSystem.UpdateRotaion(playerEntity);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Idle);
        }
        if (KeySystem.CheckAttackKeyDown(playerEntity))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.AttackReady);
        }
    }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.movement.position = MathManager.Vector3Zero;
        playerEntity.movement.rotation = MathManager.QuaternionIdentity;
        playerEntity.movement.moveSpeed = 0.0f;
        playerEntity.movement.turnSpeed = 0.0f;
    }
}
