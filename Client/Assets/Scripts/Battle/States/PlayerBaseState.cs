[PlayerState(EPlayerState.None)]
public class PlayerBaseState : BaseState<PlayerEntity>
{
    /// <summary>
    /// 状态ID
    /// </summary>
    public EPlayerState StateId
    {
        get => (EPlayerState)_stateId;
        set => _stateId = (int)value;
    }

    public override void Reset(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.State.curStateId = (int)StateId;
        playerEntity.State.nextStateId = 0;
        playerEntity.State.enterTime = battleEntity.Time;
    }

    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity) {  }

    public override void OnUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity) { }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity) 
    {
        playerEntity.State.exitTime = battleEntity.Time;
    }

    public virtual void OnCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity) { }

    public virtual void OnPostCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity) { }

}
