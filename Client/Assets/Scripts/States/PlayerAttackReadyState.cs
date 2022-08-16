[PlayerState(EPlayerState.AttackReady)]
public class PlayerAttackReadyState : PlayerBaseState
{
    public override bool TryEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        return AttackSystem.CheckAttackCdTime(playerEntity, battleEntity);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        PlayerStateSystem.ChangePlayerState(playerEntity, EPlayerState.Attack);
    }

}
