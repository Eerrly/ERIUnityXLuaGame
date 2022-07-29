
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

    public override void Enter(PlayerEntity entity) {  }

    public override void OnUpdate(PlayerEntity entity) { }

    public override void OnLateUpdate(PlayerEntity entity) { }

    public override void Exit(PlayerEntity entity) { }

}
