public class EnemyStateMachine : BaseStateMachine<EnemyEntity>
{

    private static EnemyStateMachine _instance;

    public static EnemyStateMachine Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new EnemyStateMachine();
            }
            return _instance;
        }
    }

    public EnemyStateMachine() : base()
    {
        var types = GetType().Assembly.GetExportedTypes();
        _stateDic = new BaseState<EnemyEntity>[(int)EEnemyState.Count];
        for (var i = 0; i < types.Length; ++i)
        {
            if (types[i].IsDefined(typeof(EnemyStateAttribute), false))
            {
                var state = System.Activator.CreateInstance(types[i]) as EnemyBaseState;
                var attributes = types[i].GetCustomAttributes(false);
                for (var j = 0; j < attributes.Length; ++j)
                {
                    if (attributes[j] is EnemyStateAttribute)
                    {
                        var attr = (attributes[j] as EnemyStateAttribute);
                        state.StateId = attr._state;
                    }
                }

                var stateId = (int)state.StateId;

#if UNITY_EDITOR
                if (null != _stateDic[stateId])
                {
                    UnityEngine.Debug.LogErrorFormat("The {0} state has a instance, please check. now {1} other {2}", state.StateId, types[i], _stateDic[stateId].GetType());
                }
                else
#endif
                {
                    _stateDic[stateId] = state;
                }
#if UNITY_EDITOR
                var fileds = types[i].GetFields();
                if (fileds.Length > 0)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has filed!", types[i]);
                }

                var properties = types[i].GetProperties();
                if (properties.Length > 4)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has property!", types[i]);
                }
#endif

            }
        }
    }

    public override void Update(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        var curState = _stateDic[(int)enemyEntity.curStateId];
        curState.OnUpdate(enemyEntity, battleEntity);
    }

    public void LateUpdate(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        var curState = _stateDic[(int)enemyEntity.curStateId];
        curState.OnLateUpdate(enemyEntity, battleEntity);
    }

    public bool DoChangeState(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        var nextId = enemyEntity.state.nextStateId;
        var nextState = _stateDic[nextId] as EnemyBaseState;
        if (nextId != 0 && nextState != null && nextState.TryEnter(enemyEntity, battleEntity))
        {
            var currId = enemyEntity.state.curStateId;
            var currState = _stateDic[currId] as EnemyBaseState;
            if (currId != 0 && currState != null && currState.TryExit(enemyEntity, battleEntity))
            {
                currState.OnExit(enemyEntity, battleEntity);
            }
            enemyEntity.state.nextStateId = (int)EEnemyState.None;
            nextState.Reset(enemyEntity, battleEntity);
            nextState.OnEnter(enemyEntity, battleEntity);
            if (enemyEntity.state.nextStateId != (int)EEnemyState.None)
            {
                return DoChangeState(enemyEntity, battleEntity);
            }
            return true;
        }
        else
        {
            enemyEntity.state.nextStateId = (int)EEnemyState.None;
        }
        return false;
    }

}
