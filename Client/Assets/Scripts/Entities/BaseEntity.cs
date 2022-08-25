public class BaseEntity
{
    public int ID;

    public Cell cell;

    #region Components
    public InputComponent input = new InputComponent();
    public AttackComponent attack = new AttackComponent();
    public TransformComponent transform = new TransformComponent();
    public MoveComponent movement = new MoveComponent();
    public StateComponent state = new StateComponent();
    public AnimationComponent animation = new AnimationComponent();
    public RuntimePropertyComponent runtimeProperty = new RuntimePropertyComponent();
    public PropertyComponent property = new PropertyComponent();
    #endregion

    #region Attributes
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
    #endregion

    public virtual void Reset() { }

    public virtual float GetCollisionRadius(BattleEntity battleEntity) { return float.MaxValue; }

    internal virtual void Init(BattlePlayerCommonData data) { }

}
