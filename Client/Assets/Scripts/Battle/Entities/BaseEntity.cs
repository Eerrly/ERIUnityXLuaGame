public class BaseEntity
{
    public int ID;

    public Cell cell;

    #region Components
    /// <summary>
    /// 输入组件
    /// </summary>
    public InputComponent input = new InputComponent();
    /// <summary>
    /// 位置组件
    /// </summary>
    public TransformComponent transform = new TransformComponent();
    /// <summary>
    /// 移动组件
    /// </summary>
    public MoveComponent movement = new MoveComponent();
    /// <summary>
    /// 状态组件
    /// </summary>
    public StateComponent state = new StateComponent();
    /// <summary>
    /// 运行时属性组件
    /// </summary>
    public RuntimePropertyComponent runtimeProperty = new RuntimePropertyComponent();
    /// <summary>
    /// 属性组件
    /// </summary>
    public PropertyComponent property = new PropertyComponent();
    /// <summary>
    /// 碰撞组件
    /// </summary>
    public CollisionComponent collision = new CollisionComponent();
    #endregion

    #region Attributes
    /// <summary>
    /// 当前状态ID
    /// </summary>
    public EPlayerState curStateId
    {
        get { return (EPlayerState)state.curStateId; }
    }

    /// <summary>
    /// 下一个状态ID
    /// </summary>
    public EPlayerState nextStateId
    {
        get { return (EPlayerState)state.nextStateId; }
    }

    /// <summary>
    /// 上一个状态ID
    /// </summary>
    public EPlayerState preStateId
    {
        get { return (EPlayerState)state.preStateId; }
    }
    #endregion

    /// <summary>
    /// 重置
    /// </summary>
    public virtual void Reset() { }

    /// <summary>
    /// 获取碰撞半径
    /// </summary>
    /// <param name="battleEntity"></param>
    /// <returns></returns>
    public virtual FixedNumber GetCollisionRadius(BattleEntity battleEntity) { return default(FixedNumber); }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="data"></param>
    internal virtual void Init(BattlePlayerCommonData data) { }

}
