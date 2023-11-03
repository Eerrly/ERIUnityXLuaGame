[PlayerState(EPlayerState.None)]
public class PlayerBaseState : BaseState<PlayerEntity>
{
    /// <summary>
    /// 状态ID
    /// </summary>
    public EPlayerState StateId
    {
        get { return (EPlayerState)_stateId; }
        set { _stateId = (int)value; }
    }

    public override void Reset(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.state.curStateId = (int)StateId;
        playerEntity.state.nextStateId = 0;
        playerEntity.state.enterTime = battleEntity.time;
    }

    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity) {  }

    public override void OnUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity) 
    {
        playerEntity.state.exitTime = battleEntity.time;
    }

    public virtual void OnCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity) { }

    public virtual void OnPostCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity) { }

}
