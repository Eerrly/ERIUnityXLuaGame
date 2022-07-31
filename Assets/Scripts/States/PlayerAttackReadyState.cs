[PlayerState(EPlayerState.AttackReady)]
public class PlayerAttackReadyState : PlayerBaseState
{
    public override bool TryEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        return AttackSystem.CheckAttackCdTime(playerEntity, battleEntity);
    }
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.attack.attackDistance = BattleConstant.attackDistance;
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (PhysicsSystem.IsCanAttack(playerEntity))
        {
            PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Attack);
        }
        else
        {
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

    public override void OnExit(PlayerEntity entity, BattleEntity battleEntity)
    {
        entity.attack.attackDistance = 0;
    }

}
