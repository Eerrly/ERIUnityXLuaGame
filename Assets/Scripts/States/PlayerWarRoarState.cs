[PlayerState(EPlayerState.WarRoar)]
public class PlayerWarRoarState : PlayerBaseState
{

    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.animation.animId = (int)EAnimationID.Skill;
        playerEntity.animation.fixedTransitionDuration = 0.1f;
        BuffSystem.TryEnableBuff(playerEntity, battleEntity, EBuffTriggerType.Default);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (!AnimationSystem.CheckAnimationNormalizedTimeDone(playerEntity))
            return;

        if (KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Move);
        }
        else
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Idle);
        }
    }

}
