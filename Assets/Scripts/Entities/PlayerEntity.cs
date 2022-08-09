public class PlayerEntity : BaseEntity
{
    public int ID;

    public Cell cell;

    #region Components
    public TransformComponent transform = new TransformComponent();
    public MoveComponent movement = new MoveComponent();
    public StateComponent state = new StateComponent();
    public InputComponent input = new InputComponent();
    public AnimationComponent animation = new AnimationComponent();
    public AttackComponent attack = new AttackComponent();
    public RuntimePropertyComponent runtimeProperty = new RuntimePropertyComponent();
    public PropertyComponent property = new PropertyComponent();
    #endregion

    public EPlayerState curStateId
    {
        get { return (EPlayerState)state.curStateId; }
    }

    public EPlayerState nextStateId
    {
        get { return (EPlayerState)state.nextStateId; }
    }

    public EPlayerState preStateId
    {
        get { return (EPlayerState)state.preStateId; }
    }

    internal void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;
        input.yaw = MathManager.YawStop;
        input.key = 0;
        animation.fixedTransitionDuration = 0.0f;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;
        attack.lastAttackTime = -1;
        property.camp = (ECamp)data.camp;

        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;
        movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;
        property.collsionSize = PlayerPropertyConstant.CollisionRadius;

        InitBuffs();
    }

    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    public float GetCollisionRadius(BattleEntity battleEntity)
    {
        return property.collsionSize;
    }

}
