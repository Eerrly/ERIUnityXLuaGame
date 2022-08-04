[PlayerState(EPlayerState.Attack)]
public class PlayerAttackState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
#if UNITY_DEBUG
        UnityEngine.Debug.Log("[PlayerAttackState OnEnter] targetId:" + playerEntity.attack.targetId);
#endif
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Attack);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if(AnimationSystem.CheckAnimationNormalizedTimeDone(playerEntity))
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
