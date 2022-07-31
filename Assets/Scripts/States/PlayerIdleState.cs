[PlayerState(EPlayerState.Idle)]
public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity entity, BattleEntity battleEntity)
    {
        entity.animation.animId = 0;
        entity.animation.fixedTransitionDuration = 0.0f;
    }

    public override void OnUpdate(PlayerEntity entity, BattleEntity battleEntity)
    {
    }

    public override void OnLateUpdate(PlayerEntity entity, BattleEntity battleEntity)
    {
    }

    public override void OnExit(PlayerEntity entity, BattleEntity battleEntity)
    {
    }

}
