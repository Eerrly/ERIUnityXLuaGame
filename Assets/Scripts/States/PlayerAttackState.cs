[PlayerState(EPlayerState.Attack)]
public class PlayerAttackState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.animation.animId = (int)EAnimationID.Attack;
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if(playerEntity.animation.normalizedTime >= 0.95f)
        {
            if (KeySystem.IsYawTypeStop(playerEntity.input.yaw))
            {
                PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Idle);
            }
            else
            {
                PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Move);
            }
        }
    }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.attack.lastAttackTime = battleEntity.time;
    }

}
