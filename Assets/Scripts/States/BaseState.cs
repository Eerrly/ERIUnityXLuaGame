
public class BaseState<T> where T: BaseEntity
{

    protected int _stateId;

    public virtual void Reset(T entity, BattleEntity battleEntity) { }

    public virtual bool TryEnter(T entity, BattleEntity battleEntity) { return true; }

    public virtual void OnEnter(T entity, BattleEntity battleEntity) { }

    public virtual void OnUpdate(T entity, BattleEntity battleEntity) { }

    public virtual void OnLateUpdate(T entity, BattleEntity battleEntity) { }

    public virtual bool TryExit(T entity, BattleEntity battleEntity) { return true; }

    public virtual void OnExit(T entity, BattleEntity battleEntity) { }

}
