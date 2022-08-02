[BattleState(EBattleState.None)]
public class BattleBaseState : BaseState<BattleEntity>
{

    public EBattleState StateId
    {
        get { return (EBattleState)_stateId; }
        set { _stateId = (int)value; }
    }

    public override void Reset(BattleEntity battleEntity, BattleEntity _)
    {
        battleEntity.state.curStateId = (int)StateId;
        battleEntity.state.nextStateId = 0;
        battleEntity.state.enterTime = battleEntity.time;
    }

    public override void OnEnter(BattleEntity battleEntity, BattleEntity _) { }

    public override void OnExit(BattleEntity battleEntity, BattleEntity _) { }

    public override void OnUpdate(BattleEntity battleEntity, BattleEntity _) { }

    public override void OnLateUpdate(BattleEntity battleEntity, BattleEntity _) { }

}
