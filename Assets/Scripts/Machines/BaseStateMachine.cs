/// <summary>
/// 状态机器
/// </summary>
public class BaseStateMachine<T> where T: BaseEntity
{

    protected BaseState<T>[] _stateDic;

    public BaseStateMachine() { }

    public virtual BaseState<T> GetState(int stateId)
    {
        return _stateDic[stateId];
    }

    public virtual void Update(T entity)
    {

    }

}
