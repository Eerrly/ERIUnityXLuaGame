﻿[PlayerState(EPlayerState.Idle)]
public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Idle);
        playerEntity.animation.fixedTransitionDuration = 0.1f;
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if(!KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Move);
        }
        if (KeySystem.CheckWarRoarKeyDown(playerEntity))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.WarRoar);
        }
        if (KeySystem.CheckAttackKeyDown(playerEntity))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.AttackReady);
        }
    }

}
