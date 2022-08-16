using System.Collections.Generic;

[PlayerState(EPlayerState.Attack)]
public class PlayerAttackState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.animation.loop = false;
        AnimationSystem.ChangePlayerAnimation(playerEntity, EAnimationID.Attack);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if(AnimationSystem.CheckAnimationNormalizedTime(playerEntity, 0.65f))
        {
            var entities = SectorSystem.GetWithinRangeOfTheAttack(playerEntity);
            for (int i = 0; i < entities.Count; i++)
            {
                AttackSystem.Attack(playerEntity, entities[i]);
            }
        }
        if(AnimationSystem.CheckAnimationNormalizedTime(playerEntity))
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
