[PlayerState(EPlayerState.Idle)]
public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.animation.loop = true;
        playerEntity.animation.fixedTransitionDuration = 0.1f;
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Idle);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (!KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Move);
        }
        if (KeySystem.CheckKeyCodeJDown(playerEntity))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.AttackReady);
        }
    }

}
