public class BaseEntity
{
    public int ID;

    public Cell Cell;

    #region Components
    /// <summary>
    /// 输入组件
    /// </summary>
    public readonly InputComponent Input = new InputComponent();
    /// <summary>
    /// 位置组件
    /// </summary>
    public readonly TransformComponent Transform = new TransformComponent();
    /// <summary>
    /// 移动组件
    /// </summary>
    public readonly MoveComponent Movement = new MoveComponent();
    /// <summary>
    /// 状态组件
    /// </summary>
    public readonly StateComponent State = new StateComponent();
    /// <summary>
    /// 运行时属性组件
    /// </summary>
    public readonly RuntimePropertyComponent RuntimeProperty = new RuntimePropertyComponent();
    /// <summary>
    /// 属性组件
    /// </summary>
    public readonly PropertyComponent Property = new PropertyComponent();
    /// <summary>
    /// 碰撞组件
    /// </summary>
    public readonly CollisionComponent Collision = new CollisionComponent();
    #endregion

    #region Attributes
    /// <summary>
    /// 当前状态ID
    /// </summary>
    public EPlayerState curStateId => (EPlayerState)State.curStateId;

    /// <summary>
    /// 下一个状态ID
    /// </summary>
    public EPlayerState nextStateId => (EPlayerState)State.nextStateId;

    /// <summary>
    /// 上一个状态ID
    /// </summary>
    public EPlayerState preStateId => (EPlayerState)State.preStateId;

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
