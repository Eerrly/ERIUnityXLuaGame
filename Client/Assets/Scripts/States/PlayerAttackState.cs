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
        if(AnimationSystem.CheckAnimationNormalizedTime(playerEntity))
        {
            var entities = SectorSystem.GetWithinRangeOfTheAttack(playerEntity);
            for (int i = 0; i < entities.Count; i++)
            {
                AttackSystem.Attack(playerEntity, entities[i]);
            }

            if (KeySystem.IsYawTypeStop(playerEntity.input.yaw))
            {
                EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.Idle);
            }
            else
            {
                EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.Move);
            }
        }
    }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.attack.lastAttackTime = battleEntity.time;
    }

}
