[PlayerState(EPlayerState.Move)]
public class PlayerMoveState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.animation.loop = true;
        playerEntity.animation.fixedTransitionDuration = 0.1f;
        playerEntity.movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        playerEntity.movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Run);
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
        if (KeySystem.CheckKeyCodeJDown(playerEntity))
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

    public override void OnCollision(PlayerEntity source, PlayerEntity target, BattleEntity battleEntity)
    {
#if UNITY_DEBUG
        UnityEngine.Debug.Log("[PlayerMoveState OnCollision] source:" + source.ID + ", target:" + target.ID);
#endif
    }
}
