public class PlayerEntity : BaseEntity
{
    public int ID;

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

    private bool _isAi = false;
    public bool isAi
    {
        get { return _isAi; }
        set { _isAi = value; }
    }

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
        isAi = data.isAi;
        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;
        input.yaw = MathManager.YawStop;
        input.key = 0;
        movement.moveSpeed = BattleConstant.moveSpeed;
        movement.turnSpeed = BattleConstant.turnSpeed;
        animation.fixedTransitionDuration = 0.0f;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;
        attack.lastAttackTime = -1;
        property.collsionSize = BattleConstant.collisionRadius;
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
