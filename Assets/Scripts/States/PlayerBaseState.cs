[PlayerState(EPlayerState.None)]
public class PlayerBaseState : BaseState<PlayerEntity>
{
    public EPlayerState StateId
    {
        get { return (EPlayerState)_stateId; }
        set { _stateId = (int)value; }
    }

    public override void Reset(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.state.preStateId = playerEntity.state.curStateId;
        playerEntity.state.curStateId = _stateId;
        playerEntity.state.nextStateId = 0;
        playerEntity.state.enterTime = battleEntity.time;
        playerEntity.state.exitTime = playerEntity.state.enterTime;
    }

    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity) {  }

    public override void OnUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity) { }

}
