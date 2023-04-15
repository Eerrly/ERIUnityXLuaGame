[PlayerState(EPlayerState.Move)]
public class PlayerMoveState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        playerEntity.movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;
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
            EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.Idle);
        }
        if (KeySystem.CheckKeyCodeJDown(playerEntity))
        {
            EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.AttackReady);
        }
    }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.movement.position = FixedVector3.Zero;
        playerEntity.movement.rotation = FixedQuaternion.Identity;
        playerEntity.movement.moveSpeed = FixedNumber.Zero;
        playerEntity.movement.turnSpeed = 0f;
    }
}
