
/// <summary>
/// 玩家状态
/// </summary>
[PlayerState(EPlayerState.None)]
public class PlayerBaseState : BaseState<PlayerEntity>
{
    public EPlayerState StateId
    {
        get { return (EPlayerState)_stateId; }
        set { _stateId = (int)value; }
    }

    public override void Reset(PlayerEntity entity, BattleEntity battleEntity)
    {
        entity.state.preStateId = entity.state.curStateId;
        entity.state.curStateId = _stateId;
        entity.state.nextStateId = 0;
        entity.state.enterTime = battleEntity.time;
    }

    public override void OnEnter(PlayerEntity entity, BattleEntity battleEntity) {  }

    public override void OnUpdate(PlayerEntity entity, BattleEntity battleEntity) { }

    public override void OnLateUpdate(PlayerEntity entity, BattleEntity battleEntity) { }

    public override void OnExit(PlayerEntity entity, BattleEntity battleEntity) { }

}
