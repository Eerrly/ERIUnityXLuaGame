using System;
/// <summary>
/// 玩家实体
/// </summary>
public class PlayerEntity : BaseEntity
{
    /// <summary>
    /// 玩家Id
    /// </summary>
    public int ID;

    #region Components
    public TransformComponent transform = new TransformComponent();
    public MoveComponent movement = new MoveComponent();
    public StateComponent state = new StateComponent();
    public InputComponent input = new InputComponent();
    public AnimationComponent animation = new AnimationComponent();
    #endregion

    /// <summary>
    /// 当前状态Id
    /// </summary>
    public EPlayerState curStateId
    {
        get { return (EPlayerState)state.curStateId; }
    }

    /// <summary>
    /// 下一个状态Id
    /// </summary>
    public EPlayerState nextStateId
    {
        get { return (EPlayerState)state.nextStateId; }
    }

    /// <summary>
    /// 上一个状态Id
    /// </summary>
    public EPlayerState preStateId
    {
        get { return (EPlayerState)state.preStateId; }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="data">战斗数据</param>
    internal void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;
        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;
        input.yaw = MathManager.YawStop;
        input.key = 0;
        movement.moveSpeed = BattleConstant.moveSpeed;
        movement.turnSpeed = BattleConstant.turnSpeed;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;
    }

}
