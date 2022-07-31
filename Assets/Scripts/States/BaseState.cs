
/// <summary>
/// 状态基类
/// </summary>
/// <typeparam name="T">实体</typeparam>
public class BaseState<T> where T: BaseEntity
{
    /// <summary>
    /// 状态Id
    /// </summary>
    protected int _stateId;

    /// <summary>
    /// 重置数据
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void Reset(T entity, BattleEntity battleEntity) { }

    /// <summary>
    /// 是否可以进入状态
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>是否可以</returns>
    public virtual bool TryEnter(T entity, BattleEntity battleEntity) { return true; }

    /// <summary>
    /// 进入状态
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void OnEnter(T entity, BattleEntity battleEntity) { }

    /// <summary>
    /// 轮询状态
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void OnUpdate(T entity, BattleEntity battleEntity) { }

    /// <summary>
    /// 轮询状态
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void OnLateUpdate(T entity, BattleEntity battleEntity) { }

    /// <summary>
    /// 是否可以退出状态
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>是否可以</returns>
    public virtual bool TryExit(T entity, BattleEntity battleEntity) { return true; }

    /// <summary>
    /// 退出状态
    /// </summary>
    /// <param name="entity">实体</param>
    public virtual void OnExit(T entity, BattleEntity battleEntity) { }

}
